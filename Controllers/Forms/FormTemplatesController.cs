using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.ViewModels.Components;
using FormReporting.Models.ViewModels.Metrics;
using FormReporting.Extensions;
using FormReporting.Services.Forms;
using FormReporting.Services.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FormReporting.Controllers.Forms
{
    [Authorize]
    public class FormTemplatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFormCategoryService _categoryService;
        private readonly IFormTemplateService _templateService;
        private readonly IFormBuilderService _formBuilderService;
        private readonly IUserService _userService;

        public FormTemplatesController(
            ApplicationDbContext context,
            IFormCategoryService categoryService,
            IFormTemplateService templateService,
            IFormBuilderService formBuilderService,
            IUserService userService)
        {
            _context = context;
            _categoryService = categoryService;
            _templateService = templateService;
            _formBuilderService = formBuilderService;
            _userService = userService;
        }

        /// <summary>
        /// Index - Template Management Dashboard with statistics, filters, and pagination
        /// </summary>
        public async Task<IActionResult> Index(
            string? tab,
            string? search,
            string? status,
            string? type,
            string? category,
            int page = 1)
        {
            const int pageSize = 15; // Items per page

            // ============================================================================
            // LAYER 1: BUILD QUERY WITH FILTERS
            // ============================================================================
            var query = _context.FormTemplates
                .Include(t => t.Category)
                .Include(t => t.Submissions)
                .Include(t => t.Creator)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(t =>
                    t.TemplateName.ToLower().Contains(search) ||
                    t.TemplateCode.ToLower().Contains(search) ||
                    (t.Description != null && t.Description.ToLower().Contains(search)));
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.PublishStatus.ToLower() == status.ToLower());
            }

            // Apply type filter
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(t => t.TemplateType.ToLower() == type.ToLower());
            }

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t => t.Category.CategoryName.ToLower() == category.ToLower());
            }

            // Get total count before pagination
            var totalItems = await query.CountAsync();

            // ============================================================================
            // LAYER 2: CALCULATE STATISTICS FOR STAT CARDS
            // ============================================================================
            
            // Stat 1: Total Active Templates
            ViewBag.TotalTemplates = await _context.FormTemplates
                .CountAsync(t => t.IsActive);

            // Stat 2: Published Templates (with trend)
            ViewBag.PublishedTemplates = await _context.FormTemplates
                .CountAsync(t => t.PublishStatus == "Published" && t.IsActive);

            // Calculate trend: published in last 30 days vs previous 30 days
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var sixtyDaysAgo = DateTime.Now.AddDays(-60);
            var recentPublished = await _context.FormTemplates
                .CountAsync(t => t.PublishedDate >= thirtyDaysAgo);
            var previousPublished = await _context.FormTemplates
                .CountAsync(t => t.PublishedDate >= sixtyDaysAgo && t.PublishedDate < thirtyDaysAgo);
            ViewBag.PublishedGrowth = previousPublished > 0
                ? ((recentPublished - previousPublished) / (double)previousPublished * 100)
                : 0;

            // Stat 3: Submissions This Month
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var submissionsThisMonth = await _context.FormTemplateSubmissions
                .CountAsync(s => s.SubmittedDate >= firstDayOfMonth && s.SubmittedDate != null);
            ViewBag.SubmissionsThisMonth = submissionsThisMonth;

            // Calculate trend: this month vs last month
            var firstDayOfLastMonth = firstDayOfMonth.AddMonths(-1);
            var submissionsLastMonth = await _context.FormTemplateSubmissions
                .CountAsync(s => s.SubmittedDate >= firstDayOfLastMonth && 
                                 s.SubmittedDate < firstDayOfMonth && 
                                 s.SubmittedDate != null);
            ViewBag.SubmissionsGrowth = submissionsLastMonth > 0
                ? ((submissionsThisMonth - submissionsLastMonth) / (double)submissionsLastMonth * 100)
                : 0;

            // Stat 4: Completion Rate (Approved / Total Submissions)
            var totalSubmissions = await _context.FormTemplateSubmissions.CountAsync();
            var approvedSubmissions = await _context.FormTemplateSubmissions
                .CountAsync(s => s.Status == "Approved");
            ViewBag.CompletionRate = totalSubmissions > 0
                ? (approvedSubmissions / (double)totalSubmissions * 100)
                : 0;

            // ============================================================================
            // LAYER 3: EXECUTE PAGINATED QUERY WITH PROJECTION
            // ============================================================================
            var templates = await query
                .OrderByDescending(t => t.ModifiedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new FormTemplateViewModel
                {
                    TemplateId = t.TemplateId,
                    TemplateName = t.TemplateName,
                    TemplateCode = t.TemplateCode,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category.CategoryName,
                    TemplateType = t.TemplateType,
                    PublishStatus = t.PublishStatus,
                    Version = t.Version,
                    SubmissionCount = t.Submissions.Count,
                    IsActive = t.IsActive,
                    CreatedBy = t.Creator.FullName ?? "Unknown",
                    ModifiedDate = t.ModifiedDate,
                    Description = t.Description,
                    // Submission mode & access
                    SubmissionMode = t.SubmissionMode,
                    AllowAnonymousAccess = t.AllowAnonymousAccess,
                    // Configuration status
                    SectionCount = t.Sections.Count,
                    FieldCount = t.Sections.SelectMany(s => s.Items).Count(),
                    HasFormBuilder = t.Sections.Any(),
                    HasAssignments = t.Assignments.Any(),
                    HasWorkflow = t.WorkflowId.HasValue,
                    HasMetrics = false // TODO: Implement metric mapping check when available
                })
                .ToListAsync();

            // Pass pagination info to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            // Pass filter values to view for maintaining state
            ViewBag.CurrentTab = tab ?? "templates";
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentType = type;
            ViewBag.CurrentCategory = category;

            // Get categories with template counts for sidebar navigation
            var categoriesWithCounts = await _context.FormCategories
                .Where(c => c.IsActive)
                .Select(c => new {
                    c.CategoryId,
                    c.CategoryName,
                    c.Description,
                    c.IconClass,
                    TemplateCount = c.FormTemplates.Count(t => t.IsActive)
                })
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            
            ViewBag.CategoriesWithCounts = categoriesWithCounts
                .Select(c => new { c.CategoryId, c.CategoryName, c.Description, c.IconClass, c.TemplateCount })
                .ToList();
            
            // Legacy: Keep simple category list for backwards compatibility
            ViewBag.Categories = categoriesWithCounts
                .Select(c => c.CategoryName)
                .ToList();
            
            // Total template count for "All" option
            ViewBag.AllTemplatesCount = await _context.FormTemplates.CountAsync(t => t.IsActive);

            return View("~/Views/Forms/FormTemplates/Index.cshtml", templates);
        }

        /// <summary>
        /// Details - View published template details with tabbed interface
        /// Post-publish configuration: Assignments, Workflow, Metrics, Submissions
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="tab">Active tab (overview, structure, assignments, workflow, metrics, submissions)</param>
        [HttpGet("FormTemplates/Details/{id}")]
        public async Task<IActionResult> Details(int id, string? tab = "overview")
        {
            // Load template with all related data including assignment targets
            var template = await _context.FormTemplates
                .Include(t => t.Category)
                .Include(t => t.Creator)
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Items)
                .Include(t => t.Submissions)
                .Include(t => t.SubmissionRules)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.TenantGroup)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Tenant)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Role)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Department)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.UserGroup)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.User)
                .Include(t => t.Workflow)
                    .ThenInclude(w => w!.Steps)
                        .ThenInclude(s => s.Action)
                .Include(t => t.Workflow)
                    .ThenInclude(w => w!.Steps)
                        .ThenInclude(s => s.ApproverRole)
                .Include(t => t.Workflow)
                    .ThenInclude(w => w!.Steps)
                        .ThenInclude(s => s.ApproverUser)
                .Include(t => t.Workflow)
                    .ThenInclude(w => w!.Steps)
                        .ThenInclude(s => s.AssigneeDepartment)
                .FirstOrDefaultAsync(t => t.TemplateId == id);

            if (template == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction(nameof(Index));
            }

            // Build ViewModel
            var viewModel = new TemplateDetailsViewModel
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                TemplateCode = template.TemplateCode,
                Description = template.Description,
                TemplateType = template.TemplateType,
                PublishStatus = template.PublishStatus,
                CategoryName = template.Category?.CategoryName,
                CategoryId = template.CategoryId,
                CreatedDate = template.CreatedDate,
                PublishedDate = template.PublishedDate,
                CreatorName = template.Creator?.FullName,
                Version = template.Version,
                ActiveTab = tab ?? "overview",

                // Structure
                TotalFields = template.Sections.Sum(s => s.Items.Count),
                Sections = template.Sections.OrderBy(s => s.DisplayOrder).Select(s => new TemplateSectionSummary
                {
                    SectionId = s.SectionId,
                    SectionName = s.SectionName,
                    SectionOrder = s.DisplayOrder,
                    FieldCount = s.Items.Count,
                    RequiredFieldCount = s.Items.Count(i => i.IsRequired),
                    Fields = s.Items.OrderBy(i => i.DisplayOrder).Select(i => new TemplateFieldSummary
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName,
                        ItemCode = i.ItemCode,
                        FieldType = i.DataType,
                        IsRequired = i.IsRequired,
                        ItemOrder = i.DisplayOrder
                    }).ToList()
                }).ToList(),

                // Assignments
                AssignmentCount = template.Assignments.Count,
                ActiveAssignmentCount = template.Assignments.Count(a => a.Status == "Active" || a.Status == "Pending"),
                OverdueAssignmentCount = template.Assignments.Count(a => a.Status == "Overdue"),

                // Workflow
                WorkflowId = template.WorkflowId,
                WorkflowName = template.Workflow?.WorkflowName,
                WorkflowStepCount = template.Workflow?.Steps.Count ?? 0,

                // Submission Rules
                SubmissionRuleCount = template.SubmissionRules.Count,
                ActiveSubmissionRuleCount = template.SubmissionRules.Count(r => r.Status == "Active"),

                // Submissions
                SubmissionCount = template.Submissions.Count,
                PendingSubmissionCount = template.Submissions.Count(s => s.Status == "Draft" || s.Status == "Submitted" || s.Status == "InApproval"),
                CompletedSubmissionCount = template.Submissions.Count(s => s.Status == "Approved"),

                // Statistics
                Statistics = new TemplateStatistics
                {
                    TotalSubmissions = template.Submissions.Count,
                    PendingSubmissions = template.Submissions.Count(s => s.Status == "Draft" || s.Status == "Submitted" || s.Status == "InApproval"),
                    CompletedSubmissions = template.Submissions.Count(s => s.Status == "Approved"),
                    OverdueSubmissions = template.Submissions.Count(s => s.Status == "Rejected"),
                    CompletionRate = template.Submissions.Count > 0 
                        ? (decimal)template.Submissions.Count(s => s.Status == "Approved") / template.Submissions.Count * 100 
                        : 0,
                    TotalAssignments = template.Assignments.Count,
                    ActiveAssignments = template.Assignments.Count(a => a.Status == "Active"),
                    OverdueAssignments = 0, // No longer tracked at assignment level - calculated per submission
                    AssignmentComplianceRate = template.Assignments.Count > 0
                        ? (decimal)template.Assignments.Count(a => a.Status == "Active") / template.Assignments.Count * 100
                        : 0,
                    LastSubmissionDate = template.Submissions.OrderByDescending(s => s.SubmittedDate).FirstOrDefault()?.SubmittedDate,
                    NextDueDate = null // Due dates are now calculated dynamically based on frequency rules
                }
            };

            // Load metric mappings for the Metrics tab
            var fieldMappings = await _context.FormItemMetricMappings
                .Include(m => m.Item)
                .Include(m => m.Metric)
                .Where(m => m.Item.Section.TemplateId == id && m.IsActive)
                .ToListAsync();

            var sectionMappings = await _context.FormSectionMetricMappings
                .Include(m => m.Section)
                .Include(m => m.Metric)
                .Where(m => m.Section.TemplateId == id && m.IsActive)
                .ToListAsync();

            var templateMappings = await _context.FormTemplateMetricMappings
                .Include(m => m.Metric)
                .Where(m => m.TemplateId == id && m.IsActive)
                .ToListAsync();

            // Populate metric summary data
            viewModel.FieldMappingCount = fieldMappings.Count;
            viewModel.SectionMappingCount = sectionMappings.Count;
            viewModel.TemplateKpiCount = templateMappings.Count;
            viewModel.MetricMappingCount = fieldMappings.Count + sectionMappings.Count + templateMappings.Count;
            viewModel.TotalMappableFields = template.Sections.Sum(s => s.Items.Count);
            viewModel.TotalMappableSections = template.Sections.Count;

            // Get the most recent update date from all mappings
            var allDates = fieldMappings.Select(m => m.CreatedDate)
                .Concat(sectionMappings.Select(m => m.CreatedDate))
                .Concat(templateMappings.Select(m => m.CreatedDate));
            viewModel.MetricsLastUpdated = allDates.Any() ? allDates.Max() : null;

            // Build configured metrics list for display
            var configuredMetrics = new List<MetricMappingSummary>();

            // Add field-level mappings
            configuredMetrics.AddRange(fieldMappings.Select(m => new MetricMappingSummary
            {
                MappingId = m.MappingId,
                MappingName = m.Item?.ItemName ?? "Unknown Field",
                Level = "Field",
                MappingType = m.MappingType,
                MetricName = m.Metric?.MetricName ?? "Unknown Metric",
                MetricCode = m.Metric?.MetricCode,
                IsActive = m.IsActive
            }));

            // Add section-level mappings
            configuredMetrics.AddRange(sectionMappings.Select(m => new MetricMappingSummary
            {
                MappingId = m.MappingId,
                MappingName = m.Section?.SectionName ?? "Unknown Section",
                Level = "Section",
                MappingType = m.AggregationType,
                MetricName = m.Metric?.MetricName ?? "Unknown Metric",
                MetricCode = m.Metric?.MetricCode,
                IsActive = m.IsActive
            }));

            // Add template-level mappings
            configuredMetrics.AddRange(templateMappings.Select(m => new MetricMappingSummary
            {
                MappingId = m.MappingId,
                MappingName = m.MappingName ?? "Template KPI",
                Level = "Template",
                MappingType = m.AggregationType,
                MetricName = m.Metric?.MetricName ?? "Unknown Metric",
                MetricCode = m.Metric?.MetricCode,
                IsActive = m.IsActive
            }));

            viewModel.ConfiguredMetrics = configuredMetrics;

            // Pass assignments directly to view for server-side rendering
            ViewData["Assignments"] = template.Assignments.ToList();
            
            // Pass workflow directly to view for server-side rendering
            ViewData["Workflow"] = template.Workflow;
            
            // Pass submission rules directly to view for server-side rendering
            ViewData["SubmissionRules"] = template.SubmissionRules.ToList();

            return View("~/Views/Forms/FormTemplates/Details.cshtml", viewModel);
        }

        /// <summary>
        /// Create - Display form for creating a new template OR resume editing a draft
        /// </summary>
        /// <param name="id">Optional: Template ID to resume (must be Draft status)</param>
        public async Task<IActionResult> Create(int? id = null)
        {
            // If ID provided, this is a RESUME operation
            if (id.HasValue)
            {
                return await ResumeDraft(id.Value);
            }

            // Otherwise, create NEW template
            // Build progress tracker - Step 1 active, no template ID yet
            var progress = new FormBuilderProgressConfig
            {
                BuilderId = Guid.NewGuid().ToString("N"),
                CurrentStep = FormBuilderStep.TemplateSetup,
                TemplateId = null,
                TemplateName = null,
                ShowSaveDraft = false, // Can't save draft until template created
                ExitUrl = Url.Action("Index", "FormTemplates") ?? "/Forms/FormTemplates"
            }
            .AtStep(FormBuilderStep.TemplateSetup)
            .BuildProgress();

            ViewData["Progress"] = progress;

            // Get active categories for dropdown (using service)
            ViewBag.Categories = await _categoryService.GetCategorySelectListAsync();

            return View("~/Views/Forms/FormTemplates/Create.cshtml");
        }

        /// <summary>
        /// Edit - Create a new version from a published template
        /// Clones the published template as a new draft version and starts wizard
        /// </summary>
        /// <param name="id">Published template ID to version</param>
        [HttpGet("FormTemplates/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // Load published template
                var publishedTemplate = await _templateService.LoadTemplateForEditingAsync(id);

                if (publishedTemplate == null)
                {
                    TempData["ErrorMessage"] = "Template not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Validate template can be versioned (must be published)
                if (!_templateService.CanCreateVersion(publishedTemplate))
                {
                    TempData["ErrorMessage"] = $"Cannot create version from template with status '{publishedTemplate.PublishStatus}'. Only published templates can be edited.";
                    return RedirectToAction(nameof(Index));
                }

                // Create new version via service
                var newVersion = await _templateService.CreateNewVersionAsync(id, userId: 1); // TODO: Get current user ID

                // Show success message
                TempData["SuccessMessage"] = $"Created new version {newVersion.Version} from '{publishedTemplate.TemplateName}'. You can now make changes and publish when ready.";

                // Redirect to Create with new version ID to start wizard
                return RedirectToAction(nameof(Create), new { id = newVersion.TemplateId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating new version: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// ResumeDraft - Resume editing an existing draft template
        /// Analyzes progress and routes to appropriate step (PRIVATE HELPER)
        /// </summary>
        private async Task<IActionResult> ResumeDraft(int id)
        {
            // Load template with all related data
            var template = await _templateService.LoadTemplateForEditingAsync(id);

            if (template == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction(nameof(Index));
            }

            // Only drafts can be resumed
            if (template.PublishStatus != "Draft")
            {
                TempData["ErrorMessage"] = "Cannot resume a published template. Use Edit to create a new version.";
                return RedirectToAction(nameof(Index));
            }

            // Analyze progress and determine current step
            var resumeInfo = _templateService.AnalyzeTemplateProgress(template);

            // Build progress tracker with detected statuses
            var progress = new FormBuilderProgressConfig
            {
                BuilderId = $"template-{template.TemplateId}",
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                TemplateVersion = $"v{template.Version}",
                PublishStatus = template.PublishStatus,
                CurrentStep = resumeInfo.CurrentStep,
                StepStatuses = resumeInfo.StepStatuses
            }
            .AtStep(resumeInfo.CurrentStep)
            .BuildProgress();

            ViewData["Progress"] = progress;

            // Route to appropriate step view based on current step
            // Order: Setup → Build → Publish (3-step wizard)
            return resumeInfo.CurrentStep switch
            {
                FormBuilderStep.TemplateSetup => await ResumeTemplateSetup(template),
                FormBuilderStep.FormBuilder => RedirectToAction("FormBuilder", new { id }),
                FormBuilderStep.ReviewPublish => RedirectToAction("ReviewPublish", new { id }),
                _ => await ResumeTemplateSetup(template)
            };
        }

        /// <summary>
        /// Resume template setup (Step 1) - Helper method to prepare view (PRIVATE HELPER)
        /// </summary>
        private async Task<IActionResult> ResumeTemplateSetup(Models.Entities.Forms.FormTemplate template)
        {
            // Get categories for dropdown
            ViewBag.Categories = await _categoryService.GetCategorySelectListAsync();

            // Return to Create view with template data (it will populate the form)
            return View("~/Views/Forms/FormTemplates/Create.cshtml", template);
        }

        /// <summary>
        /// STEP 1: Template Setup - Direct access to edit basic info (no resume logic)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TemplateSetup(int id)
        {
            // Load template
            var template = await _context.FormTemplates
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TemplateId == id);

            if (template == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction(nameof(Index));
            }

            // Only drafts can be edited
            if (template.PublishStatus != "Draft")
            {
                TempData["ErrorMessage"] = "Cannot edit published templates. Create a new version to make changes.";
                return RedirectToAction(nameof(Index));
            }

            // Build progress tracker for Step 1 - Force Step 1 as active
            var progress = new FormBuilderProgressConfig
            {
                BuilderId = $"template-{template.TemplateId}",
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                TemplateVersion = $"v{template.Version}",
                PublishStatus = template.PublishStatus,
                CurrentStep = FormBuilderStep.TemplateSetup,
                ShowSaveDraft = true,
                ExitUrl = Url.Action("Index", "FormTemplates") ?? "/Forms/FormTemplates"
            }
            .AtStep(FormBuilderStep.TemplateSetup)
            .BuildProgress();

            ViewData["Progress"] = progress;

            // Get categories for dropdown
            ViewBag.Categories = await _categoryService.GetCategorySelectListAsync();

            return View("~/Views/Forms/FormTemplates/Create.cshtml", template);
        }

        /// <summary>
        /// STEP 2: Form Builder - Build sections and fields
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FormBuilder(int id)
        {
            // Load template for form builder
            var viewModel = await _formBuilderService.LoadForBuilderAsync(id);

            if (viewModel == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction(nameof(Index));
            }

            // Only drafts can be edited
            if (!viewModel.IsEditable)
            {
                TempData["ErrorMessage"] = "Cannot edit published templates. Create a new version to make changes.";
                return RedirectToAction(nameof(Index));
            }

            // Build progress tracker for Step 2
            var progress = new FormBuilderProgressConfig
            {
                BuilderId = $"template-{viewModel.TemplateId}",
                TemplateId = viewModel.TemplateId,
                TemplateName = viewModel.TemplateName,
                TemplateVersion = $"v{viewModel.Version}",
                PublishStatus = viewModel.PublishStatus,
                CurrentStep = FormBuilderStep.FormBuilder,
                ShowSaveDraft = true,
                ExitUrl = Url.Action("Index", "FormTemplates") ?? "/Forms/FormTemplates"
            }
            .AtStep(FormBuilderStep.FormBuilder)
            .BuildProgress();

            ViewData["Progress"] = progress;

            // Pass FormBuilderViewModel to view
            return View("~/Views/Forms/FormTemplates/FormBuilder.cshtml", viewModel);
        }

        /// <summary>
        /// Metric Mapping - Map form fields to KPI metrics
        /// Note: This is a POST-PUBLISH configuration, not part of the 3-step wizard
        /// Accessed from Template Details page after template is published
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MetricMapping(int id)
        {
            // Load template
            var template = await _templateService.LoadTemplateForEditingAsync(id);

            if (template == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction(nameof(Index));
            }

            // Store template info in ViewData for the configuration page
            ViewData["TemplateId"] = template.TemplateId;
            ViewData["TemplateName"] = template.TemplateName;
            ViewData["TemplateVersion"] = $"v{template.Version}";
            ViewData["PublishStatus"] = template.PublishStatus;

            // Pass template to view
            return View("~/Views/Forms/FormTemplates/MetricMapping.cshtml", template);
        }

        /// <summary>
        /// STEP 3: Review & Publish - Final validation and publish template
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ReviewPublish(int id)
        {
            // Load template with all related data
            var template = await _context.FormTemplates
                .Include(t => t.Category)
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Items)
                .Include(t => t.Items)
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(t => t.TemplateId == id);

            if (template == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction(nameof(Index));
            }

            // Only drafts can be edited
            if (template.PublishStatus != "Draft")
            {
                TempData["ErrorMessage"] = "This template is already published.";
                return RedirectToAction(nameof(Index));
            }

            // Build ViewModel
            var viewModel = BuildReviewPublishViewModel(template);

            // Build progress tracker for Step 3
            var progress = new FormBuilderProgressConfig
            {
                BuilderId = $"template-{template.TemplateId}",
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                TemplateVersion = $"v{template.Version}",
                PublishStatus = template.PublishStatus,
                CurrentStep = FormBuilderStep.ReviewPublish,
                ShowSaveDraft = false,
                ExitUrl = Url.Action("Index", "FormTemplates") ?? "/Forms/FormTemplates"
            }
            .AtStep(FormBuilderStep.ReviewPublish)
            .BuildProgress();

            ViewData["Progress"] = progress;

            return View("~/Views/Forms/FormTemplates/ReviewPublish.cshtml", viewModel);
        }

        /// <summary>
        /// POST: Publish the template
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishTemplate(int id)
        {
            var template = await _context.FormTemplates
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Items)
                .FirstOrDefaultAsync(t => t.TemplateId == id);

            if (template == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction(nameof(Index));
            }

            if (template.PublishStatus != "Draft")
            {
                TempData["ErrorMessage"] = "This template is already published.";
                return RedirectToAction(nameof(Index));
            }

            // Validate before publishing
            var validationResult = ValidateTemplateForPublish(template);
            if (!validationResult.CanPublish)
            {
                TempData["ErrorMessage"] = "Template validation failed. Please fix all errors before publishing.";
                return RedirectToAction(nameof(ReviewPublish), new { id });
            }

            // Update template status
            template.PublishStatus = "Published";
            template.PublishedDate = DateTime.UtcNow;
            template.ModifiedDate = DateTime.UtcNow;

            // Get current user ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                template.ModifiedBy = userId;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Template '{template.TemplateName}' has been published successfully!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Build ReviewPublishViewModel from template
        /// </summary>
        private ReviewPublishViewModel BuildReviewPublishViewModel(Models.Entities.Forms.FormTemplate template)
        {
            var viewModel = new ReviewPublishViewModel
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                TemplateCode = template.TemplateCode,
                CategoryName = template.Category?.CategoryName,
                TemplateType = template.TemplateType,
                Version = template.Version,
                Description = template.Description,
                CreatedByName = template.Creator?.FullName ?? "Unknown",
                CreatedDate = template.CreatedDate,
                ModifiedDate = template.ModifiedDate,
                PublishStatus = template.PublishStatus,
                SectionCount = template.Sections?.Count ?? 0,
                FieldCount = template.Items?.Count ?? 0,
                RequiredFieldCount = template.Items?.Count(i => i.IsRequired) ?? 0
            };

            // Build field type summary
            if (template.Items != null && template.Items.Any())
            {
                viewModel.FieldTypeSummary = template.Items
                    .GroupBy(i => i.DataType ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            // Build section summaries
            if (template.Sections != null)
            {
                viewModel.Sections = template.Sections
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new SectionSummary
                    {
                        SectionId = s.SectionId,
                        SectionName = s.SectionName,
                        FieldCount = s.Items?.Count ?? 0,
                        RequiredFieldCount = s.Items?.Count(i => i.IsRequired) ?? 0,
                        DisplayOrder = s.DisplayOrder
                    })
                    .ToList();
            }

            // Build validation checklist
            viewModel.ValidationChecklist = BuildValidationChecklist(template);

            return viewModel;
        }

        /// <summary>
        /// Build validation checklist for template
        /// </summary>
        private List<ValidationItem> BuildValidationChecklist(Models.Entities.Forms.FormTemplate template)
        {
            var checklist = new List<ValidationItem>();

            // Check 1: Basic Info Complete
            bool hasBasicInfo = !string.IsNullOrEmpty(template.TemplateName) &&
                               !string.IsNullOrEmpty(template.TemplateCode) &&
                               template.CategoryId > 0 &&
                               !string.IsNullOrEmpty(template.TemplateType);
            checklist.Add(new ValidationItem
            {
                CheckName = "Basic Information",
                Description = "Template name, code, category, and type are required",
                IsPassed = hasBasicInfo,
                FailureMessage = hasBasicInfo ? null : "Please complete all required template information"
            });

            // Check 2: Has Sections
            bool hasSections = template.Sections != null && template.Sections.Any();
            checklist.Add(new ValidationItem
            {
                CheckName = "Has Sections",
                Description = "At least one section is required",
                IsPassed = hasSections,
                FailureMessage = hasSections ? null : "Add at least one section to the form"
            });

            // Check 3: Has Fields
            bool hasFields = template.Items != null && template.Items.Any();
            checklist.Add(new ValidationItem
            {
                CheckName = "Has Fields",
                Description = "At least one field is required",
                IsPassed = hasFields,
                FailureMessage = hasFields ? null : "Add at least one field to the form"
            });

            // Check 4: All Sections Have Fields
            bool allSectionsHaveFields = true;
            string? emptySections = null;
            if (template.Sections != null && template.Sections.Any())
            {
                var emptySecList = template.Sections
                    .Where(s => s.Items == null || !s.Items.Any())
                    .Select(s => s.SectionName)
                    .ToList();
                allSectionsHaveFields = !emptySecList.Any();
                if (!allSectionsHaveFields)
                {
                    emptySections = string.Join(", ", emptySecList);
                }
            }
            checklist.Add(new ValidationItem
            {
                CheckName = "All Sections Have Fields",
                Description = "Every section must contain at least one field",
                IsPassed = allSectionsHaveFields,
                FailureMessage = allSectionsHaveFields ? null : $"Empty sections: {emptySections}"
            });

            // Check 5: Has Description (Warning)
            bool hasDescription = !string.IsNullOrEmpty(template.Description);
            checklist.Add(new ValidationItem
            {
                CheckName = "Has Description",
                Description = "A description helps users understand the form's purpose",
                IsPassed = hasDescription,
                IsWarning = true,
                FailureMessage = hasDescription ? null : "Consider adding a description"
            });

            // Check 6: Has Required Fields (Warning)
            bool hasRequiredFields = template.Items?.Any(i => i.IsRequired) ?? false;
            checklist.Add(new ValidationItem
            {
                CheckName = "Has Required Fields",
                Description = "Forms typically have at least one required field",
                IsPassed = hasRequiredFields,
                IsWarning = true,
                FailureMessage = hasRequiredFields ? null : "All fields are optional"
            });

            return checklist;
        }

        /// <summary>
        /// Validate template can be published
        /// </summary>
        private (bool CanPublish, List<string> Errors) ValidateTemplateForPublish(Models.Entities.Forms.FormTemplate template)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(template.TemplateName))
                errors.Add("Template name is required");
            if (string.IsNullOrEmpty(template.TemplateCode))
                errors.Add("Template code is required");
            if (template.CategoryId <= 0)
                errors.Add("Category is required");
            if (string.IsNullOrEmpty(template.TemplateType))
                errors.Add("Template type is required");
            if (template.Sections == null || !template.Sections.Any())
                errors.Add("At least one section is required");
            if (template.Sections?.All(s => s.Items == null || !s.Items.Any()) == true)
                errors.Add("At least one field is required");

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// AJAX: Validate template progress before advancing to next stage
        /// Uses direct database counts for reliable validation
        /// </summary>
        [HttpGet("FormTemplates/ValidateStageCompletion")]
        public async Task<IActionResult> ValidateStageCompletion(int id, int currentStage)
        {
            try
            {
                // Validate based on current stage
                switch ((FormBuilderStep)currentStage)
                {
                    case FormBuilderStep.TemplateSetup:
                        // Step 1 validation - Check basic template fields
                        var template = await _context.FormTemplates
                            .Where(t => t.TemplateId == id)
                            .Select(t => new 
                            { 
                                t.TemplateName, 
                                t.TemplateCode, 
                                t.CategoryId,
                                t.TemplateType
                            })
                            .FirstOrDefaultAsync();

                        if (template == null)
                            return Json(new { success = false, isValid = false, message = "Template not found" });

                        bool step1Complete = !string.IsNullOrEmpty(template.TemplateName) &&
                                           !string.IsNullOrEmpty(template.TemplateCode) &&
                                           template.CategoryId > 0 &&
                                           !string.IsNullOrEmpty(template.TemplateType);
                        
                        if (!step1Complete)
                        {
                            return Json(new 
                            { 
                                success = false, 
                                isValid = false,
                                message = "Please complete all required fields (Name, Code, Category, Type) before continuing." 
                            });
                        }
                        return Json(new { success = true, isValid = true, message = "Step 1 validation passed" });

                    case FormBuilderStep.FormBuilder:
                        // Step 2 validation - Use direct database counts
                        var sectionCount = await _context.FormTemplateSections
                            .Where(s => s.TemplateId == id)
                            .CountAsync();

                        var fieldCount = await _context.FormTemplateItems
                            .Where(i => i.TemplateId == id)
                            .CountAsync();

                        // Check if any section has no fields
                        // Get all section IDs for this template
                        var sectionIds = await _context.FormTemplateSections
                            .Where(s => s.TemplateId == id)
                            .Select(s => s.SectionId)
                            .ToListAsync();

                        // Get section IDs that have at least one field
                        var sectionsWithFields = await _context.FormTemplateItems
                            .Where(i => sectionIds.Contains(i.SectionId))
                            .Select(i => i.SectionId)
                            .Distinct()
                            .ToListAsync();

                        // Count sections without fields (client-side evaluation)
                        var sectionsWithoutFields = sectionIds.Count - sectionsWithFields.Count;

                        if (sectionCount == 0)
                        {
                            return Json(new 
                            { 
                                success = false, 
                                isValid = false,
                                message = "You must add at least one section before continuing.",
                                sectionsCount = 0,
                                fieldsCount = 0
                            });
                        }

                        if (fieldCount == 0)
                        {
                            return Json(new 
                            { 
                                success = false, 
                                isValid = false,
                                message = "You must add at least one field to your sections before continuing.",
                                sectionsCount = sectionCount,
                                fieldsCount = 0
                            });
                        }

                        if (sectionsWithoutFields > 0)
                        {
                            return Json(new 
                            { 
                                success = false, 
                                isValid = false,
                                message = $"All sections must have at least one field. {sectionsWithoutFields} section(s) are empty.",
                                sectionsCount = sectionCount,
                                fieldsCount = fieldCount
                            });
                        }

                        // All validation passed
                        return Json(new 
                        { 
                            success = true, 
                            isValid = true,
                            message = "Form Builder validation passed",
                            sectionsCount = sectionCount,
                            fieldsCount = fieldCount,
                            completionPercentage = 28 // Step 2 of 7
                        });

                    default:
                        // Other stages - allow for now
                        return Json(new { success = true, isValid = true, message = "Stage validation passed" });
                }
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                Console.WriteLine($"ValidateStageCompletion Error: {ex}");
                return Json(new
                {
                    success = false,
                    isValid = false,
                    message = $"Error validating template. Please try again.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    sectionsCount = 0,
                    fieldsCount = 0
                });
            }
        }

        /// <summary>
        /// SaveDraft - Save or update template as draft (AJAX endpoint for autosave)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveDraft([FromBody] FormTemplateDraftDto dto)
        {
            try
            {
                if (dto.TemplateId == 0 || dto.TemplateId == null)
                {
                    // CREATE NEW DRAFT
                    var newTemplate = new Models.Entities.Forms.FormTemplate
                    {
                        TemplateName = dto.TemplateName ?? "Untitled Template",
                        TemplateCode = dto.TemplateCode ?? await _templateService.GenerateUniqueTemplateCodeAsync(dto.TemplateName ?? "Template"),
                        Description = dto.Description,
                        CategoryId = dto.CategoryId,
                        TemplateType = dto.TemplateType ?? "Monthly",
                        SubmissionMode = dto.SubmissionMode,
                        AllowAnonymousAccess = dto.AllowAnonymousAccess,
                        Version = 1,
                        PublishStatus = "Draft", // ✅ Save as Draft
                        IsActive = true,
                        CreatedBy = 1, // TODO: Get from current user context
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };

                    _context.FormTemplates.Add(newTemplate);
                    await _context.SaveChangesAsync();

                    // Analyze progress for response
                    var newResumeInfo = _templateService.AnalyzeTemplateProgress(newTemplate);

                    return Json(new
                    {
                        success = true,
                        templateId = newTemplate.TemplateId,
                        message = "Template saved as draft",
                        isNew = true,
                        currentStep = (int)newResumeInfo.CurrentStep,
                        completionPercentage = newResumeInfo.CompletionPercentage
                    });
                }
                else
                {
                    // UPDATE EXISTING DRAFT
                    var template = await _templateService.LoadTemplateForEditingAsync(dto.TemplateId.Value);

                    if (template == null)
                        return Json(new { success = false, message = "Template not found" });

                    if (template.PublishStatus != "Draft")
                        return Json(new { success = false, message = "Cannot edit published template" });

                    // Update fields - only update if values are provided
                    if (!string.IsNullOrWhiteSpace(dto.TemplateName))
                        template.TemplateName = dto.TemplateName;
                    
                    if (!string.IsNullOrWhiteSpace(dto.TemplateCode))
                        template.TemplateCode = dto.TemplateCode;
                    
                    // Description can be null/empty - always update
                    template.Description = dto.Description;

                    if (dto.CategoryId > 0)
                        template.CategoryId = dto.CategoryId;

                    // TemplateType can be null/empty - always update
                    template.TemplateType = dto.TemplateType;

                    // Submission Mode and Anonymous Access - always update
                    template.SubmissionMode = dto.SubmissionMode;
                    template.AllowAnonymousAccess = dto.AllowAnonymousAccess;

                    template.ModifiedDate = DateTime.UtcNow;
                    template.ModifiedBy = 1; // TODO: Get from current user context

                    await _context.SaveChangesAsync();

                    // Analyze progress for response
                    var resumeInfo = _templateService.AnalyzeTemplateProgress(template);

                    return Json(new
                    {
                        success = true,
                        templateId = template.TemplateId,
                        message = "Draft updated",
                        isNew = false,
                        currentStep = (int)resumeInfo.CurrentStep,
                        completionPercentage = resumeInfo.CompletionPercentage
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error saving draft: {ex.Message}" });
            }
        }

        // ============================================================================
        // FIELD OPERATIONS - All field endpoints moved to API/FormBuilderApiController
        // Use RESTful endpoints: /api/formbuilder/fields/*
        // ============================================================================

        /// <summary>
        /// Preview - Display template preview
        /// </summary>
        public async Task<IActionResult> Preview(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _context.FormTemplates
                .Include(t => t.Category)
                .Include(t => t.Sections)
                .Include(t => t.Items)
                .FirstOrDefaultAsync(t => t.TemplateId == id);

            if (template == null)
            {
                return NotFound();
            }

            return View("~/Views/Forms/FormTemplates/Preview.cshtml", template);
        }

        /// <summary>
        /// Edit - Display form for editing an existing template
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _context.FormTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            // Check if template is editable
            if (template.PublishStatus != "Draft")
            {
                TempData["WarningMessage"] = "Published templates are read-only. Create a new version to make structural changes.";
            }

            return View("~/Views/Forms/FormTemplates/Edit.cshtml", template);
        }

        /// <summary>
        /// Clone - Create a copy of an existing template
        /// </summary>
        public async Task<IActionResult> Clone(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _context.FormTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            // TODO: Implement clone logic
            TempData["InfoMessage"] = $"Clone feature for template '{template.TemplateName}' coming soon.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Archive - Archive a published template
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Archive(int id)
        {
            var template = await _context.FormTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            if (template.PublishStatus != "Published")
            {
                TempData["ErrorMessage"] = "Only published templates can be archived.";
                return RedirectToAction(nameof(Index));
            }

            template.PublishStatus = "Archived";
            template.ArchivedDate = DateTime.Now;
            // TODO: Set ArchivedBy to current user ID
            // template.ArchivedBy = CurrentUserId;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Template '{template.TemplateName}' archived successfully.";

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// CreateVersion - Create a new version of an existing template
        /// </summary>
        public async Task<IActionResult> CreateVersion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _context.FormTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            // TODO: Implement version creation logic
            TempData["InfoMessage"] = $"Create new version for template '{template.TemplateName}' coming soon.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Assignments - View template assignments
        /// </summary>
        public async Task<IActionResult> Assignments(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _context.FormTemplates
                .Include(t => t.Assignments)
                .FirstOrDefaultAsync(t => t.TemplateId == id);

            if (template == null)
            {
                return NotFound();
            }

            return View("~/Views/Forms/FormTemplates/Assignments.cshtml", template);
        }

        // ============================================================================
        // AJAX ENDPOINTS FOR TEMPLATE CODE GENERATION
        // ============================================================================

        /// <summary>
        /// AJAX: Check if template code exists
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckTemplateCode(string code, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { exists = false, valid = false, message = "Template code is required" });
            }

            var exists = await _templateService.TemplateCodeExistsAsync(code, excludeId);
            var isValid = _templateService.IsValidTemplateCodeFormat(code);

            return Json(new
            {
                exists = exists,
                valid = isValid,
                message = exists
                    ? "This template code already exists"
                    : isValid
                        ? "Template code is available"
                        : "Invalid format. Use TPL_UPPERCASE_LETTERS_NUMBERS"
            });
        }

        /// <summary>
        /// AJAX: Generate unique template code from name
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateTemplateCode(string name, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, code = "", message = "Template name is required" });
            }

            try
            {
                var code = await _templateService.GenerateUniqueTemplateCodeAsync(name, excludeId);
                return Json(new { success = true, code = code, message = "Code generated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, code = "", message = ex.Message });
            }
        }

        /// <summary>
        /// AJAX: Search assignable entities (Users, Roles, Departments) for AssignmentManager
        /// Returns scope-filtered results based on current user's access
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchAssignableEntities(string query, string type)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new List<object>());
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                return Json(new List<object>());
            }

            try
            {
                switch (type)
                {
                    case "User":
                        // Get users within current user's scope
                        var users = await _userService.SearchUsersAsync(User, query, limit: 20);
                        var userResults = users.Select(u => new
                        {
                            id = u.UserId,
                            type = "User",
                            name = u.FullName,
                            details = u.Email,
                            badge = u.Department?.DepartmentName,
                            tenantId = u.TenantId,
                            tenantName = u.PrimaryTenant?.TenantName
                        });
                        return Json(userResults);

                    case "Role":
                        // Get roles
                        var roles = await _context.Roles
                            .Where(r => r.IsActive && r.RoleName.ToLower().Contains(query.ToLower()))
                            .OrderBy(r => r.RoleName)
                            .Take(20)
                            .Select(r => new
                            {
                                id = r.RoleId,
                                type = "Role",
                                name = r.RoleName,
                                details = r.Description
                            })
                            .ToListAsync();
                        return Json(roles);

                    case "Department":
                        // Get departments (scope-filtered)
                        var departments = await _context.Departments
                            .Include(d => d.Tenant)
                            .Where(d => d.IsActive && d.DepartmentName.ToLower().Contains(query.ToLower()))
                            .OrderBy(d => d.DepartmentName)
                            .Take(20)
                            .Select(d => new
                            {
                                id = d.DepartmentId,
                                type = "Department",
                                name = d.DepartmentName,
                                details = d.Tenant.TenantName
                            })
                            .ToListAsync();
                        return Json(departments);

                    default:
                        return Json(new List<object>());
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// AJAX: Get users grouped by tenant for bulk selection
        /// Used in bulk assignment modals with accordion grouping
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsersGroupedByTenant(string? search = null)
        {
            try
            {
                // Get accessible users with scope filtering
                var users = await _userService.GetAccessibleUsersAsync(User, search);
                
                // Group by tenant
                var groupedUsers = users
                    .GroupBy(u => new { u.TenantId, TenantName = u.PrimaryTenant?.TenantName ?? "No Tenant" })
                    .OrderBy(g => g.Key.TenantName)
                    .Select(g => new
                    {
                        tenantId = g.Key.TenantId,
                        tenantName = g.Key.TenantName,
                        userCount = g.Count(),
                        users = g.Select(u => new
                        {
                            userId = u.UserId,
                            fullName = u.FullName,
                            email = u.Email,
                            employeeNumber = u.UserName, // Assuming UserName contains employee number
                            jobTitle = "", // Add if you have this field
                            departmentName = u.Department?.DepartmentName ?? "No Department"
                        }).ToList()
                    })
                    .ToList();
                
                return Json(groupedUsers);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Configure Metrics - Redirects to MetricMappingController
        /// </summary>
        [HttpGet]
        public IActionResult ConfigureMetrics(int id)
        {
            // Redirect to the new MetricMappingController
            return RedirectToAction("Index", "MetricMapping", new { templateId = id });
        }
    }
}

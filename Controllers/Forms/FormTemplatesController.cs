using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.ViewModels.Components;
using FormReporting.Extensions;
using FormReporting.Services.Forms;
using FormReporting.Services.Identity;

namespace FormReporting.Controllers.Forms
{
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
                    CategoryName = t.Category.CategoryName,
                    TemplateType = t.TemplateType,
                    PublishStatus = t.PublishStatus,
                    Version = t.Version,
                    SubmissionCount = t.Submissions.Count,
                    IsActive = t.IsActive,
                    CreatedBy = t.Creator.FullName ?? "Unknown",
                    ModifiedDate = t.ModifiedDate
                })
                .ToListAsync();

            // Pass pagination info to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            // Pass filter values to view for maintaining state
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentType = type;
            ViewBag.CurrentCategory = category;

            // Get categories for filter dropdown (using service)
            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = categories
                .Select(c => c.CategoryName)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return View("~/Views/Forms/FormTemplates/Index.cshtml", templates);
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
        [HttpGet("Edit/{id}")]
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
            return resumeInfo.CurrentStep switch
            {
                FormBuilderStep.TemplateSetup => await ResumeTemplateSetup(template),
                FormBuilderStep.FormBuilder => RedirectToAction("FormBuilder", new { id }),
                FormBuilderStep.MetricMapping => RedirectToAction("MetricMapping", new { id }),
                FormBuilderStep.ApprovalWorkflow => RedirectToAction("ApprovalWorkflow", new { id }),
                FormBuilderStep.FormAssignments => RedirectToAction("Assignments", new { id }),
                FormBuilderStep.ReportConfiguration => RedirectToAction("ReportConfiguration", new { id }),
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
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.ViewModels.Components;
using FormReporting.Extensions;

namespace FormReporting.Controllers.Forms
{
    public class FormTemplatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FormTemplatesController(ApplicationDbContext context)
        {
            _context = context;
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

            // Get categories for filter dropdown
            ViewBag.Categories = await _context.FormCategories
                .Where(c => c.IsActive)
                .Select(c => c.CategoryName)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View("~/Views/Forms/FormTemplates/Index.cshtml", templates);
        }

        /// <summary>
        /// Create - Display form for creating a new template
        /// </summary>
        public IActionResult Create()
        {
            // Build progress tracker - Step 1 active, no template ID yet
            var progress = new FormBuilderProgressConfig
            {
                BuilderId = Guid.NewGuid().ToString("N"),
                CurrentStep = FormBuilderStep.TemplateConfiguration,
                TemplateId = null,
                TemplateName = null,
                ShowSaveDraft = false, // Can't save draft until template created
                ExitUrl = Url.Action("Index", "FormTemplates") ?? "/Forms/FormTemplates"
            }
            .AtStep(FormBuilderStep.TemplateConfiguration)
            .BuildProgress();

            ViewData["Progress"] = progress;

            return View("~/Views/Forms/FormTemplates/Create.cshtml");
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
    }
}

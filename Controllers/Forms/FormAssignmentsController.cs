using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Services.Forms;
using FormReporting.Services.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FormReporting.Controllers.Forms
{
    /// <summary>
    /// Controller for managing form template assignments
    /// Handles all assignment-related views and operations
    /// </summary>
    [Route("FormTemplates/{templateId}/Assignments")]
    [Authorize]
    public class FormAssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFormAssignmentService _assignmentService;
        private readonly IFormTemplateService _templateService;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<FormAssignmentsController> _logger;

        public FormAssignmentsController(
            ApplicationDbContext context,
            IFormAssignmentService assignmentService,
            IFormTemplateService templateService,
            IClaimsService claimsService,
            ILogger<FormAssignmentsController> logger)
        {
            _context = context;
            _assignmentService = assignmentService;
            _templateService = templateService;
            _claimsService = claimsService;
            _logger = logger;
        }

        #region View Actions

        /// <summary>
        /// GET: /FormTemplates/{templateId}/Assignments
        /// List all assignments for a template
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index(int templateId)
        {
            var template = await _context.FormTemplates
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction("Index", "FormTemplates");
            }

            ViewData["TemplateId"] = templateId;
            ViewData["TemplateName"] = template.TemplateName;
            ViewData["TemplateCode"] = template.TemplateCode;
            ViewData["CategoryName"] = template.Category?.CategoryName;

            // Set breadcrumb
            ViewData["BreadcrumbItems"] = new List<(string Text, string? Url)>
            {
                ("Forms", null),
                ("Templates", Url.Action("Index", "FormTemplates")),
                (template.TemplateName, Url.Action("Details", "FormTemplates", new { id = templateId })),
                ("Assignments", null)
            };

            return View("~/Views/Forms/FormTemplates/Assignments/Index.cshtml");
        }

        /// <summary>
        /// GET: /FormTemplates/{templateId}/Assignments/Create
        /// Show the create assignment wizard
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int templateId)
        {
            var template = await _context.FormTemplates
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction("Index", "FormTemplates");
            }

            ViewData["TemplateId"] = templateId;
            ViewData["TemplateName"] = template.TemplateName;
            ViewData["TemplateCode"] = template.TemplateCode;
            ViewData["CategoryName"] = template.Category?.CategoryName;

            // Set breadcrumb
            ViewData["BreadcrumbItems"] = new List<(string Text, string? Url)>
            {
                ("Forms", null),
                ("Templates", Url.Action("Index", "FormTemplates")),
                (template.TemplateName, Url.Action("Details", "FormTemplates", new { id = templateId })),
                ("Assignments", Url.Action("Index", "FormAssignments", new { templateId })),
                ("Create", null)
            };

            return View("~/Views/Forms/FormTemplates/Assignments/Create.cshtml");
        }

        /// <summary>
        /// GET: /FormTemplates/{templateId}/Assignments/{id}/Edit
        /// Show the edit assignment form
        /// </summary>
        [HttpGet("{id}/Edit")]
        public async Task<IActionResult> Edit(int templateId, int id)
        {
            var template = await _context.FormTemplates
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction("Index", "FormTemplates");
            }

            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                TempData["ErrorMessage"] = "Assignment not found.";
                return RedirectToAction("Index", new { templateId });
            }

            ViewData["TemplateId"] = templateId;
            ViewData["TemplateName"] = template.TemplateName;
            ViewData["TemplateCode"] = template.TemplateCode;
            ViewData["AssignmentId"] = id;
            ViewData["Assignment"] = assignment;

            // Set breadcrumb
            ViewData["BreadcrumbItems"] = new List<(string Text, string? Url)>
            {
                ("Forms", null),
                ("Templates", Url.Action("Index", "FormTemplates")),
                (template.TemplateName, Url.Action("Details", "FormTemplates", new { id = templateId })),
                ("Assignments", Url.Action("Index", "FormAssignments", new { templateId })),
                ("Edit", null)
            };

            return View("~/Views/Forms/FormTemplates/Assignments/Edit.cshtml");
        }

        /// <summary>
        /// GET: /FormTemplates/{templateId}/Assignments/{id}
        /// Show assignment details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int templateId, int id)
        {
            var template = await _context.FormTemplates
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction("Index", "FormTemplates");
            }

            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                TempData["ErrorMessage"] = "Assignment not found.";
                return RedirectToAction("Index", new { templateId });
            }

            ViewData["TemplateId"] = templateId;
            ViewData["TemplateName"] = template.TemplateName;
            ViewData["Assignment"] = assignment;

            // Set breadcrumb
            ViewData["BreadcrumbItems"] = new List<(string Text, string? Url)>
            {
                ("Forms", null),
                ("Templates", Url.Action("Index", "FormTemplates")),
                (template.TemplateName, Url.Action("Details", "FormTemplates", new { id = templateId })),
                ("Assignments", Url.Action("Index", "FormAssignments", new { templateId })),
                ($"Assignment #{id}", null)
            };

            return View("~/Views/Forms/FormTemplates/Assignments/Details.cshtml");
        }

        #endregion

        #region Form Actions (POST/PUT/DELETE)

        /// <summary>
        /// POST: /FormTemplates/{templateId}/Assignments
        /// Create a new assignment
        /// </summary>
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(int templateId, [FromForm] AssignmentCreateDto dto)
        {
            try
            {
                // Log incoming data for debugging
                _logger.LogInformation("Creating assignment for template {TemplateId}: Type={AssignmentType}, TenantType={TenantType}, TenantGroupId={TenantGroupId}, TenantId={TenantId}, RoleId={RoleId}, DepartmentId={DepartmentId}, UserGroupId={UserGroupId}, UserId={UserId}",
                    templateId, dto.AssignmentType, dto.TenantType, dto.TenantGroupId, dto.TenantId, dto.RoleId, dto.DepartmentId, dto.UserGroupId, dto.UserId);

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning("Model validation failed: {Errors}", errors);
                    TempData["ErrorMessage"] = $"Please correct the errors: {errors}";
                    return RedirectToAction("Create", new { templateId });
                }

                dto.TemplateId = templateId;
                var result = await _assignmentService.CreateAssignmentAsync(dto);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Assignment created successfully.";
                    return RedirectToAction("Details", "FormTemplates", new { id = templateId, tab = "assignments" });
                }

                TempData["ErrorMessage"] = "Failed to create assignment - service returned null.";
                return RedirectToAction("Create", new { templateId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assignment for template {TemplateId}: {Message}", templateId, ex.Message);
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Create", new { templateId });
            }
        }

        /// <summary>
        /// POST: /FormTemplates/{templateId}/Assignments/{id}/Update
        /// Update an existing assignment (using POST for form compatibility)
        /// </summary>
        [HttpPost("{id}/Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int templateId, int id, [FromForm] AssignmentUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please correct the errors and try again.";
                    return RedirectToAction("Edit", new { templateId, id });
                }

                var result = await _assignmentService.UpdateAssignmentAsync(id, dto);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Assignment updated successfully.";
                    return RedirectToAction("Details", "FormTemplates", new { id = templateId, tab = "assignments" });
                }

                TempData["ErrorMessage"] = "Failed to update assignment.";
                return RedirectToAction("Edit", new { templateId, id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assignment {AssignmentId}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the assignment.";
                return RedirectToAction("Edit", new { templateId, id });
            }
        }

        /// <summary>
        /// POST: /FormTemplates/{templateId}/Assignments/{id}/Delete
        /// Delete an assignment (using POST for form compatibility)
        /// </summary>
        [HttpPost("{id}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int templateId, int id)
        {
            try
            {
                var success = await _assignmentService.CancelAssignmentAsync(id, "Cancelled by user");

                if (success)
                {
                    TempData["SuccessMessage"] = "Assignment cancelled successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel assignment.";
                }

                return RedirectToAction("Details", "FormTemplates", new { id = templateId, tab = "assignments" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assignment {AssignmentId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the assignment.";
                return RedirectToAction("Details", "FormTemplates", new { id = templateId, tab = "assignments" });
            }
        }

        #endregion
    }
}

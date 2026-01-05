using FormReporting.Data;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Services.Forms;
using FormReporting.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Controllers.Forms
{
    /// <summary>
    /// Controller for managing workflows
    /// Handles all workflow-related views and operations
    /// </summary>
    [Route("Workflows")]
    [Authorize]
    public class WorkflowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _workflowService;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(
            ApplicationDbContext context,
            IWorkflowService workflowService,
            IClaimsService claimsService,
            ILogger<WorkflowController> logger)
        {
            _context = context;
            _workflowService = workflowService;
            _claimsService = claimsService;
            _logger = logger;
        }

        #region View Actions

        /// <summary>
        /// GET: /Workflows
        /// List all workflows
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // TODO: Implement workflow listing
            return View("~/Views/Forms/Workflows/Index.cshtml");
        }

        /// <summary>
        /// GET: /Workflows/Create
        /// Show the create workflow wizard (standalone or with template context)
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create([FromQuery] int? templateId = null)
        {
            // Set ViewData for wizard
            ViewData["TemplateId"] = templateId;
            ViewData["IsEdit"] = false;

            // If templateId is provided, load template details for mode-aware wizard
            if (templateId.HasValue)
            {
                var template = await _context.FormTemplates
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => t.TemplateId == templateId.Value);

                if (template != null)
                {
                    ViewData["TemplateName"] = template.TemplateName;
                    ViewData["SubmissionMode"] = (int)template.SubmissionMode;
                    ViewData["SubmissionModeDisplay"] = template.SubmissionMode.ToString();
                    ViewData["AllowAnonymousAccess"] = template.AllowAnonymousAccess;

                    // Check if template already has a workflow - if so, we're adding a step
                    if (template.WorkflowId.HasValue)
                    {
                        var existingWorkflow = await _workflowService.GetWorkflowByIdAsync(template.WorkflowId.Value);
                        if (existingWorkflow != null)
                        {
                            ViewData["WorkflowName"] = existingWorkflow.WorkflowName;
                            ViewData["WorkflowDescription"] = existingWorkflow.Description;
                            ViewData["ExistingStepCount"] = existingWorkflow.StepCount;
                            ViewData["IsAddingStep"] = true;
                        }
                    }

                    // Set breadcrumb with template context
                    ViewData["BreadcrumbItems"] = new List<(string Text, string? Url)>
                    {
                        ("Forms", null),
                        ("Templates", Url.Action("Index", "FormTemplates")),
                        (template.TemplateName, Url.Action("Details", "FormTemplates", new { id = templateId, tab = "workflow" })),
                        ("Create Workflow", null)
                    };

                    return View("~/Views/Forms/Workflows/Create.cshtml");
                }
            }

            // Fallback breadcrumb for standalone workflow creation
            ViewData["BreadcrumbItems"] = new List<(string Text, string? Url)>
            {
                ("Forms", null),
                ("Workflows", Url.Action("Index", "Workflow")),
                ("Create", null)
            };

            return View("~/Views/Forms/Workflows/Create.cshtml");
        }

        /// <summary>
        /// GET: /FormTemplates/{templateId}/Workflows/Create
        /// Create workflow scoped to a specific template (semantic URL)
        /// </summary>
        [HttpGet("/FormTemplates/{templateId}/Workflows/Create")]
        public async Task<IActionResult> CreateForTemplate(int templateId)
        {
            // Verify template exists
            var template = await _context.FormTemplates
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction("Index", "FormTemplates");
            }

            // Set ViewData for wizard
            ViewData["TemplateId"] = templateId;
            ViewData["TemplateName"] = template.TemplateName;
            ViewData["IsEdit"] = false;

            // Pass submission mode for mode-aware workflow wizard
            ViewData["SubmissionMode"] = (int)template.SubmissionMode; // 1=Individual, 2=Collaborative
            ViewData["SubmissionModeDisplay"] = template.SubmissionMode.ToString(); // "Individual" or "Collaborative"
            ViewData["AllowAnonymousAccess"] = template.AllowAnonymousAccess;

            // Set breadcrumb with template context
            ViewData["BreadcrumbItems"] = new List<(string Text, string? Url)>
            {
                ("Forms", null),
                ("Templates", Url.Action("Index", "FormTemplates")),
                (template.TemplateName, Url.Action("Details", "FormTemplates", new { id = templateId, tab = "workflow" })),
                ("Create Workflow", null)
            };

            return View("~/Views/Forms/Workflows/Create.cshtml");
        }


        /// <summary>
        /// GET: /Workflows/{id}
        /// Show workflow details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var workflow = await _workflowService.GetWorkflowByIdAsync(id);
            if (workflow == null)
            {
                TempData["ErrorMessage"] = "Workflow not found.";
                return RedirectToAction("Index");
            }

            ViewData["Workflow"] = workflow;

            // Set breadcrumb
            ViewData["BreadcrumbItems"] = new List<(string Text, string? Url)>
            {
                ("Forms", null),
                ("Workflows", Url.Action("Index", "Workflow")),
                (workflow.WorkflowName, null)
            };

            return View("~/Views/Forms/Workflows/Details.cshtml");
        }

        #endregion

        #region Form Actions (POST/PUT/DELETE)

        /// <summary>
        /// POST: /Workflows
        /// Create a new workflow
        /// </summary>
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(
            [FromForm] WorkflowStepCreateDto stepData,
            [FromForm] int? templateId)
        {
            try
            {
                if (!templateId.HasValue)
                {
                    TempData["ErrorMessage"] = "Template ID is required for workflow creation.";
                    return RedirectToAction("Create");
                }

                // Get template details for auto-generating workflow name
                var template = await _context.FormTemplates.FindAsync(templateId.Value);
                if (template == null)
                {
                    TempData["ErrorMessage"] = "Template not found.";
                    return RedirectToAction("Index", "FormTemplates");
                }

                // VALIDATE BEFORE SAVE - prevents invalid data from being persisted
                var validation = await _workflowService.ValidateBeforeCreateAsync(stepData, templateId.Value);
                if (!validation.IsValid)
                {
                    TempData["ErrorMessage"] = string.Join(", ", validation.Errors);
                    return RedirectToAction("Create", new { templateId });
                }

                // Auto-generate workflow name and description
                var workflowName = $"{template.TemplateName} Workflow";
                var description = $"Workflow for {template.TemplateName}";

                // Create workflow with first step from wizard data (validation already passed)
                _logger.LogInformation("Creating workflow for template {TemplateId} with step: {StepName}", templateId.Value, stepData.StepName);
                
                var result = await _workflowService.CreateWorkflowAsync(new Models.ViewModels.Forms.WorkflowCreateDto
                {
                    WorkflowName = workflowName,
                    Description = description,
                    TemplateId = templateId.Value,
                    Steps = new List<WorkflowStepCreateDto> { stepData }
                });

                _logger.LogInformation("CreateWorkflowAsync returned: {Result}", result != null ? $"WorkflowId={result.WorkflowId}, Name={result.WorkflowName}" : "NULL");

                if (result != null)
                {
                    // Check if this was a new workflow creation or step addition
                    var isNewWorkflow = !template.WorkflowId.HasValue || template.WorkflowId != result.WorkflowId;
                    var successMessage = isNewWorkflow 
                        ? $"Workflow '{result.WorkflowName}' created and assigned to template successfully."
                        : $"Step '{stepData.StepName}' added to workflow '{result.WorkflowName}' successfully.";

                    _logger.LogInformation("Workflow created successfully. Redirecting to template details.");
                    TempData["SuccessMessage"] = successMessage;
                    return RedirectToAction("Details", "FormTemplates", new { id = templateId, tab = "workflow" });
                }

                _logger.LogWarning("CreateWorkflowAsync returned null - this should not happen");
                TempData["ErrorMessage"] = "Failed to create workflow. (result was null)";
                return RedirectToAction("Create", new { templateId });
            }
            catch (Exception ex)
            {
                // Log full exception with inner exception details
                var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error creating workflow. Inner: {InnerMessage}", innerMessage);
                
                // Show more detailed error to help debugging
                var errorMessage = ex.InnerException != null 
                    ? $"Error: {ex.InnerException.Message}" 
                    : $"Error: {ex.Message}";
                    
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("Create", new { templateId });
            }
        }

        /// <summary>
        /// GET: /Workflows/{id}/Edit
        /// Show edit workflow page
        /// </summary>
        [HttpGet("{id}/Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var workflow = await _workflowService.GetWorkflowByIdAsync(id);
                if (workflow == null)
                {
                    TempData["ErrorMessage"] = "Workflow not found.";
                    return RedirectToAction("Index");
                }

                var model = new WorkflowEditViewModel
                {
                    WorkflowId = workflow.WorkflowId,
                    WorkflowName = workflow.WorkflowName,
                    Description = workflow.Description,
                    IsActive = workflow.IsActive
                };

                return View("~/Views/Forms/Workflows/Edit.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading workflow {WorkflowId} for edit", id);
                TempData["ErrorMessage"] = "An error occurred while loading the workflow.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST: /Workflows/{id}/Update
        /// Update an existing workflow
        /// </summary>
        [HttpPost("{id}/Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromForm] string workflowName, [FromForm] string? description, [FromForm] bool isActive)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workflowName))
                {
                    TempData["ErrorMessage"] = "Workflow name is required.";
                    return RedirectToAction("Edit", new { id });
                }

                var result = await _workflowService.UpdateWorkflowAsync(id, new Models.ViewModels.Forms.WorkflowUpdateDto
                {
                    WorkflowName = workflowName,
                    Description = description,
                    IsActive = isActive
                });

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Workflow updated successfully.";
                    return RedirectToAction("Details", new { id });
                }

                TempData["ErrorMessage"] = "Failed to update workflow.";
                return RedirectToAction("Edit", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow {WorkflowId}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the workflow.";
                return RedirectToAction("Edit", new { id });
            }
        }

        /// <summary>
        /// POST: /Workflows/{id}/Delete
        /// Delete a workflow
        /// </summary>
        [HttpPost("{id}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _workflowService.DeleteWorkflowAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Workflow deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete workflow. It may be in use by templates.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow {WorkflowId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the workflow.";
                return RedirectToAction("Details", new { id });
            }
        }

        #endregion
    }
}

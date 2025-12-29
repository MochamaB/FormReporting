using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FormReporting.Services.Forms;
using FormReporting.Services.Identity;
using FormReporting.Services.Organizational;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.Entities.Identity;
using FormReporting.Data;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Controllers.API
{
    /// <summary>
    /// API Controller for Workflow Definition management
    /// Handles workflow CRUD, step management, and validation
    /// </summary>
    [ApiController]
    [Route("api/workflows")]
    [Authorize]
    public class WorkflowApiController : Controller
    {
        private readonly IWorkflowService _workflowService;
        private readonly ApplicationDbContext _context;
        private readonly IClaimsService _claimsService;
        private readonly IScopeService _scopeService;
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<WorkflowApiController> _logger;

        public WorkflowApiController(
            IWorkflowService workflowService,
            ApplicationDbContext context,
            IClaimsService claimsService,
            IScopeService scopeService,
            IUserService userService,
            IDepartmentService departmentService,
            ILogger<WorkflowApiController> logger)
        {
            _workflowService = workflowService;
            _context = context;
            _claimsService = claimsService;
            _scopeService = scopeService;
            _userService = userService;
            _departmentService = departmentService;
            _logger = logger;
        }

        #region Workflow CRUD

        /// <summary>
        /// Get all workflows (for dropdown/lookup)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWorkflows([FromQuery] bool? isActive)
        {
            try
            {
                var workflows = await _workflowService.GetWorkflowsAsync(isActive);
                return Ok(new { success = true, data = workflows });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflows");
                return StatusCode(500, new { success = false, message = "Error retrieving workflows" });
            }
        }

        /// <summary>
        /// Get workflows for dropdown (simplified)
        /// </summary>
        [HttpGet("lookup")]
        public async Task<IActionResult> GetWorkflowsLookup()
        {
            try
            {
                var workflows = await _context.WorkflowDefinitions
                    .Where(w => w.IsActive)
                    .OrderBy(w => w.WorkflowName)
                    .Select(w => new
                    {
                        workflowId = w.WorkflowId,
                        workflowName = w.WorkflowName,
                        description = w.Description,
                        stepCount = w.Steps.Count
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = workflows });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow lookup");
                return StatusCode(500, new { success = false, message = "Error retrieving workflows" });
            }
        }

        /// <summary>
        /// Get workflow by ID with all steps
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkflow(int id)
        {
            try
            {
                var workflow = await _workflowService.GetWorkflowByIdAsync(id);

                if (workflow == null)
                    return NotFound(new { success = false, message = "Workflow not found" });

                return Ok(new { success = true, data = workflow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow {WorkflowId}", id);
                return StatusCode(500, new { success = false, message = "Error retrieving workflow" });
            }
        }

        // NOTE: Workflow CRUD operations (Create, Update, Delete) are handled by
        // the MVC WorkflowController at /Workflows. This API controller only provides
        // helper endpoints for the wizard and workflow panel.

        #endregion

        #region Step Management

        /// <summary>
        /// Add a step to a workflow
        /// </summary>
        [HttpPost("{workflowId}/steps")]
        public async Task<IActionResult> AddStep(int workflowId, [FromBody] WorkflowStepCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data", errors = ModelState });

            try
            {
                var step = await _workflowService.AddStepAsync(workflowId, dto);
                return Ok(new { success = true, data = step, message = "Step added successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding step to workflow {WorkflowId}", workflowId);
                return StatusCode(500, new { success = false, message = "Error adding step" });
            }
        }

        /// <summary>
        /// Update a workflow step
        /// </summary>
        [HttpPut("steps/{stepId}")]
        public async Task<IActionResult> UpdateStep(int stepId, [FromBody] WorkflowStepUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data", errors = ModelState });

            try
            {
                var step = await _workflowService.UpdateStepAsync(stepId, dto);

                if (step == null)
                    return NotFound(new { success = false, message = "Step not found" });

                return Ok(new { success = true, data = step, message = "Step updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating step {StepId}", stepId);
                return StatusCode(500, new { success = false, message = "Error updating step" });
            }
        }

        /// <summary>
        /// Delete a workflow step
        /// </summary>
        [HttpDelete("steps/{stepId}")]
        public async Task<IActionResult> DeleteStep(int stepId)
        {
            try
            {
                var result = await _workflowService.DeleteStepAsync(stepId);

                if (!result)
                    return BadRequest(new { success = false, message = "Cannot delete step - it may have progress records or not exist" });

                return Ok(new { success = true, message = "Step deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting step {StepId}", stepId);
                return StatusCode(500, new { success = false, message = "Error deleting step" });
            }
        }

        /// <summary>
        /// Reorder workflow steps
        /// </summary>
        [HttpPost("{workflowId}/steps/reorder")]
        public async Task<IActionResult> ReorderSteps(int workflowId, [FromBody] List<StepReorderDto> newOrders)
        {
            try
            {
                var result = await _workflowService.ReorderStepsAsync(workflowId, newOrders);

                if (!result)
                    return BadRequest(new { success = false, message = "Error reordering steps" });

                return Ok(new { success = true, message = "Steps reordered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering steps for workflow {WorkflowId}", workflowId);
                return StatusCode(500, new { success = false, message = "Error reordering steps" });
            }
        }

        #endregion

        #region Validation & Utilities

        /// <summary>
        /// Validate workflow configuration
        /// </summary>
        [HttpGet("{id}/validate")]
        public async Task<IActionResult> ValidateWorkflow(int id)
        {
            try
            {
                var result = await _workflowService.ValidateWorkflowAsync(id);
                return Ok(new { success = result.IsValid, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating workflow {WorkflowId}", id);
                return StatusCode(500, new { success = false, message = "Error validating workflow" });
            }
        }

        /// <summary>
        /// Clone an existing workflow
        /// </summary>
        [HttpPost("{id}/clone")]
        public async Task<IActionResult> CloneWorkflow(int id, [FromBody] CloneWorkflowRequest request)
        {
            try
            {
                var workflow = await _workflowService.CloneWorkflowAsync(id, request.NewName);

                return CreatedAtAction(
                    nameof(GetWorkflow),
                    new { id = workflow.WorkflowId },
                    new { success = true, data = workflow, message = "Workflow cloned successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning workflow {WorkflowId}", id);
                return StatusCode(500, new { success = false, message = "Error cloning workflow" });
            }
        }

        #endregion

        #region Template-Specific Endpoints

        /// <summary>
        /// Get workflow for a specific template
        /// </summary>
        [HttpGet("template/{templateId}")]
        public async Task<IActionResult> GetTemplateWorkflow(int templateId)
        {
            try
            {
                var template = await _context.FormTemplates
                    .Include(t => t.Workflow)
                        .ThenInclude(w => w!.Steps.OrderBy(s => s.StepOrder))
                            .ThenInclude(s => s.Action)
                    .FirstOrDefaultAsync(t => t.TemplateId == templateId);

                if (template == null)
                    return NotFound(new { success = false, message = "Template not found" });

                if (template.WorkflowId == null)
                    return Ok(new { success = true, data = (object?)null, message = "No workflow assigned to this template" });

                var workflow = await _workflowService.GetWorkflowByIdAsync(template.WorkflowId.Value);
                return Ok(new { success = true, data = workflow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error retrieving template workflow" });
            }
        }

        /// <summary>
        /// Get sections for a template (for workflow target selection)
        /// </summary>
        [HttpGet("templates/{templateId}/sections")]
        public async Task<IActionResult> GetTemplateSections(int templateId)
        {
            try
            {
                var sections = await _context.FormTemplateSections
                    .Where(s => s.TemplateId == templateId)
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new
                    {
                        sectionId = s.SectionId,
                        sectionName = s.SectionName,
                        sectionDescription = s.SectionDescription,
                        displayOrder = s.DisplayOrder,
                        fieldCount = s.Items.Count
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = sections });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sections for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error retrieving sections" });
            }
        }

        /// <summary>
        /// Get fields for a template (for workflow target selection)
        /// Optionally filter by section
        /// </summary>
        [HttpGet("templates/{templateId}/fields")]
        public async Task<IActionResult> GetTemplateFields(int templateId, [FromQuery] int? sectionId = null)
        {
            try
            {
                var query = _context.FormTemplateItems
                    .Include(i => i.Section)
                    .Where(i => i.TemplateId == templateId);

                if (sectionId.HasValue)
                {
                    query = query.Where(i => i.SectionId == sectionId.Value);
                }

                var fields = await query
                    .OrderBy(i => i.Section.DisplayOrder)
                    .ThenBy(i => i.DisplayOrder)
                    .Select(i => new
                    {
                        itemId = i.ItemId,
                        itemCode = i.ItemCode,
                        itemName = i.ItemName,
                        dataType = i.DataType,
                        sectionId = i.SectionId,
                        sectionName = i.Section.SectionName,
                        displayOrder = i.DisplayOrder,
                        isRequired = i.IsRequired
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = fields });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fields for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error retrieving fields" });
            }
        }

        /// <summary>
        /// Get available workflow actions (for wizard action selection)
        /// </summary>
        [HttpGet("actions")]
        public async Task<IActionResult> GetWorkflowActions()
        {
            try
            {
                var actions = await _context.WorkflowActions
                    .Where(a => a.IsActive)
                    .OrderBy(a => a.DisplayOrder)
                    .Select(a => new
                    {
                        actionId = a.ActionId,
                        actionCode = a.ActionCode,
                        actionName = a.ActionName,
                        description = a.Description,
                        requiresSignature = a.RequiresSignature,
                        requiresComment = a.RequiresComment,
                        allowDelegate = a.AllowDelegate,
                        iconClass = a.IconClass,
                        cssClass = a.CssClass
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = actions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow actions");
                return StatusCode(500, new { success = false, message = "Error retrieving workflow actions" });
            }
        }

        /// <summary>
        /// Get available roles (for wizard assignee selection) - SCOPE FILTERED
        /// Only returns roles within user's access scope
        /// </summary>
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                // Get user's scope level using scope service
                var userScopeLevel = await _scopeService.GetUserScopeLevelAsync(User);

                if (userScopeLevel == null)
                {
                    _logger.LogWarning("User has no scope level, returning empty roles list");
                    return Ok(new { success = true, data = new List<object>() });
                }

                // Filter roles by scope level (user can only assign to roles at their level or below)
                // Compare using Level property: lower number = broader access (1=Global, 6=Individual)
                var roles = await _context.Roles
                    .Include(r => r.ScopeLevel)
                    .Where(r => r.IsActive && r.ScopeLevel.Level <= userScopeLevel.Level)
                    .OrderBy(r => r.RoleName)
                    .Select(r => new
                    {
                        roleId = r.RoleId,
                        roleName = r.RoleName,
                        roleCode = r.RoleCode,
                        description = r.Description,
                        scopeLevel = r.ScopeLevel.ScopeName
                    })
                    .ToListAsync();

                _logger.LogInformation("User with scope {ScopeName} (Level {Level}) retrieved {Count} roles",
                    userScopeLevel.ScopeName, userScopeLevel.Level, roles.Count);

                return Ok(new { success = true, data = roles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return StatusCode(500, new { success = false, message = "Error retrieving roles" });
            }
        }

        /// <summary>
        /// Get available users (for wizard assignee selection) - SCOPE FILTERED
        /// Only returns users accessible based on current user's scope
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                // Use user service to get accessible users based on scope
                var accessibleUsers = await _userService.GetAccessibleUsersAsync(User);

                // Map to response format with department info
                var users = accessibleUsers
                    .Select(u => new
                    {
                        userId = u.UserId,
                        userName = u.UserName,
                        fullName = u.FullName,
                        email = u.Email,
                        departmentId = u.DepartmentId,
                        departmentName = u.Department?.DepartmentName
                    })
                    .OrderBy(u => u.fullName)
                    .ToList();

                _logger.LogInformation("User retrieved {Count} accessible users", users.Count);

                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { success = false, message = "Error retrieving users" });
            }
        }

        /// <summary>
        /// Get available departments (for wizard assignee selection) - SCOPE FILTERED
        /// Only returns departments accessible based on current user's scope
        /// </summary>
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                // Use department service to get accessible departments based on scope
                var accessibleDepartments = await _departmentService.GetAccessibleDepartmentsAsync(User);

                // Map to response format with user count
                var departments = accessibleDepartments
                    .Select(d => new
                    {
                        departmentId = d.DepartmentId,
                        departmentName = d.DepartmentName,
                        departmentCode = d.DepartmentCode,
                        tenantId = d.TenantId,
                        userCount = d.Users?.Count(u => u.IsActive) ?? 0
                    })
                    .OrderBy(d => d.departmentName)
                    .ToList();

                _logger.LogInformation("User retrieved {Count} accessible departments", departments.Count);

                return Ok(new { success = true, data = departments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments");
                return StatusCode(500, new { success = false, message = "Error retrieving departments" });
            }
        }

        /// <summary>
        /// Validate workflow step configuration in real-time
        /// </summary>
        [HttpPost("validate-step")]
        public async Task<IActionResult> ValidateWorkflowStep([FromBody] StepValidationDto dto)
        {
            try
            {
                // Use new simplified validation that works directly with DTO
                var result = await _workflowService.ValidateStepDataAsync(dto, dto.TemplateId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating workflow step");
                return BadRequest(new { 
                    IsValid = false, 
                    StepId = dto.StepId,
                    Errors = new List<string> { ex.Message },
                    Warnings = new List<string>()
                });
            }
        }

        /// <summary>
        /// Delete a workflow and all its steps
        /// </summary>
        [HttpDelete("{workflowId}")]
        public async Task<IActionResult> DeleteWorkflow(int workflowId)
        {
            try
            {
                var success = await _workflowService.DeleteWorkflowAsync(workflowId);
                if (success)
                {
                    _logger.LogInformation("Workflow {WorkflowId} deleted successfully", workflowId);
                    return Ok(new { success = true, message = "Workflow deleted successfully" });
                }
                else
                {
                    return NotFound(new { success = false, message = "Workflow not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow {WorkflowId}", workflowId);
                return StatusCode(500, new { success = false, message = "Error deleting workflow" });
            }
        }

        #endregion
    }

    #region Request Models

    public class CloneWorkflowRequest
    {
        public string NewName { get; set; } = string.Empty;
    }

    #endregion
}

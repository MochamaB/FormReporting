using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FormReporting.Services.Forms;
using FormReporting.Models.ViewModels.Forms;

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
        private readonly ILogger<WorkflowApiController> _logger;

        public WorkflowApiController(
            IWorkflowService workflowService,
            ILogger<WorkflowApiController> logger)
        {
            _workflowService = workflowService;
            _logger = logger;
        }

        #region Workflow CRUD

        /// <summary>
        /// Get all workflows
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

        /// <summary>
        /// Create a new workflow
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflow([FromBody] WorkflowCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data", errors = ModelState });

            try
            {
                var workflow = await _workflowService.CreateWorkflowAsync(dto);

                return CreatedAtAction(
                    nameof(GetWorkflow),
                    new { id = workflow.WorkflowId },
                    new { success = true, data = workflow, message = "Workflow created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workflow");
                return StatusCode(500, new { success = false, message = "Error creating workflow" });
            }
        }

        /// <summary>
        /// Update workflow details
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkflow(int id, [FromBody] WorkflowUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data", errors = ModelState });

            try
            {
                var workflow = await _workflowService.UpdateWorkflowAsync(id, dto);

                if (workflow == null)
                    return NotFound(new { success = false, message = "Workflow not found" });

                return Ok(new { success = true, data = workflow, message = "Workflow updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workflow {WorkflowId}", id);
                return StatusCode(500, new { success = false, message = "Error updating workflow" });
            }
        }

        /// <summary>
        /// Delete a workflow
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkflow(int id)
        {
            try
            {
                var result = await _workflowService.DeleteWorkflowAsync(id);

                if (!result)
                    return NotFound(new { success = false, message = "Workflow not found" });

                return Ok(new { success = true, message = "Workflow deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow {WorkflowId}", id);
                return StatusCode(500, new { success = false, message = "Error deleting workflow" });
            }
        }

        /// <summary>
        /// Check if workflow can be deleted
        /// </summary>
        [HttpGet("{id}/can-delete")]
        public async Task<IActionResult> CanDeleteWorkflow(int id)
        {
            try
            {
                var canDelete = await _workflowService.CanDeleteWorkflowAsync(id);
                return Ok(new { success = true, canDelete });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if workflow {WorkflowId} can be deleted", id);
                return StatusCode(500, new { success = false, message = "Error checking delete status" });
            }
        }

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

        /// <summary>
        /// Get all available workflow actions
        /// </summary>
        [HttpGet("actions")]
        public async Task<IActionResult> GetWorkflowActions()
        {
            try
            {
                var actions = await _workflowService.GetWorkflowActionsAsync();
                return Ok(new { success = true, data = actions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow actions");
                return StatusCode(500, new { success = false, message = "Error retrieving workflow actions" });
            }
        }

        #endregion

        #region Lookup Data

        /// <summary>
        /// Get workflow for dropdown/select
        /// </summary>
        [HttpGet("lookup")]
        public async Task<IActionResult> GetWorkflowLookup()
        {
            try
            {
                var workflows = await _workflowService.GetWorkflowsAsync(isActive: true);
                var lookup = workflows.Select(w => new
                {
                    value = w.WorkflowId,
                    text = w.WorkflowName,
                    description = w.Description,
                    stepCount = w.StepCount
                });

                return Ok(new { success = true, data = lookup });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow lookup");
                return StatusCode(500, new { success = false, message = "Error retrieving workflows" });
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

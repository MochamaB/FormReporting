using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FormReporting.Services.Forms;
using FormReporting.Models.ViewModels.Forms;

namespace FormReporting.Controllers.API
{
    /// <summary>
    /// API Controller for Workflow Engine operations
    /// Handles runtime workflow execution, step actions, and progress tracking
    /// </summary>
    [ApiController]
    [Route("api/workflow-engine")]
    [Authorize]
    public class WorkflowEngineApiController : Controller
    {
        private readonly IWorkflowEngineService _workflowEngineService;
        private readonly ILogger<WorkflowEngineApiController> _logger;

        public WorkflowEngineApiController(
            IWorkflowEngineService workflowEngineService,
            ILogger<WorkflowEngineApiController> logger)
        {
            _workflowEngineService = workflowEngineService;
            _logger = logger;
        }

        #region Workflow Initialization

        /// <summary>
        /// Initialize workflow for a submission
        /// </summary>
        [HttpPost("submissions/{submissionId}/initialize")]
        public async Task<IActionResult> InitializeWorkflow(int submissionId)
        {
            try
            {
                var progress = await _workflowEngineService.InitializeSubmissionWorkflowAsync(submissionId);
                return Ok(new { success = true, data = progress, message = "Workflow initialized successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing workflow for submission {SubmissionId}", submissionId);
                return StatusCode(500, new { success = false, message = "Error initializing workflow" });
            }
        }

        /// <summary>
        /// Get workflow progress for a submission
        /// </summary>
        [HttpGet("submissions/{submissionId}/progress")]
        public async Task<IActionResult> GetSubmissionProgress(int submissionId)
        {
            try
            {
                var progress = await _workflowEngineService.GetSubmissionProgressAsync(submissionId);

                if (progress == null)
                    return NotFound(new { success = false, message = "No workflow progress found for this submission" });

                return Ok(new { success = true, data = progress });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress for submission {SubmissionId}", submissionId);
                return StatusCode(500, new { success = false, message = "Error retrieving progress" });
            }
        }

        /// <summary>
        /// Get current active steps for a submission
        /// </summary>
        [HttpGet("submissions/{submissionId}/current-steps")]
        public async Task<IActionResult> GetCurrentSteps(int submissionId)
        {
            try
            {
                var steps = await _workflowEngineService.GetCurrentStepsAsync(submissionId);
                return Ok(new { success = true, data = steps });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current steps for submission {SubmissionId}", submissionId);
                return StatusCode(500, new { success = false, message = "Error retrieving current steps" });
            }
        }

        /// <summary>
        /// Get workflow status for a submission
        /// </summary>
        [HttpGet("submissions/{submissionId}/status")]
        public async Task<IActionResult> GetWorkflowStatus(int submissionId)
        {
            try
            {
                var status = await _workflowEngineService.GetWorkflowStatusAsync(submissionId);
                var isComplete = await _workflowEngineService.IsWorkflowCompleteAsync(submissionId);

                return Ok(new { success = true, status, isComplete });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workflow status for submission {SubmissionId}", submissionId);
                return StatusCode(500, new { success = false, message = "Error retrieving status" });
            }
        }

        #endregion

        #region Step Actions

        /// <summary>
        /// Complete a workflow step (approve, sign, fill, etc.)
        /// </summary>
        [HttpPost("steps/complete")]
        public async Task<IActionResult> CompleteStep([FromBody] StepCompleteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data", errors = ModelState });

            try
            {
                var step = await _workflowEngineService.CompleteStepAsync(dto);
                return Ok(new { success = true, data = step, message = "Step completed successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing step {ProgressId}", dto.ProgressId);
                return StatusCode(500, new { success = false, message = "Error completing step" });
            }
        }

        /// <summary>
        /// Reject a workflow step
        /// </summary>
        [HttpPost("steps/reject")]
        public async Task<IActionResult> RejectStep([FromBody] StepRejectDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data", errors = ModelState });

            try
            {
                var step = await _workflowEngineService.RejectStepAsync(dto);
                return Ok(new { success = true, data = step, message = "Step rejected" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting step {ProgressId}", dto.ProgressId);
                return StatusCode(500, new { success = false, message = "Error rejecting step" });
            }
        }

        /// <summary>
        /// Delegate a workflow step to another user
        /// </summary>
        [HttpPost("steps/delegate")]
        public async Task<IActionResult> DelegateStep([FromBody] StepDelegateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data", errors = ModelState });

            try
            {
                var step = await _workflowEngineService.DelegateStepAsync(dto);
                return Ok(new { success = true, data = step, message = "Step delegated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delegating step {ProgressId}", dto.ProgressId);
                return StatusCode(500, new { success = false, message = "Error delegating step" });
            }
        }

        /// <summary>
        /// Get step progress by ID
        /// </summary>
        [HttpGet("steps/{progressId}")]
        public async Task<IActionResult> GetStepProgress(int progressId)
        {
            try
            {
                var step = await _workflowEngineService.GetStepProgressAsync(progressId);

                if (step == null)
                    return NotFound(new { success = false, message = "Step progress not found" });

                return Ok(new { success = true, data = step });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting step progress {ProgressId}", progressId);
                return StatusCode(500, new { success = false, message = "Error retrieving step progress" });
            }
        }

        #endregion

        #region User Queries

        /// <summary>
        /// Get pending actions for current user
        /// </summary>
        [HttpGet("my-pending-actions")]
        public async Task<IActionResult> GetMyPendingActions()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var actions = await _workflowEngineService.GetPendingActionsForUserAsync(userId);
                return Ok(new { success = true, data = actions, count = actions.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending actions");
                return StatusCode(500, new { success = false, message = "Error retrieving pending actions" });
            }
        }

        /// <summary>
        /// Get pending action count for current user (for badges/notifications)
        /// </summary>
        [HttpGet("my-pending-count")]
        public async Task<IActionResult> GetMyPendingCount()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var count = await _workflowEngineService.GetPendingActionCountAsync(userId);
                return Ok(new { success = true, count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending action count");
                return StatusCode(500, new { success = false, message = "Error retrieving count" });
            }
        }

        /// <summary>
        /// Check if current user can act on a specific step
        /// </summary>
        [HttpGet("steps/{progressId}/can-act")]
        public async Task<IActionResult> CanActOnStep(int progressId)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var canAct = await _workflowEngineService.CanUserActOnStepAsync(userId, progressId);
                return Ok(new { success = true, canAct });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user can act on step {ProgressId}", progressId);
                return StatusCode(500, new { success = false, message = "Error checking permissions" });
            }
        }

        /// <summary>
        /// Check if current user can act on a section
        /// </summary>
        [HttpGet("submissions/{submissionId}/sections/{sectionId}/can-act")]
        public async Task<IActionResult> CanActOnSection(int submissionId, int sectionId)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var canAct = await _workflowEngineService.CanUserActOnSectionAsync(userId, submissionId, sectionId);
                return Ok(new { success = true, canAct });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user can act on section {SectionId}", sectionId);
                return StatusCode(500, new { success = false, message = "Error checking permissions" });
            }
        }

        /// <summary>
        /// Check if current user can act on a field
        /// </summary>
        [HttpGet("submissions/{submissionId}/fields/{fieldId}/can-act")]
        public async Task<IActionResult> CanActOnField(int submissionId, int fieldId)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var canAct = await _workflowEngineService.CanUserActOnFieldAsync(userId, submissionId, fieldId);
                return Ok(new { success = true, canAct });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user can act on field {FieldId}", fieldId);
                return StatusCode(500, new { success = false, message = "Error checking permissions" });
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Check if step dependencies are met
        /// </summary>
        [HttpGet("steps/{progressId}/check-dependencies")]
        public async Task<IActionResult> CheckStepDependencies(int progressId)
        {
            try
            {
                var dependenciesMet = await _workflowEngineService.CheckStepDependenciesAsync(progressId);
                return Ok(new { success = true, dependenciesMet });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking dependencies for step {ProgressId}", progressId);
                return StatusCode(500, new { success = false, message = "Error checking dependencies" });
            }
        }

        #endregion
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FormReporting.Services.Forms;
using FormReporting.Models.ViewModels.Forms;

namespace FormReporting.Controllers.API
{
    /// <summary>
    /// API Controller for Form Template Assignment operations
    /// Handles assignment CRUD, target preview, and bulk operations
    /// </summary>
    [ApiController]
    [Route("api/assignments")]
    [Authorize]
    public class AssignmentsApiController : Controller
    {
        private readonly IFormAssignmentService _assignmentService;
        private readonly ILogger<AssignmentsApiController> _logger;

        public AssignmentsApiController(
            IFormAssignmentService assignmentService,
            ILogger<AssignmentsApiController> logger)
        {
            _assignmentService = assignmentService;
            _logger = logger;
        }

        #region CRUD Operations

        /// <summary>
        /// Get assignments with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAssignments([FromQuery] AssignmentFilterDto filter)
        {
            try
            {
                _logger.LogInformation("GetAssignments called with TemplateId={TemplateId}, Page={Page}, PageSize={PageSize}",
                    filter.TemplateId, filter.Page, filter.PageSize);

                var (items, totalCount) = await _assignmentService.GetAssignmentsAsync(filter);

                _logger.LogInformation("GetAssignments returning {Count} items, TotalCount={TotalCount}", 
                    items.Count, totalCount);

                return Ok(new
                {
                    success = true,
                    data = items,
                    pagination = new
                    {
                        page = filter.Page,
                        pageSize = filter.PageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignments: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Error retrieving assignments: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get assignment by ID with full details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssignment(int id)
        {
            try
            {
                var assignment = await _assignmentService.GetAssignmentByIdAsync(id);

                if (assignment == null)
                    return NotFound(new { success = false, message = "Assignment not found" });

                return Ok(new { success = true, data = assignment });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignment {AssignmentId}", id);
                return StatusCode(500, new { success = false, message = "Error retrieving assignment" });
            }
        }

        /// <summary>
        /// Create a new assignment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAssignment([FromBody] AssignmentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data", errors = ModelState });

            try
            {
                var assignment = await _assignmentService.CreateAssignmentAsync(dto);

                return CreatedAtAction(
                    nameof(GetAssignment),
                    new { id = assignment.AssignmentId },
                    new { success = true, data = assignment, message = "Assignment created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assignment");
                return StatusCode(500, new { success = false, message = "Error creating assignment" });
            }
        }

        /// <summary>
        /// Update an existing assignment
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssignment(int id, [FromBody] AssignmentUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data", errors = ModelState });

            try
            {
                var assignment = await _assignmentService.UpdateAssignmentAsync(id, dto);

                if (assignment == null)
                    return NotFound(new { success = false, message = "Assignment not found" });

                return Ok(new { success = true, data = assignment, message = "Assignment updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assignment {AssignmentId}", id);
                return StatusCode(500, new { success = false, message = "Error updating assignment" });
            }
        }

        /// <summary>
        /// Cancel an assignment
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelAssignment(int id, [FromBody] CancelAssignmentRequest request)
        {
            try
            {
                var result = await _assignmentService.CancelAssignmentAsync(id, request.Reason);

                if (!result)
                    return NotFound(new { success = false, message = "Assignment not found" });

                return Ok(new { success = true, message = "Assignment cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling assignment {AssignmentId}", id);
                return StatusCode(500, new { success = false, message = "Error cancelling assignment" });
            }
        }

        /// <summary>
        /// Extend assignment effective period
        /// </summary>
        [HttpPost("{id}/extend")]
        public async Task<IActionResult> ExtendEffectivePeriod(int id, [FromBody] ExtendEffectivePeriodRequest request)
        {
            try
            {
                var result = await _assignmentService.ExtendEffectivePeriodAsync(id, request.NewEffectiveUntil);

                if (!result)
                    return NotFound(new { success = false, message = "Assignment not found" });

                return Ok(new { success = true, message = "Effective period extended successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending effective period for assignment {AssignmentId}", id);
                return StatusCode(500, new { success = false, message = "Error extending effective period" });
            }
        }

        /// <summary>
        /// Suspend an assignment
        /// </summary>
        [HttpPost("{id}/suspend")]
        public async Task<IActionResult> SuspendAssignment(int id, [FromBody] SuspendAssignmentRequest? request)
        {
            try
            {
                var result = await _assignmentService.SuspendAssignmentAsync(id, request?.Reason);

                if (!result)
                    return NotFound(new { success = false, message = "Assignment not found" });

                return Ok(new { success = true, message = "Assignment suspended successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending assignment {AssignmentId}", id);
                return StatusCode(500, new { success = false, message = "Error suspending assignment" });
            }
        }

        /// <summary>
        /// Reactivate a suspended assignment
        /// </summary>
        [HttpPost("{id}/reactivate")]
        public async Task<IActionResult> ReactivateAssignment(int id)
        {
            try
            {
                var result = await _assignmentService.ReactivateAssignmentAsync(id);

                if (!result)
                    return BadRequest(new { success = false, message = "Cannot reactivate assignment. It may be expired or not found." });

                return Ok(new { success = true, message = "Assignment reactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating assignment {AssignmentId}", id);
                return StatusCode(500, new { success = false, message = "Error reactivating assignment" });
            }
        }

        /// <summary>
        /// Validate if a submission can be made
        /// </summary>
        [HttpGet("{id}/validate-submission")]
        public async Task<IActionResult> ValidateSubmission(int id, [FromQuery] DateTime? submissionDate)
        {
            try
            {
                var date = submissionDate ?? DateTime.UtcNow;
                var result = await _assignmentService.ValidateSubmissionAsync(id, date);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating submission for assignment {AssignmentId}", id);
                return StatusCode(500, new { success = false, message = "Error validating submission" });
            }
        }

        #endregion

        #region Target Preview

        /// <summary>
        /// Preview targets for an assignment (before creating)
        /// </summary>
        [HttpPost("preview-targets")]
        public async Task<IActionResult> PreviewTargets([FromBody] AssignmentCreateDto dto)
        {
            try
            {
                var preview = await _assignmentService.GetTargetPreviewAsync(dto);
                return Ok(new { success = true, data = preview });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing targets");
                return StatusCode(500, new { success = false, message = "Error previewing targets" });
            }
        }

        /// <summary>
        /// Get target count for an assignment type
        /// </summary>
        [HttpGet("target-count")]
        public async Task<IActionResult> GetTargetCount(
            [FromQuery] string assignmentType,
            [FromQuery] int? targetId,
            [FromQuery] string? tenantType)
        {
            try
            {
                var count = await _assignmentService.GetTargetCountAsync(assignmentType, targetId, tenantType);
                return Ok(new { success = true, count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting target count");
                return StatusCode(500, new { success = false, message = "Error getting target count" });
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Get assignment statistics for dashboard
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] int? templateId)
        {
            try
            {
                var stats = await _assignmentService.GetAssignmentStatisticsAsync(templateId);
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignment statistics");
                return StatusCode(500, new { success = false, message = "Error getting statistics" });
            }
        }

        /// <summary>
        /// Get compliance metrics
        /// </summary>
        [HttpGet("compliance")]
        public async Task<IActionResult> GetComplianceMetrics([FromQuery] AssignmentFilterDto filter)
        {
            try
            {
                var metrics = await _assignmentService.GetComplianceMetricsAsync(filter);
                return Ok(new { success = true, data = metrics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compliance metrics");
                return StatusCode(500, new { success = false, message = "Error getting compliance metrics" });
            }
        }

        #endregion

        #region User Access

        /// <summary>
        /// Check if current user has access to a template
        /// </summary>
        [HttpGet("check-access/{templateId}")]
        public async Task<IActionResult> CheckAccess(int templateId)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var hasAccess = await _assignmentService.CheckUserAccessAsync(userId, templateId);
                return Ok(new { success = true, hasAccess });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking access for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error checking access" });
            }
        }

        /// <summary>
        /// Get assignments for current user
        /// </summary>
        [HttpGet("my-assignments")]
        public async Task<IActionResult> GetMyAssignments([FromQuery] int? templateId)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var assignments = await _assignmentService.GetUserAssignmentsAsync(userId, templateId);
                return Ok(new { success = true, data = assignments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user assignments");
                return StatusCode(500, new { success = false, message = "Error getting assignments" });
            }
        }

        /// <summary>
        /// Get pending assignments for current user
        /// </summary>
        [HttpGet("my-pending")]
        public async Task<IActionResult> GetMyPendingAssignments()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var assignments = await _assignmentService.GetUserPendingAssignmentsAsync(userId);
                return Ok(new { success = true, data = assignments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending assignments");
                return StatusCode(500, new { success = false, message = "Error getting pending assignments" });
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Send reminders for selected assignments
        /// </summary>
        [HttpPost("bulk/remind")]
        public async Task<IActionResult> BulkRemind([FromBody] BulkAssignmentRequest request)
        {
            try
            {
                var count = await _assignmentService.SendRemindersAsync(request.AssignmentIds);
                return Ok(new { success = true, message = $"Sent reminders for {count} assignments" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk reminders");
                return StatusCode(500, new { success = false, message = "Error sending reminders" });
            }
        }

        /// <summary>
        /// Extend effective periods for selected assignments
        /// </summary>
        [HttpPost("bulk/extend")]
        public async Task<IActionResult> BulkExtend([FromBody] BulkExtendRequest request)
        {
            try
            {
                var count = await _assignmentService.BulkExtendEffectivePeriodsAsync(request.AssignmentIds, request.NewEffectiveUntil);
                return Ok(new { success = true, message = $"Extended effective periods for {count} assignments" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending bulk effective periods");
                return StatusCode(500, new { success = false, message = "Error extending effective periods" });
            }
        }

        /// <summary>
        /// Cancel selected assignments
        /// </summary>
        [HttpPost("bulk/cancel")]
        public async Task<IActionResult> BulkCancel([FromBody] BulkCancelRequest request)
        {
            try
            {
                var count = await _assignmentService.BulkCancelAsync(request.AssignmentIds, request.Reason);
                return Ok(new { success = true, message = $"Cancelled {count} assignments" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling bulk assignments");
                return StatusCode(500, new { success = false, message = "Error cancelling assignments" });
            }
        }

        #endregion

        #region Target Options (Scope-Filtered)

        /// <summary>
        /// Get target options for assignment type (scope-filtered)
        /// </summary>
        [HttpGet("target-options/{assignmentType}")]
        public async Task<IActionResult> GetTargetOptions(string assignmentType)
        {
            try
            {
                var options = await _assignmentService.GetTargetOptionsAsync(User, assignmentType);
                return Ok(new { success = true, data = options });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting target options for type {AssignmentType}", assignmentType);
                return StatusCode(500, new { success = false, message = "Error getting target options" });
            }
        }

        #endregion

        #region Template-Specific

        /// <summary>
        /// Get assignments for a specific template
        /// </summary>
        [HttpGet("template/{templateId}")]
        public async Task<IActionResult> GetTemplateAssignments(int templateId, [FromQuery] AssignmentFilterDto filter)
        {
            try
            {
                filter.TemplateId = templateId;
                var (items, totalCount) = await _assignmentService.GetAssignmentsAsync(filter);

                return Ok(new
                {
                    success = true,
                    data = items,
                    pagination = new
                    {
                        page = filter.Page,
                        pageSize = filter.PageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignments for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error retrieving assignments" });
            }
        }

        #endregion
    }

    #region Request Models

    public class CancelAssignmentRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class ExtendEffectivePeriodRequest
    {
        public DateTime NewEffectiveUntil { get; set; }
    }

    public class SuspendAssignmentRequest
    {
        public string? Reason { get; set; }
    }

    public class BulkAssignmentRequest
    {
        public List<int> AssignmentIds { get; set; } = new();
    }

    public class BulkExtendRequest
    {
        public List<int> AssignmentIds { get; set; } = new();
        public DateTime NewEffectiveUntil { get; set; }
    }

    public class BulkCancelRequest
    {
        public List<int> AssignmentIds { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
    }

    #endregion
}

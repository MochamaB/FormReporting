using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FormReporting.Services.Forms;
using FormReporting.Services.Identity;
using FormReporting.Models.ViewModels.Forms;

namespace FormReporting.Controllers.API
{
    /// <summary>
    /// API Controller for Template Validation
    /// Provides endpoints for dual-mode template readiness validation
    /// </summary>
    [ApiController]
    [Route("api/templates")]
    [Authorize]
    public class TemplateValidationApiController : ControllerBase
    {
        private readonly IFormTemplateService _templateService;
        private readonly IFormAssignmentService _assignmentService;
        private readonly IFormSubmissionService _submissionService;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<TemplateValidationApiController> _logger;

        public TemplateValidationApiController(
            IFormTemplateService templateService,
            IFormAssignmentService assignmentService,
            IFormSubmissionService submissionService,
            IClaimsService claimsService,
            ILogger<TemplateValidationApiController> logger)
        {
            _templateService = templateService;
            _assignmentService = assignmentService;
            _submissionService = submissionService;
            _claimsService = claimsService;
            _logger = logger;
        }

        // ===== Template Readiness Validation =====

        /// <summary>
        /// GET: /api/templates/{templateId}/validate-readiness
        /// Validate if template is ready to accept submissions based on submission mode
        /// </summary>
        [HttpGet("{templateId}/validate-readiness")]
        public async Task<IActionResult> ValidateTemplateReadiness(int templateId)
        {
            try
            {
                var result = await _templateService.ValidateTemplateReadinessAsync(templateId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating template readiness for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error validating template readiness", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/templates/{templateId}/can-accept-submissions
        /// Check if template can accept new submissions
        /// </summary>
        [HttpGet("{templateId}/can-accept-submissions")]
        public async Task<IActionResult> CanAcceptSubmissions(int templateId)
        {
            try
            {
                var canAccept = await _templateService.CanAcceptSubmissionsAsync(templateId);
                return Ok(new { success = true, data = new { canAcceptSubmissions = canAccept } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking submission acceptance for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error checking submission acceptance", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/templates/{templateId}/ready-for-collaborative
        /// Check if template is ready for collaborative workflow
        /// </summary>
        [HttpGet("{templateId}/ready-for-collaborative")]
        public async Task<IActionResult> IsReadyForCollaborativeWorkflow(int templateId)
        {
            try
            {
                var isReady = await _templateService.IsReadyForCollaborativeWorkflowAsync(templateId);
                return Ok(new { success = true, data = new { readyForCollaborative = isReady } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking collaborative readiness for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error checking collaborative readiness", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/templates/{templateId}/configuration-status
        /// Get detailed configuration status for template
        /// </summary>
        [HttpGet("{templateId}/configuration-status")]
        public async Task<IActionResult> GetTemplateConfigurationStatus(int templateId)
        {
            try
            {
                var status = await _templateService.GetTemplateConfigurationStatusAsync(templateId);
                return Ok(new { success = true, data = status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configuration status for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error getting configuration status", error = ex.Message });
            }
        }

        // ===== Assignment Coverage Validation =====

        /// <summary>
        /// GET: /api/templates/{templateId}/validate-assignment-coverage
        /// Validate assignment coverage for template based on submission mode
        /// </summary>
        [HttpGet("{templateId}/validate-assignment-coverage")]
        public async Task<IActionResult> ValidateAssignmentCoverage(int templateId)
        {
            try
            {
                var result = await _assignmentService.ValidateAssignmentCoverageAsync(templateId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating assignment coverage for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error validating assignment coverage", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/templates/{templateId}/has-sufficient-assignments
        /// Check if template has sufficient active assignments for its submission mode
        /// </summary>
        [HttpGet("{templateId}/has-sufficient-assignments")]
        public async Task<IActionResult> HasSufficientAssignments(int templateId)
        {
            try
            {
                var hasSufficient = await _assignmentService.HasSufficientAssignmentsAsync(templateId);
                return Ok(new { success = true, data = new { hasSufficientAssignments = hasSufficient } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking assignment sufficiency for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error checking assignment sufficiency", error = ex.Message });
            }
        }

        // ===== Submission Access Validation =====

        /// <summary>
        /// GET: /api/templates/{templateId}/can-user-create-submission
        /// Check if current user can create a submission for template (Individual mode)
        /// </summary>
        [HttpGet("{templateId}/can-user-create-submission")]
        public async Task<IActionResult> CanUserCreateSubmission(int templateId)
        {
            try
            {
                var userId = _claimsService.GetUserId();
                var canCreate = await _submissionService.CanUserCreateSubmissionAsync(userId, templateId);
                return Ok(new { success = true, data = new { canCreateSubmission = canCreate } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user submission access for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error checking user submission access", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/templates/{templateId}/can-create-collaborative-submission
        /// Check if system/admin can create collaborative submission for template
        /// </summary>
        [HttpGet("{templateId}/can-create-collaborative-submission")]
        public async Task<IActionResult> CanCreateCollaborativeSubmission(int templateId)
        {
            try
            {
                var canCreate = await _submissionService.CanCreateCollaborativeSubmissionAsync(templateId);
                return Ok(new { success = true, data = new { canCreateCollaborativeSubmission = canCreate } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking collaborative submission capability for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error checking collaborative submission capability", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/templates/{templateId}/validate-submission-access
        /// Get detailed submission access validation for current user
        /// </summary>
        [HttpGet("{templateId}/validate-submission-access")]
        public async Task<IActionResult> ValidateSubmissionAccess(int templateId)
        {
            try
            {
                var userId = _claimsService.GetUserId();
                var result = await _submissionService.ValidateSubmissionAccessAsync(userId, templateId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating submission access for user {UserId} and template {TemplateId}", _claimsService.GetUserId(), templateId);
                return StatusCode(500, new { success = false, message = "Error validating submission access", error = ex.Message });
            }
        }

        // ===== Dashboard/Listing Endpoints =====

        /// <summary>
        /// GET: /api/templates/with-submission-access
        /// Get templates current user can create submissions for (Individual mode)
        /// </summary>
        [HttpGet("with-submission-access")]
        public async Task<IActionResult> GetTemplatesWithSubmissionAccess()
        {
            try
            {
                var userId = _claimsService.GetUserId();
                var templates = await _submissionService.GetTemplatesWithSubmissionAccessAsync(userId);
                return Ok(new { success = true, data = templates });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting templates with submission access for user {UserId}", _claimsService.GetUserId());
                return StatusCode(500, new { success = false, message = "Error getting templates with submission access", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/templates/ready-for-collaborative-workflow
        /// Get templates ready for collaborative workflow
        /// </summary>
        [HttpGet("ready-for-collaborative-workflow")]
        public async Task<IActionResult> GetTemplatesReadyForCollaborativeWorkflow()
        {
            try
            {
                var templates = await _submissionService.GetTemplatesReadyForCollaborativeWorkflowAsync();
                return Ok(new { success = true, data = templates });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting templates ready for collaborative workflow");
                return StatusCode(500, new { success = false, message = "Error getting templates ready for collaborative workflow", error = ex.Message });
            }
        }
    }
}

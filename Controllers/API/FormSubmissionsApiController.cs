using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FormReporting.Services.Forms;
using FormReporting.Extensions;
using System.Security.Claims;

namespace FormReporting.Controllers.API
{
    /// <summary>
    /// API Controller for Form Submission operations
    /// Handles AJAX requests for auto-save, file uploads, and validation
    /// </summary>
    [ApiController]
    [Route("api/submissions")]
    [Authorize]
    public class FormSubmissionsApiController : Controller
    {
        private readonly IFormSubmissionService _submissionService;
        private readonly IFormResponseService _responseService;
        private readonly IWebHostEnvironment _environment;

        public FormSubmissionsApiController(
            IFormSubmissionService submissionService,
            IFormResponseService responseService,
            IWebHostEnvironment environment)
        {
            _submissionService = submissionService;
            _responseService = responseService;
            _environment = environment;
        }

        /// <summary>
        /// Get current user ID from claims
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("UserId")?.Value;

            if (int.TryParse(userIdClaim, out var userId))
                return userId;

            throw new UnauthorizedAccessException("User not authenticated");
        }

        // ========================================================================
        // AUTO-SAVE ENDPOINT
        // ========================================================================

        /// <summary>
        /// Auto-save draft submission (called every 30 seconds from client)
        /// POST /api/submissions/auto-save
        /// </summary>
        [HttpPost("auto-save")]
        public async Task<IActionResult> AutoSave([FromBody] AutoSaveRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Parse responses from string dictionary to typed dictionary
                var responses = new Dictionary<int, string?>();
                if (request.Responses != null)
                {
                    foreach (var kvp in request.Responses)
                    {
                        if (int.TryParse(kvp.Key, out var itemId))
                        {
                            responses[itemId] = kvp.Value;
                        }
                    }
                }

                // Parse reporting period
                var reportingPeriod = DateTime.TryParse(request.ReportingPeriod, out var rp)
                    ? rp
                    : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                var result = await _responseService.SaveDraftAsync(
                    request.SubmissionId,
                    request.TemplateId,
                    userId,
                    request.TenantId,
                    reportingPeriod,
                    responses,
                    request.CurrentSection);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        submissionId = result.SubmissionId,
                        savedAt = result.SavedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        message = "Draft saved successfully"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        errors = result.Errors
                    });
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error saving draft: {ex.Message}" });
            }
        }

        // ========================================================================
        // RESPONSES ENDPOINT
        // ========================================================================

        /// <summary>
        /// Get current responses for a submission (for resume functionality)
        /// GET /api/submissions/{id}/responses
        /// </summary>
        [HttpGet("{id}/responses")]
        public async Task<IActionResult> GetResponses(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Verify user owns this submission
                var submission = await _submissionService.GetSubmissionAsync(id);
                if (submission == null)
                {
                    return NotFound(new { success = false, message = "Submission not found" });
                }

                if (submission.SubmittedBy != userId)
                {
                    return Forbid();
                }

                var responses = await _responseService.GetResponsesAsync(id);

                return Ok(new
                {
                    success = true,
                    submissionId = id,
                    currentSection = submission.CurrentSection,
                    lastSavedDate = submission.LastSavedDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                    responses = responses.ToDictionary(
                        kvp => kvp.Key.ToString(),
                        kvp => kvp.Value?.ToString())
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving responses: {ex.Message}" });
            }
        }

        // ========================================================================
        // VALIDATION ENDPOINT
        // ========================================================================

        /// <summary>
        /// Validate all responses before final submission
        /// POST /api/submissions/{id}/validate
        /// </summary>
        [HttpPost("{id}/validate")]
        public async Task<IActionResult> ValidateSubmission(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Verify user owns this submission
                var submission = await _submissionService.GetSubmissionAsync(id);
                if (submission == null)
                {
                    return NotFound(new { success = false, message = "Submission not found" });
                }

                if (submission.SubmittedBy != userId)
                {
                    return Forbid();
                }

                var result = await _responseService.ValidateResponsesAsync(id);

                return Ok(new
                {
                    success = result.IsValid,
                    isValid = result.IsValid,
                    totalFields = result.TotalFields,
                    validFields = result.ValidFields,
                    invalidFields = result.InvalidFields,
                    errors = result.Errors
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error validating submission: {ex.Message}" });
            }
        }

        // ========================================================================
        // FILE UPLOAD ENDPOINTS
        // ========================================================================

        /// <summary>
        /// Upload file attachment for a submission
        /// POST /api/submissions/{id}/upload
        /// </summary>
        [HttpPost("{id}/upload")]
        public async Task<IActionResult> UploadFile(int id, [FromForm] FileUploadRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Verify user owns this submission
                var submission = await _submissionService.GetSubmissionAsync(id);
                if (submission == null)
                {
                    return NotFound(new { success = false, message = "Submission not found" });
                }

                if (submission.SubmittedBy != userId)
                {
                    return Forbid();
                }

                if (submission.Status != "Draft")
                {
                    return BadRequest(new { success = false, message = "Cannot upload files to a submitted form" });
                }

                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No file provided" });
                }

                // Validate file size (default 10MB max)
                var maxFileSize = request.MaxFileSize > 0 ? request.MaxFileSize : 10 * 1024 * 1024;
                if (request.File.Length > maxFileSize)
                {
                    return BadRequest(new { success = false, message = $"File size exceeds maximum allowed ({FormatFileSize(maxFileSize)})" });
                }

                // Validate file extension if allowed types specified
                if (!string.IsNullOrEmpty(request.AllowedTypes))
                {
                    var allowedExtensions = request.AllowedTypes.Split(',')
                        .Select(e => e.Trim().ToLowerInvariant())
                        .ToList();
                    var fileExtension = Path.GetExtension(request.File.FileName)?.ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { success = false, message = $"File type not allowed. Allowed types: {request.AllowedTypes}" });
                    }
                }

                // Create upload directory
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "submissions", id.ToString());
                Directory.CreateDirectory(uploadPath);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                // Store file reference in response (as relative path)
                var relativePath = $"/uploads/submissions/{id}/{uniqueFileName}";

                // Save the file path as the response value for this field
                if (request.ItemId > 0)
                {
                    var responses = new Dictionary<int, string?> { { request.ItemId, relativePath } };
                    await _responseService.SaveResponsesAsync(id, responses);
                }

                return Ok(new
                {
                    success = true,
                    message = "File uploaded successfully",
                    fileId = uniqueFileName,
                    fileName = request.File.FileName,
                    filePath = relativePath,
                    fileSize = request.File.Length,
                    fileSizeDisplay = FormatFileSize(request.File.Length)
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error uploading file: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete file attachment from a submission
        /// DELETE /api/submissions/{id}/files/{fileId}
        /// </summary>
        [HttpDelete("{id}/files/{fileId}")]
        public async Task<IActionResult> DeleteFile(int id, string fileId, [FromQuery] int itemId = 0)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Verify user owns this submission
                var submission = await _submissionService.GetSubmissionAsync(id);
                if (submission == null)
                {
                    return NotFound(new { success = false, message = "Submission not found" });
                }

                if (submission.SubmittedBy != userId)
                {
                    return Forbid();
                }

                if (submission.Status != "Draft")
                {
                    return BadRequest(new { success = false, message = "Cannot delete files from a submitted form" });
                }

                // Build file path
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "submissions", id.ToString(), fileId);

                // Delete file if exists
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Clear the response value for this field
                if (itemId > 0)
                {
                    var responses = new Dictionary<int, string?> { { itemId, null } };
                    await _responseService.SaveResponsesAsync(id, responses);
                }

                return Ok(new { success = true, message = "File deleted successfully" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting file: {ex.Message}" });
            }
        }

        // ========================================================================
        // SUBMISSION MANAGEMENT ENDPOINTS
        // ========================================================================

        /// <summary>
        /// Submit a draft form (final submission)
        /// POST /api/submissions/{id}/submit
        /// </summary>
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> Submit(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var result = await _responseService.SubmitAsync(id, userId);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        submissionId = result.SubmissionId,
                        status = result.Status,
                        message = result.Message
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        validationErrors = result.ValidationErrors
                    });
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error submitting form: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete a draft submission
        /// DELETE /api/submissions/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDraft(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var success = await _responseService.DeleteDraftAsync(id, userId);

                if (success)
                {
                    return Ok(new { success = true, message = "Draft deleted successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Cannot delete submission. It may not exist, you may not own it, or it has already been submitted." });
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting draft: {ex.Message}" });
            }
        }

        // ========================================================================
        // DRAFT CHECK ENDPOINT
        // ========================================================================

        /// <summary>
        /// Check if a draft exists for a template/tenant/period combination
        /// GET /api/submissions/check-draft
        /// </summary>
        [HttpGet("check-draft")]
        public async Task<IActionResult> CheckDraft([FromQuery] int templateId, [FromQuery] int? tenantId, [FromQuery] string period)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Parse period
                if (!DateTime.TryParse(period, out var reportingPeriod))
                {
                    reportingPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }

                var existingDraft = await _submissionService.GetExistingDraftAsync(
                    userId, templateId, tenantId, reportingPeriod);

                return Ok(new
                {
                    success = true,
                    hasDraft = existingDraft != null,
                    draftId = existingDraft?.SubmissionId,
                    lastSaved = existingDraft?.LastSavedDate?.ToString("MMM d, yyyy h:mm tt"),
                    currentSection = existingDraft?.CurrentSection ?? 0
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error checking draft: {ex.Message}" });
            }
        }

        // ========================================================================
        // AVAILABLE TEMPLATES ENDPOINT
        // ========================================================================

        /// <summary>
        /// Get available templates for the current user
        /// GET /api/submissions/templates
        /// </summary>
        [HttpGet("templates")]
        public async Task<IActionResult> GetAvailableTemplates()
        {
            try
            {
                var userId = GetCurrentUserId();

                var templates = await _submissionService.GetAvailableTemplatesAsync(userId);

                return Ok(new
                {
                    success = true,
                    templates = templates.Select(t => new
                    {
                        t.TemplateId,
                        t.TemplateName,
                        t.TemplateCode,
                        t.Description,
                        t.TemplateType,
                        CategoryName = t.Category?.CategoryName,
                        t.Version,
                        t.RequiresApproval
                    })
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving templates: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get user's submissions list
        /// GET /api/submissions/my
        /// </summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMySubmissions([FromQuery] string? status = null)
        {
            try
            {
                var userId = GetCurrentUserId();

                var submissions = await _submissionService.GetUserSubmissionsAsync(userId, status);

                return Ok(new
                {
                    success = true,
                    submissions = submissions.Select(s => new
                    {
                        s.SubmissionId,
                        s.TemplateId,
                        TemplateName = s.Template?.TemplateName,
                        s.Status,
                        s.ReportingPeriod,
                        s.SubmittedDate,
                        s.LastSavedDate,
                        s.CurrentSection,
                        TenantName = s.Tenant?.TenantName
                    })
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving submissions: {ex.Message}" });
            }
        }

        // ========================================================================
        // HELPER METHODS
        // ========================================================================

        /// <summary>
        /// Format file size in bytes to human-readable string
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }

    // ========================================================================
    // REQUEST MODELS
    // ========================================================================

    /// <summary>
    /// Request model for auto-save endpoint
    /// </summary>
    public class AutoSaveRequest
    {
        /// <summary>
        /// Submission ID (0 for new submission)
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// Template ID (required for new submissions)
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// Optional tenant ID
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Reporting period (yyyy-MM-dd format)
        /// </summary>
        public string? ReportingPeriod { get; set; }

        /// <summary>
        /// Dictionary of field responses (ItemId as string key → value)
        /// </summary>
        public Dictionary<string, string?>? Responses { get; set; }

        /// <summary>
        /// Current wizard section index (0-based)
        /// </summary>
        public int CurrentSection { get; set; }
    }

    /// <summary>
    /// Request model for file upload endpoint
    /// </summary>
    public class FileUploadRequest
    {
        /// <summary>
        /// The file to upload
        /// </summary>
        public IFormFile? File { get; set; }

        /// <summary>
        /// Form item ID this file belongs to
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Allowed file types (comma-separated extensions, e.g., ".pdf,.doc,.docx")
        /// </summary>
        public string? AllowedTypes { get; set; }

        /// <summary>
        /// Maximum file size in bytes
        /// </summary>
        public long MaxFileSize { get; set; }
    }
}

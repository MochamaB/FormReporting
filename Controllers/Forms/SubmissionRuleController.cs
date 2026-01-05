using FormReporting.Models.ViewModels.Forms;
using FormReporting.Services.Forms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormReporting.Controllers.Forms
{
    /// <summary>
    /// Controller for managing form template submission rules
    /// </summary>
    [Route("SubmissionRules")]
    [Authorize]
    public class SubmissionRuleController : Controller
    {
        private readonly ISubmissionRuleService _submissionRuleService;
        private readonly IFormTemplateService _formTemplateService;
        private readonly ILogger<SubmissionRuleController> _logger;

        public SubmissionRuleController(
            ISubmissionRuleService submissionRuleService,
            IFormTemplateService formTemplateService,
            ILogger<SubmissionRuleController> logger)
        {
            _submissionRuleService = submissionRuleService;
            _formTemplateService = formTemplateService;
            _logger = logger;
        }

        // ===== MVC ACTIONS =====

        /// <summary>
        /// Show create submission rule form
        /// </summary>
        [HttpGet("Create/{templateId:int}")]
        public async Task<IActionResult> Create(int templateId)
        {
            try
            {
                // Verify template exists and user has access
                var template = await _formTemplateService.LoadTemplateForEditingAsync(templateId);
                if (template == null)
                {
                    TempData["ErrorMessage"] = "Template not found.";
                    return RedirectToAction("Index", "FormTemplates");
                }

                // Check if user can add rules to this template
                if (!await _submissionRuleService.CanAddRuleToTemplateAsync(templateId))
                {
                    TempData["ErrorMessage"] = "Cannot add submission rules to this template.";
                    return RedirectToAction("Details", "FormTemplates", new { id = templateId });
                }

                ViewData["TemplateId"] = templateId;
                ViewData["TemplateName"] = template.TemplateName;
                ViewData["Title"] = "Create Submission Rule";

                var model = new SubmissionRuleCreateDto
                {
                    TemplateId = templateId,
                    GracePeriodDays = 0,
                    AllowLateSubmission = true
                };

                return View("~/Views/Forms/SubmissionRules/Create.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create submission rule form for template {TemplateId}", templateId);
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction("Details", "FormTemplates", new { id = templateId });
            }
        }

        /// <summary>
        /// Create new submission rule
        /// </summary>
        [HttpPost("Store")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(SubmissionRuleCreateDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewData["TemplateId"] = model.TemplateId;
                    ViewData["Title"] = "Create Submission Rule";
                    
                    // Get template name for display
                    var template = await _formTemplateService.LoadTemplateForEditingAsync(model.TemplateId);
                    ViewData["TemplateName"] = template?.TemplateName ?? "Unknown Template";
                    
                    return View("~/Views/Forms/SubmissionRules/Create.cshtml", model);
                }

                var result = await _submissionRuleService.CreateAsync(model);
                
                TempData["SuccessMessage"] = $"Submission rule '{result.RuleName}' created successfully.";
                return RedirectToAction("Details", "FormTemplates", new { id = model.TemplateId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating submission rule for template {TemplateId}", model.TemplateId);
                ModelState.AddModelError("", "An error occurred while creating the submission rule.");
                
                ViewData["TemplateId"] = model.TemplateId;
                ViewData["Title"] = "Create Submission Rule";
                
                // Get template name for display
                try
                {
                    var template = await _formTemplateService.LoadTemplateForEditingAsync(model.TemplateId);
                    ViewData["TemplateName"] = template?.TemplateName ?? "Unknown Template";
                }
                catch
                {
                    ViewData["TemplateName"] = "Unknown Template";
                }
                
                return View("~/Views/Forms/SubmissionRules/Create.cshtml", model);
            }
        }

        /// <summary>
        /// Show edit submission rule form
        /// </summary>
        [HttpGet("{id:int}/Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var rule = await _submissionRuleService.GetByIdAsync(id);
                if (rule == null)
                {
                    TempData["ErrorMessage"] = "Submission rule not found.";
                    return RedirectToAction("Index", "FormTemplates");
                }

                ViewData["SubmissionRuleId"] = id;
                ViewData["TemplateId"] = rule.TemplateId;
                ViewData["TemplateName"] = rule.TemplateName;
                ViewData["Title"] = "Edit Submission Rule";

                var model = new SubmissionRuleUpdateDto
                {
                    SubmissionRuleId = rule.SubmissionRuleId,
                    RuleName = rule.RuleName,
                    Description = rule.Description,
                    Frequency = rule.Frequency,
                    DueDay = rule.DueDay,
                    DueMonth = rule.DueMonth,
                    DueTime = rule.DueTime,
                    SpecificDueDate = rule.SpecificDueDate,
                    GracePeriodDays = rule.GracePeriodDays,
                    AllowLateSubmission = rule.AllowLateSubmission,
                    ReminderDaysBefore = rule.ReminderDaysBefore,
                    Status = rule.Status
                };

                return View("~/Views/Forms/SubmissionRules/Edit.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit submission rule form for rule {RuleId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction("Index", "FormTemplates");
            }
        }

        /// <summary>
        /// Update existing submission rule
        /// </summary>
        [HttpPost("Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(SubmissionRuleUpdateDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewData["SubmissionRuleId"] = model.SubmissionRuleId;
                    ViewData["Title"] = "Edit Submission Rule";
                    
                    // Get rule details for display
                    var existingRule = await _submissionRuleService.GetByIdAsync(model.SubmissionRuleId);
                    if (existingRule != null)
                    {
                        ViewData["TemplateId"] = existingRule.TemplateId;
                        ViewData["TemplateName"] = existingRule.TemplateName;
                    }
                    
                    return View("~/Views/Forms/SubmissionRules/Edit.cshtml", model);
                }

                var result = await _submissionRuleService.UpdateAsync(model);
                
                TempData["SuccessMessage"] = $"Submission rule '{result.RuleName}' updated successfully.";
                return RedirectToAction("Details", "FormTemplates", new { id = result.TemplateId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating submission rule {RuleId}", model.SubmissionRuleId);
                ModelState.AddModelError("", "An error occurred while updating the submission rule.");
                
                ViewData["SubmissionRuleId"] = model.SubmissionRuleId;
                ViewData["Title"] = "Edit Submission Rule";
                
                // Get rule details for display
                try
                {
                    var existingRule = await _submissionRuleService.GetByIdAsync(model.SubmissionRuleId);
                    if (existingRule != null)
                    {
                        ViewData["TemplateId"] = existingRule.TemplateId;
                        ViewData["TemplateName"] = existingRule.TemplateName;
                    }
                }
                catch
                {
                    // Ignore error getting template details for display
                }
                
                return View("~/Views/Forms/SubmissionRules/Edit.cshtml", model);
            }
        }

        // ===== API ENDPOINTS =====

        /// <summary>
        /// Get submission rules for a template (API)
        /// </summary>
        [HttpGet("/api/submissionrules/template/{templateId:int}")]
        public async Task<IActionResult> GetByTemplateId(int templateId)
        {
            try
            {
                var rules = await _submissionRuleService.GetByTemplateIdAsync(templateId);
                return Ok(new { success = true, data = rules });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submission rules for template {TemplateId}", templateId);
                return StatusCode(500, new { success = false, message = "Error retrieving submission rules" });
            }
        }

        /// <summary>
        /// Get submission rule details (API)
        /// </summary>
        [HttpGet("/api/submissionrules/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var rule = await _submissionRuleService.GetByIdAsync(id);
                if (rule == null)
                {
                    return NotFound(new { success = false, message = "Submission rule not found" });
                }

                return Ok(new { success = true, data = rule });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submission rule {RuleId}", id);
                return StatusCode(500, new { success = false, message = "Error retrieving submission rule" });
            }
        }

        /// <summary>
        /// Delete submission rule (API)
        /// </summary>
        [HttpDelete("/api/submissionrules/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get rule details before deletion for logging
                var rule = await _submissionRuleService.GetByIdAsync(id);
                if (rule == null)
                {
                    return NotFound(new { success = false, message = "Submission rule not found" });
                }

                var success = await _submissionRuleService.DeleteAsync(id);
                if (success)
                {
                    _logger.LogInformation("Deleted submission rule {RuleId} ({RuleName}) for template {TemplateId}", 
                        id, rule.RuleName, rule.TemplateId);
                    return Ok(new { success = true, message = "Submission rule deleted successfully" });
                }

                return BadRequest(new { success = false, message = "Failed to delete submission rule" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting submission rule {RuleId}", id);
                return StatusCode(500, new { success = false, message = "Error deleting submission rule" });
            }
        }

        /// <summary>
        /// Validate submission timing (API)
        /// </summary>
        [HttpPost("/api/submissionrules/validate-timing")]
        public async Task<IActionResult> ValidateSubmissionTiming([FromBody] SubmissionTimingValidationRequest request)
        {
            try
            {
                if (request.TemplateId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid template ID" });
                }

                var submissionDate = request.SubmissionDate ?? DateTime.UtcNow;
                var result = await _submissionRuleService.ValidateSubmissionTimingAsync(request.TemplateId, submissionDate);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating submission timing for template {TemplateId}", request.TemplateId);
                return StatusCode(500, new { success = false, message = "Error validating submission timing" });
            }
        }

        /// <summary>
        /// Calculate next due date for a rule configuration (API)
        /// </summary>
        [HttpPost("/api/submissionrules/calculate-due-date")]
        public IActionResult CalculateDueDate([FromBody] DueDateCalculationRequest request)
        {
            try
            {
                // Create a temporary rule object for calculation
                var tempRule = new FormReporting.Models.Entities.Forms.FormTemplateSubmissionRule
                {
                    Frequency = request.Frequency,
                    DueDay = request.DueDay,
                    DueMonth = request.DueMonth,
                    DueTime = request.DueTime,
                    SpecificDueDate = request.SpecificDueDate
                };

                var nextDueDate = _submissionRuleService.CalculateNextDueDate(tempRule, request.FromDate);
                var displayText = _submissionRuleService.GetFrequencyDisplayText(request.Frequency, request.DueDay, request.DueMonth);

                return Ok(new 
                { 
                    success = true, 
                    data = new 
                    {
                        nextDueDate = nextDueDate,
                        nextDueDateDisplay = nextDueDate?.ToString("yyyy-MM-dd HH:mm"),
                        frequencyDisplay = displayText
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating due date");
                return StatusCode(500, new { success = false, message = "Error calculating due date" });
            }
        }
    }

    /// <summary>
    /// Request model for submission timing validation
    /// </summary>
    public class SubmissionTimingValidationRequest
    {
        public int TemplateId { get; set; }
        public DateTime? SubmissionDate { get; set; }
    }

    /// <summary>
    /// Request model for due date calculation
    /// </summary>
    public class DueDateCalculationRequest
    {
        public string? Frequency { get; set; }
        public int? DueDay { get; set; }
        public int? DueMonth { get; set; }
        public TimeSpan? DueTime { get; set; }
        public DateTime? SpecificDueDate { get; set; }
        public DateTime? FromDate { get; set; }
    }
}

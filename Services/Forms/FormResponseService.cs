using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.Common;
using System.Text.RegularExpressions;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service implementation for handling form responses
    /// Manages saving, validating, and submitting form data
    /// </summary>
    public class FormResponseService : IFormResponseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFormSubmissionService _submissionService;

        public FormResponseService(ApplicationDbContext context, IFormSubmissionService submissionService)
        {
            _context = context;
            _submissionService = submissionService;
        }

        /// <inheritdoc />
        public async Task<int> SaveResponsesAsync(int submissionId, Dictionary<int, string?> responses)
        {
            var submission = await _context.FormTemplateSubmissions
                .Include(s => s.Responses)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
            {
                throw new InvalidOperationException($"Submission with ID {submissionId} not found.");
            }

            // Load item definitions for data type mapping
            var itemIds = responses.Keys.ToList();
            var items = await _context.FormTemplateItems
                .Where(i => itemIds.Contains(i.ItemId))
                .ToDictionaryAsync(i => i.ItemId);

            int savedCount = 0;

            foreach (var (itemId, value) in responses)
            {
                if (!items.TryGetValue(itemId, out var item))
                    continue;

                // Find existing response or create new
                var existingResponse = submission.Responses.FirstOrDefault(r => r.ItemId == itemId);

                if (existingResponse != null)
                {
                    // Update existing response
                    SetResponseValue(existingResponse, item.DataType, value);
                    existingResponse.ModifiedDate = DateTime.Now;
                }
                else
                {
                    // Create new response
                    var newResponse = new FormTemplateResponse
                    {
                        SubmissionId = submissionId,
                        ItemId = itemId,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    SetResponseValue(newResponse, item.DataType, value);
                    _context.FormTemplateResponses.Add(newResponse);
                }

                savedCount++;
            }

            // Update submission modified date
            submission.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return savedCount;
        }

        /// <inheritdoc />
        public async Task<AutoSaveResult> SaveDraftAsync(
            int submissionId,
            int templateId,
            int userId,
            int? tenantId,
            DateTime reportingPeriod,
            Dictionary<int, string?> responses,
            int currentSection)
        {
            var result = new AutoSaveResult();

            try
            {
                FormTemplateSubmission submission;

                if (submissionId == 0)
                {
                    // Check for existing draft first
                    var existingDraft = await _submissionService.GetExistingDraftAsync(
                        userId, templateId, tenantId, reportingPeriod);

                    if (existingDraft != null)
                    {
                        submission = existingDraft;
                    }
                    else
                    {
                        // Create new submission
                        submission = await _submissionService.CreateSubmissionAsync(
                            templateId, userId, tenantId, reportingPeriod);
                    }
                }
                else
                {
                    submission = await _context.FormTemplateSubmissions
                        .FirstOrDefaultAsync(s => s.SubmissionId == submissionId)
                        ?? throw new InvalidOperationException($"Submission {submissionId} not found.");

                    // Verify ownership
                    if (submission.SubmittedBy != userId)
                    {
                        result.Errors.Add("You do not have permission to edit this submission.");
                        return result;
                    }

                    // Verify still a draft
                    if (submission.Status != "Draft")
                    {
                        result.Errors.Add("Cannot modify a submitted form.");
                        return result;
                    }
                }

                // Save responses
                if (responses.Any())
                {
                    await SaveResponsesAsync(submission.SubmissionId, responses);
                }

                // Update draft tracking fields
                submission.LastSavedDate = DateTime.Now;
                submission.CurrentSection = currentSection;
                submission.ModifiedDate = DateTime.Now;
                submission.ModifiedBy = userId;

                await _context.SaveChangesAsync();

                result.Success = true;
                result.SubmissionId = submission.SubmissionId;
                result.SavedAt = submission.LastSavedDate.Value;
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<SubmitResult> SubmitAsync(int submissionId, int userId)
        {
            var result = new SubmitResult { SubmissionId = submissionId };

            var submission = await _context.FormTemplateSubmissions
                .Include(s => s.Template)
                .Include(s => s.Responses)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
            {
                result.ValidationErrors["_form"] = new List<string> { "Submission not found." };
                return result;
            }

            // Verify ownership
            if (submission.SubmittedBy != userId)
            {
                result.ValidationErrors["_form"] = new List<string> { "You do not have permission to submit this form." };
                return result;
            }

            // Verify still a draft
            if (submission.Status != "Draft")
            {
                result.ValidationErrors["_form"] = new List<string> { "This form has already been submitted." };
                return result;
            }

            // Validate all responses
            var validationResult = await ValidateResponsesAsync(submissionId);

            if (!validationResult.IsValid)
            {
                result.ValidationErrors = validationResult.Errors;
                return result;
            }

            // Update submission status
            submission.Status = submission.Template.RequiresApproval ? "InApproval" : "Submitted";
            submission.SubmittedDate = DateTime.Now;
            submission.ModifiedDate = DateTime.Now;
            submission.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            result.Success = true;
            result.Status = submission.Status;
            result.Message = submission.Template.RequiresApproval
                ? "Form submitted successfully. Awaiting approval."
                : "Form submitted successfully.";

            return result;
        }

        /// <inheritdoc />
        public async Task<ValidationResult> ValidateResponsesAsync(int submissionId)
        {
            var result = new ValidationResult { IsValid = true };

            var submission = await _context.FormTemplateSubmissions
                .Include(s => s.Responses)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
            {
                result.IsValid = false;
                result.Errors["_form"] = new List<string> { "Submission not found." };
                return result;
            }

            // Load all items with validations and configurations for this template
            var items = await _context.FormTemplateItems
                .Include(i => i.Validations.Where(v => v.IsActive))
                .Include(i => i.Options)
                .Include(i => i.Configurations)
                .Where(i => i.TemplateId == submission.TemplateId && i.IsActive)
                .ToListAsync();

            var responsesByItemId = submission.Responses.ToDictionary(r => r.ItemId);

            result.TotalFields = items.Count;

            foreach (var item in items)
            {
                var fieldKey = $"field_{item.ItemId}";
                var fieldErrors = new List<string>();

                responsesByItemId.TryGetValue(item.ItemId, out var response);
                var value = GetResponseValueAsString(response, item.DataType);

                // Check required
                if (item.IsRequired && string.IsNullOrWhiteSpace(value))
                {
                    fieldErrors.Add($"{item.ItemName} is required.");
                }

                // Run validation rules from FormItemValidation
                if (item.Validations != null && !string.IsNullOrWhiteSpace(value))
                {
                    foreach (var validation in item.Validations.OrderBy(v => v.ValidationOrder))
                    {
                        var error = ValidateField(value, validation, item);
                        if (!string.IsNullOrEmpty(error))
                        {
                            fieldErrors.Add(error);
                        }
                    }
                }

                // Run configuration-based validations
                if (item.Configurations != null && item.Configurations.Any() && !string.IsNullOrWhiteSpace(value))
                {
                    var configErrors = ValidateFieldConfigurations(value, item);
                    fieldErrors.AddRange(configErrors);
                }

                if (fieldErrors.Any())
                {
                    result.Errors[fieldKey] = fieldErrors;
                    result.InvalidFields++;
                    result.IsValid = false;
                }
                else
                {
                    result.ValidFields++;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDraftAsync(int submissionId, int userId)
        {
            var submission = await _context.FormTemplateSubmissions
                .Include(s => s.Responses)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
                return false;

            // Verify ownership and draft status
            if (submission.SubmittedBy != userId || submission.Status != "Draft")
                return false;

            // Delete responses first (cascade should handle this, but being explicit)
            _context.FormTemplateResponses.RemoveRange(submission.Responses);

            // Delete submission
            _context.FormTemplateSubmissions.Remove(submission);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc />
        public async Task<Dictionary<int, object?>> GetResponsesAsync(int submissionId)
        {
            var submission = await _context.FormTemplateSubmissions
                .Include(s => s.Responses)
                    .ThenInclude(r => r.Item)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
                return new Dictionary<int, object?>();

            var result = new Dictionary<int, object?>();

            foreach (var response in submission.Responses)
            {
                var value = GetResponseValue(response, response.Item?.DataType);
                result[response.ItemId] = value;
            }

            return result;
        }

        /// <summary>
        /// Set the appropriate value column based on data type
        /// </summary>
        private void SetResponseValue(FormTemplateResponse response, string? dataType, string? value)
        {
            // Clear all values first
            response.TextValue = null;
            response.NumericValue = null;
            response.DateValue = null;
            response.BooleanValue = null;

            if (string.IsNullOrWhiteSpace(value))
                return;

            var fieldType = ParseDataType(dataType);

            switch (fieldType)
            {
                case FormFieldType.Number:
                case FormFieldType.Decimal:
                case FormFieldType.Currency:
                case FormFieldType.Percentage:
                case FormFieldType.Rating:
                case FormFieldType.Slider:
                    if (decimal.TryParse(value, out var numericValue))
                        response.NumericValue = numericValue;
                    else
                        response.TextValue = value; // Fallback
                    break;

                case FormFieldType.Date:
                case FormFieldType.Time:
                case FormFieldType.DateTime:
                    if (DateTime.TryParse(value, out var dateValue))
                        response.DateValue = dateValue;
                    else
                        response.TextValue = value; // Fallback
                    break;

                case FormFieldType.Checkbox:
                    if (bool.TryParse(value, out var boolValue))
                        response.BooleanValue = boolValue;
                    else if (value.Equals("on", StringComparison.OrdinalIgnoreCase) ||
                             value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                             value.Equals("1", StringComparison.OrdinalIgnoreCase))
                        response.BooleanValue = true;
                    else if (value.Equals("off", StringComparison.OrdinalIgnoreCase) ||
                             value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                             value.Equals("0", StringComparison.OrdinalIgnoreCase))
                        response.BooleanValue = false;
                    else
                        response.TextValue = value; // Multi-checkbox as comma-separated
                    break;

                default:
                    response.TextValue = value;
                    break;
            }
        }

        /// <summary>
        /// Get response value based on data type
        /// </summary>
        private object? GetResponseValue(FormTemplateResponse? response, string? dataType)
        {
            if (response == null)
                return null;

            var fieldType = ParseDataType(dataType);

            return fieldType switch
            {
                FormFieldType.Number or FormFieldType.Decimal or FormFieldType.Currency or
                FormFieldType.Percentage or FormFieldType.Rating or FormFieldType.Slider
                    => response.NumericValue,

                FormFieldType.Date or FormFieldType.Time or FormFieldType.DateTime
                    => response.DateValue,

                FormFieldType.Checkbox when response.BooleanValue.HasValue
                    => response.BooleanValue,

                _ => response.TextValue
            };
        }

        /// <summary>
        /// Get response value as string for validation
        /// </summary>
        private string? GetResponseValueAsString(FormTemplateResponse? response, string? dataType)
        {
            if (response == null)
                return null;

            var value = GetResponseValue(response, dataType);

            return value switch
            {
                null => null,
                DateTime dt => dt.ToString("yyyy-MM-dd"),
                bool b => b.ToString(),
                decimal d => d.ToString(),
                _ => value.ToString()
            };
        }

        /// <summary>
        /// Parse data type string to FormFieldType
        /// </summary>
        private FormFieldType ParseDataType(string? dataType)
        {
            if (string.IsNullOrEmpty(dataType))
                return FormFieldType.Text;

            return dataType.ToLowerInvariant() switch
            {
                "text" => FormFieldType.Text,
                "textarea" => FormFieldType.TextArea,
                "number" => FormFieldType.Number,
                "decimal" => FormFieldType.Decimal,
                "date" => FormFieldType.Date,
                "time" => FormFieldType.Time,
                "datetime" => FormFieldType.DateTime,
                "dropdown" => FormFieldType.Dropdown,
                "radio" => FormFieldType.Radio,
                "checkbox" => FormFieldType.Checkbox,
                "multiselect" => FormFieldType.MultiSelect,
                "fileupload" => FormFieldType.FileUpload,
                "image" => FormFieldType.Image,
                "signature" => FormFieldType.Signature,
                "rating" => FormFieldType.Rating,
                "slider" => FormFieldType.Slider,
                "email" => FormFieldType.Email,
                "phone" => FormFieldType.Phone,
                "url" => FormFieldType.Url,
                "currency" => FormFieldType.Currency,
                "percentage" => FormFieldType.Percentage,
                "boolean" => FormFieldType.Checkbox,
                _ => FormFieldType.Text
            };
        }

        /// <summary>
        /// Validate a field value against a validation rule
        /// </summary>
        private string? ValidateField(string value, FormItemValidation validation, FormTemplateItem item)
        {
            switch (validation.ValidationType.ToLowerInvariant())
            {
                case "required":
                    if (string.IsNullOrWhiteSpace(value))
                        return validation.ErrorMessage;
                    break;

                case "email":
                    if (!IsValidEmail(value))
                        return validation.ErrorMessage;
                    break;

                case "phone":
                    if (!IsValidPhone(value))
                        return validation.ErrorMessage;
                    break;

                case "url":
                    if (!IsValidUrl(value))
                        return validation.ErrorMessage;
                    break;

                case "minlength":
                    if (validation.MinLength.HasValue && value.Length < validation.MinLength.Value)
                        return validation.ErrorMessage;
                    break;

                case "maxlength":
                    if (validation.MaxLength.HasValue && value.Length > validation.MaxLength.Value)
                        return validation.ErrorMessage;
                    break;

                case "minvalue":
                case "min":
                    if (validation.MinValue.HasValue && decimal.TryParse(value, out var minVal) && minVal < validation.MinValue.Value)
                        return validation.ErrorMessage;
                    break;

                case "maxvalue":
                case "max":
                    if (validation.MaxValue.HasValue && decimal.TryParse(value, out var maxVal) && maxVal > validation.MaxValue.Value)
                        return validation.ErrorMessage;
                    break;

                case "range":
                    if (decimal.TryParse(value, out var rangeVal))
                    {
                        if ((validation.MinValue.HasValue && rangeVal < validation.MinValue.Value) ||
                            (validation.MaxValue.HasValue && rangeVal > validation.MaxValue.Value))
                            return validation.ErrorMessage;
                    }
                    break;

                case "regex":
                case "pattern":
                    if (!string.IsNullOrEmpty(validation.RegexPattern))
                    {
                        try
                        {
                            if (!Regex.IsMatch(value, validation.RegexPattern))
                                return validation.ErrorMessage;
                        }
                        catch
                        {
                            // Invalid regex pattern, skip validation
                        }
                    }
                    break;

                case "integer":
                    if (!int.TryParse(value, out _))
                        return validation.ErrorMessage;
                    break;

                case "decimal":
                case "number":
                    if (!decimal.TryParse(value, out _))
                        return validation.ErrorMessage;
                    break;
            }

            return null;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Basic phone validation: digits, spaces, dashes, parentheses, plus sign
            return Regex.IsMatch(phone, @"^[\d\s\-\(\)\+]+$") && phone.Length >= 7;
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Validate field value against configuration-based rules
        /// </summary>
        private List<string> ValidateFieldConfigurations(string value, FormTemplateItem item)
        {
            var errors = new List<string>();
            var configs = item.Configurations.ToDictionary(
                c => c.ConfigKey.ToLowerInvariant(),
                c => c.ConfigValue,
                StringComparer.OrdinalIgnoreCase);

            var fieldType = ParseDataType(item.DataType);

            // ========================================================================
            // NUMERIC VALIDATIONS (from configurations)
            // ========================================================================
            if (fieldType is FormFieldType.Number or FormFieldType.Decimal or FormFieldType.Currency
                or FormFieldType.Percentage or FormFieldType.Rating or FormFieldType.Slider)
            {
                if (decimal.TryParse(value, out var numericValue))
                {
                    if (configs.TryGetValue("minvalue", out var minValStr) && decimal.TryParse(minValStr, out var minVal))
                    {
                        if (numericValue < minVal)
                            errors.Add($"{item.ItemName} must be at least {minVal}.");
                    }

                    if (configs.TryGetValue("maxvalue", out var maxValStr) && decimal.TryParse(maxValStr, out var maxVal))
                    {
                        if (numericValue > maxVal)
                            errors.Add($"{item.ItemName} must be at most {maxVal}.");
                    }

                    // Rating specific
                    if (fieldType == FormFieldType.Rating)
                    {
                        if (configs.TryGetValue("ratingmax", out var ratingMaxStr) && int.TryParse(ratingMaxStr, out var ratingMax))
                        {
                            if (numericValue > ratingMax || numericValue < 1)
                                errors.Add($"{item.ItemName} must be between 1 and {ratingMax}.");
                        }
                    }

                    // Slider specific
                    if (fieldType == FormFieldType.Slider)
                    {
                        if (configs.TryGetValue("slidermin", out var sliderMinStr) && decimal.TryParse(sliderMinStr, out var sliderMin))
                        {
                            if (numericValue < sliderMin)
                                errors.Add($"{item.ItemName} must be at least {sliderMin}.");
                        }
                        if (configs.TryGetValue("slidermax", out var sliderMaxStr) && decimal.TryParse(sliderMaxStr, out var sliderMax))
                        {
                            if (numericValue > sliderMax)
                                errors.Add($"{item.ItemName} must be at most {sliderMax}.");
                        }
                    }
                }
            }

            // ========================================================================
            // TEXT LENGTH VALIDATIONS (from configurations)
            // ========================================================================
            if (fieldType is FormFieldType.Text or FormFieldType.TextArea or FormFieldType.Email
                or FormFieldType.Phone or FormFieldType.Url)
            {
                if (configs.TryGetValue("minlength", out var minLenStr) && int.TryParse(minLenStr, out var minLen))
                {
                    if (value.Length < minLen)
                        errors.Add($"{item.ItemName} must be at least {minLen} characters.");
                }

                if (configs.TryGetValue("maxlength", out var maxLenStr) && int.TryParse(maxLenStr, out var maxLen))
                {
                    if (value.Length > maxLen)
                        errors.Add($"{item.ItemName} must be at most {maxLen} characters.");
                }
            }

            // ========================================================================
            // DATE VALIDATIONS (from configurations)
            // ========================================================================
            if (fieldType is FormFieldType.Date or FormFieldType.DateTime)
            {
                if (DateTime.TryParse(value, out var dateValue))
                {
                    if (configs.TryGetValue("mindate", out var minDateStr) && DateTime.TryParse(minDateStr, out var minDate))
                    {
                        if (dateValue.Date < minDate.Date)
                            errors.Add($"{item.ItemName} must be on or after {minDate:yyyy-MM-dd}.");
                    }

                    if (configs.TryGetValue("maxdate", out var maxDateStr) && DateTime.TryParse(maxDateStr, out var maxDate))
                    {
                        if (dateValue.Date > maxDate.Date)
                            errors.Add($"{item.ItemName} must be on or before {maxDate:yyyy-MM-dd}.");
                    }
                }
            }

            // ========================================================================
            // TIME VALIDATIONS (from configurations)
            // ========================================================================
            if (fieldType == FormFieldType.Time)
            {
                if (TimeSpan.TryParse(value, out var timeValue))
                {
                    if (configs.TryGetValue("mintime", out var minTimeStr) && TimeSpan.TryParse(minTimeStr, out var minTime))
                    {
                        if (timeValue < minTime)
                            errors.Add($"{item.ItemName} must be at or after {minTime:hh\\:mm}.");
                    }

                    if (configs.TryGetValue("maxtime", out var maxTimeStr) && TimeSpan.TryParse(maxTimeStr, out var maxTime))
                    {
                        if (timeValue > maxTime)
                            errors.Add($"{item.ItemName} must be at or before {maxTime:hh\\:mm}.");
                    }
                }
            }

            // ========================================================================
            // MULTISELECT VALIDATIONS (from configurations)
            // ========================================================================
            if (fieldType == FormFieldType.MultiSelect)
            {
                var selectedCount = string.IsNullOrEmpty(value) ? 0 : value.Split(',').Length;

                if (configs.TryGetValue("minselections", out var minSelStr) && int.TryParse(minSelStr, out var minSel))
                {
                    if (selectedCount < minSel)
                        errors.Add($"{item.ItemName} requires at least {minSel} selection(s).");
                }

                if (configs.TryGetValue("maxselections", out var maxSelStr) && int.TryParse(maxSelStr, out var maxSel))
                {
                    if (selectedCount > maxSel)
                        errors.Add($"{item.ItemName} allows at most {maxSel} selection(s).");
                }
            }

            // ========================================================================
            // FILE UPLOAD VALIDATIONS (from configurations)
            // Note: Actual file size validation happens during upload, but we can validate extension
            // ========================================================================
            if (fieldType == FormFieldType.FileUpload || fieldType == FormFieldType.Image)
            {
                if (configs.TryGetValue("allowedfiletypes", out var allowedTypes) && !string.IsNullOrEmpty(allowedTypes))
                {
                    var allowedExtensions = allowedTypes.Split(',').Select(e => e.Trim().ToLowerInvariant()).ToList();
                    var fileExtension = Path.GetExtension(value)?.ToLowerInvariant();

                    if (!string.IsNullOrEmpty(fileExtension) && !allowedExtensions.Contains(fileExtension))
                    {
                        errors.Add($"{item.ItemName} must be one of the following types: {allowedTypes}.");
                    }
                }

                if (fieldType == FormFieldType.Image)
                {
                    if (configs.TryGetValue("allowedimagetypes", out var allowedImageTypes) && !string.IsNullOrEmpty(allowedImageTypes))
                    {
                        var allowedExtensions = allowedImageTypes.Split(',').Select(e => e.Trim().ToLowerInvariant()).ToList();
                        var fileExtension = Path.GetExtension(value)?.ToLowerInvariant();

                        if (!string.IsNullOrEmpty(fileExtension) && !allowedExtensions.Contains(fileExtension))
                        {
                            errors.Add($"{item.ItemName} must be one of the following image types: {allowedImageTypes}.");
                        }
                    }
                }
            }

            // ========================================================================
            // DROPDOWN VALIDATIONS - Ensure value is in options list
            // ========================================================================
            if (fieldType is FormFieldType.Dropdown or FormFieldType.Radio)
            {
                if (item.Options != null && item.Options.Any())
                {
                    var validValues = item.Options.Where(o => o.IsActive).Select(o => o.OptionValue).ToList();
                    if (!validValues.Contains(value))
                    {
                        errors.Add($"{item.ItemName} has an invalid selection.");
                    }
                }
            }

            return errors;
        }
    }
}

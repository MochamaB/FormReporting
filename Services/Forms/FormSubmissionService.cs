using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Components;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.Common;
using FormReporting.Services.Identity;
using System.Security.Claims;
using System.Text.Json;
using System.Globalization;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service implementation for form submission operations
    /// Transforms database entities to ViewModels for rendering
    /// </summary>
    public class FormSubmissionService : IFormSubmissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IScopeService _scopeService;
        private readonly IFormTemplateService _templateService;
        private readonly IFormAssignmentService _assignmentService;

        public FormSubmissionService(
            ApplicationDbContext context, 
            IScopeService scopeService,
            IFormTemplateService templateService,
            IFormAssignmentService assignmentService)
        {
            _context = context;
            _scopeService = scopeService;
            _templateService = templateService;
            _assignmentService = assignmentService;
        }

        /// <inheritdoc />
        public async Task<FormViewModel> BuildFormViewModelAsync(int templateId, int? submissionId = null, bool readOnly = false)
        {
            // Load template with all related data
            var template = await _context.FormTemplates
                .Include(t => t.Category)
                .Include(t => t.Sections.OrderBy(s => s.DisplayOrder))
                    .ThenInclude(s => s.Items.OrderBy(i => i.DisplayOrder))
                        .ThenInclude(i => i.Options.OrderBy(o => o.DisplayOrder))
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Items)
                        .ThenInclude(i => i.Validations.Where(v => v.IsActive))
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Items)
                        .ThenInclude(i => i.Configurations)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
            {
                throw new InvalidOperationException($"Template with ID {templateId} not found.");
            }

            // Load existing responses if resuming
            Dictionary<int, FormTemplateResponse> responsesByItemId = new();
            FormTemplateSubmission? submission = null;

            if (submissionId.HasValue)
            {
                submission = await _context.FormTemplateSubmissions
                    .Include(s => s.Responses)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SubmissionId == submissionId.Value);

                if (submission != null)
                {
                    responsesByItemId = submission.Responses.ToDictionary(r => r.ItemId);
                }
            }

            // Build the ViewModel
            var viewModel = new FormViewModel
            {
                FormId = $"submission_{templateId}_{submissionId ?? 0}",
                FormTemplateId = templateId,
                ResponseId = submissionId,
                Title = template.TemplateName,
                Description = template.Description,
                SubmitUrl = "/Submissions/Submit",
                SaveDraftUrl = "/api/submissions/auto-save",
                AutoSaveUrl = "/api/submissions/auto-save",
                EnableAutoSave = !readOnly,
                AutoSaveIntervalSeconds = 30,
                AutoSaveIntervalMs = 30000,
                ShowProgressBar = true,
                RenderMode = template.Sections.Count > 1 ? FormRenderMode.Wizard : FormRenderMode.SinglePage,
                ShowStepNumbers = true,
                AllowStepSkipping = false,
                ValidateOnStepChange = true,
                SubmitButtonText = "Submit Form",
                SaveDraftButtonText = "Save Draft",
                AllowSaveDraft = !readOnly,
                ShowResetButton = !readOnly,
                ShowCancelButton = true,
                PreviousButtonText = "Previous",
                NextButtonText = "Next",
                CancelUrl = "/Submissions",
                IsReadOnly = readOnly,
                TemplateId = templateId,
                SubmissionId = submissionId ?? 0,
                CurrentSectionIndex = submission?.CurrentSection ?? 0
            };

            // Build sections
            int totalFields = 0;
            int requiredFields = 0;

            foreach (var section in template.Sections.OrderBy(s => s.DisplayOrder))
            {
                var sectionViewModel = new FormSectionViewModel
                {
                    SectionId = section.SectionId,
                    SectionName = section.SectionName,
                    SectionDescription = section.SectionDescription,
                    IconClass = section.IconClass,
                    DisplayOrder = section.DisplayOrder,
                    IsCollapsible = section.IsCollapsible,
                    IsCollapsedByDefault = section.IsCollapsedByDefault,
                    CollapseId = $"section_{section.SectionId}"
                };

                // Build fields
                var fields = new List<FormFieldViewModel>();

                foreach (var item in section.Items.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder))
                {
                    var fieldViewModel = BuildFieldViewModel(item, responsesByItemId, readOnly);
                    fields.Add(fieldViewModel);

                    totalFields++;
                    if (item.IsRequired) requiredFields++;
                }

                sectionViewModel.Fields = fields;
                sectionViewModel.IsRequired = fields.Any(f => f.IsRequired);

                // Build field rows for multi-column layout
                sectionViewModel.FieldRows = BuildFieldRows(fields);

                viewModel.Sections.Add(sectionViewModel);
            }

            viewModel.TotalFields = totalFields;
            viewModel.RequiredFields = requiredFields;

            return viewModel;
        }

        /// <summary>
        /// Build a single field ViewModel from a FormTemplateItem
        /// </summary>
        private FormFieldViewModel BuildFieldViewModel(
            FormTemplateItem item,
            Dictionary<int, FormTemplateResponse> responsesByItemId,
            bool readOnly)
        {
            var fieldType = ParseDataTypeToFieldType(item.DataType);

            var fieldViewModel = new FormFieldViewModel
            {
                FieldId = item.ItemId.ToString(),
                FieldName = item.ItemName,
                FieldDescription = item.ItemDescription,
                FieldType = fieldType,
                IsRequired = item.IsRequired,
                DefaultValue = item.DefaultValue,
                PlaceholderText = item.PlaceholderText,
                HelpText = item.HelpText,
                PrefixText = item.PrefixText,
                SuffixText = item.SuffixText,
                DisplayOrder = item.DisplayOrder,
                IsReadOnly = readOnly,
                IsDisabled = readOnly,
                InputName = $"responses[{item.ItemId}]",
                InputId = $"field_{item.ItemId}",
                LayoutType = ParseLayoutType(item.LayoutType),
                MatrixGroupId = item.MatrixGroupId,
                MatrixRowLabel = item.MatrixRowLabel
            };

            // Set current value from response or default
            if (responsesByItemId.TryGetValue(item.ItemId, out var response))
            {
                fieldViewModel.CurrentValue = ExtractResponseValue(response, fieldType);
            }
            else if (!string.IsNullOrEmpty(item.DefaultValue))
            {
                fieldViewModel.CurrentValue = item.DefaultValue;
            }

            // Build options for selection fields
            if (item.Options != null && item.Options.Any())
            {
                fieldViewModel.Options = item.Options
                    .Where(o => o.IsActive)
                    .OrderBy(o => o.DisplayOrder)
                    .Select(o => new FormFieldOption
                    {
                        Value = o.OptionValue,
                        Label = o.OptionLabel,
                        IsDefault = o.IsDefault,
                        IsSelected = IsOptionSelected(o, fieldViewModel.CurrentValue),
                        ParentOptionId = o.ParentOptionId,
                        DisplayOrder = o.DisplayOrder
                    })
                    .ToList();
            }

            // Build validations
            if (item.Validations != null && item.Validations.Any())
            {
                fieldViewModel.Validations = item.Validations
                    .Where(v => v.IsActive)
                    .OrderBy(v => v.ValidationOrder)
                    .Select(v => new FormFieldValidation
                    {
                        ValidationType = v.ValidationType,
                        MinValue = v.MinValue,
                        MaxValue = v.MaxValue,
                        MinLength = v.MinLength,
                        MaxLength = v.MaxLength,
                        RegexPattern = v.RegexPattern,
                        CustomExpression = v.CustomExpression,
                        ErrorMessage = v.ErrorMessage,
                        Severity = v.Severity,
                        ValidationOrder = v.ValidationOrder
                    })
                    .ToList();
            }

            // Parse conditional logic
            if (!string.IsNullOrEmpty(item.ConditionalLogic))
            {
                try
                {
                    fieldViewModel.ConditionalLogic = JsonSerializer.Deserialize<ConditionalLogic>(
                        item.ConditionalLogic,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    // Invalid JSON, ignore conditional logic
                }
            }

            // Extract configuration values
            if (item.Configurations != null)
            {
                ExtractFieldConfigurations(item.Configurations, fieldViewModel, fieldType);
            }

            // Build data attributes for client-side validation
            fieldViewModel.ValidationDataAttribute = BuildValidationDataAttribute(fieldViewModel.Validations);
            fieldViewModel.ConditionalLogicDataAttribute = fieldViewModel.ConditionalLogic != null
                ? JsonSerializer.Serialize(fieldViewModel.ConditionalLogic)
                : string.Empty;

            return fieldViewModel;
        }

        /// <summary>
        /// Parse DataType string to FormFieldType enum
        /// </summary>
        private FormFieldType ParseDataTypeToFieldType(string? dataType)
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
        /// Parse LayoutType string to enum
        /// </summary>
        private FieldLayoutType ParseLayoutType(string? layoutType)
        {
            if (string.IsNullOrEmpty(layoutType))
                return FieldLayoutType.Single;

            return layoutType.ToLowerInvariant() switch
            {
                "single" => FieldLayoutType.Single,
                "matrix" => FieldLayoutType.Matrix,
                "grid" => FieldLayoutType.Grid,
                "inline" => FieldLayoutType.Inline,
                _ => FieldLayoutType.Single
            };
        }

        /// <summary>
        /// Extract the appropriate value from a response based on field type
        /// </summary>
        private object? ExtractResponseValue(FormTemplateResponse response, FormFieldType fieldType)
        {
            return fieldType switch
            {
                FormFieldType.Number or FormFieldType.Decimal or FormFieldType.Currency or
                FormFieldType.Percentage or FormFieldType.Rating or FormFieldType.Slider
                    => response.NumericValue,

                FormFieldType.Date => response.DateValue?.ToString("yyyy-MM-dd"),
                FormFieldType.Time => response.DateValue?.ToString("HH:mm"),
                FormFieldType.DateTime => response.DateValue?.ToString("yyyy-MM-ddTHH:mm"),

                FormFieldType.Checkbox => response.BooleanValue,

                _ => response.TextValue
            };
        }

        /// <summary>
        /// Check if an option is selected based on current value
        /// </summary>
        private bool IsOptionSelected(FormItemOption option, object? currentValue)
        {
            if (currentValue == null)
                return option.IsDefault;

            var valueStr = currentValue.ToString();

            // For multi-select, check if value contains this option
            if (valueStr != null && valueStr.Contains(','))
            {
                var selectedValues = valueStr.Split(',').Select(v => v.Trim());
                return selectedValues.Contains(option.OptionValue);
            }

            return option.OptionValue == valueStr;
        }

        /// <summary>
        /// Extract all configuration values from FormItemConfiguration and apply to ViewModel
        /// </summary>
        private void ExtractFieldConfigurations(
            ICollection<FormItemConfiguration> configurations,
            FormFieldViewModel fieldViewModel,
            FormFieldType fieldType)
        {
            var configs = configurations.ToDictionary(
                c => c.ConfigKey.ToLowerInvariant(),
                c => c.ConfigValue,
                StringComparer.OrdinalIgnoreCase);

            // ========================================================================
            // LAYOUT & DISPLAY
            // ========================================================================
            if (configs.TryGetValue("columnwidth", out var colWidthStr) && int.TryParse(colWidthStr, out var colWidth))
                fieldViewModel.ColumnWidth = colWidth;
            if (configs.TryGetValue("cssclass", out var cssClass))
                fieldViewModel.CssClass = cssClass;
            if (configs.TryGetValue("iconclass", out var iconClass))
                fieldViewModel.IconClass = iconClass;
            if (configs.TryGetValue("iconposition", out var iconPosition))
                fieldViewModel.IconPosition = iconPosition ?? "prefix";
            if (configs.TryGetValue("tooltip", out var tooltip))
                fieldViewModel.Tooltip = tooltip;

            // ========================================================================
            // TEXT FIELD CONFIGURATIONS
            // ========================================================================
            if (configs.TryGetValue("maxlength", out var maxLengthStr) && int.TryParse(maxLengthStr, out var maxLength))
                fieldViewModel.MaxLength = maxLength;
            if (configs.TryGetValue("minlength", out var minLengthStr) && int.TryParse(minLengthStr, out var minLength))
                fieldViewModel.MinLength = minLength;
            if (configs.TryGetValue("rows", out var rowsStr) && int.TryParse(rowsStr, out var rows))
                fieldViewModel.Rows = rows;
            if (configs.TryGetValue("showcharactercount", out var showCharCount))
                fieldViewModel.ShowCharacterCount = showCharCount?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
            if (configs.TryGetValue("autocapitalize", out var autoCap))
                fieldViewModel.AutoCapitalize = autoCap;
            if (configs.TryGetValue("autocomplete", out var autoComplete))
                fieldViewModel.AutoComplete = autoComplete;
            if (configs.TryGetValue("spellcheck", out var spellCheck))
                fieldViewModel.SpellCheck = spellCheck?.Equals("true", StringComparison.OrdinalIgnoreCase);
            if (configs.TryGetValue("inputmask", out var inputMask))
                fieldViewModel.InputMask = inputMask;

            // ========================================================================
            // NUMERIC FIELD CONFIGURATIONS
            // ========================================================================
            if (configs.TryGetValue("minvalue", out var minValStr) && decimal.TryParse(minValStr, out var minVal))
                fieldViewModel.MinValue = minVal;
            if (configs.TryGetValue("maxvalue", out var maxValStr) && decimal.TryParse(maxValStr, out var maxVal))
                fieldViewModel.MaxValue = maxVal;
            if (configs.TryGetValue("step", out var stepStr) && decimal.TryParse(stepStr, out var step))
                fieldViewModel.Step = step;
            if (configs.TryGetValue("decimalplaces", out var decPlacesStr) && int.TryParse(decPlacesStr, out var decPlaces))
                fieldViewModel.DecimalPlaces = decPlaces;

            // ========================================================================
            // DATE/TIME CONFIGURATIONS
            // ========================================================================
            if (configs.TryGetValue("mindate", out var minDate))
                fieldViewModel.MinDate = minDate;
            if (configs.TryGetValue("maxdate", out var maxDate))
                fieldViewModel.MaxDate = maxDate;
            if (configs.TryGetValue("mintime", out var minTime))
                fieldViewModel.MinTime = minTime;
            if (configs.TryGetValue("maxtime", out var maxTime))
                fieldViewModel.MaxTime = maxTime;

            // ========================================================================
            // RATING FIELD CONFIGURATIONS
            // ========================================================================
            if (configs.TryGetValue("ratingmax", out var ratingMaxStr) && int.TryParse(ratingMaxStr, out var ratingMax))
                fieldViewModel.RatingMax = ratingMax;
            if (configs.TryGetValue("ratingicon", out var ratingIcon))
                fieldViewModel.RatingIcon = ratingIcon ?? "star";
            if (configs.TryGetValue("allowhalfrating", out var allowHalf))
                fieldViewModel.AllowHalfRating = allowHalf?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

            // ========================================================================
            // SLIDER FIELD CONFIGURATIONS
            // ========================================================================
            if (configs.TryGetValue("slidermin", out var sliderMinStr) && decimal.TryParse(sliderMinStr, out var sliderMin))
                fieldViewModel.SliderMin = sliderMin;
            if (configs.TryGetValue("slidermax", out var sliderMaxStr) && decimal.TryParse(sliderMaxStr, out var sliderMax))
                fieldViewModel.SliderMax = sliderMax;
            if (configs.TryGetValue("sliderstep", out var sliderStepStr) && decimal.TryParse(sliderStepStr, out var sliderStep))
                fieldViewModel.SliderStep = sliderStep;
            if (configs.TryGetValue("showsliderticks", out var showTicks))
                fieldViewModel.ShowSliderTicks = showTicks?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
            if (configs.TryGetValue("showslidervalue", out var showValue))
                fieldViewModel.ShowSliderValue = showValue?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? true;

            // ========================================================================
            // FILE UPLOAD CONFIGURATIONS
            // ========================================================================
            if (configs.TryGetValue("allowedfiletypes", out var allowedTypes))
                fieldViewModel.AllowedFileTypes = allowedTypes;
            if (configs.TryGetValue("maxfilesize", out var maxFileSizeStr) && long.TryParse(maxFileSizeStr, out var maxFileSize))
            {
                fieldViewModel.MaxFileSize = maxFileSize;
                fieldViewModel.MaxFileSizeDisplay = FormatFileSize(maxFileSize);
            }
            if (configs.TryGetValue("allowmultiplefiles", out var allowMultiple))
                fieldViewModel.AllowMultipleFiles = allowMultiple?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
            if (configs.TryGetValue("maxfiles", out var maxFilesStr) && int.TryParse(maxFilesStr, out var maxFiles))
                fieldViewModel.MaxFiles = maxFiles;

            // ========================================================================
            // IMAGE FIELD CONFIGURATIONS
            // ========================================================================
            if (configs.TryGetValue("allowedimagetypes", out var allowedImageTypes))
                fieldViewModel.AllowedImageTypes = allowedImageTypes;
            if (configs.TryGetValue("maximagewidth", out var maxWidthStr) && int.TryParse(maxWidthStr, out var maxWidth))
                fieldViewModel.MaxImageWidth = maxWidth;
            if (configs.TryGetValue("maximageheight", out var maxHeightStr) && int.TryParse(maxHeightStr, out var maxHeight))
                fieldViewModel.MaxImageHeight = maxHeight;
            if (configs.TryGetValue("enableimagecrop", out var enableCrop))
                fieldViewModel.EnableImageCrop = enableCrop?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
            if (configs.TryGetValue("imageaspectratio", out var aspectRatio))
                fieldViewModel.ImageAspectRatio = aspectRatio;

            // ========================================================================
            // SIGNATURE FIELD CONFIGURATIONS
            // ========================================================================
            if (configs.TryGetValue("signaturewidth", out var sigWidthStr) && int.TryParse(sigWidthStr, out var sigWidth))
                fieldViewModel.SignatureWidth = sigWidth;
            if (configs.TryGetValue("signatureheight", out var sigHeightStr) && int.TryParse(sigHeightStr, out var sigHeight))
                fieldViewModel.SignatureHeight = sigHeight;
            if (configs.TryGetValue("signaturepencolor", out var penColor))
                fieldViewModel.SignaturePenColor = penColor ?? "#000000";
            if (configs.TryGetValue("signaturepenwidth", out var penWidthStr) && int.TryParse(penWidthStr, out var penWidth))
                fieldViewModel.SignaturePenWidth = penWidth;

            // ========================================================================
            // DROPDOWN/SELECT CONFIGURATIONS
            // ========================================================================
            if (configs.TryGetValue("enablesearch", out var enableSearch))
                fieldViewModel.EnableSearch = enableSearch?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
            if (configs.TryGetValue("dropdownplaceholder", out var ddPlaceholder))
                fieldViewModel.DropdownPlaceholder = ddPlaceholder;
            if (configs.TryGetValue("allowclear", out var allowClear))
                fieldViewModel.AllowClear = allowClear?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
            if (configs.TryGetValue("isdynamicoptions", out var isDynamic))
                fieldViewModel.IsDynamicOptions = isDynamic?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
            if (configs.TryGetValue("optionsapiurl", out var apiUrl))
                fieldViewModel.OptionsApiUrl = apiUrl;
            if (configs.TryGetValue("cascadeparentfieldid", out var cascadeParent))
                fieldViewModel.CascadeParentFieldId = cascadeParent;

            // ========================================================================
            // MULTISELECT CONFIGURATIONS
            // ========================================================================
            if (configs.TryGetValue("minselections", out var minSelStr) && int.TryParse(minSelStr, out var minSel))
                fieldViewModel.MinSelections = minSel;
            if (configs.TryGetValue("maxselections", out var maxSelStr) && int.TryParse(maxSelStr, out var maxSel))
                fieldViewModel.MaxSelections = maxSel;

            // ========================================================================
            // PHONE FIELD CONFIGURATIONS
            // ========================================================================
            if (configs.TryGetValue("defaultcountrycode", out var countryCode))
                fieldViewModel.DefaultCountryCode = countryCode;
            if (configs.TryGetValue("phoneformat", out var phoneFormat))
                fieldViewModel.PhoneFormat = phoneFormat;

            // ========================================================================
            // CURRENCY FIELD CONFIGURATIONS
            // ========================================================================
            if (configs.TryGetValue("currencycode", out var currCode))
                fieldViewModel.CurrencyCode = currCode ?? "KES";
            if (configs.TryGetValue("currencysymbol", out var currSymbol))
                fieldViewModel.CurrencySymbol = currSymbol;
            if (configs.TryGetValue("currencysymbolasprefix", out var currPrefix))
                fieldViewModel.CurrencySymbolAsPrefix = currPrefix?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? true;
        }

        /// <summary>
        /// Format file size in bytes to human-readable string
        /// </summary>
        private string FormatFileSize(long bytes)
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

        /// <summary>
        /// Build field rows for multi-column layout
        /// Groups fields by their column width to create rows
        /// </summary>
        private List<List<FormFieldViewModel>> BuildFieldRows(List<FormFieldViewModel> fields)
        {
            var rows = new List<List<FormFieldViewModel>>();
            var currentRow = new List<FormFieldViewModel>();
            int currentRowWidth = 0;

            foreach (var field in fields)
            {
                var fieldWidth = field.ColumnWidth > 0 ? field.ColumnWidth : 12;

                // If field doesn't fit in current row, start new row
                if (currentRowWidth + fieldWidth > 12 && currentRow.Any())
                {
                    rows.Add(currentRow);
                    currentRow = new List<FormFieldViewModel>();
                    currentRowWidth = 0;
                }

                currentRow.Add(field);
                currentRowWidth += fieldWidth;

                // If row is full, start new row
                if (currentRowWidth >= 12)
                {
                    rows.Add(currentRow);
                    currentRow = new List<FormFieldViewModel>();
                    currentRowWidth = 0;
                }
            }

            // Add remaining fields
            if (currentRow.Any())
            {
                rows.Add(currentRow);
            }

            return rows;
        }

        /// <summary>
        /// Build JSON data attribute for client-side validation
        /// </summary>
        private string BuildValidationDataAttribute(List<FormFieldValidation> validations)
        {
            if (validations == null || !validations.Any())
                return string.Empty;

            return JsonSerializer.Serialize(validations);
        }

        /// <inheritdoc />
        public async Task<FormTemplateSubmission> CreateSubmissionAsync(
            int templateId,
            int userId,
            int? tenantId,
            DateTime reportingPeriod)
        {
            // Verify template exists and is published
            var template = await _context.FormTemplates
                .FirstOrDefaultAsync(t => t.TemplateId == templateId && t.PublishStatus == "Published");

            if (template == null)
            {
                throw new InvalidOperationException($"Published template with ID {templateId} not found.");
            }

            var submission = new FormTemplateSubmission
            {
                TemplateId = templateId,
                TenantId = tenantId,
                ReportingYear = reportingPeriod.Year,
                ReportingMonth = (byte)reportingPeriod.Month,
                ReportingPeriod = new DateTime(reportingPeriod.Year, reportingPeriod.Month, 1),
                SnapshotDate = DateTime.Today,
                Status = "Draft",
                SubmittedBy = userId,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                LastSavedDate = DateTime.Now,
                CurrentSection = 0
            };

            _context.FormTemplateSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            return submission;
        }

        /// <inheritdoc />
        public async Task<FormTemplateSubmission?> GetSubmissionAsync(int submissionId)
        {
            return await _context.FormTemplateSubmissions
                .Include(s => s.Template)
                .Include(s => s.Responses)
                .Include(s => s.Tenant)
                .Include(s => s.Submitter)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
        }

        /// <inheritdoc />
        public async Task<List<FormTemplateSubmission>> GetUserSubmissionsAsync(int userId, string? status = null)
        {
            var query = _context.FormTemplateSubmissions
                .Include(s => s.Template)
                .Include(s => s.Tenant)
                .Where(s => s.SubmittedBy == userId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }

            return await query
                .OrderByDescending(s => s.ModifiedDate)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<FormTemplateSubmission>> GetTemplateSubmissionsAsync(int templateId, string? status = null)
        {
            var query = _context.FormTemplateSubmissions
                .Include(s => s.Submitter)
                .Include(s => s.Tenant)
                .Where(s => s.TemplateId == templateId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }

            return await query
                .OrderByDescending(s => s.SubmittedDate ?? s.CreatedDate)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<bool> CanUserAccessTemplateAsync(int userId, int templateId)
        {
            // Basic check: template must be published and active
            // TODO: Expand this for assignment-based access control
            var template = await _context.FormTemplates
                .FirstOrDefaultAsync(t => t.TemplateId == templateId
                    && t.PublishStatus == "Published"
                    && t.IsActive);

            return template != null;
        }

        /// <inheritdoc />
        public async Task<bool> UserOwnsSubmissionAsync(int userId, int submissionId)
        {
            return await _context.FormTemplateSubmissions
                .AnyAsync(s => s.SubmissionId == submissionId && s.SubmittedBy == userId);
        }

        /// <inheritdoc />
        public async Task<List<FormTemplate>> GetAvailableTemplatesAsync(int userId)
        {
            // Basic implementation: return all published, active templates
            // TODO: Filter by assignments when implemented
            return await _context.FormTemplates
                .Include(t => t.Category)
                .Include(t => t.Sections.OrderBy(s => s.DisplayOrder))
                    .ThenInclude(s => s.Items.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder))
                .Where(t => t.PublishStatus == "Published" && t.IsActive)
                .OrderBy(t => t.Category!.CategoryName)
                .ThenBy(t => t.TemplateName)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<FormTemplateSubmission?> GetExistingDraftAsync(
            int userId,
            int templateId,
            int? tenantId,
            DateTime reportingPeriod)
        {
            var query = _context.FormTemplateSubmissions
                .Where(s => s.SubmittedBy == userId
                    && s.TemplateId == templateId
                    && s.Status == "Draft"
                    && s.ReportingPeriod.Year == reportingPeriod.Year
                    && s.ReportingPeriod.Month == reportingPeriod.Month);

            if (tenantId.HasValue)
            {
                query = query.Where(s => s.TenantId == tenantId.Value);
            }
            else
            {
                query = query.Where(s => s.TenantId == null);
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<List<Models.Entities.Organizational.Tenant>> GetTenantsAsync(List<int> tenantIds)
        {
            if (tenantIds == null || !tenantIds.Any())
            {
                return new List<Models.Entities.Organizational.Tenant>();
            }

            return await _context.Tenants
                .Where(t => tenantIds.Contains(t.TenantId) && t.IsActive)
                .OrderBy(t => t.TenantName)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Dictionary<int, TemplateSubmissionStats>> GetTemplateSubmissionStatsAsync(int userId, List<int> templateIds)
        {
            if (templateIds == null || !templateIds.Any())
            {
                return new Dictionary<int, TemplateSubmissionStats>();
            }

            // Get all submissions for the user and specified templates
            var submissions = await _context.FormTemplateSubmissions
                .Where(s => s.SubmittedBy == userId && templateIds.Contains(s.TemplateId))
                .Select(s => new { s.TemplateId, s.Status })
                .ToListAsync();

            // Group by template and calculate stats
            var stats = submissions
                .GroupBy(s => s.TemplateId)
                .ToDictionary(
                    g => g.Key,
                    g => new TemplateSubmissionStats
                    {
                        TotalResponses = g.Count(),
                        SubmittedCount = g.Count(s => s.Status == "Submitted" || s.Status == "Approved"),
                        DraftCount = g.Count(s => s.Status == "Draft")
                    }
                );

            // Ensure all requested templates have an entry (even if 0)
            foreach (var templateId in templateIds)
            {
                if (!stats.ContainsKey(templateId))
                {
                    stats[templateId] = new TemplateSubmissionStats
                    {
                        TotalResponses = 0,
                        SubmittedCount = 0,
                        DraftCount = 0
                    };
                }
            }

            return stats;
        }

        // ========================================================================
        // SCOPE-AWARE METHODS
        // ========================================================================

        /// <inheritdoc />
        public async Task<TemplateSubmissionsViewModel> GetScopedTemplateSubmissionsAsync(
            int templateId,
            ClaimsPrincipal user,
            SubmissionFilters? filters = null,
            int page = 1,
            int pageSize = 10)
        {
            // Get template with sections, items, and options
            var template = await _context.FormTemplates
                .Include(t => t.Category)
                .Include(t => t.Sections.OrderBy(s => s.DisplayOrder))
                    .ThenInclude(s => s.Items.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder))
                        .ThenInclude(i => i.Options.Where(o => o.IsActive).OrderBy(o => o.DisplayOrder))
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
            {
                throw new InvalidOperationException($"Template with ID {templateId} not found.");
            }

            // Get user scope
            var userScope = await _scopeService.GetUserScopeAsync(user);
            var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(user);

            // Build base query with scope filtering
            var query = _context.FormTemplateSubmissions
                .Include(s => s.Submitter)
                .Include(s => s.Tenant)
                .Include(s => s.Responses)
                    .ThenInclude(r => r.SelectedOption)
                .Include(s => s.Responses)
                    .ThenInclude(r => r.Item)
                        .ThenInclude(i => i.Options)
                .Where(s => s.TemplateId == templateId)
                .AsQueryable();

            // Apply scope filtering
            query = ApplyScopeFilter(query, userScope, accessibleTenantIds);

            // Apply additional filters
            if (filters != null)
            {
                query = ApplySubmissionFilters(query, filters);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Get all submissions for stats (before pagination)
            var allSubmissions = await query.Select(s => s.Status).ToListAsync();
            var summaryStats = new SubmissionSummaryStats
            {
                TotalCount = totalCount,
                DraftCount = allSubmissions.Count(s => s == "Draft"),
                SubmittedCount = allSubmissions.Count(s => s == "Submitted"),
                InApprovalCount = allSubmissions.Count(s => s == "InApproval"),
                ApprovedCount = allSubmissions.Count(s => s == "Approved"),
                RejectedCount = allSubmissions.Count(s => s == "Rejected")
            };

            // Calculate pagination
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Get paginated submissions
            var submissions = await query
                .OrderByDescending(s => s.SubmittedDate ?? s.ModifiedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get field columns with section info for table display
            var allFields = template.Sections
                .OrderBy(s => s.DisplayOrder)
                .SelectMany(s => s.Items
                    .Where(i => i.IsActive)
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new { Section = s, Item = i }))
                .ToList();

            var fieldColumns = new List<FormFieldColumnInfo>();
            int? lastSectionId = null;
            
            foreach (var field in allFields)
            {
                fieldColumns.Add(new FormFieldColumnInfo
                {
                    ItemId = field.Item.ItemId,
                    SectionId = field.Section.SectionId,
                    SectionName = field.Section.SectionName,
                    IsFirstInSection = lastSectionId != field.Section.SectionId,
                    FieldName = field.Item.ItemName,
                    ShortName = field.Item.ItemName.Length > 20 ? field.Item.ItemName.Substring(0, 17) + "..." : field.Item.ItemName,
                    DataType = field.Item.DataType ?? "Text",
                    DisplayOrder = field.Item.DisplayOrder,
                    Options = field.Item.Options?
                        .Select(o => new FieldOptionInfo
                        {
                            OptionId = o.OptionId,
                            OptionValue = o.OptionValue,
                            OptionLabel = o.OptionLabel
                        })
                        .ToList() ?? new List<FieldOptionInfo>()
                });
                lastSectionId = field.Section.SectionId;
            }

            // Get total field count for completion percentage
            var totalFields = template.Sections.SelectMany(s => s.Items).Count(i => i.IsActive);

            // Build submission rows
            var submissionRows = new List<SubmissionRowViewModel>();
            int rowNumber = (page - 1) * pageSize + 1;

            foreach (var submission in submissions)
            {
                var row = BuildSubmissionRow(submission, fieldColumns, totalFields, rowNumber++);
                submissionRows.Add(row);
            }

            // Get available tenants for filter dropdown (within scope)
            var availableTenants = await GetAvailableTenantsForFilter(templateId, accessibleTenantIds);

            // Get available submitters for filter dropdown
            var availableSubmitters = await GetAvailableSubmittersForFilter(templateId, accessibleTenantIds);

            // Build scope display text
            var scopeDisplayText = BuildScopeDisplayText(userScope, totalCount);

            // Build the ViewModel
            return new TemplateSubmissionsViewModel
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                TemplateCode = template.TemplateCode,
                Description = template.Description,
                CategoryName = template.Category?.CategoryName ?? "Uncategorized",
                CategoryIcon = template.Category?.IconClass ?? "ri-file-list-3-line",
                Version = template.Version.ToString(),
                TemplateType = template.TemplateType ?? "Form",
                PublishedDate = template.PublishedDate,
                RequiresApproval = template.RequiresApproval,
                ScopeCode = userScope.ScopeCode,
                ScopeDisplayText = scopeDisplayText,
                TotalSubmissions = totalCount,
                FieldColumns = fieldColumns,
                Submissions = submissionRows,
                Summary = summaryStats,
                Filters = filters ?? new SubmissionFilters(),
                AvailableTenants = availableTenants,
                AvailableSubmitters = availableSubmitters,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalRecords = totalCount
            };
        }

        /// <inheritdoc />
        public async Task<SubmissionDetailViewModel?> GetSubmissionDetailAsync(int submissionId, ClaimsPrincipal user)
        {
            // Get submission with all related data
            var submission = await _context.FormTemplateSubmissions
                .Include(s => s.Template)
                    .ThenInclude(t => t.Sections.OrderBy(sec => sec.DisplayOrder))
                        .ThenInclude(sec => sec.Items.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder))
                            .ThenInclude(i => i.Options)
                .Include(s => s.Responses)
                .Include(s => s.Submitter)
                .Include(s => s.Tenant)
                .Include(s => s.Reviewer)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
            {
                return null;
            }

            // Check if user can access this submission
            if (!await CanUserAccessSubmissionAsync(submissionId, user))
            {
                return null;
            }

            // Build responses dictionary for quick lookup
            var responsesByItemId = submission.Responses.ToDictionary(r => r.ItemId);

            // Get total field count
            var totalFields = submission.Template.Sections
                .SelectMany(s => s.Items)
                .Count(i => i.IsActive);

            var answeredCount = submission.Responses.Count(r => HasResponseValue(r));

            // Build section response groups
            var sections = new List<SectionResponseGroup>();
            foreach (var section in submission.Template.Sections.OrderBy(s => s.DisplayOrder))
            {
                var sectionGroup = new SectionResponseGroup
                {
                    SectionId = section.SectionId,
                    SectionName = section.SectionName,
                    SectionDescription = section.SectionDescription,
                    IconClass = section.IconClass ?? "ri-list-check",
                    DisplayOrder = section.DisplayOrder,
                    Responses = new List<ResponseDetailViewModel>()
                };

                foreach (var item in section.Items.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder))
                {
                    responsesByItemId.TryGetValue(item.ItemId, out var response);
                    var responseDetail = BuildResponseDetail(item, response);
                    sectionGroup.Responses.Add(responseDetail);
                }

                sections.Add(sectionGroup);
            }

            // Build the ViewModel
            return new SubmissionDetailViewModel
            {
                SubmissionId = submission.SubmissionId,
                SubmissionNumber = submission.SubmissionId, // Could be sequential per template
                TemplateId = submission.TemplateId,
                TemplateName = submission.Template.TemplateName,
                SubmitterName = submission.Submitter?.FullName ?? "Unknown",
                SubmitterEmail = submission.Submitter?.Email ?? "",
                SubmittedBy = submission.SubmittedBy,
                TenantName = submission.Tenant?.TenantName,
                TenantId = submission.TenantId,
                ReportingPeriod = FormatReportingPeriod(submission.ReportingYear, submission.ReportingMonth),
                ReportingYear = submission.ReportingYear,
                ReportingMonth = submission.ReportingMonth,
                Status = submission.Status,
                StatusBadgeClass = GetStatusBadgeClass(submission.Status),
                SubmittedDate = submission.SubmittedDate,
                FormattedSubmittedDate = FormatDate(submission.SubmittedDate ?? submission.ModifiedDate),
                CreatedDate = submission.CreatedDate,
                ModifiedDate = submission.ModifiedDate,
                ReviewerName = submission.Reviewer?.FullName,
                ReviewedDate = submission.ReviewedDate,
                FormattedReviewedDate = submission.ReviewedDate.HasValue ? FormatDate(submission.ReviewedDate.Value) : null,
                ApprovalComments = submission.ApprovalComments,
                AnsweredCount = answeredCount,
                TotalFields = totalFields,
                CompletionPercentage = totalFields > 0 ? Math.Round((decimal)answeredCount / totalFields * 100, 1) : 0,
                Sections = sections,
                CommentsCount = 0, // TODO: Implement comments feature
                HasWorkflowProgress = false // TODO: Check workflow progress
            };
        }

        /// <inheritdoc />
        public async Task<Dictionary<int, TemplateSubmissionStats>> GetScopedTemplateStatsAsync(
            ClaimsPrincipal user,
            List<int> templateIds)
        {
            if (templateIds == null || !templateIds.Any())
            {
                return new Dictionary<int, TemplateSubmissionStats>();
            }

            // Get user scope
            var userScope = await _scopeService.GetUserScopeAsync(user);
            var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(user);

            // Build query with scope filtering
            var query = _context.FormTemplateSubmissions
                .Where(s => templateIds.Contains(s.TemplateId))
                .AsQueryable();

            // Apply scope filtering
            query = ApplyScopeFilter(query, userScope, accessibleTenantIds);

            // Get submissions and group by template
            var submissions = await query
                .Select(s => new { s.TemplateId, s.Status })
                .ToListAsync();

            var stats = submissions
                .GroupBy(s => s.TemplateId)
                .ToDictionary(
                    g => g.Key,
                    g => new TemplateSubmissionStats
                    {
                        TotalResponses = g.Count(),
                        SubmittedCount = g.Count(s => s.Status == "Submitted" || s.Status == "Approved" || s.Status == "InApproval"),
                        DraftCount = g.Count(s => s.Status == "Draft")
                    }
                );

            // Ensure all requested templates have an entry
            foreach (var templateId in templateIds)
            {
                if (!stats.ContainsKey(templateId))
                {
                    stats[templateId] = new TemplateSubmissionStats
                    {
                        TotalResponses = 0,
                        SubmittedCount = 0,
                        DraftCount = 0
                    };
                }
            }

            return stats;
        }

        /// <inheritdoc />
        public async Task<bool> CanUserAccessSubmissionAsync(int submissionId, ClaimsPrincipal user)
        {
            var submission = await _context.FormTemplateSubmissions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
            {
                return false;
            }

            var userScope = await _scopeService.GetUserScopeAsync(user);

            // INDIVIDUAL scope: only own submissions
            if (userScope.ScopeCode == "INDIVIDUAL")
            {
                return submission.SubmittedBy == userScope.UserId;
            }

            // GLOBAL scope: access to all
            if (userScope.ScopeCode == "GLOBAL")
            {
                return true;
            }

            // For other scopes, check tenant access
            if (submission.TenantId.HasValue)
            {
                return await _scopeService.HasAccessToTenantAsync(user, submission.TenantId.Value);
            }

            // Non-tenant submissions: check if user submitted it or has global/regional access
            return submission.SubmittedBy == userScope.UserId ||
                   userScope.ScopeCode == "REGIONAL" ||
                   userScope.ScopeCode == "GLOBAL";
        }

        // ========================================================================
        // PRIVATE HELPER METHODS FOR SCOPE-AWARE OPERATIONS
        // ========================================================================

        /// <summary>
        /// Apply scope-based filtering to a submissions query
        /// </summary>
        private IQueryable<FormTemplateSubmission> ApplyScopeFilter(
            IQueryable<FormTemplateSubmission> query,
            UserScope userScope,
            List<int> accessibleTenantIds)
        {
            switch (userScope.ScopeCode?.ToUpper())
            {
                case "GLOBAL":
                    // No filtering - see all submissions
                    break;

                case "INDIVIDUAL":
                    // Only own submissions
                    query = query.Where(s => s.SubmittedBy == userScope.UserId);
                    break;

                default:
                    // REGIONAL, TENANT, DEPARTMENT, TEAM - filter by accessible tenants
                    if (accessibleTenantIds.Any())
                    {
                        query = query.Where(s =>
                            s.TenantId == null || // Non-tenant submissions visible
                            accessibleTenantIds.Contains(s.TenantId.Value));
                    }
                    else
                    {
                        // No accessible tenants - only show own submissions
                        query = query.Where(s => s.SubmittedBy == userScope.UserId);
                    }
                    break;
            }

            return query;
        }

        /// <summary>
        /// Apply additional filters to submissions query
        /// </summary>
        private IQueryable<FormTemplateSubmission> ApplySubmissionFilters(
            IQueryable<FormTemplateSubmission> query,
            SubmissionFilters filters)
        {
            // Status filter
            if (!string.IsNullOrEmpty(filters.Status))
            {
                query = query.Where(s => s.Status == filters.Status);
            }

            // Tenant filter
            if (filters.TenantId.HasValue)
            {
                query = query.Where(s => s.TenantId == filters.TenantId.Value);
            }

            // Period filter
            if (!string.IsNullOrEmpty(filters.Period))
            {
                var now = DateTime.Now;
                switch (filters.Period.ToLower())
                {
                    case "thismonth":
                        query = query.Where(s => s.ReportingYear == now.Year && s.ReportingMonth == now.Month);
                        break;
                    case "lastmonth":
                        var lastMonth = now.AddMonths(-1);
                        query = query.Where(s => s.ReportingYear == lastMonth.Year && s.ReportingMonth == lastMonth.Month);
                        break;
                    case "thisquarter":
                        var quarterStart = new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1);
                        query = query.Where(s => s.ReportingPeriod >= quarterStart);
                        break;
                    case "thisyear":
                        query = query.Where(s => s.ReportingYear == now.Year);
                        break;
                }
            }

            // Custom date range
            if (filters.DateFrom.HasValue)
            {
                query = query.Where(s => s.ReportingPeriod >= filters.DateFrom.Value);
            }
            if (filters.DateTo.HasValue)
            {
                query = query.Where(s => s.ReportingPeriod <= filters.DateTo.Value);
            }

            // Submitter filter
            if (filters.SubmitterId.HasValue)
            {
                query = query.Where(s => s.SubmittedBy == filters.SubmitterId.Value);
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var searchLower = filters.Search.ToLower();
                query = query.Where(s =>
                    (s.Submitter != null && (
                        s.Submitter.Email.ToLower().Contains(searchLower) ||
                        (s.Submitter.FirstName + " " + s.Submitter.LastName).ToLower().Contains(searchLower)
                    )) ||
                    (s.Tenant != null && s.Tenant.TenantName.ToLower().Contains(searchLower))
                );
            }

            return query;
        }

        /// <summary>
        /// Build a submission row ViewModel
        /// </summary>
        private SubmissionRowViewModel BuildSubmissionRow(
            FormTemplateSubmission submission,
            List<FormFieldColumnInfo> fieldColumns,
            int totalFields,
            int rowNumber)
        {
            var responsesByItemId = submission.Responses.ToDictionary(r => r.ItemId);
            var answeredCount = submission.Responses.Count(r => HasResponseValue(r));

            var row = new SubmissionRowViewModel
            {
                SubmissionId = submission.SubmissionId,
                SubmissionNumber = rowNumber,
                SubmissionDate = submission.SubmittedDate ?? submission.ModifiedDate,
                FormattedDate = FormatDateRelative(submission.SubmittedDate ?? submission.ModifiedDate),
                SubmitterName = submission.Submitter?.FullName ?? "Unknown",
                SubmitterEmail = submission.Submitter?.Email ?? "",
                TenantName = submission.Tenant?.TenantName,
                TenantId = submission.TenantId,
                ReportingPeriod = FormatReportingPeriod(submission.ReportingYear, submission.ReportingMonth),
                Status = submission.Status,
                StatusBadgeClass = GetStatusBadgeClass(submission.Status),
                AnsweredCount = answeredCount,
                TotalFields = totalFields,
                CompletionPercentage = totalFields > 0 ? Math.Round((decimal)answeredCount / totalFields * 100, 1) : 0,
                IsFlagged = false, // TODO: Implement flagging
                ResponsePreviews = new List<ResponsePreview>()
            };

            // Build response previews for each field column
            foreach (var fieldColumn in fieldColumns)
            {
                responsesByItemId.TryGetValue(fieldColumn.ItemId, out var response);
                var preview = BuildResponsePreview(fieldColumn, response);
                row.ResponsePreviews.Add(preview);
            }

            return row;
        }

        /// <summary>
        /// Build a response preview for table display
        /// </summary>
        private ResponsePreview BuildResponsePreview(FormFieldColumnInfo fieldColumn, FormTemplateResponse? response)
        {
            var preview = new ResponsePreview
            {
                ItemId = fieldColumn.ItemId,
                DataType = fieldColumn.DataType,
                HasValue = response != null && HasResponseValue(response)
            };

            if (response == null || !preview.HasValue)
            {
                preview.DisplayValue = "—";
                return preview;
            }

            // Get options from fieldColumn, or fallback to response.Item.Options if available
            var options = fieldColumn.Options;
            if (!options.Any() && response.Item?.Options != null && response.Item.Options.Any())
            {
                options = response.Item.Options
                    .Where(o => o.IsActive)
                    .Select(o => new FieldOptionInfo
                    {
                        OptionId = o.OptionId,
                        OptionValue = o.OptionValue,
                        OptionLabel = o.OptionLabel
                    })
                    .ToList();
            }

            // Get DataType from fieldColumn, or fallback to response.Item.DataType
            var dataType = !string.IsNullOrEmpty(fieldColumn.DataType) 
                ? fieldColumn.DataType 
                : (response.Item?.DataType ?? "Text");

            // Format based on data type
            switch (dataType.ToLower())
            {
                case "rating":
                case "slider":
                    preview.NumericValue = response.NumericValue ?? response.SelectedScoreValue;
                    preview.MaxValue = 10; // Default max, could be from configuration
                    preview.DisplayValue = preview.NumericValue.HasValue
                        ? $"{preview.NumericValue:0.#}"
                        : "—";
                    break;

                case "number":
                case "decimal":
                    preview.NumericValue = response.NumericValue;
                    preview.DisplayValue = response.NumericValue.HasValue
                        ? response.NumericValue.Value.ToString("N2")
                        : "—";
                    break;

                case "currency":
                    preview.NumericValue = response.NumericValue;
                    preview.DisplayValue = response.NumericValue.HasValue
                        ? response.NumericValue.Value.ToString("C2")
                        : "—";
                    break;

                case "percentage":
                    preview.NumericValue = response.NumericValue;
                    preview.DisplayValue = response.NumericValue.HasValue
                        ? $"{response.NumericValue.Value:N2}%"
                        : "—";
                    break;

                case "date":
                    preview.DisplayValue = response.DateValue.HasValue
                        ? response.DateValue.Value.ToString("MMM dd, yyyy")
                        : "—";
                    break;

                case "datetime":
                    preview.DisplayValue = response.DateValue.HasValue
                        ? response.DateValue.Value.ToString("MMM dd, yyyy h:mm tt")
                        : "—";
                    break;

                case "time":
                    preview.DisplayValue = !string.IsNullOrEmpty(response.TextValue)
                        ? response.TextValue
                        : (response.DateValue.HasValue ? response.DateValue.Value.ToString("h:mm tt") : "—");
                    break;

                case "boolean":
                case "toggle":
                    preview.DisplayValue = response.BooleanValue == true ? "Yes" : "No";
                    break;

                case "checkbox":
                    // Single checkbox (boolean) vs checkbox with options
                    if (options.Any())
                    {
                        // Checkbox with options - treat like multiselect
                        goto case "checkboxgroup";
                    }
                    else
                    {
                        // Single boolean checkbox
                        preview.DisplayValue = response.BooleanValue == true ? "Yes" : "No";
                    }
                    break;

                case "dropdown":
                case "radio":
                case "select":
                case "singlechoice":
                case "single-choice":
                    // Use SelectedOption if loaded, otherwise lookup by SelectedOptionId or TextValue
                    if (response.SelectedOption != null)
                    {
                        preview.DisplayValue = response.SelectedOption.OptionLabel;
                    }
                    else if (response.SelectedOptionId.HasValue && options.Any())
                    {
                        var option = options.FirstOrDefault(o => o.OptionId == response.SelectedOptionId.Value);
                        preview.DisplayValue = option?.OptionLabel ?? response.TextValue ?? "—";
                    }
                    else if (!string.IsNullOrEmpty(response.TextValue) && options.Any())
                    {
                        // Lookup by OptionValue (the stored code)
                        var option = options.FirstOrDefault(o => 
                            o.OptionValue.Equals(response.TextValue, StringComparison.OrdinalIgnoreCase));
                        preview.DisplayValue = option?.OptionLabel ?? response.TextValue;
                    }
                    else
                    {
                        preview.DisplayValue = response.TextValue ?? "—";
                    }
                    break;

                case "multiselect":
                case "multi-select":
                case "multiplechoice":
                case "multiple-choice":
                case "checkboxgroup":
                case "checkbox-group":
                case "checkboxlist":
                case "checkbox-list":
                    // TextValue contains comma-separated option IDs or values
                    if (!string.IsNullOrEmpty(response.TextValue) && options.Any())
                    {
                        var selectedValues = response.TextValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(v => v.Trim())
                            .ToList();
                        
                        var labels = new List<string>();
                        foreach (var val in selectedValues)
                        {
                            // Try to parse as ID first
                            if (int.TryParse(val, out int optionId))
                            {
                                var option = options.FirstOrDefault(o => o.OptionId == optionId);
                                if (option != null)
                                {
                                    labels.Add(option.OptionLabel);
                                    continue;
                                }
                            }
                            // Otherwise lookup by OptionValue
                            var optionByValue = options.FirstOrDefault(o => 
                                o.OptionValue.Equals(val, StringComparison.OrdinalIgnoreCase));
                            labels.Add(optionByValue?.OptionLabel ?? val);
                        }
                        
                        var joined = string.Join(", ", labels);
                        preview.DisplayValue = joined.Length > 30 ? joined.Substring(0, 27) + "..." : joined;
                    }
                    else
                    {
                        preview.DisplayValue = response.TextValue ?? "—";
                    }
                    break;

                case "fileupload":
                case "file":
                case "image":
                    preview.DisplayValue = !string.IsNullOrEmpty(response.TextValue) ? "📎 View" : "—";
                    break;

                case "signature":
                    preview.DisplayValue = !string.IsNullOrEmpty(response.TextValue) ? "✓ Signed" : "—";
                    break;

                case "email":
                    preview.DisplayValue = response.TextValue ?? "—";
                    break;

                case "phone":
                case "tel":
                    preview.DisplayValue = response.TextValue ?? "—";
                    break;

                case "url":
                case "link":
                    var url = response.TextValue ?? "";
                    preview.DisplayValue = url.Length > 30 ? url.Substring(0, 27) + "..." : (url.Length > 0 ? url : "—");
                    break;

                case "textarea":
                case "longtext":
                case "richtext":
                    var longText = response.TextValue ?? "";
                    preview.DisplayValue = longText.Length > 50 
                        ? longText.Substring(0, 47) + "..." 
                        : (longText.Length > 0 ? longText : "—");
                    break;

                default:
                    // Check if this field has options - if so, treat as selection field
                    if (options.Any())
                    {
                        // Field has options but unknown DataType - try to resolve option label
                        if (response.SelectedOption != null)
                        {
                            preview.DisplayValue = response.SelectedOption.OptionLabel;
                        }
                        else if (response.SelectedOptionId.HasValue)
                        {
                            var option = options.FirstOrDefault(o => o.OptionId == response.SelectedOptionId.Value);
                            preview.DisplayValue = option?.OptionLabel ?? response.TextValue ?? "—";
                        }
                        else if (!string.IsNullOrEmpty(response.TextValue))
                        {
                            // Try to lookup by OptionValue
                            var option = options.FirstOrDefault(o => 
                                o.OptionValue.Equals(response.TextValue, StringComparison.OrdinalIgnoreCase));
                            preview.DisplayValue = option?.OptionLabel ?? response.TextValue;
                        }
                        else
                        {
                            preview.DisplayValue = "—";
                        }
                    }
                    else
                    {
                        // Text and any other types without options
                        var textValue = response.TextValue ?? "";
                        preview.DisplayValue = textValue.Length > 30
                            ? textValue.Substring(0, 27) + "..."
                            : textValue;
                        if (string.IsNullOrEmpty(preview.DisplayValue))
                            preview.DisplayValue = "—";
                    }
                    break;
            }

            return preview;
        }

        /// <summary>
        /// Build detailed response view for offcanvas
        /// </summary>
        private ResponseDetailViewModel BuildResponseDetail(FormTemplateItem item, FormTemplateResponse? response)
        {
            var detail = new ResponseDetailViewModel
            {
                ItemId = item.ItemId,
                FieldName = item.ItemName,
                FieldDescription = item.ItemDescription,
                DataType = item.DataType ?? "Text",
                IsRequired = item.IsRequired,
                DisplayOrder = item.DisplayOrder,
                HasValue = response != null && HasResponseValue(response)
            };

            if (response == null)
            {
                detail.DisplayValue = "No response";
                return detail;
            }

            // Store raw values
            detail.TextValue = response.TextValue;
            detail.NumericValue = response.NumericValue;
            detail.DateValue = response.DateValue;
            detail.BooleanValue = response.BooleanValue;
            detail.Remarks = response.Remarks;
            detail.RatingValue = response.SelectedScoreValue ?? response.NumericValue;
            detail.ScoreWeight = response.SelectedScoreWeight;
            detail.WeightedScore = response.WeightedScore;

            // Get selected option label if applicable
            if (response.SelectedOptionId.HasValue && item.Options != null)
            {
                var selectedOption = item.Options.FirstOrDefault(o => o.OptionId == response.SelectedOptionId);
                detail.SelectedOptionLabel = selectedOption?.OptionLabel;
            }

            // Format display value based on data type
            detail.DisplayValue = FormatResponseDisplayValue(item, response);

            // Handle rating max value
            if (item.DataType?.ToLower() == "rating")
            {
                detail.RatingMax = 5; // Default, could come from configuration
            }

            return detail;
        }

        /// <summary>
        /// Format response value for display
        /// </summary>
        private string FormatResponseDisplayValue(FormTemplateItem item, FormTemplateResponse response)
        {
            var dataType = item.DataType?.ToLower() ?? "text";

            switch (dataType)
            {
                case "rating":
                    var rating = response.SelectedScoreValue ?? response.NumericValue;
                    return rating.HasValue ? $"{rating:0.#} out of 5" : "No rating";

                case "slider":
                    return response.NumericValue.HasValue
                        ? response.NumericValue.Value.ToString("N1")
                        : "No value";

                case "number":
                case "decimal":
                    return response.NumericValue.HasValue
                        ? response.NumericValue.Value.ToString("N2")
                        : "No value";

                case "currency":
                    return response.NumericValue.HasValue
                        ? response.NumericValue.Value.ToString("C2")
                        : "No value";

                case "percentage":
                    return response.NumericValue.HasValue
                        ? response.NumericValue.Value.ToString("P1")
                        : "No value";

                case "date":
                    return response.DateValue.HasValue
                        ? response.DateValue.Value.ToString("MMMM dd, yyyy")
                        : "No date";

                case "datetime":
                    return response.DateValue.HasValue
                        ? response.DateValue.Value.ToString("MMMM dd, yyyy 'at' h:mm tt")
                        : "No date";

                case "time":
                    return response.DateValue.HasValue
                        ? response.DateValue.Value.ToString("h:mm tt")
                        : "No time";

                case "checkbox":
                case "boolean":
                    return response.BooleanValue == true ? "Yes" : "No";

                case "select":
                case "dropdown":
                case "radio":
                case "singlechoice":
                case "single-choice":
                    // Prefer selected option label, fall back to text value
                    if (response.SelectedOptionId.HasValue && item.Options != null)
                    {
                        var option = item.Options.FirstOrDefault(o => o.OptionId == response.SelectedOptionId);
                        if (option != null) return option.OptionLabel;
                    }
                    return response.TextValue ?? "No selection";

                case "multiselect":
                case "checkboxgroup":
                case "checkbox-group":
                case "multi-choice":
                case "multichoice":
                    // For multi-select, TextValue contains comma-separated option IDs or labels
                    if (!string.IsNullOrEmpty(response.TextValue) && item.Options != null && item.Options.Any())
                    {
                        // Try to parse as option IDs first
                        var selectedIds = response.TextValue.Split(',')
                            .Select(s => int.TryParse(s.Trim(), out var id) ? id : (int?)null)
                            .Where(id => id.HasValue)
                            .Select(id => id!.Value)
                            .ToList();
                        
                        if (selectedIds.Any())
                        {
                            var labels = item.Options
                                .Where(o => selectedIds.Contains(o.OptionId))
                                .Select(o => o.OptionLabel)
                                .ToList();
                            if (labels.Any()) return string.Join(", ", labels);
                        }
                    }
                    return response.TextValue ?? "No selections";

                case "fileupload":
                case "image":
                    return !string.IsNullOrEmpty(response.TextValue)
                        ? "File attached"
                        : "No file";

                case "signature":
                    return !string.IsNullOrEmpty(response.TextValue)
                        ? "Signature captured"
                        : "No signature";

                default:
                    return response.TextValue ?? "No response";
            }
        }

        /// <summary>
        /// Check if a response has a value
        /// </summary>
        private bool HasResponseValue(FormTemplateResponse response)
        {
            return !string.IsNullOrEmpty(response.TextValue) ||
                   response.NumericValue.HasValue ||
                   response.DateValue.HasValue ||
                   response.BooleanValue.HasValue ||
                   response.SelectedOptionId.HasValue;
        }

        /// <summary>
        /// Get available tenants for filter dropdown
        /// </summary>
        private async Task<List<TenantFilterOption>> GetAvailableTenantsForFilter(
            int templateId,
            List<int> accessibleTenantIds)
        {
            if (!accessibleTenantIds.Any())
            {
                return new List<TenantFilterOption>();
            }

            var tenantCounts = await _context.FormTemplateSubmissions
                .Where(s => s.TemplateId == templateId &&
                           s.TenantId.HasValue &&
                           accessibleTenantIds.Contains(s.TenantId.Value))
                .GroupBy(s => s.TenantId)
                .Select(g => new { TenantId = g.Key, Count = g.Count() })
                .ToListAsync();

            var tenantIds = tenantCounts.Select(t => t.TenantId!.Value).ToList();
            var tenants = await _context.Tenants
                .Where(t => tenantIds.Contains(t.TenantId))
                .ToListAsync();

            return tenants.Select(t => new TenantFilterOption
            {
                TenantId = t.TenantId,
                TenantName = t.TenantName,
                SubmissionCount = tenantCounts.FirstOrDefault(c => c.TenantId == t.TenantId)?.Count ?? 0
            })
            .OrderBy(t => t.TenantName)
            .ToList();
        }

        /// <summary>
        /// Get available submitters for filter dropdown
        /// </summary>
        private async Task<List<SubmitterFilterOption>> GetAvailableSubmittersForFilter(
            int templateId,
            List<int> accessibleTenantIds)
        {
            var query = _context.FormTemplateSubmissions
                .Where(s => s.TemplateId == templateId && s.SubmittedBy > 0);

            // Apply tenant scope if not global
            if (accessibleTenantIds.Any())
            {
                query = query.Where(s => !s.TenantId.HasValue || accessibleTenantIds.Contains(s.TenantId.Value));
            }

            var submitterCounts = await query
                .GroupBy(s => s.SubmittedBy)
                .Select(g => new { SubmitterId = g.Key, Count = g.Count() })
                .ToListAsync();

            var submitterIds = submitterCounts.Select(s => s.SubmitterId).ToList();
            var submitters = await _context.Users
                .Where(u => submitterIds.Contains(u.UserId))
                .ToListAsync();

            return submitters.Select(u => new SubmitterFilterOption
            {
                UserId = u.UserId,
                FullName = u.FullName ?? $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email,
                SubmissionCount = submitterCounts.FirstOrDefault(c => c.SubmitterId == u.UserId)?.Count ?? 0
            })
            .OrderBy(s => s.FullName)
            .ToList();
        }

        /// <summary>
        /// Build scope display text for UI
        /// </summary>
        private string BuildScopeDisplayText(UserScope userScope, int totalCount)
        {
            var scopeLabel = userScope.ScopeCode?.ToUpper() switch
            {
                "GLOBAL" => "All Submissions",
                "REGIONAL" => $"{userScope.ScopeName} Region Submissions",
                "TENANT" => "Your Location's Submissions",
                "DEPARTMENT" => "Your Department's Submissions",
                "TEAM" => "Your Team's Submissions",
                "INDIVIDUAL" => "Your Submissions Only",
                _ => "Submissions"
            };

            return $"Viewing: {scopeLabel} ({totalCount} total)";
        }

        /// <summary>
        /// Get CSS class for status badge
        /// </summary>
        private string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Draft" => "bg-warning",
                "Submitted" => "bg-primary",
                "InApproval" => "bg-info",
                "Approved" => "bg-success",
                "Rejected" => "bg-danger",
                "Cancelled" => "bg-secondary",
                _ => "bg-secondary"
            };
        }

        /// <summary>
        /// Format date for display
        /// </summary>
        private string FormatDate(DateTime date)
        {
            return date.ToString("MMM dd, yyyy 'at' h:mm tt");
        }

        /// <summary>
        /// Format date with relative display for recent dates
        /// </summary>
        private string FormatDateRelative(DateTime date)
        {
            var now = DateTime.Now;
            var diff = now - date;

            if (diff.TotalMinutes < 1)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24 && date.Date == now.Date)
                return date.ToString("h:mm tt");
            if (diff.TotalDays < 7)
                return date.ToString("ddd h:mm tt");

            return date.ToString("MMM dd");
        }

        /// <summary>
        /// Format reporting period for display
        /// </summary>
        private string FormatReportingPeriod(int year, int month)
        {
            var date = new DateTime(year, month, 1);
            return date.ToString("MMMM yyyy");
        }

        // ========================================================================
        // EXPORT METHODS
        // ========================================================================

        /// <inheritdoc />
        public async Task<SubmissionExportData> GetSubmissionsForExportAsync(
            int templateId,
            ClaimsPrincipal user,
            SubmissionFilters? filters = null)
        {
            var exportData = new SubmissionExportData();

            // Get user scope
            var userScope = await _scopeService.GetUserScopeAsync(user);
            var accessibleTenantIds = userScope.AccessibleTenantIds;

            // Get template with sections and items
            var template = await _context.FormTemplates
                .Include(t => t.Sections.OrderBy(s => s.DisplayOrder))
                    .ThenInclude(s => s.Items.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder))
                        .ThenInclude(i => i.Options.Where(o => o.IsActive).OrderBy(o => o.DisplayOrder))
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
            {
                return exportData;
            }

            exportData.TemplateName = template.TemplateName;

            // Build headers: Fixed columns + Field columns
            exportData.Headers.Add("#");
            exportData.Headers.Add("Submission Date");
            exportData.Headers.Add("Submitter");
            exportData.Headers.Add("Location");
            exportData.Headers.Add("Status");
            exportData.Headers.Add("Reporting Period");

            // Add field columns
            var fieldColumns = new List<FormFieldColumnInfo>();
            foreach (var section in template.Sections)
            {
                foreach (var item in section.Items)
                {
                    exportData.Headers.Add(item.ItemName);
                    fieldColumns.Add(new FormFieldColumnInfo
                    {
                        ItemId = item.ItemId,
                        FieldName = item.ItemName,
                        DataType = item.DataType ?? "Text",
                        Options = item.Options?
                            .Select(o => new FieldOptionInfo
                            {
                                OptionId = o.OptionId,
                                OptionValue = o.OptionValue,
                                OptionLabel = o.OptionLabel
                            })
                            .ToList() ?? new List<FieldOptionInfo>()
                    });
                }
            }

            // Build query for submissions
            var query = _context.FormTemplateSubmissions
                .Include(s => s.Submitter)
                .Include(s => s.Tenant)
                .Include(s => s.Responses)
                    .ThenInclude(r => r.SelectedOption)
                .Include(s => s.Responses)
                    .ThenInclude(r => r.Item)
                        .ThenInclude(i => i.Options)
                .Where(s => s.TemplateId == templateId)
                .AsQueryable();

            // Apply scope filter
            if (accessibleTenantIds.Any())
            {
                query = query.Where(s => !s.TenantId.HasValue || accessibleTenantIds.Contains(s.TenantId.Value));
            }

            // Apply additional filters
            if (filters != null)
            {
                query = ApplySubmissionFilters(query, filters);
            }

            // Get all submissions (no pagination for export)
            var submissions = await query
                .OrderByDescending(s => s.SubmittedDate ?? s.ModifiedDate)
                .ToListAsync();

            // Build rows
            int rowNumber = 1;
            foreach (var submission in submissions)
            {
                var row = new List<string>
                {
                    rowNumber.ToString(),
                    (submission.SubmittedDate ?? submission.ModifiedDate).ToString("yyyy-MM-dd HH:mm"),
                    submission.Submitter?.FullName ?? "Unknown",
                    submission.Tenant?.TenantName ?? "—",
                    submission.Status,
                    $"{submission.ReportingMonth:D2}/{submission.ReportingYear}"
                };

                // Add response values
                var responsesByItemId = submission.Responses.ToDictionary(r => r.ItemId);
                foreach (var fieldColumn in fieldColumns)
                {
                    responsesByItemId.TryGetValue(fieldColumn.ItemId, out var response);
                    var preview = BuildResponsePreview(fieldColumn, response);
                    row.Add(preview.DisplayValue);
                }

                exportData.Rows.Add(row);
                rowNumber++;
            }

            return exportData;
        }

        #region Dual-Mode Submission Access Validation Methods

        public async Task<bool> CanUserCreateSubmissionAsync(int userId, int templateId)
        {
            // Get template to check submission mode
            var template = await _context.FormTemplates.FindAsync(templateId);
            if (template?.SubmissionMode != Models.Common.SubmissionMode.Individual)
            {
                return false; // Only Individual mode uses user-based submission access
            }

            // Check template readiness
            var readiness = await ValidateSubmissionAccessAsync(userId, templateId);
            return readiness.CanCreateSubmission;
        }

        public async Task<bool> CanCreateCollaborativeSubmissionAsync(int templateId)
        {
            var template = await _context.FormTemplates.FindAsync(templateId);
            if (template?.SubmissionMode != Models.Common.SubmissionMode.Collaborative)
            {
                return false; // Only Collaborative mode supports system-initiated submissions
            }

            // Check if template is ready for collaborative workflow
            return await _templateService.IsReadyForCollaborativeWorkflowAsync(templateId);
        }

        public async Task<SubmissionAccessValidationDto> ValidateSubmissionAccessAsync(int userId, int templateId)
        {
            var result = new SubmissionAccessValidationDto();

            var template = await _context.FormTemplates.FindAsync(templateId);
            if (template == null)
            {
                result.BlockingIssues.Add("Template not found");
                return result;
            }

            result.SubmissionMode = template.SubmissionMode.ToString();
            result.TemplateStatus = template.PublishStatus;

            // Check basic template readiness
            var templateReadiness = await _templateService.ValidateTemplateReadinessAsync(templateId);
            result.TemplateReady = templateReadiness.IsReady;
            result.WorkflowConfigured = template.WorkflowId.HasValue;

            if (!templateReadiness.IsReady)
            {
                result.BlockingIssues.AddRange(templateReadiness.BlockingIssues);
                result.Warnings.AddRange(templateReadiness.Warnings);
                result.CanCreateSubmission = false;
                return result;
            }

            // Mode-specific validation
            if (template.SubmissionMode == Models.Common.SubmissionMode.Individual)
            {
                // Anonymous access bypasses assignment checks
                if (template.AllowAnonymousAccess)
                {
                    result.HasAssignmentAccess = true;
                    result.CanCreateSubmission = true;
                    result.Warnings.Add("Anonymous access enabled - no assignment validation required");
                }
                else
                {
                    // Check assignment access for authenticated Individual mode
                    result.HasAssignmentAccess = await _assignmentService.CanUserCreateSubmissionAsync(templateId, userId);
                    
                    if (!result.HasAssignmentAccess)
                    {
                        result.BlockingIssues.Add("You do not have assignment access to create submissions for this template");
                    }

                    result.CanCreateSubmission = result.HasAssignmentAccess;
                }
            }
            else if (template.SubmissionMode == Models.Common.SubmissionMode.Collaborative)
            {
                // Collaborative mode is system-initiated, individual users don't create submissions
                result.HasAssignmentAccess = false;
                result.BlockingIssues.Add("Collaborative mode submissions are system-initiated, not user-created");
                result.CanCreateSubmission = false;
            }

            return result;
        }

        public async Task<List<TemplateSubmissionAccessDto>> GetTemplatesWithSubmissionAccessAsync(int userId)
        {
            var result = new List<TemplateSubmissionAccessDto>();

            // Get all published Individual mode templates
            var templates = await _context.FormTemplates
                .Include(t => t.Category)
                .Include(t => t.Assignments.Where(a => a.Status == "Active"))
                .Include(t => t.Workflow)
                .Where(t => t.PublishStatus == "Published" && 
                           t.SubmissionMode == Models.Common.SubmissionMode.Individual)
                .ToListAsync();

            // Use injected services

            foreach (var template in templates)
            {
                var accessDto = new TemplateSubmissionAccessDto
                {
                    TemplateId = template.TemplateId,
                    TemplateName = template.TemplateName,
                    TemplateCode = template.TemplateCode,
                    SubmissionMode = template.SubmissionMode.ToString(),
                    CategoryName = template.Category?.CategoryName ?? "Uncategorized",
                    ActiveAssignmentCount = template.Assignments.Count,
                    HasWorkflow = template.WorkflowId.HasValue
                };

                // Check if user can create submission
                accessDto.CanCreateSubmission = await _assignmentService.CanUserCreateSubmissionAsync(template.TemplateId, userId);
                
                // Check template readiness
                accessDto.IsReady = await _templateService.CanAcceptSubmissionsAsync(template.TemplateId);

                // Add access issues if any
                if (!accessDto.CanCreateSubmission)
                {
                    accessDto.AccessIssues.Add("No assignment access");
                }
                if (!accessDto.IsReady)
                {
                    accessDto.AccessIssues.Add("Template not ready");
                }

                result.Add(accessDto);
            }

            return result.OrderBy(t => t.TemplateName).ToList();
        }

        public async Task<List<TemplateSubmissionAccessDto>> GetTemplatesReadyForCollaborativeWorkflowAsync()
        {
            var result = new List<TemplateSubmissionAccessDto>();

            // Get all published Collaborative mode templates
            var templates = await _context.FormTemplates
                .Include(t => t.Category)
                .Include(t => t.Assignments.Where(a => a.Status == "Active"))
                .Include(t => t.Workflow)
                    .ThenInclude(w => w.Steps)
                .Where(t => t.PublishStatus == "Published" && 
                           t.SubmissionMode == Models.Common.SubmissionMode.Collaborative)
                .ToListAsync();

            // Use injected template service

            foreach (var template in templates)
            {
                var accessDto = new TemplateSubmissionAccessDto
                {
                    TemplateId = template.TemplateId,
                    TemplateName = template.TemplateName,
                    TemplateCode = template.TemplateCode,
                    SubmissionMode = template.SubmissionMode.ToString(),
                    CategoryName = template.Category?.CategoryName ?? "Uncategorized",
                    ActiveAssignmentCount = template.Assignments.Count,
                    HasWorkflow = template.WorkflowId.HasValue
                };

                // Check if template is ready for collaborative workflow
                accessDto.IsReady = await _templateService.IsReadyForCollaborativeWorkflowAsync(template.TemplateId);
                accessDto.CanCreateSubmission = accessDto.IsReady; // System can create if ready

                // Add access issues if any
                if (!accessDto.IsReady)
                {
                    var readiness = await _templateService.ValidateTemplateReadinessAsync(template.TemplateId);
                    accessDto.AccessIssues.AddRange(readiness.BlockingIssues);
                }

                result.Add(accessDto);
            }

            return result.OrderBy(t => t.TemplateName).ToList();
        }

        // Services are now properly injected via constructor

        #endregion
    }
}

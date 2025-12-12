using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Components;
using FormReporting.Models.Common;
using System.Text.Json;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service implementation for form submission operations
    /// Transforms database entities to ViewModels for rendering
    /// </summary>
    public class FormSubmissionService : IFormSubmissionService
    {
        private readonly ApplicationDbContext _context;

        public FormSubmissionService(ApplicationDbContext context)
        {
            _context = context;
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
    }
}

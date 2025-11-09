using FormReporting.Models.Common;
using FormReporting.Models.ViewModels.Components;
using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FormReporting.Extensions
{
    /// <summary>
    /// Extension methods for building forms from configuration objects
    /// Transforms FormConfig → FormViewModel for rendering
    /// </summary>
    public static class FormExtensions
    {
        // ========== MAIN TRANSFORMATION METHOD ==========

        /// <summary>
        /// Main transformation: builds complete FormViewModel from config
        /// </summary>
        public static FormViewModel BuildForm(this FormConfig config)
        {
            var viewModel = new FormViewModel
            {
                FormId = config.FormId,
                Title = config.Title,
                Description = config.Description,
                SubmitUrl = config.SubmitUrl,
                SaveDraftUrl = config.SaveDraftUrl,
                EnableAutoSave = config.EnableAutoSave,
                AutoSaveIntervalSeconds = config.AutoSaveIntervalSeconds,
                ShowProgressBar = config.ShowProgressBar,
                CssClass = config.CssClass,
                SubmitButtonText = config.SubmitButtonText,
                SaveDraftButtonText = config.SaveDraftButtonText,
                ShowResetButton = config.ShowResetButton,
                ShowCancelButton = config.ShowCancelButton,
                // Wizard mode properties
                RenderMode = config.RenderMode,
                ShowStepNumbers = config.ShowStepNumbers,
                AllowStepSkipping = config.AllowStepSkipping,
                ValidateOnStepChange = config.ValidateOnStepChange,
                PreviousButtonText = config.PreviousButtonText,
                NextButtonText = config.NextButtonText
            };

            // Transform sections
            viewModel.Sections = config.Sections
                .OrderBy(s => s.DisplayOrder)
                .Select(s => TransformSection(s, config.FormId))
                .ToList();

            // Calculate totals
            viewModel.TotalFields = viewModel.Sections
                .SelectMany(s => s.Fields)
                .Count();

            viewModel.RequiredFields = viewModel.Sections
                .SelectMany(s => s.Fields)
                .Count(f => f.IsRequired);

            return viewModel;
        }

        // ========== FLUENT API METHODS ==========

        /// <summary>
        /// Fluent API: Add section to form
        /// </summary>
        public static FormConfig WithSection(
            this FormConfig config,
            string sectionName,
            string? sectionDescription = null,
            Action<FormSectionConfig>? configureSection = null)
        {
            var section = new FormSectionConfig
            {
                SectionId = config.Sections.Count + 1,
                SectionName = sectionName,
                SectionDescription = sectionDescription,
                DisplayOrder = config.Sections.Count + 1
            };

            configureSection?.Invoke(section);
            config.Sections.Add(section);
            return config;
        }

        /// <summary>
        /// Fluent API: Set section as collapsible
        /// </summary>
        public static FormSectionConfig AsCollapsible(
            this FormSectionConfig section,
            bool startCollapsed = false)
        {
            section.IsCollapsible = true;
            section.IsCollapsedByDefault = startCollapsed;
            return section;
        }

        /// <summary>
        /// Fluent API: Set section icon
        /// </summary>
        public static FormSectionConfig WithIcon(
            this FormSectionConfig section,
            string iconClass)
        {
            section.IconClass = iconClass;
            return section;
        }

        /// <summary>
        /// Fluent API: Add field to section
        /// </summary>
        public static FormSectionConfig WithField(
            this FormSectionConfig section,
            FormFieldConfig field)
        {
            field.DisplayOrder = section.Fields.Count + 1;
            section.Fields.Add(field);
            return section;
        }

        /// <summary>
        /// Fluent API: Add text field
        /// </summary>
        public static FormSectionConfig WithTextField(
            this FormSectionConfig section,
            string fieldId,
            string fieldName,
            bool isRequired = false,
            string? placeholder = null)
        {
            return section.WithField(new FormFieldConfig
            {
                FieldId = fieldId,
                FieldName = fieldName,
                FieldType = FormFieldType.Text,
                IsRequired = isRequired,
                PlaceholderText = placeholder
            });
        }

        /// <summary>
        /// Fluent API: Add number field
        /// </summary>
        public static FormSectionConfig WithNumberField(
            this FormSectionConfig section,
            string fieldId,
            string fieldName,
            bool isRequired = false,
            decimal? minValue = null,
            decimal? maxValue = null)
        {
            var field = new FormFieldConfig
            {
                FieldId = fieldId,
                FieldName = fieldName,
                FieldType = FormFieldType.Number,
                IsRequired = isRequired
            };

            if (minValue.HasValue || maxValue.HasValue)
            {
                field.Validations.Add(new FormFieldValidation
                {
                    ValidationType = "Range",
                    MinValue = minValue,
                    MaxValue = maxValue,
                    ErrorMessage = $"Value must be between {minValue} and {maxValue}"
                });
            }

            return section.WithField(field);
        }

        /// <summary>
        /// Fluent API: Add dropdown field
        /// </summary>
        public static FormSectionConfig WithDropdownField(
            this FormSectionConfig section,
            string fieldId,
            string fieldName,
            List<FormFieldOption> options,
            bool isRequired = false)
        {
            return section.WithField(new FormFieldConfig
            {
                FieldId = fieldId,
                FieldName = fieldName,
                FieldType = FormFieldType.Dropdown,
                IsRequired = isRequired,
                Options = options
            });
        }

        /// <summary>
        /// Fluent API: Add text area field
        /// </summary>
        public static FormSectionConfig WithTextAreaField(
            this FormSectionConfig section,
            string fieldId,
            string fieldName,
            bool isRequired = false,
            string? placeholder = null)
        {
            return section.WithField(new FormFieldConfig
            {
                FieldId = fieldId,
                FieldName = fieldName,
                FieldType = FormFieldType.TextArea,
                IsRequired = isRequired,
                PlaceholderText = placeholder
            });
        }

        /// <summary>
        /// Fluent API: Add date field
        /// </summary>
        public static FormSectionConfig WithDateField(
            this FormSectionConfig section,
            string fieldId,
            string fieldName,
            bool isRequired = false)
        {
            return section.WithField(new FormFieldConfig
            {
                FieldId = fieldId,
                FieldName = fieldName,
                FieldType = FormFieldType.Date,
                IsRequired = isRequired
            });
        }

        /// <summary>
        /// Fluent API: Add date-time field
        /// </summary>
        public static FormSectionConfig WithDateTimeField(
            this FormSectionConfig section,
            string fieldId,
            string fieldName,
            bool isRequired = false)
        {
            return section.WithField(new FormFieldConfig
            {
                FieldId = fieldId,
                FieldName = fieldName,
                FieldType = FormFieldType.DateTime,
                IsRequired = isRequired
            });
        }

        /// <summary>
        /// Fluent API: Add radio button field
        /// </summary>
        public static FormSectionConfig WithRadioField(
            this FormSectionConfig section,
            string fieldId,
            string fieldName,
            List<FormFieldOption> options,
            bool isRequired = false)
        {
            return section.WithField(new FormFieldConfig
            {
                FieldId = fieldId,
                FieldName = fieldName,
                FieldType = FormFieldType.Radio,
                IsRequired = isRequired,
                Options = options
            });
        }

        /// <summary>
        /// Fluent API: Add checkbox field
        /// </summary>
        public static FormSectionConfig WithCheckboxField(
            this FormSectionConfig section,
            string fieldId,
            string fieldName,
            List<FormFieldOption> options,
            bool isRequired = false)
        {
            return section.WithField(new FormFieldConfig
            {
                FieldId = fieldId,
                FieldName = fieldName,
                FieldType = FormFieldType.Checkbox,
                IsRequired = isRequired,
                Options = options
            });
        }

        /// <summary>
        /// Fluent API: Add multi-select field
        /// </summary>
        public static FormSectionConfig WithMultiSelectField(
            this FormSectionConfig section,
            string fieldId,
            string fieldName,
            List<FormFieldOption> options,
            bool isRequired = false)
        {
            return section.WithField(new FormFieldConfig
            {
                FieldId = fieldId,
                FieldName = fieldName,
                FieldType = FormFieldType.MultiSelect,
                IsRequired = isRequired,
                Options = options
            });
        }

        /// <summary>
        /// Fluent API: Add email field
        /// </summary>
        public static FormSectionConfig WithEmailField(
            this FormSectionConfig section,
            string fieldId,
            string fieldName,
            bool isRequired = false,
            string? placeholder = null)
        {
            return section.WithField(new FormFieldConfig
            {
                FieldId = fieldId,
                FieldName = fieldName,
                FieldType = FormFieldType.Email,
                IsRequired = isRequired,
                PlaceholderText = placeholder
            });
        }

        /// <summary>
        /// Fluent API: Add phone field
        /// </summary>
        public static FormSectionConfig WithPhoneField(
            this FormSectionConfig section,
            string fieldId,
            string fieldName,
            bool isRequired = false,
            string? placeholder = null)
        {
            return section.WithField(new FormFieldConfig
            {
                FieldId = fieldId,
                FieldName = fieldName,
                FieldType = FormFieldType.Phone,
                IsRequired = isRequired,
                PlaceholderText = placeholder
            });
        }

        /// <summary>
        /// Fluent API: Add validation to field
        /// </summary>
        public static FormFieldConfig WithValidation(
            this FormFieldConfig field,
            string validationType,
            string errorMessage,
            decimal? minValue = null,
            decimal? maxValue = null,
            int? minLength = null,
            int? maxLength = null,
            string? regexPattern = null)
        {
            field.Validations.Add(new FormFieldValidation
            {
                ValidationType = validationType,
                ErrorMessage = errorMessage,
                MinValue = minValue,
                MaxValue = maxValue,
                MinLength = minLength,
                MaxLength = maxLength,
                RegexPattern = regexPattern,
                ValidationOrder = field.Validations.Count + 1
            });
            return field;
        }

        /// <summary>
        /// Fluent API: Add prefix/suffix to field
        /// </summary>
        public static FormFieldConfig WithAffixes(
            this FormFieldConfig field,
            string? prefix = null,
            string? suffix = null)
        {
            field.PrefixText = prefix;
            field.SuffixText = suffix;
            return field;
        }

        /// <summary>
        /// Fluent API: Add help text to field
        /// </summary>
        public static FormFieldConfig WithHelpText(
            this FormFieldConfig field,
            string helpText)
        {
            field.HelpText = helpText;
            return field;
        }

        /// <summary>
        /// Fluent API: Set field as read-only
        /// </summary>
        public static FormFieldConfig AsReadOnly(this FormFieldConfig field)
        {
            field.IsReadOnly = true;
            return field;
        }

        /// <summary>
        /// Fluent API: Set field column width (Bootstrap grid)
        /// </summary>
        public static FormFieldConfig WithColumnWidth(
            this FormFieldConfig field,
            int width)
        {
            field.ColumnWidth = Math.Max(1, Math.Min(12, width));
            return field;
        }

        // ========== DATABASE LOADING METHOD ==========

        /// <summary>
        /// Load form configuration from database template
        /// </summary>
        public static async Task<FormConfig> LoadFromTemplateAsync(
            int templateId,
            DbContext context)
        {
            // Query database with includes
            var template = await context.Set<FormTemplate>()
                .Include("Sections")
                .Include("Sections.Items")
                .Include("Sections.Items.Validations")
                .Include("Sections.Items.Options")
                .Include("Sections.Items.Configurations")
                .Where(t => t.TemplateId == templateId && t.IsActive)
                .FirstOrDefaultAsync();

            if (template == null)
                throw new InvalidOperationException($"Form template {templateId} not found or is inactive");

            var formConfig = new FormConfig
            {
                FormId = $"form_{templateId}",
                Title = template.TemplateName,
                Description = template.Description
            };

            // Get sections using reflection to access navigation property
            var sectionsProperty = template.GetType().GetProperty("Sections");
            var sections = sectionsProperty?.GetValue(template) as IEnumerable<FormTemplateSection> ?? new List<FormTemplateSection>();

            foreach (var section in sections.OrderBy(s => s.DisplayOrder))
            {
                var sectionConfig = new FormSectionConfig
                {
                    SectionId = section.SectionId,
                    SectionName = section.SectionName,
                    SectionDescription = section.SectionDescription,
                    IconClass = section.IconClass,
                    DisplayOrder = section.DisplayOrder,
                    IsCollapsible = section.IsCollapsible,
                    IsCollapsedByDefault = section.IsCollapsedByDefault
                };

                // Get items using reflection
                var itemsProperty = section.GetType().GetProperty("Items");
                var items = itemsProperty?.GetValue(section) as IEnumerable<FormTemplateItem> ?? new List<FormTemplateItem>();

                foreach (var item in items.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder))
                {
                    var fieldConfig = new FormFieldConfig
                    {
                        FieldId = item.ItemId.ToString(),
                        FieldName = item.ItemName,
                        FieldDescription = item.ItemDescription,
                        FieldType = ParseFieldType(item.DataType),
                        IsRequired = item.IsRequired,
                        DefaultValue = item.DefaultValue,
                        PlaceholderText = item.PlaceholderText,
                        HelpText = item.HelpText,
                        PrefixText = item.PrefixText,
                        SuffixText = item.SuffixText,
                        LayoutType = ParseLayoutType(item.LayoutType),
                        MatrixGroupId = item.MatrixGroupId,
                        MatrixRowLabel = item.MatrixRowLabel,
                        DisplayOrder = item.DisplayOrder
                    };

                    // Load options
                    var optionsProperty = item.GetType().GetProperty("Options");
                    var options = optionsProperty?.GetValue(item) as IEnumerable<FormItemOption> ?? new List<FormItemOption>();

                    fieldConfig.Options = options
                        .Where(o => o.IsActive)
                        .OrderBy(o => o.DisplayOrder)
                        .Select(o => new FormFieldOption
                        {
                            Value = o.OptionValue,
                            Label = o.OptionLabel,
                            IsDefault = o.IsDefault,
                            ParentOptionId = o.ParentOptionId,
                            DisplayOrder = o.DisplayOrder
                        }).ToList();

                    // Load validations
                    var validationsProperty = item.GetType().GetProperty("Validations");
                    var validations = validationsProperty?.GetValue(item) as IEnumerable<FormItemValidation> ?? new List<FormItemValidation>();

                    fieldConfig.Validations = validations
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
                        }).ToList();

                    // Parse conditional logic JSON
                    if (!string.IsNullOrEmpty(item.ConditionalLogic))
                    {
                        try
                        {
                            fieldConfig.ConditionalLogic = JsonSerializer.Deserialize<ConditionalLogic>(
                                item.ConditionalLogic,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        catch (JsonException)
                        {
                            // Invalid JSON, skip conditional logic
                        }
                    }

                    sectionConfig.Fields.Add(fieldConfig);
                }

                formConfig.Sections.Add(sectionConfig);
            }

            return formConfig;
        }

        // ========== VALIDATION METHOD ==========

        /// <summary>
        /// Validate form submission data
        /// </summary>
        public static FormSubmissionResult ValidateFormSubmission(
            this FormConfig config,
            Dictionary<string, object?> formData)
        {
            var result = new FormSubmissionResult
            {
                SubmittedData = formData
            };

            foreach (var section in config.Sections)
            {
                foreach (var field in section.Fields)
                {
                    var fieldErrors = new List<string>();
                    var value = formData.GetValueOrDefault($"field_{field.FieldId}");

                    foreach (var validation in field.Validations.OrderBy(v => v.ValidationOrder))
                    {
                        var isValid = validation.ValidationType switch
                        {
                            "Required" => !string.IsNullOrWhiteSpace(value?.ToString()),
                            "Email" => IsValidEmail(value?.ToString()),
                            "Phone" => IsValidPhone(value?.ToString()),
                            "Url" => IsValidUrl(value?.ToString()),
                            "MinLength" => value?.ToString()?.Length >= validation.MinLength,
                            "MaxLength" => value?.ToString()?.Length <= validation.MaxLength,
                            "MinValue" => decimal.TryParse(value?.ToString(), out var d) && d >= validation.MinValue,
                            "MaxValue" => decimal.TryParse(value?.ToString(), out var d2) && d2 <= validation.MaxValue,
                            "Range" => decimal.TryParse(value?.ToString(), out var d3) &&
                                      d3 >= validation.MinValue && d3 <= validation.MaxValue,
                            "Regex" => !string.IsNullOrEmpty(validation.RegexPattern) &&
                                      Regex.IsMatch(value?.ToString() ?? "", validation.RegexPattern),
                            _ => true
                        };

                        if (!isValid)
                        {
                            fieldErrors.Add(validation.ErrorMessage);
                            if (validation.Severity == "Error")
                            {
                                break; // Stop on first error
                            }
                        }
                    }

                    if (fieldErrors.Any())
                    {
                        result.Errors[$"field_{field.FieldId}"] = fieldErrors;
                    }
                }
            }

            result.IsValid = !result.Errors.Any();
            return result;
        }

        // ========== PRIVATE HELPER METHODS ==========

        /// <summary>
        /// Transform section config to view model
        /// </summary>
        private static FormSectionViewModel TransformSection(FormSectionConfig section, string formId)
        {
            var viewModel = new FormSectionViewModel
            {
                SectionId = section.SectionId,
                SectionName = section.SectionName,
                SectionDescription = section.SectionDescription,
                IconClass = section.IconClass,
                DisplayOrder = section.DisplayOrder,
                IsCollapsible = section.IsCollapsible,
                IsCollapsedByDefault = section.IsCollapsedByDefault,
                CollapseId = $"{formId}_section_{section.SectionId}"
            };

            // Transform fields
            var transformedFields = section.Fields
                .OrderBy(f => f.DisplayOrder)
                .Select(f => TransformField(f, formId))
                .ToList();

            viewModel.Fields = transformedFields;

            // Group fields by column width for multi-column layouts
            viewModel.FieldRows = GroupFieldsIntoRows(transformedFields);

            return viewModel;
        }

        /// <summary>
        /// Transform field config to view model
        /// </summary>
        private static FormFieldViewModel TransformField(FormFieldConfig field, string formId)
        {
            var viewModel = new FormFieldViewModel
            {
                FieldId = field.FieldId,
                FieldName = field.FieldName,
                FieldDescription = field.FieldDescription,
                FieldType = field.FieldType,
                IsRequired = field.IsRequired,
                DefaultValue = field.DefaultValue,
                PlaceholderText = field.PlaceholderText,
                HelpText = field.HelpText,
                PrefixText = field.PrefixText,
                SuffixText = field.SuffixText,
                Options = field.Options,
                Validations = field.Validations,
                ConditionalLogic = field.ConditionalLogic,
                LayoutType = field.LayoutType,
                MatrixGroupId = field.MatrixGroupId,
                MatrixRowLabel = field.MatrixRowLabel,
                CurrentValue = field.CurrentValue,
                IsReadOnly = field.IsReadOnly,
                IsDisabled = field.IsDisabled,
                CssClass = field.CssClass,
                ColumnWidth = field.ColumnWidth,

                // Generate HTML attributes
                InputName = $"field_{field.FieldId}",
                InputId = $"{formId}_field_{field.FieldId}"
            };

            // Serialize validations for client-side
            if (field.Validations.Any())
            {
                viewModel.ValidationDataAttribute = JsonSerializer.Serialize(field.Validations);
            }

            // Serialize conditional logic for client-side
            if (field.ConditionalLogic != null)
            {
                viewModel.ConditionalLogicDataAttribute = JsonSerializer.Serialize(field.ConditionalLogic);
            }

            return viewModel;
        }

        /// <summary>
        /// Group fields into rows based on column widths (Bootstrap 12-column grid)
        /// </summary>
        private static List<List<FormFieldViewModel>> GroupFieldsIntoRows(List<FormFieldViewModel> fields)
        {
            var rows = new List<List<FormFieldViewModel>>();
            var currentRow = new List<FormFieldViewModel>();
            var currentRowWidth = 0;

            foreach (var field in fields)
            {
                // If adding this field would exceed 12 columns, start new row
                if (currentRowWidth + field.ColumnWidth > 12 && currentRow.Any())
                {
                    rows.Add(currentRow);
                    currentRow = new List<FormFieldViewModel>();
                    currentRowWidth = 0;
                }

                currentRow.Add(field);
                currentRowWidth += field.ColumnWidth;

                // If we've filled exactly 12 columns, start new row
                if (currentRowWidth == 12)
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
        /// Parse field type string to enum
        /// </summary>
        private static FormFieldType ParseFieldType(string? dataType)
        {
            if (string.IsNullOrEmpty(dataType))
                return FormFieldType.Text;

            return Enum.TryParse<FormFieldType>(dataType, true, out var result)
                ? result
                : FormFieldType.Text;
        }

        /// <summary>
        /// Parse layout type string to enum
        /// </summary>
        private static FieldLayoutType ParseLayoutType(string? layoutType)
        {
            if (string.IsNullOrEmpty(layoutType))
                return FieldLayoutType.Single;

            return Enum.TryParse<FieldLayoutType>(layoutType, true, out var result)
                ? result
                : FieldLayoutType.Single;
        }

        /// <summary>
        /// Validate email format
        /// </summary>
        private static bool IsValidEmail(string? email)
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

        /// <summary>
        /// Validate phone format (basic)
        /// </summary>
        private static bool IsValidPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove common separators
            var cleaned = Regex.Replace(phone, @"[\s\-\(\)\+]", "");

            // Check if it's all digits and has reasonable length
            return Regex.IsMatch(cleaned, @"^\d{7,15}$");
        }

        /// <summary>
        /// Validate URL format
        /// </summary>
        private static bool IsValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

    }
}

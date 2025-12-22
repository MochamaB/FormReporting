using FormReporting.Data;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.Common;
using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service for Form Builder operations
    /// </summary>
    public class FormBuilderService : IFormBuilderService
    {
        private readonly ApplicationDbContext _context;

        public FormBuilderService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Load template data for form builder
        /// </summary>
        public async Task<FormBuilderViewModel?> LoadForBuilderAsync(int templateId)
        {
            var template = await _context.FormTemplates
                .Include(t => t.Category)
                .Include(t => t.Sections.OrderBy(s => s.DisplayOrder))
                    .ThenInclude(s => s.Items.OrderBy(i => i.DisplayOrder))
                        .ThenInclude(i => i.Validations)
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Items)
                        .ThenInclude(i => i.Options)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
                return null;

            var viewModel = new FormBuilderViewModel
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                TemplateCode = template.TemplateCode,
                Version = template.Version,
                PublishStatus = template.PublishStatus,
                CategoryId = template.CategoryId,
                CategoryName = template.Category?.CategoryName,
                Description = template.Description,
                Sections = template.Sections.Select(MapToSectionDto).ToList(),
                AvailableFieldTypes = GetAvailableFieldTypes()
            };

            return viewModel;
        }

        /// <summary>
        /// Map section entity to DTO
        /// </summary>
        private SectionDto MapToSectionDto(Models.Entities.Forms.FormTemplateSection section)
        {
            return new SectionDto
            {
                SectionId = section.SectionId,
                SectionName = section.SectionName,
                SectionDescription = section.SectionDescription,
                DisplayOrder = section.DisplayOrder,
                IsCollapsible = section.IsCollapsible,
                IsCollapsedByDefault = section.IsCollapsedByDefault,
                IconClass = section.IconClass,
                ColumnLayout = section.ColumnLayout,
                FieldCount = section.Items?.Count ?? 0,
                Fields = section.Items?.Select(MapToFieldDto).ToList() ?? new List<FieldDto>()
            };
        }

        /// <summary>
        /// Map field entity to DTO
        /// </summary>
        private FieldDto MapToFieldDto(Models.Entities.Forms.FormTemplateItem item)
        {
            // Parse DataType string to enum (database stores as string)
            var dataType = FormFieldType.Text; // Default
            if (!string.IsNullOrEmpty(item.DataType) && Enum.TryParse<FormFieldType>(item.DataType, out var parsedType))
            {
                dataType = parsedType;
            }

            // Map options to DTO
            var optionsDto = item.Options?
                .OrderBy(o => o.DisplayOrder)
                .Select(o => new FieldOptionDto
                {
                    OptionId = o.OptionId,
                    OptionLabel = o.OptionLabel,
                    OptionValue = o.OptionValue,
                    DisplayOrder = o.DisplayOrder,
                    IsDefault = o.IsDefault
                })
                .ToList() ?? new List<FieldOptionDto>();

            // Load configuration values from key-value store
            var configs = item.Configurations?.ToDictionary(c => c.ConfigKey, c => c.ConfigValue) ?? new Dictionary<string, string?>();

            return new FieldDto
            {
                ItemId = item.ItemId,
                TemplateId = item.TemplateId,
                SectionId = item.SectionId,
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                ItemDescription = item.ItemDescription,
                DataType = dataType,
                IsRequired = item.IsRequired,
                DisplayOrder = item.DisplayOrder,
                PlaceholderText = item.PlaceholderText,
                HelpText = item.HelpText,
                PrefixText = item.PrefixText,
                SuffixText = item.SuffixText,
                DefaultValue = item.DefaultValue,
                ConditionalLogic = item.ConditionalLogic,
                ValidationCount = item.Validations?.Count ?? 0,
                OptionCount = item.Options?.Count ?? 0,
                Options = optionsDto,  // Include full option details

                // Field-specific configurations (from key-value store)
                // Number/Text configurations
                MinValue = configs.ContainsKey("minValue") && decimal.TryParse(configs["minValue"], out var minVal) ? minVal : null,
                MaxValue = configs.ContainsKey("maxValue") && decimal.TryParse(configs["maxValue"], out var maxVal) ? maxVal : null,
                Step = configs.ContainsKey("step") && decimal.TryParse(configs["step"], out var stepVal) ? stepVal : null,
                DecimalPlaces = configs.ContainsKey("decimalPlaces") && int.TryParse(configs["decimalPlaces"], out var decPlaces) ? decPlaces : null,
                AllowNegative = configs.ContainsKey("allowNegative") && bool.TryParse(configs["allowNegative"], out var allowNeg) ? allowNeg : null,
                MinLength = configs.ContainsKey("minLength") && int.TryParse(configs["minLength"], out var minLen) ? minLen : null,
                MaxLength = configs.ContainsKey("maxLength") && int.TryParse(configs["maxLength"], out var maxLen) ? maxLen : null,
                InputMask = configs.ContainsKey("inputMask") ? configs["inputMask"] : null,
                TextTransform = configs.ContainsKey("textTransform") ? configs["textTransform"] : null,
                AutoTrim = configs.ContainsKey("autoTrim") && bool.TryParse(configs["autoTrim"], out var autoTrim) ? autoTrim : null,
                Rows = configs.ContainsKey("rows") && int.TryParse(configs["rows"], out var rows) ? rows : null,

                // Date/Time configurations
                MinDate = configs.ContainsKey("minDate") ? configs["minDate"] : null,
                MaxDate = configs.ContainsKey("maxDate") ? configs["maxDate"] : null,
                MinTime = configs.ContainsKey("minTime") ? configs["minTime"] : null,
                MaxTime = configs.ContainsKey("maxTime") ? configs["maxTime"] : null,
                DisablePastDates = configs.ContainsKey("disablePastDates") && bool.TryParse(configs["disablePastDates"], out var disablePast) ? disablePast : null,
                DisableFutureDates = configs.ContainsKey("disableFutureDates") && bool.TryParse(configs["disableFutureDates"], out var disableFuture) ? disableFuture : null,
                DefaultToToday = configs.ContainsKey("defaultToToday") && bool.TryParse(configs["defaultToToday"], out var defaultToday) ? defaultToday : null,

                // FileUpload configurations
                AllowedFileTypes = configs.ContainsKey("allowedFileTypes") ? configs["allowedFileTypes"] : null,
                MaxFileSize = configs.ContainsKey("maxFileSize") && int.TryParse(configs["maxFileSize"], out var maxFileSize) ? maxFileSize : null,
                MinFileSize = configs.ContainsKey("minFileSize") && int.TryParse(configs["minFileSize"], out var minFileSize) ? minFileSize : null,
                MaxFiles = configs.ContainsKey("maxFiles") && int.TryParse(configs["maxFiles"], out var maxFiles) ? maxFiles : null,
                AllowMultiple = configs.ContainsKey("allowMultiple") && bool.TryParse(configs["allowMultiple"], out var allowMultiple) ? allowMultiple : null,
                PreserveFileName = configs.ContainsKey("preserveFileName") && bool.TryParse(configs["preserveFileName"], out var preserveFileName) ? preserveFileName : null,

                // Image configurations
                AllowedImageTypes = configs.ContainsKey("allowedImageTypes") ? configs["allowedImageTypes"] : null,
                ImageQuality = configs.ContainsKey("imageQuality") && int.TryParse(configs["imageQuality"], out var imageQuality) ? imageQuality : null,
                MaxWidth = configs.ContainsKey("maxWidth") && int.TryParse(configs["maxWidth"], out var maxWidth) ? maxWidth : null,
                MaxHeight = configs.ContainsKey("maxHeight") && int.TryParse(configs["maxHeight"], out var maxHeight) ? maxHeight : null,
                MinWidth = configs.ContainsKey("minWidth") && int.TryParse(configs["minWidth"], out var minWidth) ? minWidth : null,
                MinHeight = configs.ContainsKey("minHeight") && int.TryParse(configs["minHeight"], out var minHeight) ? minHeight : null,
                AspectRatio = configs.ContainsKey("aspectRatio") ? configs["aspectRatio"] : null,
                ThumbnailSize = configs.ContainsKey("thumbnailSize") ? configs["thumbnailSize"] : null,
                AllowCropping = configs.ContainsKey("allowCropping") && bool.TryParse(configs["allowCropping"], out var allowCropping) ? allowCropping : null,
                AutoResize = configs.ContainsKey("autoResize") && bool.TryParse(configs["autoResize"], out var autoResize) ? autoResize : null,

                // Signature configurations
                CanvasWidth = configs.ContainsKey("canvasWidth") && int.TryParse(configs["canvasWidth"], out var canvasWidth) ? canvasWidth : null,
                CanvasHeight = configs.ContainsKey("canvasHeight") && int.TryParse(configs["canvasHeight"], out var canvasHeight) ? canvasHeight : null,
                PenColor = configs.ContainsKey("penColor") ? configs["penColor"] : null,
                PenWidth = configs.ContainsKey("penWidth") && int.TryParse(configs["penWidth"], out var penWidth) ? penWidth : null,
                BackgroundColor = configs.ContainsKey("backgroundColor") ? configs["backgroundColor"] : null,
                OutputFormat = configs.ContainsKey("outputFormat") ? configs["outputFormat"] : null,
                ShowClearButton = configs.ContainsKey("showClearButton") && bool.TryParse(configs["showClearButton"], out var showClearButton) ? showClearButton : null,
                ShowUndoButton = configs.ContainsKey("showUndoButton") && bool.TryParse(configs["showUndoButton"], out var showUndoButton) ? showUndoButton : null,
                RequireFullName = configs.ContainsKey("requireFullName") && bool.TryParse(configs["requireFullName"], out var requireFullName) ? requireFullName : null,
                ShowDateStamp = configs.ContainsKey("showDateStamp") && bool.TryParse(configs["showDateStamp"], out var showDateStamp) ? showDateStamp : null,

                // Rating configurations
                RatingMax = configs.ContainsKey("ratingMax") && int.TryParse(configs["ratingMax"], out var ratingMax) ? ratingMax : null,
                RatingIcon = configs.ContainsKey("ratingIcon") ? configs["ratingIcon"] : null,
                RatingActiveColor = configs.ContainsKey("ratingActiveColor") ? configs["ratingActiveColor"] : null,
                RatingInactiveColor = configs.ContainsKey("ratingInactiveColor") ? configs["ratingInactiveColor"] : null,
                RatingSize = configs.ContainsKey("ratingSize") ? configs["ratingSize"] : null,
                AllowHalfRating = configs.ContainsKey("allowHalfRating") && bool.TryParse(configs["allowHalfRating"], out var allowHalfRating) ? allowHalfRating : null,
                ShowRatingValue = configs.ContainsKey("showRatingValue") && bool.TryParse(configs["showRatingValue"], out var showRatingValue) ? showRatingValue : null,
                ShowRatingLabels = configs.ContainsKey("showRatingLabels") && bool.TryParse(configs["showRatingLabels"], out var showRatingLabels) ? showRatingLabels : null,
                AllowClearRating = configs.ContainsKey("allowClearRating") && bool.TryParse(configs["allowClearRating"], out var allowClearRating) ? allowClearRating : null,

                // Slider configurations
                SliderMin = configs.ContainsKey("sliderMin") && decimal.TryParse(configs["sliderMin"], out var sliderMin) ? sliderMin : null,
                SliderMax = configs.ContainsKey("sliderMax") && decimal.TryParse(configs["sliderMax"], out var sliderMax) ? sliderMax : null,
                SliderStep = configs.ContainsKey("sliderStep") && decimal.TryParse(configs["sliderStep"], out var sliderStep) ? sliderStep : null,
                SliderDefault = configs.ContainsKey("sliderDefault") && decimal.TryParse(configs["sliderDefault"], out var sliderDefault) ? sliderDefault : null,
                SliderUnit = configs.ContainsKey("sliderUnit") ? configs["sliderUnit"] : null,
                SliderPrefix = configs.ContainsKey("sliderPrefix") ? configs["sliderPrefix"] : null,
                SliderTrackColor = configs.ContainsKey("sliderTrackColor") ? configs["sliderTrackColor"] : null,
                ShowSliderValue = configs.ContainsKey("showSliderValue") && bool.TryParse(configs["showSliderValue"], out var showSliderValue) ? showSliderValue : null,
                ShowSliderTicks = configs.ContainsKey("showSliderTicks") && bool.TryParse(configs["showSliderTicks"], out var showSliderTicks) ? showSliderTicks : null,
                ShowMinMaxLabels = configs.ContainsKey("showMinMaxLabels") && bool.TryParse(configs["showMinMaxLabels"], out var showMinMaxLabels) ? showMinMaxLabels : null,
                ShowSliderInput = configs.ContainsKey("showSliderInput") && bool.TryParse(configs["showSliderInput"], out var showSliderInput) ? showSliderInput : null,

                // Currency configurations
                CurrencyCode = configs.ContainsKey("currencyCode") ? configs["currencyCode"] : null,
                CurrencySymbol = configs.ContainsKey("currencySymbol") ? configs["currencySymbol"] : null,
                CurrencyPosition = configs.ContainsKey("currencyPosition") ? configs["currencyPosition"] : null,
                CurrencyDecimals = configs.ContainsKey("currencyDecimals") && int.TryParse(configs["currencyDecimals"], out var currencyDecimals) ? currencyDecimals : null,
                ThousandSeparator = configs.ContainsKey("thousandSeparator") ? configs["thousandSeparator"] : null,
                DecimalSeparator = configs.ContainsKey("decimalSeparator") ? configs["decimalSeparator"] : null,
                CurrencyMin = configs.ContainsKey("currencyMin") && decimal.TryParse(configs["currencyMin"], out var currencyMin) ? currencyMin : null,
                CurrencyMax = configs.ContainsKey("currencyMax") && decimal.TryParse(configs["currencyMax"], out var currencyMax) ? currencyMax : null,
                AllowNegativeCurrency = configs.ContainsKey("allowNegativeCurrency") && bool.TryParse(configs["allowNegativeCurrency"], out var allowNegativeCurrency) ? allowNegativeCurrency : null,

                // Percentage configurations
                PercentageMin = configs.ContainsKey("percentageMin") && decimal.TryParse(configs["percentageMin"], out var percentageMin) ? percentageMin : null,
                PercentageMax = configs.ContainsKey("percentageMax") && decimal.TryParse(configs["percentageMax"], out var percentageMax) ? percentageMax : null,
                PercentageDecimals = configs.ContainsKey("percentageDecimals") && int.TryParse(configs["percentageDecimals"], out var percentageDecimals) ? percentageDecimals : null,
                PercentageStep = configs.ContainsKey("percentageStep") && decimal.TryParse(configs["percentageStep"], out var percentageStep) ? percentageStep : null,
                ShowPercentSymbol = configs.ContainsKey("showPercentSymbol") && bool.TryParse(configs["showPercentSymbol"], out var showPercentSymbol) ? showPercentSymbol : null,
                AllowOverHundred = configs.ContainsKey("allowOverHundred") && bool.TryParse(configs["allowOverHundred"], out var allowOverHundred) ? allowOverHundred : null,
                ShowAsSlider = configs.ContainsKey("showAsSlider") && bool.TryParse(configs["showAsSlider"], out var showAsSlider) ? showAsSlider : null,
                ShowProgressBar = configs.ContainsKey("showProgressBar") && bool.TryParse(configs["showProgressBar"], out var showProgressBar) ? showProgressBar : null,

                // Email configurations
                AllowMultipleEmails = configs.ContainsKey("allowMultipleEmails") && bool.TryParse(configs["allowMultipleEmails"], out var allowMultipleEmails) ? allowMultipleEmails : null,
                AllowedEmailDomains = configs.ContainsKey("allowedEmailDomains") ? configs["allowedEmailDomains"] : null,
                BlockedEmailDomains = configs.ContainsKey("blockedEmailDomains") ? configs["blockedEmailDomains"] : null,

                // Phone configurations
                DefaultCountryCode = configs.ContainsKey("defaultCountryCode") ? configs["defaultCountryCode"] : null,
                PhoneFormat = configs.ContainsKey("phoneFormat") ? configs["phoneFormat"] : null,
                ShowCountrySelector = configs.ContainsKey("showCountrySelector") && bool.TryParse(configs["showCountrySelector"], out var showCountrySelector) ? showCountrySelector : null,
                ValidatePhoneFormat = configs.ContainsKey("validatePhoneFormat") && bool.TryParse(configs["validatePhoneFormat"], out var validatePhoneFormat) ? validatePhoneFormat : null,

                // URL configurations
                AllowHttp = configs.ContainsKey("allowHttp") && bool.TryParse(configs["allowHttp"], out var allowHttp) ? allowHttp : null,
                AllowHttps = configs.ContainsKey("allowHttps") && bool.TryParse(configs["allowHttps"], out var allowHttps) ? allowHttps : null,
                AllowFtp = configs.ContainsKey("allowFtp") && bool.TryParse(configs["allowFtp"], out var allowFtp) ? allowFtp : null,
                AllowedProtocols = configs.ContainsKey("allowedProtocols") ? configs["allowedProtocols"] : null,
                AllowedUrlDomains = configs.ContainsKey("allowedUrlDomains") ? configs["allowedUrlDomains"] : null,
                RequireHttps = configs.ContainsKey("requireHttps") && bool.TryParse(configs["requireHttps"], out var requireHttps) ? requireHttps : null,
                ShowUrlPreview = configs.ContainsKey("showUrlPreview") && bool.TryParse(configs["showUrlPreview"], out var showUrlPreview) ? showUrlPreview : null
            };
        }

        /// <summary>
        /// Get available field types for palette
        /// </summary>
        public List<FieldTypeDto> GetAvailableFieldTypes()
        {
            return new List<FieldTypeDto>
            {
                // Input Fields
                new() { FieldType = FormFieldType.Text, DisplayName = "Text Input", Icon = "ri-text", Description = "Single-line text", Category = "Input" },
                new() { FieldType = FormFieldType.TextArea, DisplayName = "Text Area", Icon = "ri-file-text-line", Description = "Multi-line text", Category = "Input" },
                new() { FieldType = FormFieldType.Number, DisplayName = "Number", Icon = "ri-hashtag", Description = "Numeric input", Category = "Input" },
                new() { FieldType = FormFieldType.Decimal, DisplayName = "Decimal", Icon = "ri-percent-line", Description = "Decimal number", Category = "Input" },
                
                // Date/Time Fields
                new() { FieldType = FormFieldType.Date, DisplayName = "Date", Icon = "ri-calendar-line", Description = "Date picker", Category = "DateTime" },
                new() { FieldType = FormFieldType.Time, DisplayName = "Time", Icon = "ri-time-line", Description = "Time picker", Category = "DateTime" },
                new() { FieldType = FormFieldType.DateTime, DisplayName = "Date & Time", Icon = "ri-calendar-event-line", Description = "Date and time picker", Category = "DateTime" },
                
                // Selection Fields
                new() { FieldType = FormFieldType.Dropdown, DisplayName = "Dropdown", Icon = "ri-arrow-down-s-line", Description = "Single selection", Category = "Selection" },
                new() { FieldType = FormFieldType.Radio, DisplayName = "Radio Buttons", Icon = "ri-radio-button-line", Description = "Single choice", Category = "Selection" },
                new() { FieldType = FormFieldType.Checkbox, DisplayName = "Checkbox", Icon = "ri-checkbox-line", Description = "Yes/No checkbox", Category = "Selection" },
                new() { FieldType = FormFieldType.MultiSelect, DisplayName = "Multi-Select", Icon = "ri-checkbox-multiple-line", Description = "Multiple selections", Category = "Selection" },
                
                // Media Fields
                new() { FieldType = FormFieldType.FileUpload, DisplayName = "File Upload", Icon = "ri-file-upload-line", Description = "File attachment", Category = "Media" },
                new() { FieldType = FormFieldType.Image, DisplayName = "Image", Icon = "ri-image-line", Description = "Image upload", Category = "Media" },
                new() { FieldType = FormFieldType.Signature, DisplayName = "Signature", Icon = "ri-quill-pen-line", Description = "Digital signature", Category = "Media" },
                
                // Rating Fields
                new() { FieldType = FormFieldType.Rating, DisplayName = "Rating", Icon = "ri-star-line", Description = "Star rating", Category = "Rating" },
                new() { FieldType = FormFieldType.Slider, DisplayName = "Slider", Icon = "ri-slider-line", Description = "Range slider", Category = "Rating" },
                
                // Contact Fields
                new() { FieldType = FormFieldType.Email, DisplayName = "Email", Icon = "ri-mail-line", Description = "Email address", Category = "Contact" },
                new() { FieldType = FormFieldType.Phone, DisplayName = "Phone", Icon = "ri-phone-line", Description = "Phone number", Category = "Contact" },
                new() { FieldType = FormFieldType.Url, DisplayName = "URL", Icon = "ri-link", Description = "Web address", Category = "Contact" },
                
                // Specialized Fields
                new() { FieldType = FormFieldType.Currency, DisplayName = "Currency", Icon = "ri-money-dollar-circle-line", Description = "Currency amount", Category = "Specialized" },
                new() { FieldType = FormFieldType.Percentage, DisplayName = "Percentage", Icon = "ri-percent-line", Description = "Percentage value", Category = "Specialized" }
            };
        }

        /// <summary>
        /// Validate template structure
        /// </summary>
        public async Task<(bool IsValid, List<string> Errors)> ValidateTemplateStructureAsync(int templateId)
        {
            var errors = new List<string>();

            var template = await _context.FormTemplates
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Items)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
            {
                errors.Add("Template not found");
                return (false, errors);
            }

            // Must have at least one section
            if (!template.Sections.Any())
            {
                errors.Add("Template must have at least one section");
            }

            // Each section should have at least one field (warning, not blocker)
            var emptySections = template.Sections.Where(s => !s.Items.Any()).ToList();
            if (emptySections.Any())
            {
                errors.Add($"Warning: {emptySections.Count} section(s) have no fields");
            }

            // Check for duplicate section names
            var duplicateSections = template.Sections
                .GroupBy(s => s.SectionName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateSections.Any())
            {
                errors.Add($"Duplicate section names found: {string.Join(", ", duplicateSections)}");
            }

            return (!errors.Any(), errors);
        }

        /// <summary>
        /// Generate unique field code
        /// Checks ALL items across the entire template to ensure uniqueness
        /// </summary>
        public async Task<string> GenerateFieldCodeAsync(int sectionId, string? fieldName = null)
        {
            var section = await _context.FormTemplateSections
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);

            if (section == null)
                throw new KeyNotFoundException($"Section {sectionId} not found");

            // Build the section prefix (e.g., "SEC3_")
            var sectionPrefix = $"SEC{section.DisplayOrder}_";

            // Query ALL items in the template with this section prefix to find max sequence
            // This ensures uniqueness across the entire template, not just within the section
            var existingCodes = await _context.FormTemplateItems
                .Where(i => i.TemplateId == section.TemplateId && i.ItemCode.StartsWith(sectionPrefix))
                .Select(i => i.ItemCode)
                .ToListAsync();

            var maxSequence = existingCodes.Any()
                ? existingCodes.Max(code => ExtractSequenceNumber(code))
                : 0;

            var nextSequence = maxSequence + 1;

            // Format: SEC{DisplayOrder}_{Sequence:000}
            return $"SEC{section.DisplayOrder}_{nextSequence:D3}";
        }

        /// <summary>
        /// Extract sequence number from item code (e.g., "SEC1_005" → 5)
        /// </summary>
        private int ExtractSequenceNumber(string itemCode)
        {
            if (string.IsNullOrEmpty(itemCode))
                return 0;

            var parts = itemCode.Split('_');
            if (parts.Length == 2 && int.TryParse(parts[1], out var sequence))
                return sequence;

            return 0;
        }

        /// <summary>
        /// Add a new section to a template
        /// </summary>
        public async Task<SectionDto?> AddSectionAsync(int templateId, CreateSectionDto dto)
        {
            // Verify template exists
            var template = await _context.FormTemplates.FindAsync(templateId);
            if (template == null)
                return null;

            // Get next display order
            var maxOrder = await _context.FormTemplateSections
                .Where(s => s.TemplateId == templateId)
                .MaxAsync(s => (int?)s.DisplayOrder) ?? 0;

            // Create new section
            var section = new Models.Entities.Forms.FormTemplateSection
            {
                TemplateId = templateId,
                SectionName = dto.SectionName,
                SectionDescription = dto.SectionDescription,
                DisplayOrder = maxOrder + 1,
                IsCollapsible = dto.IsCollapsible,
                IsCollapsedByDefault = dto.IsCollapsedByDefault,
                IconClass = dto.IconClass,
                ColumnLayout = dto.ColumnLayout,
                CreatedDate = DateTime.UtcNow
            };

            _context.FormTemplateSections.Add(section);
            await _context.SaveChangesAsync();

            // Return as DTO
            return MapToSectionDto(section);
        }

        /// <summary>
        /// Get section by ID for editing
        /// </summary>
        public async Task<SectionDto?> GetSectionByIdAsync(int sectionId)
        {
            var section = await _context.FormTemplateSections
                .Include(s => s.Items.OrderBy(i => i.DisplayOrder))
                    .ThenInclude(i => i.Validations)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Options)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);

            if (section == null)
                return null;

            return MapToSectionDto(section);
        }

        /// <summary>
        /// Update section properties
        /// </summary>
        public async Task<bool> UpdateSectionAsync(int sectionId, UpdateSectionDto dto)
        {
            try
            {
                var section = await _context.FormTemplateSections
                    .FirstOrDefaultAsync(s => s.SectionId == sectionId);

                if (section == null)
                    return false;

                // Update properties
                section.SectionName = dto.SectionName;
                section.SectionDescription = dto.SectionDescription;
                section.IconClass = dto.IconClass;
                section.IsCollapsible = dto.IsCollapsible;
                section.IsCollapsedByDefault = dto.IsCollapsedByDefault;
                section.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log error if you have logging configured
                Console.WriteLine($"Error updating section: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete a section and all its fields
        /// </summary>
        public async Task<bool> DeleteSectionAsync(int sectionId)
        {
            try
            {
                var section = await _context.FormTemplateSections
                    .Include(s => s.Items) // Include fields for cascade delete
                    .FirstOrDefaultAsync(s => s.SectionId == sectionId);

                if (section == null)
                    return false;

                // Remove section (cascade will delete related items)
                _context.FormTemplateSections.Remove(section);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log error if you have logging configured
                Console.WriteLine($"Error deleting section: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Duplicate a section with all its fields
        /// </summary>
        public async Task<SectionDto?> DuplicateSectionAsync(int sectionId)
        {
            try
            {
                // Load original section with all related data
                var originalSection = await _context.FormTemplateSections
                    .Include(s => s.Items.OrderBy(i => i.DisplayOrder))
                        .ThenInclude(i => i.Validations)
                    .Include(s => s.Items)
                        .ThenInclude(i => i.Options)
                    .FirstOrDefaultAsync(s => s.SectionId == sectionId);

                if (originalSection == null)
                    return null;

                // Get next display order for template
                var maxOrder = await _context.FormTemplateSections
                    .Where(s => s.TemplateId == originalSection.TemplateId)
                    .MaxAsync(s => (int?)s.DisplayOrder) ?? 0;

                // Create new section (copy)
                var newSection = new Models.Entities.Forms.FormTemplateSection
                {
                    TemplateId = originalSection.TemplateId,
                    SectionName = $"{originalSection.SectionName} (Copy)",
                    SectionDescription = originalSection.SectionDescription,
                    DisplayOrder = maxOrder + 1,
                    IsCollapsible = originalSection.IsCollapsible,
                    IsCollapsedByDefault = originalSection.IsCollapsedByDefault,
                    IconClass = originalSection.IconClass,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                _context.FormTemplateSections.Add(newSection);
                await _context.SaveChangesAsync(); // Save to get new SectionId

                // Duplicate all fields in the section
                foreach (var originalItem in originalSection.Items.OrderBy(i => i.DisplayOrder))
                {
                    var newItem = new Models.Entities.Forms.FormTemplateItem
                    {
                        TemplateId = originalSection.TemplateId,
                        SectionId = newSection.SectionId,
                        ItemCode = await GenerateFieldCodeAsync(newSection.SectionId), // Generate new code
                        ItemName = originalItem.ItemName,
                        ItemDescription = originalItem.ItemDescription,
                        DataType = originalItem.DataType,
                        IsRequired = originalItem.IsRequired,
                        DisplayOrder = originalItem.DisplayOrder,
                        PlaceholderText = originalItem.PlaceholderText,
                        HelpText = originalItem.HelpText,
                        PrefixText = originalItem.PrefixText,
                        SuffixText = originalItem.SuffixText,
                        DefaultValue = originalItem.DefaultValue,
                        ConditionalLogic = originalItem.ConditionalLogic,
                        LayoutType = originalItem.LayoutType,
                        MatrixGroupId = originalItem.MatrixGroupId,
                        MatrixRowLabel = originalItem.MatrixRowLabel,
                        LibraryFieldId = originalItem.LibraryFieldId,
                        IsLibraryOverride = originalItem.IsLibraryOverride,
                        Version = originalItem.Version,
                        IsActive = originalItem.IsActive,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.FormTemplateItems.Add(newItem);
                    await _context.SaveChangesAsync(); // Save to get new ItemId

                    // Duplicate validations
                    foreach (var originalValidation in originalItem.Validations ?? new List<Models.Entities.Forms.FormItemValidation>())
                    {
                        var newValidation = new Models.Entities.Forms.FormItemValidation
                        {
                            ItemId = newItem.ItemId,
                            ValidationType = originalValidation.ValidationType,
                            MinValue = originalValidation.MinValue,
                            MaxValue = originalValidation.MaxValue,
                            MinLength = originalValidation.MinLength,
                            MaxLength = originalValidation.MaxLength,
                            RegexPattern = originalValidation.RegexPattern,
                            CustomExpression = originalValidation.CustomExpression,
                            ErrorMessage = originalValidation.ErrorMessage,
                            Severity = originalValidation.Severity,
                            ValidationOrder = originalValidation.ValidationOrder,
                            IsActive = originalValidation.IsActive,
                            CreatedDate = DateTime.UtcNow
                        };

                        _context.FormItemValidations.Add(newValidation);
                    }

                    // Duplicate options
                    foreach (var originalOption in originalItem.Options ?? new List<FormItemOption>())
                    {
                        var newOption = new FormItemOption
                        {
                            ItemId = newItem.ItemId,
                            OptionLabel = originalOption.OptionLabel,
                            OptionValue = originalOption.OptionValue,
                            DisplayOrder = originalOption.DisplayOrder,
                            IsDefault = originalOption.IsDefault
                        };

                        _context.FormItemOptions.Add(newOption);
                    }

                    await _context.SaveChangesAsync();
                }

                // Return as DTO
                return MapToSectionDto(newSection);
            }
            catch (Exception ex)
            {
                // Log error if you have logging configured
                Console.WriteLine($"Error duplicating section: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Update display order of sections after drag-drop reordering
        /// </summary>
        public async Task<bool> ReorderSectionsAsync(int templateId, List<SectionOrderDto> sections)
        {
            try
            {
                // Verify template exists
                var template = await _context.FormTemplates.FindAsync(templateId);
                if (template == null)
                    return false;

                // Update each section's display order
                foreach (var item in sections)
                {
                    var section = await _context.FormTemplateSections
                        .FirstOrDefaultAsync(s => s.SectionId == item.SectionId && s.TemplateId == templateId);

                    if (section != null)
                    {
                        section.DisplayOrder = item.DisplayOrder;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log error if you have logging configured
                Console.WriteLine($"Error reordering sections: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Add a new field to a section
        /// </summary>
        public async Task<FieldDto?> AddFieldAsync(CreateFieldDto dto)
        {
            try
            {
                Console.WriteLine($"AddFieldAsync called - SectionId: {dto.SectionId}, ItemName: {dto.ItemName}, DataType (string): {dto.DataType}");

                // Verify section exists
                var section = await _context.FormTemplateSections
                    .Include(s => s.Items)
                    .FirstOrDefaultAsync(s => s.SectionId == dto.SectionId);

                Console.WriteLine($"Section found: {section != null}, Items count: {section?.Items?.Count ?? 0}");

                if (section == null)
                {
                    Console.WriteLine("Section not found - returning null");
                    return null;
                }

                // Generate item code if not provided
                Console.WriteLine("Generating item code...");
                var itemCode = string.IsNullOrEmpty(dto.ItemCode)
                    ? await GenerateFieldCodeAsync(dto.SectionId)
                    : dto.ItemCode;
                Console.WriteLine($"Item code: {itemCode}");

                // Get next display order if not provided
                Console.WriteLine("Calculating display order...");
                var displayOrder = dto.DisplayOrder > 0
                    ? dto.DisplayOrder
                    : (section.Items != null && section.Items.Any() ? section.Items.Max(i => i.DisplayOrder) + 1 : 1);
                Console.WriteLine($"Display order: {displayOrder}");

                // Create new field
                Console.WriteLine("Creating new field entity...");
                var newField = new Models.Entities.Forms.FormTemplateItem
                {
                    TemplateId = section.TemplateId,
                    SectionId = dto.SectionId,
                    ItemCode = itemCode,
                    ItemName = dto.ItemName,
                    ItemDescription = dto.ItemDescription, // From modal
                    DataType = dto.DataType,  // DataType is now a string, no need for .ToString()
                    IsRequired = dto.IsRequired,
                    DisplayOrder = displayOrder,
                    PlaceholderText = dto.PlaceholderText,
                    HelpText = dto.HelpText,
                    DefaultValue = dto.DefaultValue,
                    LayoutType = "Single", // Default layout
                    Version = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                Console.WriteLine("Adding to context...");
                _context.FormTemplateItems.Add(newField);

                Console.WriteLine("Saving changes...");
                await _context.SaveChangesAsync();
                Console.WriteLine($"Field saved with ID: {newField.ItemId}");

                // Auto-create options for selection fields
                if (RequiresOptions(dto.DataType))
                {
                    if (dto.OptionTemplateId.HasValue && dto.OptionTemplateId.Value > 0)
                    {
                        // Use option template
                        Console.WriteLine($"Field type '{dto.DataType}' - applying option template {dto.OptionTemplateId.Value}...");
                        await ApplyOptionTemplateAsync(newField.ItemId, dto.OptionTemplateId.Value);
                    }
                    else
                    {
                        // Use default options
                        Console.WriteLine($"Field type '{dto.DataType}' requires options - creating 3 defaults...");
                        await CreateDefaultOptionsAsync(newField.ItemId, 3);
                    }
                }

                // Reload the field with navigation properties to avoid null reference issues
                Console.WriteLine("Reloading field with navigation properties...");
                var savedField = await _context.FormTemplateItems
                    .Include(i => i.Validations)
                    .Include(i => i.Options)
                    .FirstOrDefaultAsync(i => i.ItemId == newField.ItemId);

                if (savedField == null)
                {
                    Console.WriteLine("ERROR: Could not reload saved field!");
                    throw new Exception("Field was saved but could not be reloaded");
                }

                // Return as DTO
                Console.WriteLine("Mapping to DTO...");
                var result = MapToFieldDto(savedField);
                Console.WriteLine("Mapping complete");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding field: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw; // Re-throw to get full error details in controller
            }
        }

        /// <summary>
        /// Get field by ID for editing
        /// </summary>
        public async Task<FieldDto?> GetFieldByIdAsync(int fieldId)
        {
            var field = await _context.FormTemplateItems
                .Include(i => i.Validations)
                .Include(i => i.Options)
                .Include(i => i.Configurations)
                .FirstOrDefaultAsync(i => i.ItemId == fieldId);

            if (field == null)
                return null;

            return MapToFieldDto(field);
        }

        /// <summary>
        /// Update field properties
        /// </summary>
        public async Task<bool> UpdateFieldAsync(int fieldId, UpdateFieldDto dto)
        {
            try
            {
                var field = await _context.FormTemplateItems
                    .Include(i => i.Configurations)
                    .FirstOrDefaultAsync(i => i.ItemId == fieldId);

                if (field == null)
                    return false;

                // Update basic properties
                field.ItemName = dto.ItemName;
                field.ItemDescription = dto.ItemDescription;
                field.IsRequired = dto.IsRequired;
                field.PlaceholderText = dto.PlaceholderText;
                field.HelpText = dto.HelpText;
                field.PrefixText = dto.PrefixText;
                field.SuffixText = dto.SuffixText;
                field.DefaultValue = dto.DefaultValue;

                // Update field-specific configurations (Number/Text)
                await UpdateOrCreateConfigAsync(fieldId, "minValue", dto.MinValue?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "maxValue", dto.MaxValue?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "step", dto.Step?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "decimalPlaces", dto.DecimalPlaces?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "allowNegative", dto.AllowNegative?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "minLength", dto.MinLength?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "maxLength", dto.MaxLength?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "inputMask", dto.InputMask);
                await UpdateOrCreateConfigAsync(fieldId, "textTransform", dto.TextTransform);
                await UpdateOrCreateConfigAsync(fieldId, "autoTrim", dto.AutoTrim?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "rows", dto.Rows?.ToString());

                // Update Date/Time configurations
                await UpdateOrCreateConfigAsync(fieldId, "minDate", dto.MinDate);
                await UpdateOrCreateConfigAsync(fieldId, "maxDate", dto.MaxDate);
                await UpdateOrCreateConfigAsync(fieldId, "minTime", dto.MinTime);
                await UpdateOrCreateConfigAsync(fieldId, "maxTime", dto.MaxTime);
                await UpdateOrCreateConfigAsync(fieldId, "disablePastDates", dto.DisablePastDates?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "disableFutureDates", dto.DisableFutureDates?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "defaultToToday", dto.DefaultToToday?.ToString());

                // Update FileUpload configurations
                await UpdateOrCreateConfigAsync(fieldId, "allowedFileTypes", dto.AllowedFileTypes);
                await UpdateOrCreateConfigAsync(fieldId, "maxFileSize", dto.MaxFileSize?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "minFileSize", dto.MinFileSize?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "maxFiles", dto.MaxFiles?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "allowMultiple", dto.AllowMultiple?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "preserveFileName", dto.PreserveFileName?.ToString());

                // Update Image configurations
                await UpdateOrCreateConfigAsync(fieldId, "allowedImageTypes", dto.AllowedImageTypes);
                await UpdateOrCreateConfigAsync(fieldId, "imageQuality", dto.ImageQuality?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "maxWidth", dto.MaxWidth?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "maxHeight", dto.MaxHeight?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "minWidth", dto.MinWidth?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "minHeight", dto.MinHeight?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "aspectRatio", dto.AspectRatio);
                await UpdateOrCreateConfigAsync(fieldId, "thumbnailSize", dto.ThumbnailSize);
                await UpdateOrCreateConfigAsync(fieldId, "allowCropping", dto.AllowCropping?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "autoResize", dto.AutoResize?.ToString());

                // Update Signature configurations
                await UpdateOrCreateConfigAsync(fieldId, "canvasWidth", dto.CanvasWidth?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "canvasHeight", dto.CanvasHeight?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "penColor", dto.PenColor);
                await UpdateOrCreateConfigAsync(fieldId, "penWidth", dto.PenWidth?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "backgroundColor", dto.BackgroundColor);
                await UpdateOrCreateConfigAsync(fieldId, "outputFormat", dto.OutputFormat);
                await UpdateOrCreateConfigAsync(fieldId, "showClearButton", dto.ShowClearButton?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "showUndoButton", dto.ShowUndoButton?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "requireFullName", dto.RequireFullName?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "showDateStamp", dto.ShowDateStamp?.ToString());

                // Update Rating configurations
                await UpdateOrCreateConfigAsync(fieldId, "ratingMax", dto.RatingMax?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "ratingIcon", dto.RatingIcon);
                await UpdateOrCreateConfigAsync(fieldId, "ratingActiveColor", dto.RatingActiveColor);
                await UpdateOrCreateConfigAsync(fieldId, "ratingInactiveColor", dto.RatingInactiveColor);
                await UpdateOrCreateConfigAsync(fieldId, "ratingSize", dto.RatingSize);
                await UpdateOrCreateConfigAsync(fieldId, "allowHalfRating", dto.AllowHalfRating?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "showRatingValue", dto.ShowRatingValue?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "showRatingLabels", dto.ShowRatingLabels?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "allowClearRating", dto.AllowClearRating?.ToString());

                // Update Slider configurations
                await UpdateOrCreateConfigAsync(fieldId, "sliderMin", dto.SliderMin?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "sliderMax", dto.SliderMax?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "sliderStep", dto.SliderStep?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "sliderDefault", dto.SliderDefault?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "sliderUnit", dto.SliderUnit);
                await UpdateOrCreateConfigAsync(fieldId, "sliderPrefix", dto.SliderPrefix);
                await UpdateOrCreateConfigAsync(fieldId, "sliderTrackColor", dto.SliderTrackColor);
                await UpdateOrCreateConfigAsync(fieldId, "showSliderValue", dto.ShowSliderValue?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "showSliderTicks", dto.ShowSliderTicks?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "showMinMaxLabels", dto.ShowMinMaxLabels?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "showSliderInput", dto.ShowSliderInput?.ToString());

                // Update Currency configurations
                await UpdateOrCreateConfigAsync(fieldId, "currencyCode", dto.CurrencyCode);
                await UpdateOrCreateConfigAsync(fieldId, "currencySymbol", dto.CurrencySymbol);
                await UpdateOrCreateConfigAsync(fieldId, "currencyPosition", dto.CurrencyPosition);
                await UpdateOrCreateConfigAsync(fieldId, "currencyDecimals", dto.CurrencyDecimals?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "thousandSeparator", dto.ThousandSeparator);
                await UpdateOrCreateConfigAsync(fieldId, "decimalSeparator", dto.DecimalSeparator);
                await UpdateOrCreateConfigAsync(fieldId, "currencyMin", dto.CurrencyMin?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "currencyMax", dto.CurrencyMax?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "allowNegativeCurrency", dto.AllowNegativeCurrency?.ToString());

                // Update Percentage configurations
                await UpdateOrCreateConfigAsync(fieldId, "percentageMin", dto.PercentageMin?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "percentageMax", dto.PercentageMax?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "percentageDecimals", dto.PercentageDecimals?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "percentageStep", dto.PercentageStep?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "showPercentSymbol", dto.ShowPercentSymbol?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "allowOverHundred", dto.AllowOverHundred?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "showAsSlider", dto.ShowAsSlider?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "showProgressBar", dto.ShowProgressBar?.ToString());

                // Update Email configurations
                await UpdateOrCreateConfigAsync(fieldId, "allowMultipleEmails", dto.AllowMultipleEmails?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "allowedEmailDomains", dto.AllowedEmailDomains);
                await UpdateOrCreateConfigAsync(fieldId, "blockedEmailDomains", dto.BlockedEmailDomains);

                // Update Phone configurations
                await UpdateOrCreateConfigAsync(fieldId, "defaultCountryCode", dto.DefaultCountryCode);
                await UpdateOrCreateConfigAsync(fieldId, "phoneFormat", dto.PhoneFormat);
                await UpdateOrCreateConfigAsync(fieldId, "showCountrySelector", dto.ShowCountrySelector?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "validatePhoneFormat", dto.ValidatePhoneFormat?.ToString());

                // Update URL configurations
                await UpdateOrCreateConfigAsync(fieldId, "allowHttp", dto.AllowHttp?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "allowHttps", dto.AllowHttps?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "allowFtp", dto.AllowFtp?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "allowedProtocols", dto.AllowedProtocols);
                await UpdateOrCreateConfigAsync(fieldId, "allowedUrlDomains", dto.AllowedUrlDomains);
                await UpdateOrCreateConfigAsync(fieldId, "requireHttps", dto.RequireHttps?.ToString());
                await UpdateOrCreateConfigAsync(fieldId, "showUrlPreview", dto.ShowUrlPreview?.ToString());

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating field: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete a field
        /// </summary>
        public async Task<bool> DeleteFieldAsync(int fieldId)
        {
            try
            {
                var field = await _context.FormTemplateItems
                    .Include(i => i.Validations)
                    .Include(i => i.Options)
                    .Include(i => i.Configurations)
                    .FirstOrDefaultAsync(i => i.ItemId == fieldId);

                if (field == null)
                    return false;

                // Remove field (cascade will delete related items)
                _context.FormTemplateItems.Remove(field);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting field: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Duplicate a field with all its settings
        /// </summary>
        public async Task<FieldDto?> DuplicateFieldAsync(int fieldId)
        {
            try
            {
                // Load original field with all related data
                var originalField = await _context.FormTemplateItems
                    .Include(i => i.Validations)
                    .Include(i => i.Options)
                    .Include(i => i.Configurations)
                    .FirstOrDefaultAsync(i => i.ItemId == fieldId);

                if (originalField == null)
                    return null;

                // Generate new field code
                var newItemCode = await GenerateFieldCodeAsync(originalField.SectionId);

                // Get next display order
                var maxOrder = await _context.FormTemplateItems
                    .Where(i => i.SectionId == originalField.SectionId)
                    .MaxAsync(i => (int?)i.DisplayOrder) ?? 0;

                // Create new field (copy)
                var newField = new Models.Entities.Forms.FormTemplateItem
                {
                    TemplateId = originalField.TemplateId,
                    SectionId = originalField.SectionId,
                    ItemCode = newItemCode,
                    ItemName = $"{originalField.ItemName} (Copy)",
                    ItemDescription = originalField.ItemDescription,
                    DataType = originalField.DataType,
                    IsRequired = originalField.IsRequired,
                    DisplayOrder = maxOrder + 1,
                    PlaceholderText = originalField.PlaceholderText,
                    HelpText = originalField.HelpText,
                    PrefixText = originalField.PrefixText,
                    SuffixText = originalField.SuffixText,
                    DefaultValue = originalField.DefaultValue,
                    ConditionalLogic = originalField.ConditionalLogic,
                    LayoutType = originalField.LayoutType,
                    MatrixGroupId = originalField.MatrixGroupId,
                    MatrixRowLabel = originalField.MatrixRowLabel,
                    LibraryFieldId = originalField.LibraryFieldId,
                    IsLibraryOverride = originalField.IsLibraryOverride,
                    Version = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.FormTemplateItems.Add(newField);
                await _context.SaveChangesAsync();

                // Duplicate validations
                foreach (var validation in originalField.Validations ?? new List<Models.Entities.Forms.FormItemValidation>())
                {
                    var newValidation = new Models.Entities.Forms.FormItemValidation
                    {
                        ItemId = newField.ItemId,
                        ValidationType = validation.ValidationType,
                        MinValue = validation.MinValue,
                        MaxValue = validation.MaxValue,
                        MinLength = validation.MinLength,
                        MaxLength = validation.MaxLength,
                        RegexPattern = validation.RegexPattern,
                        CustomExpression = validation.CustomExpression,
                        ErrorMessage = validation.ErrorMessage,
                        Severity = validation.Severity,
                        ValidationOrder = validation.ValidationOrder,
                        IsActive = validation.IsActive,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.FormItemValidations.Add(newValidation);
                }

                // Duplicate options
                foreach (var option in originalField.Options ?? new List<Models.Entities.Forms.FormItemOption>())
                {
                    var newOption = new Models.Entities.Forms.FormItemOption
                    {
                        ItemId = newField.ItemId,
                        OptionLabel = option.OptionLabel,
                        OptionValue = option.OptionValue,
                        DisplayOrder = option.DisplayOrder,
                        IsDefault = option.IsDefault
                    };

                    _context.FormItemOptions.Add(newOption);
                }

                // Duplicate configurations
                foreach (var config in originalField.Configurations ?? new List<Models.Entities.Forms.FormItemConfiguration>())
                {
                    var newConfig = new Models.Entities.Forms.FormItemConfiguration
                    {
                        ItemId = newField.ItemId,
                        ConfigKey = config.ConfigKey,
                        ConfigValue = config.ConfigValue
                    };

                    _context.FormItemConfigurations.Add(newConfig);
                }

                await _context.SaveChangesAsync();

                // Return as DTO
                return MapToFieldDto(newField);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error duplicating field: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Update display order of fields after drag-drop reordering
        /// </summary>
        public async Task<bool> ReorderFieldsAsync(int sectionId, List<FieldOrderDto> fields)
        {
            try
            {
                // Verify section exists
                var section = await _context.FormTemplateSections.FindAsync(sectionId);
                if (section == null)
                    return false;

                // Update each field's display order
                foreach (var item in fields)
                {
                    var field = await _context.FormTemplateItems
                        .FirstOrDefaultAsync(i => i.ItemId == item.ItemId && i.SectionId == sectionId);

                    if (field != null)
                    {
                        field.DisplayOrder = item.DisplayOrder;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reordering fields: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update field type (inline quick edit)
        /// </summary>
        public async Task<bool> UpdateFieldTypeAsync(int fieldId, string newType)
        {
            try
            {
                var field = await _context.FormTemplateItems
                    .Include(f => f.Options)
                    .FirstOrDefaultAsync(f => f.ItemId == fieldId);

                if (field == null)
                    return false;

                var oldType = field.DataType ?? "Text";
                var oldNeedsOptions = RequiresOptions(oldType);
                var newNeedsOptions = RequiresOptions(newType);

                Console.WriteLine($"Changing field type from {oldType} to {newType}");
                Console.WriteLine($"Old needs options: {oldNeedsOptions}, New needs options: {newNeedsOptions}");

                // Case 1: Changing TO an options field (no options exist)
                if (!oldNeedsOptions && newNeedsOptions)
                {
                    if (!field.Options.Any())
                    {
                        Console.WriteLine("Creating default options...");
                        await CreateDefaultOptionsAsync(fieldId, 3);
                    }
                }

                // Case 2: Changing FROM options field (options exist)
                // We keep the options (data preservation) - they just won't be used
                // No action needed

                // Case 3: Both need options - keep existing ones
                // No action needed

                // Update field type
                field.DataType = newType;
                await _context.SaveChangesAsync();

                Console.WriteLine("Field type updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating field type: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Move a field to a different section (cross-section drag)
        /// Updates the field's SectionId
        /// </summary>
        public async Task<bool> MoveFieldToSectionAsync(int fieldId, int targetSectionId)
        {
            try
            {
                // Verify field exists
                var field = await _context.FormTemplateItems.FindAsync(fieldId);
                if (field == null)
                    return false;

                // Verify target section exists
                var targetSection = await _context.FormTemplateSections.FindAsync(targetSectionId);
                if (targetSection == null)
                    return false;

                // Update field's section
                field.SectionId = targetSectionId;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving field to section: {ex.Message}");
                return false;
            }
        }

        // ========================================================================
        // OPTIONS MANAGEMENT
        // ========================================================================

        /// <summary>
        /// Add a new option to a field
        /// </summary>
        public async Task<FieldOptionDto?> AddOptionAsync(int fieldId, FieldOptionDto dto)
        {
            try
            {
                // Verify field exists and requires options
                var field = await _context.FormTemplateItems
                    .Include(f => f.Options)
                    .FirstOrDefaultAsync(f => f.ItemId == fieldId);

                if (field == null)
                    return null;

                if (!RequiresOptions(field.DataType ?? "Text"))
                    return null;

                // Auto-generate value from label if empty
                var optionValue = string.IsNullOrWhiteSpace(dto.OptionValue)
                    ? GenerateUniqueOptionValue(dto.OptionLabel, fieldId)
                    : dto.OptionValue.Trim();

                // Validate unique value
                if (await _context.FormItemOptions.AnyAsync(o => o.ItemId == fieldId && o.OptionValue == optionValue))
                {
                    // Try appending counter
                    optionValue = GenerateUniqueOptionValue(optionValue, fieldId);
                }

                // Get max display order
                var maxOrder = await _context.FormItemOptions
                    .Where(o => o.ItemId == fieldId)
                    .MaxAsync(o => (int?)o.DisplayOrder) ?? 0;

                // Create new option
                var newOption = new FormItemOption
                {
                    ItemId = fieldId,
                    OptionLabel = dto.OptionLabel.Trim(),
                    OptionValue = optionValue,
                    DisplayOrder = maxOrder + 1,
                    IsDefault = dto.IsDefault,
                    IsActive = dto.IsActive
                };

                _context.FormItemOptions.Add(newOption);
                await _context.SaveChangesAsync();

                // Return DTO
                return new FieldOptionDto
                {
                    OptionId = newOption.OptionId,
                    OptionLabel = newOption.OptionLabel,
                    OptionValue = newOption.OptionValue,
                    DisplayOrder = newOption.DisplayOrder,
                    IsDefault = newOption.IsDefault,
                    IsActive = newOption.IsActive
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding option: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Update an existing option
        /// </summary>
        public async Task<bool> UpdateOptionAsync(int optionId, FieldOptionDto dto)
        {
            try
            {
                var option = await _context.FormItemOptions
                    .Include(o => o.Item)
                    .FirstOrDefaultAsync(o => o.OptionId == optionId);

                if (option == null)
                    return false;

                // Validate unique value (excluding current option)
                var duplicateValue = await _context.FormItemOptions
                    .AnyAsync(o => o.ItemId == option.ItemId &&
                                   o.OptionId != optionId &&
                                   o.OptionValue == dto.OptionValue.Trim());

                if (duplicateValue)
                    throw new InvalidOperationException("Option value must be unique within the field");

                // Update option
                option.OptionLabel = dto.OptionLabel.Trim();
                option.OptionValue = dto.OptionValue.Trim();
                option.IsDefault = dto.IsDefault;
                option.IsActive = dto.IsActive;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating option: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete an option (enforces minimum 2 options rule)
        /// </summary>
        public async Task<bool> DeleteOptionAsync(int optionId)
        {
            try
            {
                var option = await _context.FormItemOptions
                    .Include(o => o.Item)
                    .FirstOrDefaultAsync(o => o.OptionId == optionId);

                if (option == null)
                    return false;

                // Count options for this field
                var optionCount = await _context.FormItemOptions
                    .CountAsync(o => o.ItemId == option.ItemId);

                // Enforce minimum 2 options
                if (optionCount <= 2)
                {
                    throw new InvalidOperationException("Cannot delete option. Selection fields require at least 2 options.");
                }

                _context.FormItemOptions.Remove(option);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting option: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Reorder options within a field
        /// </summary>
        public async Task<bool> ReorderOptionsAsync(int fieldId, List<ReorderOptionDto> updates)
        {
            try
            {
                foreach (var update in updates)
                {
                    var option = await _context.FormItemOptions
                        .FirstOrDefaultAsync(o => o.OptionId == update.OptionId && o.ItemId == fieldId);

                    if (option != null)
                    {
                        option.DisplayOrder = update.DisplayOrder;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reordering options: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Set an option as default
        /// </summary>
        public async Task<bool> SetDefaultOptionAsync(int optionId, int fieldId)
        {
            try
            {
                var field = await _context.FormTemplateItems
                    .Include(f => f.Options)
                    .FirstOrDefaultAsync(f => f.ItemId == fieldId);

                if (field == null)
                    return false;

                // Determine if single or multi-select
                var isSingleSelect = field.DataType == "Dropdown" || field.DataType == "Radio";

                if (isSingleSelect)
                {
                    // Single-select: Unset all other defaults
                    foreach (var opt in field.Options)
                    {
                        opt.IsDefault = (opt.OptionId == optionId);
                    }
                }
                else
                {
                    // Multi-select: Toggle this option's default
                    var option = field.Options.FirstOrDefault(o => o.OptionId == optionId);
                    if (option != null)
                    {
                        option.IsDefault = !option.IsDefault;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting default option: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Apply an option template to a field (replaces existing options)
        /// </summary>
        public async Task<FieldDto?> ApplyOptionTemplateAsync(int fieldId, int templateId)
        {
            try
            {
                Console.WriteLine($"Applying template {templateId} to field {fieldId}");

                // Load field with options
                var field = await _context.FormTemplateItems
                    .Include(i => i.Options)
                    .Include(i => i.Validations)
                    .Include(i => i.Configurations)
                    .FirstOrDefaultAsync(i => i.ItemId == fieldId);

                if (field == null)
                {
                    Console.WriteLine($"Field {fieldId} not found");
                    return null;
                }

                // Validate field type supports options
                if (!RequiresOptions(field.DataType ?? "Text"))
                {
                    throw new InvalidOperationException($"Field type '{field.DataType}' does not support options");
                }

                // Load template with items
                var template = await _context.FormItemOptionTemplates
                    .Include(t => t.Items.OrderBy(i => i.DisplayOrder))
                    .FirstOrDefaultAsync(t => t.TemplateId == templateId);

                if (template == null)
                {
                    throw new ArgumentException($"Template with ID {templateId} not found");
                }

                Console.WriteLine($"Found template '{template.TemplateName}' with {template.Items.Count} items");

                // Clear existing options
                if (field.Options.Any())
                {
                    Console.WriteLine($"Removing {field.Options.Count} existing options");
                    _context.FormItemOptions.RemoveRange(field.Options);
                    await _context.SaveChangesAsync();
                }

                // Create new options from template
                var newOptions = new List<Models.Entities.Forms.FormItemOption>();
                foreach (var templateItem in template.Items.OrderBy(i => i.DisplayOrder))
                {
                    newOptions.Add(new Models.Entities.Forms.FormItemOption
                    {
                        ItemId = fieldId,
                        OptionValue = templateItem.OptionValue,
                        OptionLabel = templateItem.OptionLabel,
                        DisplayOrder = templateItem.DisplayOrder,
                        ScoreValue = templateItem.ScoreValue,
                        ScoreWeight = templateItem.ScoreWeight,
                        IsDefault = templateItem.IsDefault,
                        IsActive = true
                    });
                }

                Console.WriteLine($"Creating {newOptions.Count} new options from template");
                _context.FormItemOptions.AddRange(newOptions);

                // Increment template usage count
                template.UsageCount++;
                Console.WriteLine($"Incremented template usage count to {template.UsageCount}");

                await _context.SaveChangesAsync();

                // Reload field with new options
                var updatedField = await GetFieldByIdAsync(fieldId);
                Console.WriteLine($"Template applied successfully. Field now has {updatedField?.Options.Count} options");

                return updatedField;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying template: {ex.Message}");
                throw;
            }
        }

        // ========================================================================
        // HELPER METHODS - Option Management
        // ========================================================================

        /// <summary>
        /// Check if field type requires options (Dropdown, Radio, Checkbox, MultiSelect)
        /// </summary>
        private bool RequiresOptions(string dataType)
        {
            return dataType switch
            {
                "Dropdown" => true,
                "Radio" => true,
                "Checkbox" => true,
                "MultiSelect" => true,
                _ => false
            };
        }

        /// <summary>
        /// Create default options for selection fields
        /// Uses existing FormItemOption model - no modifications needed
        /// </summary>
        /// <param name="fieldId">Field ID to create options for</param>
        /// <param name="count">Number of default options to create (default: 3)</param>
        private async Task CreateDefaultOptionsAsync(int fieldId, int count = 3)
        {
            var defaultOptions = new List<Models.Entities.Forms.FormItemOption>();

            for (int i = 1; i <= count; i++)
            {
                defaultOptions.Add(new Models.Entities.Forms.FormItemOption
                {
                    ItemId = fieldId,
                    OptionLabel = $"Option {i}",           // User-facing label
                    OptionValue = $"option_{i}",           // System identifier (lowercase_underscore)
                    DisplayOrder = i,
                    IsDefault = false,                     // No default selection
                    IsActive = true
                });
            }

            _context.FormItemOptions.AddRange(defaultOptions);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Created {count} default options for field {fieldId}");
        }

        /// <summary>
        /// Generate a unique option value from a label
        /// Converts to lowercase, replaces spaces/special chars with underscores
        /// Appends counter if value already exists
        /// </summary>
        /// <param name="label">Option label to generate value from</param>
        /// <param name="fieldId">Field ID to check uniqueness within</param>
        /// <returns>Unique option value</returns>
        private string GenerateUniqueOptionValue(string label, int fieldId)
        {
            // Convert label to value format
            var baseValue = label
                .ToLower()
                .Trim()
                .Replace(" ", "_")
                .Replace("-", "_")
                .Trim('_');

            // If empty after conversion, use generic
            if (string.IsNullOrWhiteSpace(baseValue))
                baseValue = "option";

            // Check if base value is unique
            var existing = _context.FormItemOptions
                .Where(o => o.ItemId == fieldId && o.OptionValue.StartsWith(baseValue))
                .Select(o => o.OptionValue)
                .ToList();

            if (!existing.Contains(baseValue))
                return baseValue;

            // Append counter for uniqueness
            int counter = 1;
            while (existing.Contains($"{baseValue}_{counter}"))
                counter++;

            return $"{baseValue}_{counter}";
        }

        // ========================================================================
        // VALIDATION MANAGEMENT
        // ========================================================================

        /// <summary>
        /// Add a validation rule to a field
        /// </summary>
        public async Task<ValidationRuleDto?> AddValidationAsync(int fieldId, CreateValidationDto dto)
        {
            try
            {
                // Verify field exists
                var field = await _context.FormTemplateItems.FindAsync(fieldId);
                if (field == null)
                {
                    throw new InvalidOperationException($"Field with ID {fieldId} not found");
                }

                // Get max validation order
                var maxOrder = await _context.FormItemValidations
                    .Where(v => v.ItemId == fieldId)
                    .MaxAsync(v => (int?)v.ValidationOrder) ?? 0;

                // Create new validation
                var newValidation = new FormItemValidation
                {
                    ItemId = fieldId,
                    ValidationType = dto.ValidationType,
                    MinValue = dto.MinValue,
                    MaxValue = dto.MaxValue,
                    MinLength = dto.MinLength,
                    MaxLength = dto.MaxLength,
                    RegexPattern = dto.RegexPattern,
                    CustomExpression = dto.CustomExpression,
                    ErrorMessage = string.IsNullOrWhiteSpace(dto.ErrorMessage)
                        ? GetDefaultErrorMessage(dto.ValidationType)
                        : dto.ErrorMessage,
                    Severity = dto.Severity,
                    ValidationOrder = maxOrder + 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.FormItemValidations.Add(newValidation);
                await _context.SaveChangesAsync();

                // Map to DTO
                return new ValidationRuleDto
                {
                    ValidationId = newValidation.ItemValidationId,
                    ValidationType = newValidation.ValidationType,
                    MinValue = newValidation.MinValue,
                    MaxValue = newValidation.MaxValue,
                    MinLength = newValidation.MinLength,
                    MaxLength = newValidation.MaxLength,
                    RegexPattern = newValidation.RegexPattern,
                    CustomExpression = newValidation.CustomExpression,
                    ErrorMessage = newValidation.ErrorMessage,
                    Severity = newValidation.Severity,
                    ValidationOrder = newValidation.ValidationOrder,
                    IsActive = newValidation.IsActive
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add validation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update an existing validation rule
        /// </summary>
        public async Task<bool> UpdateValidationAsync(int validationId, UpdateValidationDto dto)
        {
            try
            {
                var validation = await _context.FormItemValidations.FindAsync(validationId);
                if (validation == null)
                {
                    throw new InvalidOperationException($"Validation with ID {validationId} not found");
                }

                // Update properties
                validation.ValidationType = dto.ValidationType;
                validation.MinValue = dto.MinValue;
                validation.MaxValue = dto.MaxValue;
                validation.MinLength = dto.MinLength;
                validation.MaxLength = dto.MaxLength;
                validation.RegexPattern = dto.RegexPattern;
                validation.CustomExpression = dto.CustomExpression;
                validation.ErrorMessage = dto.ErrorMessage;
                validation.Severity = dto.Severity;
                validation.IsActive = dto.IsActive;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update validation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delete a validation rule
        /// </summary>
        public async Task<bool> DeleteValidationAsync(int validationId)
        {
            try
            {
                var validation = await _context.FormItemValidations.FindAsync(validationId);
                if (validation == null)
                {
                    throw new InvalidOperationException($"Validation with ID {validationId} not found");
                }

                _context.FormItemValidations.Remove(validation);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete validation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Reorder validation rules within a field
        /// </summary>
        public async Task<bool> ReorderValidationsAsync(int fieldId, List<ReorderValidationDto> updates)
        {
            try
            {
                // Verify field exists
                var field = await _context.FormTemplateItems.FindAsync(fieldId);
                if (field == null)
                {
                    throw new InvalidOperationException($"Field with ID {fieldId} not found");
                }

                // Update each validation's order
                foreach (var update in updates)
                {
                    var validation = await _context.FormItemValidations.FindAsync(update.ValidationId);
                    if (validation != null && validation.ItemId == fieldId)
                    {
                        validation.ValidationOrder = update.ValidationOrder;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to reorder validations: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get all validation rules for a field
        /// </summary>
        public async Task<List<ValidationRuleDto>> GetValidationsForFieldAsync(int fieldId)
        {
            try
            {
                var validations = await _context.FormItemValidations
                    .Where(v => v.ItemId == fieldId && v.IsActive)
                    .OrderBy(v => v.ValidationOrder)
                    .ToListAsync();

                return validations.Select(v => new ValidationRuleDto
                {
                    ValidationId = v.ItemValidationId,
                    ValidationType = v.ValidationType,
                    MinValue = v.MinValue,
                    MaxValue = v.MaxValue,
                    MinLength = v.MinLength,
                    MaxLength = v.MaxLength,
                    RegexPattern = v.RegexPattern,
                    CustomExpression = v.CustomExpression,
                    ErrorMessage = v.ErrorMessage,
                    Severity = v.Severity,
                    ValidationOrder = v.ValidationOrder,
                    IsActive = v.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get validations: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get default error message for validation type
        /// </summary>
        private string GetDefaultErrorMessage(string validationType)
        {
            return validationType switch
            {
                "Required" => "This field is required.",
                "Email" => "Please enter a valid email address.",
                "Phone" => "Please enter a valid phone number.",
                "URL" => "Please enter a valid URL.",
                "MinLength" => "Input is too short.",
                "MaxLength" => "Input is too long.",
                "MinValue" => "Value is too low.",
                "MaxValue" => "Value is too high.",
                "Range" => "Value is out of range.",
                "Pattern" => "Input does not match the required pattern.",
                "Integer" => "Please enter a whole number.",
                "Decimal" => "Please enter a valid decimal number.",
                "Number" => "Please enter a valid number.",
                "Date" => "Please enter a valid date.",
                _ => "Invalid input."
            };
        }

        // ========================================================================
        // FIELD CONFIGURATION MANAGEMENT (Helper Methods)
        // ========================================================================

        /// <summary>
        /// Update or create a configuration entry for a field
        /// If value is null/empty, deletes the configuration entry
        /// </summary>
        private async Task UpdateOrCreateConfigAsync(int fieldId, string configKey, string? configValue)
        {
            // Find existing configuration
            var existing = await _context.FormItemConfigurations
                .FirstOrDefaultAsync(c => c.ItemId == fieldId && c.ConfigKey == configKey);

            if (string.IsNullOrWhiteSpace(configValue))
            {
                // Delete if value is null/empty
                if (existing != null)
                {
                    _context.FormItemConfigurations.Remove(existing);
                }
            }
            else
            {
                if (existing != null)
                {
                    // Update existing
                    existing.ConfigValue = configValue;
                }
                else
                {
                    // Create new
                    var newConfig = new FormItemConfiguration
                    {
                        ItemId = fieldId,
                        ConfigKey = configKey,
                        ConfigValue = configValue
                    };
                    _context.FormItemConfigurations.Add(newConfig);
                }
            }
        }

        // ========================================================================
        // CONDITIONAL LOGIC MANAGEMENT
        // ========================================================================

        /// <summary>
        /// Get conditional logic for a field
        /// </summary>
        public async Task<ConditionalLogicDto?> GetConditionalLogicAsync(int fieldId)
        {
            var field = await _context.FormTemplateItems
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.ItemId == fieldId);

            if (field == null || string.IsNullOrEmpty(field.ConditionalLogic))
                return null;

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<ConditionalLogicDto>(
                    field.ConditionalLogic,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Save conditional logic for a field
        /// </summary>
        public async Task<bool> SaveConditionalLogicAsync(int fieldId, ConditionalLogicDto dto)
        {
            var field = await _context.FormTemplateItems
                .FirstOrDefaultAsync(f => f.ItemId == fieldId);

            if (field == null)
                return false;

            // Serialize to JSON
            var json = System.Text.Json.JsonSerializer.Serialize(dto, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            field.ConditionalLogic = json;
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Delete/clear conditional logic for a field
        /// </summary>
        public async Task<bool> DeleteConditionalLogicAsync(int fieldId)
        {
            var field = await _context.FormTemplateItems
                .FirstOrDefaultAsync(f => f.ItemId == fieldId);

            if (field == null)
                return false;

            field.ConditionalLogic = null;
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Get available fields for conditional logic
        /// </summary>
        public async Task<List<AvailableFieldDto>> GetAvailableFieldsForLogicAsync(int templateId, int? excludeFieldId = null)
        {
            var query = _context.FormTemplateItems
                .Include(f => f.Section)
                .Include(f => f.Options.Where(o => o.IsActive))
                .Where(f => f.TemplateId == templateId && f.IsActive);

            if (excludeFieldId.HasValue)
            {
                query = query.Where(f => f.ItemId != excludeFieldId.Value);
            }

            var fields = await query
                .OrderBy(f => f.Section.DisplayOrder)
                .ThenBy(f => f.DisplayOrder)
                .ToListAsync();

            return fields.Select(f => new AvailableFieldDto
            {
                ItemId = f.ItemId,
                ItemCode = f.ItemCode,
                ItemName = f.ItemName,
                DataType = f.DataType ?? "Text",
                SectionId = f.SectionId,
                SectionName = f.Section?.SectionName,
                Options = f.Options?.Select(o => new FieldOptionDto
                {
                    OptionId = o.OptionId,
                    OptionLabel = o.OptionLabel,
                    OptionValue = o.OptionValue,
                    DisplayOrder = o.DisplayOrder,
                    IsDefault = o.IsDefault,
                    IsActive = o.IsActive
                }).OrderBy(o => o.DisplayOrder).ToList()
            }).ToList();
        }
    }
}

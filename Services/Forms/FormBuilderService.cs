using FormReporting.Data;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.Common;
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

            return new FieldDto
            {
                ItemId = item.ItemId,
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
                OptionCount = item.Options?.Count ?? 0
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
        /// </summary>
        public async Task<string> GenerateFieldCodeAsync(int sectionId, string? fieldName = null)
        {
            var section = await _context.FormTemplateSections
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);

            if (section == null)
                throw new KeyNotFoundException($"Section {sectionId} not found");

            // Get next sequence number
            var maxSequence = section.Items.Any() 
                ? section.Items.Max(i => ExtractSequenceNumber(i.ItemCode))
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
                CreatedDate = DateTime.UtcNow
            };

            _context.FormTemplateSections.Add(section);
            await _context.SaveChangesAsync();

            // Return as DTO
            return MapToSectionDto(section);
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
    }
}

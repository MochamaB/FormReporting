using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Services.Forms;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Extensions;

namespace FormReporting.Controllers.API
{
    /// <summary>
    /// API Controller for Form Builder operations
    /// Handles AJAX requests from the form builder UI
    /// </summary>
    [ApiController]
    [Route("api/formbuilder")]
    public class FormBuilderApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFormBuilderService _formBuilderService;
        private readonly IFormItemOptionTemplateService _optionTemplateService;

        public FormBuilderApiController(
            ApplicationDbContext context,
            IFormBuilderService formBuilderService,
            IFormItemOptionTemplateService optionTemplateService)
        {
            _context = context;
            _formBuilderService = formBuilderService;
            _optionTemplateService = optionTemplateService;
        }

        /// <summary>
        /// Get template data for builder
        /// </summary>
        [HttpGet("{templateId}")]
        public async Task<IActionResult> GetTemplateData(int templateId)
        {
            var data = await _formBuilderService.LoadForBuilderAsync(templateId);
            
            if (data == null)
                return NotFound(new { success = false, message = "Template not found" });

            return Ok(new { success = true, data });
        }

        /// <summary>
        /// Get available field types for palette
        /// </summary>
        [HttpGet("field-types")]
        public IActionResult GetFieldTypes()
        {
            var fieldTypes = _formBuilderService.GetAvailableFieldTypes();
            return Ok(new { success = true, fieldTypes });
        }

        /// <summary>
        /// Validate template structure
        /// </summary>
        [HttpGet("{templateId}/validate")]
        public async Task<IActionResult> ValidateTemplate(int templateId)
        {
            var (isValid, errors) = await _formBuilderService.ValidateTemplateStructureAsync(templateId);
            
            return Ok(new 
            { 
                success = isValid, 
                isValid, 
                errors 
            });
        }

        /// <summary>
        /// Add a new section to a template
        /// </summary>
        [HttpPost("sections/add")]
        public async Task<IActionResult> AddSection([FromBody] AddSectionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data" });

            var section = await _formBuilderService.AddSectionAsync(request.TemplateId, request.Section);

            if (section == null)
                return NotFound(new { success = false, message = "Template not found" });

            return Ok(new { success = true, section });
        }

        /// <summary>
        /// Get section details by ID
        /// </summary>
        [HttpGet("sections/{sectionId}")]
        public async Task<IActionResult> GetSection(int sectionId)
        {
            var section = await _formBuilderService.GetSectionByIdAsync(sectionId);

            if (section == null)
                return NotFound(new { success = false, message = "Section not found" });

            return Ok(new { success = true, data = section });
        }

        /// <summary>
        /// Render section card HTML for canvas update (without page reload)
        /// GET /api/formbuilder/sections/{sectionId}/render
        /// </summary>
        [HttpGet("sections/{sectionId}/render")]
        public async Task<IActionResult> RenderSectionCard(int sectionId)
        {
            try
            {
                var section = await _formBuilderService.GetSectionByIdAsync(sectionId);

                if (section == null)
                {
                    return NotFound(new { success = false, message = "Section not found" });
                }

                // Render the section partial view to HTML string
                var html = await this.RenderViewAsync(
                    "~/Views/Forms/FormTemplates/Partials/FormBuilder/Components/_BuilderSection.cshtml",
                    section
                );

                return Ok(new { success = true, html, sectionId = section.SectionId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error rendering section", error = ex.Message });
            }
        }

        /// <summary>
        /// Update section properties
        /// </summary>
        [HttpPut("sections/{sectionId}")]
        public async Task<IActionResult> UpdateSection(int sectionId, [FromBody] UpdateSectionDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data" });

            var success = await _formBuilderService.UpdateSectionAsync(sectionId, request);

            if (!success)
                return NotFound(new { success = false, message = "Section not found or update failed" });

            return Ok(new { success = true, message = "Section updated successfully" });
        }

        /// <summary>
        /// Update section configuration (layout, spacing, etc.)
        /// TODO: Implement FormItemConfiguration table to store key-value pairs
        /// </summary>
        [HttpPut("sections/{sectionId}/configuration")]
        public async Task<IActionResult> UpdateSectionConfiguration(int sectionId, [FromBody] dynamic configData)
        {
            // TODO: Implement configuration storage using FormItemConfiguration table
            // For now, return success to allow frontend testing
            // Configuration will be stored as key-value pairs:
            // - columnLayout, sectionWidth, backgroundStyle, showSectionNumber, topPadding, bottomPadding

            return Ok(new
            {
                success = true,
                message = "Configuration saved (placeholder - will be implemented with FormItemConfiguration table)"
            });
        }

        /// <summary>
        /// Delete a section and all its fields
        /// </summary>
        [HttpDelete("sections/{sectionId}")]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            var success = await _formBuilderService.DeleteSectionAsync(sectionId);

            if (!success)
                return NotFound(new { success = false, message = "Section not found or delete failed" });

            return Ok(new { success = true, message = "Section deleted successfully" });
        }

        /// <summary>
        /// Duplicate a section with all its fields
        /// </summary>
        [HttpPost("sections/{sectionId}/duplicate")]
        public async Task<IActionResult> DuplicateSection(int sectionId)
        {
            var duplicatedSection = await _formBuilderService.DuplicateSectionAsync(sectionId);

            if (duplicatedSection == null)
                return NotFound(new { success = false, message = "Section not found or duplication failed" });

            return Ok(new { success = true, section = duplicatedSection, message = "Section duplicated successfully" });
        }

        /// <summary>
        /// Reorder sections after drag-drop
        /// </summary>
        [HttpPut("sections/reorder")]
        public async Task<IActionResult> ReorderSections([FromBody] ReorderSectionsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data" });

            var success = await _formBuilderService.ReorderSectionsAsync(request.TemplateId, request.Sections);

            if (!success)
                return BadRequest(new { success = false, message = "Failed to reorder sections" });

            return Ok(new { success = true, message = "Section order updated successfully" });
        }

        // ============================================================================
        // FIELD OPERATIONS
        // ============================================================================

        /// <summary>
        /// Add a new field to a section
        /// </summary>
        [HttpPost("fields")]
        public async Task<IActionResult> AddField([FromBody] CreateFieldDto dto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(dto.ItemName))
                {
                    return BadRequest(new { success = false, message = "Field name is required" });
                }

                if (dto.SectionId <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid section ID" });
                }

                // Call service to add field
                var newField = await _formBuilderService.AddFieldAsync(dto);

                if (newField == null)
                {
                    return NotFound(new { success = false, message = "Failed to add field. Section not found." });
                }

                return Ok(new
                {
                    success = true,
                    message = "Field added successfully",
                    field = newField
                });
            }
            catch (Exception ex)
            {
                // Get the innermost exception for detailed error message
                var innerEx = ex;
                while (innerEx.InnerException != null)
                    innerEx = innerEx.InnerException;
                
                var errorMessage = $"Error adding field: {ex.Message}";
                if (innerEx != ex)
                    errorMessage += $" | Inner: {innerEx.Message}";
                
                Console.WriteLine($"[AddField ERROR] {errorMessage}");
                Console.WriteLine($"[AddField STACK] {ex.StackTrace}");
                
                return StatusCode(500, new { success = false, message = errorMessage });
            }
        }

        /// <summary>
        /// Get field by ID (for loading properties panel)
        /// </summary>
        [HttpGet("fields/{fieldId}")]
        public async Task<IActionResult> GetField(int fieldId)
        {
            try
            {
                var field = await _formBuilderService.GetFieldByIdAsync(fieldId);

                if (field == null)
                {
                    return NotFound(new { success = false, message = "Field not found" });
                }

                return Ok(new
                {
                    success = true,
                    field = field
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving field: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update field properties (full update)
        /// </summary>
        [HttpPut("fields/{fieldId}")]
        public async Task<IActionResult> UpdateField(int fieldId, [FromBody] UpdateFieldDto dto)
        {
            try
            {
                var success = await _formBuilderService.UpdateFieldAsync(fieldId, dto);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Field not found" });
                }

                return Ok(new { success = true, message = "Field updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating field: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update field type only (partial update for inline type change)
        /// Uses smart option handling: auto-creates defaults for selection fields
        /// </summary>
        [HttpPatch("fields/{fieldId}/type")]
        public async Task<IActionResult> UpdateFieldType(int fieldId, [FromBody] UpdateFieldTypeDto dto)
        {
            try
            {
                // Call service method which handles smart option creation
                var success = await _formBuilderService.UpdateFieldTypeAsync(fieldId, dto.DataType);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Field not found" });
                }

                return Ok(new { success = true, message = "Field type updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating field type: {ex.Message}" });
            }
        }

        /// <summary>
        /// Move a field to a different section (partial update for cross-section drag)
        /// </summary>
        [HttpPatch("fields/{fieldId}/section")]
        public async Task<IActionResult> MoveFieldToSection(int fieldId, [FromBody] MoveFieldToSectionDto dto)
        {
            try
            {
                var success = await _formBuilderService.MoveFieldToSectionAsync(fieldId, dto.SectionId);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Field or section not found" });
                }

                return Ok(new { success = true, message = "Field moved to new section successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error moving field: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete a field (cascade deletes validations, options, configurations)
        /// </summary>
        [HttpDelete("fields/{fieldId}")]
        public async Task<IActionResult> DeleteField(int fieldId)
        {
            try
            {
                var success = await _formBuilderService.DeleteFieldAsync(fieldId);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Field not found" });
                }

                return Ok(new { success = true, message = "Field deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting field: {ex.Message}" });
            }
        }

        /// <summary>
        /// Duplicate a field with all its settings (validations, options, configurations)
        /// </summary>
        [HttpPost("fields/{fieldId}/duplicate")]
        public async Task<IActionResult> DuplicateField(int fieldId)
        {
            try
            {
                var newField = await _formBuilderService.DuplicateFieldAsync(fieldId);

                if (newField == null)
                {
                    return NotFound(new { success = false, message = "Field not found" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Field duplicated successfully",
                    field = newField
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error duplicating field: {ex.Message}" });
            }
        }

        /// <summary>
        /// Reorder fields within a section after drag-drop
        /// </summary>
        [HttpPut("sections/{sectionId}/fields/reorder")]
        public async Task<IActionResult> ReorderFields(int sectionId, [FromBody] List<FieldOrderDto> fields)
        {
            try
            {
                var success = await _formBuilderService.ReorderFieldsAsync(sectionId, fields);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Section not found" });
                }

                return Ok(new { success = true, message = "Fields reordered successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error reordering fields: {ex.Message}" });
            }
        }

        // ========================================================================
        // OPTIONS MANAGEMENT ENDPOINTS
        // ========================================================================

        /// <summary>
        /// Add a new option to a field
        /// POST /api/formbuilder/fields/{fieldId}/options
        /// </summary>
        [HttpPost("fields/{fieldId}/options")]
        public async Task<IActionResult> AddOption(int fieldId, [FromBody] FieldOptionDto dto)
        {
            try
            {
                var newOption = await _formBuilderService.AddOptionAsync(fieldId, dto);

                if (newOption == null)
                {
                    return NotFound(new { success = false, message = "Field not found or does not support options" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Option added successfully",
                    option = newOption
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error adding option: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update an existing option
        /// PUT /api/formbuilder/options/{optionId}
        /// </summary>
        [HttpPut("options/{optionId}")]
        public async Task<IActionResult> UpdateOption(int optionId, [FromBody] FieldOptionDto dto)
        {
            try
            {
                var success = await _formBuilderService.UpdateOptionAsync(optionId, dto);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Option not found" });
                }

                return Ok(new { success = true, message = "Option updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating option: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete an option
        /// DELETE /api/formbuilder/options/{optionId}
        /// </summary>
        [HttpDelete("options/{optionId}")]
        public async Task<IActionResult> DeleteOption(int optionId)
        {
            try
            {
                var success = await _formBuilderService.DeleteOptionAsync(optionId);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Option not found" });
                }

                return Ok(new { success = true, message = "Option deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting option: {ex.Message}" });
            }
        }

        /// <summary>
        /// Reorder options within a field after drag-drop
        /// PUT /api/formbuilder/fields/{fieldId}/options/reorder
        /// </summary>
        [HttpPut("fields/{fieldId}/options/reorder")]
        public async Task<IActionResult> ReorderOptions(int fieldId, [FromBody] List<ReorderOptionDto> updates)
        {
            try
            {
                var success = await _formBuilderService.ReorderOptionsAsync(fieldId, updates);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Field not found" });
                }

                return Ok(new { success = true, message = "Options reordered successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error reordering options: {ex.Message}" });
            }
        }

        /// <summary>
        /// Set an option as default (pre-selected)
        /// PATCH /api/formbuilder/options/{optionId}/default
        /// </summary>
        [HttpPatch("options/{optionId}/default")]
        public async Task<IActionResult> SetDefaultOption(int optionId, [FromQuery] int fieldId)
        {
            try
            {
                var success = await _formBuilderService.SetDefaultOptionAsync(optionId, fieldId);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Option or field not found" });
                }

                return Ok(new { success = true, message = "Default option set successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error setting default option: {ex.Message}" });
            }
        }

        // ========================================================================
        // OPTION TEMPLATES ENDPOINTS
        // ========================================================================

        /// <summary>
        /// Get all active option templates (no field type filter - templates work for all option-based fields)
        /// GET /api/formbuilder/option-templates
        /// </summary>
        [HttpGet("option-templates")]
        public async Task<IActionResult> GetOptionTemplates()
        {
            try
            {
                // Get all active templates - they work for any option-based field (Radio, Dropdown, Checkbox, MultiSelect)
                var templates = await _optionTemplateService.GetActiveTemplatesAsync();

                var templateDtos = templates.Select(t => new
                {
                    t.TemplateId,
                    t.TemplateName,
                    t.TemplateCode,
                    t.Category,
                    t.SubCategory,
                    t.Description,
                    t.HasScoring,
                    t.ScoringType,
                    ItemCount = t.Items.Count,
                    Items = t.Items.Select(i => new
                    {
                        i.OptionValue,
                        i.OptionLabel,
                        i.DisplayOrder,
                        i.ScoreValue,
                        i.ScoreWeight,
                        i.ColorHint,
                        i.IconClass,
                        i.IsDefault
                    }).OrderBy(i => i.DisplayOrder).ToList()
                }).ToList();

                return Ok(new { success = true, templates = templateDtos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching templates", error = ex.Message });
            }
        }

        /// <summary>
        /// Get lightweight option templates list for dropdown selectors (faster, no items loaded)
        /// GET /api/formbuilder/option-templates/list
        /// </summary>
        [HttpGet("option-templates/list")]
        public async Task<IActionResult> GetOptionTemplatesList()
        {
            try
            {
                // Use lightweight query - no Include for Items
                var templates = await _optionTemplateService.GetTemplateSelectListAsync();

                return Ok(new { success = true, templates });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching templates", error = ex.Message });
            }
        }

        /// <summary>
        /// Apply option template to field (replaces existing options)
        /// POST /api/formbuilder/fields/{fieldId}/apply-template/{templateId}
        /// </summary>
        [HttpPost("fields/{fieldId}/apply-template/{templateId}")]
        public async Task<IActionResult> ApplyOptionTemplate(int fieldId, int templateId)
        {
            try
            {
                var result = await _formBuilderService.ApplyOptionTemplateAsync(fieldId, templateId);

                if (result == null)
                {
                    return NotFound(new { success = false, message = "Field not found or field type doesn't support options" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Template applied successfully",
                    field = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error applying template", error = ex.Message });
            }
        }

        /// <summary>
        /// Render field card HTML for canvas update (without page reload)
        /// GET /api/formbuilder/fields/{fieldId}/render
        /// </summary>
        [HttpGet("fields/{fieldId}/render")]
        public async Task<IActionResult> RenderFieldCard(int fieldId)
        {
            try
            {
                var field = await _formBuilderService.GetFieldByIdAsync(fieldId);

                if (field == null)
                {
                    return NotFound(new { success = false, message = "Field not found" });
                }

                // Render the field preview partial view to HTML string
                var html = await this.RenderViewAsync(
                    "~/Views/Forms/FormTemplates/Partials/FormBuilder/Components/_BuilderFieldPreview.cshtml",
                    field
                );

                return Ok(new { success = true, html });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error rendering field", error = ex.Message });
            }
        }

        /// <summary>
        /// Render options table HTML for properties panel update (without page reload)
        /// GET /api/formbuilder/fields/{fieldId}/options/render
        /// </summary>
        [HttpGet("fields/{fieldId}/options/render")]
        public async Task<IActionResult> RenderOptionsTable(int fieldId)
        {
            try
            {
                // Query FormItemOption entities directly from database
                // The partial view expects IEnumerable<FormItemOption>, not DTOs
                var options = await _context.FormItemOptions
                    .Where(o => o.ItemId == fieldId)
                    .OrderBy(o => o.DisplayOrder)
                    .ToListAsync();

                // Render the options table partial view to HTML string
                var html = await this.RenderViewAsync(
                    "~/Views/Forms/FormTemplates/Partials/FormBuilder/Properties/_OptionsTable.cshtml",
                    options
                );

                return Ok(new { success = true, html, optionCount = options.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error rendering options table", error = ex.Message });
            }
        }

        // ========================================================================
        // VALIDATION MANAGEMENT ENDPOINTS
        // ========================================================================

        /// <summary>
        /// Add a validation rule to a field
        /// POST /api/formbuilder/fields/{fieldId}/validations
        /// </summary>
        [HttpPost("fields/{fieldId}/validations")]
        public async Task<IActionResult> AddValidation(int fieldId, [FromBody] CreateValidationDto dto)
        {
            try
            {
                var validation = await _formBuilderService.AddValidationAsync(fieldId, dto);

                if (validation == null)
                {
                    return BadRequest(new { success = false, message = "Failed to add validation" });
                }

                return Ok(new { success = true, message = "Validation added successfully", data = validation });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error adding validation: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update an existing validation rule
        /// PUT /api/formbuilder/validations/{validationId}
        /// </summary>
        [HttpPut("validations/{validationId}")]
        public async Task<IActionResult> UpdateValidation(int validationId, [FromBody] UpdateValidationDto dto)
        {
            try
            {
                var success = await _formBuilderService.UpdateValidationAsync(validationId, dto);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Validation not found" });
                }

                return Ok(new { success = true, message = "Validation updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating validation: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete a validation rule
        /// DELETE /api/formbuilder/validations/{validationId}
        /// </summary>
        [HttpDelete("validations/{validationId}")]
        public async Task<IActionResult> DeleteValidation(int validationId)
        {
            try
            {
                var success = await _formBuilderService.DeleteValidationAsync(validationId);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Validation not found" });
                }

                return Ok(new { success = true, message = "Validation deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting validation: {ex.Message}" });
            }
        }

        /// <summary>
        /// Reorder validation rules within a field
        /// PUT /api/formbuilder/fields/{fieldId}/validations/reorder
        /// </summary>
        [HttpPut("fields/{fieldId}/validations/reorder")]
        public async Task<IActionResult> ReorderValidations(int fieldId, [FromBody] List<ReorderValidationDto> updates)
        {
            try
            {
                var success = await _formBuilderService.ReorderValidationsAsync(fieldId, updates);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Field not found" });
                }

                return Ok(new { success = true, message = "Validations reordered successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error reordering validations: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get all validation rules for a field
        /// GET /api/formbuilder/fields/{fieldId}/validations
        /// </summary>
        [HttpGet("fields/{fieldId}/validations")]
        public async Task<IActionResult> GetValidations(int fieldId)
        {
            try
            {
                var validations = await _formBuilderService.GetValidationsForFieldAsync(fieldId);

                return Ok(new { success = true, data = validations });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error getting validations: {ex.Message}" });
            }
        }
    }

    /// <summary>
    /// Request model for adding a section
    /// </summary>
    public class AddSectionRequest
    {
        public int TemplateId { get; set; }
        public CreateSectionDto Section { get; set; } = new();
    }

    /// <summary>
    /// Request model for reordering sections
    /// </summary>
    public class ReorderSectionsRequest
    {
        public int TemplateId { get; set; }
        public List<SectionOrderDto> Sections { get; set; } = new();
    }
}

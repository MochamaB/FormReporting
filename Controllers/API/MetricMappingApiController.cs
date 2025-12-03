using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Services.Metrics;
using FormReporting.Models.ViewModels.Metrics;
using FormReporting.Models.Entities.Forms;

namespace FormReporting.Controllers.API
{
    /// <summary>
    /// API Controller for Metric Mapping operations
    /// Handles AJAX requests from the metric mapping UI
    /// </summary>
    [ApiController]
    [Route("api/metric-mapping")]
    public class MetricMappingApiController : ControllerBase
    {
        private readonly IMetricMappingService _mappingService;
        private readonly ApplicationDbContext _context;

        public MetricMappingApiController(
            IMetricMappingService mappingService,
            ApplicationDbContext context)
        {
            _mappingService = mappingService;
            _context = context;
        }

        // ===================================================================
        // GET ENDPOINTS - Data Retrieval
        // ===================================================================

        /// <summary>
        /// Get all field data for a template (sections, fields, mapping status)
        /// Used to populate the left panel with collapsible sections
        /// </summary>
        [HttpGet("template/{templateId}/fields")]
        public async Task<IActionResult> GetTemplateFields(int templateId)
        {
            try
            {
                // Load template with all sections, items, and mappings
                var template = await _context.FormTemplates
                    .Include(t => t.Sections)
                        .ThenInclude(s => s.Items)
                            .ThenInclude(i => i.MetricMappings.Where(m => m.IsActive))
                                .ThenInclude(m => m.Metric)
                    .FirstOrDefaultAsync(t => t.TemplateId == templateId);

                if (template == null)
                    return NotFound(new { success = false, message = "Template not found" });

                // Build sections with fields and mapping status
                var sections = template.Sections
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new
                    {
                        sectionId = s.SectionId,
                        sectionName = s.SectionName,
                        sectionDescription = s.SectionDescription,
                        displayOrder = s.DisplayOrder,
                        fields = s.Items
                            .OrderBy(i => i.DisplayOrder)
                            .Select(i => new
                            {
                                itemId = i.ItemId,
                                itemName = i.ItemName,
                                itemCode = i.ItemCode,
                                dataType = i.DataType,
                                isMapped = i.MetricMappings.Any(),
                                mappingType = i.MetricMappings.FirstOrDefault()?.MappingType,
                                metricName = i.MetricMappings.FirstOrDefault()?.Metric?.MetricName,
                                metricCode = i.MetricMappings.FirstOrDefault()?.Metric?.MetricCode,
                                statusIcon = GetStatusIcon(i.MetricMappings),
                                statusClass = GetStatusClass(i.MetricMappings)
                            }).ToList(),
                        mappedCount = s.Items.Count(i => i.MetricMappings.Any()),
                        totalFields = s.Items.Count
                    }).ToList();

                // Calculate totals from all sections
                var allItems = template.Sections.SelectMany(s => s.Items).ToList();
                var totalFields = allItems.Count;
                var mappedFields = allItems.Count(i => i.MetricMappings.Any());

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        templateId = template.TemplateId,
                        templateName = template.TemplateName,
                        sections = sections,
                        totalFields = totalFields,
                        mappedFields = mappedFields
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get mapping details for a specific field
        /// Used when clicking a field in the left panel
        /// </summary>
        [HttpGet("field/{fieldId}")]
        public async Task<IActionResult> GetFieldMapping(int fieldId)
        {
            try
            {
                // Get field details
                var field = await _context.FormTemplateItems
                    .Include(i => i.Section)
                    .Include(i => i.Options)
                    .FirstOrDefaultAsync(i => i.ItemId == fieldId);

                if (field == null)
                    return NotFound(new { success = false, message = "Field not found" });

                // Get existing mapping
                var mappings = await _mappingService.GetMappingsForItemAsync(fieldId);
                var mapping = mappings.FirstOrDefault();

                // Get available metrics for this field type
                var availableMetrics = await _mappingService.GetAvailableMetricsForFieldAsync(fieldId);

                return Ok(new
                {
                    success = true,
                    field = new
                    {
                        itemId = field.ItemId,
                        itemName = field.ItemName,
                        itemCode = field.ItemCode,
                        dataType = field.DataType,
                        sectionName = field.Section?.SectionName,
                        isRequired = field.IsRequired,
                        hasOptions = field.Options.Any(),
                        options = field.Options.Select(o => new
                        {
                            optionId = o.OptionId,
                            optionLabel = o.OptionLabel,
                            optionValue = o.OptionValue,
                            scoreValue = o.ScoreValue
                        }).ToList()
                    },
                    mapping = mapping,
                    availableMetrics = availableMetrics,
                    isMapped = mapping != null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get unmapped fields for a template
        /// </summary>
        [HttpGet("template/{templateId}/unmapped")]
        public async Task<IActionResult> GetUnmappedFields(int templateId)
        {
            try
            {
                var unmapped = await _mappingService.GetUnmappedFieldsAsync(templateId);
                return Ok(new { success = true, data = unmapped, count = unmapped.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get all active metrics for dropdown selection
        /// </summary>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetActiveMetrics()
        {
            try
            {
                var metrics = await _context.MetricDefinitions
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.MetricName)
                    .Select(m => new
                    {
                        metricId = m.MetricId,
                        metricCode = m.MetricCode,
                        metricName = m.MetricName,
                        category = m.Category,
                        dataType = m.DataType,
                        unit = m.Unit,
                        sourceType = m.SourceType,
                        description = m.Description
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = metrics, count = metrics.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ===================================================================
        // POST ENDPOINTS - Create Operations
        // ===================================================================

        /// <summary>
        /// Create a new metric mapping
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateMapping([FromBody] CreateMappingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { success = false, message = "Invalid data", errors });
                }

                // Validate mapping
                var (isValid, validationErrors) = await _mappingService.ValidateMappingAsync(dto);
                if (!isValid)
                    return BadRequest(new { success = false, message = "Validation failed", errors = validationErrors });

                // Create mapping
                var mapping = await _mappingService.CreateMappingAsync(dto);

                return Ok(new
                {
                    success = true,
                    message = "Mapping created successfully",
                    mappingId = mapping.MappingId,
                    data = new
                    {
                        mappingId = mapping.MappingId,
                        itemId = mapping.ItemId,
                        metricId = mapping.MetricId,
                        mappingType = mapping.MappingType
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ===================================================================
        // PUT ENDPOINTS - Update Operations
        // ===================================================================

        /// <summary>
        /// Update an existing metric mapping
        /// </summary>
        [HttpPut("update/{mappingId}")]
        public async Task<IActionResult> UpdateMapping(int mappingId, [FromBody] UpdateMappingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { success = false, message = "Invalid data", errors });
                }

                var success = await _mappingService.UpdateMappingAsync(mappingId, dto);

                if (!success)
                    return NotFound(new { success = false, message = "Mapping not found" });

                return Ok(new { success = true, message = "Mapping updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ===================================================================
        // DELETE ENDPOINTS - Delete Operations
        // ===================================================================

        /// <summary>
        /// Delete a metric mapping
        /// </summary>
        [HttpDelete("delete/{mappingId}")]
        public async Task<IActionResult> DeleteMapping(int mappingId)
        {
            try
            {
                var success = await _mappingService.DeleteMappingAsync(mappingId);

                if (!success)
                    return NotFound(new { success = false, message = "Mapping not found" });

                return Ok(new { success = true, message = "Mapping deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ===================================================================
        // POST ENDPOINTS - Testing & Validation
        // ===================================================================

        /// <summary>
        /// Test a mapping with sample values
        /// </summary>
        [HttpPost("test/{mappingId}")]
        public async Task<IActionResult> TestMapping(int mappingId, [FromBody] Dictionary<int, string> sampleValues)
        {
            try
            {
                var (success, result, error) = await _mappingService.TestMappingAsync(mappingId, sampleValues);

                return Ok(new
                {
                    success = success,
                    result = result,
                    error = error,
                    message = success ? "Test completed successfully" : "Test failed"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Validate a formula before creating mapping
        /// </summary>
        [HttpPost("validate-formula")]
        public IActionResult ValidateFormula([FromBody] FormulaBuilderDto formulaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { success = false, message = "Invalid formula data", errors });
                }

                // Basic validation
                if (string.IsNullOrWhiteSpace(formulaDto.Formula))
                    return BadRequest(new { success = false, message = "Formula cannot be empty" });

                if (!formulaDto.SourceItemIds.Any())
                    return BadRequest(new { success = false, message = "Source items are required" });

                if (!formulaDto.ItemAliases.Any())
                    return BadRequest(new { success = false, message = "Item aliases are required for formula variables" });

                // Check if all source items have aliases
                var missingAliases = formulaDto.SourceItemIds
                    .Where(id => !formulaDto.ItemAliases.Values.Contains(id))
                    .ToList();

                if (missingAliases.Any())
                    return BadRequest(new { success = false, message = "All source items must have aliases defined" });

                return Ok(new
                {
                    success = true,
                    message = "Formula is valid",
                    formula = formulaDto.Formula,
                    variables = formulaDto.ItemAliases.Keys.ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ===================================================================
        // HELPER METHODS
        // ===================================================================

        /// <summary>
        /// Get status icon based on mapping type
        /// </summary>
        private string GetStatusIcon(ICollection<FormItemMetricMapping> mappings)
        {
            if (!mappings.Any())
                return "ri-alert-line"; // Unmapped

            var mapping = mappings.First();
            return mapping.MappingType switch
            {
                "Direct" => "ri-check-line",
                "SystemCalculated" => "ri-function-line",
                "BinaryCompliance" => "ri-checkbox-circle-line",
                "Derived" => "ri-git-merge-line",
                _ => "ri-link-line"
            };
        }

        /// <summary>
        /// Get status CSS class based on mapping type
        /// </summary>
        private string GetStatusClass(ICollection<FormItemMetricMapping> mappings)
        {
            if (!mappings.Any())
                return "text-warning"; // Unmapped

            var mapping = mappings.First();
            return mapping.MappingType switch
            {
                "Direct" => "text-success",
                "SystemCalculated" => "text-primary",
                "BinaryCompliance" => "text-info",
                "Derived" => "text-secondary",
                _ => "text-muted"
            };
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.Common;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.Entities.Metrics;
using FormReporting.Models.ViewModels.Metrics;
using FormReporting.Services.Metrics;

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
        private readonly IFieldMappingValidationService _validationService;
        private readonly ApplicationDbContext _context;

        public MetricMappingApiController(
            IMetricMappingService mappingService,
            IFieldMappingValidationService validationService,
            ApplicationDbContext context)
        {
            _mappingService = mappingService;
            _validationService = validationService;
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

        /// <summary>
        /// Get valid mapping types for a specific field type
        /// </summary>
        [HttpGet("field-type/{fieldType}/mapping-types")]
        public IActionResult GetValidMappingTypes(FormFieldType fieldType)
        {
            try
            {
                var validTypes = _validationService.GetValidMappingTypes(fieldType);
                var recommendedType = _validationService.GetRecommendedMappingType(fieldType);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        fieldType = fieldType.ToString(),
                        validMappingTypes = validTypes,
                        recommendedMappingType = recommendedType,
                        mappingTypeDescriptions = new Dictionary<string, string>
                        {
                            ["Direct"] = "Direct 1:1 value mapping from field response",
                            ["BinaryCompliance"] = "Check if field value matches expected value",
                            ["Calculated"] = "Calculate value using formula or aggregation",
                            ["Derived"] = "Value derived from other mappings or external sources"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get filtered metrics for field mapping based on field type and hierarchy
        /// </summary>
        [HttpGet("field/{fieldId}/compatible-metrics")]
        public async Task<IActionResult> GetCompatibleMetrics(int fieldId)
        {
            try
            {
                // Get field details
                var field = await _context.FormTemplateItems
                    .FirstOrDefaultAsync(i => i.ItemId == fieldId);

                if (field == null)
                    return NotFound(new { success = false, message = "Field not found" });

                // Get compatible metrics based on field type and hierarchy
                var metrics = await _context.MetricDefinitions
                    .Where(m => m.IsActive && 
                               m.MetricScope == "Field" && 
                               m.SourceType == "UserInput")
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
                        description = m.Description,
                        thresholdGreen = m.ThresholdGreen,
                        thresholdYellow = m.ThresholdYellow,
                        thresholdRed = m.ThresholdRed
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        fieldType = field.DataType,
                        compatibleMetrics = metrics,
                        count = metrics.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get field data for wizard (data only, no HTML - wizard rendering is handled by MVC controller)
        /// </summary>
        [HttpGet("field/{fieldId}/data")]
        public async Task<IActionResult> GetFieldDataForWizard(int fieldId)
        {
            try
            {
                // Get field information with all related data
                var field = await _context.FormTemplateItems
                    .Include(i => i.Section)
                    .Include(i => i.Options)
                    .Include(i => i.Template)
                    .FirstOrDefaultAsync(i => i.ItemId == fieldId);

                if (field == null)
                    return NotFound(new { success = false, message = "Field not found" });

                // Create field data object
                var fieldData = new
                {
                    itemId = field.ItemId,
                    itemName = field.ItemName,
                    itemCode = field.ItemCode,
                    dataType = field.DataType.ToString(),
                    sectionName = field.Section?.SectionName,
                    templateId = field.TemplateId,
                    templateName = field.Template?.TemplateName,
                    isRequired = field.IsRequired,
                    hasOptions = field.Options.Any(),
                    options = field.Options.Select(o => new
                    {
                        optionId = o.OptionId,
                        optionText = o.OptionLabel,
                        scoreValue = o.ScoreValue
                    }).ToList()
                };

                return Ok(new
                {
                    success = true,
                    data = fieldData
                });
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
        /// Create a new MetricDefinition from field mapping modal
        /// </summary>
        [HttpPost("create-metric")]
        public async Task<IActionResult> CreateMetricDefinition([FromBody] CreateMetricDefinitionDto dto)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(dto.MetricName))
                    return BadRequest(new { success = false, message = "Metric name is required" });

                // Generate metric code if not provided - include template context if available
                var metricCode = !string.IsNullOrWhiteSpace(dto.MetricCode) 
                    ? dto.MetricCode 
                    : GenerateMetricCode(dto.MetricName, dto.TemplateId);

                // Check if metric code already exists
                var existingMetric = await _context.MetricDefinitions
                    .FirstOrDefaultAsync(m => m.MetricCode == metricCode);

                if (existingMetric != null)
                    return BadRequest(new { success = false, message = $"Metric code '{metricCode}' already exists" });

                // Create new metric definition
                var metric = new MetricDefinition
                {
                    MetricCode = metricCode,
                    MetricName = dto.MetricName,
                    Description = dto.Description,
                    DataType = dto.DataType ?? "Decimal",
                    Unit = dto.Unit,
                    Category = dto.Category ?? "Performance",
                    MetricScope = "Field", // Always Field for field mappings
                    SourceType = "UserInput", // Always UserInput for field mappings
                    HierarchyLevel = 0, // Field level is 0
                    ThresholdGreen = dto.ThresholdGreen,
                    ThresholdYellow = dto.ThresholdYellow,
                    ThresholdRed = dto.ThresholdRed,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.MetricDefinitions.Add(metric);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Metric definition created successfully",
                    data = new
                    {
                        metricId = metric.MetricId,
                        metricCode = metric.MetricCode,
                        metricName = metric.MetricName,
                        category = metric.Category,
                        dataType = metric.DataType,
                        unit = metric.Unit
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new field metric mapping
        /// </summary>
        [HttpPost("field/create")]
        public async Task<IActionResult> CreateFieldMapping([FromBody] CreateFieldMappingDto dto)
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
                var (isValid, validationErrors) = await _mappingService.ValidateFieldMappingAsync(dto);
                if (!isValid)
                    return BadRequest(new { success = false, message = "Validation failed", errors = validationErrors });

                // Create mapping
                var mapping = await _mappingService.CreateFieldMappingAsync(dto);

                return Ok(new
                {
                    success = true,
                    message = "Field mapping created successfully",
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
        // SECTION LEVEL ENDPOINTS
        // ===================================================================

        /// <summary>
        /// Get all section mappings for a template
        /// </summary>
        [HttpGet("template/{templateId}/sections")]
        public async Task<IActionResult> GetTemplateSections(int templateId)
        {
            try
            {
                var sections = await _context.FormTemplateSections
                    .Include(s => s.Items)
                        .ThenInclude(i => i.MetricMappings.Where(m => m.IsActive))
                    .Where(s => s.TemplateId == templateId)
                    .OrderBy(s => s.DisplayOrder)
                    .ToListAsync();

                var sectionMappings = await _context.FormSectionMetricMappings
                    .Include(m => m.Metric)
                    .Include(m => m.Sources)
                        .ThenInclude(s => s.ItemMapping)
                            .ThenInclude(im => im.Item)
                    .Where(m => m.Section.TemplateId == templateId && m.IsActive)
                    .ToListAsync();

                var result = sections.Select(s => new
                {
                    sectionId = s.SectionId,
                    sectionName = s.SectionName,
                    displayOrder = s.DisplayOrder,
                    totalFields = s.Items.Count,
                    mappedFields = s.Items.Count(i => i.MetricMappings.Any()),
                    hasMapping = sectionMappings.Any(m => m.SectionId == s.SectionId),
                    mapping = sectionMappings.FirstOrDefault(m => m.SectionId == s.SectionId) is var mapping && mapping != null
                        ? new
                        {
                            mappingId = mapping.MappingId,
                            mappingName = mapping.MappingName,
                            aggregationType = mapping.AggregationType,
                            metricName = mapping.Metric?.MetricName,
                            metricCode = mapping.Metric?.MetricCode,
                            sourceCount = mapping.Sources.Count
                        }
                        : null
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get section mapping details
        /// </summary>
        [HttpGet("section/{sectionId}")]
        public async Task<IActionResult> GetSectionMapping(int sectionId)
        {
            try
            {
                var section = await _context.FormTemplateSections
                    .Include(s => s.Items)
                        .ThenInclude(i => i.MetricMappings.Where(m => m.IsActive))
                            .ThenInclude(m => m.Metric)
                    .FirstOrDefaultAsync(s => s.SectionId == sectionId);

                if (section == null)
                    return NotFound(new { success = false, message = "Section not found" });

                var mapping = await _context.FormSectionMetricMappings
                    .Include(m => m.Metric)
                    .Include(m => m.Sources)
                        .ThenInclude(s => s.ItemMapping)
                            .ThenInclude(im => im.Item)
                    .FirstOrDefaultAsync(m => m.SectionId == sectionId && m.IsActive);

                var availableFieldMappings = section.Items
                    .SelectMany(i => i.MetricMappings)
                    .Select(m => new
                    {
                        mappingId = m.MappingId,
                        itemId = m.ItemId,
                        itemName = m.Item?.ItemName,
                        metricName = m.Metric?.MetricName,
                        mappingType = m.MappingType
                    }).ToList();

                return Ok(new
                {
                    success = true,
                    section = new
                    {
                        sectionId = section.SectionId,
                        sectionName = section.SectionName,
                        totalFields = section.Items.Count,
                        mappedFields = section.Items.Count(i => i.MetricMappings.Any())
                    },
                    mapping = mapping != null ? new
                    {
                        mappingId = mapping.MappingId,
                        mappingName = mapping.MappingName,
                        aggregationType = mapping.AggregationType,
                        metricId = mapping.MetricId,
                        metricName = mapping.Metric?.MetricName,
                        sources = mapping.Sources.Select(s => new
                        {
                            sourceId = s.Id,
                            itemMappingId = s.ItemMappingId,
                            itemName = s.ItemMapping?.Item?.ItemName,
                            weight = s.Weight,
                            displayOrder = s.DisplayOrder
                        }).ToList()
                    } : null,
                    availableFieldMappings = availableFieldMappings,
                    hasMapping = mapping != null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ===================================================================
        // TEMPLATE LEVEL ENDPOINTS
        // ===================================================================

        /// <summary>
        /// Get all template KPIs for a template
        /// </summary>
        [HttpGet("template/{templateId}/kpis")]
        public async Task<IActionResult> GetTemplateKpis(int templateId)
        {
            try
            {
                var templateMappings = await _context.FormTemplateMetricMappings
                    .Include(m => m.Metric)
                    .Include(m => m.Sources)
                        .ThenInclude(s => s.SectionMapping)
                            .ThenInclude(sm => sm.Section)
                    .Where(m => m.TemplateId == templateId && m.IsActive)
                    .ToListAsync();

                var sectionMappings = await _context.FormSectionMetricMappings
                    .Include(m => m.Section)
                    .Include(m => m.Metric)
                    .Where(m => m.Section.TemplateId == templateId && m.IsActive)
                    .ToListAsync();

                var result = new
                {
                    templateKpis = templateMappings.Select(m => new
                    {
                        mappingId = m.MappingId,
                        kpiName = m.MappingName,
                        aggregationType = m.AggregationType,
                        metricName = m.Metric?.MetricName,
                        metricCode = m.Metric?.MetricCode,
                        sourceCount = m.Sources.Count,
                        sources = m.Sources.Select(s => new
                        {
                            sourceId = s.Id,
                            sectionName = s.SectionMapping?.Section?.SectionName,
                            weight = s.Weight
                        }).ToList()
                    }).ToList(),
                    availableSectionMappings = sectionMappings.Select(m => new
                    {
                        mappingId = m.MappingId,
                        sectionId = m.SectionId,
                        sectionName = m.Section?.SectionName,
                        mappingName = m.MappingName,
                        aggregationType = m.AggregationType,
                        metricName = m.Metric?.MetricName
                    }).ToList()
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get template KPI details
        /// </summary>
        [HttpGet("template-kpi/{mappingId}")]
        public async Task<IActionResult> GetTemplateKpiDetails(int mappingId)
        {
            try
            {
                var mapping = await _context.FormTemplateMetricMappings
                    .Include(m => m.Template)
                    .Include(m => m.Metric)
                    .Include(m => m.Sources)
                        .ThenInclude(s => s.SectionMapping)
                            .ThenInclude(sm => sm.Section)
                    .FirstOrDefaultAsync(m => m.MappingId == mappingId);

                if (mapping == null)
                    return NotFound(new { success = false, message = "Template KPI not found" });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        mappingId = mapping.MappingId,
                        templateId = mapping.TemplateId,
                        kpiName = mapping.MappingName,
                        aggregationType = mapping.AggregationType,
                        metricId = mapping.MetricId,
                        metricName = mapping.Metric?.MetricName,
                        metricCode = mapping.Metric?.MetricCode,
                        sources = mapping.Sources.Select(s => new
                        {
                            sourceId = s.Id,
                            sectionMappingId = s.SectionMappingId,
                            sectionName = s.SectionMapping?.Section?.SectionName,
                            weight = s.Weight,
                            displayOrder = s.DisplayOrder
                        }).ToList()
                    }
                });
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

        /// <summary>
        /// Save field mapping (standalone, link to metric, or create new metric)
        /// </summary>
        [HttpPost("field/save")]
        public async Task<IActionResult> SaveFieldMapping([FromBody] CreateFieldMappingDto dto)
        {
            try
            {
                // Validate required fields
                if (dto.ItemId <= 0)
                    return BadRequest(new { success = false, message = "Invalid field ID" });

                if (string.IsNullOrWhiteSpace(dto.MappingType))
                    return BadRequest(new { success = false, message = "Mapping type is required" });

                // Validate mapping type is valid for field type
                var field = await _context.FormTemplateItems
                    .FirstOrDefaultAsync(i => i.ItemId == dto.ItemId);

                if (field == null)
                    return NotFound(new { success = false, message = "Field not found" });

                // Convert string to FormFieldType enum for validation
                if (Enum.TryParse<FormFieldType>(field.DataType.ToString(), out var fieldType))
                {
                    if (!_validationService.IsValidMappingType(fieldType, dto.MappingType))
                        return BadRequest(new { success = false, message = $"Mapping type '{dto.MappingType}' is not valid for field type '{field.DataType}'" });
                }

                // Check if mapping already exists
                var existingMapping = await _context.FormItemMetricMappings
                    .FirstOrDefaultAsync(m => m.ItemId == dto.ItemId && m.IsActive);

                FormItemMetricMapping mapping;

                if (existingMapping != null)
                {
                    // Update existing mapping
                    mapping = existingMapping;
                    mapping.MetricId = dto.MetricId;
                    mapping.MappingName = dto.MappingName;
                    mapping.MappingType = dto.MappingType;
                    mapping.AggregationType = dto.AggregationType ?? "Direct";
                    mapping.TransformationLogic = dto.TransformationLogic;
                    mapping.ExpectedValue = dto.ExpectedValue;
                    // Note: FormItemMetricMapping doesn't have ModifiedDate property
                }
                else
                {
                    // Create new mapping
                    mapping = new FormItemMetricMapping
                    {
                        ItemId = dto.ItemId,
                        MetricId = dto.MetricId,
                        MappingName = dto.MappingName,
                        MappingType = dto.MappingType,
                        AggregationType = dto.AggregationType ?? "Direct",
                        TransformationLogic = dto.TransformationLogic,
                        ExpectedValue = dto.ExpectedValue,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.FormItemMetricMappings.Add(mapping);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Field mapping saved successfully",
                    data = new
                    {
                        mappingId = mapping.MappingId,
                        itemId = mapping.ItemId,
                        metricId = mapping.MetricId,
                        mappingName = mapping.MappingName,
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
        // HELPER METHODS
        // ===================================================================

        /// <summary>
        /// Generate metric code from metric name with global context
        /// </summary>
        private string GenerateMetricCode(string metricName, int? templateId = null)
        {
            // Remove special characters and convert to uppercase
            var fieldConcept = new string(metricName
                .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                .ToArray())
                .Replace(" ", "_")
                .ToUpper();

            // Create global code with template context
            var code = templateId.HasValue 
                ? $"{fieldConcept}_TEMPLATE_{templateId.Value}"
                : fieldConcept;

            // Limit to 50 characters for global codes
            if (code.Length > 50)
                code = code.Substring(0, 50);

            return code;
        }

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

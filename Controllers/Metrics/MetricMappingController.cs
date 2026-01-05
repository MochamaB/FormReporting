using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.Common;
using FormReporting.Models.ViewModels.Metrics;
using FormReporting.Services.Metrics;
using Microsoft.AspNetCore.Authorization;

namespace FormReporting.Controllers.Metrics
{
    /// <summary>
    /// MVC Controller for Metric Mapping operations
    /// Handles page rendering, partial views, and form-based CRUD
    /// </summary>
    [Authorize]
    [Route("Metrics/Mapping")]
    public class MetricMappingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMetricMappingService _mappingService;
        private readonly IFieldMappingValidationService _validationService;

        public MetricMappingController(
            ApplicationDbContext context,
            IMetricMappingService mappingService,
            IFieldMappingValidationService validationService)
        {
            _context = context;
            _mappingService = mappingService;
            _validationService = validationService;
        }

        // ===================================================================
        // MAIN PAGES - Full Page Views
        // ===================================================================

        /// <summary>
        /// Configure Metrics Overview - HorizontalWizard showing Field/Section/Template levels
        /// </summary>
        [HttpGet("Configure/{templateId}")]
        public async Task<IActionResult> Index(int templateId)
        {
            // Get template with sections and items
            var template = await _context.FormTemplates
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Items)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
            {
                return NotFound();
            }

            // Get existing field mappings
            var fieldMappings = await _context.FormItemMetricMappings
                .Include(m => m.Item)
                .Include(m => m.Metric)
                .Where(m => m.Item.Section.TemplateId == templateId && m.IsActive)
                .ToListAsync();

            // Get existing section mappings
            var sectionMappings = await _context.FormSectionMetricMappings
                .Include(m => m.Section)
                .Include(m => m.Metric)
                .Include(m => m.Sources)
                    .ThenInclude(s => s.ItemMapping)
                .Where(m => m.Section.TemplateId == templateId && m.IsActive)
                .ToListAsync();

            // Get existing template mappings
            var templateMappings = await _context.FormTemplateMetricMappings
                .Include(m => m.Template)
                .Include(m => m.Metric)
                .Include(m => m.Sources)
                    .ThenInclude(s => s.SectionMapping)
                .Where(m => m.TemplateId == templateId && m.IsActive)
                .ToListAsync();

            // Create ViewModel
            var viewModel = new MetricConfigurationViewModel
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                TemplateCode = template.TemplateCode,
                CategoryName = template.Category?.CategoryName ?? "Uncategorized",
                
                // Field level data
                Sections = template.Sections
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new SectionMetricConfigViewModel
                    {
                        SectionId = s.SectionId,
                        SectionName = s.SectionName,
                        DisplayOrder = s.DisplayOrder,
                        Fields = s.Items
                            .OrderBy(i => i.DisplayOrder)
                            .Select(i => new FieldMetricConfigViewModel
                            {
                                ItemId = i.ItemId,
                                ItemName = i.ItemName,
                                ItemCode = i.ItemCode,
                                DataType = i.DataType,
                                IsRequired = i.IsRequired,
                                Mapping = fieldMappings.FirstOrDefault(m => m.ItemId == i.ItemId)
                            }).ToList(),
                        Mapping = sectionMappings.FirstOrDefault(m => m.SectionId == s.SectionId)
                    }).ToList(),
                
                // Template level data
                TemplateMappings = templateMappings
            };

            return View("~/Views/Forms/FormTemplates/MetricConfig/ConfigureMetrics.cshtml", viewModel);
        }

        /// <summary>
        /// Map Field - Full page wizard for mapping a single field
        /// </summary>
        [HttpGet("Configure/{templateId}/Field/{fieldId}")]
        public async Task<IActionResult> MapField(int templateId, int fieldId)
        {
            // Load field data with all related information
            var field = await _context.FormTemplateItems
                .Include(i => i.Section)
                .Include(i => i.Options)
                .Include(i => i.Template)
                .Include(i => i.MetricMappings.Where(m => m.IsActive))
                    .ThenInclude(m => m.Metric)
                .FirstOrDefaultAsync(i => i.ItemId == fieldId && i.TemplateId == templateId);

            if (field == null)
                return NotFound("Field not found");

            // Parse DataType string to FormFieldType enum
            var fieldType = FormFieldType.Text;
            if (!string.IsNullOrEmpty(field.DataType) && Enum.TryParse<FormFieldType>(field.DataType, out var parsedType))
            {
                fieldType = parsedType;
            }

            // Get valid mapping types for this field type
            var validMappingTypes = _validationService.GetValidMappingTypes(fieldType);
            var recommendedType = _validationService.GetRecommendedMappingType(fieldType);

            // Get compatible metrics for linking
            var compatibleMetrics = await _context.MetricDefinitions
                .Where(m => m.IsActive && m.MetricScope == "Field" && m.SourceType == "UserInput")
                .OrderBy(m => m.Category)
                .ThenBy(m => m.MetricName)
                .Select(m => new CompatibleMetricViewModel
                {
                    MetricId = m.MetricId,
                    MetricCode = m.MetricCode,
                    MetricName = m.MetricName,
                    Category = m.Category,
                    DataType = m.DataType,
                    Unit = m.Unit,
                    Description = m.Description
                })
                .ToListAsync();

            // Get existing mapping if any
            var existingMapping = field.MetricMappings.FirstOrDefault();

            // Create ViewModel
            var viewModel = new FieldMappingWizardViewModel
            {
                FieldId = field.ItemId,
                FieldName = field.ItemName ?? string.Empty,
                FieldCode = field.ItemCode ?? string.Empty,
                DataType = field.DataType ?? "Text",
                SectionName = field.Section?.SectionName,
                TemplateId = field.TemplateId,
                TemplateName = field.Template?.TemplateName,
                IsRequired = field.IsRequired,
                Options = field.Options?.Select(o => new FieldOptionViewModel
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionLabel ?? string.Empty,
                    ScoreValue = o.ScoreValue ?? 0
                }).ToList() ?? new List<FieldOptionViewModel>(),
                ValidMappingTypes = validMappingTypes?.Select(t => new MappingTypeOption
                {
                    Value = t,
                    Text = GetMappingTypeDisplayText(t),
                    Description = GetMappingTypeDescription(t),
                    IsRecommended = t == recommendedType
                }).ToList() ?? new List<MappingTypeOption>(),
                RecommendedMappingType = recommendedType,
                CompatibleMetrics = compatibleMetrics ?? new List<CompatibleMetricViewModel>(),
                ExistingMapping = existingMapping != null ? new ExistingMappingViewModel
                {
                    MappingId = existingMapping.MappingId,
                    MappingName = existingMapping.MappingName,
                    MappingType = existingMapping.MappingType,
                    AggregationType = existingMapping.AggregationType,
                    ExpectedValue = existingMapping.ExpectedValue,
                    MetricId = existingMapping.MetricId,
                    MetricName = existingMapping.Metric?.MetricName,
                    MetricCode = existingMapping.Metric?.MetricCode
                } : null
            };

            return View("~/Views/Forms/FormTemplates/MetricConfig/MapField.cshtml", viewModel);
        }

        /// <summary>
        /// Save Field Mapping - POST action for the field mapping wizard
        /// </summary>
        [HttpPost("Configure/{templateId}/Field/{fieldId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFieldMapping(int templateId, int fieldId, CreateFieldMappingDto request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(MapField), new { templateId, fieldId });
            }

            try
            {
                // Verify field exists and belongs to template
                var field = await _context.FormTemplateItems
                    .FirstOrDefaultAsync(i => i.ItemId == fieldId && i.TemplateId == templateId);

                if (field == null)
                {
                    TempData["Error"] = "Field not found.";
                    return RedirectToAction(nameof(Index), new { templateId });
                }

                int? metricId = null;

                // Handle metric option
                switch (request.MetricOption)
                {
                    case "create-new":
                        // Create new metric definition
                        var newMetric = new Models.Entities.Metrics.MetricDefinition
                        {
                            MetricName = request.NewMetricName ?? $"{field.ItemName} Metric",
                            MetricCode = request.NewMetricCode ?? GenerateMetricCode(request.NewMetricName ?? field.ItemName),
                            Description = request.NewMetricDescription,
                            Category = request.NewMetricCategory ?? "Performance",
                            DataType = request.NewMetricDataType ?? "Decimal",
                            Unit = request.NewMetricUnit ?? "Score",
                            MetricScope = "Field",
                            HierarchyLevel = 0, // Field level
                            SourceType = request.MappingType == "Calculated" ? "SystemCalculated" : "UserInput",
                            AggregationType = request.NewMetricAggregationType ?? request.AggregationType,
                            IsKPI = request.NewMetricIsKPI,
                            ThresholdGreen = request.NewMetricGreen,
                            ThresholdYellow = request.NewMetricYellow,
                            ThresholdRed = request.NewMetricRed,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        };
                        _context.MetricDefinitions.Add(newMetric);
                        await _context.SaveChangesAsync();
                        metricId = newMetric.MetricId;
                        break;

                    case "link-existing":
                        metricId = request.MetricId;
                        break;

                    case "standalone":
                    default:
                        metricId = null;
                        break;
                }

                // Check for existing mapping
                var existingMapping = await _context.FormItemMetricMappings
                    .FirstOrDefaultAsync(m => m.ItemId == fieldId && m.IsActive);

                if (existingMapping != null)
                {
                    // Update existing mapping
                    existingMapping.MappingName = request.MappingName;
                    existingMapping.MappingType = request.MappingType;
                    existingMapping.AggregationType = request.AggregationType;
                    existingMapping.ExpectedValue = request.ExpectedValue;
                    existingMapping.MetricId = metricId;
                    // Note: FormItemMetricMapping doesn't have ModifiedDate property
                }
                else
                {
                    // Create new mapping
                    var newMapping = new Models.Entities.Forms.FormItemMetricMapping
                    {
                        ItemId = fieldId,
                        MappingName = request.MappingName,
                        MappingType = request.MappingType,
                        AggregationType = request.AggregationType,
                        ExpectedValue = request.ExpectedValue,
                        MetricId = metricId,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.FormItemMetricMappings.Add(newMapping);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Field mapping for '{field.ItemName}' saved successfully.";
                return RedirectToAction(nameof(Index), new { templateId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error saving mapping: {ex.Message}";
                return RedirectToAction(nameof(MapField), new { templateId, fieldId });
            }
        }

        /// <summary>
        /// Generate metric code from name
        /// </summary>
        private string GenerateMetricCode(string name)
        {
            if (string.IsNullOrEmpty(name)) return "METRIC_" + DateTime.UtcNow.Ticks;
            
            return name
                .Replace(" ", "_")
                .ToUpperInvariant()
                .Substring(0, Math.Min(name.Length, 30));
        }

        // ===================================================================
        // PARTIAL RENDERING (AJAX) - For Modal Content (Legacy - to be removed)
        // ===================================================================

        /// <summary>
        /// Render the field mapping wizard partial for AJAX loading
        /// This is the key action that renders the proper wizard with circular progress indicators
        /// </summary>
        [HttpGet("RenderFieldMappingWizard/{fieldId}")]
        public async Task<IActionResult> RenderFieldMappingWizard(int fieldId)
        {
            try
            {
                // Load field data with all related information
                var field = await _context.FormTemplateItems
                .Include(i => i.Section)
                .Include(i => i.Options)
                .Include(i => i.Template)
                .Include(i => i.MetricMappings.Where(m => m.IsActive))
                    .ThenInclude(m => m.Metric)
                .FirstOrDefaultAsync(i => i.ItemId == fieldId);

            if (field == null)
                return NotFound("Field not found");

            // Parse DataType string to FormFieldType enum
            var fieldType = FormFieldType.Text; // Default
            if (!string.IsNullOrEmpty(field.DataType) && Enum.TryParse<FormFieldType>(field.DataType, out var parsedType))
            {
                fieldType = parsedType;
            }

            // Get valid mapping types for this field type
            var validMappingTypes = _validationService.GetValidMappingTypes(fieldType);
            var recommendedType = _validationService.GetRecommendedMappingType(fieldType);

            // Get compatible metrics for linking
            var compatibleMetrics = await _context.MetricDefinitions
                .Where(m => m.IsActive && m.MetricScope == "Field" && m.SourceType == "UserInput")
                .OrderBy(m => m.Category)
                .ThenBy(m => m.MetricName)
                .Select(m => new CompatibleMetricViewModel
                {
                    MetricId = m.MetricId,
                    MetricCode = m.MetricCode,
                    MetricName = m.MetricName,
                    Category = m.Category,
                    DataType = m.DataType,
                    Unit = m.Unit,
                    Description = m.Description
                })
                .ToListAsync();

            // Get existing mapping if any
            var existingMapping = field.MetricMappings.FirstOrDefault();

            // Create ViewModel with null-safe assignments
            var viewModel = new FieldMappingWizardViewModel
            {
                FieldId = field.ItemId,
                FieldName = field.ItemName ?? string.Empty,
                FieldCode = field.ItemCode ?? string.Empty,
                DataType = field.DataType ?? "Text",
                SectionName = field.Section?.SectionName,
                TemplateId = field.TemplateId,
                TemplateName = field.Template?.TemplateName,
                IsRequired = field.IsRequired,
                Options = field.Options?.Select(o => new FieldOptionViewModel
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionLabel ?? string.Empty,
                    ScoreValue = o.ScoreValue ?? 0
                }).ToList() ?? new List<FieldOptionViewModel>(),
                ValidMappingTypes = validMappingTypes?.Select(t => new MappingTypeOption
                {
                    Value = t,
                    Text = GetMappingTypeDisplayText(t),
                    Description = GetMappingTypeDescription(t),
                    IsRecommended = t == recommendedType
                }).ToList() ?? new List<MappingTypeOption>(),
                RecommendedMappingType = recommendedType,
                CompatibleMetrics = compatibleMetrics ?? new List<CompatibleMetricViewModel>(),
                ExistingMapping = existingMapping != null ? new ExistingMappingViewModel
                {
                    MappingId = existingMapping.MappingId,
                    MappingName = existingMapping.MappingName,
                    MappingType = existingMapping.MappingType,
                    AggregationType = existingMapping.AggregationType,
                    ExpectedValue = existingMapping.ExpectedValue,
                    MetricId = existingMapping.MetricId,
                    MetricName = existingMapping.Metric?.MetricName,
                    MetricCode = existingMapping.Metric?.MetricCode
                } : null
            };

            // Return the wizard partial view
            return PartialView("~/Views/Forms/FormTemplates/MetricConfig/_FieldMappingWizard.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                // Return error details for debugging
                return Content($"<div class='alert alert-danger'><strong>Error:</strong> {ex.Message}<br/><small>{ex.StackTrace}</small></div>", "text/html");
            }
        }

        /// <summary>
        /// Render the field information panel partial
        /// </summary>
        [HttpGet("RenderFieldInfoPanel/{fieldId}")]
        public async Task<IActionResult> RenderFieldInfoPanel(int fieldId)
        {
            var field = await _context.FormTemplateItems
                .Include(i => i.Section)
                .Include(i => i.Options)
                .FirstOrDefaultAsync(i => i.ItemId == fieldId);

            if (field == null)
                return NotFound("Field not found");

            var viewModel = new FieldMappingWizardViewModel
            {
                FieldId = field.ItemId,
                FieldName = field.ItemName,
                FieldCode = field.ItemCode,
                DataType = field.DataType.ToString(),
                SectionName = field.Section?.SectionName,
                IsRequired = field.IsRequired,
                Options = field.Options.Select(o => new FieldOptionViewModel
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionLabel,
                    ScoreValue = o.ScoreValue ?? 0
                }).ToList()
            };

            return PartialView("~/Views/Forms/FormTemplates/MetricConfig/_FieldInformationPanel.cshtml", viewModel);
        }

        // ===================================================================
        // HELPER METHODS
        // ===================================================================

        /// <summary>
        /// Get display text for mapping type
        /// </summary>
        private string GetMappingTypeDisplayText(string mappingType)
        {
            return mappingType switch
            {
                "Direct" => "Direct (1:1 Value)",
                "BinaryCompliance" => "Binary Compliance (Yes/No)",
                "Calculated" => "Calculated (Formula)",
                "Derived" => "Derived (Complex)",
                _ => mappingType
            };
        }

        /// <summary>
        /// Get description for mapping type
        /// </summary>
        private string GetMappingTypeDescription(string mappingType)
        {
            return mappingType switch
            {
                "Direct" => "Use the field value directly as the metric value",
                "BinaryCompliance" => "Check if field value matches expected value (1 or 0)",
                "Calculated" => "Calculate value using formula or aggregation",
                "Derived" => "Value derived from other mappings or external sources",
                _ => string.Empty
            };
        }
    }
}

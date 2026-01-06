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
                .Include(m => m.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(m => m.Unit)
                .Where(m => m.IsActive && m.MetricScope == "Field" && m.SourceType == "UserInput")
                .OrderBy(m => m.SubCategory.Category.CategoryName)
                .ThenBy(m => m.SubCategory.SubCategoryName)
                .ThenBy(m => m.MetricName)
                .Select(m => new CompatibleMetricViewModel
                {
                    MetricId = m.MetricId,
                    MetricCode = m.MetricCode,
                    MetricName = m.MetricName,
                    SubCategoryId = m.SubCategoryId,
                    SubCategoryName = m.SubCategory != null ? m.SubCategory.SubCategoryName : null,
                    CategoryId = m.SubCategory != null ? m.SubCategory.CategoryId : null,
                    CategoryName = m.SubCategory != null && m.SubCategory.Category != null ? m.SubCategory.Category.CategoryName : null,
                    DataType = m.DataType,
                    UnitId = m.UnitId,
                    UnitName = m.Unit != null ? m.Unit.UnitName : null,
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
                    ExpectedValue = existingMapping.ExpectedValue,
                    MetricId = existingMapping.MetricId,
                    MetricName = existingMapping.Metric?.MetricName,
                    MetricCode = existingMapping.Metric?.MetricCode
                } : null
            };

            // Populate ViewBag with MetricCategories and MetricUnits for dropdowns
            ViewBag.MetricCategories = await _context.MetricCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            ViewBag.MetricUnits = await _context.MetricUnits
                .Where(u => u.IsActive)
                .OrderBy(u => u.DisplayOrder)
                .ToListAsync();

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
                            SubCategoryId = request.NewMetricSubCategoryId,
                            DataType = request.NewMetricDataType ?? "Decimal",
                            UnitId = request.NewMetricUnitId,
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
        // SECTION MAPPING - Full Page Wizard
        // ===================================================================

        /// <summary>
        /// Map Section - Full page wizard for creating/editing section metric mappings
        /// </summary>
        [HttpGet("Configure/{templateId}/Section/{sectionId}")]
        public async Task<IActionResult> MapSection(int templateId, int sectionId)
        {
            // Load section with related data
            var section = await _context.FormTemplateSections
                .Include(s => s.Template)
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId && s.TemplateId == templateId);

            if (section == null)
            {
                TempData["Error"] = "Section not found.";
                return RedirectToAction(nameof(Index), new { templateId });
            }

            // Get field mappings in this section (available for aggregation)
            var fieldMappings = await _context.FormItemMetricMappings
                .Include(m => m.Item)
                .Include(m => m.Metric)
                .Where(m => m.Item.SectionId == sectionId && m.IsActive)
                .OrderBy(m => m.Item.DisplayOrder)
                .ToListAsync();

            // Get existing section mapping if any
            var existingMapping = await _context.FormSectionMetricMappings
                .Include(m => m.Metric)
                .Include(m => m.Sources)
                    .ThenInclude(s => s.ItemMapping)
                        .ThenInclude(im => im.Item)
                .FirstOrDefaultAsync(m => m.SectionId == sectionId && m.IsActive);

            // Get compatible metrics for linking (Section scope)
            var compatibleMetrics = await _context.MetricDefinitions
                .Include(m => m.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(m => m.Unit)
                .Where(m => m.IsActive && (m.MetricScope == "Section" || m.MetricScope == null))
                .OrderBy(m => m.SubCategory.Category.CategoryName)
                .ThenBy(m => m.SubCategory.SubCategoryName)
                .ThenBy(m => m.MetricName)
                .Select(m => new CompatibleMetricViewModel
                {
                    MetricId = m.MetricId,
                    MetricCode = m.MetricCode,
                    MetricName = m.MetricName,
                    SubCategoryId = m.SubCategoryId,
                    SubCategoryName = m.SubCategory != null ? m.SubCategory.SubCategoryName : null,
                    CategoryId = m.SubCategory != null ? m.SubCategory.CategoryId : null,
                    CategoryName = m.SubCategory != null && m.SubCategory.Category != null ? m.SubCategory.Category.CategoryName : null,
                    DataType = m.DataType,
                    UnitId = m.UnitId,
                    UnitName = m.Unit != null ? m.Unit.UnitName : null,
                    Description = m.Description
                })
                .ToListAsync();

            // Build ViewModel
            var viewModel = new SectionMappingWizardViewModel
            {
                SectionId = section.SectionId,
                SectionName = section.SectionName ?? string.Empty,
                TemplateId = section.TemplateId,
                TemplateName = section.Template?.TemplateName,
                DisplayOrder = section.DisplayOrder,
                AvailableFieldMappings = fieldMappings.Select(m => new FieldMappingSourceViewModel
                {
                    MappingId = m.MappingId,
                    ItemId = m.ItemId,
                    ItemName = m.Item?.ItemName ?? string.Empty,
                    ItemCode = m.Item?.ItemCode,
                    DataType = m.Item?.DataType ?? "Text",
                    MappingType = m.MappingType,
                    MappingName = m.MappingName,
                    MetricId = m.MetricId,
                    MetricName = m.Metric?.MetricName,
                    IsSelected = existingMapping?.Sources.Any(s => s.ItemMappingId == m.MappingId) ?? false,
                    Weight = existingMapping?.Sources.FirstOrDefault(s => s.ItemMappingId == m.MappingId)?.Weight,
                    DisplayOrder = existingMapping?.Sources.FirstOrDefault(s => s.ItemMappingId == m.MappingId)?.DisplayOrder ?? m.Item?.DisplayOrder ?? 0
                }).ToList(),
                CompatibleMetrics = compatibleMetrics,
                ExistingMapping = existingMapping != null ? new ExistingSectionMappingViewModel
                {
                    MappingId = existingMapping.MappingId,
                    MappingName = existingMapping.MappingName,
                    MappingType = existingMapping.MappingType,
                    AggregationType = existingMapping.AggregationType,
                    MetricId = existingMapping.MetricId,
                    MetricName = existingMapping.Metric?.MetricName,
                    MetricCode = existingMapping.Metric?.MetricCode,
                    Sources = existingMapping.Sources.Select(s => new SelectedSourceViewModel
                    {
                        ItemMappingId = s.ItemMappingId,
                        ItemName = s.ItemMapping?.Item?.ItemName ?? string.Empty,
                        Weight = s.Weight,
                        DisplayOrder = s.DisplayOrder
                    }).ToList()
                } : null
            };

            // Populate ViewBag with MetricCategories and MetricUnits for dropdowns
            ViewBag.MetricCategories = await _context.MetricCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            ViewBag.MetricUnits = await _context.MetricUnits
                .Where(u => u.IsActive)
                .OrderBy(u => u.DisplayOrder)
                .ToListAsync();

            return View("~/Views/Forms/FormTemplates/MetricConfig/MapSection.cshtml", viewModel);
        }

        /// <summary>
        /// Save Section Mapping - POST action for the section mapping wizard
        /// </summary>
        [HttpPost("Configure/{templateId}/Section/{sectionId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSectionMapping(int templateId, int sectionId, CreateSectionMappingDto request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(MapSection), new { templateId, sectionId });
            }

            try
            {
                // Verify section exists and belongs to template
                var section = await _context.FormTemplateSections
                    .FirstOrDefaultAsync(s => s.SectionId == sectionId && s.TemplateId == templateId);

                if (section == null)
                {
                    TempData["Error"] = "Section not found.";
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
                            MetricName = request.NewMetricName ?? $"{section.SectionName} Metric",
                            MetricCode = request.NewMetricCode ?? GenerateMetricCode(request.NewMetricName ?? section.SectionName ?? "Section"),
                            Description = request.NewMetricDescription,
                            SubCategoryId = request.NewMetricSubCategoryId,
                            DataType = request.NewMetricDataType ?? "Decimal",
                            UnitId = request.NewMetricUnitId,
                            MetricScope = "Section",
                            HierarchyLevel = 1, // Section level
                            SourceType = request.MappingType == "Calculated" ? "SystemCalculated" : "Aggregated",
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
                var existingMapping = await _context.FormSectionMetricMappings
                    .Include(m => m.Sources)
                    .FirstOrDefaultAsync(m => m.SectionId == sectionId && m.IsActive);

                if (existingMapping != null)
                {
                    // Update existing mapping
                    existingMapping.MappingName = request.MappingName;
                    existingMapping.MappingType = request.MappingType;
                    existingMapping.AggregationType = request.AggregationType;
                    existingMapping.MetricId = metricId;

                    // Remove old sources
                    _context.FormSectionMetricSources.RemoveRange(existingMapping.Sources);

                    // Add new sources
                    if (request.Sources != null && request.Sources.Any())
                    {
                        foreach (var source in request.Sources)
                        {
                            existingMapping.Sources.Add(new Models.Entities.Forms.FormSectionMetricSource
                            {
                                SectionMappingId = existingMapping.MappingId,
                                ItemMappingId = source.ItemMappingId,
                                Weight = source.Weight,
                                DisplayOrder = source.DisplayOrder
                            });
                        }
                    }
                }
                else
                {
                    // Create new mapping
                    var newMapping = new Models.Entities.Forms.FormSectionMetricMapping
                    {
                        SectionId = sectionId,
                        MappingName = request.MappingName,
                        MappingType = request.MappingType,
                        AggregationType = request.AggregationType,
                        MetricId = metricId,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    // Add sources
                    if (request.Sources != null && request.Sources.Any())
                    {
                        foreach (var source in request.Sources)
                        {
                            newMapping.Sources.Add(new Models.Entities.Forms.FormSectionMetricSource
                            {
                                ItemMappingId = source.ItemMappingId,
                                Weight = source.Weight,
                                DisplayOrder = source.DisplayOrder
                            });
                        }
                    }

                    _context.FormSectionMetricMappings.Add(newMapping);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Section mapping for '{section.SectionName}' saved successfully.";
                return RedirectToAction(nameof(Index), new { templateId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error saving mapping: {ex.Message}";
                return RedirectToAction(nameof(MapSection), new { templateId, sectionId });
            }
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
                .Include(m => m.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(m => m.Unit)
                .Where(m => m.IsActive && m.MetricScope == "Field" && m.SourceType == "UserInput")
                .OrderBy(m => m.SubCategory.Category.CategoryName)
                .ThenBy(m => m.SubCategory.SubCategoryName)
                .ThenBy(m => m.MetricName)
                .Select(m => new CompatibleMetricViewModel
                {
                    MetricId = m.MetricId,
                    MetricCode = m.MetricCode,
                    MetricName = m.MetricName,
                    SubCategoryId = m.SubCategoryId,
                    SubCategoryName = m.SubCategory != null ? m.SubCategory.SubCategoryName : null,
                    CategoryId = m.SubCategory != null ? m.SubCategory.CategoryId : null,
                    CategoryName = m.SubCategory != null && m.SubCategory.Category != null ? m.SubCategory.Category.CategoryName : null,
                    DataType = m.DataType,
                    UnitId = m.UnitId,
                    UnitName = m.Unit != null ? m.Unit.UnitName : null,
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

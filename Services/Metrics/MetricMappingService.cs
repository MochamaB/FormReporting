using FormReporting.Data;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Metrics;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Services.Metrics
{
    public class MetricMappingService : IMetricMappingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMetricDefinitionService _metricDefinitionService;

        public MetricMappingService(ApplicationDbContext context, IMetricDefinitionService metricDefinitionService)
        {
            _context = context;
            _metricDefinitionService = metricDefinitionService;
        }

        public async Task<List<MetricMappingViewModel>> GetMappingsForTemplateAsync(int templateId)
        {
            var mappings = await _context.FormItemMetricMappings
                .Include(m => m.Item)
                    .ThenInclude(i => i.Section)
                .Include(m => m.Metric)
                .Where(m => m.Item.TemplateId == templateId && m.IsActive)
                .OrderBy(m => m.Item.Section.DisplayOrder)
                .ThenBy(m => m.Item.DisplayOrder)
                .ToListAsync();

            return mappings.Select(MapToViewModel).ToList();
        }

        public async Task<List<MetricMappingViewModel>> GetMappingsForItemAsync(int itemId)
        {
            var mappings = await _context.FormItemMetricMappings
                .Include(m => m.Item)
                    .ThenInclude(i => i.Section)
                .Include(m => m.Metric)
                .Where(m => m.ItemId == itemId && m.IsActive)
                .ToListAsync();

            return mappings.Select(MapToViewModel).ToList();
        }

        public async Task<MetricMappingViewModel?> GetMappingByIdAsync(int mappingId)
        {
            var mapping = await _context.FormItemMetricMappings
                .Include(m => m.Item)
                    .ThenInclude(i => i.Section)
                .Include(m => m.Metric)
                .FirstOrDefaultAsync(m => m.MappingId == mappingId);

            return mapping != null ? MapToViewModel(mapping) : null;
        }

        public async Task<MetricMappingViewModel> CreateFieldMappingAsync(CreateFieldMappingDto dto)
        {
            // Validate
            var validation = await ValidateFieldMappingAsync(dto);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Invalid mapping: {string.Join(", ", validation.Errors)}");
            }

            // Check for duplicate mapping (only if linking to a metric)
            if (dto.MetricId.HasValue && await _context.FormItemMetricMappings.AnyAsync(m => 
                m.ItemId == dto.ItemId && m.MetricId == dto.MetricId && m.IsActive))
            {
                throw new InvalidOperationException("This field is already mapped to this metric");
            }

            var mapping = new FormItemMetricMapping
            {
                ItemId = dto.ItemId,
                MappingName = dto.MappingName,
                MetricId = dto.MetricId,
                MappingType = dto.MappingType,
                TransformationLogic = dto.TransformationLogic,
                ExpectedValue = dto.ExpectedValue,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.FormItemMetricMappings.Add(mapping);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var created = await _context.FormItemMetricMappings
                .Include(m => m.Item)
                    .ThenInclude(i => i.Section)
                .Include(m => m.Metric)
                .FirstAsync(m => m.MappingId == mapping.MappingId);

            return MapToViewModel(created);
        }

        public async Task<bool> UpdateMappingAsync(int mappingId, UpdateMappingDto dto)
        {
            var mapping = await _context.FormItemMetricMappings.FindAsync(mappingId);
            if (mapping == null)
                return false;

            if (dto.TransformationLogic != null)
                mapping.TransformationLogic = dto.TransformationLogic;

            if (dto.ExpectedValue != null)
                mapping.ExpectedValue = dto.ExpectedValue;

            if (dto.IsActive.HasValue)
                mapping.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMappingAsync(int mappingId)
        {
            var mapping = await _context.FormItemMetricMappings.FindAsync(mappingId);
            if (mapping == null)
                return false;

            // Soft delete
            mapping.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool IsValid, List<string> Errors)> ValidateFieldMappingAsync(CreateFieldMappingDto dto)
        {
            var errors = new List<string>();

            // Check if item exists
            var item = await _context.FormTemplateItems.FindAsync(dto.ItemId);
            if (item == null)
            {
                errors.Add("Field not found");
                return (false, errors);
            }

            // Check if metric exists (only if MetricId is provided)
            if (dto.MetricId.HasValue)
            {
                var metric = await _context.MetricDefinitions.FindAsync(dto.MetricId);
                if (metric == null)
                {
                    errors.Add("Metric not found");
                    return (false, errors);
                }
            }

            // Validate mapping type
            var validTypes = new[] { "Direct", "Calculated", "BinaryCompliance", "Derived" };
            if (!validTypes.Contains(dto.MappingType))
            {
                errors.Add($"Invalid mapping type. Must be one of: {string.Join(", ", validTypes)}");
            }

            // Type-specific validation
            if (dto.MappingType == "Calculated" && string.IsNullOrEmpty(dto.TransformationLogic))
            {
                errors.Add("Calculated mappings require TransformationLogic (formula)");
            }

            if (dto.MappingType == "BinaryCompliance" && string.IsNullOrEmpty(dto.ExpectedValue))
            {
                errors.Add("BinaryCompliance mappings require ExpectedValue");
            }

            // Validate formula if Calculated
            if (dto.MappingType == "Calculated" && !string.IsNullOrEmpty(dto.TransformationLogic))
            {
                try
                {
                    var formulaDto = FormulaBuilderDto.FromJson(dto.TransformationLogic);
                    if (formulaDto == null)
                    {
                        errors.Add("Invalid formula JSON structure");
                    }
                    else if (string.IsNullOrEmpty(formulaDto.Formula))
                    {
                        errors.Add("Formula cannot be empty");
                    }
                }
                catch
                {
                    errors.Add("Invalid TransformationLogic JSON");
                }
            }

            return (!errors.Any(), errors);
        }

        public async Task<(bool Success, decimal? Result, string? Error)> TestMappingAsync(
            int mappingId, Dictionary<int, string> sampleValues)
        {
            var mapping = await _context.FormItemMetricMappings
                .Include(m => m.Item)
                .FirstOrDefaultAsync(m => m.MappingId == mappingId);

            if (mapping == null)
                return (false, null, "Mapping not found");

            try
            {
                decimal? result = mapping.MappingType switch
                {
                    "Direct" => ParseDirectValue(sampleValues, mapping.ItemId),
                    "SystemCalculated" => await TestCalculatedMapping(mapping, sampleValues),
                    "BinaryCompliance" => TestBinaryCompliance(sampleValues, mapping),
                    _ => null
                };

                return (true, result, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public async Task<List<MetricDefinitionViewModel>> GetAvailableMetricsForFieldAsync(int itemId)
        {
            var item = await _context.FormTemplateItems.FindAsync(itemId);
            if (item == null)
                return new List<MetricDefinitionViewModel>();

            return await _metricDefinitionService.GetMetricsForFieldTypeAsync(item.DataType);
        }

        public async Task<List<UnmappedFieldViewModel>> GetUnmappedFieldsAsync(int templateId)
        {
            // Get all fields in template
            var allFields = await _context.FormTemplateItems
                .Include(i => i.Section)
                .Where(i => i.TemplateId == templateId && i.IsActive)
                .ToListAsync();

            // Get mapped field IDs
            var mappedFieldIds = await _context.FormItemMetricMappings
                .Where(m => m.Item.TemplateId == templateId && m.IsActive)
                .Select(m => m.ItemId)
                .Distinct()
                .ToListAsync();

            // Filter to unmapped fields
            var unmappedFields = allFields.Where(f => !mappedFieldIds.Contains(f.ItemId));

            var result = new List<UnmappedFieldViewModel>();
            foreach (var field in unmappedFields)
            {
                var suggested = await GetAvailableMetricsForFieldAsync(field.ItemId);
                
                result.Add(new UnmappedFieldViewModel
                {
                    ItemId = field.ItemId,
                    ItemName = field.ItemName,
                    ItemCode = field.ItemCode,
                    DataType = field.DataType,
                    SectionId = field.SectionId,
                    SectionName = field.Section?.SectionName ?? "",
                    DisplayOrder = field.DisplayOrder,
                    SuggestedMetrics = suggested.Take(5).ToList() // Top 5 suggestions
                });
            }

            return result.OrderBy(f => f.SectionName).ThenBy(f => f.DisplayOrder).ToList();
        }

        // Helper methods
        private static MetricMappingViewModel MapToViewModel(FormItemMetricMapping mapping)
        {
            return new MetricMappingViewModel
            {
                MappingId = mapping.MappingId,
                ItemId = mapping.ItemId,
                ItemName = mapping.Item?.ItemName ?? "",
                ItemCode = mapping.Item?.ItemCode ?? "",
                ItemDataType = mapping.Item?.DataType ?? "",
                SectionId = mapping.Item?.SectionId ?? 0,
                SectionName = mapping.Item?.Section?.SectionName,
                MetricId = mapping.MetricId,
                MetricCode = mapping.Metric?.MetricCode ?? "",
                MetricName = mapping.Metric?.MetricName ?? "",
                MetricDataType = mapping.Metric?.DataType ?? "",
                MetricUnit = mapping.Metric?.Unit?.UnitName,
                MetricCategory = mapping.Metric?.SubCategory?.Category?.CategoryName,
                MappingType = mapping.MappingType,
                TransformationLogic = mapping.TransformationLogic,
                ExpectedValue = mapping.ExpectedValue,
                IsActive = mapping.IsActive
            };
        }

        private static decimal? ParseDirectValue(Dictionary<int, string> values, int itemId)
        {
            if (!values.TryGetValue(itemId, out var value))
                return null;

            return decimal.TryParse(value, out var result) ? result : null;
        }

        private async Task<decimal?> TestCalculatedMapping(FormItemMetricMapping mapping, Dictionary<int, string> sampleValues)
        {
            if (string.IsNullOrEmpty(mapping.TransformationLogic))
                return null;

            var formulaDto = FormulaBuilderDto.FromJson(mapping.TransformationLogic);
            if (formulaDto == null)
                return null;

            // Build variables dictionary
            var variables = new Dictionary<string, decimal>();
            foreach (var alias in formulaDto.ItemAliases)
            {
                if (sampleValues.TryGetValue(alias.Value, out var valueStr) && 
                    decimal.TryParse(valueStr, out var value))
                {
                    variables[alias.Key] = value;
                }
                else
                {
                    throw new Exception($"Missing or invalid value for {alias.Key}");
                }
            }

            // Simple formula evaluation (for testing - production should use NCalc)
            var result = EvaluateSimpleFormula(formulaDto.Formula, variables);

            if (formulaDto.RoundTo.HasValue)
                result = Math.Round(result, formulaDto.RoundTo.Value);

            return result;
        }

        private static decimal? TestBinaryCompliance(Dictionary<int, string> values, FormItemMetricMapping mapping)
        {
            if (!values.TryGetValue(mapping.ItemId, out var value))
                return null;

            var isCompliant = string.Equals(value, mapping.ExpectedValue, StringComparison.OrdinalIgnoreCase);
            return isCompliant ? 100m : 0m;
        }

        private static decimal EvaluateSimpleFormula(string formula, Dictionary<string, decimal> variables)
        {
            // This is a SIMPLIFIED evaluator for testing
            // Production should use NCalc or similar library
            var expression = formula;

            foreach (var variable in variables)
            {
                expression = expression.Replace(variable.Key, variable.Value.ToString());
            }

            // Very basic evaluation - supports only simple arithmetic
            // TODO: Replace with NCalc for production
            try
            {
                var dataTable = new System.Data.DataTable();
                var result = dataTable.Compute(expression, "");
                return Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Formula evaluation failed: {ex.Message}");
            }
        }
    }
}

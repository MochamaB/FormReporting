using FormReporting.Data;
using FormReporting.Models.Entities.Metrics;
using FormReporting.Models.ViewModels.Metrics;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Services.Metrics
{
    public class MetricDefinitionService : IMetricDefinitionService
    {
        private readonly ApplicationDbContext _context;

        public MetricDefinitionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MetricDefinitionViewModel>> GetAllMetricsAsync()
        {
            return await _context.MetricDefinitions
                .Include(m => m.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(m => m.Unit)
                .Where(m => m.IsActive)
                .OrderBy(m => m.SubCategory.Category.CategoryName)
                .ThenBy(m => m.SubCategory.SubCategoryName)
                .ThenBy(m => m.MetricName)
                .Select(m => MapToViewModel(m))
                .ToListAsync();
        }

        public async Task<List<MetricDefinitionViewModel>> GetMetricsByCategoryAsync(string category)
        {
            return await _context.MetricDefinitions
                .Include(m => m.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(m => m.Unit)
                .Where(m => m.IsActive && m.SubCategory.Category.CategoryName == category)
                .OrderBy(m => m.SubCategory.SubCategoryName)
                .ThenBy(m => m.MetricName)
                .Select(m => MapToViewModel(m))
                .ToListAsync();
        }

        public async Task<List<MetricDefinitionViewModel>> GetKPIMetricsAsync()
        {
            return await _context.MetricDefinitions
                .Include(m => m.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(m => m.Unit)
                .Where(m => m.IsActive && m.IsKPI)
                .OrderBy(m => m.SubCategory.Category.CategoryName)
                .ThenBy(m => m.SubCategory.SubCategoryName)
                .ThenBy(m => m.MetricName)
                .Select(m => MapToViewModel(m))
                .ToListAsync();
        }

        public async Task<MetricDefinitionViewModel?> GetMetricByCodeAsync(string metricCode)
        {
            var metric = await _context.MetricDefinitions
                .FirstOrDefaultAsync(m => m.MetricCode == metricCode);

            return metric != null ? MapToViewModel(metric) : null;
        }

        public async Task<MetricDefinitionViewModel?> GetMetricByIdAsync(int metricId)
        {
            var metric = await _context.MetricDefinitions
                .FirstOrDefaultAsync(m => m.MetricId == metricId);

            return metric != null ? MapToViewModel(metric) : null;
        }

        public async Task<MetricDefinitionViewModel> CreateMetricAsync(CreateMetricDefinitionDto dto)
        {
            // Check for duplicate code
            if (await _context.MetricDefinitions.AnyAsync(m => m.MetricCode == dto.MetricCode))
            {
                throw new InvalidOperationException($"Metric code '{dto.MetricCode}' already exists");
            }

            var metric = new MetricDefinition
            {
                MetricCode = dto.MetricCode,
                MetricName = dto.MetricName,
                SubCategoryId = dto.SubCategoryId,
                Description = dto.Description,
                SourceType = dto.SourceType,
                DataType = dto.DataType,
                UnitId = dto.UnitId,
                AggregationType = dto.AggregationType,
                IsKPI = dto.IsKPI,
                ThresholdGreen = dto.ThresholdGreen,
                ThresholdYellow = dto.ThresholdYellow,
                ThresholdRed = dto.ThresholdRed,
                ExpectedValue = dto.ExpectedValue,
                ComplianceRule = dto.ComplianceRule,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.MetricDefinitions.Add(metric);
            await _context.SaveChangesAsync();

            return MapToViewModel(metric);
        }

        public async Task<bool> UpdateMetricThresholdsAsync(int metricId, UpdateMetricThresholdsDto dto)
        {
            var metric = await _context.MetricDefinitions.FindAsync(metricId);
            if (metric == null)
                return false;

            metric.ThresholdGreen = dto.ThresholdGreen;
            metric.ThresholdYellow = dto.ThresholdYellow;
            metric.ThresholdRed = dto.ThresholdRed;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMetricAsync(int metricId, UpdateMetricDefinitionDto dto)
        {
            var metric = await _context.MetricDefinitions.FindAsync(metricId);
            if (metric == null)
                return false;

            if (!string.IsNullOrEmpty(dto.MetricName))
                metric.MetricName = dto.MetricName;

            if (dto.Description != null)
                metric.Description = dto.Description;

            if (dto.SubCategoryId.HasValue)
                metric.SubCategoryId = dto.SubCategoryId.Value;

            if (dto.ThresholdGreen.HasValue)
                metric.ThresholdGreen = dto.ThresholdGreen;

            if (dto.ThresholdYellow.HasValue)
                metric.ThresholdYellow = dto.ThresholdYellow;

            if (dto.ThresholdRed.HasValue)
                metric.ThresholdRed = dto.ThresholdRed;

            if (dto.IsActive.HasValue)
                metric.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateMetricAsync(int metricId)
        {
            var metric = await _context.MetricDefinitions.FindAsync(metricId);
            if (metric == null)
                return false;

            metric.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MetricDefinitionViewModel>> GetMetricsForFieldTypeAsync(string fieldDataType)
        {
            // Map field types to compatible metric data types
            var compatibleMetricTypes = GetCompatibleMetricTypes(fieldDataType);

            return await _context.MetricDefinitions
                .Include(m => m.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(m => m.Unit)
                .Where(m => m.IsActive && compatibleMetricTypes.Contains(m.DataType))
                .OrderBy(m => m.SubCategory.Category.CategoryName)
                .ThenBy(m => m.SubCategory.SubCategoryName)
                .ThenBy(m => m.MetricName)
                .Select(m => MapToViewModel(m))
                .ToListAsync();
        }

        // Helper methods
        private static MetricDefinitionViewModel MapToViewModel(MetricDefinition metric)
        {
            return new MetricDefinitionViewModel
            {
                MetricId = metric.MetricId,
                MetricCode = metric.MetricCode,
                MetricName = metric.MetricName,
                SubCategoryId = metric.SubCategoryId,
                SubCategoryName = metric.SubCategory?.SubCategoryName,
                CategoryId = metric.SubCategory?.CategoryId,
                CategoryName = metric.SubCategory?.Category?.CategoryName,
                Description = metric.Description,
                SourceType = metric.SourceType,
                DataType = metric.DataType,
                UnitId = metric.UnitId,
                UnitName = metric.Unit?.UnitName,
                AggregationType = metric.AggregationType,
                IsKPI = metric.IsKPI,
                ThresholdGreen = metric.ThresholdGreen,
                ThresholdYellow = metric.ThresholdYellow,
                ThresholdRed = metric.ThresholdRed,
                ExpectedValue = metric.ExpectedValue,
                ComplianceRule = metric.ComplianceRule,
                IsActive = metric.IsActive
            };
        }

        private static List<string> GetCompatibleMetricTypes(string fieldDataType)
        {
            return fieldDataType switch
            {
                "Number" => new List<string> { "Integer", "Decimal", "Percentage" },
                "Decimal" => new List<string> { "Decimal", "Percentage" },
                "Text" => new List<string> { "Text" },
                "TextArea" => new List<string> { "Text" },
                "Date" => new List<string> { "Date" },
                "DateTime" => new List<string> { "DateTime", "Date" },
                "Checkbox" => new List<string> { "Boolean", "Percentage" },
                "Radio" => new List<string> { "Boolean", "Text", "Percentage" },
                "Dropdown" => new List<string> { "Text", "Boolean", "Percentage" },
                _ => new List<string> { "Text" }
            };
        }
    }
}

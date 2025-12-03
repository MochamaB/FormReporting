using FormReporting.Models.ViewModels.Metrics;

namespace FormReporting.Services.Metrics
{
    /// <summary>
    /// Service for managing form item to metric mappings
    /// </summary>
    public interface IMetricMappingService
    {
        /// <summary>
        /// Get all mappings for a form template
        /// </summary>
        Task<List<MetricMappingViewModel>> GetMappingsForTemplateAsync(int templateId);

        /// <summary>
        /// Get mappings for a specific form field
        /// </summary>
        Task<List<MetricMappingViewModel>> GetMappingsForItemAsync(int itemId);

        /// <summary>
        /// Get single mapping by ID
        /// </summary>
        Task<MetricMappingViewModel?> GetMappingByIdAsync(int mappingId);

        /// <summary>
        /// Create new field-to-metric mapping
        /// </summary>
        Task<MetricMappingViewModel> CreateMappingAsync(CreateMappingDto dto);

        /// <summary>
        /// Update existing mapping
        /// </summary>
        Task<bool> UpdateMappingAsync(int mappingId, UpdateMappingDto dto);

        /// <summary>
        /// Delete mapping
        /// </summary>
        Task<bool> DeleteMappingAsync(int mappingId);

        /// <summary>
        /// Validate mapping configuration (test formula, check field compatibility)
        /// </summary>
        Task<(bool IsValid, List<string> Errors)> ValidateMappingAsync(CreateMappingDto dto);

        /// <summary>
        /// Test mapping with sample values
        /// </summary>
        Task<(bool Success, decimal? Result, string? Error)> TestMappingAsync(int mappingId, Dictionary<int, string> sampleValues);

        /// <summary>
        /// Get available metrics for a field (based on field type compatibility)
        /// </summary>
        Task<List<MetricDefinitionViewModel>> GetAvailableMetricsForFieldAsync(int itemId);

        /// <summary>
        /// Get unmapped fields for a template
        /// </summary>
        Task<List<UnmappedFieldViewModel>> GetUnmappedFieldsAsync(int templateId);
    }
}

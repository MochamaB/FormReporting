using FormReporting.Models.ViewModels.Metrics;

namespace FormReporting.Services.Metrics
{
    /// <summary>
    /// Service for managing metric definitions (KPI catalog)
    /// </summary>
    public interface IMetricDefinitionService
    {
        /// <summary>
        /// Get all active metric definitions
        /// </summary>
        Task<List<MetricDefinitionViewModel>> GetAllMetricsAsync();

        /// <summary>
        /// Get metric definitions by category
        /// </summary>
        Task<List<MetricDefinitionViewModel>> GetMetricsByCategoryAsync(string category);

        /// <summary>
        /// Get only KPI metrics (IsKPI = true)
        /// </summary>
        Task<List<MetricDefinitionViewModel>> GetKPIMetricsAsync();

        /// <summary>
        /// Get metric by unique code
        /// </summary>
        Task<MetricDefinitionViewModel?> GetMetricByCodeAsync(string metricCode);

        /// <summary>
        /// Get metric by ID
        /// </summary>
        Task<MetricDefinitionViewModel?> GetMetricByIdAsync(int metricId);

        /// <summary>
        /// Create new metric definition
        /// </summary>
        Task<MetricDefinitionViewModel> CreateMetricAsync(CreateMetricDefinitionDto dto);

        /// <summary>
        /// Update metric thresholds
        /// </summary>
        Task<bool> UpdateMetricThresholdsAsync(int metricId, UpdateMetricThresholdsDto dto);

        /// <summary>
        /// Update metric details
        /// </summary>
        Task<bool> UpdateMetricAsync(int metricId, UpdateMetricDefinitionDto dto);

        /// <summary>
        /// Deactivate metric (soft delete)
        /// </summary>
        Task<bool> DeactivateMetricAsync(int metricId);

        /// <summary>
        /// Get metrics compatible with field type
        /// </summary>
        Task<List<MetricDefinitionViewModel>> GetMetricsForFieldTypeAsync(string fieldDataType);
    }
}

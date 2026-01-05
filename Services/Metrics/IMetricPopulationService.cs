using FormReporting.Models.Entities.Forms;

namespace FormReporting.Services.Metrics
{
    /// <summary>
    /// Service for auto-populating metrics from form submissions
    /// THIS IS THE CRITICAL ENGINE that transforms form data into metric values
    /// </summary>
    public interface IMetricPopulationService
    {
        /// <summary>
        /// Main engine: Populate all metrics from a form submission
        /// Called automatically after form submission
        /// </summary>
        Task PopulateMetricsFromSubmissionAsync(int submissionId);

        /// <summary>
        /// Process Direct mapping (1:1 field to metric)
        /// </summary>
        Task<decimal?> ProcessDirectMappingAsync(FormItemMetricMapping mapping, FormTemplateSubmission submission);

        /// <summary>
        /// Process Calculated mapping (formula from multiple fields)
        /// </summary>
        Task<decimal?> ProcessCalculatedMappingAsync(FormItemMetricMapping mapping, FormTemplateSubmission submission);

        /// <summary>
        /// Process Binary Compliance mapping (Yes/No → 100%/0%)
        /// </summary>
        Task<decimal?> ProcessBinaryComplianceMappingAsync(FormItemMetricMapping mapping, FormTemplateSubmission submission);

        /// <summary>
        /// Evaluate formula with variable substitution
        /// Example: "(operational / total) * 100" with {operational: 23, total: 25} → 92.0
        /// </summary>
        Task<decimal> EvaluateFormulaAsync(string formula, Dictionary<string, decimal> variables);

        /// <summary>
        /// Upsert metric value to TenantMetrics table
        /// Updates if exists for same (TenantId, MetricId, Period), inserts if new
        /// </summary>
        Task UpsertTenantMetricAsync(int tenantId, int? metricId, DateTime reportingPeriod, 
            decimal numericValue, string? textValue, string sourceType, int sourceReferenceId);

        /// <summary>
        /// Log successful metric population
        /// </summary>
        Task LogPopulationSuccessAsync(int submissionId, int? metricId, int mappingId, 
            int sourceItemId, string sourceValue, decimal calculatedValue, string? formula, int processingTimeMs);

        /// <summary>
        /// Log failed metric population
        /// </summary>
        Task LogPopulationErrorAsync(int submissionId, int? metricId, int mappingId, 
            int sourceItemId, string errorMessage, string? formula);

        /// <summary>
        /// Recalculate metrics for a submission (if mapping changed)
        /// </summary>
        Task RecalculateMetricsAsync(int submissionId);

        /// <summary>
        /// Get population log for a submission
        /// </summary>
        Task<List<MetricPopulationLog>> GetPopulationLogsAsync(int submissionId);
    }
}

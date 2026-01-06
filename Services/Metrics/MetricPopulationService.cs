using FormReporting.Data;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.Entities.Metrics;
using FormReporting.Models.ViewModels.Metrics;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FormReporting.Services.Metrics
{
    /// <summary>
    /// THE CRITICAL ENGINE: Auto-populates metrics from form submissions
    /// </summary>
    public class MetricPopulationService : IMetricPopulationService
    {
        private readonly ApplicationDbContext _context;

        public MetricPopulationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// MAIN ENGINE: Process all metric mappings for a submission
        /// </summary>
        public async Task PopulateMetricsFromSubmissionAsync(int submissionId)
        {
            var stopwatch = Stopwatch.StartNew();

            // Load submission with all responses
            var submission = await _context.FormTemplateSubmissions
                .Include(s => s.Responses)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
                throw new KeyNotFoundException($"Submission {submissionId} not found");

            // Skip metric population if no tenant (e.g., non-location forms like training feedback)
            if (!submission.TenantId.HasValue)
                return;

            // Get all active mappings for this template
            var mappings = await _context.FormItemMetricMappings
                .Include(m => m.Item)
                .Include(m => m.Metric)
                .Where(m => m.Item.TemplateId == submission.TemplateId && m.IsActive)
                .ToListAsync();

            if (!mappings.Any())
                return; // No mappings configured

            // Determine reporting period (first day of the month from submission date)
            var submittedDate = submission.SubmittedDate ?? DateTime.Now;
            var reportingPeriod = new DateTime(submittedDate.Year, submittedDate.Month, 1);

            // Process each mapping
            foreach (var mapping in mappings)
            {
                var mappingStopwatch = Stopwatch.StartNew();

                try
                {
                    decimal? metricValue = mapping.MappingType switch
                    {
                        "Direct" => await ProcessDirectMappingAsync(mapping, submission),
                        "SystemCalculated" => await ProcessCalculatedMappingAsync(mapping, submission),
                        "BinaryCompliance" => await ProcessBinaryComplianceMappingAsync(mapping, submission),
                        _ => null
                    };

                    if (metricValue.HasValue)
                    {
                        // Save to TenantMetrics
                        await UpsertTenantMetricAsync(
                            tenantId: submission.TenantId.Value, // Safe to use .Value here due to check above
                            metricId: mapping.MetricId,
                            reportingPeriod: reportingPeriod,
                            numericValue: metricValue.Value,
                            textValue: FormatMetricValue(metricValue.Value, mapping.Metric),
                            sourceType: mapping.MappingType,
                            sourceReferenceId: submissionId
                        );

                        // Log success
                        var sourceValue = GetSourceValue(submission, mapping.ItemId);
                        await LogPopulationSuccessAsync(
                            submissionId: submissionId,
                            metricId: mapping.MetricId,
                            mappingId: mapping.MappingId,
                            sourceItemId: mapping.ItemId,
                            sourceValue: sourceValue,
                            calculatedValue: metricValue.Value,
                            formula: ExtractFormula(mapping.TransformationLogic),
                            processingTimeMs: (int)mappingStopwatch.ElapsedMilliseconds
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue processing other mappings
                    await LogPopulationErrorAsync(
                        submissionId: submissionId,
                        metricId: mapping.MetricId,
                        mappingId: mapping.MappingId,
                        sourceItemId: mapping.ItemId,
                        errorMessage: ex.Message,
                        formula: ExtractFormula(mapping.TransformationLogic)
                    );
                }

                mappingStopwatch.Stop();
            }

            stopwatch.Stop();
            Console.WriteLine($"Populated {mappings.Count} metrics in {stopwatch.ElapsedMilliseconds}ms");
        }

        public async Task<decimal?> ProcessDirectMappingAsync(FormItemMetricMapping mapping, FormTemplateSubmission submission)
        {
            // Get response value for this item
            var response = submission.Responses?.FirstOrDefault(r => r.ItemId == mapping.ItemId);
            if (response == null)
                return null;

            // Try numeric value first
            if (response.NumericValue.HasValue)
                return response.NumericValue.Value;

            // Try boolean value
            if (response.BooleanValue.HasValue)
                return response.BooleanValue.Value ? 1m : 0m;

            // Try text value
            if (!string.IsNullOrWhiteSpace(response.TextValue))
            {
                // Try parsing as number
                if (decimal.TryParse(response.TextValue, out var value))
                    return value;

                // Convert Yes/No to 1/0
                if (string.Equals(response.TextValue, "Yes", StringComparison.OrdinalIgnoreCase))
                    return 1m;
                if (string.Equals(response.TextValue, "No", StringComparison.OrdinalIgnoreCase))
                    return 0m;
            }

            return null;
        }

        public async Task<decimal?> ProcessCalculatedMappingAsync(FormItemMetricMapping mapping, FormTemplateSubmission submission)
        {
            if (string.IsNullOrEmpty(mapping.TransformationLogic))
                throw new InvalidOperationException("SystemCalculated mapping requires TransformationLogic");

            var formulaDto = FormulaBuilderDto.FromJson(mapping.TransformationLogic);
            if (formulaDto == null)
                throw new InvalidOperationException("Invalid TransformationLogic JSON");

            // Build variables dictionary from source items
            var variables = new Dictionary<string, decimal>();

            foreach (var alias in formulaDto.ItemAliases)
            {
                var response = submission.Responses?.FirstOrDefault(r => r.ItemId == alias.Value);
                if (response == null)
                {
                    throw new InvalidOperationException($"Missing response for field {alias.Key} (ItemId: {alias.Value})");
                }

                decimal value;
                if (response.NumericValue.HasValue)
                {
                    value = response.NumericValue.Value;
                }
                else if (!string.IsNullOrWhiteSpace(response.TextValue) && decimal.TryParse(response.TextValue, out var parsedValue))
                {
                    value = parsedValue;
                }
                else
                {
                    throw new InvalidOperationException($"Invalid numeric value for {alias.Key} (ItemId: {alias.Value})");
                }

                variables[alias.Key] = value;
            }

            // Validate division by zero if configured
            if (formulaDto.ValidateDivisionByZero)
            {
                var denominators = ExtractDenominators(formulaDto.Formula, variables);
                if (denominators.Any(d => d == 0))
                {
                    throw new DivideByZeroException("Division by zero detected in formula");
                }
            }

            // Evaluate formula
            var result = await EvaluateFormulaAsync(formulaDto.Formula, variables);

            // Apply rounding
            if (formulaDto.RoundTo.HasValue)
                result = Math.Round(result, formulaDto.RoundTo.Value);

            // Validate range
            if (formulaDto.MinValue.HasValue && result < formulaDto.MinValue.Value)
                result = formulaDto.MinValue.Value;

            if (formulaDto.MaxValue.HasValue && result > formulaDto.MaxValue.Value)
                result = formulaDto.MaxValue.Value;

            return result;
        }

        public async Task<decimal?> ProcessBinaryComplianceMappingAsync(FormItemMetricMapping mapping, FormTemplateSubmission submission)
        {
            var response = submission.Responses?.FirstOrDefault(r => r.ItemId == mapping.ItemId);
            if (response == null)
                return null;

            var expectedValue = mapping.ExpectedValue ?? "Yes";
            string actualValue;

            // Get the actual value from the appropriate column
            if (response.BooleanValue.HasValue)
            {
                actualValue = response.BooleanValue.Value ? "Yes" : "No";
            }
            else if (!string.IsNullOrWhiteSpace(response.TextValue))
            {
                actualValue = response.TextValue;
            }
            else
            {
                return null;
            }

            // Check if response matches expected value (case-insensitive)
            var isCompliant = string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase);

            // Return 100% for compliant, 0% for non-compliant
            return isCompliant ? 100m : 0m;
        }

        public Task<decimal> EvaluateFormulaAsync(string formula, Dictionary<string, decimal> variables)
        {
            // Substitute variables in formula
            var expression = formula;
            foreach (var variable in variables)
            {
                expression = expression.Replace(variable.Key, variable.Value.ToString());
            }

            // Evaluate using DataTable.Compute (simple evaluator)
            // TODO: Replace with NCalc or DynamicExpresso for production
            try
            {
                var dataTable = new System.Data.DataTable();
                var result = dataTable.Compute(expression, "");
                return Task.FromResult(Convert.ToDecimal(result));
            }
            catch (DivideByZeroException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Formula evaluation failed: {ex.Message}");
            }
        }

        public async Task UpsertTenantMetricAsync(int tenantId, int? metricId, DateTime reportingPeriod,
            decimal numericValue, string? textValue, string sourceType, int sourceReferenceId)
        {
            // Check if metric value already exists for this tenant/metric/period
            var existing = await _context.TenantMetrics
                .FirstOrDefaultAsync(m =>
                    m.TenantId == tenantId &&
                    m.MetricId == metricId &&
                    m.ReportingPeriod == reportingPeriod);

            if (existing != null)
            {
                // Update existing
                existing.NumericValue = numericValue;
                existing.TextValue = textValue;
                existing.SourceType = sourceType;
                existing.SourceReferenceId = sourceReferenceId;
                existing.CapturedDate = DateTime.UtcNow;
            }
            else
            {
                // Insert new
                var metric = new TenantMetric
                {
                    TenantId = tenantId,
                    MetricId = metricId,
                    ReportingPeriod = reportingPeriod,
                    NumericValue = numericValue,
                    TextValue = textValue,
                    SourceType = sourceType,
                    SourceReferenceId = sourceReferenceId,
                    CapturedDate = DateTime.UtcNow
                };

                _context.TenantMetrics.Add(metric);
            }

            await _context.SaveChangesAsync();
        }

        public async Task LogPopulationSuccessAsync(int submissionId, int? metricId, int mappingId,
            int sourceItemId, string sourceValue, decimal calculatedValue, string? formula, int processingTimeMs)
        {
            var log = new MetricPopulationLog
            {
                SubmissionId = submissionId,
                MetricId = metricId,
                MappingId = mappingId,
                SourceItemId = sourceItemId,
                SourceValue = sourceValue,
                CalculatedValue = calculatedValue,
                CalculationFormula = formula,
                PopulatedDate = DateTime.UtcNow,
                Status = "Success",
                ProcessingTimeMs = processingTimeMs
            };

            _context.MetricPopulationLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogPopulationErrorAsync(int submissionId, int? metricId, int mappingId,
            int sourceItemId, string errorMessage, string? formula)
        {
            var log = new MetricPopulationLog
            {
                SubmissionId = submissionId,
                MetricId = metricId,
                MappingId = mappingId,
                SourceItemId = sourceItemId,
                CalculationFormula = formula,
                PopulatedDate = DateTime.UtcNow,
                Status = "Failed",
                ErrorMessage = errorMessage
            };

            _context.MetricPopulationLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task RecalculateMetricsAsync(int submissionId)
        {
            // Delete existing population logs for this submission
            var existingLogs = await _context.MetricPopulationLogs
                .Where(l => l.SubmissionId == submissionId)
                .ToListAsync();

            _context.MetricPopulationLogs.RemoveRange(existingLogs);
            await _context.SaveChangesAsync();

            // Repopulate
            await PopulateMetricsFromSubmissionAsync(submissionId);
        }

        public async Task<List<MetricPopulationLog>> GetPopulationLogsAsync(int submissionId)
        {
            return await _context.MetricPopulationLogs
                .Include(l => l.Metric)
                .Include(l => l.SourceItem)
                .Where(l => l.SubmissionId == submissionId)
                .OrderBy(l => l.PopulatedDate)
                .ToListAsync();
        }

        // Helper methods
        private static string? ExtractFormula(string? transformationLogic)
        {
            if (string.IsNullOrEmpty(transformationLogic))
                return null;

            try
            {
                var formulaDto = FormulaBuilderDto.FromJson(transformationLogic);
                return formulaDto?.Formula;
            }
            catch
            {
                return null;
            }
        }

        private string GetSourceValue(FormTemplateSubmission submission, int itemId)
        {
            var response = submission.Responses?.FirstOrDefault(r => r.ItemId == itemId);
            if (response == null)
                return "";

            // Return the appropriate value based on what's populated
            if (response.NumericValue.HasValue)
                return response.NumericValue.Value.ToString();
            if (response.BooleanValue.HasValue)
                return response.BooleanValue.Value ? "Yes" : "No";
            if (!string.IsNullOrWhiteSpace(response.TextValue))
                return response.TextValue;
            if (response.DateValue.HasValue)
                return response.DateValue.Value.ToString("yyyy-MM-dd");

            return "";
        }

        private static string? FormatMetricValue(decimal value, MetricDefinition? metric)
        {
            if (metric == null)
                return value.ToString();

            var unitName = metric.Unit?.UnitName;
            return unitName switch
            {
                "Percentage" => $"{value:F2}%",
                "Count" => value.ToString("N0"),
                "Status" => value == 1 ? "Yes" : "No",
                _ => value.ToString()
            };
        }

        private static List<decimal> ExtractDenominators(string formula, Dictionary<string, decimal> variables)
        {
            var denominators = new List<decimal>();

            // Simple denominator extraction for division operations
            // This is a simplified version - production should use proper expression parsing
            var parts = formula.Split('/');
            for (int i = 1; i < parts.Length; i++)
            {
                var denomPart = parts[i].Trim().Split(new[] { ' ', ')', '*', '+', '-' })[0];
                if (variables.TryGetValue(denomPart, out var value))
                {
                    denominators.Add(value);
                }
            }

            return denominators;
        }
    }
}

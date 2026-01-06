namespace FormReporting.Models.ViewModels.Metrics
{
    /// <summary>
    /// View model for metric definition display
    /// </summary>
    public class MetricDefinitionViewModel
    {
        public int MetricId { get; set; }
        public string MetricCode { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }

        // Source configuration
        public string SourceType { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? AggregationType { get; set; }

        // KPI configuration
        public bool IsKPI { get; set; }
        public decimal? ThresholdGreen { get; set; }
        public decimal? ThresholdYellow { get; set; }
        public decimal? ThresholdRed { get; set; }

        // For binary/compliance metrics
        public string? ExpectedValue { get; set; }
        public string? ComplianceRule { get; set; }

        public bool IsActive { get; set; }

        // Display helpers
        public string SourceTypeDisplay => SourceType switch
        {
            "UserInput" => "User Input",
            "SystemCalculated" => "System Calculated",
            "ExternalSystem" => "External System",
            "ComplianceTracking" => "Compliance Tracking",
            "AutomatedCheck" => "Automated Check",
            _ => SourceType
        };

        public string DataTypeDisplay => DataType switch
        {
            "Integer" => "Integer",
            "Decimal" => "Decimal",
            "Percentage" => "Percentage",
            "Boolean" => "Yes/No",
            "Text" => "Text",
            "Date" => "Date",
            "DateTime" => "Date & Time",
            _ => DataType
        };

        public string DisplayName => $"{MetricName} ({UnitName ?? DataType})";

        public bool HasThresholds => ThresholdGreen.HasValue || ThresholdYellow.HasValue;
    }
}

namespace FormReporting.Models.ViewModels.Metrics
{
    /// <summary>
    /// View model for field-to-metric mapping display
    /// </summary>
    public class MetricMappingViewModel
    {
        public int MappingId { get; set; }

        // Field information
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemDataType { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string? SectionName { get; set; }

        // Metric information
        public int? MetricId { get; set; }
        public string MetricCode { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public string MetricDataType { get; set; } = string.Empty;
        public string? MetricUnit { get; set; }
        public string? MetricCategory { get; set; }

        // Mapping configuration
        public string MappingType { get; set; } = string.Empty;
        public string? TransformationLogic { get; set; }
        public string? ExpectedValue { get; set; }
        public bool IsActive { get; set; }

        // Display helpers
        public string MappingTypeDisplay => MappingType switch
        {
            "Direct" => "Direct (1:1)",
            "SystemCalculated" => "Calculated (Formula)",
            "BinaryCompliance" => "Binary Compliance (Yes/No)",
            "Derived" => "Derived (Complex)",
            _ => MappingType
        };

        public bool HasFormula => MappingType == "SystemCalculated" || MappingType == "Derived";

        public string TransformationSummary
        {
            get
            {
                if (MappingType == "Direct")
                    return "No transformation";

                if (MappingType == "BinaryCompliance")
                    return $"Expected: {ExpectedValue ?? "Yes"} → 100%, Other → 0%";

                if (HasFormula && !string.IsNullOrEmpty(TransformationLogic))
                {
                    // Try to extract formula from JSON
                    try
                    {
                        var json = System.Text.Json.JsonDocument.Parse(TransformationLogic);
                        if (json.RootElement.TryGetProperty("formula", out var formulaProp))
                            return formulaProp.GetString() ?? "Formula defined";
                    }
                    catch { }
                }

                return "Custom logic";
            }
        }

        public string DisplaySummary => $"{ItemName} → {MetricName} ({MappingTypeDisplay})";
    }
}

namespace FormReporting.Models.ViewModels.Metrics
{
    /// <summary>
    /// View model for fields that are not mapped to metrics
    /// Used to show users which fields could be mapped
    /// </summary>
    public class UnmappedFieldViewModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        // Suggested metrics based on field type
        public List<MetricDefinitionViewModel> SuggestedMetrics { get; set; } = new();

        public string DataTypeDisplay => DataType switch
        {
            "Text" => "Text",
            "TextArea" => "Text Area",
            "Number" => "Number",
            "Decimal" => "Decimal",
            "Date" => "Date",
            "DateTime" => "Date & Time",
            "Dropdown" => "Dropdown",
            "Radio" => "Radio",
            "Checkbox" => "Checkbox",
            _ => DataType
        };
    }
}

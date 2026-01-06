namespace FormReporting.Models.ViewModels.Metrics
{
    /// <summary>
    /// ViewModel for the Section Mapping Wizard
    /// Contains all data needed to render the wizard and its partials
    /// </summary>
    public class SectionMappingWizardViewModel
    {
        // Section Information
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public int DisplayOrder { get; set; }
        
        // Field Mappings in this section (available for aggregation)
        public List<FieldMappingSourceViewModel> AvailableFieldMappings { get; set; } = new();
        public int TotalFieldMappings => AvailableFieldMappings.Count;
        public bool HasFieldMappings => AvailableFieldMappings.Any();
        
        // Existing Mapping (if editing)
        public ExistingSectionMappingViewModel? ExistingMapping { get; set; }
        
        // Valid Mapping Types for sections
        public List<MappingTypeOption> ValidMappingTypes { get; set; } = new()
        {
            new MappingTypeOption { Value = "Aggregated", Text = "Aggregated", Description = "Combine field values using aggregation function", IsRecommended = true },
            new MappingTypeOption { Value = "Calculated", Text = "Calculated", Description = "Custom formula combining field values" }
        };
        
        // Valid Aggregation Types for sections
        public List<AggregationTypeOption> ValidAggregationTypes { get; set; } = new()
        {
            new AggregationTypeOption { Value = "Average", Text = "Average", Description = "Average of all source field values" },
            new AggregationTypeOption { Value = "Sum", Text = "Sum", Description = "Sum of all source field values" },
            new AggregationTypeOption { Value = "Count", Text = "Count", Description = "Count of source fields" },
            new AggregationTypeOption { Value = "WeightedAverage", Text = "Weighted Average", Description = "Weighted average based on field importance" },
            new AggregationTypeOption { Value = "Min", Text = "Minimum", Description = "Minimum value among sources" },
            new AggregationTypeOption { Value = "Max", Text = "Maximum", Description = "Maximum value among sources" },
            new AggregationTypeOption { Value = "Percentage", Text = "Percentage", Description = "Percentage of compliant/positive values" }
        };
        
        // Compatible Metrics for linking
        public List<CompatibleMetricViewModel> CompatibleMetrics { get; set; } = new();
    }

    /// <summary>
    /// Field mapping available as source for section aggregation
    /// </summary>
    public class FieldMappingSourceViewModel
    {
        public int MappingId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public string DataType { get; set; } = string.Empty;
        public string MappingType { get; set; } = string.Empty;
        public string AggregationType { get; set; } = string.Empty;
        public string? MappingName { get; set; }
        public int? MetricId { get; set; }
        public string? MetricName { get; set; }
        
        // For pre-selection when editing
        public bool IsSelected { get; set; }
        public decimal? Weight { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Existing section mapping data if section is already mapped
    /// </summary>
    public class ExistingSectionMappingViewModel
    {
        public int MappingId { get; set; }
        public string? MappingName { get; set; }
        public string? MappingType { get; set; }
        public string? AggregationType { get; set; }
        public int? MetricId { get; set; }
        public string? MetricName { get; set; }
        public string? MetricCode { get; set; }
        public List<SelectedSourceViewModel> Sources { get; set; } = new();
    }

    /// <summary>
    /// Selected source in existing mapping
    /// </summary>
    public class SelectedSourceViewModel
    {
        public int ItemMappingId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal? Weight { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Aggregation type option with description
    /// </summary>
    public class AggregationTypeOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}

namespace FormReporting.Models.ViewModels.Metrics
{
    /// <summary>
    /// ViewModel for the Field Mapping Wizard
    /// Contains all data needed to render the wizard and its partials
    /// </summary>
    public class FieldMappingWizardViewModel
    {
        // Field Information
        public int FieldId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string FieldCode { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string? SectionName { get; set; }
        public int TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public bool IsRequired { get; set; }
        public bool HasOptions => Options.Any();
        
        // Field Options (for dropdowns, checkboxes, etc.)
        public List<FieldOptionViewModel> Options { get; set; } = new();
        
        // Existing Mapping (if any)
        public ExistingMappingViewModel? ExistingMapping { get; set; }
        
        // Available Mapping Types for this field type
        public List<MappingTypeOption> ValidMappingTypes { get; set; } = new();
        public string? RecommendedMappingType { get; set; }
        
        // Compatible Metrics for linking
        public List<CompatibleMetricViewModel> CompatibleMetrics { get; set; } = new();
    }

    /// <summary>
    /// Field option for dropdown/checkbox fields
    /// </summary>
    public class FieldOptionViewModel
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public decimal ScoreValue { get; set; }
    }

    /// <summary>
    /// Existing mapping data if field is already mapped
    /// </summary>
    public class ExistingMappingViewModel
    {
        public int MappingId { get; set; }
        public string? MappingName { get; set; }
        public string? MappingType { get; set; }
        public string? AggregationType { get; set; }
        public string? ExpectedValue { get; set; }
        public int? MetricId { get; set; }
        public string? MetricName { get; set; }
        public string? MetricCode { get; set; }
    }

    /// <summary>
    /// Mapping type option with description
    /// </summary>
    public class MappingTypeOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsRecommended { get; set; }
    }

    /// <summary>
    /// Compatible metric for linking
    /// </summary>
    public class CompatibleMetricViewModel
    {
        public int MetricId { get; set; }
        public string MetricCode { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? DataType { get; set; }
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? Description { get; set; }
    }
}

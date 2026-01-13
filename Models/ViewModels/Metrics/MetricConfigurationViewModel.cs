using FormReporting.Models.Entities.Forms;

namespace FormReporting.Models.ViewModels.Metrics
{
    /// <summary>
    /// ViewModel for hierarchical metric configuration page
    /// </summary>
    public class MetricConfigurationViewModel
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        // Field and Section level data
        public List<SectionMetricConfigViewModel> Sections { get; set; } = new List<SectionMetricConfigViewModel>();

        // Template level data
        public List<FormTemplateMetricMapping> TemplateMappings { get; set; } = new List<FormTemplateMetricMapping>();

        // Summary statistics
        public int TotalFields => Sections.SelectMany(s => s.Fields).Count();
        public int ConfiguredFields => Sections.SelectMany(s => s.Fields).Count(f => f.Mapping != null);
        public int TotalSections => Sections.Count;
        public int ConfiguredSections => Sections.Count(s => s.Mapping != null);
        public int ConfiguredTemplateKPIs => TemplateMappings.Count;

        // Progress percentages
        public decimal FieldProgress => TotalFields > 0 ? (decimal)ConfiguredFields / TotalFields * 100 : 0;
        public decimal SectionProgress => TotalSections > 0 ? (decimal)ConfiguredSections / TotalSections * 100 : 0;
    }

    /// <summary>
    /// ViewModel for section-level metric configuration
    /// </summary>
    public class SectionMetricConfigViewModel
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string SectionDescription { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        // Fields in this section
        public List<FieldMetricConfigViewModel> Fields { get; set; } = new List<FieldMetricConfigViewModel>();

        // Section-level mapping
        public FormSectionMetricMapping? Mapping { get; set; }

        // Helper properties
        public bool HasMapping => Mapping != null;
        public int ConfiguredFieldCount => Fields.Count(f => f.HasMappings);
        public int TotalFieldCount => Fields.Count;
        public bool CanConfigureSection => ConfiguredFieldCount > 0;
    }

    /// <summary>
    /// ViewModel for field-level metric configuration
    /// </summary>
    public class FieldMetricConfigViewModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }

        // Multiple field-level mappings
        public List<FormItemMetricMapping> Mappings { get; set; } = new List<FormItemMetricMapping>();

        // Settable property for backward compatibility - also updates Mappings list
        private FormItemMetricMapping? _mapping;
        public FormItemMetricMapping? Mapping
        {
            get => _mapping ?? Mappings.FirstOrDefault();
            set
            {
                _mapping = value;
                if (value != null && !Mappings.Contains(value))
                {
                    Mappings.Clear();
                    Mappings.Add(value);
                }
            }
        }

        // Helper properties
        public bool HasMappings => Mappings.Any() || _mapping != null;
        public bool HasMapping => HasMappings; // Backward compatibility
        public string MappingStatus => HasMappings ? "Configured" : "Not Mapped";
        public string MappingType => Mapping?.MappingType ?? "None";
        public string MetricName => Mapping?.Metric?.MetricName ?? "No Metric";
    }
}

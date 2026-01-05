using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Links template sections to metrics for automatic KPI tracking
    /// </summary>
    [Table("FormSectionMetricMappings")]
    public class FormSectionMetricMapping
    {
        [Key]
        public int MappingId { get; set; }

        [Required]
        public int SectionId { get; set; }

        public int? MetricId { get; set; }

        // Mapping name for identification
        [Required]
        [StringLength(100)]
        public string MappingName { get; set; } = string.Empty;

        // Mapping types
        [Required]
        [StringLength(30)]
        public string MappingType { get; set; } = string.Empty; // 'Aggregated', 'Calculated'

        // Aggregation type
        [Required]
        [StringLength(20)]
        public string AggregationType { get; set; } = string.Empty; // AVG, SUM, COUNT, WeightedAverage

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(SectionId))]
        public virtual FormTemplateSection Section { get; set; } = null!;

        [ForeignKey(nameof(MetricId))]
        public virtual MetricDefinition? Metric { get; set; }

        public virtual ICollection<FormSectionMetricSource> Sources { get; set; } = new List<FormSectionMetricSource>();
    }
}

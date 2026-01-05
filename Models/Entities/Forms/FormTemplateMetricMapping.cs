using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Links form templates to metrics for automatic KPI tracking
    /// </summary>
    [Table("FormTemplateMetricMappings")]
    public class FormTemplateMetricMapping
    {
        [Key]
        public int MappingId { get; set; }

        [Required]
        public int TemplateId { get; set; }

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
        public string AggregationType { get; set; } = string.Empty; // AVG, SUM, WeightedAverage

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(TemplateId))]
        public virtual FormTemplate Template { get; set; } = null!;

        [ForeignKey(nameof(MetricId))]
        public virtual MetricDefinition? Metric { get; set; }

        public virtual ICollection<FormTemplateMetricSource> Sources { get; set; } = new List<FormTemplateMetricSource>();
    }
}

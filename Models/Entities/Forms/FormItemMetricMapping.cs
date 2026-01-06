using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Links template items to metrics for automatic KPI tracking
    /// </summary>
    [Table("FormItemMetricMappings")]
    public class FormItemMetricMapping
    {
        [Key]
        public int MappingId { get; set; }

        [Required]
        public int ItemId { get; set; }

        public int? MetricId { get; set; }

        // Mapping name for identification
        [Required]
        [StringLength(100)]
        public string MappingName { get; set; } = string.Empty;

        // === MAPPING LOGIC ===
        [Required]
        [StringLength(20)]
        public string MappingType { get; set; } = "Direct"; // Direct, Calculated, Derived

        [Required]
        [StringLength(20)]
        public string OutputType { get; set; } = "Raw"; // Raw, Percentage, Normalized

        // === TRANSFORMATION ===
        public string? TransformationLogic { get; set; } // JSON for Calculated/Derived mappings

        // === EXPECTED VALUE COMPARISON (replaces Binary) ===
        [StringLength(100)]
        public string? ExpectedValue { get; set; } // For compliance checking against field value

        [StringLength(20)]
        public string? ComparisonOperator { get; set; } = "Equals"; // Equals, GreaterThan, LessThan, Contains

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(ItemId))]
        public virtual FormTemplateItem Item { get; set; } = null!;

        [ForeignKey(nameof(MetricId))]
        public virtual MetricDefinition? Metric { get; set; }

        public virtual ICollection<MetricPopulationLog> PopulationLogs { get; set; } = new List<MetricPopulationLog>();
    }
}

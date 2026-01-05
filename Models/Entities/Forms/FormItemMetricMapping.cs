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

        // Mapping types
        [Required]
        [StringLength(30)]
        public string MappingType { get; set; } = string.Empty; // 'Direct', 'Calculated', 'BinaryCompliance', 'Derived'

        // Aggregation type
        [Required]
        [StringLength(20)]
        public string AggregationType { get; set; } = string.Empty; // Direct, Sum, Count, Avg

        // For calculated metrics (e.g., availability% = operational/total * 100)
        public string? TransformationLogic { get; set; } // JSON: {"formula": "(item21 / item20) * 100", "items": [21, 20]}

        // For binary compliance metrics (e.g., Is LAN working? Expected: Yes)
        [StringLength(100)]
        public string? ExpectedValue { get; set; } // 'TRUE', 'Yes', '100%', etc.

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

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

        [Required]
        public int MetricId { get; set; }

        // Mapping types
        [Required]
        [StringLength(30)]
        public string MappingType { get; set; } = string.Empty; // 'Direct', 'Calculated', 'BinaryCompliance', 'Derived'

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
        public virtual MetricDefinition Metric { get; set; } = null!;

        public virtual ICollection<MetricPopulationLog> PopulationLogs { get; set; } = new List<MetricPopulationLog>();
    }
}

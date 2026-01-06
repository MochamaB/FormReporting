using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Metrics
{
    /// <summary>
    /// Lookup table for metric units (e.g., Count, Percentage, Score, Days)
    /// </summary>
    [Table("MetricUnits")]
    public class MetricUnit
    {
        [Key]
        public int UnitId { get; set; }

        [Required]
        [StringLength(20)]
        public string UnitCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string UnitName { get; set; } = string.Empty;

        /// <summary>
        /// Symbol to display after values (e.g., "%", "hrs", "")
        /// </summary>
        [StringLength(10)]
        public string? UnitSymbol { get; set; }

        /// <summary>
        /// Format pattern for displaying values (e.g., "{0}%", "{0} items")
        /// </summary>
        [StringLength(50)]
        public string? FormatPattern { get; set; }

        /// <summary>
        /// Suggested aggregation type for this unit (e.g., "Average" for percentages)
        /// </summary>
        [StringLength(20)]
        public string? SuggestedAggregation { get; set; }

        /// <summary>
        /// Category of unit for grouping in UI (e.g., "Quantity", "Time", "Data")
        /// </summary>
        [StringLength(30)]
        public string? UnitCategory { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<MetricDefinition> MetricDefinitions { get; set; } = new List<MetricDefinition>();
    }
}

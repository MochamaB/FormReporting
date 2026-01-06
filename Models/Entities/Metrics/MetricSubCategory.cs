using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Metrics
{
    /// <summary>
    /// Subcategories of metrics with constraints for data types, aggregation types, and units
    /// </summary>
    [Table("MetricSubCategories")]
    public class MetricSubCategory
    {
        [Key]
        public int SubCategoryId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string SubCategoryCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SubCategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // === CONSTRAINTS ===
        /// <summary>
        /// JSON array of allowed data types (e.g., ["Percentage","Decimal","Integer"])
        /// </summary>
        [Required]
        public string AllowedDataTypes { get; set; } = string.Empty;

        /// <summary>
        /// JSON array of allowed aggregation types (e.g., ["AVG","SUM","LAST_VALUE"])
        /// </summary>
        [Required]
        public string AllowedAggregationTypes { get; set; } = string.Empty;

        // === DEFAULTS ===
        [Required]
        [StringLength(20)]
        public string DefaultDataType { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string DefaultAggregationType { get; set; } = string.Empty;

        public int? DefaultUnitId { get; set; }

        // === SCOPE CONSTRAINTS ===
        /// <summary>
        /// Comma-separated list of allowed scopes (e.g., "Field,Section,Template")
        /// </summary>
        [Required]
        [StringLength(50)]
        public string AllowedScopes { get; set; } = "Field,Section,Template";

        /// <summary>
        /// Default scope when creating metrics with this subcategory
        /// </summary>
        [StringLength(20)]
        public string? DefaultScope { get; set; } = "Field";

        // === THRESHOLD SUGGESTIONS ===
        [Column(TypeName = "decimal(18,4)")]
        public decimal? SuggestedThresholdGreen { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? SuggestedThresholdYellow { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? SuggestedThresholdRed { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(CategoryId))]
        public virtual MetricCategory Category { get; set; } = null!;

        [ForeignKey(nameof(DefaultUnitId))]
        public virtual MetricUnit? DefaultUnit { get; set; }

        public virtual ICollection<MetricSubCategoryUnit> AllowedUnits { get; set; } = new List<MetricSubCategoryUnit>();
        public virtual ICollection<MetricDefinition> MetricDefinitions { get; set; } = new List<MetricDefinition>();
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Metrics
{
    /// <summary>
    /// Junction table linking subcategories to their allowed units
    /// </summary>
    [Table("MetricSubCategoryUnits")]
    public class MetricSubCategoryUnit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        [Required]
        public int UnitId { get; set; }

        /// <summary>
        /// Is this the default unit for this subcategory?
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Display order in unit dropdown
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(SubCategoryId))]
        public virtual MetricSubCategory SubCategory { get; set; } = null!;

        [ForeignKey(nameof(UnitId))]
        public virtual MetricUnit Unit { get; set; } = null!;
    }
}

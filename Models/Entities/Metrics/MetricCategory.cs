using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Metrics
{
    /// <summary>
    /// Lookup table for metric categories (e.g., Hardware, Software, Performance, Compliance)
    /// </summary>
    [Table("MetricCategories")]
    public class MetricCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(30)]
        public string CategoryCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Icon class for UI display (e.g., "ri-computer-line")
        /// </summary>
        [StringLength(50)]
        public string? IconClass { get; set; }

        /// <summary>
        /// Color hint for UI display (e.g., "#4CAF50")
        /// </summary>
        [StringLength(20)]
        public string? ColorHint { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<MetricSubCategory> SubCategories { get; set; } = new List<MetricSubCategory>();
    }
}

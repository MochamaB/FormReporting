using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Hardware
{
    /// <summary>
    /// Hierarchical hardware categories
    /// </summary>
    [Table("HardwareCategories")]
    public class HardwareCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(ParentCategoryId))]
        public virtual HardwareCategory? ParentCategory { get; set; }

        public virtual ICollection<HardwareCategory> ChildCategories { get; set; } = new List<HardwareCategory>();
        public virtual ICollection<HardwareItem> HardwareItems { get; set; } = new List<HardwareItem>();
    }
}

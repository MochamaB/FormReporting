using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Hardware
{
    /// <summary>
    /// Master list of hardware items
    /// </summary>
    [Table("HardwareItems")]
    public class HardwareItem
    {
        [Key]
        public int HardwareItemId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Manufacturer { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        public string? Specifications { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(CategoryId))]
        public virtual HardwareCategory Category { get; set; } = null!;

        public virtual ICollection<TenantHardware> TenantHardware { get; set; } = new List<TenantHardware>();
    }
}

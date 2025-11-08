using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;

namespace FormReporting.Models.Entities.Hardware
{
    /// <summary>
    /// Actual hardware inventory per tenant
    /// </summary>
    [Table("TenantHardware")]
    public class TenantHardware
    {
        [Key]
        public int TenantHardwareId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        public int HardwareItemId { get; set; }

        [StringLength(50)]
        public string? AssetTag { get; set; }

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [StringLength(200)]
        public string? Location { get; set; } // Server Room, Office, etc.

        [Column(TypeName = "date")]
        public DateTime? PurchaseDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? WarrantyExpiryDate { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } // Operational, Faulty, UnderRepair, Retired, InStorage, PendingDeployment, Disposed

        public int Quantity { get; set; } = 1;

        [StringLength(1000)]
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;

        [ForeignKey(nameof(HardwareItemId))]
        public virtual HardwareItem HardwareItem { get; set; } = null!;

        public virtual ICollection<HardwareMaintenanceLog> MaintenanceLogs { get; set; } = new List<HardwareMaintenanceLog>();
    }
}

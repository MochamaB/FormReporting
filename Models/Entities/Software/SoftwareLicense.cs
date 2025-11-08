using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Software
{
    /// <summary>
    /// Centralized software license management
    /// </summary>
    [Table("SoftwareLicenses")]
    public class SoftwareLicense
    {
        [Key]
        public int LicenseId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(500)]
        public string LicenseKey { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LicenseType { get; set; } = string.Empty; // 'Perpetual', 'Subscription', 'Trial', 'Volume', 'Academic', 'OEM'

        // Purchase information
        [Column(TypeName = "date")]
        public DateTime? PurchaseDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ExpiryDate { get; set; }

        public int QuantityPurchased { get; set; } = 1;

        public int QuantityUsed { get; set; } = 0;

        [StringLength(100)]
        public string? PurchaseOrderNumber { get; set; }

        [StringLength(200)]
        public string? Vendor { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cost { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = "KES";

        // Contact and support
        [StringLength(200)]
        public string? SupportContact { get; set; }

        [StringLength(50)]
        public string? SupportPhone { get; set; }

        [StringLength(200)]
        public string? SupportEmail { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(ProductId))]
        public virtual SoftwareProduct Product { get; set; } = null!;

        public virtual ICollection<TenantSoftwareInstallation> Installations { get; set; } = new List<TenantSoftwareInstallation>();
    }
}

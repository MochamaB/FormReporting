using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Software
{
    /// <summary>
    /// Software products catalog
    /// </summary>
    [Table("SoftwareProducts")]
    public class SoftwareProduct
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Vendor { get; set; }

        [StringLength(100)]
        public string? ProductCategory { get; set; } // System, Application, Utility, Security

        [StringLength(50)]
        public string? LicenseModel { get; set; } // 'PerUser', 'PerDevice', 'Enterprise', 'Subscription', 'OpenSource', 'Concurrent'

        public bool IsKTDAProduct { get; set; } = false;

        public bool RequiresLicense { get; set; } = false;

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<SoftwareVersion> Versions { get; set; } = new List<SoftwareVersion>();
        public virtual ICollection<SoftwareLicense> Licenses { get; set; } = new List<SoftwareLicense>();
        public virtual ICollection<TenantSoftwareInstallation> Installations { get; set; } = new List<TenantSoftwareInstallation>();
    }
}

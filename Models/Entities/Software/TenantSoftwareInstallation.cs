using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Software
{
    /// <summary>
    /// Tenant software installations with license tracking
    /// </summary>
    [Table("TenantSoftwareInstallations")]
    public class TenantSoftwareInstallation
    {
        [Key]
        public int InstallationId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int VersionId { get; set; }

        public int? LicenseId { get; set; } // Link to centralized license

        [Column(TypeName = "date")]
        public DateTime? InstallationDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? LastVerifiedDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active"; // 'Active', 'Deprecated', 'NeedsUpgrade', 'EndOfLife', 'Uninstalled'

        [StringLength(30)]
        public string? InstallationType { get; set; } // 'Server', 'Workstation', 'Cloud', 'Virtual', 'Container'

        [StringLength(500)]
        public string? InstallationPath { get; set; } // Where software is installed

        // Machine/Instance details
        [StringLength(100)]
        public string? MachineName { get; set; }

        [StringLength(50)]
        public string? IPAddress { get; set; }

        // Verification
        public int? VerifiedBy { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;

        [ForeignKey(nameof(ProductId))]
        public virtual SoftwareProduct Product { get; set; } = null!;

        [ForeignKey(nameof(VersionId))]
        public virtual SoftwareVersion Version { get; set; } = null!;

        [ForeignKey(nameof(LicenseId))]
        public virtual SoftwareLicense? License { get; set; }

        [ForeignKey(nameof(VerifiedBy))]
        public virtual User? Verifier { get; set; }

        [ForeignKey(nameof(ModifiedBy))]
        public virtual User? Modifier { get; set; }

        public virtual ICollection<SoftwareInstallationHistory> History { get; set; } = new List<SoftwareInstallationHistory>();
    }
}

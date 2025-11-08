using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Software
{
    /// <summary>
    /// Software version registry with security tracking
    /// </summary>
    [Table("SoftwareVersions")]
    public class SoftwareVersion
    {
        [Key]
        public int VersionId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string VersionNumber { get; set; } = string.Empty;

        // Version comparison support
        public int? MajorVersion { get; set; } // e.g., 10 from "10.2.5"

        public int? MinorVersion { get; set; } // e.g., 2 from "10.2.5"

        public int? PatchVersion { get; set; } // e.g., 5 from "10.2.5"

        [Column(TypeName = "date")]
        public DateTime? ReleaseDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EndOfLifeDate { get; set; }

        public bool IsCurrentVersion { get; set; } = false;

        public bool IsSupported { get; set; } = true;

        [StringLength(20)]
        public string SecurityLevel { get; set; } = "Stable"; // 'Critical', 'Stable', 'Vulnerable', 'Unsupported'

        public bool MinimumSupportedVersion { get; set; } = false;

        public string? ReleaseNotes { get; set; }

        [StringLength(500)]
        public string? DownloadUrl { get; set; }

        public long? FileSize { get; set; } // Download size in bytes

        [StringLength(64)]
        public string? ChecksumSHA256 { get; set; } // For integrity verification

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(ProductId))]
        public virtual SoftwareProduct Product { get; set; } = null!;

        public virtual ICollection<TenantSoftwareInstallation> Installations { get; set; } = new List<TenantSoftwareInstallation>();
        public virtual ICollection<SoftwareInstallationHistory> HistoryAsFromVersion { get; set; } = new List<SoftwareInstallationHistory>();
        public virtual ICollection<SoftwareInstallationHistory> HistoryAsToVersion { get; set; } = new List<SoftwareInstallationHistory>();
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Software
{
    /// <summary>
    /// Audit trail of software installations, upgrades, and changes
    /// </summary>
    [Table("SoftwareInstallationHistory")]
    public class SoftwareInstallationHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public int InstallationId { get; set; }

        public int? FromVersionId { get; set; }

        [Required]
        public int ToVersionId { get; set; }

        [Required]
        [StringLength(20)]
        public string ChangeType { get; set; } = string.Empty; // 'Install', 'Upgrade', 'Downgrade', 'Uninstall', 'Reinstall', 'Patch'

        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int ChangedBy { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        public bool SuccessStatus { get; set; } = true; // Did the change succeed?

        public string? ErrorMessage { get; set; }

        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey(nameof(InstallationId))]
        public virtual TenantSoftwareInstallation Installation { get; set; } = null!;

        [ForeignKey(nameof(FromVersionId))]
        public virtual SoftwareVersion? FromVersion { get; set; }

        [ForeignKey(nameof(ToVersionId))]
        public virtual SoftwareVersion ToVersion { get; set; } = null!;

        [ForeignKey(nameof(ChangedBy))]
        public virtual User ChangedByUser { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Hardware
{
    /// <summary>
    /// Hardware maintenance and service log
    /// </summary>
    [Table("HardwareMaintenanceLog")]
    public class HardwareMaintenanceLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public int TenantHardwareId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime MaintenanceDate { get; set; }

        [StringLength(50)]
        public string? MaintenanceType { get; set; } // Preventive, Corrective, Upgrade, Emergency, Calibration, Inspection

        public string? Description { get; set; }

        public int? PerformedBy { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cost { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NextMaintenanceDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(TenantHardwareId))]
        public virtual TenantHardware TenantHardware { get; set; } = null!;
    }
}

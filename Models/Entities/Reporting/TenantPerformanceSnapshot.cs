using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;

namespace FormReporting.Models.Entities.Reporting
{
    /// <summary>
    /// Pre-aggregated daily/weekly/monthly metrics per tenant for dashboard performance
    /// </summary>
    [Table("TenantPerformanceSnapshot")]
    public class TenantPerformanceSnapshot
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SnapshotId { get; set; }

        /// <summary>
        /// Tenant ID
        /// </summary>
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// Snapshot date (supports daily, weekly, monthly snapshots)
        /// </summary>
        [Required]
        [Column(TypeName = "date")]
        public DateTime SnapshotDate { get; set; }

        /// <summary>
        /// Snapshot type: Daily, Weekly, Monthly, Quarterly
        /// </summary>
        [Required]
        [StringLength(20)]
        public string SnapshotType { get; set; } = string.Empty;

        /// <summary>
        /// Flexible metrics storage (JSON for extensibility)
        /// All metrics in flexible format
        /// </summary>
        [Required]
        public string MetricsData { get; set; } = string.Empty;

        /// <summary>
        /// Total devices (denormalized for quick queries)
        /// </summary>
        public int TotalDevices { get; set; } = 0;

        /// <summary>
        /// Working devices count
        /// </summary>
        public int WorkingDevices { get; set; } = 0;

        /// <summary>
        /// Faulty devices count
        /// </summary>
        public int FaultyDevices { get; set; } = 0;

        /// <summary>
        /// Uptime percentage
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? UptimePercent { get; set; }

        /// <summary>
        /// Open tickets count
        /// </summary>
        public int OpenTickets { get; set; } = 0;

        /// <summary>
        /// Compliance score
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? ComplianceScore { get; set; }

        /// <summary>
        /// Total expenses
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalExpenses { get; set; }

        /// <summary>
        /// When the snapshot was generated
        /// </summary>
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Who/what generated this snapshot (HangfireJob, Manual, Scheduled)
        /// </summary>
        [StringLength(50)]
        public string? GeneratedBy { get; set; }

        /// <summary>
        /// Data version for schema evolution
        /// </summary>
        public int DataVersion { get; set; } = 1;

        // Navigation properties
        /// <summary>
        /// The tenant this snapshot belongs to
        /// </summary>
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;
    }
}

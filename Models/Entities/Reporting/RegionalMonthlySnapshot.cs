using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;

namespace FormReporting.Models.Entities.Reporting
{
    /// <summary>
    /// Aggregated monthly metrics per region (factories only)
    /// </summary>
    [Table("RegionalMonthlySnapshot")]
    public class RegionalMonthlySnapshot
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SnapshotId { get; set; }

        /// <summary>
        /// Region ID
        /// </summary>
        [Required]
        public int RegionId { get; set; }

        /// <summary>
        /// Year and month (first day of month)
        /// </summary>
        [Required]
        [Column(TypeName = "date")]
        public DateTime YearMonth { get; set; }

        /// <summary>
        /// Total factories in the region
        /// </summary>
        public int TotalFactories { get; set; } = 0;

        /// <summary>
        /// Total devices across all factories
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
        /// Total tickets in the region
        /// </summary>
        public int TotalTickets { get; set; } = 0;

        /// <summary>
        /// Open tickets count
        /// </summary>
        public int OpenTickets { get; set; } = 0;

        /// <summary>
        /// Resolved tickets count
        /// </summary>
        public int ResolvedTickets { get; set; } = 0;

        /// <summary>
        /// Average resolution time in days
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? AvgResolutionDays { get; set; }

        /// <summary>
        /// Total expenses for the region
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalExpenses { get; set; }

        /// <summary>
        /// Average compliance score across factories
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? AvgComplianceScore { get; set; }

        /// <summary>
        /// When the snapshot was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation properties
        /// <summary>
        /// The region this snapshot belongs to
        /// </summary>
        [ForeignKey(nameof(RegionId))]
        public virtual Region Region { get; set; } = null!;
    }
}

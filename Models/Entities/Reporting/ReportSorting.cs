using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Models.Entities.Reporting
{
    /// <summary>
    /// ORDER BY logic for reports
    /// </summary>
    [Table("ReportSorting")]
    public class ReportSorting
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SortId { get; set; }

        /// <summary>
        /// Report ID
        /// </summary>
        [Required]
        public int ReportId { get; set; }

        /// <summary>
        /// Form item ID to sort by (if applicable)
        /// </summary>
        public int? ItemId { get; set; }

        /// <summary>
        /// Metric ID to sort by (if applicable)
        /// </summary>
        public int? MetricId { get; set; }

        /// <summary>
        /// System field name to sort by
        /// Examples: TenantName, SubmissionDate
        /// </summary>
        [StringLength(100)]
        public string? SystemFieldName { get; set; }

        /// <summary>
        /// Sort order: 0 = primary sort, 1 = secondary sort, etc.
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Sort direction: ASC, DESC
        /// </summary>
        [StringLength(10)]
        public string SortDirection { get; set; } = "ASC";

        // Navigation properties
        /// <summary>
        /// The report this sorting belongs to
        /// </summary>
        [ForeignKey(nameof(ReportId))]
        public virtual ReportDefinition Report { get; set; } = null!;

        /// <summary>
        /// Form template item (if applicable)
        /// </summary>
        [ForeignKey(nameof(ItemId))]
        public virtual FormTemplateItem? Item { get; set; }

        /// <summary>
        /// Metric definition (if applicable)
        /// </summary>
        [ForeignKey(nameof(MetricId))]
        public virtual MetricDefinition? Metric { get; set; }
    }
}

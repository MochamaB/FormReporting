using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Models.Entities.Reporting
{
    /// <summary>
    /// GROUP BY logic for reports
    /// </summary>
    [Table("ReportGroupings")]
    public class ReportGrouping
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroupingId { get; set; }

        /// <summary>
        /// Report ID
        /// </summary>
        [Required]
        public int ReportId { get; set; }

        /// <summary>
        /// Group by type: Tenant, Region, Month, Year, Quarter, Week, Day, TenantType, Category, FieldValue, MetricValue
        /// </summary>
        [Required]
        [StringLength(30)]
        public string GroupByType { get; set; } = string.Empty;

        /// <summary>
        /// Form item ID to group by (if applicable)
        /// </summary>
        public int? ItemId { get; set; }

        /// <summary>
        /// Metric ID to group by (if applicable)
        /// </summary>
        public int? MetricId { get; set; }

        /// <summary>
        /// System field name to group by
        /// Examples: TenantName, RegionName, SubmissionMonth
        /// </summary>
        [StringLength(100)]
        public string? SystemFieldName { get; set; }

        /// <summary>
        /// Group order for nested grouping
        /// 0 = first level, 1 = second level, etc.
        /// </summary>
        public int GroupOrder { get; set; } = 0;

        /// <summary>
        /// Sort direction: ASC, DESC
        /// </summary>
        [StringLength(10)]
        public string SortDirection { get; set; } = "ASC";

        /// <summary>
        /// Show subtotal rows for this grouping?
        /// </summary>
        public bool ShowSubtotals { get; set; } = true;

        /// <summary>
        /// Show grand total row?
        /// </summary>
        public bool ShowGrandTotal { get; set; } = true;

        // Navigation properties
        /// <summary>
        /// The report this grouping belongs to
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

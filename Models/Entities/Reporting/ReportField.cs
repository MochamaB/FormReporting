using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Models.Entities.Reporting
{
    /// <summary>
    /// Defines which columns/fields to include in a report
    /// </summary>
    [Table("ReportFields")]
    public class ReportField
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FieldId { get; set; }

        /// <summary>
        /// Report ID
        /// </summary>
        [Required]
        public int ReportId { get; set; }

        /// <summary>
        /// Field source type: FormItem, Metric, Computed, SystemField
        /// </summary>
        [Required]
        [StringLength(30)]
        public string SourceType { get; set; } = string.Empty;

        /// <summary>
        /// Form template item ID (if SourceType = FormItem)
        /// </summary>
        public int? ItemId { get; set; }

        /// <summary>
        /// Metric ID (if SourceType = Metric)
        /// </summary>
        public int? MetricId { get; set; }

        /// <summary>
        /// System field name (if SourceType = SystemField)
        /// Examples: TenantName, RegionName, SubmissionDate
        /// </summary>
        [StringLength(100)]
        public string? SystemFieldName { get; set; }

        /// <summary>
        /// Custom column header/display name
        /// </summary>
        [Required]
        [StringLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Display order in the report
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Is this field visible in the report?
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Column width for UI rendering
        /// </summary>
        public int? ColumnWidth { get; set; }

        /// <summary>
        /// Aggregation type: Sum, Avg, Count, Min, Max, CountDistinct, First, Last, None
        /// </summary>
        [StringLength(20)]
        public string? AggregationType { get; set; }

        /// <summary>
        /// Format string for display
        /// Examples: #,##0.00, yyyy-MM-dd, 0.0%
        /// </summary>
        [StringLength(50)]
        public string? FormatString { get; set; }

        /// <summary>
        /// Computation formula for computed fields (JSON)
        /// </summary>
        public string? ComputationFormula { get; set; }

        /// <summary>
        /// Conditional formatting rules (JSON)
        /// Example: {"rules": [{"condition": ">90", "color": "green"}]}
        /// </summary>
        public string? ConditionalFormatting { get; set; }

        // Navigation properties
        /// <summary>
        /// The report this field belongs to
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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Models.Entities.Reporting
{
    /// <summary>
    /// WHERE clause conditions for reports
    /// </summary>
    [Table("ReportFilters")]
    public class ReportFilter
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FilterId { get; set; }

        /// <summary>
        /// Report ID
        /// </summary>
        [Required]
        public int ReportId { get; set; }

        /// <summary>
        /// Filter type: TenantId, RegionId, DateRange, Status, FieldValue, MetricValue, TenantType, Custom
        /// </summary>
        [Required]
        [StringLength(30)]
        public string FilterType { get; set; } = string.Empty;

        /// <summary>
        /// Form item ID to filter on (if applicable)
        /// </summary>
        public int? ItemId { get; set; }

        /// <summary>
        /// Metric ID to filter on (if applicable)
        /// </summary>
        public int? MetricId { get; set; }

        /// <summary>
        /// System field name to filter on
        /// Examples: SubmittedDate, TenantType, Status
        /// </summary>
        [StringLength(100)]
        public string? SystemFieldName { get; set; }

        /// <summary>
        /// Filter operator: Equals, NotEquals, GreaterThan, LessThan, Between, In, Contains, IsNull, etc.
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Operator { get; set; } = string.Empty;

        /// <summary>
        /// Filter value (JSON)
        /// Single value: {"value": "Factory"}
        /// Multiple values: {"values": [1,2,3]}
        /// </summary>
        public string? FilterValue { get; set; }

        /// <summary>
        /// Is this filter required (cannot be removed)?
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// Can user change the filter value?
        /// </summary>
        public bool AllowUserOverride { get; set; } = true;

        /// <summary>
        /// Prompt user for value when running report?
        /// </summary>
        public bool IsParameterized { get; set; } = false;

        /// <summary>
        /// Parameter label for user prompt
        /// Example: "Select Region:"
        /// </summary>
        [StringLength(200)]
        public string? ParameterLabel { get; set; }

        /// <summary>
        /// Default parameter value
        /// </summary>
        [StringLength(500)]
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Display order for filters
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        // Navigation properties
        /// <summary>
        /// The report this filter belongs to
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

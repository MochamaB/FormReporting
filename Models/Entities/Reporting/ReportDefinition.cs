using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Common;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Reporting
{
    /// <summary>
    /// User-created custom report configurations
    /// </summary>
    [Table("ReportDefinitions")]
    public class ReportDefinition : BaseEntity, IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportId { get; set; }

        /// <summary>
        /// Report name
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ReportName { get; set; } = string.Empty;

        /// <summary>
        /// Unique report code
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ReportCode { get; set; } = string.Empty;

        /// <summary>
        /// Report description
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Form template ID if this is a form-based report
        /// </summary>
        public int? TemplateId { get; set; }

        /// <summary>
        /// Report type: Tabular, Chart, Pivot, Dashboard, CrossTab, Matrix
        /// </summary>
        [Required]
        [StringLength(30)]
        public string ReportType { get; set; } = string.Empty;

        /// <summary>
        /// Report category: Operations, Finance, Compliance, Hardware, Software
        /// </summary>
        [StringLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Chart type if ReportType = Chart
        /// Bar, Line, Pie, Doughnut, Area, Column, Scatter, Bubble, Radar
        /// </summary>
        [StringLength(30)]
        public string? ChartType { get; set; }

        /// <summary>
        /// Chart-specific configuration (JSON)
        /// </summary>
        public string? ChartConfiguration { get; set; }

        /// <summary>
        /// Page layout configuration (JSON)
        /// Includes orientation, paper size, margins
        /// </summary>
        public string? LayoutConfiguration { get; set; }

        /// <summary>
        /// Can other users see this report?
        /// </summary>
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// Is this a system-provided report (cannot be deleted)?
        /// </summary>
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// User who created/owns this report
        /// </summary>
        [Required]
        public int OwnerUserId { get; set; }

        /// <summary>
        /// Report version
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Is this report active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the report was last run
        /// </summary>
        public DateTime? LastRunDate { get; set; }

        /// <summary>
        /// How many times this report has been run (popularity tracking)
        /// </summary>
        public int RunCount { get; set; } = 0;

        // Navigation properties
        /// <summary>
        /// Form template if this is a form-based report
        /// </summary>
        [ForeignKey(nameof(TemplateId))]
        public virtual FormTemplate? Template { get; set; }

        /// <summary>
        /// Report owner
        /// </summary>
        [ForeignKey(nameof(OwnerUserId))]
        public virtual User Owner { get; set; } = null!;

        /// <summary>
        /// Report fields/columns
        /// </summary>
        public virtual ICollection<ReportField> Fields { get; set; } = new List<ReportField>();

        /// <summary>
        /// Report filters
        /// </summary>
        public virtual ICollection<ReportFilter> Filters { get; set; } = new List<ReportFilter>();

        /// <summary>
        /// Report groupings
        /// </summary>
        public virtual ICollection<ReportGrouping> Groupings { get; set; } = new List<ReportGrouping>();

        /// <summary>
        /// Report sorting
        /// </summary>
        public virtual ICollection<ReportSorting> Sortings { get; set; } = new List<ReportSorting>();

        /// <summary>
        /// Report schedules
        /// </summary>
        public virtual ICollection<ReportSchedule> Schedules { get; set; } = new List<ReportSchedule>();

        /// <summary>
        /// Report cache entries
        /// </summary>
        public virtual ICollection<ReportCache> CacheEntries { get; set; } = new List<ReportCache>();

        /// <summary>
        /// Access control entries
        /// </summary>
        public virtual ICollection<ReportAccessControl> AccessControls { get; set; } = new List<ReportAccessControl>();

        /// <summary>
        /// Execution log entries
        /// </summary>
        public virtual ICollection<ReportExecutionLog> ExecutionLogs { get; set; } = new List<ReportExecutionLog>();
    }
}

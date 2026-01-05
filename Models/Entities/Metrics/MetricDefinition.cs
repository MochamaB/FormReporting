using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Metrics
{
    /// <summary>
    /// Defines KPI metrics and performance indicators tracked across the system
    /// </summary>
    [Table("MetricDefinitions")]
    public class MetricDefinition
    {
        [Key]
        public int MetricId { get; set; }

        [Required]
        [StringLength(50)]
        public string MetricCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string MetricName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; } // Infrastructure, Software, Hardware, Performance, Compliance

        // Source configuration
        [Required]
        [StringLength(30)]
        public string SourceType { get; set; } = string.Empty; // UserInput, SystemCalculated, ExternalSystem, ComplianceTracking, AutomatedCheck

        [Required]
        [StringLength(20)]
        public string DataType { get; set; } = string.Empty; // Integer, Decimal, Percentage, Boolean, Text, Duration, Date, DateTime

        [StringLength(50)]
        public string? Unit { get; set; } // Count, Percentage, Version, Status, Days, Hours, Minutes, Seconds, GB, MB, KB, TB, Bytes, None

        [StringLength(20)]
        public string? AggregationType { get; set; } // SUM, AVG, MAX, MIN, LAST_VALUE, COUNT, NONE

        // Hierarchy support
        [StringLength(30)]
        public string? MetricScope { get; set; } // Field, Section, Template

        public int? HierarchyLevel { get; set; } // 0=Field, 1=Section, 2=Template

        public int? ParentMetricId { get; set; } // Self-reference for hierarchy

        // KPI thresholds
        public bool IsKPI { get; set; } = false;

        [Column(TypeName = "decimal(18,4)")]
        public decimal? ThresholdGreen { get; set; } // Target/Good

        [Column(TypeName = "decimal(18,4)")]
        public decimal? ThresholdYellow { get; set; } // Warning

        [Column(TypeName = "decimal(18,4)")]
        public decimal? ThresholdRed { get; set; } // Critical

        // Expected value for binary/compliance metrics
        [StringLength(100)]
        public string? ExpectedValue { get; set; } // 'TRUE', 'Yes', '100%', etc.

        // Compliance rules (JSON for deadline tracking, validation rules)
        public string? ComplianceRule { get; set; } // JSON: {"type": "deadline", "daysAfterPeriodEnd": 2}

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(ParentMetricId))]
        public virtual MetricDefinition? ParentMetric { get; set; }
        
        public virtual ICollection<MetricDefinition> ChildMetrics { get; set; } = new List<MetricDefinition>();
        public virtual ICollection<TenantMetric> TenantMetrics { get; set; } = new List<TenantMetric>();
        public virtual ICollection<SystemMetricLog> SystemMetricLogs { get; set; } = new List<SystemMetricLog>();
        public virtual ICollection<Forms.FormItemMetricMapping> FormItemMetricMappings { get; set; } = new List<Forms.FormItemMetricMapping>();
    }
}

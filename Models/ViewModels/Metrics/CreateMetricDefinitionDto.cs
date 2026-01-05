using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Metrics
{
    /// <summary>
    /// DTO for creating new metric definition
    /// </summary>
    public class CreateMetricDefinitionDto
    {
        [Required(ErrorMessage = "Metric Code is required")]
        [StringLength(50, ErrorMessage = "Metric Code cannot exceed 50 characters")]
        [Display(Name = "Metric Code")]
        [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Metric Code must contain only uppercase letters, numbers, and underscores")]
        public string MetricCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Metric Name is required")]
        [StringLength(200, ErrorMessage = "Metric Name cannot exceed 200 characters")]
        [Display(Name = "Metric Name")]
        public string MetricName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        [Display(Name = "Category")]
        public string? Category { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Source Type is required")]
        [StringLength(30, ErrorMessage = "Source Type cannot exceed 30 characters")]
        [Display(Name = "Source Type")]
        public string SourceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data Type is required")]
        [StringLength(20, ErrorMessage = "Data Type cannot exceed 20 characters")]
        [Display(Name = "Data Type")]
        public string DataType { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Unit cannot exceed 50 characters")]
        [Display(Name = "Unit")]
        public string? Unit { get; set; }

        [StringLength(20, ErrorMessage = "Aggregation Type cannot exceed 20 characters")]
        [Display(Name = "Aggregation Type")]
        public string? AggregationType { get; set; }

        [Display(Name = "Mark as KPI")]
        public bool IsKPI { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Threshold Green must be a positive number")]
        [Display(Name = "Green Threshold")]
        public decimal? ThresholdGreen { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Threshold Yellow must be a positive number")]
        [Display(Name = "Yellow Threshold")]
        public decimal? ThresholdYellow { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Threshold Red must be a positive number")]
        [Display(Name = "Red Threshold")]
        public decimal? ThresholdRed { get; set; }

        [StringLength(100, ErrorMessage = "Expected Value cannot exceed 100 characters")]
        [Display(Name = "Expected Value")]
        public string? ExpectedValue { get; set; }

        [Display(Name = "Compliance Rule (JSON)")]
        public string? ComplianceRule { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Template ID for generating global metric codes
        /// </summary>
        public int? TemplateId { get; set; }
    }

    /// <summary>
    /// DTO for updating metric definition
    /// </summary>
    public class UpdateMetricDefinitionDto
    {
        [StringLength(200)]
        public string? MetricName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        public decimal? ThresholdGreen { get; set; }
        public decimal? ThresholdYellow { get; set; }
        public decimal? ThresholdRed { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for updating metric thresholds only
    /// </summary>
    public class UpdateMetricThresholdsDto
    {
        [Required]
        public decimal ThresholdGreen { get; set; }

        [Required]
        public decimal ThresholdYellow { get; set; }

        public decimal? ThresholdRed { get; set; }

        [StringLength(500)]
        public string? ChangeReason { get; set; }
    }
}

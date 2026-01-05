using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Metrics
{
    // =========================================================================
    // BASE DTO - Shared fields for all mapping levels
    // =========================================================================

    /// <summary>
    /// Base DTO with common fields for Field, Section, and Template mappings
    /// </summary>
    public abstract class BaseMappingDto
    {
        [Required]
        [StringLength(100)]
        public string MappingName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string MappingType { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string AggregationType { get; set; } = string.Empty;

        /// <summary>
        /// Link to existing metric (nullable for standalone mappings)
        /// </summary>
        public int? MetricId { get; set; }

        // ===== Wizard-specific: Metric Option =====

        /// <summary>
        /// How to handle metric: standalone, create-new, link-existing
        /// </summary>
        public string MetricOption { get; set; } = "standalone";

        // ===== Wizard-specific: New Metric Fields (when MetricOption = "create-new") =====

        [StringLength(100)]
        public string? NewMetricName { get; set; }

        [StringLength(50)]
        public string? NewMetricCode { get; set; }

        [StringLength(500)]
        public string? NewMetricDescription { get; set; }

        [StringLength(50)]
        public string? NewMetricCategory { get; set; }

        [StringLength(20)]
        public string? NewMetricDataType { get; set; }

        [StringLength(20)]
        public string? NewMetricUnit { get; set; }

        public decimal? NewMetricGreen { get; set; }
        public decimal? NewMetricYellow { get; set; }
        public decimal? NewMetricRed { get; set; }

        /// <summary>
        /// Whether the new metric should be tracked as a KPI
        /// </summary>
        public bool NewMetricIsKPI { get; set; } = false;

        /// <summary>
        /// Aggregation type for the new metric (synced from mapping's AggregationType)
        /// </summary>
        [StringLength(20)]
        public string? NewMetricAggregationType { get; set; }
    }

    // =========================================================================
    // FIELD LEVEL - Maps FormTemplateItem to Metric
    // =========================================================================

    /// <summary>
    /// DTO for creating/updating field-level metric mappings
    /// Maps to FormItemMetricMapping entity
    /// </summary>
    public class CreateFieldMappingDto : BaseMappingDto
    {
        [Required]
        public int ItemId { get; set; }

        /// <summary>
        /// JSON configuration for calculated metrics
        /// Example: {"formula": "(item21/item20)*100", "sourceItems": [20,21], "roundTo": 2}
        /// </summary>
        public string? TransformationLogic { get; set; }

        /// <summary>
        /// Expected value for BinaryCompliance mappings
        /// Example: "Yes", "TRUE", "Compliant"
        /// </summary>
        [StringLength(100)]
        public string? ExpectedValue { get; set; }
    }

    // =========================================================================
    // SECTION LEVEL - Aggregates Field mappings into Section metric
    // =========================================================================

    /// <summary>
    /// DTO for creating/updating section-level metric mappings
    /// Maps to FormSectionMetricMapping entity
    /// </summary>
    public class CreateSectionMappingDto : BaseMappingDto
    {
        [Required]
        public int SectionId { get; set; }

        /// <summary>
        /// Source field mappings to aggregate (with optional weights)
        /// </summary>
        public List<SectionSourceDto> Sources { get; set; } = new();
    }

    /// <summary>
    /// Source item mapping for section aggregation
    /// Maps to FormSectionMetricSource entity
    /// </summary>
    public class SectionSourceDto
    {
        [Required]
        public int ItemMappingId { get; set; }

        /// <summary>
        /// Weight for weighted average calculations (0.0 to 1.0)
        /// </summary>
        public decimal? Weight { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }

    // =========================================================================
    // TEMPLATE LEVEL - Aggregates Section mappings into Template KPI
    // =========================================================================

    /// <summary>
    /// DTO for creating/updating template-level metric mappings (KPIs)
    /// Maps to FormTemplateMetricMapping entity
    /// </summary>
    public class CreateTemplateMappingDto : BaseMappingDto
    {
        [Required]
        public int TemplateId { get; set; }

        /// <summary>
        /// Source section mappings to aggregate (with optional weights)
        /// </summary>
        public List<TemplateSourceDto> Sources { get; set; } = new();
    }

    /// <summary>
    /// Source section mapping for template aggregation
    /// Maps to FormTemplateMetricSource entity
    /// </summary>
    public class TemplateSourceDto
    {
        [Required]
        public int SectionMappingId { get; set; }

        /// <summary>
        /// Weight for weighted average calculations (0.0 to 1.0)
        /// </summary>
        public decimal? Weight { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }

    // =========================================================================
    // UPDATE DTO - Shared for all mapping levels
    // =========================================================================

    /// <summary>
    /// DTO for updating any mapping type (Field, Section, or Template)
    /// Only includes fields that can be updated after creation
    /// </summary>
    public class UpdateMappingDto
    {
        [StringLength(100)]
        public string? MappingName { get; set; }

        [StringLength(20)]
        public string? AggregationType { get; set; }

        public string? TransformationLogic { get; set; }

        [StringLength(100)]
        public string? ExpectedValue { get; set; }

        public bool? IsActive { get; set; }
    }
}

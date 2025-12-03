using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Metrics
{
    /// <summary>
    /// DTO for creating new metric mapping
    /// </summary>
    public class CreateMappingDto
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        public int MetricId { get; set; }

        [Required]
        [StringLength(30)]
        public string MappingType { get; set; } = string.Empty; // Direct, SystemCalculated, BinaryCompliance, Derived

        /// <summary>
        /// JSON configuration for transformation
        /// For SystemCalculated: {"formula": "(item21/item20)*100", "sourceItems": [20,21], "roundTo": 2}
        /// For Derived: {"type": "weighted_average", "components": [...]}
        /// </summary>
        public string? TransformationLogic { get; set; }

        /// <summary>
        /// Expected value for BinaryCompliance mappings
        /// </summary>
        [StringLength(100)]
        public string? ExpectedValue { get; set; }
    }

    /// <summary>
    /// DTO for updating metric mapping
    /// </summary>
    public class UpdateMappingDto
    {
        public string? TransformationLogic { get; set; }
        public string? ExpectedValue { get; set; }
        public bool? IsActive { get; set; }
    }
}

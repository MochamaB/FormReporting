using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Individual options within an option template
    /// Example: For "Satisfaction Scale" template, items are: "Very Satisfied", "Satisfied", etc.
    /// </summary>
    [Table("FormItemOptionTemplateItems")]
    public class FormItemOptionTemplateItem
    {
        [Key]
        public int TemplateItemId { get; set; }

        // Relationship
        [Required]
        public int TemplateId { get; set; }

        // Option Details
        [Required]
        [StringLength(200)]
        public string OptionValue { get; set; } = string.Empty; // "very_satisfied"

        [Required]
        [StringLength(200)]
        public string OptionLabel { get; set; } = string.Empty; // "Very Satisfied"

        [Required]
        public int DisplayOrder { get; set; } // 1, 2, 3...

        // Scoring (Optional - for assessment forms)
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ScoreValue { get; set; } // 5, 4, 3, 2, 1

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ScoreWeight { get; set; } // Weight multiplier (default: 1.0)

        // Visual Hints (Optional)
        [StringLength(50)]
        public string? IconClass { get; set; } // "ri-emotion-happy-line" for positive options

        [StringLength(7)]
        public string? ColorHint { get; set; } // "#28a745" (green for positive)

        // Metadata
        public bool IsDefault { get; set; } = false; // Default selected option?

        // Navigation properties
        [ForeignKey(nameof(TemplateId))]
        public virtual FormItemOptionTemplate Template { get; set; } = null!;
    }
}

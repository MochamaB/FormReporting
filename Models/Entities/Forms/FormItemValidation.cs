using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Self-contained validation rules per field with inline parameters
    /// </summary>
    [Table("FormItemValidations")]
    public class FormItemValidation
    {
        [Key]
        public int ItemValidationId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        [StringLength(50)]
        public string ValidationType { get; set; } = string.Empty; // Required, Email, Phone, URL, Range, MinLength, MaxLength, Pattern, Custom, CrossField, Date, Number, Integer, Decimal

        // Inline validation parameters (no external rule table needed)
        [Column(TypeName = "decimal(18,4)")]
        public decimal? MinValue { get; set; } // For numeric range validation

        [Column(TypeName = "decimal(18,4)")]
        public decimal? MaxValue { get; set; } // For numeric range validation

        public int? MinLength { get; set; } // For text length validation

        public int? MaxLength { get; set; } // For text length validation

        [StringLength(500)]
        public string? RegexPattern { get; set; } // For pattern matching

        public string? CustomExpression { get; set; } // For complex custom validation logic

        public int ValidationOrder { get; set; } = 0; // Order of validation execution

        [Required]
        [StringLength(500)]
        public string ErrorMessage { get; set; } = string.Empty; // Error message to display

        [StringLength(20)]
        public string Severity { get; set; } = "Error"; // Error, Warning, Info

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(ItemId))]
        public virtual FormTemplateItem Item { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Auto-calculated fields based on other field values
    /// </summary>
    [Table("FormItemCalculations")]
    public class FormItemCalculation
    {
        [Key]
        public int CalculationId { get; set; }

        [Required]
        public int TargetItemId { get; set; } // Field that displays calculated result

        [Required]
        public string CalculationFormula { get; set; } = string.Empty; // JSON: {"formula": "(item1 * item2)", "sourceItems": [1, 2], "roundTo": 2}

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(TargetItemId))]
        public virtual FormTemplateItem TargetItem { get; set; } = null!;
    }
}

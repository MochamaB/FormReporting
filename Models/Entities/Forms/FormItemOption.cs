using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Options for dropdown, multi-select, radio button fields
    /// </summary>
    [Table("FormItemOptions")]
    public class FormItemOption
    {
        [Key]
        public int OptionId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        [StringLength(200)]
        public string OptionValue { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string OptionLabel { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public int? ParentOptionId { get; set; } // For cascading dropdowns

        // ===== SCORING FIELDS (For Assessment Forms) =====
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ScoreValue { get; set; } // Points assigned to this option

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ScoreWeight { get; set; } // Weight multiplier (default: 1.0)

        // Navigation properties
        [ForeignKey(nameof(ItemId))]
        public virtual FormTemplateItem Item { get; set; } = null!;

        [ForeignKey(nameof(ParentOptionId))]
        public virtual FormItemOption? ParentOption { get; set; }

        public virtual ICollection<FormItemOption> ChildOptions { get; set; } = new List<FormItemOption>();
    }
}

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

        // Navigation properties
        [ForeignKey(nameof(ItemId))]
        public virtual FormTemplateItem Item { get; set; } = null!;

        [ForeignKey(nameof(ParentOptionId))]
        public virtual FormItemOption? ParentOption { get; set; }

        public virtual ICollection<FormItemOption> ChildOptions { get; set; } = new List<FormItemOption>();
    }
}

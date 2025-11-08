using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Field-specific configuration settings (min/max values, file types, etc.)
    /// </summary>
    [Table("FormItemConfiguration")]
    public class FormItemConfiguration
    {
        [Key]
        public int ConfigId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string ConfigKey { get; set; } = string.Empty; // 'minValue', 'maxValue', 'allowedFileTypes', 'ratingMax', etc.

        [StringLength(500)]
        public string? ConfigValue { get; set; }

        // Navigation properties
        [ForeignKey(nameof(ItemId))]
        public virtual FormTemplateItem Item { get; set; } = null!;
    }
}

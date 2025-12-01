using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Categories for organizing option templates (Rating, Agreement, Binary, etc.)
    /// Optional table for better organization of templates in UI
    /// </summary>
    [Table("FormItemOptionTemplateCategories")]
    public class FormItemOptionTemplateCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = string.Empty; // "Rating", "Agreement", "Frequency"

        [StringLength(500)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;

        [StringLength(50)]
        public string? IconClass { get; set; } // "ri-star-line"

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<FormItemOptionTemplate> Templates { get; set; } = new List<FormItemOptionTemplate>();
    }
}

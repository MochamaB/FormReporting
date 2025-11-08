using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Categorizes form templates by operational area
    /// </summary>
    [Table("FormCategories")]
    public class FormCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string CategoryCode { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;

        [StringLength(50)]
        public string? IconClass { get; set; } // e.g., 'fa-network-wired', 'fa-server', 'fa-desktop'

        [StringLength(20)]
        public string? Color { get; set; } // For UI visual grouping

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<FormTemplate> FormTemplates { get; set; } = new List<FormTemplate>();
    }
}

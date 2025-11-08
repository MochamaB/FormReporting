using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Reusable field definitions that can be used across multiple form templates
    /// </summary>
    [Table("FieldLibrary")]
    public class FieldLibrary
    {
        [Key]
        public int LibraryFieldId { get; set; }

        [Required]
        [StringLength(200)]
        public string FieldName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FieldCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FieldType { get; set; } = string.Empty; // Text, Number, Boolean, Date, Dropdown, etc.

        [StringLength(100)]
        public string? Category { get; set; } // 'Common', 'HR', 'ICT', 'Finance'

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? DefaultConfiguration { get; set; } // JSON: complete field setup (validations, options, etc.)

        public bool IsActive { get; set; } = true;

        [Required]
        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(CreatedBy))]
        public virtual User Creator { get; set; } = null!;

        public virtual ICollection<FormTemplateItem> FormTemplateItems { get; set; } = new List<FormTemplateItem>();
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Groups questions/fields within a form template (sections/pages)
    /// </summary>
    [Table("FormTemplateSections")]
    public class FormTemplateSection
    {
        [Key]
        public int SectionId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [Required]
        [StringLength(100)]
        public string SectionName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? SectionDescription { get; set; }

        [Required]
        public int DisplayOrder { get; set; } = 0;

        public bool IsCollapsible { get; set; } = true;

        public bool IsCollapsedByDefault { get; set; } = false;

        [StringLength(50)]
        public string? IconClass { get; set; } // e.g., 'fa-desktop', 'fa-network-wired', 'fa-cube'

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(TemplateId))]
        public virtual FormTemplate Template { get; set; } = null!;

        public virtual ICollection<FormTemplateItem> Items { get; set; } = new List<FormTemplateItem>();
        public virtual ICollection<SectionRouting> SourceRoutings { get; set; } = new List<SectionRouting>();
        public virtual ICollection<SectionRouting> TargetRoutings { get; set; } = new List<SectionRouting>();
    }
}

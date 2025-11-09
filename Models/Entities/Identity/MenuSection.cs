using FormReporting.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    [Table("MenuSections")]
    public class MenuSection : BaseEntity, IActivatable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuSectionId { get; set; }

        [Required]
        [StringLength(100)]
        public string SectionName { get; set; } = string.Empty;

        [StringLength(10)]
        public string? SectionCode { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
    }
}
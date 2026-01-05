using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Junction table linking template metric mappings to their source section mappings
    /// </summary>
    [Table("FormTemplateMetricSources")]
    public class FormTemplateMetricSource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TemplateMappingId { get; set; }

        [Required]
        public int SectionMappingId { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        public decimal? Weight { get; set; } // For weighted averages

        public int DisplayOrder { get; set; } = 0;

        // Navigation properties
        [ForeignKey(nameof(TemplateMappingId))]
        public virtual FormTemplateMetricMapping TemplateMapping { get; set; } = null!;

        [ForeignKey(nameof(SectionMappingId))]
        public virtual FormSectionMetricMapping SectionMapping { get; set; } = null!;
    }
}

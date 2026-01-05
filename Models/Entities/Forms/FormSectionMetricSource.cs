using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Junction table linking section metric mappings to their source item mappings
    /// </summary>
    [Table("FormSectionMetricSources")]
    public class FormSectionMetricSource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SectionMappingId { get; set; }

        [Required]
        public int ItemMappingId { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        public decimal? Weight { get; set; } // For weighted averages

        public int DisplayOrder { get; set; } = 0;

        // Navigation properties
        [ForeignKey(nameof(SectionMappingId))]
        public virtual FormSectionMetricMapping SectionMapping { get; set; } = null!;

        [ForeignKey(nameof(ItemMappingId))]
        public virtual FormItemMetricMapping ItemMapping { get; set; } = null!;
    }
}

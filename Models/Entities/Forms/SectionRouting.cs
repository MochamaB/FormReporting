using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Skip logic - jump to different sections based on answers
    /// </summary>
    [Table("SectionRouting")]
    public class SectionRouting
    {
        [Key]
        public int RoutingId { get; set; }

        [Required]
        public int SourceSectionId { get; set; }

        [Required]
        public int SourceItemId { get; set; } // Question that triggers routing

        public int? TargetSectionId { get; set; } // NULL = end form (go to submit)

        [Required]
        [StringLength(20)]
        public string ConditionType { get; set; } = string.Empty; // 'equals', 'not_equals', 'contains', 'greater_than', 'less_than', 'is_empty'

        [StringLength(500)]
        public string? ConditionValue { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(SourceSectionId))]
        public virtual FormTemplateSection SourceSection { get; set; } = null!;

        [ForeignKey(nameof(SourceItemId))]
        public virtual FormTemplateItem SourceItem { get; set; } = null!;

        [ForeignKey(nameof(TargetSectionId))]
        public virtual FormTemplateSection? TargetSection { get; set; }
    }
}

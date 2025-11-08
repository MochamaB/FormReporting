using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Defines multi-level approval workflows
    /// </summary>
    [Table("WorkflowDefinitions")]
    public class WorkflowDefinition
    {
        [Key]
        public int WorkflowId { get; set; }

        [Required]
        [StringLength(100)]
        public string WorkflowName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(CreatedBy))]
        public virtual User Creator { get; set; } = null!;

        public virtual ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
        public virtual ICollection<FormTemplate> FormTemplates { get; set; } = new List<FormTemplate>();
    }
}

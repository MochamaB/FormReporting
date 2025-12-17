using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Defines workflow action types (Fill, Sign, Approve, Reject, Review, Verify)
    /// </summary>
    [Table("WorkflowActions")]
    public class WorkflowAction
    {
        [Key]
        public int ActionId { get; set; }

        [Required]
        [StringLength(50)]
        public string ActionCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ActionName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Whether this action requires a signature (e.g., Sign action)
        /// </summary>
        public bool RequiresSignature { get; set; } = false;

        /// <summary>
        /// Whether this action requires a comment (e.g., Reject action)
        /// </summary>
        public bool RequiresComment { get; set; } = false;

        /// <summary>
        /// Whether this action can be delegated to another user
        /// </summary>
        public bool AllowDelegate { get; set; } = true;

        /// <summary>
        /// Icon class for UI display (e.g., "bi-pen", "bi-check-circle")
        /// </summary>
        [StringLength(50)]
        public string? IconClass { get; set; }

        /// <summary>
        /// CSS class for styling (e.g., "text-success", "text-danger")
        /// </summary>
        [StringLength(50)]
        public string? CssClass { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<WorkflowStep> WorkflowSteps { get; set; } = new List<WorkflowStep>();
    }
}

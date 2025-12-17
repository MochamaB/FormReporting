using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Core form template definition with publish workflow and approval configuration
    /// </summary>
    [Table("FormTemplates")]
    public class FormTemplate
    {
        [Key]
        public int TemplateId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string TemplateType { get; set; } = string.Empty; // Daily, Weekly, Monthly, Quarterly, Annual

        public int Version { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public bool RequiresApproval { get; set; } = true;

        public int? WorkflowId { get; set; } // Link to approval workflow

        // Publish Status Workflow
        [Required]
        [StringLength(20)]
        public string PublishStatus { get; set; } = "Draft"; // Draft, Published, Archived, Deprecated

        public DateTime? PublishedDate { get; set; }

        public int? PublishedBy { get; set; }

        public DateTime? ArchivedDate { get; set; }

        public int? ArchivedBy { get; set; }

        [StringLength(500)]
        public string? ArchivedReason { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(CategoryId))]
        public virtual FormCategory Category { get; set; } = null!;

        [ForeignKey(nameof(WorkflowId))]
        public virtual WorkflowDefinition? Workflow { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User Creator { get; set; } = null!;

        [ForeignKey(nameof(ModifiedBy))]
        public virtual User? Modifier { get; set; }

        [ForeignKey(nameof(PublishedBy))]
        public virtual User? Publisher { get; set; }

        [ForeignKey(nameof(ArchivedBy))]
        public virtual User? Archiver { get; set; }

        public virtual ICollection<FormTemplateSection> Sections { get; set; } = new List<FormTemplateSection>();
        public virtual ICollection<FormTemplateItem> Items { get; set; } = new List<FormTemplateItem>();
        public virtual ICollection<FormTemplateSubmission> Submissions { get; set; } = new List<FormTemplateSubmission>();
        public virtual ICollection<FormTemplateAssignment> Assignments { get; set; } = new List<FormTemplateAssignment>();
        public virtual ICollection<FormTemplateSubmissionRule> SubmissionRules { get; set; } = new List<FormTemplateSubmissionRule>();
        public virtual ICollection<FormAnalytics> Analytics { get; set; } = new List<FormAnalytics>();
    }
}

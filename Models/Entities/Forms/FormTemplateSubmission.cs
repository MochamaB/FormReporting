using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Instances of form template completion by users
    /// </summary>
    [Table("FormTemplateSubmissions")]
    public class FormTemplateSubmission
    {
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        public int? TenantId { get; set; } // Nullable for non-location forms (appraisals, training feedback)

        [Required]
        public int ReportingYear { get; set; }

        [Required]
        public byte ReportingMonth { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime ReportingPeriod { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime SnapshotDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, InApproval, Approved, Rejected

        [Required]
        public int SubmittedBy { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public int? ReviewedBy { get; set; } // For simple single-level approvals

        public DateTime? ReviewedDate { get; set; }

        public string? ApprovalComments { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(TemplateId))]
        public virtual FormTemplate Template { get; set; } = null!;

        [ForeignKey(nameof(TenantId))]
        public virtual Tenant? Tenant { get; set; }

        [ForeignKey(nameof(SubmittedBy))]
        public virtual User Submitter { get; set; } = null!;

        [ForeignKey(nameof(ReviewedBy))]
        public virtual User? Reviewer { get; set; }

        [ForeignKey(nameof(ModifiedBy))]
        public virtual User? Modifier { get; set; }

        public virtual ICollection<FormTemplateResponse> Responses { get; set; } = new List<FormTemplateResponse>();
        public virtual ICollection<SubmissionWorkflowProgress> WorkflowProgress { get; set; } = new List<SubmissionWorkflowProgress>();
        public virtual ICollection<MetricPopulationLog> MetricPopulationLogs { get; set; } = new List<MetricPopulationLog>();
        public virtual ICollection<FormAnalytics> Analytics { get; set; } = new List<FormAnalytics>();
    }
}

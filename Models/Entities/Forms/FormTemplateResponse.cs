using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Individual field answers using EAV pattern for flexible data storage
    /// </summary>
    [Table("FormTemplateResponses")]
    public class FormTemplateResponse
    {
        [Key]
        public long ResponseId { get; set; }

        [Required]
        public int SubmissionId { get; set; }

        [Required]
        public int ItemId { get; set; }

        public string? TextValue { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? NumericValue { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateValue { get; set; }

        public bool? BooleanValue { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(SubmissionId))]
        public virtual FormTemplateSubmission Submission { get; set; } = null!;

        [ForeignKey(nameof(ItemId))]
        public virtual FormTemplateItem Item { get; set; } = null!;
    }
}

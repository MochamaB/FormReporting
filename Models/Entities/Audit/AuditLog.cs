using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Audit
{
    /// <summary>
    /// Audit logs for tracking data changes across all tables
    /// </summary>
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuditId { get; set; }

        [Required]
        [StringLength(100)]
        public string TableName { get; set; } = string.Empty;

        [Required]
        public int RecordId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public int? ChangedBy { get; set; }

        public DateTime ChangedDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? IPAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        // Navigation properties
        [ForeignKey(nameof(ChangedBy))]
        public virtual User? Changer { get; set; }
    }
}

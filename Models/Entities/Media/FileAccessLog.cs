using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Media
{
    /// <summary>
    /// Security audit trail - tracks who accessed which files
    /// </summary>
    [Table("FileAccessLog")]
    public class FileAccessLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccessLogId { get; set; }

        [Required]
        public long FileId { get; set; }

        [Required]
        public int AccessedBy { get; set; }

        public DateTime AccessDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string AccessType { get; set; } = string.Empty;

        [StringLength(50)]
        public string? IPAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(20)]
        public string AccessResult { get; set; } = "Success";

        // Navigation properties
        [ForeignKey(nameof(FileId))]
        public virtual MediaFile File { get; set; } = null!;

        [ForeignKey(nameof(AccessedBy))]
        public virtual User Accessor { get; set; } = null!;
    }
}

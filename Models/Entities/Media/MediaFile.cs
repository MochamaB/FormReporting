using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Media
{
    /// <summary>
    /// Master file storage for all uploads across the system
    /// </summary>
    [Table("MediaFiles")]
    public class MediaFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FileId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? FileExtension { get; set; }

        [StringLength(100)]
        public string? MimeType { get; set; }

        [Required]
        [StringLength(20)]
        public string StorageProvider { get; set; } = "Local";

        [Required]
        [StringLength(1000)]
        public string StoragePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string? StorageContainer { get; set; }

        [Required]
        public long FileSize { get; set; }

        [StringLength(64)]
        public string? FileHash { get; set; }

        public bool IsImage { get; set; } = false;

        public int? ImageWidth { get; set; }

        public int? ImageHeight { get; set; }

        [StringLength(1000)]
        public string? ThumbnailPath { get; set; }

        public int? PageCount { get; set; }

        [StringLength(500)]
        public string? DocumentTitle { get; set; }

        [StringLength(20)]
        public string AccessLevel { get; set; } = "Private";

        public bool IsEncrypted { get; set; } = false;

        [StringLength(500)]
        public string? EncryptionKey { get; set; }

        public bool IsVirusSafe { get; set; } = false;

        public DateTime? VirusScanDate { get; set; }

        [StringLength(100)]
        public string? VirusScanResult { get; set; }

        [Required]
        public int UploadedBy { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastAccessedDate { get; set; }

        public int AccessCount { get; set; } = 0;

        public DateTime? ExpiryDate { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedDate { get; set; }

        public int? DeletedBy { get; set; }

        [StringLength(500)]
        public string? DeleteReason { get; set; }

        public string? Tags { get; set; }

        public string? SearchableText { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UploadedBy))]
        public virtual User Uploader { get; set; } = null!;

        [ForeignKey(nameof(DeletedBy))]
        public virtual User? Deleter { get; set; }

        public virtual ICollection<EntityMediaFile> EntityMediaFiles { get; set; } = new List<EntityMediaFile>();

        public virtual ICollection<FileAccessLog> AccessLogs { get; set; } = new List<FileAccessLog>();
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Media
{
    /// <summary>
    /// Polymorphic association - links files to any entity/record in the system
    /// </summary>
    [Table("EntityMediaFiles")]
    public class EntityMediaFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long EntityMediaId { get; set; }

        [Required]
        public long FileId { get; set; }

        [Required]
        [StringLength(50)]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        public long EntityId { get; set; }

        [StringLength(50)]
        public string? AttachmentType { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsPrimary { get; set; } = false;

        public bool IsRequired { get; set; } = false;

        [StringLength(500)]
        public string? Caption { get; set; }

        [StringLength(100)]
        public string? FieldName { get; set; }

        public long? ResponseId { get; set; }

        [Required]
        public int AttachedBy { get; set; }

        public DateTime AttachedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(FileId))]
        public virtual MediaFile File { get; set; } = null!;

        [ForeignKey(nameof(AttachedBy))]
        public virtual User Attacher { get; set; } = null!;
    }
}

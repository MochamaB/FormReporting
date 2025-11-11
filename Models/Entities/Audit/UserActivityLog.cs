using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Audit
{
    /// <summary>
    /// User activity log for tracking user actions across the system
    /// </summary>
    [Table("UserActivityLog")]
    public class UserActivityLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ActivityId { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(50)]
        public string? ActivityType { get; set; }

        [StringLength(100)]
        public string? EntityType { get; set; }

        public int? EntityId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? IPAddress { get; set; }

        [StringLength(500)]
        public string? DeviceInfo { get; set; }

        public DateTime ActivityDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}

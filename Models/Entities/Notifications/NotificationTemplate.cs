using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Common;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Notifications
{
    /// <summary>
    /// Reusable notification message templates
    /// </summary>
    [Table("NotificationTemplates")]
    public class NotificationTemplate : BaseEntity, IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TemplateId { get; set; }

        /// <summary>
        /// Template code (unique identifier)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string TemplateCode { get; set; } = string.Empty;

        /// <summary>
        /// Template name
        /// </summary>
        [Required]
        [StringLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// Template description
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Template category
        /// </summary>
        [StringLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Subject template (supports placeholders)
        /// </summary>
        [Required]
        [StringLength(500)]
        public string SubjectTemplate { get; set; } = string.Empty;

        /// <summary>
        /// Body template (supports placeholders and HTML)
        /// </summary>
        [Required]
        public string BodyTemplate { get; set; } = string.Empty;

        /// <summary>
        /// SMS template (short version for SMS)
        /// </summary>
        [StringLength(500)]
        public string? SmsTemplate { get; set; }

        /// <summary>
        /// Push notification template (short version)
        /// </summary>
        [StringLength(500)]
        public string? PushTemplate { get; set; }

        /// <summary>
        /// Available placeholders (JSON array)
        /// Example: ["{{UserName}}", "{{FormName}}", "{{DueDate}}"]
        /// </summary>
        public string? AvailablePlaceholders { get; set; }

        /// <summary>
        /// Default priority: Low, Normal, High, Urgent
        /// </summary>
        [StringLength(20)]
        public string DefaultPriority { get; set; } = "Normal";

        /// <summary>
        /// Default channels to use (JSON array)
        /// Example: ["Email", "InApp"]
        /// </summary>
        public string? DefaultChannels { get; set; }

        /// <summary>
        /// Is this template active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Is this a system template (cannot be deleted)?
        /// </summary>
        public bool IsSystemTemplate { get; set; } = false;

        /// <summary>
        /// User who created this template
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// User who last modified this template
        /// </summary>
        public int? ModifiedBy { get; set; }

        // Navigation properties
        /// <summary>
        /// User who created this template
        /// </summary>
        [ForeignKey(nameof(CreatedBy))]
        public virtual User? Creator { get; set; }

        /// <summary>
        /// User who last modified this template
        /// </summary>
        [ForeignKey(nameof(ModifiedBy))]
        public virtual User? Modifier { get; set; }

        /// <summary>
        /// Notifications generated from this template
        /// </summary>
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        /// <summary>
        /// Alert definitions using this template
        /// </summary>
        public virtual ICollection<AlertDefinition> AlertDefinitions { get; set; } = new List<AlertDefinition>();
    }
}

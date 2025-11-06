using FormReporting.Models.Common;
using FormReporting.Models.Entities.Organizational;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    /// <summary>
    /// Represents explicit tenant access exceptions (outside normal role-based access)
    /// </summary>
    [Table("UserTenantAccess")]
    public class UserTenantAccess : IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccessId { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Tenant ID
        /// </summary>
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// User ID who granted this access
        /// </summary>
        [Required]
        public int GrantedBy { get; set; }

        /// <summary>
        /// Date when access was granted
        /// </summary>
        public DateTime GrantedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional expiry date for time-limited access
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Reason why this access was granted (e.g., audit, project, temporary assignment)
        /// </summary>
        [StringLength(500)]
        public string? Reason { get; set; }

        /// <summary>
        /// Indicates if this access is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        /// <summary>
        /// User who has this access
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Tenant this user has access to
        /// </summary>
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;

        /// <summary>
        /// User who granted this access
        /// </summary>
        [ForeignKey(nameof(GrantedBy))]
        public virtual User Grantor { get; set; } = null!;
    }
}

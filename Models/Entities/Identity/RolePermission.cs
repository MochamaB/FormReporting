using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    /// <summary>
    /// Represents the assignment of permissions to roles
    /// </summary>
    [Table("RolePermissions")]
    public class RolePermission
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RolePermissionId { get; set; }

        /// <summary>
        /// Role ID
        /// </summary>
        [Required]
        public int RoleId { get; set; }

        /// <summary>
        /// Permission ID
        /// </summary>
        [Required]
        public int PermissionId { get; set; }

        /// <summary>
        /// Allow/Deny: 1 = granted, 0 = explicitly denied
        /// </summary>
        public bool IsGranted { get; set; } = true;

        /// <summary>
        /// Date when the permission was assigned
        /// </summary>
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User ID who assigned this permission
        /// </summary>
        public int? AssignedBy { get; set; }

        // Navigation properties
        /// <summary>
        /// Role this permission is assigned to
        /// </summary>
        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; } = null!;

        /// <summary>
        /// Permission being assigned
        /// </summary>
        [ForeignKey(nameof(PermissionId))]
        public virtual Permission Permission { get; set; } = null!;

        /// <summary>
        /// User who assigned this permission
        /// </summary>
        [ForeignKey(nameof(AssignedBy))]
        public virtual User? Assigner { get; set; }
    }
}

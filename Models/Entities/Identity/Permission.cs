using FormReporting.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    /// <summary>
    /// Represents granular functional permissions within modules
    /// </summary>
    [Table("Permissions")]
    public class Permission : IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PermissionId { get; set; }

        /// <summary>
        /// Module ID this permission belongs to
        /// </summary>
        [Required]
        public int ModuleId { get; set; }

        /// <summary>
        /// Permission name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string PermissionName { get; set; } = string.Empty;

        /// <summary>
        /// Unique permission code (e.g., 'Templates.Design', 'Forms.Submit', 'Forms.Approve')
        /// </summary>
        [Required]
        [StringLength(100)]
        public string PermissionCode { get; set; } = string.Empty;

        /// <summary>
        /// Description of the permission
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Permission type: View, Create, Edit, Delete, Approve, Export, Manage, Custom
        /// </summary>
        [Required]
        [StringLength(20)]
        public string PermissionType { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the permission is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when the permission was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// Module this permission belongs to
        /// </summary>
        [ForeignKey(nameof(ModuleId))]
        public virtual Module Module { get; set; } = null!;

        /// <summary>
        /// Role assignments for this permission
        /// </summary>
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}

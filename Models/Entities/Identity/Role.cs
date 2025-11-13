using FormReporting.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    /// <summary>
    /// Represents a role in the RBAC system with hierarchical scope-based access
    /// </summary>
    [Table("Roles")]
    public class Role : IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }

        /// <summary>
        /// Unique role name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Unique role code
        /// </summary>
        [Required]
        [StringLength(50)]
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// Description of the role
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Foreign key to ScopeLevel - defines the access scope for this role
        /// Determines the breadth of data visibility (Global, Regional, Tenant, Department, etc.)
        /// </summary>
        [Required]
        public int ScopeLevelId { get; set; }

        /// <summary>
        /// Indicates if the role is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when the role was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// The scope level that defines this role's data access breadth
        /// </summary>
        [ForeignKey(nameof(ScopeLevelId))]
        public virtual ScopeLevel ScopeLevel { get; set; } = null!;

        /// <summary>
        /// Users assigned to this role
        /// </summary>
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Permissions assigned to this role
        /// </summary>
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        /// <summary>
        /// Menu items visible to this role
        /// </summary>
        public virtual ICollection<RoleMenuItem> RoleMenuItems { get; set; } = new List<RoleMenuItem>();
    }
}

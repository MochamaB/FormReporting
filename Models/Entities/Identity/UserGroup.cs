using FormReporting.Models.Common;
using FormReporting.Models.Entities.Organizational;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    /// <summary>
    /// Represents user groups (training cohorts, project teams, committees)
    /// </summary>
    [Table("UserGroups")]
    public class UserGroup : BaseEntity, IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserGroupId { get; set; }

        /// <summary>
        /// Tenant ID (optional, for tenant-specific groups)
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Group name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Unique group code
        /// </summary>
        [Required]
        [StringLength(50)]
        public string GroupCode { get; set; } = string.Empty;

        /// <summary>
        /// Group type (e.g., Training, Project, Committee)
        /// </summary>
        [StringLength(50)]
        public string? GroupType { get; set; }

        /// <summary>
        /// Description of the group
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Indicates if the group is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// User ID who created this group
        /// </summary>
        [Required]
        public int CreatedBy { get; set; }

        // Navigation properties
        /// <summary>
        /// Tenant this group belongs to (optional)
        /// </summary>
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant? Tenant { get; set; }

        /// <summary>
        /// User who created this group
        /// </summary>
        [ForeignKey(nameof(CreatedBy))]
        public virtual User Creator { get; set; } = null!;

        /// <summary>
        /// Members of this group
        /// </summary>
        public virtual ICollection<UserGroupMember> Members { get; set; } = new List<UserGroupMember>();
    }
}

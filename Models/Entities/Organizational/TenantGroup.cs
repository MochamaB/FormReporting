using FormReporting.Models.Common;
using FormReporting.Models.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Organizational
{
    /// <summary>
    /// Represents custom groupings of tenants for flexible assignment
    /// </summary>
    [Table("TenantGroups")]
    public class TenantGroup : BaseEntity, IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TenantGroupId { get; set; }

        /// <summary>
        /// Unique group name
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
        /// Description of the tenant group
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
        /// User who created this group
        /// </summary>
        [ForeignKey(nameof(CreatedBy))]
        public virtual User Creator { get; set; } = null!;

        /// <summary>
        /// Members of this tenant group
        /// </summary>
        public virtual ICollection<TenantGroupMember> Members { get; set; } = new List<TenantGroupMember>();
    }
}

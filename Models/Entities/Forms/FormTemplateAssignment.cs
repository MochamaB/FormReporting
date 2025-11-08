using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Form template assignments with 8 assignment types (tenant-based + user-based)
    /// </summary>
    [Table("FormTemplateAssignments")]
    public class FormTemplateAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        // Assignment Type (8 types total)
        [Required]
        [StringLength(50)]
        public string AssignmentType { get; set; } = string.Empty;
        // Tenant-based: 'All', 'TenantType', 'TenantGroup', 'SpecificTenant'
        // User-based: 'Role', 'Department', 'UserGroup', 'SpecificUser'

        // Tenant-based assignment targets
        [StringLength(20)]
        public string? TenantType { get; set; }

        public int? TenantGroupId { get; set; }

        public int? TenantId { get; set; }

        // User-based assignment targets
        public int? RoleId { get; set; }

        public int? DepartmentId { get; set; }

        public int? UserGroupId { get; set; }

        public int? UserId { get; set; }

        // Metadata
        [Required]
        public int AssignedBy { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey(nameof(TemplateId))]
        public virtual FormTemplate Template { get; set; } = null!;

        [ForeignKey(nameof(TenantGroupId))]
        public virtual TenantGroup? TenantGroup { get; set; }

        [ForeignKey(nameof(TenantId))]
        public virtual Tenant? Tenant { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Role? Role { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }

        [ForeignKey(nameof(UserGroupId))]
        public virtual UserGroup? UserGroup { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(AssignedBy))]
        public virtual User AssignedByUser { get; set; } = null!;
    }
}

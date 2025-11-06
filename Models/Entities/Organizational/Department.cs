using FormReporting.Models.Common;
using FormReporting.Models.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Organizational
{
    /// <summary>
    /// Represents organizational departments within tenants
    /// </summary>
    [Table("Departments")]
    public class Department : BaseEntity, IActivatable, IMultiTenant
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepartmentId { get; set; }

        /// <summary>
        /// Tenant ID this department belongs to
        /// </summary>
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// Name of the department
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        /// <summary>
        /// Unique department code within the tenant
        /// </summary>
        [Required]
        [StringLength(50)]
        public string DepartmentCode { get; set; } = string.Empty;

        /// <summary>
        /// Description of the department
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Parent department ID for hierarchical structure (nullable for top-level departments)
        /// </summary>
        public int? ParentDepartmentId { get; set; }

        /// <summary>
        /// Indicates if the department is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        /// <summary>
        /// Tenant this department belongs to
        /// </summary>
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;

        /// <summary>
        /// Parent department (for hierarchical structure)
        /// </summary>
        [ForeignKey(nameof(ParentDepartmentId))]
        public virtual Department? ParentDepartment { get; set; }

        /// <summary>
        /// Child departments
        /// </summary>
        [InverseProperty(nameof(ParentDepartment))]
        public virtual ICollection<Department> ChildDepartments { get; set; } = new List<Department>();

        /// <summary>
        /// Users in this department
        /// </summary>
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}

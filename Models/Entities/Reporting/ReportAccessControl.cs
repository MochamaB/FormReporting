using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;
using FormReporting.Models.Entities.Organizational;

namespace FormReporting.Models.Entities.Reporting
{
    [Table("ReportAccessControl")]
    public class ReportAccessControl
    {
        [Key]
        public int AccessId { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        [StringLength(20)]
        public string AccessType { get; set; } = string.Empty;

        public int? UserId { get; set; }

        public int? RoleId { get; set; }

        public int? DepartmentId { get; set; }

        [Required]
        [StringLength(20)]
        public string PermissionLevel { get; set; } = string.Empty;

        [Required]
        public int GrantedBy { get; set; }

        public DateTime GrantedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(ReportId))]
        public virtual ReportDefinition Report { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Role? Role { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }

        [ForeignKey(nameof(GrantedBy))]
        public virtual User Grantor { get; set; } = null!;
    }
}

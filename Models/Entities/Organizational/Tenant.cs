using FormReporting.Models.Common;
using FormReporting.Models.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Organizational
{
    /// <summary>
    /// Unified tenant table representing HeadOffice, Factories, and Subsidiaries
    /// </summary>
    [Table("Tenants")]
    public class Tenant : BaseEntity, IActivatable, IAuditable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TenantId { get; set; }

        /// <summary>
        /// Type of tenant: HeadOffice, Factory, or Subsidiary
        /// </summary>
        [Required]
        [StringLength(20)]
        public string TenantType { get; set; } = string.Empty;

        /// <summary>
        /// Unique tenant code
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TenantCode { get; set; } = string.Empty;

        /// <summary>
        /// Name of the tenant
        /// </summary>
        [Required]
        [StringLength(200)]
        public string TenantName { get; set; } = string.Empty;

        /// <summary>
        /// Region ID (only for Factory type tenants)
        /// </summary>
        public int? RegionId { get; set; }

        /// <summary>
        /// Physical location/address
        /// </summary>
        [StringLength(500)]
        public string? Location { get; set; }

        /// <summary>
        /// Geographic latitude
        /// </summary>
        [Column(TypeName = "decimal(10, 7)")]
        public decimal? Latitude { get; set; }

        /// <summary>
        /// Geographic longitude
        /// </summary>
        [Column(TypeName = "decimal(10, 7)")]
        public decimal? Longitude { get; set; }

        /// <summary>
        /// Contact phone number
        /// </summary>
        [StringLength(50)]
        public string? ContactPhone { get; set; }

        /// <summary>
        /// Contact email address
        /// </summary>
        [StringLength(200)]
        [EmailAddress]
        public string? ContactEmail { get; set; }

        /// <summary>
        /// User ID of the tenant manager
        /// </summary>
        public int? ManagerUserId { get; set; }

        /// <summary>
        /// User ID of the ICT support person
        /// </summary>
        public int? ICTSupportUserId { get; set; }

        /// <summary>
        /// Indicates if the tenant is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// User ID who created this record
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// User ID who last modified this record
        /// </summary>
        public int? ModifiedBy { get; set; }

        // Navigation properties
        /// <summary>
        /// Region this tenant belongs to (for Factory type only)
        /// </summary>
        [ForeignKey(nameof(RegionId))]
        public virtual Region? Region { get; set; }

        /// <summary>
        /// Manager user
        /// </summary>
        [ForeignKey(nameof(ManagerUserId))]
        public virtual User? Manager { get; set; }

        /// <summary>
        /// ICT Support user
        /// </summary>
        [ForeignKey(nameof(ICTSupportUserId))]
        public virtual User? ICTSupport { get; set; }

        /// <summary>
        /// User who created this record
        /// </summary>
        [ForeignKey(nameof(CreatedBy))]
        public virtual User? Creator { get; set; }

        /// <summary>
        /// User who last modified this record
        /// </summary>
        [ForeignKey(nameof(ModifiedBy))]
        public virtual User? Modifier { get; set; }

        /// <summary>
        /// Departments in this tenant
        /// </summary>
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

        /// <summary>
        /// Tenant group memberships
        /// </summary>
        public virtual ICollection<TenantGroupMember> GroupMemberships { get; set; } = new List<TenantGroupMember>();
    }
}

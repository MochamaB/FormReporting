using FormReporting.Models.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Organizational
{
    /// <summary>
    /// Represents membership of a tenant in a tenant group
    /// </summary>
    [Table("TenantGroupMembers")]
    public class TenantGroupMember
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroupMemberId { get; set; }

        /// <summary>
        /// Tenant group ID
        /// </summary>
        [Required]
        public int TenantGroupId { get; set; }

        /// <summary>
        /// Tenant ID
        /// </summary>
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// User ID who added this member
        /// </summary>
        [Required]
        public int AddedBy { get; set; }

        /// <summary>
        /// Date when the member was added (UTC)
        /// </summary>
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// Tenant group this membership belongs to
        /// </summary>
        [ForeignKey(nameof(TenantGroupId))]
        public virtual TenantGroup TenantGroup { get; set; } = null!;

        /// <summary>
        /// Tenant that is a member of the group
        /// </summary>
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;

        /// <summary>
        /// User who added this tenant to the group
        /// </summary>
        [ForeignKey(nameof(AddedBy))]
        public virtual User AddedByUser { get; set; } = null!;
    }
}

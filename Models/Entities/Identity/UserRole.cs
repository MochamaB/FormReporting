using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    /// <summary>
    /// Represents the many-to-many relationship between Users and Roles
    /// </summary>
    [Table("UserRoles")]
    public class UserRole
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserRoleId { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Role ID
        /// </summary>
        [Required]
        public int RoleId { get; set; }

        /// <summary>
        /// Date when the role was assigned
        /// </summary>
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User ID who assigned this role
        /// </summary>
        public int? AssignedBy { get; set; }

        // Navigation properties
        /// <summary>
        /// User this role is assigned to
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Role being assigned
        /// </summary>
        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; } = null!;

        /// <summary>
        /// User who assigned this role
        /// </summary>
        [ForeignKey(nameof(AssignedBy))]
        public virtual User? Assigner { get; set; }
    }
}

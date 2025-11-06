using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    /// <summary>
    /// Represents membership of users in user groups
    /// </summary>
    [Table("UserGroupMembers")]
    public class UserGroupMember
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserGroupMemberId { get; set; }

        /// <summary>
        /// User group ID
        /// </summary>
        [Required]
        public int UserGroupId { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// User ID who added this member
        /// </summary>
        [Required]
        public int AddedBy { get; set; }

        /// <summary>
        /// Date when the member was added
        /// </summary>
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// User group this membership belongs to
        /// </summary>
        [ForeignKey(nameof(UserGroupId))]
        public virtual UserGroup UserGroup { get; set; } = null!;

        /// <summary>
        /// User who is a member of the group
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// User who added this member to the group
        /// </summary>
        [ForeignKey(nameof(AddedBy))]
        public virtual User AddedByUser { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    /// <summary>
    /// Represents role-based menu visibility control
    /// </summary>
    [Table("RoleMenuItems")]
    public class RoleMenuItem
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleMenuItemId { get; set; }

        /// <summary>
        /// Role ID
        /// </summary>
        [Required]
        public int RoleId { get; set; }

        /// <summary>
        /// Menu item ID
        /// </summary>
        [Required]
        public int MenuItemId { get; set; }

        /// <summary>
        /// Indicates if the menu item is visible to this role
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Date when this menu was assigned
        /// </summary>
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// Role this menu item is assigned to
        /// </summary>
        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; } = null!;

        /// <summary>
        /// Menu item being assigned
        /// </summary>
        [ForeignKey(nameof(MenuItemId))]
        public virtual MenuItem MenuItem { get; set; } = null!;
    }
}

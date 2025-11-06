using FormReporting.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    /// <summary>
    /// Represents dynamic sidebar navigation menu items
    /// </summary>
    [Table("MenuItems")]
    public class MenuItem : BaseEntity, IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuItemId { get; set; }

        /// <summary>
        /// Parent menu item ID (for hierarchical menus)
        /// </summary>
        public int? ParentMenuItemId { get; set; }

        /// <summary>
        /// Module ID this menu item is linked to
        /// </summary>
        public int? ModuleId { get; set; }

        /// <summary>
        /// Menu title displayed to users
        /// </summary>
        [Required]
        [StringLength(100)]
        public string MenuTitle { get; set; } = string.Empty;

        /// <summary>
        /// Unique menu code
        /// </summary>
        [Required]
        [StringLength(50)]
        public string MenuCode { get; set; } = string.Empty;

        /// <summary>
        /// Icon class for menu item
        /// </summary>
        [StringLength(50)]
        public string? Icon { get; set; }

        /// <summary>
        /// URL path (e.g., '/Forms/Assigned', '/Dashboard/Index')
        /// </summary>
        [StringLength(200)]
        public string? Route { get; set; }

        /// <summary>
        /// Controller name
        /// </summary>
        [StringLength(100)]
        public string? Controller { get; set; }

        /// <summary>
        /// Action name
        /// </summary>
        [StringLength(100)]
        public string? Action { get; set; }

        /// <summary>
        /// Area name
        /// </summary>
        [StringLength(100)]
        public string? Area { get; set; }

        /// <summary>
        /// Display order for sorting
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Menu nesting level (1=top, 2=submenu, 3=nested submenu)
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// Indicates if the menu item is visible
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Indicates if the menu item is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Must be authenticated to see this menu item
        /// </summary>
        public bool RequiresAuth { get; set; } = true;

        // Navigation properties
        /// <summary>
        /// Parent menu item
        /// </summary>
        [ForeignKey(nameof(ParentMenuItemId))]
        public virtual MenuItem? ParentMenuItem { get; set; }

        /// <summary>
        /// Module this menu item is linked to
        /// </summary>
        [ForeignKey(nameof(ModuleId))]
        public virtual Module? Module { get; set; }

        /// <summary>
        /// Child menu items
        /// </summary>
        [InverseProperty(nameof(ParentMenuItem))]
        public virtual ICollection<MenuItem> ChildMenuItems { get; set; } = new List<MenuItem>();

        /// <summary>
        /// Role visibility assignments
        /// </summary>
        public virtual ICollection<RoleMenuItem> RoleMenuItems { get; set; } = new List<RoleMenuItem>();
    }
}

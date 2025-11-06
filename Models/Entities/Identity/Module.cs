using FormReporting.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    /// <summary>
    /// Represents high-level application feature areas (modules)
    /// </summary>
    [Table("Modules")]
    public class Module : IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ModuleId { get; set; }

        /// <summary>
        /// Unique module name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// Unique module code
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ModuleCode { get; set; } = string.Empty;

        /// <summary>
        /// Description of the module
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Icon class (e.g., 'ri-dashboard-line', 'ri-file-list-line')
        /// </summary>
        [StringLength(50)]
        public string? Icon { get; set; }

        /// <summary>
        /// Display order for sorting
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Indicates if the module is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when the module was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// Permissions within this module
        /// </summary>
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

        /// <summary>
        /// Menu items linked to this module
        /// </summary>
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}

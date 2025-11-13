using FormReporting.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    /// <summary>
    /// Defines hierarchical access scope levels for role-based data visibility
    /// Determines the breadth of data access (Global, Regional, Tenant, Department, etc.)
    /// </summary>
    [Table("ScopeLevels")]
    public class ScopeLevel : IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScopeLevelId { get; set; }

        /// <summary>
        /// Display name of the scope level
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ScopeName { get; set; } = string.Empty;

        /// <summary>
        /// Unique code for the scope level
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ScopeCode { get; set; } = string.Empty;

        /// <summary>
        /// Hierarchical level: 1=Global, 2=Regional, 3=Tenant, 4=Department, 5=Team, 6=Individual
        /// Lower number = broader access scope
        /// </summary>
        [Required]
        public int Level { get; set; }

        /// <summary>
        /// Description of what this scope level means
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Indicates if this scope level is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when the scope level was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// Roles that use this scope level
        /// </summary>
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}

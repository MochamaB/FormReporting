using FormReporting.Models.Common;
using FormReporting.Models.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Organizational
{
    /// <summary>
    /// Represents a geographic region for grouping factories
    /// </summary>
    [Table("Regions")]
    public class Region : BaseEntity, IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RegionId { get; set; }

        /// <summary>
        /// Unique region number
        /// </summary>
        [Required]
        public int RegionNumber { get; set; }

        /// <summary>
        /// Name of the region
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RegionName { get; set; } = string.Empty;

        /// <summary>
        /// Unique code for the region
        /// </summary>
        [Required]
        [StringLength(20)]
        public string RegionCode { get; set; } = string.Empty;

        /// <summary>
        /// User ID of the regional manager (nullable, FK added later after Users table)
        /// </summary>
        public int? RegionalManagerUserId { get; set; }

        /// <summary>
        /// Indicates if the region is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        /// <summary>
        /// Regional manager user
        /// </summary>
        [ForeignKey(nameof(RegionalManagerUserId))]
        public virtual User? RegionalManager { get; set; }

        /// <summary>
        /// Tenants (factories) in this region
        /// </summary>
        public virtual ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
    }
}

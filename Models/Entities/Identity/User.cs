using FormReporting.Models.Common;
using FormReporting.Models.Entities.Organizational;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Identity
{
    /// <summary>
    /// Represents a user account (ASP.NET Core Identity compatible)
    /// </summary>
    [Table("Users")]
    public class User : BaseEntity, IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        // ASP.NET Identity fields
        /// <summary>
        /// Unique username
        /// </summary>
        [Required]
        [StringLength(256)]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Normalized username for search/comparison
        /// </summary>
        [Required]
        [StringLength(256)]
        public string NormalizedUserName { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        [Required]
        [StringLength(256)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Normalized email for search/comparison
        /// </summary>
        [Required]
        [StringLength(256)]
        public string NormalizedEmail { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if email is confirmed
        /// </summary>
        public bool EmailConfirmed { get; set; } = false;

        /// <summary>
        /// Hashed password
        /// </summary>
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Security stamp for password changes
        /// </summary>
        public string? SecurityStamp { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Indicates if phone number is confirmed
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; } = false;

        /// <summary>
        /// Indicates if two-factor authentication is enabled
        /// </summary>
        public bool TwoFactorEnabled { get; set; } = false;

        /// <summary>
        /// Date/time when lockout ends (UTC)
        /// </summary>
        public DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// Indicates if lockout is enabled for this user
        /// </summary>
        public bool LockoutEnabled { get; set; } = true;

        /// <summary>
        /// Number of failed access attempts
        /// </summary>
        public int AccessFailedCount { get; set; } = 0;

        // KTDA Custom fields
        /// <summary>
        /// User's first name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Employee number (unique)
        /// </summary>
        [StringLength(50)]
        public string? EmployeeNumber { get; set; }

        /// <summary>
        /// Primary/Home Tenant ID (required)
        /// This is the user's base tenant for scope-based access
        /// </summary>
        [Required]
        public int TenantId { get; set; }

        /// <summary>
        /// Department ID (optional - not all users are in specific departments)
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Indicates if the user is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Last login date/time
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        // Navigation properties
        /// <summary>
        /// Primary/Home Tenant this user belongs to
        /// Relationship configured in UserConfiguration.cs via Fluent API
        /// </summary>
        public virtual Tenant PrimaryTenant { get; set; } = null!;

        /// <summary>
        /// Department this user belongs to (optional)
        /// </summary>
        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }

        /// <summary>
        /// Role assignments for this user
        /// </summary>
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Tenant access exceptions/additions for this user
        /// Grants access to tenants beyond their primary scope (projects, audits, etc.)
        /// </summary>
        public virtual ICollection<UserTenantAccess> TenantAccesses { get; set; } = new List<UserTenantAccess>();

        /// <summary>
        /// User group memberships
        /// </summary>
        public virtual ICollection<UserGroupMember> GroupMemberships { get; set; } = new List<UserGroupMember>();

        /// <summary>
        /// Full name (computed property)
        /// </summary>
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}

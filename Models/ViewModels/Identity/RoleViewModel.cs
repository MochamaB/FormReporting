using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Identity
{
    /// <summary>
    /// View model for displaying role information
    /// </summary>
    public class RoleViewModel
    {
        public int RoleId { get; set; }

        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [Display(Name = "Role Code")]
        public string RoleCode { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Scope Level")]
        public string ScopeLevelName { get; set; } = string.Empty;

        [Display(Name = "Scope Code")]
        public string ScopeCode { get; set; } = string.Empty;

        [Display(Name = "Level")]
        public int Level { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "User Count")]
        public int UserCount { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Computed property for status badge
        /// </summary>
        public string StatusBadge => IsActive 
            ? "<span class='badge bg-success-subtle text-success'>Active</span>" 
            : "<span class='badge bg-danger-subtle text-danger'>Inactive</span>";

        /// <summary>
        /// Computed property for scope level badge
        /// </summary>
        public string ScopeBadge => Level switch
        {
            1 => "<span class='badge bg-primary-subtle text-primary'>Global</span>",
            2 => "<span class='badge bg-info-subtle text-info'>Regional</span>",
            3 => "<span class='badge bg-warning-subtle text-warning'>Tenant</span>",
            4 => "<span class='badge bg-secondary-subtle text-secondary'>Department</span>",
            5 => "<span class='badge bg-dark-subtle text-dark'>Team</span>",
            6 => "<span class='badge bg-light text-dark'>Individual</span>",
            _ => "<span class='badge bg-light text-dark'>Unknown</span>"
        };
    }

    /// <summary>
    /// View model for creating/editing roles with wizard support
    /// </summary>
    public class RoleEditViewModel
    {
        // Step 1: Basic Details
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Role name is required")]
        [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters")]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role code is required")]
        [StringLength(50, ErrorMessage = "Role code cannot exceed 50 characters")]
        [Display(Name = "Role Code")]
        [RegularExpression(@"^[A-Z_]+$", ErrorMessage = "Role code must contain only uppercase letters and underscores")]
        public string RoleCode { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Scope level is required")]
        [Display(Name = "Scope Level")]
        public int ScopeLevelId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Step 2: Assign Permissions
        /// <summary>
        /// List of selected permission IDs
        /// </summary>
        public List<int> SelectedPermissionIds { get; set; } = new();

        /// <summary>
        /// Optional: Copy permissions from this role ID
        /// </summary>
        public int? CopyFromRoleId { get; set; }

        // Step 3: Assign Users
        /// <summary>
        /// List of selected user IDs to assign to this role
        /// </summary>
        public List<int> SelectedUserIds { get; set; } = new();
    }

    /// <summary>
    /// View model for the Roles index page
    /// </summary>
    public class RolesIndexViewModel
    {
        public IEnumerable<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();
        public int TotalRoles { get; set; }
        public int ActiveRoles { get; set; }
        public int InactiveRoles { get; set; }
    }
}

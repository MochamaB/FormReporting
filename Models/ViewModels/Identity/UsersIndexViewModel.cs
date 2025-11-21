namespace FormReporting.Models.ViewModels.Identity
{
    /// <summary>
    /// ViewModel for Users Index page
    /// Contains statistics and list of scope-filtered users
    /// </summary>
    public class UsersIndexViewModel
    {
        // Statistics for stat cards
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int EmailConfirmedUsers { get; set; }

        // List of users for datatable (scope-filtered)
        public List<UserViewModel> Users { get; set; } = new();
    }

    /// <summary>
    /// Individual user data for table rows
    /// </summary>
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmployeeNumber { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int RoleCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedDate { get; set; }

        // Computed properties for rendering
        public string StatusBadge => IsActive
            ? "<span class=\"badge bg-success-subtle text-success\"><i class=\"ri-checkbox-circle-line me-1\"></i>Active</span>"
            : "<span class=\"badge bg-danger-subtle text-danger\"><i class=\"ri-close-circle-line me-1\"></i>Inactive</span>";

        public string EmailBadge => EmailConfirmed
            ? "<span class=\"badge bg-info-subtle text-info\"><i class=\"ri-mail-check-line me-1\"></i>Verified</span>"
            : "<span class=\"badge bg-warning-subtle text-warning\"><i class=\"ri-mail-line me-1\"></i>Unverified</span>";

        public string TenantBadge => $"<span class=\"badge bg-primary-subtle text-primary\"><i class=\"ri-building-line me-1\"></i>{TenantName}</span>";

        public string LastLoginDisplay => LastLoginDate.HasValue
            ? LastLoginDate.Value.ToString("MMM dd, yyyy")
            : "<span class=\"text-muted\">Never</span>";

        public string RolesBadge => RoleCount > 0
            ? $"<span class=\"badge bg-secondary\">{RoleCount}</span>"
            : "<span class=\"badge bg-light text-muted\">0</span>";
    }
}

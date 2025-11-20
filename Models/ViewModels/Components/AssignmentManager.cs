namespace FormReporting.Models.ViewModels.Components
{
    /// <summary>
    /// Configuration for AssignmentManager component (POCO - what controllers create)
    /// Supports multiple assignment types: Users, Roles, Departments, UserGroups, Tenants
    /// </summary>
    public class AssignmentManagerConfig
    {
        /// <summary>
        /// Unique identifier for this assignment manager instance
        /// Used for DOM IDs and AJAX callbacks
        /// </summary>
        public string ManagerId { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Context label (e.g., "Approval Workflow", "Form Assignments", "User Group Members")
        /// </summary>
        public string ContextLabel { get; set; } = "Assignments";

        /// <summary>
        /// Supported assignment types for this instance
        /// </summary>
        public List<AssignmentType> SupportedTypes { get; set; } = new List<AssignmentType>();

        /// <summary>
        /// Current assignments (existing data)
        /// </summary>
        public List<AssignmentItem> CurrentAssignments { get; set; } = new List<AssignmentItem>();

        /// <summary>
        /// Show level/step ordering (for workflows with multiple approval levels)
        /// </summary>
        public bool ShowLevels { get; set; } = false;

        /// <summary>
        /// Allow multiple assignments per level
        /// </summary>
        public bool AllowMultiplePerLevel { get; set; } = true;

        /// <summary>
        /// Show add button
        /// </summary>
        public bool ShowAddButton { get; set; } = true;

        /// <summary>
        /// Show remove button
        /// </summary>
        public bool ShowRemoveButton { get; set; } = true;

        /// <summary>
        /// Collapsible section (useful for conditional display)
        /// </summary>
        public bool IsCollapsible { get; set; } = false;

        /// <summary>
        /// Initially collapsed (if collapsible)
        /// </summary>
        public bool InitiallyCollapsed { get; set; } = true;

        /// <summary>
        /// Custom CSS classes
        /// </summary>
        public string CssClasses { get; set; } = "";

        /// <summary>
        /// Help text to display
        /// </summary>
        public string? HelpText { get; set; }

        /// <summary>
        /// AJAX endpoint for searching assignable entities
        /// </summary>
        public string? SearchEndpoint { get; set; }
        
        /// <summary>
        /// Path to custom modal content partial for context-specific forms
        /// If null, uses generic modal content
        /// </summary>
        public string? ModalContentPartial { get; set; }
    }

    /// <summary>
    /// View model for AssignmentManager (render-ready - what partials receive)
    /// </summary>
    public class AssignmentManagerViewModel
    {
        public string ManagerId { get; set; } = string.Empty;
        public string ContextLabel { get; set; } = string.Empty;
        public List<AssignmentTypeViewModel> SupportedTypes { get; set; } = new List<AssignmentTypeViewModel>();
        public List<AssignmentItemViewModel> CurrentAssignments { get; set; } = new List<AssignmentItemViewModel>();
        public bool ShowLevels { get; set; }
        public bool AllowMultiplePerLevel { get; set; }
        public bool ShowAddButton { get; set; }
        public bool ShowRemoveButton { get; set; }
        public bool IsCollapsible { get; set; }
        public bool InitiallyCollapsed { get; set; }
        public string CssClasses { get; set; } = string.Empty;
        public string? HelpText { get; set; }
        public string? SearchEndpoint { get; set; }
        public string? ModalContentPartial { get; set; }

        // Computed properties
        public string CollapseId => $"collapse-{ManagerId}";
        public string AddModalId => $"addModal-{ManagerId}";
        public bool HasAssignments => CurrentAssignments.Any();
        public int AssignmentCount => CurrentAssignments.Count;
        public string ContainerCssClasses => $"assignment-manager {CssClasses}".Trim();
    }

    /// <summary>
    /// Assignment type (User, Role, Department, etc.)
    /// </summary>
    public class AssignmentType
    {
        public string TypeCode { get; set; } = string.Empty; // "User", "Role", "Department", "UserGroup", "Tenant"
        public string TypeLabel { get; set; } = string.Empty; // "Specific User", "By Role", etc.
        public string Icon { get; set; } = "ri-user-line";
        public bool IsDefault { get; set; } = false;
    }

    /// <summary>
    /// View model for assignment type
    /// </summary>
    public class AssignmentTypeViewModel
    {
        public string TypeCode { get; set; } = string.Empty;
        public string TypeLabel { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public string CssClasses => IsDefault ? "active" : "";
    }

    /// <summary>
    /// Individual assignment item (existing assignment)
    /// </summary>
    public class AssignmentItem
    {
        public int? AssignmentId { get; set; } // Database ID (null for new)
        public string AssignmentType { get; set; } = string.Empty; // "User", "Role", etc.
        public int TargetId { get; set; } // UserId, RoleId, DepartmentId, etc.
        public string TargetName { get; set; } = string.Empty; // Display name
        public string? TargetDetails { get; set; } // Additional info (email, department, etc.)
        public int? Level { get; set; } // For multi-level approvals
        public string? LevelLabel { get; set; } // "Level 1", "Step 1", etc.
        public bool IsMandatory { get; set; } = true;
        public DateTime? AssignedDate { get; set; }
        public string? AssignedBy { get; set; }
    }

    /// <summary>
    /// View model for assignment item (render-ready)
    /// </summary>
    public class AssignmentItemViewModel
    {
        public int? AssignmentId { get; set; }
        public string AssignmentType { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public string TargetName { get; set; } = string.Empty;
        public string? TargetDetails { get; set; }
        public int? Level { get; set; }
        public string? LevelLabel { get; set; }
        public bool IsMandatory { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string? AssignedBy { get; set; }

        // Computed properties
        public string TypeIcon => AssignmentType switch
        {
            "User" => "ri-user-line",
            "Role" => "ri-shield-user-line",
            "Department" => "ri-building-line",
            "UserGroup" => "ri-group-line",
            "Tenant" => "ri-community-line",
            _ => "ri-folder-user-line"
        };

        public string TypeBadgeColor => AssignmentType switch
        {
            "User" => "primary",
            "Role" => "success",
            "Department" => "info",
            "UserGroup" => "warning",
            "Tenant" => "secondary",
            _ => "light"
        };

        public string AssignedDateFormatted => AssignedDate?.ToString("MMM dd, yyyy") ?? "";
        public string MandatoryBadge => IsMandatory ? "<span class='badge bg-danger-subtle text-danger'>Required</span>" : "";
    }
}

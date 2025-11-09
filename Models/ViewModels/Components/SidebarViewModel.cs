namespace FormReporting.Models.ViewModels.Components
{
    /// <summary>
    /// ViewModel for sidebar navigation
    /// </summary>
    public class SidebarViewModel
    {
        public List<MenuSectionViewModel> Sections { get; set; } = new();
    }

    /// <summary>
    /// Represents a menu section (e.g., "Menu", "Management", "System")
    /// </summary>
    public class MenuSectionViewModel
    {
        public int MenuSectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string? SectionCode { get; set; }
        public int DisplayOrder { get; set; }
        public List<ModuleViewModel> Modules { get; set; } = new();
    }

    /// <summary>
    /// Represents a collapsible module (e.g., "Dashboards", "Forms & Checklists")
    /// </summary>
    public class ModuleViewModel
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public List<MenuItemViewModel> MenuItems { get; set; } = new();

        /// <summary>
        /// Bootstrap collapse target ID (e.g., "sidebarDashboards")
        /// </summary>
        public string CollapseId => $"sidebar{ModuleCode}";
    }

    /// <summary>
    /// Represents an individual menu item (e.g., "Analytics", "My Tickets")
    /// </summary>
    public class MenuItemViewModel
    {
        public int MenuItemId { get; set; }
        public string MenuTitle { get; set; } = string.Empty;
        public string MenuCode { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Route { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Area { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets the generated URL for this menu item
        /// </summary>
        public string Url { get; set; } = "#";
    }
}

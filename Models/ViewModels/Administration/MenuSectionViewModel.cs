using System.Collections.Generic;

namespace FormReporting.Models.ViewModels.Administration
{
    /// <summary>
    /// View model for menu sections with their associated modules and menu items
    /// </summary>
    public class MenuSectionViewModel
    {
        public int MenuSectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string SectionCode { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        // Modules in this section
        public List<ModuleViewModel> Modules { get; set; } = new List<ModuleViewModel>();

        // Total count of menu items in this section
        public int TotalMenuItems { get; set; }

        // Display helpers
        public string SectionIcon => SectionCode switch
        {
            "MAIN" => "ri-home-line",
            "ASSETS" => "ri-database-2-line",
            "ORG" => "ri-building-line",
            "METRICS" => "ri-line-chart-line",
            "FINANCE" => "ri-money-dollar-circle-line",
            "ADMIN" => "ri-settings-3-line",
            _ => "ri-folder-line"
        };

        public string SectionBadgeColor => SectionCode switch
        {
            "MAIN" => "primary",
            "ASSETS" => "success",
            "ORG" => "info",
            "METRICS" => "warning",
            "FINANCE" => "danger",
            "ADMIN" => "secondary",
            _ => "light"
        };
    }

    /// <summary>
    /// Simplified module view model for section display
    /// </summary>
    public class ModuleViewModel
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        // Menu items in this module
        public List<MenuItemViewModel> MenuItems { get; set; } = new List<MenuItemViewModel>();

        // Display helpers
        public string StatusBadge => IsActive 
            ? "<span class='badge bg-success-subtle text-success'>Active</span>" 
            : "<span class='badge bg-danger-subtle text-danger'>Inactive</span>";

        public string IconDisplay => !string.IsNullOrEmpty(Icon) 
            ? $"<i class='{Icon} fs-16 align-middle text-primary me-2'></i>" 
            : "";
    }
}

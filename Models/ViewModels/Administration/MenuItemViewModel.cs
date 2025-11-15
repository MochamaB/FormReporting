using System;
using System.Collections.Generic;

namespace FormReporting.Models.ViewModels.Administration
{
    /// <summary>
    /// View model for displaying and editing menu items
    /// </summary>
    public class MenuItemViewModel
    {
        public int MenuItemId { get; set; }
        public int? ParentMenuItemId { get; set; }
        public int ModuleId { get; set; }
        public string MenuTitle { get; set; } = string.Empty;
        public string MenuCode { get; set; } = string.Empty;
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public int Level { get; set; }
        public bool IsActive { get; set; }
        public bool IsVisible { get; set; }
        public bool RequiresAuth { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation properties for display
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public string? ParentMenuTitle { get; set; }
        public string SectionName { get; set; } = string.Empty;

        // Child items for tree structure
        public List<MenuItemViewModel> Children { get; set; } = new List<MenuItemViewModel>();

        // Display helpers
        public string StatusBadge => IsActive 
            ? "<span class='badge bg-success-subtle text-success'>Active</span>" 
            : "<span class='badge bg-danger-subtle text-danger'>Inactive</span>";

        public string VisibilityBadge => IsVisible 
            ? "<span class='badge bg-info-subtle text-info'>Visible</span>" 
            : "<span class='badge bg-secondary-subtle text-secondary'>Hidden</span>";

        public string LevelClass => Level switch
        {
            1 => "nested-1",
            2 => "nested-2",
            3 => "nested-3",
            _ => "nested-4"
        };

        public string IconDisplay => !string.IsNullOrEmpty(Icon) 
            ? $"<i class='{Icon} fs-16 align-middle text-primary me-2'></i>" 
            : "";

        public bool HasChildren => Children.Count > 0;

        public string FullPath => !string.IsNullOrEmpty(Controller) && !string.IsNullOrEmpty(Action)
            ? $"/{Controller}/{Action}"
            : "#";
    }
}

using System.Collections.Generic;

namespace FormReporting.Models.ViewModels.Administration
{
    /// <summary>
    /// View model for the complete menu tree structure
    /// Used for rendering the entire menu hierarchy in the management interface
    /// </summary>
    public class MenuTreeViewModel
    {
        public List<MenuSectionViewModel> Sections { get; set; } = new List<MenuSectionViewModel>();

        // Statistics for dashboard
        public int TotalSections { get; set; }
        public int TotalModules { get; set; }
        public int TotalMenuItems { get; set; }
        public int ActiveMenuItems { get; set; }
        public int InactiveMenuItems { get; set; }

        // Display helpers
        public string TreeSummary => $"{TotalSections} Sections, {TotalModules} Modules, {TotalMenuItems} Menu Items";
    }

    /// <summary>
    /// Request model for reordering menu items via drag & drop
    /// </summary>
    public class ReorderMenuItemRequest
    {
        public int MenuItemId { get; set; }
        public int NewDisplayOrder { get; set; }
        public int? NewParentMenuItemId { get; set; }
        public int? NewModuleId { get; set; }
    }

    /// <summary>
    /// Request model for bulk reordering
    /// </summary>
    public class BulkReorderRequest
    {
        public List<ReorderMenuItemRequest> Items { get; set; } = new List<ReorderMenuItemRequest>();
    }

    /// <summary>
    /// Request model for saving menu item changes (create or update)
    /// </summary>
    public class SaveMenuItemRequest
    {
        public int MenuItemId { get; set; }  // 0 for create, value for update
        public string MenuTitle { get; set; } = string.Empty;
        public string MenuCode { get; set; } = string.Empty;
        public int? ModuleId { get; set; }
        public int? ParentMenuItemId { get; set; }
        public int Level { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsVisible { get; set; }
        public bool RequiresAuth { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
    }

    /// <summary>
    /// Response model for AJAX operations
    /// </summary>
    public class MenuManagementResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// View model for icon picker
    /// </summary>
    public class IconViewModel
    {
        public string IconClass { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}

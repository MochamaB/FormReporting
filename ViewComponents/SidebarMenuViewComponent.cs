using FormReporting.Data;
using FormReporting.Extensions;
using FormReporting.Models.ViewModels.Components;
using Microsoft.AspNetCore.Mvc;

namespace FormReporting.ViewComponents
{
    /// <summary>
    /// ViewComponent for rendering the dynamic sidebar menu
    /// </summary>
    public class SidebarMenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public SidebarMenuViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Invoke the sidebar menu component
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get current route data
            var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
            var currentAction = ViewContext.RouteData.Values["action"]?.ToString();
            var currentArea = ViewContext.RouteData.Values["area"]?.ToString();

            // Load menu structure from database
            var sidebarViewModel = await _context.LoadSidebarMenuAsync(Url);

            // Mark active menu items and expanded modules
            MarkActiveMenuItems(sidebarViewModel, currentController, currentAction, currentArea);

            // Return the default view with the loaded data
            return View(sidebarViewModel);
        }

        /// <summary>
        /// Mark active menu items and expand their parent modules
        /// </summary>
        private void MarkActiveMenuItems(SidebarViewModel sidebar, string? currentController, string? currentAction, string? currentArea)
        {
            foreach (var section in sidebar.Sections)
            {
                foreach (var module in section.Modules)
                {
                    foreach (var menuItem in module.MenuItems)
                    {
                        // Check if this menu item matches the current route
                        bool isActive = IsMenuItemActive(menuItem, currentController, currentAction, currentArea);
                        menuItem.IsActive = isActive;

                        // If any menu item is active, mark the module as expanded
                        if (isActive)
                        {
                            module.IsExpanded = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if a menu item matches the current route
        /// </summary>
        private bool IsMenuItemActive(MenuItemViewModel menuItem, string? currentController, string? currentAction, string? currentArea)
        {
            // Normalize values for comparison (case-insensitive)
            var itemController = menuItem.Controller?.ToLower();
            var itemArea = menuItem.Area?.ToLower();

            currentController = currentController?.ToLower();
            currentArea = currentArea?.ToLower();

            // Match on controller only (not action) so all actions in the controller are highlighted
            // For example: Regions/Index, Regions/Create, Regions/Edit all highlight "Regions" menu item
            bool controllerMatch = !string.IsNullOrEmpty(itemController) && itemController == currentController;
            
            // Area must match (or both be empty)
            bool areaMatch = string.IsNullOrEmpty(itemArea) && string.IsNullOrEmpty(currentArea) ||
                            itemArea == currentArea;

            return controllerMatch && areaMatch;
        }
    }
}

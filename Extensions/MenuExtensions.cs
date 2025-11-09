using FormReporting.Data;
using FormReporting.Models.ViewModels.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace FormReporting.Extensions
{
    /// <summary>
    /// Extension methods for loading and transforming menu data
    /// </summary>
    public static class MenuExtensions
    {
        /// <summary>
        /// Load sidebar menu structure from database
        /// </summary>
        public static async Task<SidebarViewModel> LoadSidebarMenuAsync(
            this ApplicationDbContext context,
            IUrlHelper? urlHelper = null)
        {
            var sidebar = new SidebarViewModel();

            // Load menu sections with their modules and menu items
            var sections = await context.MenuSections
                .Where(s => s.IsActive)
                .Include(s => s.Modules.Where(m => m.IsActive))
                    .ThenInclude(m => m.MenuItems.Where(mi => mi.IsActive && mi.IsVisible))
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            // Transform to ViewModels
            sidebar.Sections = sections.Select(section => new MenuSectionViewModel
            {
                MenuSectionId = section.MenuSectionId,
                SectionName = section.SectionName,
                SectionCode = section.SectionCode,
                DisplayOrder = section.DisplayOrder,
                Modules = section.Modules
                    .OrderBy(m => m.DisplayOrder)
                    .Select(module => new ModuleViewModel
                    {
                        ModuleId = module.ModuleId,
                        ModuleName = module.ModuleName,
                        ModuleCode = module.ModuleCode,
                        Icon = module.Icon,
                        DisplayOrder = module.DisplayOrder,
                        MenuItems = module.MenuItems
                            .OrderBy(mi => mi.DisplayOrder)
                            .Select(menuItem => new MenuItemViewModel
                            {
                                MenuItemId = menuItem.MenuItemId,
                                MenuTitle = menuItem.MenuTitle,
                                MenuCode = menuItem.MenuCode,
                                Icon = menuItem.Icon,
                                Route = menuItem.Route,
                                Controller = menuItem.Controller,
                                Action = menuItem.Action,
                                Area = menuItem.Area,
                                DisplayOrder = menuItem.DisplayOrder,
                                IsActive = menuItem.IsActive,
                                Url = GenerateUrl(menuItem, urlHelper)
                            }).ToList()
                    }).ToList()
            }).ToList();

            return sidebar;
        }

        /// <summary>
        /// Load sidebar menu for a specific user role (future: role-based filtering)
        /// </summary>
        public static async Task<SidebarViewModel> LoadSidebarMenuForRoleAsync(
            this ApplicationDbContext context,
            string roleId,
            IUrlHelper? urlHelper = null)
        {
            // For now, just load all menus
            // TODO: Filter by RoleMenuItem when implementing role-based access
            return await context.LoadSidebarMenuAsync(urlHelper);
        }

        /// <summary>
        /// Generate URL for menu item
        /// </summary>
        private static string GenerateUrl(
            Models.Entities.Identity.MenuItem menuItem,
            IUrlHelper? urlHelper)
        {
            // If explicit route is provided, use it
            if (!string.IsNullOrEmpty(menuItem.Route))
            {
                return menuItem.Route;
            }

            // If controller and action are provided, generate URL
            if (!string.IsNullOrEmpty(menuItem.Controller) && !string.IsNullOrEmpty(menuItem.Action))
            {
                if (urlHelper != null)
                {
                    return urlHelper.Action(
                        menuItem.Action,
                        menuItem.Controller,
                        string.IsNullOrEmpty(menuItem.Area) ? null : new { area = menuItem.Area }
                    ) ?? "#";
                }

                // Fallback: construct URL manually
                var areaPrefix = string.IsNullOrEmpty(menuItem.Area) ? "" : $"/{menuItem.Area}";
                return $"{areaPrefix}/{menuItem.Controller}/{menuItem.Action}";
            }

            // Default to # for placeholder links
            return "#";
        }
    }
}

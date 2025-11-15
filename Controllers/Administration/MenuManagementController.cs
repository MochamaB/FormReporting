using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.ViewModels.Administration;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Controllers.Administration
{
    [Route("Administration/[controller]")]
    public class MenuManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Main menu management page with tree view and properties panel
        /// </summary>
        [HttpGet("Index")]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // Load complete menu tree structure
            var menuTree = await BuildMenuTreeAsync();

            return View("~/Views/Administration/MenuManagement/Index.cshtml", menuTree);
        }

        /// <summary>
        /// API endpoint to get menu tree structure as JSON
        /// Used for AJAX refresh and initial load
        /// </summary>
        [HttpGet("GetMenuTree")]
        public async Task<JsonResult> GetMenuTree()
        {
            try
            {
                var menuTree = await BuildMenuTreeAsync();
                return Json(new MenuManagementResponse
                {
                    Success = true,
                    Message = "Menu tree loaded successfully",
                    Data = menuTree
                });
            }
            catch (Exception ex)
            {
                return Json(new MenuManagementResponse
                {
                    Success = false,
                    Message = "Failed to load menu tree",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// API endpoint to get a single menu item by ID
        /// Used when clicking an item in the tree to load properties
        /// </summary>
        [HttpGet("GetMenuItem/{id}")]
        public async Task<JsonResult> GetMenuItem(int id)
        {
            try
            {
                var menuItem = await _context.MenuItems
                    .Include(m => m.Module)
                        .ThenInclude(mod => mod.MenuSection)
                    .Include(m => m.ParentMenuItem)
                    .Where(m => m.MenuItemId == id)
                    .Select(m => new MenuItemViewModel
                    {
                        MenuItemId = m.MenuItemId,
                        ParentMenuItemId = m.ParentMenuItemId,
                        ModuleId = m.ModuleId ?? 0,
                        MenuTitle = m.MenuTitle ?? string.Empty,
                        MenuCode = m.MenuCode ?? string.Empty,
                        Controller = m.Controller,
                        Action = m.Action,
                        Icon = m.Icon,
                        DisplayOrder = m.DisplayOrder,
                        Level = m.Level,
                        IsActive = m.IsActive,
                        IsVisible = m.IsVisible,
                        RequiresAuth = m.RequiresAuth,
                        CreatedDate = m.CreatedDate,
                        ModuleName = m.Module != null ? m.Module.ModuleName ?? string.Empty : string.Empty,
                        ModuleCode = m.Module != null ? m.Module.ModuleCode ?? string.Empty : string.Empty,
                        ParentMenuTitle = m.ParentMenuItem != null ? m.ParentMenuItem.MenuTitle : null,
                        SectionName = m.Module != null && m.Module.MenuSection != null ? m.Module.MenuSection.SectionName ?? string.Empty : string.Empty
                    })
                    .FirstOrDefaultAsync();

                if (menuItem == null)
                {
                    return Json(new MenuManagementResponse
                    {
                        Success = false,
                        Message = "Menu item not found"
                    });
                }

                return Json(new MenuManagementResponse
                {
                    Success = true,
                    Message = "Menu item loaded successfully",
                    Data = menuItem
                });
            }
            catch (Exception ex)
            {
                return Json(new MenuManagementResponse
                {
                    Success = false,
                    Message = "Failed to load menu item",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// API endpoint to save menu item changes (create or update)
        /// </summary>
        [HttpPost("SaveMenuItem")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveMenuItem([FromBody] SaveMenuItemRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new MenuManagementResponse
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.MenuTitle))
                {
                    return Json(new MenuManagementResponse
                    {
                        Success = false,
                        Message = "Menu title is required",
                        Errors = new List<string> { "MenuTitle is required" }
                    });
                }

                if (string.IsNullOrWhiteSpace(request.MenuCode))
                {
                    return Json(new MenuManagementResponse
                    {
                        Success = false,
                        Message = "Menu code is required",
                        Errors = new List<string> { "MenuCode is required" }
                    });
                }

                if (!request.ModuleId.HasValue)
                {
                    return Json(new MenuManagementResponse
                    {
                        Success = false,
                        Message = "Module is required",
                        Errors = new List<string> { "ModuleId is required" }
                    });
                }

                // Check if this is create (MenuItemId = 0) or update
                if (request.MenuItemId == 0)
                {
                    // CREATE NEW MENU ITEM
                    var newMenuItem = new MenuItem
                    {
                        MenuTitle = request.MenuTitle,
                        MenuCode = request.MenuCode,
                        ModuleId = request.ModuleId,
                        ParentMenuItemId = request.ParentMenuItemId,
                        Level = request.Level,
                        Icon = request.Icon,
                        DisplayOrder = request.DisplayOrder,
                        IsActive = request.IsActive,
                        IsVisible = request.IsVisible,
                        RequiresAuth = request.RequiresAuth,
                        Controller = request.Controller,
                        Action = request.Action,
                        CreatedDate = DateTime.Now
                    };

                    _context.MenuItems.Add(newMenuItem);
                    await _context.SaveChangesAsync();

                    return Json(new MenuManagementResponse
                    {
                        Success = true,
                        Message = "Menu item created successfully",
                        Data = new { menuItemId = newMenuItem.MenuItemId }
                    });
                }
                else
                {
                    // UPDATE EXISTING MENU ITEM
                    var menuItem = await _context.MenuItems.FindAsync(request.MenuItemId);
                    if (menuItem == null)
                    {
                        return Json(new MenuManagementResponse
                        {
                            Success = false,
                            Message = "Menu item not found"
                        });
                    }

                    // Update properties
                    menuItem.MenuTitle = request.MenuTitle;
                    menuItem.MenuCode = request.MenuCode;
                    menuItem.ModuleId = request.ModuleId;
                    menuItem.ParentMenuItemId = request.ParentMenuItemId;
                    menuItem.Level = request.Level;
                    menuItem.Icon = request.Icon;
                    menuItem.DisplayOrder = request.DisplayOrder;
                    menuItem.IsActive = request.IsActive;
                    menuItem.IsVisible = request.IsVisible;
                    menuItem.RequiresAuth = request.RequiresAuth;
                    menuItem.Controller = request.Controller;
                    menuItem.Action = request.Action;

                    await _context.SaveChangesAsync();

                    return Json(new MenuManagementResponse
                    {
                        Success = true,
                        Message = "Menu item updated successfully"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new MenuManagementResponse
                {
                    Success = false,
                    Message = "Failed to save menu item",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// API endpoint to delete a menu item
        /// </summary>
        [HttpPost("DeleteMenuItem/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteMenuItem(int id)
        {
            try
            {
                var menuItem = await _context.MenuItems
                    .Include(m => m.ChildMenuItems)
                    .FirstOrDefaultAsync(m => m.MenuItemId == id);

                if (menuItem == null)
                {
                    return Json(new MenuManagementResponse
                    {
                        Success = false,
                        Message = "Menu item not found"
                    });
                }

                // Check if item has children
                if (menuItem.ChildMenuItems != null && menuItem.ChildMenuItems.Any())
                {
                    return Json(new MenuManagementResponse
                    {
                        Success = false,
                        Message = $"Cannot delete '{menuItem.MenuTitle}' because it has {menuItem.ChildMenuItems.Count} child item(s). Please delete or move the child items first.",
                        Errors = new List<string> { "Item has children" }
                    });
                }

                // Delete the menu item
                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();

                return Json(new MenuManagementResponse
                {
                    Success = true,
                    Message = $"Menu item '{menuItem.MenuTitle}' deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new MenuManagementResponse
                {
                    Success = false,
                    Message = "Failed to delete menu item",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// API endpoint to reorder menu items after drag & drop
        /// </summary>
        [HttpPost("ReorderItems")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ReorderItems([FromBody] BulkReorderRequest request)
        {
            try
            {
                if (request == null || request.Items == null || !request.Items.Any())
                {
                    return Json(new MenuManagementResponse
                    {
                        Success = false,
                        Message = "No items to reorder"
                    });
                }

                // Update each menu item's display order
                foreach (var item in request.Items)
                {
                    var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                    if (menuItem != null)
                    {
                        menuItem.DisplayOrder = item.NewDisplayOrder;
                        
                        // Update parent if changed
                        if (item.NewParentMenuItemId.HasValue)
                        {
                            menuItem.ParentMenuItemId = item.NewParentMenuItemId;
                        }
                        
                        // Update module if changed
                        if (item.NewModuleId.HasValue)
                        {
                            menuItem.ModuleId = item.NewModuleId;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new MenuManagementResponse
                {
                    Success = true,
                    Message = $"Successfully reordered {request.Items.Count} menu items"
                });
            }
            catch (Exception ex)
            {
                return Json(new MenuManagementResponse
                {
                    Success = false,
                    Message = "Failed to reorder menu items",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Helper method to build the complete menu tree structure
        /// </summary>
        private async Task<MenuTreeViewModel> BuildMenuTreeAsync()
        {
            // Load all menu sections with their modules and menu items
            var sections = await _context.MenuSections
                .Include(s => s.Modules.Where(m => m.IsActive))
                    .ThenInclude(m => m.MenuItems)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            var menuTree = new MenuTreeViewModel();

            foreach (var section in sections)
            {
                var sectionViewModel = new MenuSectionViewModel
                {
                    MenuSectionId = section.MenuSectionId,
                    SectionName = section.SectionName,
                    SectionCode = section.SectionCode,
                    DisplayOrder = section.DisplayOrder
                };

                // Build modules for this section
                foreach (var module in section.Modules.OrderBy(m => m.DisplayOrder))
                {
                    var moduleViewModel = new ModuleViewModel
                    {
                        ModuleId = module.ModuleId,
                        ModuleName = module.ModuleName,
                        ModuleCode = module.ModuleCode,
                        Icon = module.Icon,
                        DisplayOrder = module.DisplayOrder,
                        IsActive = module.IsActive
                    };

                    // Build menu items for this module (only top-level items, Level 1)
                    var topLevelItems = module.MenuItems
                        .Where(mi => mi.ParentMenuItemId == null)
                        .OrderBy(mi => mi.DisplayOrder)
                        .ToList();

                    foreach (var menuItem in topLevelItems)
                    {
                        var menuItemViewModel = BuildMenuItemViewModel(menuItem, module.MenuItems.ToList());
                        moduleViewModel.MenuItems.Add(menuItemViewModel);
                    }

                    sectionViewModel.Modules.Add(moduleViewModel);
                    sectionViewModel.TotalMenuItems += moduleViewModel.MenuItems.Count;
                }

                menuTree.Sections.Add(sectionViewModel);
            }

            // Calculate statistics
            menuTree.TotalSections = menuTree.Sections.Count;
            menuTree.TotalModules = menuTree.Sections.Sum(s => s.Modules.Count);
            menuTree.TotalMenuItems = await _context.MenuItems.CountAsync();
            menuTree.ActiveMenuItems = await _context.MenuItems.CountAsync(m => m.IsActive);
            menuTree.InactiveMenuItems = menuTree.TotalMenuItems - menuTree.ActiveMenuItems;

            return menuTree;
        }

        /// <summary>
        /// Recursively build menu item view model with children
        /// </summary>
        private MenuItemViewModel BuildMenuItemViewModel(
            Models.Entities.Identity.MenuItem menuItem, 
            List<Models.Entities.Identity.MenuItem> allItems)
        {
            var viewModel = new MenuItemViewModel
            {
                MenuItemId = menuItem.MenuItemId,
                ParentMenuItemId = menuItem.ParentMenuItemId,
                ModuleId = menuItem.ModuleId ?? 0,
                MenuTitle = menuItem.MenuTitle ?? string.Empty,
                MenuCode = menuItem.MenuCode ?? string.Empty,
                Controller = menuItem.Controller,
                Action = menuItem.Action,
                Icon = menuItem.Icon,
                DisplayOrder = menuItem.DisplayOrder,
                Level = menuItem.Level,
                IsActive = menuItem.IsActive,
                IsVisible = menuItem.IsVisible,
                RequiresAuth = menuItem.RequiresAuth,
                CreatedDate = menuItem.CreatedDate
            };

            // Find and add children recursively
            var children = allItems
                .Where(mi => mi.ParentMenuItemId == menuItem.MenuItemId)
                .OrderBy(mi => mi.DisplayOrder)
                .ToList();

            foreach (var child in children)
            {
                var childViewModel = BuildMenuItemViewModel(child, allItems);
                viewModel.Children.Add(childViewModel);
            }

            return viewModel;
        }
    }
}

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
            // Load menu structure from database
            var sidebarViewModel = await _context.LoadSidebarMenuAsync(Url);

            // Return the default view with the loaded data
            return View(sidebarViewModel);
        }
    }
}

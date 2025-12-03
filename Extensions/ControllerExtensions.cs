using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FormReporting.Extensions
{
    /// <summary>
    /// Extension methods for ASP.NET Core controllers
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Render a Razor view to HTML string
        /// Useful for AJAX endpoints that need to return rendered HTML
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="controller">Controller instance</param>
        /// <param name="viewName">View name or path (e.g., "_Partial" or "~/Views/Folder/_Partial.cshtml")</param>
        /// <param name="model">Model to pass to the view</param>
        /// <returns>Rendered HTML as string</returns>
        public static async Task<string> RenderViewAsync<TModel>(
            this Controller controller,
            string viewName,
            TModel model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                var viewEngine = controller.HttpContext.RequestServices
                    .GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

                if (viewEngine == null)
                {
                    throw new InvalidOperationException("View engine not found");
                }

                var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);

                if (!viewResult.Success)
                {
                    // Try as a path
                    viewResult = viewEngine.GetView(null, viewName, false);
                }

                if (!viewResult.Success)
                {
                    var searchedLocations = string.Join(", ", viewResult.SearchedLocations ?? new string[0]);
                    throw new InvalidOperationException(
                        $"View '{viewName}' not found. Searched locations: {searchedLocations}");
                }

                var viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.ToString();
            }
        }
    }
}

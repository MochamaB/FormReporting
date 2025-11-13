namespace FormReporting.Models.ViewModels.Components
{
    // ============================================================================
    // CONFIGURATION CLASSES (What users create in views - optional)
    // ============================================================================

    /// <summary>
    /// Configuration object for breadcrumb navigation
    /// If not provided, breadcrumbs will be auto-generated from route data
    /// </summary>
    public class BreadcrumbConfig
    {
        /// <summary>
        /// Page title (displayed on the left)
        /// If null, will be extracted from ViewData["Title"]
        /// </summary>
        public string? PageTitle { get; set; }

        /// <summary>
        /// Custom breadcrumb items
        /// If empty, will be auto-generated from route data
        /// </summary>
        public List<BreadcrumbItem> Items { get; set; } = new();

        /// <summary>
        /// Show Home link at the beginning?
        /// </summary>
        public bool ShowHomeLink { get; set; } = true;

        /// <summary>
        /// Custom home link text
        /// </summary>
        public string HomeLinkText { get; set; } = "Home";

        /// <summary>
        /// Custom home link URL
        /// </summary>
        public string? HomeLinkUrl { get; set; }
    }

    /// <summary>
    /// Single breadcrumb item
    /// </summary>
    public class BreadcrumbItem
    {
        /// <summary>
        /// Display text
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Link URL (null for active/current item)
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }

    // ============================================================================
    // VIEW MODEL CLASSES (What partials receive for rendering)
    // ============================================================================

    /// <summary>
    /// Breadcrumb view model - ready for rendering
    /// Created by BreadcrumbExtensions.BuildBreadcrumb()
    /// Contains all render-ready data, no logic needed in views
    /// </summary>
    public class BreadcrumbViewModel
    {
        /// <summary>
        /// Page title (left side)
        /// </summary>
        public string PageTitle { get; set; } = string.Empty;

        /// <summary>
        /// Ordered breadcrumb items (render-ready)
        /// </summary>
        public List<BreadcrumbItemViewModel> Items { get; set; } = new();

        /// <summary>
        /// Has any breadcrumb items?
        /// </summary>
        public bool HasItems => Items.Any();
    }

    /// <summary>
    /// Breadcrumb item view model (render-ready)
    /// </summary>
    public class BreadcrumbItemViewModel
    {
        /// <summary>
        /// Display text
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Link URL (null for active item)
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Is this the active/current item?
        /// </summary>
        public bool IsActive => string.IsNullOrEmpty(Url);

        /// <summary>
        /// Pre-computed CSS classes
        /// </summary>
        public string CssClasses { get; set; } = string.Empty;

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}

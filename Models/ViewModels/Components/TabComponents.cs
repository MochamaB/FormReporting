namespace FormReporting.Models.ViewModels.Components
{
    // ============================================================================
    // ENUMS
    // ============================================================================

    /// <summary>
    /// Tabs layout types
    /// </summary>
    public enum TabsLayout
    {
        /// <summary>
        /// Horizontal tabs (top navigation)
        /// </summary>
        Horizontal = 1,

        /// <summary>
        /// Vertical tabs (side navigation)
        /// </summary>
        Vertical = 2
    }

    /// <summary>
    /// Tabs style variations
    /// </summary>
    public enum TabsStyle
    {
        /// <summary>
        /// Standard Bootstrap tabs
        /// </summary>
        Standard = 1,

        /// <summary>
        /// Custom bordered tabs (nav-tabs-custom)
        /// </summary>
        CustomBordered = 2,

        /// <summary>
        /// Pills style tabs
        /// </summary>
        Pills = 3
    }

    /// <summary>
    /// Navigation mode for tabs
    /// </summary>
    public enum TabsNavigationMode
    {
        /// <summary>
        /// Client-side tab switching using Bootstrap's data-bs-toggle (default)
        /// </summary>
        ClientSide = 1,

        /// <summary>
        /// Server-side navigation using href links (page reload on tab click)
        /// </summary>
        ServerSide = 2
    }

    // ============================================================================
    // CONFIGURATION CLASSES (What users create in views)
    // ============================================================================

    /// <summary>
    /// Configuration object for creating tabs
    /// User creates this in views to define WHAT to display
    /// Extensions handle HOW to transform this into TabsViewModel
    /// </summary>
    public class TabsConfig
    {
        /// <summary>
        /// Unique ID for the tabs component
        /// </summary>
        public string TabsId { get; set; } = $"tabs_{Guid.NewGuid():N}";

        /// <summary>
        /// Layout type: Horizontal or Vertical
        /// </summary>
        public TabsLayout Layout { get; set; } = TabsLayout.Horizontal;

        /// <summary>
        /// Style: Standard, CustomBordered, or Pills
        /// </summary>
        public TabsStyle Style { get; set; } = TabsStyle.CustomBordered;

        /// <summary>
        /// Navigation mode: ClientSide (default) or ServerSide
        /// ServerSide uses href links for page navigation instead of tab switching
        /// </summary>
        public TabsNavigationMode NavigationMode { get; set; } = TabsNavigationMode.ClientSide;

        /// <summary>
        /// Color theme (e.g., "success", "primary", "info", "danger")
        /// Used for nav-tabs-custom nav-{ColorTheme}
        /// </summary>
        public string ColorTheme { get; set; } = "success";

        /// <summary>
        /// List of tabs
        /// </summary>
        public List<TabConfig> Tabs { get; set; } = new();

        /// <summary>
        /// Use justified layout (tabs take full width)
        /// </summary>
        public bool IsJustified { get; set; } = false;

        /// <summary>
        /// Wrap tabs in a card
        /// </summary>
        public bool WrapInCard { get; set; } = true;

        /// <summary>
        /// Card title (if wrapped in card)
        /// </summary>
        public string? CardTitle { get; set; }

        /// <summary>
        /// Card subtitle (if wrapped in card)
        /// </summary>
        public string? CardSubtitle { get; set; }

        /// <summary>
        /// Additional CSS classes for container
        /// </summary>
        public string? ContainerCssClass { get; set; }
    }

    /// <summary>
    /// Individual tab configuration (Config layer)
    /// </summary>
    public class TabConfig
    {
        /// <summary>
        /// Unique tab identifier
        /// </summary>
        public string TabId { get; set; } = string.Empty;

        /// <summary>
        /// Tab title/label
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Optional icon class (e.g., "ri-home-line")
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Optional description/subtitle
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Optional badge text (e.g., "5", "New", "Premium")
        /// </summary>
        public string? Badge { get; set; }

        /// <summary>
        /// Badge color (e.g., "primary", "danger", "success")
        /// </summary>
        public string BadgeColor { get; set; } = "primary";

        /// <summary>
        /// Is this tab active by default?
        /// </summary>
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// Is this tab disabled?
        /// </summary>
        public bool IsDisabled { get; set; } = false;

        /// <summary>
        /// URL for server-side navigation (used when NavigationMode = ServerSide)
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Path to partial view containing tab content
        /// </summary>
        public string? ContentPartialPath { get; set; }

        /// <summary>
        /// Raw HTML content (if not using partial)
        /// </summary>
        public string? ContentHtml { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }

    // ============================================================================
    // VIEW MODEL CLASSES (What partials receive for rendering)
    // ============================================================================

    /// <summary>
    /// Tabs view model - ready for rendering
    /// Created by TabsExtensions.BuildTabs()
    /// Contains all render-ready data, no logic needed in views
    /// </summary>
    public class TabsViewModel
    {
        /// <summary>
        /// Unique tabs ID
        /// </summary>
        public string TabsId { get; set; } = string.Empty;

        /// <summary>
        /// Layout type
        /// </summary>
        public TabsLayout Layout { get; set; }

        /// <summary>
        /// Style type
        /// </summary>
        public TabsStyle Style { get; set; }

        /// <summary>
        /// Color theme
        /// </summary>
        public string ColorTheme { get; set; } = string.Empty;

        /// <summary>
        /// Navigation mode
        /// </summary>
        public TabsNavigationMode NavigationMode { get; set; } = TabsNavigationMode.ClientSide;

        /// <summary>
        /// List of transformed tabs (ViewModel version)
        /// </summary>
        public List<TabViewModel> Tabs { get; set; } = new();

        /// <summary>
        /// Is justified layout?
        /// </summary>
        public bool IsJustified { get; set; }

        /// <summary>
        /// Wrap in card?
        /// </summary>
        public bool WrapInCard { get; set; }

        /// <summary>
        /// Card title
        /// </summary>
        public string? CardTitle { get; set; }

        /// <summary>
        /// Card subtitle
        /// </summary>
        public string? CardSubtitle { get; set; }

        /// <summary>
        /// Container CSS classes
        /// </summary>
        public string? ContainerCssClass { get; set; }

        // Pre-computed CSS classes
        /// <summary>
        /// Nav CSS classes (pre-computed)
        /// </summary>
        public string NavClasses { get; set; } = string.Empty;

        /// <summary>
        /// Tab content CSS classes (pre-computed)
        /// </summary>
        public string TabContentClasses { get; set; } = string.Empty;

        // Computed properties
        /// <summary>
        /// Total number of tabs
        /// </summary>
        public int TotalTabs => Tabs.Count;

        /// <summary>
        /// Active tab index (0-based)
        /// </summary>
        public int ActiveTabIndex => Tabs.FindIndex(t => t.IsActive);
    }

    /// <summary>
    /// Individual tab view model (ViewModel layer)
    /// All properties pre-computed for rendering - NO logic in partial views
    /// </summary>
    public class TabViewModel
    {
        /// <summary>
        /// Tab unique identifier
        /// </summary>
        public string TabId { get; set; } = string.Empty;

        /// <summary>
        /// Tab title/label
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Icon class
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Description/subtitle
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Badge text
        /// </summary>
        public string? Badge { get; set; }

        /// <summary>
        /// Badge CSS classes (pre-computed)
        /// </summary>
        public string BadgeClasses { get; set; } = string.Empty;

        /// <summary>
        /// Is this tab active?
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Is this tab disabled?
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// URL for server-side navigation
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Content partial path
        /// </summary>
        public string? ContentPartialPath { get; set; }

        /// <summary>
        /// Raw HTML content
        /// </summary>
        public string? ContentHtml { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }

        // Pre-computed properties for rendering
        /// <summary>
        /// Nav link CSS classes (pre-computed)
        /// </summary>
        public string NavLinkClasses { get; set; } = string.Empty;

        /// <summary>
        /// Tab pane CSS classes (pre-computed)
        /// </summary>
        public string TabPaneClasses { get; set; } = string.Empty;

        /// <summary>
        /// ARIA selected attribute value ("true" or "false")
        /// </summary>
        public string AriaSelected => IsActive ? "true" : "false";

        /// <summary>
        /// Target ID for data-bs-toggle
        /// </summary>
        public string TargetId => $"#{TabId}";

        /// <summary>
        /// Href for navigation
        /// </summary>
        public string Href => TargetId;
    }
}

using FormReporting.Models.ViewModels.Components;

namespace FormReporting.Extensions
{
    /// <summary>
    /// Extension methods for building tabs from configuration objects
    /// Handles all transformation logic - Views only provide data, Extensions build the structure
    /// Follows three-layer pattern: Config → Extension → ViewModel
    /// </summary>
    public static class TabsExtensions
    {
        /// <summary>
        /// Transforms TabsConfig into TabsViewModel
        /// Validates, auto-numbers, applies defaults, transforms to render-ready ViewModels
        /// ALL logic happens here - partials receive fully prepared data with ZERO logic needed
        /// </summary>
        public static TabsViewModel BuildTabs(this TabsConfig config)
        {
            // 1. Validate
            if (config.Tabs == null || !config.Tabs.Any())
                throw new ArgumentException("Tabs component must have at least one tab");

            // 2. Sort tabs by DisplayOrder
            var orderedTabs = config.Tabs.OrderBy(t => t.DisplayOrder).ToList();

            // 3. Ensure at least one tab is active
            if (!orderedTabs.Any(t => t.IsActive))
                orderedTabs[0].IsActive = true;

            // 4. Generate tab IDs if not provided
            for (int i = 0; i < orderedTabs.Count; i++)
            {
                if (string.IsNullOrEmpty(orderedTabs[i].TabId))
                    orderedTabs[i].TabId = $"{config.TabsId}-tab-{i + 1}";
            }

            // 5. Build nav CSS classes
            var navClasses = BuildNavClasses(config);

            // 6. Transform Config Tabs → ViewModel Tabs
            var viewModelTabs = orderedTabs.Select(tab => new TabViewModel
            {
                TabId = tab.TabId,
                Title = tab.Title,
                Icon = tab.Icon,
                Description = tab.Description,
                Badge = tab.Badge,
                BadgeClasses = BuildBadgeClasses(tab.BadgeColor),
                IsActive = tab.IsActive,
                IsDisabled = tab.IsDisabled,
                Url = tab.Url,
                ContentPartialPath = tab.ContentPartialPath,
                ContentHtml = tab.ContentHtml,
                DisplayOrder = tab.DisplayOrder,
                NavLinkClasses = BuildNavLinkClasses(tab.IsActive, tab.IsDisabled),
                TabPaneClasses = BuildTabPaneClasses(tab.IsActive)
            }).ToList();

            // 7. Return fully prepared ViewModel
            return new TabsViewModel
            {
                TabsId = config.TabsId,
                Layout = config.Layout,
                Style = config.Style,
                ColorTheme = config.ColorTheme,
                NavigationMode = config.NavigationMode,
                Tabs = viewModelTabs,
                IsJustified = config.IsJustified,
                WrapInCard = config.WrapInCard,
                CardTitle = config.CardTitle,
                CardSubtitle = config.CardSubtitle,
                ContainerCssClass = config.ContainerCssClass,
                NavClasses = navClasses,
                TabContentClasses = "tab-content text-muted"
            };
        }

        // ========== Fluent API Methods ==========

        /// <summary>
        /// Add a tab to the configuration
        /// </summary>
        public static TabsConfig WithTab(this TabsConfig config,
            string tabId,
            string title,
            string? icon = null,
            string? description = null,
            string? badge = null,
            string badgeColor = "primary",
            bool isActive = false,
            bool isDisabled = false,
            string? contentPartialPath = null,
            string? contentHtml = null)
        {
            config.Tabs.Add(new TabConfig
            {
                TabId = tabId,
                Title = title,
                Icon = icon,
                Description = description,
                Badge = badge,
                BadgeColor = badgeColor,
                IsActive = isActive,
                IsDisabled = isDisabled,
                ContentPartialPath = contentPartialPath,
                ContentHtml = contentHtml,
                DisplayOrder = config.Tabs.Count + 1
            });
            return config;
        }

        /// <summary>
        /// Set horizontal layout
        /// </summary>
        public static TabsConfig WithHorizontalLayout(this TabsConfig config)
        {
            config.Layout = TabsLayout.Horizontal;
            return config;
        }

        /// <summary>
        /// Set vertical layout
        /// </summary>
        public static TabsConfig WithVerticalLayout(this TabsConfig config)
        {
            config.Layout = TabsLayout.Vertical;
            return config;
        }

        /// <summary>
        /// Set custom bordered style
        /// </summary>
        public static TabsConfig WithCustomBorderedStyle(this TabsConfig config)
        {
            config.Style = TabsStyle.CustomBordered;
            return config;
        }

        /// <summary>
        /// Set pills style
        /// </summary>
        public static TabsConfig WithPillsStyle(this TabsConfig config)
        {
            config.Style = TabsStyle.Pills;
            return config;
        }

        /// <summary>
        /// Set standard style
        /// </summary>
        public static TabsConfig WithStandardStyle(this TabsConfig config)
        {
            config.Style = TabsStyle.Standard;
            return config;
        }

        /// <summary>
        /// Set color theme
        /// </summary>
        public static TabsConfig WithColorTheme(this TabsConfig config, string colorTheme)
        {
            config.ColorTheme = colorTheme;
            return config;
        }

        /// <summary>
        /// Enable justified layout
        /// </summary>
        public static TabsConfig WithJustifiedLayout(this TabsConfig config, bool justified = true)
        {
            config.IsJustified = justified;
            return config;
        }

        /// <summary>
        /// Wrap tabs in card
        /// </summary>
        public static TabsConfig WithCard(this TabsConfig config,
            string? title = null,
            string? subtitle = null)
        {
            config.WrapInCard = true;
            config.CardTitle = title;
            config.CardSubtitle = subtitle;
            return config;
        }

        /// <summary>
        /// Set server-side navigation mode (uses href links instead of tab switching)
        /// </summary>
        public static TabsConfig WithServerSideNavigation(this TabsConfig config)
        {
            config.NavigationMode = TabsNavigationMode.ServerSide;
            return config;
        }

        /// <summary>
        /// Add a tab with URL for server-side navigation
        /// </summary>
        public static TabsConfig WithNavigationTab(this TabsConfig config,
            string tabId,
            string title,
            string url,
            string? icon = null,
            string? badge = null,
            string badgeColor = "secondary",
            bool isActive = false)
        {
            config.Tabs.Add(new TabConfig
            {
                TabId = tabId,
                Title = title,
                Url = url,
                Icon = icon,
                Badge = badge,
                BadgeColor = badgeColor,
                IsActive = isActive,
                DisplayOrder = config.Tabs.Count + 1
            });
            return config;
        }

        // ========== Helper Methods (For Extension Use Only - NOT for Views) ==========

        /// <summary>
        /// Build nav CSS classes based on layout and style
        /// </summary>
        private static string BuildNavClasses(TabsConfig config)
        {
            var classes = new List<string>();

            // Base nav class
            classes.Add("nav");

            // Layout-specific classes
            if (config.Layout == TabsLayout.Vertical)
            {
                classes.Add("flex-column");
            }

            // Style-specific classes
            switch (config.Style)
            {
                case TabsStyle.CustomBordered:
                    classes.Add("nav-tabs");
                    classes.Add("nav-tabs-custom");
                    if (!string.IsNullOrEmpty(config.ColorTheme))
                        classes.Add($"nav-{config.ColorTheme}");
                    break;

                case TabsStyle.Pills:
                    classes.Add("nav-pills");
                    break;

                case TabsStyle.Standard:
                    classes.Add("nav-tabs");
                    break;
            }

            // Justified layout
            if (config.IsJustified)
            {
                classes.Add("nav-justified");
            }

            // Additional spacing for vertical tabs
            if (config.Layout == TabsLayout.Vertical)
            {
                classes.Add("text-center");
            }

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Build nav link CSS classes
        /// </summary>
        private static string BuildNavLinkClasses(bool isActive, bool isDisabled)
        {
            var classes = new List<string> { "nav-link" };

            if (isActive)
                classes.Add("active");

            if (isDisabled)
                classes.Add("disabled");

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Build tab pane CSS classes
        /// </summary>
        private static string BuildTabPaneClasses(bool isActive)
        {
            var classes = new List<string> { "tab-pane", "fade" };

            if (isActive)
            {
                classes.Add("show");
                classes.Add("active");
            }

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Build badge CSS classes
        /// </summary>
        private static string BuildBadgeClasses(string badgeColor)
        {
            return $"badge bg-{badgeColor} ms-1";
        }

        // ========== Test Data Generator ==========

        /// <summary>
        /// Creates a test tabs configuration with sample data
        /// </summary>
        public static TabsConfig CreateTestTabsConfig(
            TabsLayout layout = TabsLayout.Horizontal,
            TabsStyle style = TabsStyle.CustomBordered)
        {
            var config = new TabsConfig
            {
                TabsId = "testTabs",
                Layout = layout,
                Style = style,
                ColorTheme = "success",
                IsJustified = true,
                WrapInCard = true,
                CardTitle = "Test Tabs Component",
                CardSubtitle = $"Testing {layout} layout with {style} style"
            };

            config
                .WithTab("home", "Home",
                    icon: "ri-home-line",
                    badge: "5",
                    badgeColor: "danger",
                    isActive: true,
                    contentHtml: "<p>This is the home tab content. You can add any HTML here or use a partial view.</p>")
                .WithTab("profile", "Profile",
                    icon: "ri-user-line",
                    description: "User information",
                    contentHtml: "<p>Profile tab content with user information goes here.</p>")
                .WithTab("messages", "Messages",
                    icon: "ri-message-line",
                    badge: "New",
                    badgeColor: "success",
                    contentHtml: "<p>Your messages will appear here.</p>")
                .WithTab("settings", "Settings",
                    icon: "ri-settings-line",
                    description: "App settings",
                    contentHtml: "<p>Configure your application settings here.</p>")
                .WithTab("disabled", "Disabled",
                    icon: "ri-lock-line",
                    isDisabled: true,
                    contentHtml: "<p>This tab is disabled.</p>");

            return config;
        }
    }
}

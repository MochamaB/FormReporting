using FormReporting.Models.ViewModels.Components;

namespace FormReporting.Extensions
{
    /// <summary>
    /// Extension methods for building detail cards from configuration objects
    /// Handles all transformation logic - Views only provide data, Extensions build the structure
    /// Follows three-layer pattern: Config → Extension → ViewModel
    /// </summary>
    public static class DetailCardExtensions
    {
        /// <summary>
        /// Transforms DetailCardConfig into DetailCardViewModel
        /// Validates, orders items, applies defaults, transforms to render-ready ViewModels
        /// ALL logic happens here - partials receive fully prepared data with ZERO logic needed
        /// </summary>
        public static DetailCardViewModel BuildDetailCard(this DetailCardConfig config)
        {
            // 1. Validate
            if (string.IsNullOrEmpty(config.Title))
                throw new ArgumentException("Detail card must have a title");

            // 2. Sort items by DisplayOrder
            var orderedMetaItems = config.MetaItems.OrderBy(m => m.DisplayOrder).ToList();
            var orderedBadges = config.Badges.OrderBy(b => b.DisplayOrder).ToList();
            var orderedActions = config.Actions.OrderBy(a => a.DisplayOrder).ToList();

            // 3. Build view model
            return new DetailCardViewModel
            {
                Variant = config.Variant,
                BackgroundClasses = BuildBackgroundClasses(config),
                BackgroundImageUrl = config.BackgroundImageUrl,
                IconImageUrl = config.IconImageUrl,
                IconClass = config.IconClass,
                AvatarImageUrl = config.AvatarImageUrl,
                AvatarClasses = BuildAvatarClasses(config.AvatarSize),
                Title = config.Title,
                Subtitle = config.Subtitle,
                MetaItems = orderedMetaItems.Select(m => new DetailMetaItemViewModel
                {
                    IconClass = m.IconClass,
                    Label = m.Label,
                    Value = m.Value,
                    DisplayOrder = m.DisplayOrder
                }).ToList(),
                Badges = orderedBadges.Select(b => new DetailBadgeViewModel
                {
                    Text = b.Text,
                    BadgeClasses = BuildBadgeClasses(b.Color),
                    DisplayOrder = b.DisplayOrder
                }).ToList(),
                Actions = orderedActions.Select(a => new DetailActionViewModel
                {
                    IconClass = a.IconClass,
                    Title = a.Title,
                    ButtonClasses = BuildActionButtonClasses(a.IsActive),
                    ActionUrl = a.ActionUrl,
                    DisplayOrder = a.DisplayOrder
                }).ToList(),
                ContainerClasses = BuildContainerClasses(config)
            };
        }

        // ========== Fluent API Methods ==========

        /// <summary>
        /// Set detail card variant
        /// </summary>
        public static DetailCardConfig WithVariant(this DetailCardConfig config, DetailCardVariant variant)
        {
            config.Variant = variant;
            return config;
        }

        /// <summary>
        /// Set background color (for Standard variant)
        /// </summary>
        public static DetailCardConfig WithBackgroundColor(this DetailCardConfig config, string color)
        {
            config.BackgroundColor = color;
            return config;
        }

        /// <summary>
        /// Set background image (for Profile variant)
        /// </summary>
        public static DetailCardConfig WithBackgroundImage(this DetailCardConfig config, string imageUrl)
        {
            config.BackgroundImageUrl = imageUrl;
            return config;
        }

        /// <summary>
        /// Set icon/logo image
        /// </summary>
        public static DetailCardConfig WithIconImage(this DetailCardConfig config, string imageUrl)
        {
            config.IconImageUrl = imageUrl;
            return config;
        }

        /// <summary>
        /// Set icon class
        /// </summary>
        public static DetailCardConfig WithIconClass(this DetailCardConfig config, string iconClass)
        {
            config.IconClass = iconClass;
            return config;
        }

        /// <summary>
        /// Set avatar image
        /// </summary>
        public static DetailCardConfig WithAvatar(this DetailCardConfig config, string imageUrl, string size = "lg")
        {
            config.AvatarImageUrl = imageUrl;
            config.AvatarSize = size;
            return config;
        }

        /// <summary>
        /// Set title and subtitle
        /// </summary>
        public static DetailCardConfig WithTitle(this DetailCardConfig config, string title, string? subtitle = null)
        {
            config.Title = title;
            config.Subtitle = subtitle;
            return config;
        }

        /// <summary>
        /// Add a meta item
        /// </summary>
        public static DetailCardConfig WithMetaItem(this DetailCardConfig config,
            string value,
            string? label = null,
            string? iconClass = null)
        {
            config.MetaItems.Add(new DetailMetaItem
            {
                IconClass = iconClass,
                Label = label,
                Value = value,
                DisplayOrder = config.MetaItems.Count + 1
            });
            return config;
        }

        /// <summary>
        /// Add a badge
        /// </summary>
        public static DetailCardConfig WithBadge(this DetailCardConfig config,
            string text,
            string color = "info")
        {
            config.Badges.Add(new DetailBadge
            {
                Text = text,
                Color = color,
                DisplayOrder = config.Badges.Count + 1
            });
            return config;
        }

        /// <summary>
        /// Add an action button
        /// </summary>
        public static DetailCardConfig WithAction(this DetailCardConfig config,
            string iconClass,
            string? title = null,
            bool isActive = false,
            string? actionUrl = null)
        {
            config.Actions.Add(new DetailAction
            {
                IconClass = iconClass,
                Title = title,
                IsActive = isActive,
                ActionUrl = actionUrl,
                DisplayOrder = config.Actions.Count + 1
            });
            return config;
        }

        // ========== Helper Methods (For Extension Use Only - NOT for Views) ==========

        /// <summary>
        /// Build background CSS classes
        /// </summary>
        private static string BuildBackgroundClasses(DetailCardConfig config)
        {
            return config.Variant == DetailCardVariant.Standard
                ? $"bg-{config.BackgroundColor}"
                : "profile-foreground position-relative";
        }

        /// <summary>
        /// Build avatar CSS classes
        /// </summary>
        private static string BuildAvatarClasses(string size)
        {
            return $"avatar-{size}";
        }

        /// <summary>
        /// Build badge CSS classes
        /// </summary>
        private static string BuildBadgeClasses(string color)
        {
            return $"badge rounded-pill bg-{color} fs-12";
        }

        /// <summary>
        /// Build action button CSS classes
        /// </summary>
        private static string BuildActionButtonClasses(bool isActive)
        {
            var classes = new List<string>
            {
                "btn",
                "py-0",
                "fs-16",
                "favourite-btn",
                "material-shadow-none"
            };

            if (isActive)
                classes.Add("active");
            else
                classes.Add("text-body");

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Build container CSS classes
        /// </summary>
        private static string BuildContainerClasses(DetailCardConfig config)
        {
            var classes = new List<string> { "card" };

            if (config.UseNegativeMargin)
            {
                classes.Add("mt-n4");
                classes.Add("mx-n4");
            }

            if (!string.IsNullOrEmpty(config.ContainerCssClass))
                classes.Add(config.ContainerCssClass);

            return string.Join(" ", classes);
        }

        // ========== Test Data Generator ==========

        /// <summary>
        /// Creates a test detail card configuration with sample data (Standard variant)
        /// </summary>
        public static DetailCardConfig CreateTestDetailCardConfig()
        {
            var config = new DetailCardConfig
            {
                Variant = DetailCardVariant.Standard,
                BackgroundColor = "warning-subtle",
                IconImageUrl = "/assets/images/brands/slack.png",
                Title = "Velzon - Admin & Dashboard",
                Subtitle = null
            };

            config
                .WithMetaItem("Themesbrand", iconClass: "ri-building-line")
                .WithMetaItem("15 Sep, 2021", label: "Create Date :")
                .WithMetaItem("29 Dec, 2021", label: "Due Date :")
                .WithBadge("New", "info")
                .WithBadge("High", "danger")
                .WithAction("ri-star-fill", "Favorite", isActive: true)
                .WithAction("ri-share-line", "Share")
                .WithAction("ri-flag-line", "Flag");

            return config;
        }

        /// <summary>
        /// Creates a test profile detail card configuration (Profile variant)
        /// </summary>
        public static DetailCardConfig CreateTestProfileCardConfig()
        {
            var config = new DetailCardConfig
            {
                Variant = DetailCardVariant.Profile,
                BackgroundImageUrl = "/assets/images/profile-bg.jpg",
                AvatarImageUrl = "/assets/images/users/avatar-1.jpg",
                AvatarSize = "lg",
                Title = "Anna Adame",
                Subtitle = "Owner & Founder"
            };

            config
                .WithMetaItem("24", label: "Projects")
                .WithMetaItem("1.3K", label: "Followers")
                .WithMetaItem("856", label: "Following")
                .WithAction("ri-edit-line", "Edit Profile");

            return config;
        }
    }
}

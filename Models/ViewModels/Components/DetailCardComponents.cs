namespace FormReporting.Models.ViewModels.Components
{
    // ============================================================================
    // ENUMS
    // ============================================================================

    /// <summary>
    /// Detail card layout variants
    /// </summary>
    public enum DetailCardVariant
    {
        /// <summary>
        /// Project/Entity overview style with colored background
        /// Reference: apps-projects-overview.html
        /// </summary>
        Standard = 1,

        /// <summary>
        /// Profile style with background image banner
        /// Reference: pages-profile.html
        /// </summary>
        Profile = 2
    }

    // ============================================================================
    // CONFIGURATION CLASSES (What users create in views)
    // ============================================================================

    /// <summary>
    /// Configuration object for creating detail cards
    /// User creates this in views to define WHAT to display
    /// Extensions handle HOW to transform this into DetailCardViewModel
    /// </summary>
    public class DetailCardConfig
    {
        /// <summary>
        /// Card variant/style
        /// </summary>
        public DetailCardVariant Variant { get; set; } = DetailCardVariant.Standard;

        /// <summary>
        /// Background color class (e.g., "warning-subtle", "info-subtle", "primary-subtle")
        /// Only for Standard variant
        /// </summary>
        public string BackgroundColor { get; set; } = "warning-subtle";

        /// <summary>
        /// Background image URL (for Profile variant)
        /// </summary>
        public string? BackgroundImageUrl { get; set; }

        /// <summary>
        /// Icon/Logo image URL (for Standard variant)
        /// </summary>
        public string? IconImageUrl { get; set; }

        /// <summary>
        /// Icon class (alternative to image, e.g., "ri-building-line")
        /// </summary>
        public string? IconClass { get; set; }

        /// <summary>
        /// Avatar image URL (for Profile variant)
        /// </summary>
        public string? AvatarImageUrl { get; set; }

        /// <summary>
        /// Avatar size (sm, md, lg, xl)
        /// </summary>
        public string AvatarSize { get; set; } = "lg";

        /// <summary>
        /// Main title/heading
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Subtitle/description (e.g., job title, company name)
        /// </summary>
        public string? Subtitle { get; set; }

        /// <summary>
        /// Meta information items (dates, status, etc.)
        /// Displayed in horizontal stack with dividers
        /// </summary>
        public List<DetailMetaItem> MetaItems { get; set; } = new();

        /// <summary>
        /// Badges (tags, status badges, priority badges)
        /// </summary>
        public List<DetailBadge> Badges { get; set; } = new();

        /// <summary>
        /// Action buttons (favorite, share, edit, etc.)
        /// </summary>
        public List<DetailAction> Actions { get; set; } = new();

        /// <summary>
        /// Additional CSS classes for container
        /// </summary>
        public string? ContainerCssClass { get; set; }

        /// <summary>
        /// Apply negative margin to extend card edges
        /// </summary>
        public bool UseNegativeMargin { get; set; } = true;
    }

    /// <summary>
    /// Meta information item (label + value)
    /// </summary>
    public class DetailMetaItem
    {
        /// <summary>
        /// Icon class (optional, e.g., "ri-building-line")
        /// </summary>
        public string? IconClass { get; set; }

        /// <summary>
        /// Label/title (optional, e.g., "Create Date :")
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Value/content
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Badge configuration
    /// </summary>
    public class DetailBadge
    {
        /// <summary>
        /// Badge text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Badge color (info, danger, success, warning, etc.)
        /// </summary>
        public string Color { get; set; } = "info";

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Action button configuration
    /// </summary>
    public class DetailAction
    {
        /// <summary>
        /// Button icon class (e.g., "ri-star-fill", "ri-share-line")
        /// </summary>
        public string IconClass { get; set; } = string.Empty;

        /// <summary>
        /// Button title/tooltip
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Is this action active? (e.g., favorited)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Action URL or JavaScript function
        /// </summary>
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }

    // ============================================================================
    // VIEW MODEL CLASSES (What partials receive for rendering)
    // ============================================================================

    /// <summary>
    /// Detail card view model - ready for rendering
    /// Created by DetailCardExtensions.BuildDetailCard()
    /// Contains all render-ready data, no logic needed in views
    /// </summary>
    public class DetailCardViewModel
    {
        /// <summary>
        /// Card variant
        /// </summary>
        public DetailCardVariant Variant { get; set; }

        /// <summary>
        /// Pre-computed background classes
        /// </summary>
        public string BackgroundClasses { get; set; } = string.Empty;

        /// <summary>
        /// Background image URL (if applicable)
        /// </summary>
        public string? BackgroundImageUrl { get; set; }

        /// <summary>
        /// Icon/Logo image URL
        /// </summary>
        public string? IconImageUrl { get; set; }

        /// <summary>
        /// Icon class
        /// </summary>
        public string? IconClass { get; set; }

        /// <summary>
        /// Avatar image URL
        /// </summary>
        public string? AvatarImageUrl { get; set; }

        /// <summary>
        /// Pre-computed avatar classes
        /// </summary>
        public string AvatarClasses { get; set; } = string.Empty;

        /// <summary>
        /// Main title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Subtitle
        /// </summary>
        public string? Subtitle { get; set; }

        /// <summary>
        /// Ordered meta items (render-ready)
        /// </summary>
        public List<DetailMetaItemViewModel> MetaItems { get; set; } = new();

        /// <summary>
        /// Ordered badges (render-ready)
        /// </summary>
        public List<DetailBadgeViewModel> Badges { get; set; } = new();

        /// <summary>
        /// Ordered actions (render-ready)
        /// </summary>
        public List<DetailActionViewModel> Actions { get; set; } = new();

        /// <summary>
        /// Pre-computed container classes
        /// </summary>
        public string ContainerClasses { get; set; } = string.Empty;

        /// <summary>
        /// Has any icon/avatar?
        /// </summary>
        public bool HasIcon => !string.IsNullOrEmpty(IconImageUrl) || !string.IsNullOrEmpty(IconClass);

        /// <summary>
        /// Has avatar?
        /// </summary>
        public bool HasAvatar => !string.IsNullOrEmpty(AvatarImageUrl);

        /// <summary>
        /// Has meta items?
        /// </summary>
        public bool HasMetaItems => MetaItems.Any();

        /// <summary>
        /// Has badges?
        /// </summary>
        public bool HasBadges => Badges.Any();

        /// <summary>
        /// Has actions?
        /// </summary>
        public bool HasActions => Actions.Any();
    }

    /// <summary>
    /// Meta item view model (render-ready)
    /// </summary>
    public class DetailMetaItemViewModel
    {
        /// <summary>
        /// Icon class
        /// </summary>
        public string? IconClass { get; set; }

        /// <summary>
        /// Label
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Has icon?
        /// </summary>
        public bool HasIcon => !string.IsNullOrEmpty(IconClass);

        /// <summary>
        /// Has label?
        /// </summary>
        public bool HasLabel => !string.IsNullOrEmpty(Label);
    }

    /// <summary>
    /// Badge view model (render-ready)
    /// </summary>
    public class DetailBadgeViewModel
    {
        /// <summary>
        /// Badge text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Pre-computed badge classes
        /// </summary>
        public string BadgeClasses { get; set; } = string.Empty;

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Action view model (render-ready)
    /// </summary>
    public class DetailActionViewModel
    {
        /// <summary>
        /// Button icon class
        /// </summary>
        public string IconClass { get; set; } = string.Empty;

        /// <summary>
        /// Button title/tooltip
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Pre-computed button classes
        /// </summary>
        public string ButtonClasses { get; set; } = string.Empty;

        /// <summary>
        /// Action URL or JavaScript
        /// </summary>
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}

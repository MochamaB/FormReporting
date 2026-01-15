namespace FormReporting.Models.ViewModels.Dashboard.Widgets
{
    /// <summary>
    /// Data view model for List widgets
    /// Displays a list of items with optional icons, badges, and links
    /// </summary>
    public class ListDataViewModel
    {
        /// <summary>
        /// List items to display
        /// </summary>
        public List<ListItemViewModel> Items { get; set; } = new();

        /// <summary>
        /// Maximum number of items to display (0 = unlimited)
        /// </summary>
        public int MaxItems { get; set; } = 0;

        /// <summary>
        /// Whether to show item numbers/ranking
        /// </summary>
        public bool ShowNumbers { get; set; } = false;

        /// <summary>
        /// Whether to show dividers between items
        /// </summary>
        public bool ShowDividers { get; set; } = true;

        /// <summary>
        /// Empty state message when no items
        /// </summary>
        public string EmptyMessage { get; set; } = "No items to display";

        /// <summary>
        /// Optional "View All" link URL
        /// </summary>
        public string? ViewAllUrl { get; set; }

        /// <summary>
        /// "View All" link text
        /// </summary>
        public string ViewAllText { get; set; } = "View All";

        /// <summary>
        /// Gets whether there are any items
        /// </summary>
        public bool HasItems => Items.Count > 0;

        /// <summary>
        /// Gets the items to display (limited by MaxItems if set)
        /// </summary>
        public IEnumerable<ListItemViewModel> DisplayItems =>
            MaxItems > 0 ? Items.Take(MaxItems) : Items;
    }

    /// <summary>
    /// Single item in a list widget
    /// </summary>
    public class ListItemViewModel
    {
        /// <summary>
        /// Primary text for the item
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Secondary/subtitle text
        /// </summary>
        public string? SubText { get; set; }

        /// <summary>
        /// Icon class for the item (e.g., "ri-file-list-line")
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Icon color class (e.g., "primary", "success")
        /// </summary>
        public string IconColor { get; set; } = "primary";

        /// <summary>
        /// Optional link URL for the item
        /// </summary>
        public string? Link { get; set; }

        /// <summary>
        /// Badge text (e.g., count, status)
        /// </summary>
        public string? Badge { get; set; }

        /// <summary>
        /// Badge color class (e.g., "primary", "success", "warning", "danger")
        /// </summary>
        public string BadgeColor { get; set; } = "primary";

        /// <summary>
        /// Optional right-side value (e.g., amount, percentage)
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Color class for the value
        /// </summary>
        public string? ValueColor { get; set; }

        /// <summary>
        /// Optional timestamp for the item
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Formatted timestamp string
        /// </summary>
        public string? TimestampText { get; set; }

        /// <summary>
        /// Optional avatar/image URL
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Avatar initials if no image (e.g., "JD" for John Doe)
        /// </summary>
        public string? AvatarInitials { get; set; }

        /// <summary>
        /// CSS class for the list item
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Whether this item is highlighted/featured
        /// </summary>
        public bool IsHighlighted { get; set; } = false;

        /// <summary>
        /// Whether the item is clickable (has a link)
        /// </summary>
        public bool IsClickable => !string.IsNullOrEmpty(Link);
    }
}

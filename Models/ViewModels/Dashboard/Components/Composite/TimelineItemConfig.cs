using FormReporting.Models.ViewModels.Dashboard.Components.Atomic;

namespace FormReporting.Models.ViewModels.Dashboard.Components.Composite
{
    /// <summary>
    /// TIER 2 COMPOSITE: Timeline item
    /// Single timeline entry with icon, content, and timestamp
    /// </summary>
    public class TimelineItemConfig
    {
        /// <summary>
        /// Icon configuration
        /// </summary>
        public IconConfig Icon { get; set; } = new IconConfig { Size = "xs" };

        /// <summary>
        /// Item title/heading
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Item description/content
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Timestamp label (e.g., "2 hours ago", "Yesterday")
        /// </summary>
        public string Timestamp { get; set; } = string.Empty;

        /// <summary>
        /// Badge (optional - for status/category)
        /// </summary>
        public BadgeConfig? Badge { get; set; }

        /// <summary>
        /// Is this the last item? (affects vertical line rendering)
        /// </summary>
        public bool IsLast { get; set; } = false;

        /// <summary>
        /// Additional content (HTML)
        /// </summary>
        public string? AdditionalContent { get; set; }

        /// <summary>
        /// Helper: Create activity timeline item
        /// </summary>
        public static TimelineItemConfig Activity(
            string title,
            string description,
            string timestamp,
            string iconClass = "ri-checkbox-circle-line",
            string colorTheme = "success")
        {
            return new TimelineItemConfig
            {
                Title = title,
                Description = description,
                Timestamp = timestamp,
                Icon = IconConfig.Remix(iconClass, colorTheme, "xs")
            };
        }

        /// <summary>
        /// Helper: Create event timeline item
        /// </summary>
        public static TimelineItemConfig Event(
            string title,
            string timestamp,
            string iconClass = "ri-calendar-event-line",
            string colorTheme = "info")
        {
            return new TimelineItemConfig
            {
                Title = title,
                Timestamp = timestamp,
                Icon = IconConfig.Remix(iconClass, colorTheme, "xs")
            };
        }
    }

    /// <summary>
    /// Timeline container (list of timeline items)
    /// </summary>
    public class TimelineConfig
    {
        /// <summary>
        /// Timeline items
        /// </summary>
        public List<TimelineItemConfig> Items { get; set; } = new List<TimelineItemConfig>();

        /// <summary>
        /// Timeline title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Show in card wrapper
        /// </summary>
        public bool ShowCard { get; set; } = true;

        /// <summary>
        /// Card CSS classes
        /// </summary>
        public string? CardClass { get; set; }
    }
}

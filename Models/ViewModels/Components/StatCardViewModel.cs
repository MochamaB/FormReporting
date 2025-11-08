namespace FormReporting.Models.ViewModels.Components
{
    /// <summary>
    /// Configuration object for creating a row of statistic cards
    /// Used to define WHAT data to display (titles, values, icons, etc.)
    /// Extensions handle HOW to transform this into individual card ViewModels
    /// </summary>
    public class StatsRowConfig
    {
        /// <summary>
        /// Titles for each card (e.g., "Total Users", "Active Roles")
        /// </summary>
        public List<string> Titles { get; set; } = new();

        /// <summary>
        /// Values to display (e.g., "1,234", "$456k", "78%")
        /// </summary>
        public List<string> Values { get; set; } = new();

        /// <summary>
        /// Remix icon classes (e.g., "ri-user-line", "ri-shield-check-line")
        /// </summary>
        public List<string> Icons { get; set; } = new();

        /// <summary>
        /// Color themes: primary, secondary, success, warning, danger, info
        /// Optional - defaults to ["primary", "secondary", "success", "warning"] cycling
        /// </summary>
        public List<string>? ColorThemes { get; set; }

        /// <summary>
        /// Type of stat card to render
        /// </summary>
        public CardType CardType { get; set; } = CardType.TileBoxCard;

        /// <summary>
        /// Bootstrap grid column class (default: "col-xl-3 col-md-6" for 4 cards per row)
        /// </summary>
        public string ColumnClass { get; set; } = "col-xl-3 col-md-6";

        // ========== Optional Features for TileBoxCard ==========

        /// <summary>
        /// Links shown at bottom of TileBoxCard (e.g., "View net earnings")
        /// </summary>
        public List<string>? LinkTexts { get; set; }

        /// <summary>
        /// URLs for the links
        /// </summary>
        public List<string>? LinkUrls { get; set; }

        /// <summary>
        /// Trend percentages (e.g., "16.24", "3.57")
        /// </summary>
        public List<string>? TrendPercentages { get; set; }

        /// <summary>
        /// Direction of trend (Up/Down/Neutral)
        /// </summary>
        public List<TrendDirection>? TrendDirections { get; set; }

        // ========== Optional Features for StatisticsCard ==========

        /// <summary>
        /// Comparison text (e.g., "vs. previous month", "vs. last year")
        /// </summary>
        public List<string>? ComparisonTexts { get; set; }

        /// <summary>
        /// Badge values for comparison (e.g., "16.24 %", "3.96 %")
        /// </summary>
        public List<string>? BadgeValues { get; set; }

        /// <summary>
        /// Badge colors: success, danger, warning, info
        /// </summary>
        public List<string>? BadgeColors { get; set; }

        // ========== Optional Features for IconLeftCard ==========

        /// <summary>
        /// Subtitle text shown below main value (e.g., "From 1930 last year")
        /// </summary>
        public List<string>? Subtitles { get; set; }
    }

    /// <summary>
    /// Individual statistic card view model
    /// This is what gets passed to partial views for rendering
    /// </summary>
    public class StatCardViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = "ri-dashboard-line";
        public string ColorTheme { get; set; } = "primary";
        public CardType CardType { get; set; } = CardType.TileBoxCard;
        public string ColumnClass { get; set; } = "col-xl-3 col-md-6";

        // Optional properties
        public string? LinkText { get; set; }
        public string? LinkUrl { get; set; }
        public string? TrendPercentage { get; set; }
        public TrendDirection TrendDirection { get; set; } = TrendDirection.Neutral;
        public string? ComparisonText { get; set; }
        public string? BadgeValue { get; set; }
        public string? BadgeColor { get; set; }
        public string? Subtitle { get; set; }

        // For background fill cards
        public bool HasBackgroundColor { get; set; } = false;
    }

    /// <summary>
    /// Types of statistic cards available in Velzon theme
    /// </summary>
    public enum CardType
    {
        /// <summary>
        /// Card with animated effect, trend at top, value in middle, link at bottom, icon on right
        /// Used for: Dashboard overview cards (Earnings, Orders, Customers, Balance)
        /// Features: Optional background color, trend indicator, bottom link
        /// </summary>
        TileBoxCard,

        /// <summary>
        /// Single card containing multiple statistics in columns with borders
        /// Used for: CRM metrics, grouped statistics
        /// Features: Horizontal layout, multiple stats in one card, arrow indicators
        /// </summary>
        CRMWidgetCard,

        /// <summary>
        /// Card with title, large value, comparison badge, icon on top right
        /// Used for: Analytics metrics (Users, Sessions, Visit Duration, Bounce Rate)
        /// Features: "vs. previous month" badge, optional background color
        /// </summary>
        StatisticsCard,

        /// <summary>
        /// Card with icon on left, title and value in middle, trend badge on far right
        /// Used for: Summary metrics (Total Sales, Users, Revenue, Stores)
        /// Features: Icon-first layout, trend badge, optional background color
        /// </summary>
        IconLeftCard
    }

    /// <summary>
    /// Trend direction for indicators
    /// </summary>
    public enum TrendDirection
    {
        Up,
        Down,
        Neutral
    }
}

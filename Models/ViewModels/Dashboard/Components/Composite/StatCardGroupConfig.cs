namespace FormReporting.Models.ViewModels.Dashboard.Components.Composite
{
    /// <summary>
    /// TIER 2 COMPOSITE: Stat card group
    /// Displays multiple stat cards in a responsive row
    /// </summary>
    public class StatCardGroupConfig
    {
        /// <summary>
        /// List of stat cards to display
        /// </summary>
        public List<StatCardConfig> Cards { get; set; } = new List<StatCardConfig>();

        /// <summary>
        /// Column configuration: col-xl-3, col-lg-4, col-md-6, etc.
        /// </summary>
        public string ColumnClass { get; set; } = "col-xl-3 col-md-6";

        /// <summary>
        /// Row CSS classes
        /// </summary>
        public string? RowClass { get; set; }

        /// <summary>
        /// Add margin bottom to row
        /// </summary>
        public string MarginBottom { get; set; } = "mb-4";

        /// <summary>
        /// Helper: Create 4-column layout (most common)
        /// </summary>
        public static StatCardGroupConfig FourColumns(params StatCardConfig[] cards)
        {
            return new StatCardGroupConfig
            {
                Cards = cards.ToList(),
                ColumnClass = "col-xl-3 col-md-6" // 4 cols on xl, 2 on md
            };
        }

        /// <summary>
        /// Helper: Create 3-column layout
        /// </summary>
        public static StatCardGroupConfig ThreeColumns(params StatCardConfig[] cards)
        {
            return new StatCardGroupConfig
            {
                Cards = cards.ToList(),
                ColumnClass = "col-xl-4 col-md-6" // 3 cols on xl, 2 on md
            };
        }

        /// <summary>
        /// Helper: Create 2-column layout
        /// </summary>
        public static StatCardGroupConfig TwoColumns(params StatCardConfig[] cards)
        {
            return new StatCardGroupConfig
            {
                Cards = cards.ToList(),
                ColumnClass = "col-lg-6" // 2 cols on lg and up
            };
        }
    }
}

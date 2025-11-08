using FormReporting.Models.ViewModels.Components;

namespace FormReporting.Extensions
{
    /// <summary>
    /// Extension methods for building statistic cards from configuration objects
    /// Handles all transformation logic - Views only provide data, Extensions build the structure
    /// </summary>
    public static class StatCardExtensions
    {
        // Default color themes for cycling when not specified
        private static readonly string[] DefaultColorThemes = { "primary", "secondary", "success", "warning" };

        /// <summary>
        /// Transforms StatsRowConfig into a list of StatCardViewModel objects
        /// Each element in the Titles/Values/Icons arrays becomes one card
        /// </summary>
        public static List<StatCardViewModel> BuildStatsRow(this StatsRowConfig config)
        {
            var cards = new List<StatCardViewModel>();

            // Validate that we have at least titles
            if (config.Titles == null || !config.Titles.Any())
            {
                throw new ArgumentException("StatsRowConfig must have at least one title");
            }

            // Ensure Values and Icons lists match Titles length
            int cardCount = config.Titles.Count;
            config.Values = EnsureListLength(config.Values, cardCount, "0");
            config.Icons = EnsureListLength(config.Icons, cardCount, "ri-dashboard-line");

            // Build each card
            for (int i = 0; i < cardCount; i++)
            {
                var card = new StatCardViewModel
                {
                    Title = config.Titles[i],
                    Value = config.Values[i],
                    Icon = config.Icons[i],
                    ColorTheme = GetColorTheme(config.ColorThemes, i),
                    CardType = config.CardType,
                    ColumnClass = config.ColumnClass
                };

                // Apply optional properties based on card type
                ApplyOptionalProperties(card, config, i);

                cards.Add(card);
            }

            return cards;
        }

        /// <summary>
        /// Fluent API: Set card type
        /// </summary>
        public static StatsRowConfig WithCardType(this StatsRowConfig config, CardType cardType)
        {
            config.CardType = cardType;
            return config;
        }

        /// <summary>
        /// Fluent API: Set column class for responsive grid
        /// </summary>
        public static StatsRowConfig WithColumnClass(this StatsRowConfig config, string columnClass)
        {
            config.ColumnClass = columnClass;
            return config;
        }

        /// <summary>
        /// Fluent API: Add trend indicators (for TileBoxCard and IconLeftCard)
        /// </summary>
        public static StatsRowConfig WithTrends(this StatsRowConfig config,
            List<string> percentages,
            List<TrendDirection> directions)
        {
            config.TrendPercentages = percentages;
            config.TrendDirections = directions;
            return config;
        }

        /// <summary>
        /// Fluent API: Add comparison badges (for StatisticsCard)
        /// </summary>
        public static StatsRowConfig WithComparisons(this StatsRowConfig config,
            List<string> badgeValues,
            List<string> badgeColors,
            List<string>? comparisonTexts = null)
        {
            config.BadgeValues = badgeValues;
            config.BadgeColors = badgeColors;
            config.ComparisonTexts = comparisonTexts ?? Enumerable.Repeat("vs. previous month", config.Titles.Count).ToList();
            return config;
        }

        /// <summary>
        /// Fluent API: Add bottom links (for TileBoxCard)
        /// </summary>
        public static StatsRowConfig WithLinks(this StatsRowConfig config,
            List<string> linkTexts,
            List<string> linkUrls)
        {
            config.LinkTexts = linkTexts;
            config.LinkUrls = linkUrls;
            return config;
        }

        /// <summary>
        /// Fluent API: Add subtitles (for IconLeftCard)
        /// </summary>
        public static StatsRowConfig WithSubtitles(this StatsRowConfig config, List<string> subtitles)
        {
            config.Subtitles = subtitles;
            return config;
        }

        /// <summary>
        /// Fluent API: Make specific card have background color
        /// </summary>
        public static StatsRowConfig WithBackgroundColors(this StatsRowConfig config, params int[] cardIndices)
        {
            // This is stored in ColorThemes - we'll apply bg-{theme} class in partial view
            // The card will check HasBackgroundColor flag
            return config;
        }

        // ========== PRIVATE HELPER METHODS ==========

        private static List<T> EnsureListLength<T>(List<T> list, int requiredLength, T defaultValue)
        {
            if (list == null)
            {
                list = new List<T>();
            }

            while (list.Count < requiredLength)
            {
                list.Add(defaultValue);
            }

            return list;
        }

        private static string GetColorTheme(List<string>? colorThemes, int index)
        {
            if (colorThemes != null && index < colorThemes.Count)
            {
                return colorThemes[index];
            }

            // Cycle through default themes
            return DefaultColorThemes[index % DefaultColorThemes.Length];
        }

        private static void ApplyOptionalProperties(StatCardViewModel card, StatsRowConfig config, int index)
        {
            // Links (TileBoxCard)
            if (config.LinkTexts != null && index < config.LinkTexts.Count)
            {
                card.LinkText = config.LinkTexts[index];
            }
            if (config.LinkUrls != null && index < config.LinkUrls.Count)
            {
                card.LinkUrl = config.LinkUrls[index];
            }

            // Trends (TileBoxCard, IconLeftCard)
            if (config.TrendPercentages != null && index < config.TrendPercentages.Count)
            {
                card.TrendPercentage = config.TrendPercentages[index];
            }
            if (config.TrendDirections != null && index < config.TrendDirections.Count)
            {
                card.TrendDirection = config.TrendDirections[index];
            }

            // Comparisons (StatisticsCard)
            if (config.ComparisonTexts != null && index < config.ComparisonTexts.Count)
            {
                card.ComparisonText = config.ComparisonTexts[index];
            }
            if (config.BadgeValues != null && index < config.BadgeValues.Count)
            {
                card.BadgeValue = config.BadgeValues[index];
            }
            if (config.BadgeColors != null && index < config.BadgeColors.Count)
            {
                card.BadgeColor = config.BadgeColors[index];
            }

            // Subtitles (IconLeftCard)
            if (config.Subtitles != null && index < config.Subtitles.Count)
            {
                card.Subtitle = config.Subtitles[index];
            }

            // Check if this card should have background color
            // For now, we'll determine this based on CardType and specific color themes
            // Cards with "info", "primary", "success" backgrounds in examples
            card.HasBackgroundColor = false; // Will be set explicitly when creating cards
        }

        /// <summary>
        /// Get trend icon and color based on direction
        /// </summary>
        public static (string icon, string colorClass) GetTrendIndicator(this TrendDirection direction)
        {
            return direction switch
            {
                TrendDirection.Up => ("ri-arrow-right-up-line", "text-success"),
                TrendDirection.Down => ("ri-arrow-right-down-line", "text-danger"),
                TrendDirection.Neutral => ("ri-subtract-line", "text-muted"),
                _ => ("ri-subtract-line", "text-muted")
            };
        }

        /// <summary>
        /// Helper to create default test data for a stat card row
        /// Useful for quickly testing card layouts without setting up data
        /// </summary>
        public static StatsRowConfig CreateDefaultTestConfig(CardType cardType = CardType.TileBoxCard)
        {
            return cardType switch
            {
                CardType.TileBoxCard => new StatsRowConfig
                {
                    Titles = new List<string> { "Total Earnings", "Orders", "Customers", "My Balance" },
                    Values = new List<string> { "$559.25k", "36,894", "183.35M", "$165.89k" },
                    Icons = new List<string> { "ri-money-dollar-circle-line", "ri-shopping-bag-line", "ri-user-line", "ri-wallet-line" },
                    ColorThemes = new List<string> { "success", "info", "warning", "primary" },
                    CardType = CardType.TileBoxCard,
                    LinkTexts = new List<string> { "View net earnings", "View all orders", "See details", "Withdraw money" },
                    LinkUrls = new List<string> { "#", "#", "#", "#" },
                    TrendPercentages = new List<string> { "16.24", "3.57", "29.08", "0.00" },
                    TrendDirections = new List<TrendDirection> { TrendDirection.Up, TrendDirection.Down, TrendDirection.Up, TrendDirection.Neutral }
                },

                CardType.StatisticsCard => new StatsRowConfig
                {
                    Titles = new List<string> { "Users", "Sessions", "Avg. Visit Duration", "Bounce Rate" },
                    Values = new List<string> { "28.05k", "97.66k", "3m 40sec", "33.48%" },
                    Icons = new List<string> { "ri-user-line", "ri-pulse-line", "ri-time-line", "ri-external-link-line" },
                    ColorThemes = new List<string> { "info", "info", "primary", "info" },
                    CardType = CardType.StatisticsCard,
                    BadgeValues = new List<string> { "16.24 %", "3.96 %", "0.24 %", "7.05 %" },
                    BadgeColors = new List<string> { "success", "danger", "danger", "success" },
                    ComparisonTexts = Enumerable.Repeat("vs. previous month", 4).ToList()
                },

                CardType.IconLeftCard => new StatsRowConfig
                {
                    Titles = new List<string> { "Total Sales", "Number of Users", "Total Revenue", "Number of Stores" },
                    Values = new List<string> { "2,045", "7,522", "$2,845.05", "405k" },
                    Icons = new List<string> { "bx bx-shopping-bag", "bx bxs-user-account", "bx bxs-badge-dollar", "bx bx-store-alt" },
                    ColorThemes = new List<string> { "success", "warning", "danger", "info" },
                    CardType = CardType.IconLeftCard,
                    Subtitles = new List<string> { "From 1930 last year", "From 9530 last year", "From $1,750.04 last year", "From 308 last year" },
                    TrendPercentages = new List<string> { "6.11", "10.35", "22.96", "16.31" },
                    TrendDirections = new List<TrendDirection> { TrendDirection.Up, TrendDirection.Down, TrendDirection.Up, TrendDirection.Up }
                },

                _ => new StatsRowConfig
                {
                    Titles = new List<string> { "Default Card" },
                    Values = new List<string> { "100" },
                    Icons = new List<string> { "ri-dashboard-line" },
                    CardType = cardType
                }
            };
        }
    }
}

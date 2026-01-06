using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Data.Seeders
{
    /// <summary>
    /// Seeds predefined metric units.
    /// IMPORTANT: The order of units determines their IDs (auto-increment).
    /// MetricSubCategorySeeder and MetricSubCategoryUnitSeeder depend on these IDs.
    /// DO NOT reorder without updating dependent seeders.
    ///
    /// Unit IDs:
    /// 1=COUNT, 2=ITEMS, 3=UNITS, 4=PERCENT, 5=RATIO,
    /// 6=SCORE, 7=POINTS, 8=RATING, 9=DAYS, 10=HOURS,
    /// 11=MINUTES, 12=SECONDS, 13=GB, 14=MB, 15=TB,
    /// 16=KES, 17=USD, 18=STATUS, 19=VERSION, 20=NONE
    /// </summary>
    public static class MetricUnitSeeder
    {
        public static void SeedMetricUnits(ApplicationDbContext context)
        {
            // Skip if units already exist
            if (context.MetricUnits.Any())
                return;

            var units = new List<MetricUnit>
            {
                // ============================================================
                // QUANTITY UNITS (IDs 1-3)
                // ============================================================
                // ID 1
                new MetricUnit
                {
                    UnitCode = "COUNT",
                    UnitName = "Count",
                    UnitSymbol = "",
                    FormatPattern = "{0}",
                    SuggestedAggregation = "SUM",
                    UnitCategory = "Quantity",
                    Description = "Simple count of items",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                // ID 2
                new MetricUnit
                {
                    UnitCode = "ITEMS",
                    UnitName = "Items",
                    UnitSymbol = "",
                    FormatPattern = "{0} items",
                    SuggestedAggregation = "SUM",
                    UnitCategory = "Quantity",
                    Description = "Number of items",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                // ID 3
                new MetricUnit
                {
                    UnitCode = "UNITS",
                    UnitName = "Units",
                    UnitSymbol = "",
                    FormatPattern = "{0} units",
                    SuggestedAggregation = "SUM",
                    UnitCategory = "Quantity",
                    Description = "Number of units",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // PERCENTAGE/RATIO UNITS (IDs 4-5)
                // ============================================================
                // ID 4
                new MetricUnit
                {
                    UnitCode = "PERCENT",
                    UnitName = "Percentage",
                    UnitSymbol = "%",
                    FormatPattern = "{0}%",
                    SuggestedAggregation = "AVG",
                    UnitCategory = "Percentage",
                    Description = "Percentage value (0-100)",
                    DisplayOrder = 10,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                // ID 5
                new MetricUnit
                {
                    UnitCode = "RATIO",
                    UnitName = "Ratio",
                    UnitSymbol = "",
                    FormatPattern = "{0:F2}",
                    SuggestedAggregation = "AVG",
                    UnitCategory = "Percentage",
                    Description = "Ratio value (0-1 or x:y)",
                    DisplayOrder = 11,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // SCORE UNITS (IDs 6-8)
                // ============================================================
                // ID 6
                new MetricUnit
                {
                    UnitCode = "SCORE",
                    UnitName = "Score",
                    UnitSymbol = "",
                    FormatPattern = "{0}",
                    SuggestedAggregation = "AVG",
                    UnitCategory = "Score",
                    Description = "Numeric score value",
                    DisplayOrder = 20,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                // ID 7
                new MetricUnit
                {
                    UnitCode = "POINTS",
                    UnitName = "Points",
                    UnitSymbol = "pts",
                    FormatPattern = "{0} pts",
                    SuggestedAggregation = "SUM",
                    UnitCategory = "Score",
                    Description = "Points earned or accumulated",
                    DisplayOrder = 21,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                // ID 8
                new MetricUnit
                {
                    UnitCode = "RATING",
                    UnitName = "Rating",
                    UnitSymbol = "",
                    FormatPattern = "{0}/5",
                    SuggestedAggregation = "AVG",
                    UnitCategory = "Score",
                    Description = "Rating on a scale (e.g., 1-5)",
                    DisplayOrder = 22,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // TIME UNITS (IDs 9-12)
                // ============================================================
                // ID 9
                new MetricUnit
                {
                    UnitCode = "DAYS",
                    UnitName = "Days",
                    UnitSymbol = "d",
                    FormatPattern = "{0} days",
                    SuggestedAggregation = "AVG",
                    UnitCategory = "Time",
                    Description = "Duration in days",
                    DisplayOrder = 30,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                // ID 10
                new MetricUnit
                {
                    UnitCode = "HOURS",
                    UnitName = "Hours",
                    UnitSymbol = "hrs",
                    FormatPattern = "{0} hrs",
                    SuggestedAggregation = "SUM",
                    UnitCategory = "Time",
                    Description = "Duration in hours",
                    DisplayOrder = 31,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                // ID 11
                new MetricUnit
                {
                    UnitCode = "MINUTES",
                    UnitName = "Minutes",
                    UnitSymbol = "min",
                    FormatPattern = "{0} min",
                    SuggestedAggregation = "AVG",
                    UnitCategory = "Time",
                    Description = "Duration in minutes",
                    DisplayOrder = 32,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                // ID 12
                new MetricUnit
                {
                    UnitCode = "SECONDS",
                    UnitName = "Seconds",
                    UnitSymbol = "sec",
                    FormatPattern = "{0} sec",
                    SuggestedAggregation = "AVG",
                    UnitCategory = "Time",
                    Description = "Duration in seconds",
                    DisplayOrder = 33,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // DATA SIZE UNITS (IDs 13-15)
                // ============================================================
                // ID 13
                new MetricUnit
                {
                    UnitCode = "GB",
                    UnitName = "Gigabytes",
                    UnitSymbol = "GB",
                    FormatPattern = "{0} GB",
                    SuggestedAggregation = "SUM",
                    UnitCategory = "Data",
                    Description = "Data size in gigabytes",
                    DisplayOrder = 40,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                // ID 14
                new MetricUnit
                {
                    UnitCode = "MB",
                    UnitName = "Megabytes",
                    UnitSymbol = "MB",
                    FormatPattern = "{0} MB",
                    SuggestedAggregation = "SUM",
                    UnitCategory = "Data",
                    Description = "Data size in megabytes",
                    DisplayOrder = 41,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                // ID 15
                new MetricUnit
                {
                    UnitCode = "TB",
                    UnitName = "Terabytes",
                    UnitSymbol = "TB",
                    FormatPattern = "{0} TB",
                    SuggestedAggregation = "SUM",
                    UnitCategory = "Data",
                    Description = "Data size in terabytes",
                    DisplayOrder = 42,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // CURRENCY UNITS (IDs 16-17)
                // ============================================================
                // ID 16
                new MetricUnit
                {
                    UnitCode = "KES",
                    UnitName = "Kenyan Shillings",
                    UnitSymbol = "KES",
                    FormatPattern = "KES {0:N2}",
                    SuggestedAggregation = "SUM",
                    UnitCategory = "Currency",
                    Description = "Amount in Kenyan Shillings",
                    DisplayOrder = 50,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                // ID 17
                new MetricUnit
                {
                    UnitCode = "USD",
                    UnitName = "US Dollars",
                    UnitSymbol = "$",
                    FormatPattern = "${0:N2}",
                    SuggestedAggregation = "SUM",
                    UnitCategory = "Currency",
                    Description = "Amount in US Dollars",
                    DisplayOrder = 51,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // STATUS UNITS (IDs 18-19)
                // ============================================================
                // ID 18
                new MetricUnit
                {
                    UnitCode = "STATUS",
                    UnitName = "Status",
                    UnitSymbol = "",
                    FormatPattern = "{0}",
                    SuggestedAggregation = "LAST_VALUE",
                    UnitCategory = "Status",
                    Description = "Status indicator",
                    DisplayOrder = 60,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                // ID 19
                new MetricUnit
                {
                    UnitCode = "VERSION",
                    UnitName = "Version",
                    UnitSymbol = "",
                    FormatPattern = "v{0}",
                    SuggestedAggregation = "LAST_VALUE",
                    UnitCategory = "Status",
                    Description = "Version number",
                    DisplayOrder = 61,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // OTHER UNITS (ID 20)
                // ============================================================
                // ID 20
                new MetricUnit
                {
                    UnitCode = "NONE",
                    UnitName = "None",
                    UnitSymbol = "",
                    FormatPattern = "{0}",
                    SuggestedAggregation = null,
                    UnitCategory = "Other",
                    Description = "No specific unit",
                    DisplayOrder = 99,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.MetricUnits.AddRange(units);
            context.SaveChanges();
        }
    }
}

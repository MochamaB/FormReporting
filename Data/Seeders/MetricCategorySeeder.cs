using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Data.Seeders
{
    /// <summary>
    /// Seeds predefined metric categories based on metric behavior types
    /// Categories define HOW metrics behave: Score, Performance, Trend, Comparison, Time, Count
    /// </summary>
    public static class MetricCategorySeeder
    {
        public static void SeedMetricCategories(ApplicationDbContext context)
        {
            // Skip if categories already exist
            if (context.MetricCategories.Any())
                return;

            var categories = new List<MetricCategory>
            {
                // SCORE - Metrics that measure quality, satisfaction, or ratings
                new MetricCategory
                {
                    CategoryCode = "SCORE",
                    CategoryName = "Score",
                    Description = "Quality, satisfaction, and rating metrics. Used for assessments, evaluations, and graded measurements.",
                    IconClass = "ri-star-line",
                    ColorHint = "#FFB400",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // PERFORMANCE - Metrics that measure efficiency, rates, and percentages
                new MetricCategory
                {
                    CategoryCode = "PERFORMANCE",
                    CategoryName = "Performance",
                    Description = "Efficiency, utilization, and rate metrics. Used for availability, uptime, and performance indicators.",
                    IconClass = "ri-speed-line",
                    ColorHint = "#4CAF50",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // TREND - Metrics that track changes over time
                new MetricCategory
                {
                    CategoryCode = "TREND",
                    CategoryName = "Trend",
                    Description = "Growth, change, and progression metrics. Used for tracking increases, decreases, and patterns over time.",
                    IconClass = "ri-line-chart-line",
                    ColorHint = "#2196F3",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // COMPARISON - Metrics that compare values or calculate ratios
                new MetricCategory
                {
                    CategoryCode = "COMPARISON",
                    CategoryName = "Comparison",
                    Description = "Ratio, variance, and comparative metrics. Used for benchmarking and relative measurements.",
                    IconClass = "ri-scales-3-line",
                    ColorHint = "#9C27B0",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // TIME - Metrics that measure duration or time-based values
                new MetricCategory
                {
                    CategoryCode = "TIME",
                    CategoryName = "Time",
                    Description = "Duration, response time, and time-based metrics. Used for measuring how long things take.",
                    IconClass = "ri-time-line",
                    ColorHint = "#FF5722",
                    DisplayOrder = 5,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // COUNT - Metrics that count occurrences or quantities
                new MetricCategory
                {
                    CategoryCode = "COUNT",
                    CategoryName = "Count",
                    Description = "Quantity, frequency, and occurrence metrics. Used for counting items, events, or instances.",
                    IconClass = "ri-hashtag",
                    ColorHint = "#607D8B",
                    DisplayOrder = 6,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // COMPLIANCE - Metrics that track yes/no compliance status
                new MetricCategory
                {
                    CategoryCode = "COMPLIANCE",
                    CategoryName = "Compliance",
                    Description = "Binary compliance and adherence metrics. Used for pass/fail, yes/no, and compliance tracking.",
                    IconClass = "ri-checkbox-circle-line",
                    ColorHint = "#00BCD4",
                    DisplayOrder = 7,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // FINANCIAL - Metrics that track monetary values
                new MetricCategory
                {
                    CategoryCode = "FINANCIAL",
                    CategoryName = "Financial",
                    Description = "Cost, budget, and monetary metrics. Used for financial tracking and cost analysis.",
                    IconClass = "ri-money-dollar-circle-line",
                    ColorHint = "#8BC34A",
                    DisplayOrder = 8,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // FORM_ANALYTICS - Form-specific metrics
                new MetricCategory
                {
                    CategoryCode = "FORM_ANALYTICS",
                    CategoryName = "Form Analytics",
                    Description = "Form completion, abandonment, and user interaction metrics. Used for form performance analysis.",
                    IconClass = "ri-file-chart-line",
                    ColorHint = "#9C27B0",
                    DisplayOrder = 9,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // TEXT_ANALYTICS - Text analysis metrics
                new MetricCategory
                {
                    CategoryCode = "TEXT_ANALYTICS",
                    CategoryName = "Text Analytics",
                    Description = "Text sentiment, keyword frequency, and content analysis metrics. Used for text field analysis.",
                    IconClass = "ri-text",
                    ColorHint = "#E91E63",
                    DisplayOrder = 10,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ADVANCED_ANALYTICS - Advanced analytical metrics
                new MetricCategory
                {
                    CategoryCode = "ADVANCED_ANALYTICS",
                    CategoryName = "Advanced Analytics",
                    Description = "Correlation, predictive, and complex analytical metrics. Used for advanced data analysis.",
                    IconClass = "ri-brain-line",
                    ColorHint = "#607D8B",
                    DisplayOrder = 11,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.MetricCategories.AddRange(categories);
            context.SaveChanges();
        }
    }
}

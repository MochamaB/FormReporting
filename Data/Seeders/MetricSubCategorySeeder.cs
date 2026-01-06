using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Data.Seeders
{
    /// <summary>
    /// Seeds predefined metric subcategories with their constraints.
    /// SubCategories define specific metric types within each category and constrain:
    /// - Allowed DataTypes
    /// - Allowed AggregationTypes
    /// - Allowed Scopes (Field, Section, Template)
    /// - Default values
    /// - Suggested thresholds
    ///
    /// SubCategory IDs (in seeding order):
    /// ─────────────────────────────────────────────────────────────────────
    /// SCORE (1-6):        1=SATISFACTION_SCORE, 2=QUALITY_SCORE, 3=RATING_SCORE,
    ///                     4=WEIGHTED_SCORE, 5=COMPOSITE_SCORE, 6=THRESHOLD_SCORE
    /// PERFORMANCE (7-11): 7=AVAILABILITY_RATE, 8=EFFICIENCY_RATE, 9=UTILIZATION_RATE,
    ///                     10=SUCCESS_RATE, 11=COMPLETION_RATE
    /// TREND (12-15):      12=GROWTH_RATE, 13=CHANGE_RATE, 14=PERIOD_COMPARISON, 15=MOVING_AVERAGE
    /// COMPARISON (16-20): 16=RATIO, 17=VARIANCE, 18=BENCHMARK, 19=EXPECTED_VALUE, 20=TARGET_VS_ACTUAL
    /// TIME (21-26):       21=DURATION, 22=RESPONSE_TIME, 23=LEAD_TIME, 24=DOWNTIME,
    ///                     25=TIME_IN_STATE, 26=AGE
    /// COUNT (27-32):      27=QUANTITY, 28=FREQUENCY, 29=INCIDENTS, 30=CAPACITY,
    ///                     31=SELECTION_COUNT, 32=UNIQUE_COUNT
    /// COMPLIANCE (33-36): 33=COMPLIANCE_RATE, 34=AUDIT_SCORE, 35=SLA_COMPLIANCE, 36=POLICY_COMPLIANCE
    /// FINANCIAL (37-40):  37=COST, 38=BUDGET, 39=BUDGET_VARIANCE, 40=ROI
    /// </summary>
    public static class MetricSubCategorySeeder
    {
        // Unit IDs from MetricUnitSeeder (must match)
        private const int UNIT_COUNT = 1;
        private const int UNIT_ITEMS = 2;
        private const int UNIT_UNITS = 3;
        private const int UNIT_PERCENT = 4;
        private const int UNIT_RATIO = 5;
        private const int UNIT_SCORE = 6;
        private const int UNIT_POINTS = 7;
        private const int UNIT_RATING = 8;
        private const int UNIT_DAYS = 9;
        private const int UNIT_HOURS = 10;
        private const int UNIT_MINUTES = 11;
        private const int UNIT_SECONDS = 12;
        private const int UNIT_GB = 13;
        private const int UNIT_MB = 14;
        private const int UNIT_TB = 15;
        private const int UNIT_KES = 16;
        private const int UNIT_USD = 17;
        private const int UNIT_STATUS = 18;
        private const int UNIT_VERSION = 19;
        private const int UNIT_NONE = 20;

        public static void SeedMetricSubCategories(ApplicationDbContext context)
        {
            // Skip if subcategories already exist
            if (context.MetricSubCategories.Any())
                return;

            var subCategories = new List<MetricSubCategory>
            {
                // ============================================================
                // SCORE CATEGORY (CategoryId = 1)
                // Metrics that produce numerical or weighted scores
                // ============================================================

                // ID: 1 - Aggregates rating fields in a section
                new MetricSubCategory
                {
                    CategoryId = 1,
                    SubCategoryCode = "SUBMISSION_SCORE",
                    SubCategoryName = "Satisfaction Score",
                    Description = "Measures scores by aggregating fields or sections or |Template. Best used at section level to combine multiple fields.",
                    DisplayOrder = 1,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,LAST_VALUE,MIN,MAX",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 80m,
                    SuggestedThresholdYellow = 60m,
                    SuggestedThresholdRed = 40m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 2 - Quality assessments
                new MetricSubCategory
                {
                    CategoryId = 1,
                    SubCategoryCode = "QUALITY_SCORE",
                    SubCategoryName = "Quality Score",
                    Description = "Measures quality levels of products, services, or processes. Typically aggregates quality-related fields in a section.",
                    DisplayOrder = 2,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,MIN,MAX,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 90m,
                    SuggestedThresholdYellow = 70m,
                    SuggestedThresholdRed = 50m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 3 - Single field rating (1-5 stars, 1-10 scale)
                new MetricSubCategory
                {
                    CategoryId = 1,
                    SubCategoryCode = "RATING_SCORE",
                    SubCategoryName = "Rating Score",
                    Description = "Scale-based ratings from a single field (e.g., 1-5 stars, 1-10 scale). Used for individual rating fields.",
                    DisplayOrder = 3,
                    IsActive = true,
                    AllowedDataTypes = "Decimal,Integer",
                    AllowedAggregationTypes = "AVG,MIN,MAX,LAST_VALUE",
                    AllowedScopes = "Field",
                    DefaultScope = "Field",
                    DefaultDataType = "Decimal",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_RATING,
                    SuggestedThresholdGreen = 4m,
                    SuggestedThresholdYellow = 3m,
                    SuggestedThresholdRed = 2m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 4 - NEW: Weighted combination of fields
                new MetricSubCategory
                {
                    CategoryId = 1,
                    SubCategoryCode = "WEIGHTED_SCORE",
                    SubCategoryName = "Weighted Score",
                    Description = "Combines multiple fields using assigned weights. Each field's ScoreWeight determines its contribution to the total.",
                    DisplayOrder = 4,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,SUM,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 80m,
                    SuggestedThresholdYellow = 60m,
                    SuggestedThresholdRed = 40m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 5 - NEW: Multi-field composite calculation
                new MetricSubCategory
                {
                    CategoryId = 1,
                    SubCategoryCode = "COMPOSITE_SCORE",
                    SubCategoryName = "Composite Score",
                    Description = "Creates a single metric from multiple fields using custom calculation logic (e.g., Wellness Score from 5 health questions).",
                    DisplayOrder = 5,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,SUM,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_SCORE,
                    SuggestedThresholdGreen = 80m,
                    SuggestedThresholdYellow = 60m,
                    SuggestedThresholdRed = 40m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 6 - NEW: Pass/fail threshold scoring
                new MetricSubCategory
                {
                    CategoryId = 1,
                    SubCategoryCode = "THRESHOLD_SCORE",
                    SubCategoryName = "Threshold Score",
                    Description = "Pass/fail scoring based on whether values meet defined thresholds. Returns 100% if passed, 0% if failed.",
                    DisplayOrder = 6,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Boolean",
                    AllowedAggregationTypes = "AVG,LAST_VALUE,COUNT",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 100m,
                    SuggestedThresholdYellow = 80m,
                    SuggestedThresholdRed = 60m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 7 - NEW: Risk assessment scoring
                new MetricSubCategory
                {
                    CategoryId = 1,
                    SubCategoryCode = "RISK_SCORE",
                    SubCategoryName = "Risk Score",
                    Description = "Risk assessment metrics that evaluate potential risks and vulnerabilities.",
                    DisplayOrder = 7,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,MAX,LAST_VALUE",
                    AllowedScopes = "Field,Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 20m,
                    SuggestedThresholdYellow = 50m,
                    SuggestedThresholdRed = 80m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 8 - NEW: Health assessment scoring
                new MetricSubCategory
                {
                    CategoryId = 1,
                    SubCategoryCode = "HEALTH_SCORE",
                    SubCategoryName = "Health Score",
                    Description = "System or service health indicators that measure overall wellness and performance.",
                    DisplayOrder = 8,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,MIN,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Template",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 80m,
                    SuggestedThresholdYellow = 60m,
                    SuggestedThresholdRed = 40m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 9 - NEW: Normalized scoring (0-100)
                new MetricSubCategory
                {
                    CategoryId = 1,
                    SubCategoryCode = "NORMALIZED_SCORE",
                    SubCategoryName = "Normalized Score",
                    Description = "Scores normalized to 0-100 scale for consistent comparison across different metrics.",
                    DisplayOrder = 9,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MIN,MAX,LAST_VALUE",
                    AllowedScopes = "Field,Section,Template",
                    DefaultScope = "Field",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 80m,
                    SuggestedThresholdYellow = 60m,
                    SuggestedThresholdRed = 40m,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // PERFORMANCE CATEGORY (CategoryId = 2)
                // Metrics measuring how well something performs
                // ============================================================

                // ID: 7 - System/service uptime
                new MetricSubCategory
                {
                    CategoryId = 2,
                    SubCategoryCode = "AVAILABILITY_RATE",
                    SubCategoryName = "Availability Rate",
                    Description = "Measures uptime and availability of systems, services, or resources. Can be a single field or aggregated.",
                    DisplayOrder = 1,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MIN,LAST_VALUE",
                    AllowedScopes = "Field,Section,Template",
                    DefaultScope = "Field",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 99m,
                    SuggestedThresholdYellow = 95m,
                    SuggestedThresholdRed = 90m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 8 - Resource efficiency
                new MetricSubCategory
                {
                    CategoryId = 2,
                    SubCategoryCode = "EFFICIENCY_RATE",
                    SubCategoryName = "Efficiency Rate",
                    Description = "Measures how efficiently resources are used (output vs input). Usually calculated from multiple inputs.",
                    DisplayOrder = 2,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MIN,MAX,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 85m,
                    SuggestedThresholdYellow = 70m,
                    SuggestedThresholdRed = 50m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 9 - Capacity usage
                new MetricSubCategory
                {
                    CategoryId = 2,
                    SubCategoryCode = "UTILIZATION_RATE",
                    SubCategoryName = "Utilization Rate",
                    Description = "Measures how much of available capacity is being used. Often from a single capacity field.",
                    DisplayOrder = 3,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MAX,LAST_VALUE",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 80m,
                    SuggestedThresholdYellow = 60m,
                    SuggestedThresholdRed = 40m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 10 - Success vs failures
                new MetricSubCategory
                {
                    CategoryId = 2,
                    SubCategoryCode = "SUCCESS_RATE",
                    SubCategoryName = "Success Rate",
                    Description = "Measures the percentage of successful outcomes vs total attempts. Aggregates success/fail across items.",
                    DisplayOrder = 4,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MIN,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 95m,
                    SuggestedThresholdYellow = 80m,
                    SuggestedThresholdRed = 60m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 11 - NEW: Form/section completion
                new MetricSubCategory
                {
                    CategoryId = 2,
                    SubCategoryCode = "COMPLETION_RATE",
                    SubCategoryName = "Completion Rate",
                    Description = "Percentage of fields completed in a section or form. Tracks form abandonment and field skip rates.",
                    DisplayOrder = 5,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MIN,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 100m,
                    SuggestedThresholdYellow = 80m,
                    SuggestedThresholdRed = 50m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 12 - NEW: System reliability metrics
                new MetricSubCategory
                {
                    CategoryId = 2,
                    SubCategoryCode = "RELIABILITY",
                    SubCategoryName = "Reliability",
                    Description = "System reliability index measuring consistent performance over time.",
                    DisplayOrder = 6,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MIN,LAST_VALUE",
                    AllowedScopes = "Field,Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 95m,
                    SuggestedThresholdYellow = 85m,
                    SuggestedThresholdRed = 70m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 13 - NEW: System stability metrics
                new MetricSubCategory
                {
                    CategoryId = 2,
                    SubCategoryCode = "STABILITY",
                    SubCategoryName = "Stability",
                    Description = "System stability indicators measuring consistent behavior and minimal fluctuations.",
                    DisplayOrder = 7,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MIN,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 90m,
                    SuggestedThresholdYellow = 75m,
                    SuggestedThresholdRed = 60m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 14 - NEW: Failure rate metrics
                new MetricSubCategory
                {
                    CategoryId = 2,
                    SubCategoryCode = "FAILURE_RATE",
                    SubCategoryName = "Failure Rate",
                    Description = "Failure frequency metrics measuring the rate of failures or errors.",
                    DisplayOrder = 8,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MAX,LAST_VALUE",
                    AllowedScopes = "Field,Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 5m,
                    SuggestedThresholdYellow = 15m,
                    SuggestedThresholdRed = 30m,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // TREND CATEGORY (CategoryId = 3)
                // Time-series and progression metrics
                // ============================================================

                // ID: 12 - Growth measurement
                new MetricSubCategory
                {
                    CategoryId = 3,
                    SubCategoryCode = "GROWTH_RATE",
                    SubCategoryName = "Growth Rate",
                    Description = "Measures percentage increase or decrease over time. Template-level for form-wide trends.",
                    DisplayOrder = 1,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,LAST_VALUE,SUM",
                    AllowedScopes = "Template",
                    DefaultScope = "Template",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 10m,
                    SuggestedThresholdYellow = 0m,
                    SuggestedThresholdRed = -10m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 13 - Rate of change
                new MetricSubCategory
                {
                    CategoryId = 3,
                    SubCategoryCode = "CHANGE_RATE",
                    SubCategoryName = "Change Rate",
                    Description = "Measures the rate of change between periods. Tracks answer shift and submission volume changes.",
                    DisplayOrder = 2,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,LAST_VALUE,SUM",
                    AllowedScopes = "Template",
                    DefaultScope = "Template",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 14 - NEW: Period-over-period comparison
                new MetricSubCategory
                {
                    CategoryId = 3,
                    SubCategoryCode = "PERIOD_COMPARISON",
                    SubCategoryName = "Period Comparison",
                    Description = "Compares current period values to previous period (month-over-month, year-over-year).",
                    DisplayOrder = 3,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,LAST_VALUE",
                    AllowedScopes = "Template",
                    DefaultScope = "Template",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 5m,
                    SuggestedThresholdYellow = 0m,
                    SuggestedThresholdRed = -5m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 15 - NEW: Rolling average
                new MetricSubCategory
                {
                    CategoryId = 3,
                    SubCategoryCode = "MOVING_AVERAGE",
                    SubCategoryName = "Moving Average",
                    Description = "Rolling average across submissions over a defined period (e.g., 7-day, 30-day moving average).",
                    DisplayOrder = 4,
                    IsActive = true,
                    AllowedDataTypes = "Decimal,Percentage",
                    AllowedAggregationTypes = "AVG,LAST_VALUE",
                    AllowedScopes = "Template",
                    DefaultScope = "Template",
                    DefaultDataType = "Decimal",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_NONE,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // COMPARISON CATEGORY (CategoryId = 4)
                // Metrics comparing against targets, benchmarks, or expected values
                // ============================================================

                // ID: 16 - Ratio comparison
                new MetricSubCategory
                {
                    CategoryId = 4,
                    SubCategoryCode = "RATIO",
                    SubCategoryName = "Ratio",
                    Description = "Compares two quantities as a ratio (e.g., 2:1, 0.5). Can compare two fields or sections.",
                    DisplayOrder = 1,
                    IsActive = true,
                    AllowedDataTypes = "Decimal,Integer",
                    AllowedAggregationTypes = "AVG,LAST_VALUE,MIN,MAX",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Decimal",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_RATIO,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 17 - Actual vs expected variance
                new MetricSubCategory
                {
                    CategoryId = 4,
                    SubCategoryCode = "VARIANCE",
                    SubCategoryName = "Variance",
                    Description = "Measures the difference between actual and expected values. Works at any level.",
                    DisplayOrder = 2,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,LAST_VALUE,SUM",
                    AllowedScopes = "Field,Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 5m,
                    SuggestedThresholdYellow = 15m,
                    SuggestedThresholdRed = 25m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 18 - Standard/target comparison
                new MetricSubCategory
                {
                    CategoryId = 4,
                    SubCategoryCode = "BENCHMARK",
                    SubCategoryName = "Benchmark",
                    Description = "Compares performance against a standard, target, or industry benchmark.",
                    DisplayOrder = 3,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 100m,
                    SuggestedThresholdYellow = 80m,
                    SuggestedThresholdRed = 60m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 19 - NEW: Expected value comparison (replaces BINARY_COMPLIANCE)
                new MetricSubCategory
                {
                    CategoryId = 4,
                    SubCategoryCode = "EXPECTED_VALUE",
                    SubCategoryName = "Expected Value",
                    Description = "Compares a single field value against an expected value using operators (Equals, GreaterThan, LessThan, Contains). Returns 100% if matched, 0% if not.",
                    DisplayOrder = 4,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Boolean",
                    AllowedAggregationTypes = "AVG,LAST_VALUE,COUNT",
                    AllowedScopes = "Field",
                    DefaultScope = "Field",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 100m,
                    SuggestedThresholdYellow = 100m,
                    SuggestedThresholdRed = 0m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 20 - NEW: Target vs actual comparison
                new MetricSubCategory
                {
                    CategoryId = 4,
                    SubCategoryCode = "TARGET_VS_ACTUAL",
                    SubCategoryName = "Target vs Actual",
                    Description = "Compares a field's actual value to its defined target. Shows percentage of target achieved.",
                    DisplayOrder = 5,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,LAST_VALUE",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 100m,
                    SuggestedThresholdYellow = 80m,
                    SuggestedThresholdRed = 60m,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // TIME CATEGORY (CategoryId = 5)
                // Duration and temporal behavior metrics
                // ============================================================

                // ID: 21 - Elapsed time
                new MetricSubCategory
                {
                    CategoryId = 5,
                    SubCategoryCode = "DURATION",
                    SubCategoryName = "Duration",
                    Description = "Measures how long something takes (total elapsed time). Can be single field or aggregated.",
                    DisplayOrder = 1,
                    IsActive = true,
                    AllowedDataTypes = "Duration,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,SUM,MAX,MIN",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Duration",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_HOURS,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 22 - Response time
                new MetricSubCategory
                {
                    CategoryId = 5,
                    SubCategoryCode = "RESPONSE_TIME",
                    SubCategoryName = "Response Time",
                    Description = "Measures time to respond to a request or event. Single field measurement.",
                    DisplayOrder = 2,
                    IsActive = true,
                    AllowedDataTypes = "Duration,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,MAX,MIN,LAST_VALUE",
                    AllowedScopes = "Field",
                    DefaultScope = "Field",
                    DefaultDataType = "Duration",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_MINUTES,
                    SuggestedThresholdGreen = 5m,
                    SuggestedThresholdYellow = 15m,
                    SuggestedThresholdRed = 30m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 23 - Process lead time
                new MetricSubCategory
                {
                    CategoryId = 5,
                    SubCategoryCode = "LEAD_TIME",
                    SubCategoryName = "Lead Time",
                    Description = "Measures time from start to completion of a process. End-to-end measurement.",
                    DisplayOrder = 3,
                    IsActive = true,
                    AllowedDataTypes = "Duration,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,MAX,MIN",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Duration",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_DAYS,
                    SuggestedThresholdGreen = 3m,
                    SuggestedThresholdYellow = 7m,
                    SuggestedThresholdRed = 14m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 24 - Downtime measurement
                new MetricSubCategory
                {
                    CategoryId = 5,
                    SubCategoryCode = "DOWNTIME",
                    SubCategoryName = "Downtime",
                    Description = "Measures time when a system or service is unavailable.",
                    DisplayOrder = 4,
                    IsActive = true,
                    AllowedDataTypes = "Duration,Decimal,Integer",
                    AllowedAggregationTypes = "SUM,AVG,MAX",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Duration",
                    DefaultAggregationType = "SUM",
                    DefaultUnitId = UNIT_HOURS,
                    SuggestedThresholdGreen = 1m,
                    SuggestedThresholdYellow = 4m,
                    SuggestedThresholdRed = 8m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 25 - NEW: Time in a particular state
                new MetricSubCategory
                {
                    CategoryId = 5,
                    SubCategoryCode = "TIME_IN_STATE",
                    SubCategoryName = "Time In State",
                    Description = "Measures how long something remained in a particular state (e.g., time in 'Pending' status).",
                    DisplayOrder = 5,
                    IsActive = true,
                    AllowedDataTypes = "Duration,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,SUM,MAX,MIN",
                    AllowedScopes = "Field",
                    DefaultScope = "Field",
                    DefaultDataType = "Duration",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_HOURS,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 26 - NEW: Age since a date
                new MetricSubCategory
                {
                    CategoryId = 5,
                    SubCategoryCode = "AGE",
                    SubCategoryName = "Age",
                    Description = "Calculates days/time elapsed since a date field value (e.g., asset age, days since last review).",
                    DisplayOrder = 6,
                    IsActive = true,
                    AllowedDataTypes = "Duration,Integer",
                    AllowedAggregationTypes = "AVG,MAX,MIN,LAST_VALUE",
                    AllowedScopes = "Field",
                    DefaultScope = "Field",
                    DefaultDataType = "Integer",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_DAYS,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // COUNT CATEGORY (CategoryId = 6)
                // Frequency and quantity metrics
                // ============================================================

                // ID: 27 - Total count
                new MetricSubCategory
                {
                    CategoryId = 6,
                    SubCategoryCode = "QUANTITY",
                    SubCategoryName = "Quantity",
                    Description = "Counts the total number of items or resources. Can be single field or sum across section.",
                    DisplayOrder = 1,
                    IsActive = true,
                    AllowedDataTypes = "Integer,Decimal",
                    AllowedAggregationTypes = "SUM,AVG,LAST_VALUE,COUNT",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Integer",
                    DefaultAggregationType = "SUM",
                    DefaultUnitId = UNIT_COUNT,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 28 - Occurrence frequency
                new MetricSubCategory
                {
                    CategoryId = 6,
                    SubCategoryCode = "FREQUENCY",
                    SubCategoryName = "Frequency",
                    Description = "Counts how often something occurs within a period.",
                    DisplayOrder = 2,
                    IsActive = true,
                    AllowedDataTypes = "Integer,Decimal",
                    AllowedAggregationTypes = "SUM,AVG,MAX",
                    AllowedScopes = "Field,Section,Template",
                    DefaultScope = "Field",
                    DefaultDataType = "Integer",
                    DefaultAggregationType = "SUM",
                    DefaultUnitId = UNIT_COUNT,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 29 - Incident count
                new MetricSubCategory
                {
                    CategoryId = 6,
                    SubCategoryCode = "INCIDENTS",
                    SubCategoryName = "Incidents",
                    Description = "Counts the number of incidents, issues, or problems.",
                    DisplayOrder = 3,
                    IsActive = true,
                    AllowedDataTypes = "Integer",
                    AllowedAggregationTypes = "SUM,AVG,MAX,COUNT",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Integer",
                    DefaultAggregationType = "SUM",
                    DefaultUnitId = UNIT_COUNT,
                    SuggestedThresholdGreen = 0m,
                    SuggestedThresholdYellow = 5m,
                    SuggestedThresholdRed = 10m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 30 - Capacity amount
                new MetricSubCategory
                {
                    CategoryId = 6,
                    SubCategoryCode = "CAPACITY",
                    SubCategoryName = "Capacity",
                    Description = "Measures total capacity or available slots/resources. Single field measurement.",
                    DisplayOrder = 4,
                    IsActive = true,
                    AllowedDataTypes = "Integer,Decimal",
                    AllowedAggregationTypes = "SUM,AVG,LAST_VALUE",
                    AllowedScopes = "Field",
                    DefaultScope = "Field",
                    DefaultDataType = "Integer",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_UNITS,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 31 - NEW: Selection option count
                new MetricSubCategory
                {
                    CategoryId = 6,
                    SubCategoryCode = "SELECTION_COUNT",
                    SubCategoryName = "Selection Count",
                    Description = "Counts how many times a specific option was selected (for radio, dropdown, checkbox fields). Used for option frequency analysis.",
                    DisplayOrder = 5,
                    IsActive = true,
                    AllowedDataTypes = "Integer,Percentage",
                    AllowedAggregationTypes = "SUM,AVG,COUNT",
                    AllowedScopes = "Field",
                    DefaultScope = "Field",
                    DefaultDataType = "Integer",
                    DefaultAggregationType = "SUM",
                    DefaultUnitId = UNIT_COUNT,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 32 - NEW: Distinct value count
                new MetricSubCategory
                {
                    CategoryId = 6,
                    SubCategoryCode = "UNIQUE_COUNT",
                    SubCategoryName = "Unique Count",
                    Description = "Counts distinct/unique values across fields in a section or form.",
                    DisplayOrder = 6,
                    IsActive = true,
                    AllowedDataTypes = "Integer",
                    AllowedAggregationTypes = "COUNT,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Integer",
                    DefaultAggregationType = "COUNT",
                    DefaultUnitId = UNIT_COUNT,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // COMPLIANCE CATEGORY (CategoryId = 7)
                // Policy, standard, and SLA alignment metrics
                // NOTE: BINARY_COMPLIANCE removed - use EXPECTED_VALUE in Comparison
                // ============================================================

                // ID: 33 - Compliance rate
                new MetricSubCategory
                {
                    CategoryId = 7,
                    SubCategoryCode = "COMPLIANCE_RATE",
                    SubCategoryName = "Compliance Rate",
                    Description = "Percentage of items/checks that are compliant. Aggregates compliance status across section.",
                    DisplayOrder = 1,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MIN,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 100m,
                    SuggestedThresholdYellow = 90m,
                    SuggestedThresholdRed = 70m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 34 - Audit score
                new MetricSubCategory
                {
                    CategoryId = 7,
                    SubCategoryCode = "AUDIT_SCORE",
                    SubCategoryName = "Audit Score",
                    Description = "Score from compliance audits or assessments. Typically form-wide.",
                    DisplayOrder = 2,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal,Integer",
                    AllowedAggregationTypes = "AVG,MIN,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Template",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 90m,
                    SuggestedThresholdYellow = 75m,
                    SuggestedThresholdRed = 50m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 35 - NEW: SLA compliance
                new MetricSubCategory
                {
                    CategoryId = 7,
                    SubCategoryCode = "SLA_COMPLIANCE",
                    SubCategoryName = "SLA Compliance",
                    Description = "Measures adherence to Service Level Agreement targets. Compares actual vs SLA target.",
                    DisplayOrder = 3,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Boolean",
                    AllowedAggregationTypes = "AVG,MIN,LAST_VALUE",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 100m,
                    SuggestedThresholdYellow = 95m,
                    SuggestedThresholdRed = 90m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 36 - NEW: Policy compliance
                new MetricSubCategory
                {
                    CategoryId = 7,
                    SubCategoryCode = "POLICY_COMPLIANCE",
                    SubCategoryName = "Policy Compliance",
                    Description = "Tracks adherence to organizational policies. Aggregates policy checks in section.",
                    DisplayOrder = 4,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MIN,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 100m,
                    SuggestedThresholdYellow = 90m,
                    SuggestedThresholdRed = 70m,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // FINANCIAL CATEGORY (CategoryId = 8)
                // Cost, budget, and financial metrics
                // ============================================================

                // ID: 37 - Cost tracking
                new MetricSubCategory
                {
                    CategoryId = 8,
                    SubCategoryCode = "COST",
                    SubCategoryName = "Cost",
                    Description = "Tracks expenses and costs. Can be single cost field or sum across section.",
                    DisplayOrder = 1,
                    IsActive = true,
                    AllowedDataTypes = "Decimal,Integer",
                    AllowedAggregationTypes = "SUM,AVG,MAX,LAST_VALUE",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Decimal",
                    DefaultAggregationType = "SUM",
                    DefaultUnitId = UNIT_KES,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 38 - Budget amounts
                new MetricSubCategory
                {
                    CategoryId = 8,
                    SubCategoryCode = "BUDGET",
                    SubCategoryName = "Budget",
                    Description = "Tracks budget amounts and allocations.",
                    DisplayOrder = 2,
                    IsActive = true,
                    AllowedDataTypes = "Decimal,Integer",
                    AllowedAggregationTypes = "SUM,LAST_VALUE",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Decimal",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_KES,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 39 - Budget variance
                new MetricSubCategory
                {
                    CategoryId = 8,
                    SubCategoryCode = "BUDGET_VARIANCE",
                    SubCategoryName = "Budget Variance",
                    Description = "Measures difference between budgeted and actual amounts. Section or template level.",
                    DisplayOrder = 3,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,LAST_VALUE,SUM",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 5m,
                    SuggestedThresholdYellow = 15m,
                    SuggestedThresholdRed = 25m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 40 - Return on investment
                new MetricSubCategory
                {
                    CategoryId = 8,
                    SubCategoryCode = "ROI",
                    SubCategoryName = "Return on Investment",
                    Description = "Measures return relative to investment cost. Template-level form-wide metric.",
                    DisplayOrder = 4,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,LAST_VALUE",
                    AllowedScopes = "Template",
                    DefaultScope = "Template",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 20m,
                    SuggestedThresholdYellow = 10m,
                    SuggestedThresholdRed = 0m,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // FORM_ANALYTICS CATEGORY (CategoryId = 9)
                // Form-specific metrics for completion, abandonment, and user interaction
                // ============================================================

                // ID: 41 - Form completion tracking
                new MetricSubCategory
                {
                    CategoryId = 9,
                    SubCategoryCode = "FORM_COMPLETION_RATE",
                    SubCategoryName = "Form Completion Rate",
                    Description = "Overall form completion percentage tracking user drop-off and abandonment points.",
                    DisplayOrder = 1,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MIN,LAST_VALUE",
                    AllowedScopes = "Template",
                    DefaultScope = "Template",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 90m,
                    SuggestedThresholdYellow = 70m,
                    SuggestedThresholdRed = 50m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 42 - Form abandonment tracking
                new MetricSubCategory
                {
                    CategoryId = 9,
                    SubCategoryCode = "FORM_ABANDONMENT_RATE",
                    SubCategoryName = "Form Abandonment Rate",
                    Description = "Percentage of users who start but don't complete the form, identifying drop-off points.",
                    DisplayOrder = 2,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MAX,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Template",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 10m,
                    SuggestedThresholdYellow = 30m,
                    SuggestedThresholdRed = 50m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 43 - Field skip rate tracking
                new MetricSubCategory
                {
                    CategoryId = 9,
                    SubCategoryCode = "FIELD_SKIP_RATE",
                    SubCategoryName = "Field Skip Rate",
                    Description = "Percentage of users who skip individual fields, identifying problematic questions.",
                    DisplayOrder = 3,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,MAX,LAST_VALUE",
                    AllowedScopes = "Field",
                    DefaultScope = "Field",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 5m,
                    SuggestedThresholdYellow = 15m,
                    SuggestedThresholdRed = 30m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 44 - Form completion time
                new MetricSubCategory
                {
                    CategoryId = 9,
                    SubCategoryCode = "TIME_TO_COMPLETE",
                    SubCategoryName = "Time to Complete",
                    Description = "Average, median, and distribution of time spent completing the entire form.",
                    DisplayOrder = 4,
                    IsActive = true,
                    AllowedDataTypes = "Decimal,Integer",
                    AllowedAggregationTypes = "AVG,MIN,MAX,LAST_VALUE",
                    AllowedScopes = "Template",
                    DefaultScope = "Template",
                    DefaultDataType = "Decimal",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_MINUTES,
                    SuggestedThresholdGreen = 10m,
                    SuggestedThresholdYellow = 20m,
                    SuggestedThresholdRed = 30m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 45 - Submission volume tracking
                new MetricSubCategory
                {
                    CategoryId = 9,
                    SubCategoryCode = "SUBMISSION_VOLUME",
                    SubCategoryName = "Submission Volume",
                    Description = "Number of form submissions per time period for volume analysis.",
                    DisplayOrder = 5,
                    IsActive = true,
                    AllowedDataTypes = "Integer,Count",
                    AllowedAggregationTypes = "SUM,COUNT,AVG,LAST_VALUE",
                    AllowedScopes = "Template",
                    DefaultScope = "Template",
                    DefaultDataType = "Integer",
                    DefaultAggregationType = "SUM",
                    DefaultUnitId = UNIT_COUNT,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // TEXT_ANALYTICS CATEGORY (CategoryId = 10)
                // Text analysis and content metrics
                // ============================================================

                // ID: 46 - Text sentiment analysis
                new MetricSubCategory
                {
                    CategoryId = 10,
                    SubCategoryCode = "SENTIMENT_SCORE",
                    SubCategoryName = "Sentiment Score",
                    Description = "Positive/negative sentiment analysis of text field responses.",
                    DisplayOrder = 1,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,LAST_VALUE",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 70m,
                    SuggestedThresholdYellow = 50m,
                    SuggestedThresholdRed = 30m,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 47 - Keyword frequency analysis
                new MetricSubCategory
                {
                    CategoryId = 10,
                    SubCategoryCode = "WORD_FREQUENCY",
                    SubCategoryName = "Word Frequency",
                    Description = "Frequency of specific keywords or phrases in text responses.",
                    DisplayOrder = 2,
                    IsActive = true,
                    AllowedDataTypes = "Integer,Count",
                    AllowedAggregationTypes = "SUM,COUNT,AVG",
                    AllowedScopes = "Field,Section",
                    DefaultScope = "Field",
                    DefaultDataType = "Integer",
                    DefaultAggregationType = "SUM",
                    DefaultUnitId = UNIT_COUNT,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 48 - Text length analysis
                new MetricSubCategory
                {
                    CategoryId = 10,
                    SubCategoryCode = "TEXT_LENGTH",
                    SubCategoryName = "Text Length",
                    Description = "Character or word count metrics for text field responses.",
                    DisplayOrder = 3,
                    IsActive = true,
                    AllowedDataTypes = "Integer,Count",
                    AllowedAggregationTypes = "AVG,MIN,MAX,SUM",
                    AllowedScopes = "Field",
                    DefaultScope = "Field",
                    DefaultDataType = "Integer",
                    DefaultAggregationType = "AVG",
                    DefaultUnitId = UNIT_COUNT,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================
                // ADVANCED_ANALYTICS CATEGORY (CategoryId = 11)
                // Advanced analytical and predictive metrics
                // ============================================================

                // ID: 49 - Field correlation analysis
                new MetricSubCategory
                {
                    CategoryId = 11,
                    SubCategoryCode = "CORRELATION",
                    SubCategoryName = "Correlation",
                    Description = "Field-to-field relationship analysis measuring how responses correlate.",
                    DisplayOrder = 1,
                    IsActive = true,
                    AllowedDataTypes = "Decimal,Percentage",
                    AllowedAggregationTypes = "AVG,LAST_VALUE",
                    AllowedScopes = "Section,Template",
                    DefaultScope = "Section",
                    DefaultDataType = "Decimal",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_RATIO,
                    SuggestedThresholdGreen = null,
                    SuggestedThresholdYellow = null,
                    SuggestedThresholdRed = null,
                    CreatedDate = DateTime.UtcNow
                },

                // ID: 50 - Predictive indicators
                new MetricSubCategory
                {
                    CategoryId = 11,
                    SubCategoryCode = "PREDICTIVE_INDICATOR",
                    SubCategoryName = "Predictive Indicator",
                    Description = "ML-based predictions identifying which responses predict specific outcomes.",
                    DisplayOrder = 2,
                    IsActive = true,
                    AllowedDataTypes = "Percentage,Decimal",
                    AllowedAggregationTypes = "AVG,LAST_VALUE",
                    AllowedScopes = "Template",
                    DefaultScope = "Template",
                    DefaultDataType = "Percentage",
                    DefaultAggregationType = "LAST_VALUE",
                    DefaultUnitId = UNIT_PERCENT,
                    SuggestedThresholdGreen = 80m,
                    SuggestedThresholdYellow = 60m,
                    SuggestedThresholdRed = 40m,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.MetricSubCategories.AddRange(subCategories);
            context.SaveChanges();
        }
    }
}

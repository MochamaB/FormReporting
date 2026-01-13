using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Data.Seeders
{
    /// <summary>
    /// Seeds the junction table linking subcategories to their allowed units.
    /// This determines which units appear in the dropdown when a user selects a subcategory.
    ///
    /// DEPENDENCIES:
    /// - Must run AFTER MetricUnitSeeder (needs unit IDs)
    /// - Must run AFTER MetricSubCategorySeeder (needs subcategory IDs)
    ///
    /// Unit IDs: 1=COUNT, 2=ITEMS, 3=UNITS, 4=PERCENT, 5=RATIO, 6=SCORE, 7=POINTS, 8=RATING,
    ///           9=DAYS, 10=HOURS, 11=MINUTES, 12=SECONDS, 13=GB, 14=MB, 15=TB,
    ///           16=KES, 17=USD, 18=STATUS, 19=VERSION, 20=NONE
    ///
    /// SubCategory IDs (in seeding order):
    /// ─────────────────────────────────────────────────────────────────────
    /// SCORE (1-9):        1=SATISFACTION_SCORE, 2=QUALITY_SCORE, 3=RATING_SCORE,
    ///                     4=WEIGHTED_SCORE, 5=COMPOSITE_SCORE, 6=THRESHOLD_SCORE,
    ///                     7=RISK_SCORE, 8=HEALTH_SCORE, 9=NORMALIZED_SCORE
    /// PERFORMANCE (10-14): 10=AVAILABILITY_RATE, 11=EFFICIENCY_RATE, 12=UTILIZATION_RATE,
    ///                      13=SUCCESS_RATE, 14=COMPLETION_RATE, 15=RELIABILITY,
    ///                      16=STABILITY, 17=FAILURE_RATE
    /// TREND (18-21):      18=GROWTH_RATE, 19=CHANGE_RATE, 20=PERIOD_COMPARISON, 21=MOVING_AVERAGE
    /// COMPARISON (22-26): 22=RATIO, 23=VARIANCE, 24=BENCHMARK, 25=EXPECTED_VALUE, 26=TARGET_VS_ACTUAL
    /// TIME (27-32):       27=DURATION, 28=RESPONSE_TIME, 29=LEAD_TIME, 30=DOWNTIME,
    ///                     31=TIME_IN_STATE, 32=AGE
    /// COUNT (33-38):      33=QUANTITY, 34=FREQUENCY, 35=INCIDENTS, 36=CAPACITY,
    ///                     37=SELECTION_COUNT, 38=UNIQUE_COUNT
    /// COMPLIANCE (39-42): 39=COMPLIANCE_RATE, 40=AUDIT_SCORE, 41=SLA_COMPLIANCE, 42=POLICY_COMPLIANCE
    /// FINANCIAL (43-46):  43=COST, 44=BUDGET, 45=BUDGET_VARIANCE, 46=ROI
    /// FORM_ANALYTICS (47-51): 47=FORM_COMPLETION_RATE, 48=FORM_ABANDONMENT_RATE, 49=FIELD_SKIP_RATE,
    ///                         50=TIME_TO_COMPLETE, 51=SUBMISSION_VOLUME
    /// TEXT_ANALYTICS (52-54): 52=SENTIMENT_SCORE, 53=WORD_FREQUENCY, 54=TEXT_LENGTH
    /// ADVANCED_ANALYTICS (55-56): 55=CORRELATION, 56=PREDICTIVE_INDICATOR
    /// </summary>
    public static class MetricSubCategoryUnitSeeder
    {
        // Unit IDs
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

        // ============================================================
        // SCORE SubCategory IDs (1-9)
        // ============================================================
        private const int SC_SATISFACTION_SCORE = 1;
        private const int SC_QUALITY_SCORE = 2;
        private const int SC_RATING_SCORE = 3;
        private const int SC_WEIGHTED_SCORE = 4;
        private const int SC_COMPOSITE_SCORE = 5;
        private const int SC_THRESHOLD_SCORE = 6;
        private const int SC_RISK_SCORE = 7;
        private const int SC_HEALTH_SCORE = 8;
        private const int SC_NORMALIZED_SCORE = 9;

        // ============================================================
        // PERFORMANCE SubCategory IDs (10-17)
        // ============================================================
        private const int SC_AVAILABILITY_RATE = 10;
        private const int SC_EFFICIENCY_RATE = 11;
        private const int SC_UTILIZATION_RATE = 12;
        private const int SC_SUCCESS_RATE = 13;
        private const int SC_COMPLETION_RATE = 14;
        private const int SC_RELIABILITY = 15;
        private const int SC_STABILITY = 16;
        private const int SC_FAILURE_RATE = 17;

        // ============================================================
        // TREND SubCategory IDs (18-21)
        // ============================================================
        private const int SC_GROWTH_RATE = 18;
        private const int SC_CHANGE_RATE = 19;
        private const int SC_PERIOD_COMPARISON = 20;
        private const int SC_MOVING_AVERAGE = 21;

        // ============================================================
        // COMPARISON SubCategory IDs (22-26)
        // ============================================================
        private const int SC_RATIO = 22;
        private const int SC_VARIANCE = 23;
        private const int SC_BENCHMARK = 24;
        private const int SC_EXPECTED_VALUE = 25;
        private const int SC_TARGET_VS_ACTUAL = 26;

        // ============================================================
        // TIME SubCategory IDs (27-32)
        // ============================================================
        private const int SC_DURATION = 27;
        private const int SC_RESPONSE_TIME = 28;
        private const int SC_LEAD_TIME = 29;
        private const int SC_DOWNTIME = 30;
        private const int SC_TIME_IN_STATE = 31;
        private const int SC_AGE = 32;

        // ============================================================
        // COUNT SubCategory IDs (33-38)
        // ============================================================
        private const int SC_QUANTITY = 33;
        private const int SC_FREQUENCY = 34;
        private const int SC_INCIDENTS = 35;
        private const int SC_CAPACITY = 36;
        private const int SC_SELECTION_COUNT = 37;
        private const int SC_UNIQUE_COUNT = 38;

        // ============================================================
        // COMPLIANCE SubCategory IDs (39-42)
        // ============================================================
        private const int SC_COMPLIANCE_RATE = 39;
        private const int SC_AUDIT_SCORE = 40;
        private const int SC_SLA_COMPLIANCE = 41;
        private const int SC_POLICY_COMPLIANCE = 42;

        // ============================================================
        // FINANCIAL SubCategory IDs (43-46)
        // ============================================================
        private const int SC_COST = 43;
        private const int SC_BUDGET = 44;
        private const int SC_BUDGET_VARIANCE = 45;
        private const int SC_ROI = 46;

        // ============================================================
        // FORM_ANALYTICS SubCategory IDs (47-51)
        // ============================================================
        private const int SC_FORM_COMPLETION_RATE = 47;
        private const int SC_FORM_ABANDONMENT_RATE = 48;
        private const int SC_FIELD_SKIP_RATE = 49;
        private const int SC_TIME_TO_COMPLETE = 50;
        private const int SC_SUBMISSION_VOLUME = 51;

        // ============================================================
        // TEXT_ANALYTICS SubCategory IDs (52-54)
        // ============================================================
        private const int SC_SENTIMENT_SCORE = 52;
        private const int SC_WORD_FREQUENCY = 53;
        private const int SC_TEXT_LENGTH = 54;

        // ============================================================
        // ADVANCED_ANALYTICS SubCategory IDs (55-56)
        // ============================================================
        private const int SC_CORRELATION = 55;
        private const int SC_PREDICTIVE_INDICATOR = 56;

        public static void SeedMetricSubCategoryUnits(ApplicationDbContext context)
        {
            // Skip if already seeded
            if (context.MetricSubCategoryUnits.Any())
                return;

            var links = new List<MetricSubCategoryUnit>();

            // ============================================================
            // SCORE SUBCATEGORIES (1-6)
            // ============================================================

            // SATISFACTION_SCORE: Percentage, Score, Rating
            links.AddRange(CreateLinks(SC_SATISFACTION_SCORE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_SCORE, false, 2),
                (UNIT_RATING, false, 3)
            }));

            // QUALITY_SCORE: Percentage, Score, Points
            links.AddRange(CreateLinks(SC_QUALITY_SCORE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_SCORE, false, 2),
                (UNIT_POINTS, false, 3)
            }));

            // RATING_SCORE: Rating, Score, Points
            links.AddRange(CreateLinks(SC_RATING_SCORE, new[]
            {
                (UNIT_RATING, true, 1),    // Default
                (UNIT_SCORE, false, 2),
                (UNIT_POINTS, false, 3)
            }));

            // WEIGHTED_SCORE: Percentage, Score, Points
            links.AddRange(CreateLinks(SC_WEIGHTED_SCORE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_SCORE, false, 2),
                (UNIT_POINTS, false, 3)
            }));

            // COMPOSITE_SCORE: Score, Percentage, Points
            links.AddRange(CreateLinks(SC_COMPOSITE_SCORE, new[]
            {
                (UNIT_SCORE, true, 1),     // Default
                (UNIT_PERCENT, false, 2),
                (UNIT_POINTS, false, 3)
            }));

            // THRESHOLD_SCORE: Percentage, Status
            links.AddRange(CreateLinks(SC_THRESHOLD_SCORE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_STATUS, false, 2)
            }));

            // ============================================================
            // PERFORMANCE SUBCATEGORIES (7-11)
            // ============================================================

            // AVAILABILITY_RATE: Percentage only
            links.AddRange(CreateLinks(SC_AVAILABILITY_RATE, new[]
            {
                (UNIT_PERCENT, true, 1)    // Default
            }));

            // EFFICIENCY_RATE: Percentage, Ratio
            links.AddRange(CreateLinks(SC_EFFICIENCY_RATE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_RATIO, false, 2)
            }));

            // UTILIZATION_RATE: Percentage, Ratio
            links.AddRange(CreateLinks(SC_UTILIZATION_RATE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_RATIO, false, 2)
            }));

            // SUCCESS_RATE: Percentage only
            links.AddRange(CreateLinks(SC_SUCCESS_RATE, new[]
            {
                (UNIT_PERCENT, true, 1)    // Default
            }));

            // COMPLETION_RATE: Percentage only
            links.AddRange(CreateLinks(SC_COMPLETION_RATE, new[]
            {
                (UNIT_PERCENT, true, 1)    // Default
            }));

            // ============================================================
            // TREND SUBCATEGORIES (12-15)
            // ============================================================

            // GROWTH_RATE: Percentage, Ratio
            links.AddRange(CreateLinks(SC_GROWTH_RATE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_RATIO, false, 2)
            }));

            // CHANGE_RATE: Percentage, Ratio, Count
            links.AddRange(CreateLinks(SC_CHANGE_RATE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_RATIO, false, 2),
                (UNIT_COUNT, false, 3)
            }));

            // PERIOD_COMPARISON: Percentage, Ratio
            links.AddRange(CreateLinks(SC_PERIOD_COMPARISON, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_RATIO, false, 2)
            }));

            // MOVING_AVERAGE: None, Percentage, Score
            links.AddRange(CreateLinks(SC_MOVING_AVERAGE, new[]
            {
                (UNIT_NONE, true, 1),      // Default (inherits from source)
                (UNIT_PERCENT, false, 2),
                (UNIT_SCORE, false, 3)
            }));

            // ============================================================
            // COMPARISON SUBCATEGORIES (16-20)
            // ============================================================

            // RATIO: Ratio, Percentage
            links.AddRange(CreateLinks(SC_RATIO, new[]
            {
                (UNIT_RATIO, true, 1),     // Default
                (UNIT_PERCENT, false, 2)
            }));

            // VARIANCE: Percentage, Count
            links.AddRange(CreateLinks(SC_VARIANCE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_COUNT, false, 2)
            }));

            // BENCHMARK: Percentage, Score
            links.AddRange(CreateLinks(SC_BENCHMARK, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_SCORE, false, 2)
            }));

            // EXPECTED_VALUE: Percentage, Status (replaces BINARY_COMPLIANCE)
            links.AddRange(CreateLinks(SC_EXPECTED_VALUE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_STATUS, false, 2)
            }));

            // TARGET_VS_ACTUAL: Percentage, Ratio
            links.AddRange(CreateLinks(SC_TARGET_VS_ACTUAL, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_RATIO, false, 2)
            }));

            // ============================================================
            // TIME SUBCATEGORIES (21-26)
            // ============================================================

            // DURATION: Hours, Days, Minutes, Seconds
            links.AddRange(CreateLinks(SC_DURATION, new[]
            {
                (UNIT_HOURS, true, 1),     // Default
                (UNIT_DAYS, false, 2),
                (UNIT_MINUTES, false, 3),
                (UNIT_SECONDS, false, 4)
            }));

            // RESPONSE_TIME: Minutes, Seconds, Hours
            links.AddRange(CreateLinks(SC_RESPONSE_TIME, new[]
            {
                (UNIT_MINUTES, true, 1),   // Default
                (UNIT_SECONDS, false, 2),
                (UNIT_HOURS, false, 3)
            }));

            // LEAD_TIME: Days, Hours
            links.AddRange(CreateLinks(SC_LEAD_TIME, new[]
            {
                (UNIT_DAYS, true, 1),      // Default
                (UNIT_HOURS, false, 2)
            }));

            // DOWNTIME: Hours, Minutes, Days
            links.AddRange(CreateLinks(SC_DOWNTIME, new[]
            {
                (UNIT_HOURS, true, 1),     // Default
                (UNIT_MINUTES, false, 2),
                (UNIT_DAYS, false, 3)
            }));

            // TIME_IN_STATE: Hours, Days, Minutes
            links.AddRange(CreateLinks(SC_TIME_IN_STATE, new[]
            {
                (UNIT_HOURS, true, 1),     // Default
                (UNIT_DAYS, false, 2),
                (UNIT_MINUTES, false, 3)
            }));

            // AGE: Days, Hours
            links.AddRange(CreateLinks(SC_AGE, new[]
            {
                (UNIT_DAYS, true, 1),      // Default
                (UNIT_HOURS, false, 2)
            }));

            // ============================================================
            // COUNT SUBCATEGORIES (27-32)
            // ============================================================

            // QUANTITY: Count, Items, Units
            links.AddRange(CreateLinks(SC_QUANTITY, new[]
            {
                (UNIT_COUNT, true, 1),     // Default
                (UNIT_ITEMS, false, 2),
                (UNIT_UNITS, false, 3)
            }));

            // FREQUENCY: Count, Items
            links.AddRange(CreateLinks(SC_FREQUENCY, new[]
            {
                (UNIT_COUNT, true, 1),     // Default
                (UNIT_ITEMS, false, 2)
            }));

            // INCIDENTS: Count only
            links.AddRange(CreateLinks(SC_INCIDENTS, new[]
            {
                (UNIT_COUNT, true, 1)      // Default
            }));

            // CAPACITY: Units, Count, Items
            links.AddRange(CreateLinks(SC_CAPACITY, new[]
            {
                (UNIT_UNITS, true, 1),     // Default
                (UNIT_COUNT, false, 2),
                (UNIT_ITEMS, false, 3)
            }));

            // SELECTION_COUNT: Count, Percentage
            links.AddRange(CreateLinks(SC_SELECTION_COUNT, new[]
            {
                (UNIT_COUNT, true, 1),     // Default
                (UNIT_PERCENT, false, 2)
            }));

            // UNIQUE_COUNT: Count only
            links.AddRange(CreateLinks(SC_UNIQUE_COUNT, new[]
            {
                (UNIT_COUNT, true, 1)      // Default
            }));

            // ============================================================
            // COMPLIANCE SUBCATEGORIES (33-36)
            // ============================================================

            // COMPLIANCE_RATE: Percentage only
            links.AddRange(CreateLinks(SC_COMPLIANCE_RATE, new[]
            {
                (UNIT_PERCENT, true, 1)    // Default
            }));

            // AUDIT_SCORE: Percentage, Score, Points
            links.AddRange(CreateLinks(SC_AUDIT_SCORE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_SCORE, false, 2),
                (UNIT_POINTS, false, 3)
            }));

            // SLA_COMPLIANCE: Percentage, Status
            links.AddRange(CreateLinks(SC_SLA_COMPLIANCE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_STATUS, false, 2)
            }));

            // POLICY_COMPLIANCE: Percentage only
            links.AddRange(CreateLinks(SC_POLICY_COMPLIANCE, new[]
            {
                (UNIT_PERCENT, true, 1)    // Default
            }));

            // ============================================================
            // FINANCIAL SUBCATEGORIES (37-40)
            // ============================================================

            // COST: KES, USD
            links.AddRange(CreateLinks(SC_COST, new[]
            {
                (UNIT_KES, true, 1),       // Default
                (UNIT_USD, false, 2)
            }));

            // BUDGET: KES, USD
            links.AddRange(CreateLinks(SC_BUDGET, new[]
            {
                (UNIT_KES, true, 1),       // Default
                (UNIT_USD, false, 2)
            }));

            // BUDGET_VARIANCE: Percentage, KES, USD
            links.AddRange(CreateLinks(SC_BUDGET_VARIANCE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_KES, false, 2),
                (UNIT_USD, false, 3)
            }));

            // ROI: Percentage, Ratio
            links.AddRange(CreateLinks(SC_ROI, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_RATIO, false, 2)
            }));

            // ============================================================
            // NEW SCORE SUBCATEGORIES (7-9)
            // ============================================================

            // RISK_SCORE: Percentage, Score
            links.AddRange(CreateLinks(SC_RISK_SCORE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_SCORE, false, 2)
            }));

            // HEALTH_SCORE: Percentage, Score
            links.AddRange(CreateLinks(SC_HEALTH_SCORE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_SCORE, false, 2)
            }));

            // NORMALIZED_SCORE: Percentage
            links.AddRange(CreateLinks(SC_NORMALIZED_SCORE, new[]
            {
                (UNIT_PERCENT, true, 1)    // Default
            }));

            // ============================================================
            // NEW PERFORMANCE SUBCATEGORIES (15-17)
            // ============================================================

            // RELIABILITY: Percentage
            links.AddRange(CreateLinks(SC_RELIABILITY, new[]
            {
                (UNIT_PERCENT, true, 1)    // Default
            }));

            // STABILITY: Percentage
            links.AddRange(CreateLinks(SC_STABILITY, new[]
            {
                (UNIT_PERCENT, true, 1)    // Default
            }));

            // FAILURE_RATE: Percentage
            links.AddRange(CreateLinks(SC_FAILURE_RATE, new[]
            {
                (UNIT_PERCENT, true, 1)    // Default
            }));

            // ============================================================
            // FORM_ANALYTICS SUBCATEGORIES (47-51)
            // ============================================================

            // FORM_COMPLETION_RATE: Percentage
            links.AddRange(CreateLinks(SC_FORM_COMPLETION_RATE, new[]
            {
                (UNIT_PERCENT, true, 1)    // Default
            }));

            // FORM_ABANDONMENT_RATE: Percentage
            links.AddRange(CreateLinks(SC_FORM_ABANDONMENT_RATE, new[]
            {
                (UNIT_PERCENT, true, 1)    // Default
            }));

            // FIELD_SKIP_RATE: Percentage
            links.AddRange(CreateLinks(SC_FIELD_SKIP_RATE, new[]
            {
                (UNIT_PERCENT, true, 1)    // Default
            }));

            // TIME_TO_COMPLETE: Minutes, Hours, Seconds
            links.AddRange(CreateLinks(SC_TIME_TO_COMPLETE, new[]
            {
                (UNIT_MINUTES, true, 1),   // Default
                (UNIT_HOURS, false, 2),
                (UNIT_SECONDS, false, 3)
            }));

            // SUBMISSION_VOLUME: Count, Items
            links.AddRange(CreateLinks(SC_SUBMISSION_VOLUME, new[]
            {
                (UNIT_COUNT, true, 1),     // Default
                (UNIT_ITEMS, false, 2)
            }));

            // ============================================================
            // TEXT_ANALYTICS SUBCATEGORIES (52-54)
            // ============================================================

            // SENTIMENT_SCORE: Percentage, Score
            links.AddRange(CreateLinks(SC_SENTIMENT_SCORE, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_SCORE, false, 2)
            }));

            // WORD_FREQUENCY: Count, Items
            links.AddRange(CreateLinks(SC_WORD_FREQUENCY, new[]
            {
                (UNIT_COUNT, true, 1),     // Default
                (UNIT_ITEMS, false, 2)
            }));

            // TEXT_LENGTH: Count (characters/words)
            links.AddRange(CreateLinks(SC_TEXT_LENGTH, new[]
            {
                (UNIT_COUNT, true, 1)      // Default
            }));

            // ============================================================
            // ADVANCED_ANALYTICS SUBCATEGORIES (55-56)
            // ============================================================

            // CORRELATION: Ratio, Percentage
            links.AddRange(CreateLinks(SC_CORRELATION, new[]
            {
                (UNIT_RATIO, true, 1),     // Default
                (UNIT_PERCENT, false, 2)
            }));

            // PREDICTIVE_INDICATOR: Percentage, Score
            links.AddRange(CreateLinks(SC_PREDICTIVE_INDICATOR, new[]
            {
                (UNIT_PERCENT, true, 1),   // Default
                (UNIT_SCORE, false, 2)
            }));

            context.MetricSubCategoryUnits.AddRange(links);
            context.SaveChanges();
        }

        /// <summary>
        /// Helper to create multiple links for a subcategory
        /// </summary>
        private static IEnumerable<MetricSubCategoryUnit> CreateLinks(
            int subCategoryId,
            (int unitId, bool isDefault, int displayOrder)[] units)
        {
            return units.Select(u => new MetricSubCategoryUnit
            {
                SubCategoryId = subCategoryId,
                UnitId = u.unitId,
                IsDefault = u.isDefault,
                DisplayOrder = u.displayOrder,
                CreatedDate = DateTime.UtcNow
            });
        }
    }
}

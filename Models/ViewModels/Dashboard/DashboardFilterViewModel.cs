namespace FormReporting.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Filter state for dashboard data queries
    /// </summary>
    public class DashboardFilterViewModel
    {
        /// <summary>
        /// Start date for date range filter
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for date range filter
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Filter by specific tenant ID
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Filter by specific region ID
        /// </summary>
        public int? RegionId { get; set; }

        /// <summary>
        /// Filter by form category ID
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Filter by submission status
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Text search filter
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Quick date filter preset (e.g., "today", "this-week", "this-month", "this-year")
        /// </summary>
        public string? DatePreset { get; set; }

        /// <summary>
        /// Checks if any filters are active
        /// </summary>
        public bool HasActiveFilters =>
            StartDate.HasValue ||
            EndDate.HasValue ||
            TenantId.HasValue ||
            RegionId.HasValue ||
            CategoryId.HasValue ||
            !string.IsNullOrEmpty(Status) ||
            !string.IsNullOrEmpty(SearchTerm);

        /// <summary>
        /// Creates a copy of the filter with date preset applied
        /// </summary>
        public DashboardFilterViewModel WithDatePreset(string preset)
        {
            var filter = new DashboardFilterViewModel
            {
                TenantId = TenantId,
                RegionId = RegionId,
                CategoryId = CategoryId,
                Status = Status,
                SearchTerm = SearchTerm,
                DatePreset = preset
            };

            var today = DateTime.Today;

            switch (preset?.ToLower())
            {
                case "today":
                    filter.StartDate = today;
                    filter.EndDate = today;
                    break;
                case "yesterday":
                    filter.StartDate = today.AddDays(-1);
                    filter.EndDate = today.AddDays(-1);
                    break;
                case "this-week":
                    filter.StartDate = today.AddDays(-(int)today.DayOfWeek);
                    filter.EndDate = today;
                    break;
                case "last-week":
                    var lastWeekStart = today.AddDays(-(int)today.DayOfWeek - 7);
                    filter.StartDate = lastWeekStart;
                    filter.EndDate = lastWeekStart.AddDays(6);
                    break;
                case "this-month":
                    filter.StartDate = new DateTime(today.Year, today.Month, 1);
                    filter.EndDate = today;
                    break;
                case "last-month":
                    var lastMonth = today.AddMonths(-1);
                    filter.StartDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    filter.EndDate = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
                    break;
                case "this-quarter":
                    var quarterStart = new DateTime(today.Year, ((today.Month - 1) / 3) * 3 + 1, 1);
                    filter.StartDate = quarterStart;
                    filter.EndDate = today;
                    break;
                case "this-year":
                    filter.StartDate = new DateTime(today.Year, 1, 1);
                    filter.EndDate = today;
                    break;
                case "last-year":
                    filter.StartDate = new DateTime(today.Year - 1, 1, 1);
                    filter.EndDate = new DateTime(today.Year - 1, 12, 31);
                    break;
            }

            return filter;
        }
    }
}

using FormReporting.Data;
using FormReporting.Models.Common;
using FormReporting.Models.ViewModels.Dashboard;
using FormReporting.Models.ViewModels.Dashboard.Widgets;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Services.Dashboard.Providers
{
    /// <summary>
    /// Widget data provider for Form-related dashboard widgets
    /// Provides data for form templates, submissions, and completion metrics
    /// </summary>
    public class FormWidgetProvider : WidgetDataProviderBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FormWidgetProvider> _logger;

        /// <summary>
        /// Widget keys supported by this provider
        /// </summary>
        private static readonly string[] _supportedWidgets = new[]
        {
            // StatCard widgets
            "form-template-count",
            "form-submission-count",
            "form-completion-rate",
            "form-pending-count",

            // Chart widgets (Phase 2 - Section C)
            "form-submissions-by-tenant",
            "form-submissions-by-status",

            // Table widgets (Phase 2 - Section D)
            "form-recent-submissions"
        };

        public FormWidgetProvider(
            ApplicationDbContext context,
            ILogger<FormWidgetProvider> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public override string ProviderKey => "form";

        /// <inheritdoc />
        public override IEnumerable<string> SupportedWidgets => _supportedWidgets;

        /// <inheritdoc />
        public override async Task<object?> GetWidgetDataAsync(
            string widgetKey,
            DashboardFilterViewModel? filters = null,
            ContextType contextType = ContextType.None,
            int? contextId = null)
        {
            try
            {
                return widgetKey.ToLower() switch
                {
                    // StatCard widgets
                    "form-template-count" => await GetTemplateCountAsync(filters, contextType, contextId),
                    "form-submission-count" => await GetSubmissionCountAsync(filters, contextType, contextId),
                    "form-completion-rate" => await GetCompletionRateAsync(filters, contextType, contextId),
                    "form-pending-count" => await GetPendingCountAsync(filters, contextType, contextId),

                    // Chart widgets (to be implemented in Section C)
                    "form-submissions-by-tenant" => await GetSubmissionsByTenantAsync(filters, contextType, contextId),
                    "form-submissions-by-status" => await GetSubmissionsByStatusAsync(filters, contextType, contextId),

                    // Table widgets (to be implemented in Section D)
                    "form-recent-submissions" => await GetRecentSubmissionsAsync(filters, contextType, contextId),

                    _ => null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting widget data for {WidgetKey}", widgetKey);
                return null;
            }
        }

        #region StatCard Methods (Section B)

        /// <summary>
        /// Gets the count of published form templates
        /// </summary>
        private async Task<StatCardDataViewModel> GetTemplateCountAsync(
            DashboardFilterViewModel? filters,
            ContextType contextType,
            int? contextId)
        {
            var query = _context.FormTemplates
                .Where(t => t.PublishStatus == "Published" && t.IsActive);

            // Apply category filter if specified
            if (filters?.CategoryId.HasValue == true)
            {
                query = query.Where(t => t.CategoryId == filters.CategoryId.Value);
            }

            // Apply context filter (for specific form template)
            if (contextType == ContextType.FormTemplate && contextId.HasValue)
            {
                query = query.Where(t => t.TemplateId == contextId.Value);
            }

            var count = await query.CountAsync();

            // Get previous period count for trend (last 30 days comparison)
            var previousCount = await GetPreviousPeriodTemplateCountAsync(filters);
            var (trendValue, trendDirection) = CalculateTrend(count, previousCount);

            return new StatCardDataViewModel
            {
                Value = count.ToString("N0"),
                Label = "Published Forms",
                Icon = "ri-file-list-3-line",
                IconColor = "primary",
                TrendValue = trendValue,
                TrendDirection = trendDirection,
                TrendLabel = "vs last period",
                UpIsGood = true
            };
        }

        /// <summary>
        /// Gets the total count of form submissions
        /// </summary>
        private async Task<StatCardDataViewModel> GetSubmissionCountAsync(
            DashboardFilterViewModel? filters,
            ContextType contextType,
            int? contextId)
        {
            var query = _context.FormTemplateSubmissions.AsQueryable();

            // Apply filters
            query = ApplySubmissionFilters(query, filters, contextType, contextId);

            var count = await query.CountAsync();

            // Get previous period for trend
            var previousCount = await GetPreviousPeriodSubmissionCountAsync(filters, contextType, contextId);
            var (trendValue, trendDirection) = CalculateTrend(count, previousCount);

            return new StatCardDataViewModel
            {
                Value = count.ToString("N0"),
                Label = "Total Submissions",
                Icon = "ri-file-check-line",
                IconColor = "success",
                TrendValue = trendValue,
                TrendDirection = trendDirection,
                TrendLabel = "vs last period",
                UpIsGood = true
            };
        }

        /// <summary>
        /// Gets the completion rate (percentage of approved/submitted vs total)
        /// </summary>
        private async Task<StatCardDataViewModel> GetCompletionRateAsync(
            DashboardFilterViewModel? filters,
            ContextType contextType,
            int? contextId)
        {
            var query = _context.FormTemplateSubmissions.AsQueryable();

            // Apply filters
            query = ApplySubmissionFilters(query, filters, contextType, contextId);

            var totalCount = await query.CountAsync();

            // Completed = Submitted, Approved (not Draft, InApproval, or Rejected)
            var completedCount = await query
                .Where(s => s.Status == "Submitted" || s.Status == "Approved")
                .CountAsync();

            var rate = totalCount > 0
                ? Math.Round((decimal)completedCount / totalCount * 100, 1)
                : 0;

            // Get previous period for trend
            var previousRate = await GetPreviousPeriodCompletionRateAsync(filters, contextType, contextId);
            var (trendValue, trendDirection) = CalculateTrend(rate, previousRate);

            return new StatCardDataViewModel
            {
                Value = $"{rate}%",
                Label = "Completion Rate",
                Icon = "ri-pie-chart-line",
                IconColor = rate >= 80 ? "success" : rate >= 50 ? "warning" : "danger",
                TrendValue = trendValue,
                TrendDirection = trendDirection,
                TrendLabel = "vs last period",
                UpIsGood = true,
                SecondaryValue = $"{completedCount:N0} of {totalCount:N0}"
            };
        }

        /// <summary>
        /// Gets the count of pending submissions (Draft + InApproval)
        /// </summary>
        private async Task<StatCardDataViewModel> GetPendingCountAsync(
            DashboardFilterViewModel? filters,
            ContextType contextType,
            int? contextId)
        {
            var query = _context.FormTemplateSubmissions
                .Where(s => s.Status == "Draft" || s.Status == "InApproval");

            // Apply filters
            query = ApplySubmissionFilters(query, filters, contextType, contextId);

            var count = await query.CountAsync();

            // Get breakdown
            var draftCount = await query.Where(s => s.Status == "Draft").CountAsync();
            var inApprovalCount = count - draftCount;

            return new StatCardDataViewModel
            {
                Value = count.ToString("N0"),
                Label = "Pending Submissions",
                Icon = "ri-time-line",
                IconColor = count > 0 ? "warning" : "success",
                TrendValue = null,
                TrendDirection = null,
                UpIsGood = false, // Lower pending count is better
                SecondaryValue = $"{draftCount:N0} draft, {inApprovalCount:N0} in approval"
            };
        }

        #endregion

        #region Chart Methods (Section C - Placeholder)

        /// <summary>
        /// Gets submissions grouped by tenant for bar chart
        /// </summary>
        private async Task<ChartDataViewModel?> GetSubmissionsByTenantAsync(
            DashboardFilterViewModel? filters,
            ContextType contextType,
            int? contextId)
        {
            // To be implemented in Section C
            await Task.CompletedTask;
            return null;
        }

        /// <summary>
        /// Gets submissions grouped by status for pie chart
        /// </summary>
        private async Task<ChartDataViewModel?> GetSubmissionsByStatusAsync(
            DashboardFilterViewModel? filters,
            ContextType contextType,
            int? contextId)
        {
            // To be implemented in Section C
            await Task.CompletedTask;
            return null;
        }

        #endregion

        #region Table Methods (Section D - Placeholder)

        /// <summary>
        /// Gets recent submissions for data table
        /// </summary>
        private async Task<TableDataViewModel?> GetRecentSubmissionsAsync(
            DashboardFilterViewModel? filters,
            ContextType contextType,
            int? contextId)
        {
            // To be implemented in Section D
            await Task.CompletedTask;
            return null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Applies common filters to submission queries
        /// </summary>
        private IQueryable<Models.Entities.Forms.FormTemplateSubmission> ApplySubmissionFilters(
            IQueryable<Models.Entities.Forms.FormTemplateSubmission> query,
            DashboardFilterViewModel? filters,
            ContextType contextType,
            int? contextId)
        {
            // Apply date range filter
            if (filters?.StartDate.HasValue == true)
            {
                query = query.Where(s => s.CreatedDate >= filters.StartDate.Value);
            }
            if (filters?.EndDate.HasValue == true)
            {
                var endDate = filters.EndDate.Value.AddDays(1); // Include the end date
                query = query.Where(s => s.CreatedDate < endDate);
            }

            // Apply tenant filter
            if (filters?.TenantId.HasValue == true)
            {
                query = query.Where(s => s.TenantId == filters.TenantId.Value);
            }

            // Apply category filter (via template relationship)
            if (filters?.CategoryId.HasValue == true)
            {
                query = query.Where(s => s.Template.CategoryId == filters.CategoryId.Value);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(filters?.Status))
            {
                query = query.Where(s => s.Status == filters.Status);
            }

            // Apply context filter
            switch (contextType)
            {
                case ContextType.FormTemplate when contextId.HasValue:
                    query = query.Where(s => s.TemplateId == contextId.Value);
                    break;
                case ContextType.Tenant when contextId.HasValue:
                    query = query.Where(s => s.TenantId == contextId.Value);
                    break;
            }

            return query;
        }

        /// <summary>
        /// Gets previous period template count for trend calculation
        /// </summary>
        private async Task<int> GetPreviousPeriodTemplateCountAsync(DashboardFilterViewModel? filters)
        {
            // Compare with 30 days ago snapshot
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            return await _context.FormTemplates
                .Where(t => t.PublishStatus == "Published"
                    && t.IsActive
                    && t.CreatedDate <= thirtyDaysAgo)
                .CountAsync();
        }

        /// <summary>
        /// Gets previous period submission count for trend calculation
        /// </summary>
        private async Task<int> GetPreviousPeriodSubmissionCountAsync(
            DashboardFilterViewModel? filters,
            ContextType contextType,
            int? contextId)
        {
            // Calculate previous period based on current filter
            var (previousStart, previousEnd) = GetPreviousPeriodDates(filters);

            var previousFilters = new DashboardFilterViewModel
            {
                StartDate = previousStart,
                EndDate = previousEnd,
                TenantId = filters?.TenantId,
                CategoryId = filters?.CategoryId,
                Status = filters?.Status
            };

            var query = _context.FormTemplateSubmissions.AsQueryable();
            query = ApplySubmissionFilters(query, previousFilters, contextType, contextId);

            return await query.CountAsync();
        }

        /// <summary>
        /// Gets previous period completion rate for trend calculation
        /// </summary>
        private async Task<decimal> GetPreviousPeriodCompletionRateAsync(
            DashboardFilterViewModel? filters,
            ContextType contextType,
            int? contextId)
        {
            var (previousStart, previousEnd) = GetPreviousPeriodDates(filters);

            var previousFilters = new DashboardFilterViewModel
            {
                StartDate = previousStart,
                EndDate = previousEnd,
                TenantId = filters?.TenantId,
                CategoryId = filters?.CategoryId,
                Status = filters?.Status
            };

            var query = _context.FormTemplateSubmissions.AsQueryable();
            query = ApplySubmissionFilters(query, previousFilters, contextType, contextId);

            var totalCount = await query.CountAsync();
            if (totalCount == 0) return 0;

            var completedCount = await query
                .Where(s => s.Status == "Submitted" || s.Status == "Approved")
                .CountAsync();

            return Math.Round((decimal)completedCount / totalCount * 100, 1);
        }

        /// <summary>
        /// Calculates previous period dates based on current filter
        /// </summary>
        private (DateTime? start, DateTime? end) GetPreviousPeriodDates(DashboardFilterViewModel? filters)
        {
            if (filters?.StartDate.HasValue != true || filters?.EndDate.HasValue != true)
            {
                // Default: compare to previous 30 days
                var now = DateTime.Now;
                return (now.AddDays(-60), now.AddDays(-30));
            }

            var periodLength = (filters.EndDate.Value - filters.StartDate.Value).Days;
            var previousEnd = filters.StartDate.Value.AddDays(-1);
            var previousStart = previousEnd.AddDays(-periodLength);

            return (previousStart, previousEnd);
        }

        /// <summary>
        /// Calculates trend value and direction
        /// </summary>
        private (decimal? value, string? direction) CalculateTrend(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return current > 0 ? (100, "up") : (null, null);
            }

            var change = ((current - previous) / previous) * 100;
            var direction = change > 0 ? "up" : change < 0 ? "down" : "neutral";

            return (Math.Abs(Math.Round(change, 1)), direction);
        }

        #endregion
    }
}

using FormReporting.Models.ViewModels.Forms;
using System.Security.Claims;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service for generating submission statistics and reporting
    /// Handles counts, status breakdowns, on-time tracking, and completion metrics
    /// </summary>
    public interface IFormSubmissionStatisticsService
    {
        /// <summary>
        /// Get total submission count for a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="startDate">Optional date range start</param>
        /// <param name="endDate">Optional date range end</param>
        /// <param name="tenantId">Optional tenant filter</param>
        /// <returns>Total submission count</returns>
        Task<int> GetTotalSubmissionsAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null);

        /// <summary>
        /// Get submission count breakdown by status
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="startDate">Optional date range start</param>
        /// <param name="endDate">Optional date range end</param>
        /// <param name="tenantId">Optional tenant filter</param>
        /// <returns>Dictionary of status counts</returns>
        Task<Dictionary<string, int>> GetSubmissionsByStatusAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null);

        /// <summary>
        /// Calculate on-time vs late submission percentages
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="startDate">Optional date range start</param>
        /// <param name="endDate">Optional date range end</param>
        /// <param name="tenantId">Optional tenant filter</param>
        /// <returns>On-time statistics view model</returns>
        Task<OnTimeStatisticsViewModel> GetOnTimeStatisticsAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null);

        /// <summary>
        /// Calculate average completion time (CreatedDate to SubmittedDate)
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="startDate">Optional date range start</param>
        /// <param name="endDate">Optional date range end</param>
        /// <param name="tenantId">Optional tenant filter</param>
        /// <returns>Average completion time in hours</returns>
        Task<double?> GetAverageCompletionTimeAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null);

        /// <summary>
        /// Get submission trends over time (daily, weekly, or monthly)
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="startDate">Date range start</param>
        /// <param name="endDate">Date range end</param>
        /// <param name="groupBy">Grouping period: Daily, Weekly, Monthly</param>
        /// <param name="tenantId">Optional tenant filter</param>
        /// <returns>Time series data of submission counts</returns>
        Task<List<SubmissionTrendDataPoint>> GetSubmissionTrendsAsync(int templateId, DateTime startDate, DateTime endDate, string groupBy = "Daily", int? tenantId = null);

        /// <summary>
        /// Get comprehensive statistics dashboard data for a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="startDate">Optional date range start</param>
        /// <param name="endDate">Optional date range end</param>
        /// <param name="tenantId">Optional tenant filter</param>
        /// <returns>Complete statistics dashboard view model</returns>
        Task<TemplateStatisticsDashboardViewModel> GetTemplateStatisticsDashboardAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null);

        /// <summary>
        /// Get submission rate by tenant comparison
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="startDate">Optional date range start</param>
        /// <param name="endDate">Optional date range end</param>
        /// <returns>List of tenant submission statistics</returns>
        Task<List<TenantSubmissionStatistics>> GetTenantComparisonAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get submission rate by user/submitter
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="startDate">Optional date range start</param>
        /// <param name="endDate">Optional date range end</param>
        /// <param name="tenantId">Optional tenant filter</param>
        /// <returns>List of user submission statistics</returns>
        Task<List<UserSubmissionStatistics>> GetUserSubmissionRatesAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null);

        /// <summary>
        /// Get recent submissions list
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="count">Number of recent submissions to retrieve</param>
        /// <param name="tenantId">Optional tenant filter</param>
        /// <param name="currentUser">Current user for scope filtering</param>
        /// <returns>List of recent submission summaries</returns>
        Task<List<SubmissionSummaryViewModel>> GetRecentSubmissionsAsync(int templateId, int count = 10, int? tenantId = null, ClaimsPrincipal currentUser = null);

        // Scope-aware methods that can handle single or multiple templates
        Task<List<SubmissionSummaryViewModel>> GetRecentSubmissionsAsync(List<int> templateIds, int count = 10, int? tenantId = null, ClaimsPrincipal currentUser = null);
        Task<Dictionary<string, int>> GetSubmissionsByStatusAsync(List<int> templateIds, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null, ClaimsPrincipal currentUser = null);
        Task<List<SubmissionTrendDataPoint>> GetSubmissionTrendsAsync(List<int> templateIds, DateTime startDate, DateTime endDate, string groupBy = "Daily", int? tenantId = null, ClaimsPrincipal currentUser = null);
    }
}

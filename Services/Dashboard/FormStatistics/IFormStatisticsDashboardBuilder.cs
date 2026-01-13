using FormReporting.Models.ViewModels.Dashboard.Components.Composite;
using FormReporting.Models.ViewModels.Dashboard.FormStatistics;
using System.Security.Claims;

namespace FormReporting.Services.Dashboard.FormStatistics
{
    /// <summary>
    /// Interface for building Form Statistics Dashboard components
    /// </summary>
    public interface IFormStatisticsDashboardBuilder
    {
        /// <summary>
        /// Build complete dashboard configuration
        /// </summary>
        Task<DashboardConfig> BuildDashboardAsync(
            int? templateId, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            int? tenantId = null,
            DashboardMode mode = DashboardMode.FullPage);
        
        /// <summary>
        /// Build quick stats cards (Total, Submitted, On-Time %, Avg Time)
        /// </summary>
        Task<List<StatCardConfig>> BuildQuickStatsAsync(
            int? templateId,  // Make nullable to support overall view
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            int? tenantId = null,
            ClaimsPrincipal currentUser = null);  // Add user for scope filtering
        
        /// <summary>
        /// Build submission trend line chart
        /// </summary>
        Task<ChartCardConfig> BuildTrendChartAsync(
            int? templateId,  // Make nullable to support overall view
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            int? tenantId = null, 
            string groupBy = "Daily",
            ClaimsPrincipal currentUser = null);  // Add user for scope filtering
        
        /// <summary>
        /// Build status breakdown pie chart
        /// </summary>
        Task<ChartCardConfig> BuildStatusChartAsync(
            int? templateId,  // Make nullable to support overall view
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            int? tenantId = null,
            ClaimsPrincipal currentUser = null);  // Add user for scope filtering
        
        /// <summary>
        /// Build recent submissions table
        /// </summary>
        Task<DataTableConfig> BuildRecentSubmissionsTableAsync(
            int? templateId,  // Make nullable to support overall view
            int count = 10, 
            int? tenantId = null,
            ClaimsPrincipal currentUser = null);  // Add user for scope filtering
        
        /// <summary>
        /// Build tenant comparison table
        /// </summary>
        Task<DataTableConfig> BuildTenantComparisonTableAsync(
            int templateId, 
            DateTime? startDate = null, 
            DateTime? endDate = null);
    }
}

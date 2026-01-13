using FormReporting.Models.ViewModels.Dashboard.Components.Composite;
using FormReporting.Services.Organizational;
using System.Security.Claims;

namespace FormReporting.Services.Dashboard
{
    /// <summary>
    /// Interface for dashboard filter operations
    /// Provides reusable filter components for all dashboards
    /// </summary>
    public interface IFilterService
    {
        /// <summary>
        /// Build standard filter panel configuration for dashboards
        /// </summary>
        Task<FilterPanelConfig> BuildStandardFiltersAsync(
            ClaimsPrincipal currentUser,
            int? templateId = null,
            string? mode = null);

        /// <summary>
        /// Get region filter options based on user scope
        /// </summary>
        Task<List<SelectOption>> GetRegionOptionsAsync(ClaimsPrincipal currentUser);

        /// <summary>
        /// Get tenant filter options based on user scope and optional region filter
        /// </summary>
        Task<List<SelectOption>> GetTenantOptionsAsync(ClaimsPrincipal currentUser, int? regionId = null);

        /// <summary>
        /// Get status filter options based on template submissions
        /// </summary>
        Task<List<SelectOption>> GetStatusOptionsAsync(ClaimsPrincipal currentUser, int? templateId = null);

        /// <summary>
        /// Get submitter filter options based on template submissions
        /// </summary>
        Task<List<SelectOption>> GetSubmitterOptionsAsync(ClaimsPrincipal currentUser, int? templateId = null);

        /// <summary>
        /// Get template filter options for overall dashboard
        /// </summary>
        Task<List<SelectOption>> GetTemplateOptionsAsync(ClaimsPrincipal currentUser);

        /// <summary>
        /// Get group by options for date aggregation
        /// </summary>
        List<SelectOption> GetGroupByOptions();
    }
}

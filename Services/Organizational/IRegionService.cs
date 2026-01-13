using FormReporting.Models.Entities.Organizational;
using System.Security.Claims;

namespace FormReporting.Services.Organizational
{
    /// <summary>
    /// Interface for region-related operations with scope-based access control
    /// </summary>
    public interface IRegionService
    {
        /// <summary>
        /// Get all regions the current user can access based on their scope
        /// </summary>
        Task<List<Region>> GetAccessibleRegionsAsync(ClaimsPrincipal currentUser);

        /// <summary>
        /// Get a specific region by ID if user has access
        /// </summary>
        Task<Region?> GetRegionByIdAsync(ClaimsPrincipal currentUser, int regionId);

        /// <summary>
        /// Get tenants in a specific region (filtered by user scope)
        /// </summary>
        Task<List<Tenant>> GetTenantsByRegionAsync(ClaimsPrincipal currentUser, int regionId);

        /// <summary>
        /// Check if current user can access specified region
        /// </summary>
        Task<bool> CanAccessRegionAsync(ClaimsPrincipal currentUser, int regionId);

        /// <summary>
        /// Get regions grouped by region code for dropdowns
        /// </summary>
        Task<Dictionary<string, List<Region>>> GetRegionsGroupedByCodeAsync(ClaimsPrincipal currentUser);
    }
}

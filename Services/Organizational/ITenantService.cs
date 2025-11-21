using FormReporting.Models.Entities.Organizational;
using System.Security.Claims;

namespace FormReporting.Services.Organizational
{
    /// <summary>
    /// Interface for tenant-related operations with scope-based access control
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// Get all tenants the current user can access based on their scope
        /// </summary>
        /// <param name="currentUser">Current user's claims principal</param>
        /// <param name="searchQuery">Optional search term to filter tenants</param>
        /// <returns>List of accessible tenants with navigation properties loaded</returns>
        Task<List<Tenant>> GetAccessibleTenantsAsync(ClaimsPrincipal currentUser, string? searchQuery = null);

        /// <summary>
        /// Get a specific tenant by ID if user has access
        /// </summary>
        /// <param name="currentUser">Current user's claims principal</param>
        /// <param name="tenantId">Tenant ID to retrieve</param>
        /// <returns>Tenant if accessible, null otherwise</returns>
        Task<Tenant?> GetTenantByIdAsync(ClaimsPrincipal currentUser, int tenantId);

        /// <summary>
        /// Check if current user can access specified tenant
        /// </summary>
        /// <param name="currentUser">Current user's claims principal</param>
        /// <param name="tenantId">Tenant ID to check</param>
        /// <returns>True if accessible, false otherwise</returns>
        Task<bool> CanAccessTenantAsync(ClaimsPrincipal currentUser, int tenantId);

        /// <summary>
        /// Get tenants grouped by region (for dropdowns and filters)
        /// </summary>
        /// <param name="currentUser">Current user's claims principal</param>
        /// <returns>Tenants grouped by region</returns>
        Task<Dictionary<string, List<Tenant>>> GetTenantsGroupedByRegionAsync(ClaimsPrincipal currentUser);
    }
}

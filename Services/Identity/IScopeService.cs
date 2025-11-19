using FormReporting.Models.Entities.Identity;
using System.Security.Claims;

namespace FormReporting.Services.Identity
{
    /// <summary>
    /// Interface for scope and access control operations
    /// </summary>
    public interface IScopeService
    {
        /// <summary>
        /// Get complete scope information for a user
        /// </summary>
        /// <param name="user">Claims principal</param>
        /// <returns>UserScope object with all scope details</returns>
        Task<UserScope> GetUserScopeAsync(ClaimsPrincipal user);

        /// <summary>
        /// Get list of tenant IDs the user can access (scope + exceptions)
        /// </summary>
        /// <param name="user">Claims principal</param>
        /// <returns>List of accessible tenant IDs</returns>
        Task<List<int>> GetAccessibleTenantIdsAsync(ClaimsPrincipal user);

        /// <summary>
        /// Check if user has access to a specific tenant
        /// </summary>
        /// <param name="user">Claims principal</param>
        /// <param name="tenantId">Tenant ID to check</param>
        /// <returns>True if user has access, false otherwise</returns>
        Task<bool> HasAccessToTenantAsync(ClaimsPrincipal user, int tenantId);

        /// <summary>
        /// Check if user has a specific permission
        /// </summary>
        /// <param name="user">Claims principal</param>
        /// <param name="permissionCode">Permission code (e.g., "Forms.Submit")</param>
        /// <returns>True if user has permission, false otherwise</returns>
        bool HasPermission(ClaimsPrincipal user, string permissionCode);

        /// <summary>
        /// Check if user has a specific role
        /// </summary>
        /// <param name="user">Claims principal</param>
        /// <param name="roleName">Role name</param>
        /// <returns>True if user has role, false otherwise</returns>
        bool HasRole(ClaimsPrincipal user, string roleName);

        /// <summary>
        /// Get user's scope level from claims
        /// </summary>
        /// <param name="user">Claims principal</param>
        /// <returns>ScopeLevel entity or null</returns>
        Task<ScopeLevel?> GetUserScopeLevelAsync(ClaimsPrincipal user);
    }
}

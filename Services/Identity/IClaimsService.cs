using FormReporting.Models.Entities.Identity;
using System.Security.Claims;

namespace FormReporting.Services.Identity
{
    /// <summary>
    /// Interface for building and managing user claims
    /// </summary>
    public interface IClaimsService
    {
        /// <summary>
        /// Build all claims for a user (WF-2.7)
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>List of claims</returns>
        Task<List<Claim>> BuildUserClaimsAsync(User user);

        /// <summary>
        /// Get identity claims (UserId, UserName, Email, FullName, etc.)
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>List of identity claims</returns>
        Task<List<Claim>> GetIdentityClaimsAsync(User user);

        /// <summary>
        /// Get role claims from UserRoles
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>List of role claims</returns>
        Task<List<Claim>> GetRoleClaimsAsync(User user);

        /// <summary>
        /// Get permission claims from RolePermissions
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>List of permission claims</returns>
        Task<List<Claim>> GetPermissionClaimsAsync(User user);

        /// <summary>
        /// Get tenant access claims based on ScopeLevel
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>List of tenant access claims</returns>
        Task<List<Claim>> GetTenantAccessClaimsAsync(User user);

        /// <summary>
        /// Get scope claims from user's roles
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>List of scope claims</returns>
        Task<List<Claim>> GetScopeClaimsAsync(User user);

        /// <summary>
        /// Get template access claims based on user's scope and tenant access
        /// Templates are filtered by tenant access and published status
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>List of template access claims</returns>
        Task<List<Claim>> GetTemplateAccessClaimsAsync(User user);

        /// <summary>
        /// Get region access claims based on user's scope
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>List of region access claims</returns>
        Task<List<Claim>> GetRegionAccessClaimsAsync(User user);

        /// <summary>
        /// Invalidate cached claims for a user (when roles/permissions change)
        /// </summary>
        /// <param name="userId">User ID</param>
        Task InvalidateUserClaimsCacheAsync(int userId);

        // ===== Current User Helpers =====

        /// <summary>
        /// Get the current user's ID from HttpContext
        /// </summary>
        /// <returns>User ID or 0 if not authenticated</returns>
        int GetUserId();

        /// <summary>
        /// Get the current user's full name from HttpContext
        /// </summary>
        /// <returns>Full name or empty string</returns>
        string GetUserFullName();

        /// <summary>
        /// Get the client IP address from HttpContext
        /// </summary>
        /// <returns>IP address or null</returns>
        string? GetClientIP();

        /// <summary>
        /// Check if current user has a specific role
        /// </summary>
        bool HasRole(string roleName);

        /// <summary>
        /// Check if current user has a specific permission
        /// </summary>
        bool HasPermission(string permissionCode);
    }
}

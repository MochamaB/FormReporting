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
        /// Invalidate cached claims for a user (when roles/permissions change)
        /// </summary>
        /// <param name="userId">User ID</param>
        Task InvalidateUserClaimsCacheAsync(int userId);
    }
}

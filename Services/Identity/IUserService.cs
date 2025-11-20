using FormReporting.Models.Entities.Identity;
using System.Security.Claims;

namespace FormReporting.Services.Identity
{
    /// <summary>
    /// Service interface for user operations with scope-based access control
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get users accessible to the current user based on their scope
        /// </summary>
        /// <param name="currentUser">Current user's claims principal</param>
        /// <param name="searchQuery">Optional search query</param>
        /// <returns>List of accessible users</returns>
        Task<List<User>> GetAccessibleUsersAsync(ClaimsPrincipal currentUser, string? searchQuery = null);

        /// <summary>
        /// Search users by name, email, or username within current user's scope
        /// </summary>
        /// <param name="currentUser">Current user's claims principal</param>
        /// <param name="query">Search query</param>
        /// <param name="limit">Maximum number of results</param>
        /// <returns>List of matching users</returns>
        Task<List<User>> SearchUsersAsync(ClaimsPrincipal currentUser, string query, int limit = 20);

        /// <summary>
        /// Get user by ID if accessible to current user
        /// </summary>
        /// <param name="currentUser">Current user's claims principal</param>
        /// <param name="userId">User ID to retrieve</param>
        /// <returns>User if accessible, null otherwise</returns>
        Task<User?> GetUserByIdAsync(ClaimsPrincipal currentUser, int userId);

        /// <summary>
        /// Check if current user can access specified user
        /// </summary>
        /// <param name="currentUser">Current user's claims principal</param>
        /// <param name="targetUserId">Target user ID</param>
        /// <returns>True if accessible, false otherwise</returns>
        Task<bool> CanAccessUserAsync(ClaimsPrincipal currentUser, int targetUserId);

        /// <summary>
        /// Get accessible users grouped by tenant (for bulk selection UIs)
        /// </summary>
        /// <param name="currentUser">Current user's claims principal</param>
        /// <param name="searchQuery">Optional search query</param>
        /// <returns>Users grouped by tenant with metadata</returns>
        Task<List<object>> GetUsersGroupedByTenantAsync(ClaimsPrincipal currentUser, string? searchQuery = null);
    }
}

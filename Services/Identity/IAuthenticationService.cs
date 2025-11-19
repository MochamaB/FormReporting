using FormReporting.Models.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace FormReporting.Services.Identity
{
    /// <summary>
    /// Interface for authentication operations
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticate user and sign in
        /// </summary>
        /// <param name="usernameOrEmail">Username or email address</param>
        /// <param name="password">User password</param>
        /// <param name="rememberMe">Remember me option</param>
        /// <returns>SignInResult indicating success or failure</returns>
        Task<SignInResult> LoginAsync(string usernameOrEmail, string password, bool rememberMe);

        /// <summary>
        /// Sign out the current user
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Get user by username or email
        /// </summary>
        /// <param name="usernameOrEmail">Username or email address</param>
        /// <returns>User entity or null if not found</returns>
        Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail);

        /// <summary>
        /// Validate if user account is active
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>True if active, false otherwise</returns>
        Task<bool> ValidateUserIsActiveAsync(User user);

        /// <summary>
        /// Check if user account is locked
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>True if locked, false otherwise</returns>
        Task<bool> IsAccountLockedAsync(User user);

        /// <summary>
        /// Update user's last login date
        /// </summary>
        /// <param name="userId">User ID</param>
        Task UpdateLastLoginAsync(int userId);

        /// <summary>
        /// Increment failed access count for user
        /// </summary>
        /// <param name="user">User entity</param>
        Task IncrementAccessFailedCountAsync(User user);

        /// <summary>
        /// Reset failed access count to zero
        /// </summary>
        /// <param name="user">User entity</param>
        Task ResetAccessFailedCountAsync(User user);

        /// <summary>
        /// Check if password is correct for user
        /// </summary>
        /// <param name="user">User entity</param>
        /// <param name="password">Password to check</param>
        /// <returns>True if password is correct, false otherwise</returns>
        Task<bool> CheckPasswordAsync(User user, string password);
    }
}

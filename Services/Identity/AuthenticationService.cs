using FormReporting.Data;
using FormReporting.Models.Entities.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FormReporting.Services.Identity
{
    /// <summary>
    /// Implementation of authentication operations (without ASP.NET Core Identity)
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClaimsService _claimsService;

        public AuthenticationService(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            IHttpContextAccessor httpContextAccessor,
            IClaimsService claimsService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
            _claimsService = claimsService;
        }

        /// <summary>
        /// Authenticate user and sign in (WF-2.6)
        /// </summary>
        public async Task<SignInResult> LoginAsync(string usernameOrEmail, string password, bool rememberMe)
        {
            var user = await GetUserByUsernameOrEmailAsync(usernameOrEmail);
            
            if (user == null)
            {
                return SignInResult.Failed;
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return SignInResult.NotAllowed;
            }

            // Check if account is locked
            if (await IsAccountLockedAsync(user))
            {
                return SignInResult.LockedOut;
            }

            // Verify password
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, password);
            
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return SignInResult.Failed;
            }

            // Build all user claims using ClaimsService (WF-2.7)
            var claims = await _claimsService.BuildUserClaimsAsync(user);

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);

            return SignInResult.Success;
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        public async Task LogoutAsync()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Get user by username or email
        /// </summary>
        public async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            // Try to find by username first (case-insensitive)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == usernameOrEmail.ToLower());
            
            // If not found, try by email (case-insensitive)
            if (user == null)
            {
                user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == usernameOrEmail.ToLower());
            }

            return user;
        }

        /// <summary>
        /// Validate if user account is active
        /// </summary>
        public Task<bool> ValidateUserIsActiveAsync(User user)
        {
            return Task.FromResult(user.IsActive);
        }

        /// <summary>
        /// Check if user account is locked (WF-2.6)
        /// </summary>
        public Task<bool> IsAccountLockedAsync(User user)
        {
            if (user.LockoutEnd == null)
            {
                return Task.FromResult(false);
            }

            // Check if lockout period has expired
            var isLocked = user.LockoutEnd > DateTimeOffset.UtcNow;
            return Task.FromResult(isLocked);
        }

        /// <summary>
        /// Update user's last login date
        /// </summary>
        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Increment failed access count (WF-2.6: Lock after 5 attempts)
        /// </summary>
        public async Task IncrementAccessFailedCountAsync(User user)
        {
            user.AccessFailedCount++;
            
            // Lock account if 5 or more failed attempts
            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30);
            }
            
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Reset failed access count to zero
        /// </summary>
        public async Task ResetAccessFailedCountAsync(User user)
        {
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Check if password is correct for user
        /// </summary>
        public Task<bool> CheckPasswordAsync(User user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, password);
            return Task.FromResult(result == PasswordVerificationResult.Success);
        }
    }
}

using FormReporting.Data;
using FormReporting.Models.ViewModels.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FormReporting.Controllers.Identity
{
    /// <summary>
    /// Handles password management (forgot, reset, change)
    /// </summary>
    [Route("Password")]
    public class PasswordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<FormReporting.Models.Entities.Identity.User> _passwordHasher;
        private readonly IDataProtector _dataProtector;
        private readonly ILogger<PasswordController> _logger;

        public PasswordController(
            ApplicationDbContext context,
            IPasswordHasher<FormReporting.Models.Entities.Identity.User> passwordHasher,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<PasswordController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _dataProtector = dataProtectionProvider.CreateProtector("PasswordResetTokens");
            _logger = logger;
        }

        #region Forgot Password

        /// <summary>
        /// Display forgot password page
        /// </summary>
        [HttpGet("ForgotPassword")]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Handle forgot password submission (WF-2.8)
        /// </summary>
        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (user == null || !user.IsActive)
                {
                    // Don't reveal that user doesn't exist or is inactive
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                // Generate password reset token (expires in 24 hours)
                var token = GeneratePasswordResetToken(user.UserId);

                // TODO: Send notification via NotificationService (Step 6)
                // For now, just log the token (REMOVE IN PRODUCTION!)
                _logger.LogInformation("Password reset token for {Email}: {Token}", user.Email, token);

                // Store token temporarily (you could also store in database)
                TempData["ResetToken"] = token;
                TempData["ResetEmail"] = user.Email;

                await LogPasswordResetRequestedAsync(user.UserId);

                return RedirectToAction("ForgotPasswordConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for email: {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// Display forgot password confirmation page
        /// </summary>
        [HttpGet("ForgotPasswordConfirmation")]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        #endregion

        #region Reset Password

        /// <summary>
        /// Display reset password page
        /// </summary>
        [HttpGet("ResetPassword")]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? token, string? email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        /// <summary>
        /// Handle reset password submission (WF-2.8)
        /// </summary>
        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (user == null)
                {
                    // Don't reveal that user doesn't exist
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                // Validate token
                if (!ValidatePasswordResetToken(model.Token, user.UserId))
                {
                    ModelState.AddModelError("", "Invalid or expired reset token.");
                    return View(model);
                }

                // Hash new password
                user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
                user.SecurityStamp = Guid.NewGuid().ToString();

                await _context.SaveChangesAsync();

                // TODO: Send password changed notification (Step 6)
                await LogPasswordResetCompletedAsync(user.UserId);

                _logger.LogInformation("Password reset completed for user: {Email}", user.Email);

                return RedirectToAction("ResetPasswordConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for email: {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// Display reset password confirmation page
        /// </summary>
        [HttpGet("ResetPasswordConfirmation")]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        #endregion

        #region Change Password

        /// <summary>
        /// Display change password page (authenticated users)
        /// </summary>
        [HttpGet("ChangePassword")]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Handle change password submission (authenticated users)
        /// </summary>
        [HttpPost("ChangePassword")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return RedirectToAction("Login", "Account");
                }

                var userId = int.Parse(userIdStr);
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Verify current password
                var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, model.CurrentPassword);

                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View(model);
                }

                // Hash new password
                user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
                user.SecurityStamp = Guid.NewGuid().ToString();

                await _context.SaveChangesAsync();

                // TODO: Send password changed notification (Step 6)
                await LogPasswordChangedAsync(userId);

                _logger.LogInformation("Password changed for user: {UserId}", userId);

                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(model);
            }
        }

        #endregion

        #region Token Management

        /// <summary>
        /// Generate password reset token (expires in 24 hours)
        /// </summary>
        private string GeneratePasswordResetToken(int userId)
        {
            var tokenData = $"{userId}|{DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds()}";
            return _dataProtector.Protect(tokenData);
        }

        /// <summary>
        /// Validate password reset token
        /// </summary>
        private bool ValidatePasswordResetToken(string token, int userId)
        {
            try
            {
                var tokenData = _dataProtector.Unprotect(token);
                var parts = tokenData.Split('|');

                if (parts.Length != 2)
                {
                    return false;
                }

                var tokenUserId = int.Parse(parts[0]);
                var expiryTimestamp = long.Parse(parts[1]);
                var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expiryTimestamp);

                // Check if token is for correct user and not expired
                return tokenUserId == userId && DateTimeOffset.UtcNow < expiryDate;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Audit Logging (Placeholders)

        private Task LogPasswordResetRequestedAsync(int userId)
        {
            // TODO: Implement audit logging (Step 11)
            _logger.LogInformation("Password reset requested - UserId: {UserId}", userId);
            return Task.CompletedTask;
        }

        private Task LogPasswordResetCompletedAsync(int userId)
        {
            // TODO: Implement audit logging (Step 11)
            _logger.LogInformation("Password reset completed - UserId: {UserId}", userId);
            return Task.CompletedTask;
        }

        private Task LogPasswordChangedAsync(int userId)
        {
            // TODO: Implement audit logging (Step 11)
            _logger.LogInformation("Password changed - UserId: {UserId}", userId);
            return Task.CompletedTask;
        }

        #endregion
    }
}

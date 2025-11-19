using FormReporting.Models.ViewModels.Identity;
using FormReporting.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FormReporting.Controllers.Identity
{
    /// <summary>
    /// Handles user authentication (login/logout)
    /// </summary>
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAuthenticationService authService,
            ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Display login page
        /// </summary>
        [HttpGet("Login")]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            // If already authenticated, redirect to dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Handle login submission (WF-2.6)
        /// </summary>
        [HttpPost("Login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 1. Get user
                var user = await _authService.GetUserByUsernameOrEmailAsync(model.UserName);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                    await LogFailedLoginAsync(model.UserName, GetClientIpAddress());
                    return View(model);
                }

                // 2. Check IsActive
                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Your account has been deactivated. Please contact administrator.");
                    _logger.LogWarning("Inactive user attempted login: {UserName}", model.UserName);
                    return View(model);
                }

                // 3. Check lockout
                if (await _authService.IsAccountLockedAsync(user))
                {
                    var lockoutEnd = user.LockoutEnd?.LocalDateTime;
                    ModelState.AddModelError("", $"Account is locked until {lockoutEnd:yyyy-MM-dd HH:mm}. Too many failed login attempts.");
                    _logger.LogWarning("Locked user attempted login: {UserName}", model.UserName);
                    return View(model);
                }

                // 4. Validate password and sign in
                var result = await _authService.LoginAsync(model.UserName, model.Password, model.RememberMe);

                if (result.Succeeded)
                {
                    // Success!
                    await _authService.UpdateLastLoginAsync(user.UserId);
                    await _authService.ResetAccessFailedCountAsync(user);
                    await LogSuccessfulLoginAsync(user.UserId, GetClientIpAddress());

                    _logger.LogInformation("User logged in: {UserName}", model.UserName);

                    return RedirectToLocal(returnUrl);
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "Account locked due to multiple failed login attempts.");
                    return View(model);
                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError("", "Login not allowed. Please contact administrator.");
                    return View(model);
                }
                else
                {
                    // Failed login
                    await _authService.IncrementAccessFailedCountAsync(user);

                    // Check if now locked
                    if (user.AccessFailedCount >= 5)
                    {
                        user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(30);
                        await LogAccountLockedAsync(user.UserId);

                        ModelState.AddModelError("", "Account locked due to multiple failed login attempts. Locked for 30 minutes.");
                        _logger.LogWarning("Account locked: {UserName}", model.UserName);
                    }
                    else
                    {
                        var remaining = 5 - user.AccessFailedCount;
                        ModelState.AddModelError("", $"Invalid login attempt. {remaining} attempts remaining.");
                    }

                    await LogFailedLoginAsync(model.UserName, GetClientIpAddress());
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {UserName}", model.UserName);
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// Handle logout (supports both GET and POST)
        /// GET is used for navbar links, POST for form submissions
        /// </summary>
        [HttpGet("Logout")]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);

            await _authService.LogoutAsync();

            if (!string.IsNullOrEmpty(userId))
            {
                await LogLogoutAsync(int.Parse(userId));
                _logger.LogInformation("User logged out: {UserName}", userName);
            }

            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Access denied page
        /// </summary>
        [HttpGet("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helper Methods

        /// <summary>
        /// Redirect to local URL or default to dashboard
        /// </summary>
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// Get client IP address
        /// </summary>
        private string? GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        /// <summary>
        /// Log successful login (placeholder for audit logging)
        /// </summary>
        private Task LogSuccessfulLoginAsync(int userId, string? ipAddress)
        {
            // TODO: Implement audit logging (Step 11)
            _logger.LogInformation("Successful login - UserId: {UserId}, IP: {IpAddress}", userId, ipAddress);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log failed login attempt (placeholder for audit logging)
        /// </summary>
        private Task LogFailedLoginAsync(string userName, string? ipAddress)
        {
            // TODO: Implement audit logging (Step 11)
            _logger.LogWarning("Failed login attempt - UserName: {UserName}, IP: {IpAddress}", userName, ipAddress);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log account locked event (placeholder for audit logging)
        /// </summary>
        private Task LogAccountLockedAsync(int userId)
        {
            // TODO: Implement audit logging (Step 11)
            _logger.LogWarning("Account locked - UserId: {UserId}", userId);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log logout event (placeholder for audit logging)
        /// </summary>
        private Task LogLogoutAsync(int userId)
        {
            // TODO: Implement audit logging (Step 11)
            _logger.LogInformation("User logout - UserId: {UserId}", userId);
            return Task.CompletedTask;
        }

        #endregion
    }
}

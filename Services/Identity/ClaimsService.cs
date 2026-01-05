using FormReporting.Data;
using FormReporting.Models.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FormReporting.Services.Identity
{
    /// <summary>
    /// Implementation of claims building and management (WF-2.7)
    /// </summary>
    public class ClaimsService : IClaimsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Build all claims for a user (WF-2.7)
        /// </summary>
        public async Task<List<Claim>> BuildUserClaimsAsync(User user)
        {
            var claims = new List<Claim>();

            // 1. Identity Claims
            claims.AddRange(await GetIdentityClaimsAsync(user));

            // 2. Role Claims
            claims.AddRange(await GetRoleClaimsAsync(user));

            // 3. Permission Claims
            claims.AddRange(await GetPermissionClaimsAsync(user));

            // 4. Scope Claims
            claims.AddRange(await GetScopeClaimsAsync(user));

            // 5. Tenant Access Claims
            claims.AddRange(await GetTenantAccessClaimsAsync(user));

            return claims;
        }

        /// <summary>
        /// Get identity claims (UserId, UserName, Email, FullName, etc.)
        /// </summary>
        public async Task<List<Claim>> GetIdentityClaimsAsync(User user)
        {
            // Load user with primary tenant and optional department
            var userWithRelations = await _context.Users
                .Include(u => u.PrimaryTenant)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName)
            };

            // Add employee number if exists
            if (!string.IsNullOrEmpty(user.EmployeeNumber))
            {
                claims.Add(new Claim("EmployeeNumber", user.EmployeeNumber));
            }

            // Add primary tenant information (ALWAYS exists - required field)
            if (userWithRelations?.PrimaryTenant != null)
            {
                claims.Add(new Claim("PrimaryTenantId", userWithRelations.TenantId.ToString()));
                claims.Add(new Claim("TenantName", userWithRelations.PrimaryTenant.TenantName));

                // Add region if tenant has one
                if (userWithRelations.PrimaryTenant.RegionId.HasValue)
                {
                    claims.Add(new Claim("RegionId", userWithRelations.PrimaryTenant.RegionId.Value.ToString()));
                }
            }

            // Add department information if user is assigned to one (optional)
            if (userWithRelations?.Department != null)
            {
                claims.Add(new Claim("DepartmentId", userWithRelations.Department.DepartmentId.ToString()));
                claims.Add(new Claim("DepartmentName", userWithRelations.Department.DepartmentName));
            }

            return claims;
        }

        /// <summary>
        /// Get role claims from UserRoles
        /// </summary>
        public async Task<List<Claim>> GetRoleClaimsAsync(User user)
        {
            var roles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == user.UserId && ur.Role.IsActive)
                .Select(ur => ur.Role)
                .ToListAsync();

            var claims = new List<Claim>();

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
                claims.Add(new Claim("RoleId", role.RoleId.ToString()));
            }

            return claims;
        }

        /// <summary>
        /// Get permission claims from RolePermissions
        /// </summary>
        public async Task<List<Claim>> GetPermissionClaimsAsync(User user)
        {
            // Get all permissions via: UserRoles → RolePermissions → Permissions
            var permissions = await _context.UserRoles
                .Where(ur => ur.UserId == user.UserId && ur.Role.IsActive)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Where(rp => rp.Permission.IsActive)
                .Select(rp => rp.Permission.PermissionCode)
                .Distinct()
                .ToListAsync();

            var claims = new List<Claim>();

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            return claims;
        }

        /// <summary>
        /// Get scope claims from user's roles (using ScopeLevel model)
        /// </summary>
        public async Task<List<Claim>> GetScopeClaimsAsync(User user)
        {
            // Get the highest scope level (lowest Level number = broadest access)
            var highestScope = await _context.UserRoles
                .Where(ur => ur.UserId == user.UserId && ur.Role.IsActive)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.ScopeLevel)
                .Select(ur => ur.Role.ScopeLevel)
                .OrderBy(sl => sl.Level)  // Lower level = broader access
                .FirstOrDefaultAsync();

            var claims = new List<Claim>();

            if (highestScope != null)
            {
                claims.Add(new Claim("ScopeLevel", highestScope.Level.ToString()));
                claims.Add(new Claim("ScopeName", highestScope.ScopeName));
                claims.Add(new Claim("ScopeCode", highestScope.ScopeCode));
            }

            return claims;
        }

        /// <summary>
        /// Get tenant access claims based on ScopeLevel and UserTenantAccess
        /// Uses ScopeCode to determine access pattern (dynamic, not hardcoded)
        /// </summary>
        public async Task<List<Claim>> GetTenantAccessClaimsAsync(User user)
        {
            var claims = new List<Claim>();

            // Get user with primary tenant
            var userWithTenant = await _context.Users
                .Include(u => u.PrimaryTenant)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            // Get highest scope level (lowest Level number = broadest access)
            var highestScope = await _context.UserRoles
                .Where(ur => ur.UserId == user.UserId && ur.Role.IsActive)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.ScopeLevel)
                .Select(ur => ur.Role.ScopeLevel)
                .OrderBy(sl => sl.Level)
                .FirstOrDefaultAsync();

            if (highestScope != null && userWithTenant != null)
            {
                // Use ScopeCode to determine access pattern (dynamic approach)
                switch (highestScope.ScopeCode.ToUpper())
                {
                    case "GLOBAL":
                        // Access to all tenants
                        claims.Add(new Claim("TenantAccess", "*"));
                        break;

                    case "REGIONAL":
                        // Access to tenants in user's region
                        if (userWithTenant.PrimaryTenant?.RegionId != null)
                        {
                            claims.Add(new Claim("TenantAccess", $"Region:{userWithTenant.PrimaryTenant.RegionId}"));
                        }
                        break;

                    case "TENANT":
                        // Access to user's primary tenant only
                        claims.Add(new Claim("TenantAccess", $"Tenant:{userWithTenant.TenantId}"));
                        break;

                    case "DEPARTMENT":
                    case "DEPT_GROUP":
                        // Access limited to user's department
                        if (userWithTenant.DepartmentId != null)
                        {
                            claims.Add(new Claim("TenantAccess", $"Department:{userWithTenant.DepartmentId}"));
                        }
                        else
                        {
                            // If no department, default to tenant level
                            claims.Add(new Claim("TenantAccess", $"Tenant:{userWithTenant.TenantId}"));
                        }
                        break;

                    case "TEAM":
                        // Access limited to user's team (tenant level)
                        claims.Add(new Claim("TenantAccess", $"Tenant:{userWithTenant.TenantId}"));
                        break;

                    case "INDIVIDUAL":
                        // Access to user's own data only
                        claims.Add(new Claim("TenantAccess", $"User:{user.UserId}"));
                        break;

                    default:
                        // Unknown scope code - default to most restrictive (individual)
                        claims.Add(new Claim("TenantAccess", $"User:{user.UserId}"));
                        break;
                }
            }

            // Add UserTenantAccess exceptions (temporary/project-based access)
            var tenantAccessExceptions = await _context.UserTenantAccesses
                .Where(uta => uta.UserId == user.UserId 
                    && uta.IsActive 
                    && (uta.ExpiryDate == null || uta.ExpiryDate > DateTime.UtcNow))
                .Select(uta => uta.TenantId)
                .ToListAsync();

            foreach (var tenantId in tenantAccessExceptions)
            {
                claims.Add(new Claim("TenantAccessException", tenantId.ToString()));
            }

            return claims;
        }

        /// <summary>
        /// Invalidate cached claims for a user (placeholder for future caching implementation)
        /// </summary>
        public Task InvalidateUserClaimsCacheAsync(int userId)
        {
            // TODO: Implement claims caching and invalidation
            // For now, claims are built fresh on each login
            return Task.CompletedTask;
        }

        #region Current User Helpers

        /// <summary>
        /// Get the current user's ID from HttpContext
        /// </summary>
        public int GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return 0;

            // Try standard ASP.NET Core claim first, then fallback to custom UserId claim
            var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                           ?? user.FindFirst("UserId")?.Value;
            
            if (userIdClaim != null && int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return 0;
        }

        /// <summary>
        /// Get the current user's full name from HttpContext
        /// </summary>
        public string GetUserFullName()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("FullName")?.Value ?? string.Empty;
        }

        /// <summary>
        /// Get the client IP address from HttpContext
        /// </summary>
        public string? GetClientIP()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            // Try to get forwarded IP first (for proxies/load balancers)
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').First().Trim();
            }

            return httpContext.Connection.RemoteIpAddress?.ToString();
        }

        /// <summary>
        /// Check if current user has a specific role
        /// </summary>
        public bool HasRole(string roleName)
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(roleName) ?? false;
        }

        /// <summary>
        /// Check if current user has a specific permission
        /// </summary>
        public bool HasPermission(string permissionCode)
        {
            return _httpContextAccessor.HttpContext?.User?.HasClaim("Permission", permissionCode) ?? false;
        }

        #endregion
    }
}

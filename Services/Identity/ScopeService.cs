using FormReporting.Data;
using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FormReporting.Services.Identity
{
    /// <summary>
    /// Implementation of scope and access control operations
    /// </summary>
    public class ScopeService : IScopeService
    {
        private readonly ApplicationDbContext _context;

        public ScopeService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get complete scope information for a user
        /// </summary>
        public async Task<UserScope> GetUserScopeAsync(ClaimsPrincipal user)
        {
            var userScope = new UserScope
            {
                UserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                UserName = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                ScopeName = user.FindFirst("ScopeName")?.Value ?? string.Empty,
                ScopeCode = user.FindFirst("ScopeCode")?.Value ?? string.Empty
            };

            // Parse scope level
            if (int.TryParse(user.FindFirst("ScopeLevel")?.Value, out int level))
            {
                userScope.Level = level;
            }

            // Parse primary tenant ID
            if (int.TryParse(user.FindFirst("PrimaryTenantId")?.Value, out int tenantId))
            {
                userScope.PrimaryTenantId = tenantId;
            }

            // Parse region ID
            if (int.TryParse(user.FindFirst("RegionId")?.Value, out int regionId))
            {
                userScope.RegionId = regionId;
            }

            // Parse department ID
            if (int.TryParse(user.FindFirst("DepartmentId")?.Value, out int deptId))
            {
                userScope.DepartmentId = deptId;
            }

            // Get roles
            userScope.Roles = user.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Get permissions
            userScope.Permissions = user.FindAll("Permission")
                .Select(c => c.Value)
                .ToList();

            // Get accessible tenant IDs
            userScope.AccessibleTenantIds = await GetAccessibleTenantIdsAsync(user);

            // Get tenant access exceptions
            userScope.TenantAccessExceptions = user.FindAll("TenantAccessException")
                .Select(c => int.Parse(c.Value))
                .ToList();

            return userScope;
        }

        /// <summary>
        /// Get list of tenant IDs the user can access (scope + exceptions)
        /// </summary>
        public async Task<List<int>> GetAccessibleTenantIdsAsync(ClaimsPrincipal user)
        {
            var tenantIds = new List<int>();

            // Get scope code from claims
            var scopeCode = user.FindFirst("ScopeCode")?.Value?.ToUpper();
            var tenantAccessClaim = user.FindFirst("TenantAccess")?.Value;

            if (string.IsNullOrEmpty(scopeCode) || string.IsNullOrEmpty(tenantAccessClaim))
            {
                return tenantIds;
            }

            // Calculate scope-based access
            switch (scopeCode)
            {
                case "GLOBAL":
                    // Access to ALL tenants
                    tenantIds = await _context.Tenants
                        .Where(t => t.IsActive)
                        .Select(t => t.TenantId)
                        .ToListAsync();
                    break;

                case "REGIONAL":
                    // Access to all tenants in user's region
                    if (tenantAccessClaim.StartsWith("Region:"))
                    {
                        var regionIdStr = tenantAccessClaim.Replace("Region:", "");
                        if (int.TryParse(regionIdStr, out int regionId))
                        {
                            tenantIds = await _context.Tenants
                                .Where(t => t.RegionId == regionId && t.IsActive)
                                .Select(t => t.TenantId)
                                .ToListAsync();
                        }
                    }
                    break;

                case "TENANT":
                    // Access to user's primary tenant only
                    if (tenantAccessClaim.StartsWith("Tenant:"))
                    {
                        var tenantIdStr = tenantAccessClaim.Replace("Tenant:", "");
                        if (int.TryParse(tenantIdStr, out int tenantId))
                        {
                            tenantIds.Add(tenantId);
                        }
                    }
                    break;

                case "DEPARTMENT":
                case "DEPT_GROUP":
                    // Access to user's department (tenant level)
                    if (tenantAccessClaim.StartsWith("Department:"))
                    {
                        var deptIdStr = tenantAccessClaim.Replace("Department:", "");
                        if (int.TryParse(deptIdStr, out int deptId))
                        {
                            // Get tenant ID from department
                            var tenantId = await _context.Departments
                                .Where(d => d.DepartmentId == deptId)
                                .Select(d => d.TenantId)
                                .FirstOrDefaultAsync();
                            
                            if (tenantId > 0)
                            {
                                tenantIds.Add(tenantId);
                            }
                        }
                    }
                    break;

                case "TEAM":
                    // Access to user's tenant (team level)
                    if (tenantAccessClaim.StartsWith("Tenant:"))
                    {
                        var tenantIdStr = tenantAccessClaim.Replace("Tenant:", "");
                        if (int.TryParse(tenantIdStr, out int tenantId))
                        {
                            tenantIds.Add(tenantId);
                        }
                    }
                    break;

                case "INDIVIDUAL":
                    // Access to user's own data only (no tenant-wide access)
                    // Individual scope typically doesn't grant tenant access
                    break;
            }

            // Add UserTenantAccess exceptions
            var exceptions = user.FindAll("TenantAccessException")
                .Select(c => int.Parse(c.Value))
                .ToList();

            tenantIds.AddRange(exceptions);

            // Return distinct tenant IDs
            return tenantIds.Distinct().ToList();
        }

        /// <summary>
        /// Check if user has access to a specific tenant
        /// </summary>
        public async Task<bool> HasAccessToTenantAsync(ClaimsPrincipal user, int tenantId)
        {
            var accessibleTenantIds = await GetAccessibleTenantIdsAsync(user);
            return accessibleTenantIds.Contains(tenantId);
        }

        /// <summary>
        /// Check if user has a specific permission
        /// </summary>
        public bool HasPermission(ClaimsPrincipal user, string permissionCode)
        {
            return user.HasClaim("Permission", permissionCode);
        }

        /// <summary>
        /// Check if user has a specific role
        /// </summary>
        public bool HasRole(ClaimsPrincipal user, string roleName)
        {
            return user.IsInRole(roleName);
        }

        /// <summary>
        /// Get user's scope level from claims
        /// </summary>
        public async Task<ScopeLevel?> GetUserScopeLevelAsync(ClaimsPrincipal user)
        {
            var scopeCode = user.FindFirst("ScopeCode")?.Value;
            
            if (string.IsNullOrEmpty(scopeCode))
            {
                return null;
            }

            return await _context.ScopeLevels
                .FirstOrDefaultAsync(sl => sl.ScopeCode == scopeCode);
        }
    }
}

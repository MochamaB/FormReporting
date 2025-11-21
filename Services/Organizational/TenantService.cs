using FormReporting.Data;
using FormReporting.Models.Entities.Organizational;
using FormReporting.Services.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FormReporting.Services.Organizational
{
    /// <summary>
    /// Implementation of tenant operations with scope-based access control
    /// Filters tenants based on user's scope (GLOBAL, REGIONAL, TENANT, DEPARTMENT)
    /// </summary>
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;
        private readonly IScopeService _scopeService;

        public TenantService(ApplicationDbContext context, IScopeService scopeService)
        {
            _context = context;
            _scopeService = scopeService;
        }

        /// <summary>
        /// Get all tenants the current user can access based on their scope
        /// </summary>
        public async Task<List<Tenant>> GetAccessibleTenantsAsync(ClaimsPrincipal currentUser, string? searchQuery = null)
        {
            // Get user's scope information
            var scope = await _scopeService.GetUserScopeAsync(currentUser);
            var scopeCode = scope.ScopeCode?.ToUpper();

            if (string.IsNullOrEmpty(scopeCode))
            {
                return new List<Tenant>();
            }

            IQueryable<Tenant> query = _context.Tenants
                .Include(t => t.Region)
                .Include(t => t.Departments)
                .Where(t => t.IsActive);

            // Apply scope-based filtering
            switch (scopeCode)
            {
                case "GLOBAL":
                    // Global scope - access to all active tenants
                    break;

                case "REGIONAL":
                    // Regional scope - only tenants in user's region
                    if (scope.RegionId.HasValue)
                    {
                        query = query.Where(t => t.RegionId == scope.RegionId.Value);
                    }
                    else
                    {
                        return new List<Tenant>();
                    }
                    break;

                case "TENANT":
                case "TEAM":
                    // Tenant scope - only user's primary tenant
                    if (scope.PrimaryTenantId.HasValue)
                    {
                        query = query.Where(t => t.TenantId == scope.PrimaryTenantId.Value);
                    }
                    else
                    {
                        return new List<Tenant>();
                    }
                    break;

                case "DEPARTMENT":
                case "DEPT_GROUP":
                    // Department scope - user's department's tenant
                    if (scope.PrimaryTenantId.HasValue)
                    {
                        query = query.Where(t => t.TenantId == scope.PrimaryTenantId.Value);
                    }
                    else
                    {
                        return new List<Tenant>();
                    }
                    break;

                case "INDIVIDUAL":
                    // Individual scope - same as department (user's tenant)
                    if (scope.PrimaryTenantId.HasValue)
                    {
                        query = query.Where(t => t.TenantId == scope.PrimaryTenantId.Value);
                    }
                    else
                    {
                        return new List<Tenant>();
                    }
                    break;

                default:
                    // Unknown scope - no access
                    return new List<Tenant>();
            }

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                query = query.Where(t =>
                    t.TenantName.ToLower().Contains(searchLower) ||
                    t.TenantCode.ToLower().Contains(searchLower) ||
                    (t.Location != null && t.Location.ToLower().Contains(searchLower)));
            }

            return await query
                .OrderBy(t => t.Region.RegionName)
                .ThenBy(t => t.TenantName)
                .ToListAsync();
        }

        /// <summary>
        /// Get a specific tenant by ID if user has access
        /// </summary>
        public async Task<Tenant?> GetTenantByIdAsync(ClaimsPrincipal currentUser, int tenantId)
        {
            var accessibleTenants = await GetAccessibleTenantsAsync(currentUser);
            return accessibleTenants.FirstOrDefault(t => t.TenantId == tenantId);
        }

        /// <summary>
        /// Check if current user can access specified tenant
        /// </summary>
        public async Task<bool> CanAccessTenantAsync(ClaimsPrincipal currentUser, int tenantId)
        {
            var tenant = await GetTenantByIdAsync(currentUser, tenantId);
            return tenant != null;
        }

        /// <summary>
        /// Get tenants grouped by region (for dropdowns and filters)
        /// </summary>
        public async Task<Dictionary<string, List<Tenant>>> GetTenantsGroupedByRegionAsync(ClaimsPrincipal currentUser)
        {
            var tenants = await GetAccessibleTenantsAsync(currentUser);

            return tenants
                .GroupBy(t => t.Region?.RegionName ?? "No Region")
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(t => t.TenantName).ToList()
                );
        }
    }
}

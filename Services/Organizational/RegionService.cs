using FormReporting.Data;
using FormReporting.Models.Entities.Organizational;
using FormReporting.Services.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FormReporting.Services.Organizational
{
    /// <summary>
    /// Implementation of region operations with scope-based access control
    /// Filters regions based on user's scope (GLOBAL, REGIONAL, TENANT, DEPARTMENT)
    /// </summary>
    public class RegionService : IRegionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IScopeService _scopeService;

        public RegionService(ApplicationDbContext context, IScopeService scopeService)
        {
            _context = context;
            _scopeService = scopeService;
        }

        /// <summary>
        /// Get all regions the current user can access based on their scope
        /// </summary>
        public async Task<List<Region>> GetAccessibleRegionsAsync(ClaimsPrincipal currentUser)
        {
            // Get user's scope information
            var scope = await _scopeService.GetUserScopeAsync(currentUser);
            var scopeCode = scope.ScopeCode?.ToUpper();

            if (string.IsNullOrEmpty(scopeCode))
            {
                return new List<Region>();
            }

            IQueryable<Region> query = _context.Regions
                .Include(r => r.Tenants)
                .Where(r => r.IsActive);

            // Apply scope-based filtering
            switch (scopeCode)
            {
                case "GLOBAL":
                    // Global scope - access to all active regions
                    break;

                case "REGIONAL":
                    // Regional scope - only user's region
                    if (scope.RegionId.HasValue)
                    {
                        query = query.Where(r => r.RegionId == scope.RegionId.Value);
                    }
                    else
                    {
                        return new List<Region>();
                    }
                    break;

                case "TENANT":
                case "TEAM":
                    // Tenant scope - get region from user's tenant
                    if (scope.PrimaryTenantId.HasValue)
                    {
                        query = query.Where(r => r.Tenants.Any(t => t.TenantId == scope.PrimaryTenantId.Value));
                    }
                    else
                    {
                        return new List<Region>();
                    }
                    break;

                case "DEPARTMENT":
                case "DEPT_GROUP":
                    // Department scope - get region from department's tenant
                    if (scope.PrimaryTenantId.HasValue)
                    {
                        query = query.Where(r => r.Tenants.Any(t => t.TenantId == scope.PrimaryTenantId.Value));
                    }
                    else
                    {
                        return new List<Region>();
                    }
                    break;

                case "INDIVIDUAL":
                    // Individual scope - same as department (user's tenant's region)
                    if (scope.PrimaryTenantId.HasValue)
                    {
                        query = query.Where(r => r.Tenants.Any(t => t.TenantId == scope.PrimaryTenantId.Value));
                    }
                    else
                    {
                        return new List<Region>();
                    }
                    break;

                default:
                    // Unknown scope - no access
                    return new List<Region>();
            }

            return await query
                .OrderBy(r => r.RegionName)
                .ToListAsync();
        }

        /// <summary>
        /// Get a specific region by ID if user has access
        /// </summary>
        public async Task<Region?> GetRegionByIdAsync(ClaimsPrincipal currentUser, int regionId)
        {
            var accessibleRegions = await GetAccessibleRegionsAsync(currentUser);
            return accessibleRegions.FirstOrDefault(r => r.RegionId == regionId);
        }

        /// <summary>
        /// Get tenants in a specific region (filtered by user scope)
        /// </summary>
        public async Task<List<Tenant>> GetTenantsByRegionAsync(ClaimsPrincipal currentUser, int regionId)
        {
            // First check if user can access the region
            var region = await GetRegionByIdAsync(currentUser, regionId);
            if (region == null)
            {
                return new List<Tenant>();
            }

            // Get accessible tenants (already filtered by scope)
            var accessibleTenants = await _context.Tenants
                .Include(t => t.Region)
                .Include(t => t.Departments)
                .Where(t => t.IsActive && t.RegionId == regionId)
                .OrderBy(t => t.TenantName)
                .ToListAsync();

            return accessibleTenants;
        }

        /// <summary>
        /// Check if current user can access specified region
        /// </summary>
        public async Task<bool> CanAccessRegionAsync(ClaimsPrincipal currentUser, int regionId)
        {
            var region = await GetRegionByIdAsync(currentUser, regionId);
            return region != null;
        }

        /// <summary>
        /// Get regions grouped by region code for dropdowns
        /// </summary>
        public async Task<Dictionary<string, List<Region>>> GetRegionsGroupedByCodeAsync(ClaimsPrincipal currentUser)
        {
            var regions = await GetAccessibleRegionsAsync(currentUser);

            return regions
                .GroupBy(r => r.RegionCode)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(r => r.RegionName).ToList()
                );
        }
    }
}

using FormReporting.Data;
using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FormReporting.Services.Identity
{
    /// <summary>
    /// Service implementation for user operations with scope-based access control
    /// Uses the same scope logic as ScopeService to determine user access
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IScopeService _scopeService;

        public UserService(ApplicationDbContext context, IScopeService scopeService)
        {
            _context = context;
            _scopeService = scopeService;
        }

        /// <summary>
        /// Get users accessible to the current user based on their scope
        /// Uses ScopeService to determine accessible tenants, then filters users accordingly
        /// </summary>
        public async Task<List<User>> GetAccessibleUsersAsync(ClaimsPrincipal currentUser, string? searchQuery = null)
        {
            // Get current user's scope information
            var scope = await _scopeService.GetUserScopeAsync(currentUser);
            var scopeCode = scope.ScopeCode?.ToUpper();

            if (string.IsNullOrEmpty(scopeCode))
            {
                return new List<User>();
            }

            IQueryable<User> query = _context.Users
                .Include(u => u.PrimaryTenant)
                .Include(u => u.Department)
                .Where(u => u.IsActive);

            // Apply scope-based filtering using the scope information
            switch (scopeCode)
            {
                case "GLOBAL":
                    // Global scope - access to all active users
                    break;

                case "REGIONAL":
                    // Regional scope - users in accessible tenants (via ScopeService)
                    var regionalTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(currentUser);
                    if (regionalTenantIds.Any())
                    {
                        query = query.Where(u => regionalTenantIds.Contains(u.TenantId));
                    }
                    else
                    {
                        return new List<User>(); // No accessible tenants
                    }
                    break;

                case "TENANT":
                case "TEAM":
                    // Tenant/Team scope - users in accessible tenants
                    var tenantIds = await _scopeService.GetAccessibleTenantIdsAsync(currentUser);
                    if (tenantIds.Any())
                    {
                        query = query.Where(u => tenantIds.Contains(u.TenantId));
                    }
                    else
                    {
                        return new List<User>();
                    }
                    break;

                case "DEPARTMENT":
                case "DEPT_GROUP":
                    // Department scope - users in same department
                    if (scope.DepartmentId.HasValue)
                    {
                        query = query.Where(u => u.DepartmentId == scope.DepartmentId.Value);
                    }
                    else
                    {
                        return new List<User>();
                    }
                    break;

                case "INDIVIDUAL":
                    // Individual scope - only self
                    query = query.Where(u => u.UserId == scope.UserId);
                    break;

                default:
                    // Unknown scope - no access
                    return new List<User>();
            }

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(searchLower) ||
                    u.LastName.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower) ||
                    u.UserName.ToLower().Contains(searchLower) ||
                    (u.EmployeeNumber != null && u.EmployeeNumber.ToLower().Contains(searchLower)));
            }

            return await query
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        /// <summary>
        /// Search users by name, email, or username within current user's scope
        /// </summary>
        public async Task<List<User>> SearchUsersAsync(ClaimsPrincipal currentUser, string query, int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return new List<User>();
            }

            var users = await GetAccessibleUsersAsync(currentUser, query);
            return users.Take(limit).ToList();
        }

        /// <summary>
        /// Get user by ID if accessible to current user
        /// </summary>
        public async Task<User?> GetUserByIdAsync(ClaimsPrincipal currentUser, int userId)
        {
            var accessibleUsers = await GetAccessibleUsersAsync(currentUser);
            return accessibleUsers.FirstOrDefault(u => u.UserId == userId);
        }

        /// <summary>
        /// Check if current user can access specified user
        /// </summary>
        public async Task<bool> CanAccessUserAsync(ClaimsPrincipal currentUser, int targetUserId)
        {
            var user = await GetUserByIdAsync(currentUser, targetUserId);
            return user != null;
        }

        /// <summary>
        /// Get accessible users grouped by tenant (for bulk selection UIs like AssignmentManager)
        /// Returns anonymous objects formatted for AJAX responses with tenant grouping
        /// </summary>
        public async Task<List<object>> GetUsersGroupedByTenantAsync(ClaimsPrincipal currentUser, string? searchQuery = null)
        {
            // Get accessible users with scope filtering
            var users = await GetAccessibleUsersAsync(currentUser, searchQuery);

            // Group by tenant and return structured data
            var groupedUsers = users
                .GroupBy(u => new { u.TenantId, TenantName = u.PrimaryTenant?.TenantName ?? "No Tenant" })
                .OrderBy(g => g.Key.TenantName)
                .Select(g => new
                {
                    tenantId = g.Key.TenantId,
                    tenantName = g.Key.TenantName,
                    userCount = g.Count(),
                    users = g.Select(u => new
                    {
                        userId = u.UserId,
                        fullName = u.FullName,
                        email = u.Email,
                        employeeNumber = u.EmployeeNumber ?? "",
                        departmentName = u.Department?.DepartmentName ?? "No Department"
                    }).ToList()
                })
                .Cast<object>()
                .ToList();

            return groupedUsers;
        }
    }
}

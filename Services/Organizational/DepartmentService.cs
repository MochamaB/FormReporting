using FormReporting.Data;
using FormReporting.Models.Entities.Organizational;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FormReporting.Services.Organizational
{
    /// <summary>
    /// Service for department-related operations with scope-based access control
    /// </summary>
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        public DepartmentService(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        /// <summary>
        /// Get all departments for a specific tenant
        /// </summary>
        public async Task<List<Department>> GetDepartmentsByTenantAsync(int tenantId)
        {
            return await _context.Departments
                .Include(d => d.ParentDepartment)
                .Include(d => d.ChildDepartments)
                .Where(d => d.TenantId == tenantId)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
        }

        /// <summary>
        /// Get all departments the current user can access based on their scope
        /// </summary>
        public async Task<List<Department>> GetAccessibleDepartmentsAsync(ClaimsPrincipal currentUser, string? searchQuery = null)
        {
            // Get accessible tenants first (respects user scope)
            var accessibleTenants = await _tenantService.GetAccessibleTenantsAsync(currentUser);
            var tenantIds = accessibleTenants.Select(t => t.TenantId).ToList();

            var query = _context.Departments
                .Include(d => d.Tenant)
                .Include(d => d.ParentDepartment)
                .Where(d => tenantIds.Contains(d.TenantId));

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim().ToLower();
                query = query.Where(d =>
                    d.DepartmentName.ToLower().Contains(searchQuery) ||
                    d.DepartmentCode.ToLower().Contains(searchQuery) ||
                    (d.Description != null && d.Description.ToLower().Contains(searchQuery)));
            }

            return await query
                .OrderBy(d => d.Tenant.TenantName)
                .ThenBy(d => d.DepartmentName)
                .ToListAsync();
        }

        /// <summary>
        /// Get a specific department by ID with all related data
        /// </summary>
        public async Task<Department?> GetDepartmentByIdAsync(int departmentId)
        {
            return await _context.Departments
                .Include(d => d.Tenant)
                .Include(d => d.ParentDepartment)
                .Include(d => d.ChildDepartments)
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
        }

        /// <summary>
        /// Check if department can be deleted (WF-1.9: Delete Department with Users Check)
        /// </summary>
        public async Task<bool> CanDeleteDepartmentAsync(int departmentId)
        {
            // Check if department has users assigned
            var hasUsers = await _context.Users.AnyAsync(u => u.DepartmentId == departmentId);
            return !hasUsers;
        }

        /// <summary>
        /// Get department hierarchy for a tenant (WF-1.8: Department Hierarchy View)
        /// Returns only root departments with their children loaded
        /// </summary>
        public async Task<List<Department>> GetDepartmentHierarchyAsync(int tenantId)
        {
            // Get all departments for tenant
            var allDepartments = await _context.Departments
                .Include(d => d.ChildDepartments)
                    .ThenInclude(cd => cd.ChildDepartments)
                .Where(d => d.TenantId == tenantId)
                .ToListAsync();

            // Return only root departments (ParentDepartmentId is null)
            // Children are already loaded via Include
            return allDepartments
                .Where(d => d.ParentDepartmentId == null)
                .OrderBy(d => d.DepartmentName)
                .ToList();
        }

        /// <summary>
        /// Get departments grouped by tenant
        /// </summary>
        public async Task<Dictionary<int, List<Department>>> GetDepartmentsGroupedByTenantAsync(ClaimsPrincipal currentUser)
        {
            var departments = await GetAccessibleDepartmentsAsync(currentUser);
            return departments
                .GroupBy(d => d.TenantId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}

using FormReporting.Models.Entities.Organizational;
using System.Security.Claims;

namespace FormReporting.Services.Organizational
{
    /// <summary>
    /// Interface for department-related operations with scope-based access control
    /// </summary>
    public interface IDepartmentService
    {
        /// <summary>
        /// Get all departments for a specific tenant
        /// </summary>
        Task<List<Department>> GetDepartmentsByTenantAsync(int tenantId);

        /// <summary>
        /// Get all departments the current user can access based on their scope
        /// </summary>
        Task<List<Department>> GetAccessibleDepartmentsAsync(ClaimsPrincipal currentUser, string? searchQuery = null);

        /// <summary>
        /// Get a specific department by ID with related data
        /// </summary>
        Task<Department?> GetDepartmentByIdAsync(int departmentId);

        /// <summary>
        /// Check if a department can be deleted (no users assigned)
        /// </summary>
        Task<bool> CanDeleteDepartmentAsync(int departmentId);

        /// <summary>
        /// Get department hierarchy for a tenant (root departments with children)
        /// </summary>
        Task<List<Department>> GetDepartmentHierarchyAsync(int tenantId);

        /// <summary>
        /// Get departments grouped by tenant
        /// </summary>
        Task<Dictionary<int, List<Department>>> GetDepartmentsGroupedByTenantAsync(ClaimsPrincipal currentUser);
    }
}

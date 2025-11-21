using FormReporting.Data;
using FormReporting.Models.ViewModels.Identity;
using FormReporting.Services.Identity;
using FormReporting.Services.Organizational;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Controllers.Identity
{
    /// <summary>
    /// Controller for managing users with scope-based access control
    /// </summary>
    [Route("Identity/[controller]")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly ITenantService _tenantService;

        public UsersController(ApplicationDbContext context, IUserService userService, ITenantService tenantService)
        {
            _context = context;
            _userService = userService;
            _tenantService = tenantService;
        }

        /// <summary>
        /// Display the users index page with statistics and datatable
        /// Uses UserService for scope-based filtering
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? search, string? status, string? tenant, int? page)
        {
            // 1. GET SCOPE-FILTERED USERS using UserService
            var scopedUsers = await _userService.GetAccessibleUsersAsync(User, search);

            // 2. APPLY STATUS FILTER
            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status.ToLower() == "active";
                scopedUsers = scopedUsers.Where(u => u.IsActive == isActive).ToList();
            }

            // 3. APPLY TENANT FILTER
            if (!string.IsNullOrEmpty(tenant) && int.TryParse(tenant, out int tenantId))
            {
                scopedUsers = scopedUsers.Where(u => u.TenantId == tenantId).ToList();
            }

            // 4. CALCULATE STATISTICS (from all accessible users, before pagination)
            var totalUsers = scopedUsers.Count;
            var activeUsers = scopedUsers.Count(u => u.IsActive);
            var inactiveUsers = scopedUsers.Count(u => !u.IsActive);
            var emailConfirmed = scopedUsers.Count(u => u.EmailConfirmed);

            // 5. PAGINATION
            var pageSize = 10;
            var totalRecords = scopedUsers.Count;
            var currentPage = page ?? 1;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var skip = (currentPage - 1) * pageSize;

            // 6. GET PAGINATED DATA
            var pagedUsers = scopedUsers
                .OrderBy(u => u.PrimaryTenant.TenantName)
                .ThenBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            // 7. BUILD VIEW MODEL
            var viewModel = new UsersIndexViewModel
            {
                Users = pagedUsers.Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserName = u.UserName,
                    Email = u.Email,
                    EmailConfirmed = u.EmailConfirmed,
                    PhoneNumber = u.PhoneNumber,
                    EmployeeNumber = u.EmployeeNumber,
                    TenantId = u.TenantId,
                    TenantName = u.PrimaryTenant?.TenantName ?? "N/A",
                    DepartmentName = u.Department?.DepartmentName ?? "No Department",
                    RoleCount = u.UserRoles.Count,
                    IsActive = u.IsActive,
                    LastLoginDate = u.LastLoginDate,
                    CreatedDate = u.CreatedDate
                }).ToList(),
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers,
                EmailConfirmedUsers = emailConfirmed
            };

            // 8. PASS PAGINATION AND FILTER DATA TO VIEW
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentTenant = tenant;

            // 9. GET ACCESSIBLE TENANTS FOR FILTER DROPDOWN (using TenantService)
            var accessibleTenants = await _tenantService.GetAccessibleTenantsAsync(User);
            var tenantOptions = accessibleTenants
                .Select(t => new { t.TenantId, t.TenantName })
                .OrderBy(t => t.TenantName)
                .ToList();
            ViewBag.AccessibleTenants = tenantOptions;

            return View("Views/Identity/Users/Index.cshtml",viewModel);
        }
    }
}

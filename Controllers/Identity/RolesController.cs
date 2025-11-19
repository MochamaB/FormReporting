using FormReporting.Data;
using FormReporting.Models.Entities.Identity;
using FormReporting.Models.ViewModels.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Controllers.Identity
{
    /// <summary>
    /// Controller for managing roles in the RBAC system
    /// </summary>
    [Route("Identity/[controller]")]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Display the roles index page with datatable
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? search, string? status, int? page)
        {
            var query = _context.Roles
                .Include(r => r.ScopeLevel)
                .Include(r => r.UserRoles)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => 
                    r.RoleName.Contains(search) || 
                    r.RoleCode.Contains(search) ||
                    r.Description!.Contains(search) ||
                    r.ScopeLevel.ScopeName.Contains(search));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status.ToLower() == "active";
                query = query.Where(r => r.IsActive == isActive);
            }

            // Get total counts for statistics
            var allRoles = await _context.Roles.ToListAsync();
            var totalRoles = allRoles.Count;
            var activeRoles = allRoles.Count(r => r.IsActive);
            var inactiveRoles = allRoles.Count(r => !r.IsActive);

            // Pagination
            var pageSize = 10;
            var totalRecords = await query.CountAsync();
            var currentPage = page ?? 1;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var skip = (currentPage - 1) * pageSize;

            var roles = await query
                .OrderBy(r => r.ScopeLevel.Level)
                .ThenBy(r => r.RoleName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new RolesIndexViewModel
            {
                Roles = roles.Select(r => new RoleViewModel
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    RoleCode = r.RoleCode,
                    Description = r.Description,
                    ScopeLevelName = r.ScopeLevel.ScopeName,
                    ScopeCode = r.ScopeLevel.ScopeCode,
                    Level = r.ScopeLevel.Level,
                    IsActive = r.IsActive,
                    UserCount = r.UserRoles.Count,
                    CreatedDate = r.CreatedDate
                }),
                TotalRoles = totalRoles,
                ActiveRoles = activeRoles,
                InactiveRoles = inactiveRoles
            };

            // Pass pagination data to view
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;

            return View("Views/Identity/Roles/Index.cshtml", viewModel);
        }

        /// <summary>
        /// API endpoint to get roles data for DataTables
        /// </summary>
        [HttpGet("GetRolesData")]
        public async Task<IActionResult> GetRolesData()
        {
            var roles = await _context.Roles
                .Include(r => r.ScopeLevel)
                .Include(r => r.UserRoles)
                .OrderBy(r => r.ScopeLevel.Level)
                .ThenBy(r => r.RoleName)
                .ToListAsync();

            var data = roles.Select(r => new RoleViewModel
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                RoleCode = r.RoleCode,
                Description = r.Description,
                ScopeLevelName = r.ScopeLevel.ScopeName,
                ScopeCode = r.ScopeLevel.ScopeCode,
                Level = r.ScopeLevel.Level,
                IsActive = r.IsActive,
                UserCount = r.UserRoles.Count,
                CreatedDate = r.CreatedDate
            });

            return Json(new { data });
        }

        /// <summary>
        /// Show create role page
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var model = new RoleEditViewModel
            {
                IsActive = true
            };

            // Load scope levels for dropdown
            ViewBag.ScopeLevels = await _context.ScopeLevels
                .Where(s => s.IsActive)
                .OrderBy(s => s.Level)
                .Select(s => new { s.ScopeLevelId, s.ScopeName, s.Level })
                .ToListAsync();

            return View("Views/Identity/Roles/Create.cshtml", model);
        }

        /// <summary>
        /// Handle create role submission
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate RoleCode
                if (await _context.Roles.AnyAsync(r => r.RoleCode == model.RoleCode))
                {
                    ModelState.AddModelError("RoleCode", "A role with this code already exists.");
                    ViewBag.ScopeLevels = await _context.ScopeLevels
                        .Where(s => s.IsActive)
                        .OrderBy(s => s.Level)
                        .Select(s => new { s.ScopeLevelId, s.ScopeName, s.Level })
                        .ToListAsync();
                    return View(model);
                }

                // Check for duplicate RoleName
                if (await _context.Roles.AnyAsync(r => r.RoleName == model.RoleName))
                {
                    ModelState.AddModelError("RoleName", "A role with this name already exists.");
                    ViewBag.ScopeLevels = await _context.ScopeLevels
                        .Where(s => s.IsActive)
                        .OrderBy(s => s.Level)
                        .Select(s => new { s.ScopeLevelId, s.ScopeName, s.Level })
                        .ToListAsync();
                    return View(model);
                }

                var role = new Role
                {
                    RoleName = model.RoleName,
                    RoleCode = model.RoleCode.ToUpper(),
                    Description = model.Description,
                    ScopeLevelId = model.ScopeLevelId,
                    IsActive = model.IsActive,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Role '{role.RoleName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ScopeLevels = await _context.ScopeLevels
                .Where(s => s.IsActive)
                .OrderBy(s => s.Level)
                .Select(s => new { s.ScopeLevelId, s.ScopeName, s.Level })
                .ToListAsync();
            return View("Views/Identity/Roles/Index.cshtml", model);
        }

        /// <summary>
        /// Show edit role page
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new RoleEditViewModel
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                RoleCode = role.RoleCode,
                Description = role.Description,
                ScopeLevelId = role.ScopeLevelId,
                IsActive = role.IsActive
            };

            ViewBag.ScopeLevels = await _context.ScopeLevels
                .Where(s => s.IsActive)
                .OrderBy(s => s.Level)
                .Select(s => new { s.ScopeLevelId, s.ScopeName, s.Level })
                .ToListAsync();

            return View("Views/Identity/Roles/Edit.cshtml", model);
        }

        /// <summary>
        /// Handle edit role submission
        /// </summary>
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await _context.Roles.FindAsync(model.RoleId);
                if (role == null)
                {
                    TempData["ErrorMessage"] = "Role not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check for duplicate RoleCode (excluding current role)
                if (await _context.Roles.AnyAsync(r => r.RoleCode == model.RoleCode && r.RoleId != model.RoleId))
                {
                    ModelState.AddModelError("RoleCode", "A role with this code already exists.");
                    ViewBag.ScopeLevels = await _context.ScopeLevels
                        .Where(s => s.IsActive)
                        .OrderBy(s => s.Level)
                        .Select(s => new { s.ScopeLevelId, s.ScopeName, s.Level })
                        .ToListAsync();
                    return View(model);
                }

                // Check for duplicate RoleName (excluding current role)
                if (await _context.Roles.AnyAsync(r => r.RoleName == model.RoleName && r.RoleId != model.RoleId))
                {
                    ModelState.AddModelError("RoleName", "A role with this name already exists.");
                    ViewBag.ScopeLevels = await _context.ScopeLevels
                        .Where(s => s.IsActive)
                        .OrderBy(s => s.Level)
                        .Select(s => new { s.ScopeLevelId, s.ScopeName, s.Level })
                        .ToListAsync();
                    return View(model);
                }

                role.RoleName = model.RoleName;
                role.RoleCode = model.RoleCode.ToUpper();
                role.Description = model.Description;
                role.ScopeLevelId = model.ScopeLevelId;
                role.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Role '{role.RoleName}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ScopeLevels = await _context.ScopeLevels
                .Where(s => s.IsActive)
                .OrderBy(s => s.Level)
                .Select(s => new { s.ScopeLevelId, s.ScopeName, s.Level })
                .ToListAsync();
            return View(model);
        }

        /// <summary>
        /// Delete a role
        /// </summary>
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
            {
                return Json(new { success = false, message = "Role not found." });
            }

            // Check if role has users
            if (role.UserRoles.Any())
            {
                return Json(new 
                { 
                    success = false, 
                    message = $"Cannot delete role '{role.RoleName}' because it has {role.UserRoles.Count} assigned user(s). Please reassign or remove these users first." 
                });
            }

            // Prevent deletion of system roles
            var systemRoles = new[] { "SYSTEM_ADMIN", "HO_ICT_MGR", "EMPLOYEE", "EXECUTIVE", "AUDITOR" };
            if (systemRoles.Contains(role.RoleCode))
            {
                return Json(new 
                { 
                    success = false, 
                    message = $"Cannot delete system role '{role.RoleName}'. System roles are protected." 
                });
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Role '{role.RoleName}' deleted successfully." });
        }

        /// <summary>
        /// Get role details for view modal
        /// </summary>
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var role = await _context.Roles
                .Include(r => r.ScopeLevel)
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
            {
                return Json(new { success = false, message = "Role not found." });
            }

            var viewModel = new RoleViewModel
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                RoleCode = role.RoleCode,
                Description = role.Description,
                ScopeLevelName = role.ScopeLevel.ScopeName,
                ScopeCode = role.ScopeLevel.ScopeCode,
                Level = role.ScopeLevel.Level,
                IsActive = role.IsActive,
                UserCount = role.UserRoles.Count,
                CreatedDate = role.CreatedDate
            };

            return Json(new { success = true, data = viewModel });
        }
    }
}

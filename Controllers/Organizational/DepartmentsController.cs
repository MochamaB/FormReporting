using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.Entities.Organizational;
using FormReporting.Services.Organizational;
using Microsoft.AspNetCore.Authorization;

namespace FormReporting.Controllers.Organizational
{
    /// <summary>
    /// Controller for Department CRUD operations
    /// Implements WF-1.7, WF-1.8, WF-1.9 from Section1_Workflows_Actions.md
    /// </summary>
    [Authorize]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDepartmentService _departmentService;
        private readonly ITenantService _tenantService;

        public DepartmentsController(
            ApplicationDbContext context,
            IDepartmentService departmentService,
            ITenantService tenantService)
        {
            _context = context;
            _departmentService = departmentService;
            _tenantService = tenantService;
        }

        // ============================================================================
        // INDEX - List all departments with filters and pagination
        // ============================================================================
        public async Task<IActionResult> Index(string? search, int? tenantId, string? status, int page = 1)
        {
            const int pageSize = 15;

            // Get accessible departments based on user scope
            var scopedDepartments = await _departmentService.GetAccessibleDepartmentsAsync(User, search);

            // Apply tenant filter
            if (tenantId.HasValue)
            {
                scopedDepartments = scopedDepartments.Where(d => d.TenantId == tenantId.Value).ToList();
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "active")
                    scopedDepartments = scopedDepartments.Where(d => d.IsActive).ToList();
                else if (status.ToLower() == "inactive")
                    scopedDepartments = scopedDepartments.Where(d => !d.IsActive).ToList();
            }

            // Calculate statistics
            ViewBag.TotalDepartments = scopedDepartments.Count(d => d.IsActive);
            ViewBag.InactiveDepartments = scopedDepartments.Count(d => !d.IsActive);
            ViewBag.ParentDepartments = scopedDepartments.Count(d => d.ParentDepartmentId == null && d.IsActive);
            ViewBag.ChildDepartments = scopedDepartments.Count(d => d.ParentDepartmentId != null && d.IsActive);

            // Calculate trend (departments created in last 30 days)
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var recentDepartments = scopedDepartments.Count(d => d.CreatedDate >= thirtyDaysAgo);
            ViewBag.RecentDepartments = recentDepartments;

            // Pagination
            var totalItems = scopedDepartments.Count;
            var departments = scopedDepartments
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentTenantId = tenantId;
            ViewBag.CurrentStatus = status;

            // Load tenants for filter dropdown
            var tenants = await _tenantService.GetAccessibleTenantsAsync(User);
            ViewBag.Tenants = tenants;

            return View("~/Views/Organizational/Departments/Index.cshtml", departments);
        }

        // ============================================================================
        // CREATE - Display form (WF-1.7: Create Department)
        // ============================================================================
        public async Task<IActionResult> Create(int? tenantId)
        {
            // Load accessible tenants for dropdown
            var tenants = await _tenantService.GetAccessibleTenantsAsync(User);
            ViewBag.Tenants = tenants;

            // Don't pre-load parent departments - let JavaScript handle this dynamically
            // This prevents JSON parsing errors and improves UX
            ViewBag.ParentDepartments = null;

            var model = new Department { TenantId = tenantId ?? 0 };
            return View("~/Views/Organizational/Departments/Create.cshtml", model);
        }

        // ============================================================================
        // CREATE - Handle form submission (WF-1.7: Create Department)
        // Business Rules:
        // - DepartmentCode must be unique within Tenant
        // - ParentDepartmentId must belong to same Tenant
        // ============================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            try
            {
                // Remove auto-generated Required validation errors for fields we validate manually
                if (ModelState.ContainsKey("TenantId"))
                {
                    var tenantIdErrors = ModelState["TenantId"].Errors
                        .Where(e => e.ErrorMessage.Contains("required", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    foreach (var error in tenantIdErrors)
                    {
                        ModelState["TenantId"].Errors.Remove(error);
                    }
                }

                // Validate required fields with custom messages
                if (string.IsNullOrWhiteSpace(department.DepartmentName))
                {
                    ModelState.AddModelError("DepartmentName", "Department name is required");
                }

                if (string.IsNullOrWhiteSpace(department.DepartmentCode))
                {
                    ModelState.AddModelError("DepartmentCode", "Department code is required");
                }

                if (department.TenantId <= 0)
                {
                    ModelState.AddModelError("TenantId", "Please select a tenant");
                }

                // Business Rule: Validate DepartmentCode uniqueness within tenant
                if (department.TenantId > 0 && !string.IsNullOrWhiteSpace(department.DepartmentCode))
                {
                    var codeExists = await _context.Departments
                        .AnyAsync(d => d.TenantId == department.TenantId &&
                                      d.DepartmentCode.ToLower() == department.DepartmentCode.ToLower());

                    if (codeExists)
                    {
                        ModelState.AddModelError("DepartmentCode",
                            "A department with this code already exists in the selected tenant");
                    }
                }

                // Business Rule: Validate parent department belongs to same tenant
                if (department.ParentDepartmentId.HasValue)
                {
                    var parentDept = await _context.Departments
                        .FirstOrDefaultAsync(d => d.DepartmentId == department.ParentDepartmentId.Value);

                    if (parentDept == null)
                    {
                        ModelState.AddModelError("ParentDepartmentId",
                            "Selected parent department does not exist");
                    }
                    else if (parentDept.TenantId != department.TenantId)
                    {
                        ModelState.AddModelError("ParentDepartmentId",
                            "Parent department must belong to the same tenant");
                    }
                }

                if (ModelState.IsValid)
                {
                    department.CreatedDate = DateTime.UtcNow;
                    department.ModifiedDate = DateTime.UtcNow;
                    department.IsActive = true;

                    _context.Add(department);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Department '{department.DepartmentName}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating department: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while creating the department. Please try again.";
            }

            // Reload dropdowns on validation failure
            var tenants = await _tenantService.GetAccessibleTenantsAsync(User);
            ViewBag.Tenants = tenants;

            // Don't pre-load parent departments - let JavaScript handle it
            ViewBag.ParentDepartments = null;

            return View("~/Views/Organizational/Departments/Create.cshtml", department);
        }

        // ============================================================================
        // EDIT - Display form
        // ============================================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _departmentService.GetDepartmentByIdAsync(id.Value);
            if (department == null)
            {
                return NotFound();
            }

            // Load accessible tenants
            var tenants = await _tenantService.GetAccessibleTenantsAsync(User);
            ViewBag.Tenants = tenants;

            // Load parent departments (exclude self and descendants to prevent circular reference)
            var departments = await _departmentService.GetDepartmentsByTenantAsync(department.TenantId);
            ViewBag.ParentDepartments = departments
                .Where(d => d.IsActive && d.DepartmentId != id.Value)
                .ToList();

            return View("~/Views/Organizational/Departments/Edit.cshtml", department);
        }

        // ============================================================================
        // EDIT - Handle form submission
        // ============================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.DepartmentId)
            {
                return NotFound();
            }

            // Validate code uniqueness (excluding current record)
            var codeExists = await _context.Departments
                .AnyAsync(d => d.TenantId == department.TenantId &&
                              d.DepartmentCode.ToLower() == department.DepartmentCode.ToLower() &&
                              d.DepartmentId != id);

            if (codeExists)
            {
                ModelState.AddModelError("DepartmentCode",
                    "A department with this code already exists in the selected tenant");
            }

            // Validate parent department
            if (department.ParentDepartmentId.HasValue)
            {
                // Cannot set self as parent
                if (department.ParentDepartmentId.Value == id)
                {
                    ModelState.AddModelError("ParentDepartmentId",
                        "A department cannot be its own parent");
                }
                else
                {
                    var parentDept = await _context.Departments
                        .FirstOrDefaultAsync(d => d.DepartmentId == department.ParentDepartmentId.Value);

                    if (parentDept == null)
                    {
                        ModelState.AddModelError("ParentDepartmentId",
                            "Selected parent department does not exist");
                    }
                    else if (parentDept.TenantId != department.TenantId)
                    {
                        ModelState.AddModelError("ParentDepartmentId",
                            "Parent department must belong to the same tenant");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    department.ModifiedDate = DateTime.UtcNow;
                    _context.Update(department);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Department '{department.DepartmentName}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await DepartmentExistsAsync(department.DepartmentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Reload dropdowns on validation failure
            var tenants = await _tenantService.GetAccessibleTenantsAsync(User);
            ViewBag.Tenants = tenants;

            var departments = await _departmentService.GetDepartmentsByTenantAsync(department.TenantId);
            ViewBag.ParentDepartments = departments
                .Where(d => d.IsActive && d.DepartmentId != id)
                .ToList();

            return View("~/Views/Organizational/Departments/Edit.cshtml", department);
        }

        // ============================================================================
        // DETAILS - Show department info (WF-1.8: Department Hierarchy View)
        // ============================================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _departmentService.GetDepartmentByIdAsync(id.Value);
            if (department == null)
            {
                return NotFound();
            }

            return View("~/Views/Organizational/Departments/Details.cshtml", department);
        }

        // ============================================================================
        // DELETE - With users check (WF-1.9: Delete Department with Users Check)
        // Business Rules:
        // - Cannot delete if users are assigned
        // - Cannot delete if child departments exist
        // ============================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            // Business Rule: Check if department has users (WF-1.9)
            var canDelete = await _departmentService.CanDeleteDepartmentAsync(id);
            if (!canDelete)
            {
                var userCount = department.Users.Count;
                TempData["ErrorMessage"] = $"Cannot delete '{department.DepartmentName}'. " +
                    $"{userCount} user{(userCount > 1 ? "s are" : " is")} assigned to this department. " +
                    "Reassign users first or deactivate the department instead.";
                return RedirectToAction(nameof(Index));
            }

            // Business Rule: Check for child departments
            if (department.ChildDepartments.Any())
            {
                var childCount = department.ChildDepartments.Count;
                TempData["ErrorMessage"] = $"Cannot delete '{department.DepartmentName}'. " +
                    $"It has {childCount} sub-department{(childCount > 1 ? "s" : "")}. " +
                    "Delete or reassign child departments first.";
                return RedirectToAction(nameof(Index));
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Department '{department.DepartmentName}' deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================================
        // HIERARCHY VIEW - Tree structure (WF-1.8: Department Hierarchy View)
        // ============================================================================
        public async Task<IActionResult> Hierarchy(int? tenantId)
        {
            if (!tenantId.HasValue)
            {
                // Show tenant selection page
                var tenants = await _tenantService.GetAccessibleTenantsAsync(User);
                ViewBag.Tenants = tenants;
                return View("~/Views/Organizational/Departments/Hierarchy.cshtml");
            }

            // Load hierarchy for selected tenant
            var hierarchy = await _departmentService.GetDepartmentHierarchyAsync(tenantId.Value);
            var tenant = await _context.Tenants.FindAsync(tenantId.Value);

            ViewBag.TenantName = tenant?.TenantName;
            ViewBag.TenantId = tenantId.Value;

            return View("~/Views/Organizational/Departments/Hierarchy.cshtml", hierarchy);
        }

        // ============================================================================
        // AJAX - Get departments by tenant (for dynamic dropdowns)
        // ============================================================================
        [HttpGet]
        public async Task<IActionResult> GetDepartmentsByTenant(int tenantId)
        {
            var departments = await _departmentService.GetDepartmentsByTenantAsync(tenantId);
            var result = departments.Select(d => new
            {
                id = d.DepartmentId,
                name = d.DepartmentName,
                code = d.DepartmentCode,
                isActive = d.IsActive
            });

            return Json(result);
        }

        // ============================================================================
        // Helper Methods
        // ============================================================================
        private async Task<bool> DepartmentExistsAsync(int id)
        {
            return await _context.Departments.AnyAsync(e => e.DepartmentId == id);
        }
    }
}

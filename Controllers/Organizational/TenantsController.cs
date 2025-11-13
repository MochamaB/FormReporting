using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.ViewModels.Organizational;
using FormReporting.Models.Entities.Organizational;

namespace FormReporting.Controllers.Organizational
{
    public class TenantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TenantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Index - List all tenants with statistics, filters, and pagination
        /// </summary>
        public async Task<IActionResult> Index(string? search, string? status, string? type, int page = 1)
        {
            const int pageSize = 15; // Items per page

            // Start with base query
            var query = _context.Tenants
                .Include(t => t.Region)
                .Include(t => t.Departments)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(t =>
                    t.TenantCode.ToLower().Contains(search) ||
                    t.TenantName.ToLower().Contains(search) ||
                    t.TenantType.ToLower().Contains(search) ||
                    (t.Location != null && t.Location.ToLower().Contains(search)));
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "active")
                    query = query.Where(t => t.IsActive);
                else if (status.ToLower() == "inactive")
                    query = query.Where(t => !t.IsActive);
            }

            // Apply tenant type filter
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(t => t.TenantType.ToLower() == type.ToLower());
            }

            // Get total count before pagination
            var totalItems = await query.CountAsync();

            // Calculate statistics (on filtered data)
            var allTenants = await query.ToListAsync();

            ViewBag.TotalTenants = allTenants.Count(t => t.IsActive);
            ViewBag.TotalDepartments = allTenants.Sum(t => t.Departments.Count(d => d.IsActive));
            ViewBag.HeadOfficeCount = allTenants.Count(t => t.TenantType.ToLower() == "headoffice" && t.IsActive);
            ViewBag.FactoryCount = allTenants.Count(t => t.TenantType.ToLower() == "factory" && t.IsActive);
            ViewBag.SubsidiaryCount = allTenants.Count(t => t.TenantType.ToLower() == "subsidiary" && t.IsActive);
            ViewBag.InactiveTenants = allTenants.Count(t => !t.IsActive);

            // Calculate trend (tenants created in last 30 days vs previous 30 days)
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var sixtyDaysAgo = DateTime.Now.AddDays(-60);
            var recentTenants = await _context.Tenants.CountAsync(t => t.CreatedDate >= thirtyDaysAgo);
            var previousTenants = await _context.Tenants.CountAsync(t => t.CreatedDate >= sixtyDaysAgo && t.CreatedDate < thirtyDaysAgo);
            ViewBag.TenantGrowth = previousTenants > 0
                ? ((recentTenants - previousTenants) / (double)previousTenants * 100)
                : 0;

            // Execute query with pagination
            var tenants = await query
                .OrderBy(t => t.TenantCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TenantViewModel
                {
                    TenantId = t.TenantId,
                    TenantCode = t.TenantCode,
                    TenantName = t.TenantName,
                    TenantType = t.TenantType,
                    RegionName = t.Region != null ? t.Region.RegionName : null,
                    Location = t.Location,
                    DepartmentCount = t.Departments.Count(d => d.IsActive),
                    IsActive = t.IsActive,
                    CreatedDate = t.CreatedDate,
                    ModifiedDate = t.ModifiedDate
                })
                .ToListAsync();

            // Pass pagination info to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            // Pass filter values to view for maintaining state
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentType = type;

            return View("~/Views/Organizational/Tenants/Index.cshtml", tenants);
        }

        /// <summary>
        /// Create - Display wizard for creating a new tenant
        /// Implements WF-1.10: Tenant Creation Wizard (4 Steps)
        /// </summary>
        public async Task<IActionResult> Create()
        {
            // Load data needed for wizard
            ViewBag.Regions = await _context.Regions
                .Where(r => r.IsActive)
                .OrderBy(r => r.RegionName)
                .Select(r => new { r.RegionId, r.RegionName })
                .ToListAsync();

            ViewBag.TenantGroups = await _context.TenantGroups
                .Where(g => g.IsActive)
                .OrderBy(g => g.GroupName)
                .Select(g => new { g.TenantGroupId, g.GroupName, g.Description })
                .ToListAsync();

            // Check if HeadOffice already exists (Business Rule)
            var headOfficeExists = await _context.Tenants
                .AnyAsync(t => t.TenantType.ToLower() == "headoffice");
            ViewBag.HeadOfficeExists = headOfficeExists;

            var model = new TenantCreateViewModel();
            return View("~/Views/Organizational/Tenants/Create.cshtml", model);
        }

        /// <summary>
        /// Create - Handle tenant creation from wizard
        /// Post-Creation Actions: Create tenant, departments, group memberships, send notifications
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TenantCreateViewModel model)
        {
            // Validate all steps
            var step1Errors = model.ValidateStep1();
            var step2Errors = model.ValidateStep2();
            var step3Errors = model.ValidateStep3();

            var allErrors = step1Errors.Concat(step2Errors).Concat(step3Errors).ToList();

            if (allErrors.Any())
            {
                foreach (var error in allErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                // Reload dropdown data
                ViewBag.Regions = await _context.Regions
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.RegionName)
                    .Select(r => new { r.RegionId, r.RegionName })
                    .ToListAsync();

                ViewBag.TenantGroups = await _context.TenantGroups
                    .Where(g => g.IsActive)
                    .OrderBy(g => g.GroupName)
                    .Select(g => new { g.TenantGroupId, g.GroupName, g.Description })
                    .ToListAsync();

                return View("~/Views/Organizational/Tenants/Create.cshtml", model);
            }

            // Business Rule: Check TenantCode uniqueness
            var codeExists = await _context.Tenants
                .AnyAsync(t => t.TenantCode.ToLower() == model.TenantCode.ToLower());
            if (codeExists)
            {
                ModelState.AddModelError("TenantCode", "A tenant with this code already exists");
                return View("~/Views/Organizational/Tenants/Create.cshtml", model);
            }

            // Business Rule: Only one HeadOffice allowed
            if (model.TenantType.ToLower() == "headoffice")
            {
                var headOfficeExists = await _context.Tenants
                    .AnyAsync(t => t.TenantType.ToLower() == "headoffice");
                if (headOfficeExists)
                {
                    ModelState.AddModelError("TenantType", "A Head Office already exists. Only one is allowed.");
                    return View("~/Views/Organizational/Tenants/Create.cshtml", model);
                }
            }

            // Start transaction for multiple operations
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Create Tenant record
                var tenant = new Tenant
                {
                    TenantCode = model.TenantCode,
                    TenantName = model.TenantName,
                    TenantType = model.TenantType,
                    RegionId = model.RegionId,
                    Location = model.Location,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    ContactPhone = model.ContactPhone,
                    ContactEmail = model.ContactEmail,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                // 2. Create Department records
                var departmentsToCreate = model.CreateDefaultDepartments && !model.Departments.Any()
                    ? GetDefaultDepartments()
                    : model.Departments;

                foreach (var deptModel in departmentsToCreate)
                {
                    var department = new Department
                    {
                        TenantId = tenant.TenantId,
                        DepartmentCode = deptModel.DepartmentCode,
                        DepartmentName = deptModel.DepartmentName,
                        Description = deptModel.Description,
                        IsActive = deptModel.IsActive,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };

                    _context.Departments.Add(department);
                }
                await _context.SaveChangesAsync();

                // 3. Create TenantGroupMembers records
                if (model.SelectedGroupIds != null && model.SelectedGroupIds.Any())
                {
                    foreach (var groupId in model.SelectedGroupIds)
                    {
                        var membership = new TenantGroupMember
                        {
                            TenantGroupId = groupId,
                            TenantId = tenant.TenantId,
                            AddedBy = 1, // TODO: Replace with current user ID
                            AddedDate = DateTime.UtcNow
                        };

                        _context.TenantGroupMembers.Add(membership);
                    }
                    await _context.SaveChangesAsync();
                }

                // Commit transaction
                await transaction.CommitAsync();

                // 4. TODO: Trigger notification to Regional Manager (if Factory)
                // This would be implemented when notification system is ready

                TempData["SuccessMessage"] = $"Tenant '{tenant.TenantName}' created successfully with {departmentsToCreate.Count} departments!";

                // 5. Redirect to tenant details or user management
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"Error creating tenant: {ex.Message}");

                // Reload dropdown data
                ViewBag.Regions = await _context.Regions
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.RegionName)
                    .Select(r => new { r.RegionId, r.RegionName })
                    .ToListAsync();

                ViewBag.TenantGroups = await _context.TenantGroups
                    .Where(g => g.IsActive)
                    .OrderBy(g => g.GroupName)
                    .Select(g => new { g.TenantGroupId, g.GroupName, g.Description })
                    .ToListAsync();

                return View("~/Views/Organizational/Tenants/Create.cshtml", model);
            }
        }

        /// <summary>
        /// Get default departments for tenant creation
        /// </summary>
        private List<DepartmentCreateModel> GetDefaultDepartments()
        {
            return new List<DepartmentCreateModel>
            {
                new DepartmentCreateModel
                {
                    DepartmentCode = "GEN",
                    DepartmentName = "General",
                    IsActive = true,
                    IsRemovable = false
                },
                new DepartmentCreateModel
                {
                    DepartmentCode = "ICT",
                    DepartmentName = "ICT Department",
                    IsActive = true,
                    IsRemovable = false
                },
                new DepartmentCreateModel
                {
                    DepartmentCode = "FIN",
                    DepartmentName = "Finance Department",
                    IsActive = true,
                    IsRemovable = false
                },
                new DepartmentCreateModel
                {
                    DepartmentCode = "OPS",
                    DepartmentName = "Operations Department",
                    IsActive = true,
                    IsRemovable = false
                }
            };
        }
    }
}

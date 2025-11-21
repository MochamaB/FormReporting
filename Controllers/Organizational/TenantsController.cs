using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.ViewModels.Organizational;
using FormReporting.Models.Entities.Organizational;
using FormReporting.Services.Organizational;

namespace FormReporting.Controllers.Organizational
{
    public class TenantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        public TenantsController(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        /// <summary>
        /// Index - List all tenants with statistics, filters, and pagination
        /// Uses TenantService for scope-based access control
        /// </summary>
        public async Task<IActionResult> Index(string? search, string? status, string? type, int page = 1)
        {
            const int pageSize = 15; // Items per page

            // 1. GET SCOPE-FILTERED TENANTS using TenantService (with search)
            var scopedTenants = await _tenantService.GetAccessibleTenantsAsync(User, search);

            // 2. APPLY STATUS FILTER
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "active")
                    scopedTenants = scopedTenants.Where(t => t.IsActive).ToList();
                else if (status.ToLower() == "inactive")
                    scopedTenants = scopedTenants.Where(t => !t.IsActive).ToList();
            }

            // 3. APPLY TENANT TYPE FILTER
            if (!string.IsNullOrWhiteSpace(type))
            {
                scopedTenants = scopedTenants.Where(t => t.TenantType.ToLower() == type.ToLower()).ToList();
            }

            // 4. CALCULATE STATISTICS (from scope-filtered data)
            ViewBag.TotalTenants = scopedTenants.Count(t => t.IsActive);
            ViewBag.TotalDepartments = scopedTenants.Sum(t => t.Departments.Count(d => d.IsActive));
            ViewBag.HeadOfficeCount = scopedTenants.Count(t => t.TenantType.ToLower() == "headoffice" && t.IsActive);
            ViewBag.FactoryCount = scopedTenants.Count(t => t.TenantType.ToLower() == "factory" && t.IsActive);
            ViewBag.SubsidiaryCount = scopedTenants.Count(t => t.TenantType.ToLower() == "subsidiary" && t.IsActive);
            ViewBag.InactiveTenants = scopedTenants.Count(t => !t.IsActive);

            // 5. CALCULATE TREND (from accessible tenants only)
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var sixtyDaysAgo = DateTime.Now.AddDays(-60);
            var recentTenants = scopedTenants.Count(t => t.CreatedDate >= thirtyDaysAgo);
            var previousTenants = scopedTenants.Count(t => t.CreatedDate >= sixtyDaysAgo && t.CreatedDate < thirtyDaysAgo);
            ViewBag.TenantGrowth = previousTenants > 0
                ? ((recentTenants - previousTenants) / (double)previousTenants * 100)
                : 0;

            // 6. PAGINATION
            var totalItems = scopedTenants.Count;

            // 7. GET PAGINATED DATA
            var tenants = scopedTenants
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
                .ToList();

            // 8. PASS PAGINATION INFO TO VIEW
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            // 9. PASS FILTER VALUES TO VIEW for maintaining state
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

        // ============================================================================
        // EDIT TENANT
        // ============================================================================

        /// <summary>
        /// GET: Edit tenant
        /// Uses TenantService for scope-based access control
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Check if user has access to this tenant (scope-based)
            var canAccess = await _tenantService.CanAccessTenantAsync(User, id);
            if (!canAccess)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this tenant";
                return RedirectToAction(nameof(Index));
            }

            var tenant = await _context.Tenants
                .Include(t => t.Region)
                .FirstOrDefaultAsync(t => t.TenantId == id);

            if (tenant == null)
            {
                TempData["ErrorMessage"] = "Tenant not found";
                return RedirectToAction(nameof(Index));
            }

            // Map to ViewModel
            var model = new TenantEditViewModel
            {
                TenantId = tenant.TenantId,
                TenantCode = tenant.TenantCode,
                TenantName = tenant.TenantName,
                TenantType = tenant.TenantType,
                RegionId = tenant.RegionId,
                Location = tenant.Location,
                Latitude = tenant.Latitude,
                Longitude = tenant.Longitude,
                ContactPhone = tenant.ContactPhone,
                ContactEmail = tenant.ContactEmail,
                IsActive = tenant.IsActive,
                CreatedDate = tenant.CreatedDate,
                CreatedBy = tenant.CreatedBy ?? 0,
                ModifiedDate = tenant.ModifiedDate,
                ModifiedBy = tenant.ModifiedBy
            };

            // Get currently assigned groups
            model.SelectedGroupIds = await _context.TenantGroupMembers
                .Where(tgm => tgm.TenantId == id)
                .Select(tgm => tgm.TenantGroupId)
                .ToListAsync();

            // Load dropdown data
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

            // Load existing departments for display
            ViewBag.ExistingDepartments = await _context.Departments
                .Where(d => d.TenantId == id)
                .OrderBy(d => d.DepartmentName)
                .Select(d => new
                {
                    d.DepartmentId,
                    d.DepartmentCode,
                    d.DepartmentName,
                    d.Description,
                    d.IsActive,
                    d.CreatedDate
                })
                .ToListAsync();

            // Check if HeadOffice exists (excluding current tenant if it's already HeadOffice)
            ViewBag.HeadOfficeExists = await _context.Tenants
                .AnyAsync(t => t.TenantType.ToLower() == "headoffice" && t.TenantId != id);

            return View("~/Views/Organizational/Tenants/Edit.cshtml", model);
        }

        /// <summary>
        /// POST: Update tenant
        /// Uses TenantService for scope-based access control
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TenantEditViewModel model)
        {
            // Check if user has access to this tenant (scope-based)
            var canAccess = await _tenantService.CanAccessTenantAsync(User, model.TenantId);
            if (!canAccess)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this tenant";
                return RedirectToAction(nameof(Index));
            }

            // Custom validation
            var validationErrors = model.ValidateBasicDetails();
            foreach (var error in validationErrors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            // Check for duplicate tenant code (excluding current tenant)
            if (await _context.Tenants.AnyAsync(t => t.TenantCode.ToLower() == model.TenantCode.ToLower() && t.TenantId != model.TenantId))
            {
                ModelState.AddModelError("TenantCode", "A tenant with this code already exists");
            }

            // Business rule: Cannot change type if there's only one HeadOffice
            var existingTenant = await _context.Tenants.FindAsync(model.TenantId);
            if (existingTenant != null && existingTenant.TenantType.ToLower() == "headoffice" && model.TenantType.ToLower() != "headoffice")
            {
                if (!await _context.Tenants.AnyAsync(t => t.TenantType.ToLower() == "headoffice" && t.TenantId != model.TenantId))
                {
                    ModelState.AddModelError("TenantType", "Cannot change type - at least one HeadOffice must exist");
                }
            }

            if (!ModelState.IsValid)
            {
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

                ViewBag.ExistingDepartments = await _context.Departments
                    .Where(d => d.TenantId == model.TenantId)
                    .OrderBy(d => d.DepartmentName)
                    .Select(d => new
                    {
                        d.DepartmentId,
                        d.DepartmentCode,
                        d.DepartmentName,
                        d.Description,
                        d.IsActive,
                        d.CreatedDate
                    })
                    .ToListAsync();

                // Check if HeadOffice exists (excluding current tenant if it's already HeadOffice)
                ViewBag.HeadOfficeExists = await _context.Tenants
                    .AnyAsync(t => t.TenantType.ToLower() == "headoffice" && t.TenantId != model.TenantId);

                return View("~/Views/Organizational/Tenants/Edit.cshtml", model);
            }

            // Start transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tenant = await _context.Tenants.FindAsync(model.TenantId);
                if (tenant == null)
                {
                    TempData["ErrorMessage"] = "Tenant not found";
                    return RedirectToAction(nameof(Index));
                }

                // Update tenant properties
                tenant.TenantCode = model.TenantCode;
                tenant.TenantName = model.TenantName;
                tenant.TenantType = model.TenantType;
                tenant.RegionId = model.RegionId;
                tenant.Location = model.Location;
                tenant.Latitude = model.Latitude;
                tenant.Longitude = model.Longitude;
                tenant.ContactPhone = model.ContactPhone;
                tenant.ContactEmail = model.ContactEmail;
                tenant.IsActive = model.IsActive;
                tenant.ModifiedDate = DateTime.UtcNow;
                tenant.ModifiedBy = 1; // TODO: Replace with current user ID

                _context.Tenants.Update(tenant);
                await _context.SaveChangesAsync();

                // Update TenantGroupMembers
                // Remove existing memberships
                var existingMemberships = await _context.TenantGroupMembers
                    .Where(tgm => tgm.TenantId == model.TenantId)
                    .ToListAsync();
                _context.TenantGroupMembers.RemoveRange(existingMemberships);

                // Add new memberships
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
                }
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Tenant '{tenant.TenantName}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"Error updating tenant: {ex.Message}");

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

                ViewBag.ExistingDepartments = await _context.Departments
                    .Where(d => d.TenantId == model.TenantId)
                    .OrderBy(d => d.DepartmentName)
                    .Select(d => new
                    {
                        d.DepartmentId,
                        d.DepartmentCode,
                        d.DepartmentName,
                        d.Description,
                        d.IsActive,
                        d.CreatedDate
                    })
                    .ToListAsync();

                // Check if HeadOffice exists (excluding current tenant if it's already HeadOffice)
                ViewBag.HeadOfficeExists = await _context.Tenants
                    .AnyAsync(t => t.TenantType.ToLower() == "headoffice" && t.TenantId != model.TenantId);

                return View("~/Views/Organizational/Tenants/Edit.cshtml", model);
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

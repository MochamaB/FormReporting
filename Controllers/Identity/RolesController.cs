using FormReporting.Data;
using FormReporting.Models.Entities.Identity;
using FormReporting.Models.ViewModels.Identity;
using FormReporting.Models.ViewModels.Components;
using FormReporting.Extensions;
using FormReporting.Services.Identity;
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
        private readonly IUserService _userService;

        public RolesController(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
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
        /// Show create role page with wizard
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var model = new RoleEditViewModel
            {
                IsActive = true
            };

            // ═══════════════════════════════════════════════════════════
            // STEP 1 DATA: Load Scope Levels
            // ═══════════════════════════════════════════════════════════
            ViewBag.ScopeLevels = await _context.ScopeLevels
                .Where(s => s.IsActive)
                .OrderBy(s => s.Level)
                .ToListAsync();

            // ═══════════════════════════════════════════════════════════
            // STEP 2 DATA: Load Permissions grouped by Module
            // ═══════════════════════════════════════════════════════════
            var modules = await _context.Modules
                .Include(m => m.Permissions.Where(p => p.IsActive))
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
            ViewBag.Modules = modules;

            // Load existing roles for "Copy from existing role" feature
            var existingRoles = await _context.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoleName)
                .Select(r => new { r.RoleId, r.RoleName, r.RoleCode })
                .ToListAsync();
            ViewBag.ExistingRoles = existingRoles;

            // ═══════════════════════════════════════════════════════════
            // STEP 3 DATA: Users loaded via AJAX (see GetUsersGroupedByTenant endpoint)
            // ═══════════════════════════════════════════════════════════
            // No server-side user loading needed - handled by external JS module

            // ═══════════════════════════════════════════════════════════
            // WIZARD CONFIGURATION
            // ═══════════════════════════════════════════════════════════
            var wizardConfig = new WizardConfig
            {
                FormId = "roleCreationWizard",
                Layout = WizardLayout.Vertical,
                Steps = new List<WizardStep>
                {
                    // STEP 1: Basic Role Details
                    new WizardStep
                    {
                        StepId = "basic-details",
                        StepNumber = 1,
                        Title = "Basic Details",
                        Description = "Role information",
                        Instructions = "Enter role name, code, and scope level",
                        Icon = "ri-information-line",
                        State = WizardStepState.Active,
                        ContentPartialPath = "~/Views/Identity/Roles/Partials/_BasicDetails.cshtml",
                        ShowPrevious = false,
                        ShowNext = true,
                        NextButtonText = "Next: Permissions"
                    },

                    // STEP 2: Assign Permissions
                    new WizardStep
                    {
                        StepId = "assign-permissions",
                        StepNumber = 2,
                        Title = "Assign Permissions",
                        Description = "Role capabilities",
                        Instructions = "Select permissions or copy from existing role",
                        Icon = "ri-shield-check-line",
                        State = WizardStepState.Pending,
                        ContentPartialPath = "~/Views/Identity/Roles/Partials/_AssignPermissions.cshtml",
                        ShowPrevious = true,
                        ShowNext = true,
                        NextButtonText = "Next: Users"
                    },

                    // STEP 3: Assign Users
                    new WizardStep
                    {
                        StepId = "assign-users",
                        StepNumber = 3,
                        Title = "Assign Users",
                        Description = "Role members",
                        Instructions = "Select users who will have this role (optional)",
                        Icon = "ri-user-add-line",
                        State = WizardStepState.Pending,
                        ContentPartialPath = "~/Views/Identity/Roles/Partials/_AssignUsers.cshtml",
                        ShowPrevious = true,
                        ShowNext = true,
                        NextButtonText = "Next: Review"
                    },

                    // STEP 4: Review & Confirm
                    new WizardStep
                    {
                        StepId = "review-confirm",
                        StepNumber = 4,
                        Title = "Review & Confirm",
                        Description = "Final review",
                        Instructions = "Review all settings and create role",
                        Icon = "ri-checkbox-circle-line",
                        State = WizardStepState.Pending,
                        ContentPartialPath = "~/Views/Identity/Roles/Partials/_ReviewAndConfirm.cshtml",
                        ShowPrevious = true,
                        ShowNext = false,
                        CustomButtonHtml = "<button type='submit' class='btn btn-primary'><i class='ri-add-line me-1'></i>Create Role</button>"
                    }
                }
            };

            var wizard = wizardConfig.BuildWizard();
            ViewData["Wizard"] = wizard;

            return View("Views/Identity/Roles/Create.cshtml", model);
        }

        /// <summary>
        /// Handle create role submission with permissions and users
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
                    await LoadCreateViewData();
                    return View(model);
                }

                // Check for duplicate RoleName
                if (await _context.Roles.AnyAsync(r => r.RoleName == model.RoleName))
                {
                    ModelState.AddModelError("RoleName", "A role with this name already exists.");
                    await LoadCreateViewData();
                    return View(model);
                }

                // Use transaction for atomic creation
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Create Role
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

                        // 2. Create RolePermissions (if any selected)
                        if (model.SelectedPermissionIds != null && model.SelectedPermissionIds.Any())
                        {
                            foreach (var permissionId in model.SelectedPermissionIds)
                            {
                                var rolePermission = new RolePermission
                                {
                                    RoleId = role.RoleId,
                                    PermissionId = permissionId,
                                    IsGranted = true,
                                    AssignedDate = DateTime.UtcNow,
                                    AssignedBy = null // TODO: Get current user ID when auth is implemented
                                };
                                _context.RolePermissions.Add(rolePermission);
                            }
                            await _context.SaveChangesAsync();
                        }

                        // 3. Create UserRoles (if any users selected)
                        if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
                        {
                            foreach (var userId in model.SelectedUserIds)
                            {
                                var userRole = new UserRole
                                {
                                    UserId = userId,
                                    RoleId = role.RoleId,
                                    AssignedDate = DateTime.UtcNow,
                                    AssignedBy = null // TODO: Get current user ID when auth is implemented
                                };
                                _context.UserRoles.Add(userRole);
                            }
                            await _context.SaveChangesAsync();
                        }

                        // Commit transaction
                        await transaction.CommitAsync();

                        // Success message with details
                        var message = $"Role '{role.RoleName}' created successfully.";
                        if (model.SelectedPermissionIds?.Any() == true)
                        {
                            message += $" {model.SelectedPermissionIds.Count} permission(s) assigned.";
                        }
                        if (model.SelectedUserIds?.Any() == true)
                        {
                            message += $" {model.SelectedUserIds.Count} user(s) assigned.";
                        }

                        TempData["SuccessMessage"] = message;
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "An error occurred while creating the role: " + ex.Message);
                        await LoadCreateViewData();
                        return View(model);
                    }
                }
            }

            await LoadCreateViewData();
            return View(model);
        }

        /// <summary>
        /// Helper method to load all view data for Create page
        /// </summary>
        private async Task LoadCreateViewData()
        {
            // Load scope levels
            ViewBag.ScopeLevels = await _context.ScopeLevels
                .Where(s => s.IsActive)
                .OrderBy(s => s.Level)
                .ToListAsync();

            // Load modules with permissions
            var modules = await _context.Modules
                .Include(m => m.Permissions.Where(p => p.IsActive))
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
            ViewBag.Modules = modules;

            // Load existing roles for copy feature
            var existingRoles = await _context.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoleName)
                .Select(r => new { r.RoleId, r.RoleName, r.RoleCode })
                .ToListAsync();
            ViewBag.ExistingRoles = existingRoles;

            // Users loaded via AJAX (see GetUsersGroupedByTenant endpoint)
            // No server-side user loading needed - handled by external JS module

            // Rebuild wizard config
            var wizardConfig = new WizardConfig
            {
                FormId = "roleCreationWizard",
                Layout = WizardLayout.Vertical,
                Steps = new List<WizardStep>
                {
                    new WizardStep
                    {
                        StepId = "basic-details",
                        StepNumber = 1,
                        Title = "Basic Details",
                        Description = "Role information",
                        Icon = "ri-information-line",
                        State = WizardStepState.Active,
                        ContentPartialPath = "~/Views/Identity/Roles/Partials/_BasicDetails.cshtml",
                        ShowPrevious = false,
                        ShowNext = true
                    },
                    new WizardStep
                    {
                        StepId = "assign-permissions",
                        StepNumber = 2,
                        Title = "Assign Permissions",
                        Description = "Role capabilities",
                        Icon = "ri-shield-check-line",
                        State = WizardStepState.Pending,
                        ContentPartialPath = "~/Views/Identity/Roles/Partials/_AssignPermissions.cshtml",
                        ShowPrevious = true,
                        ShowNext = true
                    },
                    new WizardStep
                    {
                        StepId = "assign-users",
                        StepNumber = 3,
                        Title = "Assign Users",
                        Description = "Role members",
                        Icon = "ri-user-add-line",
                        State = WizardStepState.Pending,
                        ContentPartialPath = "~/Views/Identity/Roles/Partials/_AssignUsers.cshtml",
                        ShowPrevious = true,
                        ShowNext = true
                    },
                    new WizardStep
                    {
                        StepId = "review-confirm",
                        StepNumber = 4,
                        Title = "Review & Confirm",
                        Description = "Final review",
                        Icon = "ri-checkbox-circle-line",
                        State = WizardStepState.Pending,
                        ContentPartialPath = "~/Views/Identity/Roles/Partials/_ReviewAndConfirm.cshtml",
                        ShowPrevious = true,
                        ShowNext = false,
                        CustomButtonHtml = "<button type='submit' class='btn btn-primary'><i class='ri-add-line me-1'></i>Create Role</button>"
                    }
                }
            };

            var wizard = wizardConfig.BuildWizard();
            ViewData["Wizard"] = wizard;
        }

        /// <summary>
        /// Show edit role page
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            // Load role with related data
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            // Map to ViewModel
            var model = new RoleEditViewModel
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                RoleCode = role.RoleCode,
                Description = role.Description,
                ScopeLevelId = role.ScopeLevelId,
                IsActive = role.IsActive,
                // Load existing permissions
                SelectedPermissionIds = role.RolePermissions
                    .Where(rp => rp.IsGranted)
                    .Select(rp => rp.PermissionId)
                    .ToList(),
                // Load existing users
                SelectedUserIds = role.UserRoles
                    .Where(ur => ur.IsActive)
                    .Select(ur => ur.UserId)
                    .ToList()
            };

            // ═══════════════════════════════════════════════════════════
            // TAB 1 DATA: Load Scope Levels
            // ═══════════════════════════════════════════════════════════
            ViewBag.ScopeLevels = await _context.ScopeLevels
                .Where(s => s.IsActive)
                .OrderBy(s => s.Level)
                .ToListAsync();

            // ═══════════════════════════════════════════════════════════
            // TAB 2 DATA: Load Permissions grouped by Module
            // ═══════════════════════════════════════════════════════════
            var modules = await _context.Modules
                .Include(m => m.Permissions.Where(p => p.IsActive))
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
            ViewBag.Modules = modules;

            // Load existing roles for "Copy from existing role" feature
            var existingRoles = await _context.Roles
                .Where(r => r.IsActive && r.RoleId != id) // Exclude current role
                .OrderBy(r => r.RoleName)
                .Select(r => new { r.RoleId, r.RoleName, r.RoleCode })
                .ToListAsync();
            ViewBag.ExistingRoles = existingRoles;

            // ═══════════════════════════════════════════════════════════
            // TAB 3 DATA: Users loaded via AJAX (see GetUsersGroupedByTenant endpoint)
            // ═══════════════════════════════════════════════════════════
            // No server-side user loading needed - handled by external JS module

            return View("Views/Identity/Roles/Edit.cshtml", model);
        }

        /// <summary>
        /// Handle edit role submission
        /// </summary>
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadEditViewData(model.RoleId);
                return View(model);
            }

            // Check for duplicate RoleCode (excluding current role)
            if (await _context.Roles.AnyAsync(r => r.RoleCode == model.RoleCode && r.RoleId != model.RoleId))
            {
                ModelState.AddModelError("RoleCode", "A role with this code already exists.");
                await LoadEditViewData(model.RoleId);
                return View(model);
            }

            // Check for duplicate RoleName (excluding current role)
            if (await _context.Roles.AnyAsync(r => r.RoleName == model.RoleName && r.RoleId != model.RoleId))
            {
                ModelState.AddModelError("RoleName", "A role with this name already exists.");
                await LoadEditViewData(model.RoleId);
                return View(model);
            }

            // Use transaction for atomic update
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. UPDATE ROLE BASIC DETAILS
                    var role = await _context.Roles.FindAsync(model.RoleId);
                    if (role == null)
                    {
                        TempData["ErrorMessage"] = "Role not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    role.RoleName = model.RoleName;
                    role.RoleCode = model.RoleCode.ToUpper();
                    role.Description = model.Description;
                    role.ScopeLevelId = model.ScopeLevelId;
                    role.IsActive = model.IsActive;
                    await _context.SaveChangesAsync();

                    // 2. UPDATE ROLE PERMISSIONS
                    // Remove existing permissions
                    var existingPermissions = await _context.RolePermissions
                        .Where(rp => rp.RoleId == model.RoleId)
                        .ToListAsync();
                    _context.RolePermissions.RemoveRange(existingPermissions);
                    await _context.SaveChangesAsync();

                    // Add new permissions (if any selected)
                    if (model.SelectedPermissionIds != null && model.SelectedPermissionIds.Any())
                    {
                        foreach (var permissionId in model.SelectedPermissionIds)
                        {
                            var rolePermission = new RolePermission
                            {
                                RoleId = model.RoleId,
                                PermissionId = permissionId,
                                IsGranted = true,
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = "System" // TODO: Replace with actual user
                            };
                            _context.RolePermissions.Add(rolePermission);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // 3. UPDATE USER ROLES
                    // Remove existing user roles
                    var existingUserRoles = await _context.UserRoles
                        .Where(ur => ur.RoleId == model.RoleId)
                        .ToListAsync();
                    _context.UserRoles.RemoveRange(existingUserRoles);
                    await _context.SaveChangesAsync();

                    // Add new user roles (if any users selected)
                    if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
                    {
                        foreach (var userId in model.SelectedUserIds)
                        {
                            var userRole = new UserRole
                            {
                                UserId = userId,
                                RoleId = model.RoleId,
                                IsActive = true,
                                CreatedDate = DateTime.UtcNow,
                                CreatedBy = "System" // TODO: Replace with actual user
                            };
                            _context.UserRoles.Add(userRole);
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"Role '{role.RoleName}' updated successfully with {model.SelectedPermissionIds?.Count ?? 0} permission(s) and {model.SelectedUserIds?.Count ?? 0} user(s).";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"An error occurred while updating the role: {ex.Message}");
                    await LoadEditViewData(model.RoleId);
                    return View(model);
                }
            }
        }

        /// <summary>
        /// Helper method to load ViewData for Edit view (on errors)
        /// </summary>
        private async Task LoadEditViewData(int roleId)
        {
            // Load scope levels
            ViewBag.ScopeLevels = await _context.ScopeLevels
                .Where(s => s.IsActive)
                .OrderBy(s => s.Level)
                .ToListAsync();

            // Load modules with permissions
            var modules = await _context.Modules
                .Include(m => m.Permissions.Where(p => p.IsActive))
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
            ViewBag.Modules = modules;

            // Load existing roles for copy feature (exclude current role)
            var existingRoles = await _context.Roles
                .Where(r => r.IsActive && r.RoleId != roleId)
                .OrderBy(r => r.RoleName)
                .Select(r => new { r.RoleId, r.RoleName, r.RoleCode })
                .ToListAsync();
            ViewBag.ExistingRoles = existingRoles;
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

        /// <summary>
        /// AJAX endpoint to check if role code is available
        /// </summary>
        [HttpGet("CheckRoleCode")]
        public async Task<IActionResult> CheckRoleCode(string code, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { exists = false, valid = false, message = "Code is required" });
            }

            // Validate format
            if (!System.Text.RegularExpressions.Regex.IsMatch(code, @"^[A-Z_]+$"))
            {
                return Json(new { exists = false, valid = false, message = "Only uppercase letters and underscores allowed" });
            }

            // Check if exists
            var exists = excludeId.HasValue
                ? await _context.Roles.AnyAsync(r => r.RoleCode == code && r.RoleId != excludeId.Value)
                : await _context.Roles.AnyAsync(r => r.RoleCode == code);

            return Json(new
            {
                exists,
                valid = !exists,
                message = exists ? "Code already exists" : "Code is available"
            });
        }

        /// <summary>
        /// AJAX endpoint to get permissions for a role (for copying)
        /// </summary>
        [HttpGet("GetRolePermissions")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            var permissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.IsGranted)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            return Json(new
            {
                success = true,
                permissionIds,
                message = $"Found {permissionIds.Count} permission(s)"
            });
        }

        /// <summary>
        /// AJAX Endpoint: Get accessible users grouped by tenant (for bulk selection in wizard)
        /// Uses UserService which applies scope-based filtering
        /// </summary>
        [HttpGet("GetUsersGroupedByTenant")]
        public async Task<IActionResult> GetUsersGroupedByTenant(string? search = null)
        {
            try
            {
                var groupedUsers = await _userService.GetUsersGroupedByTenantAsync(User, search);
                return Json(groupedUsers);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}

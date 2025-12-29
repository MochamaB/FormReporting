using FormReporting.Data;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Services.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service for managing form template assignments
    /// </summary>
    public class FormAssignmentService : IFormAssignmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IClaimsService _claimsService;
        private readonly IScopeService _scopeService;
        private readonly ILogger<FormAssignmentService> _logger;

        public FormAssignmentService(
            ApplicationDbContext context,
            IClaimsService claimsService,
            IScopeService scopeService,
            ILogger<FormAssignmentService> logger)
        {
            _context = context;
            _claimsService = claimsService;
            _scopeService = scopeService;
            _logger = logger;
        }

        #region Assignment CRUD

        public async Task<(List<AssignmentListDto> Items, int TotalCount)> GetAssignmentsAsync(AssignmentFilterDto filter)
        {
            _logger.LogInformation("GetAssignmentsAsync: TemplateId={TemplateId}, Page={Page}, PageSize={PageSize}",
                filter.TemplateId, filter.Page, filter.PageSize);

            var query = _context.FormTemplateAssignments
                .Include(a => a.Template)
                .Include(a => a.AssignedByUser)
                .Include(a => a.TenantGroup)
                .Include(a => a.Tenant)
                .Include(a => a.Role)
                .Include(a => a.Department)
                .Include(a => a.UserGroup)
                .Include(a => a.User)
                .AsQueryable();

            // Apply filters
            if (filter.TemplateId.HasValue)
                query = query.Where(a => a.TemplateId == filter.TemplateId.Value);

            if (!string.IsNullOrEmpty(filter.AssignmentType))
                query = query.Where(a => a.AssignmentType == filter.AssignmentType);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(a => a.Status == filter.Status);

            if (filter.EffectiveAsOf.HasValue)
            {
                var asOf = filter.EffectiveAsOf.Value;
                query = query.Where(a => a.EffectiveFrom <= asOf && 
                                        (a.EffectiveUntil == null || a.EffectiveUntil >= asOf));
            }

            if (filter.AllowAnonymous.HasValue)
                query = query.Where(a => a.AllowAnonymous == filter.AllowAnonymous.Value);

            if (filter.IsExpired == true)
                query = query.Where(a => a.EffectiveUntil.HasValue && a.EffectiveUntil < DateTime.UtcNow);
            else if (filter.IsExpired == false)
                query = query.Where(a => !a.EffectiveUntil.HasValue || a.EffectiveUntil >= DateTime.UtcNow);

            var totalCount = await query.CountAsync();
            _logger.LogInformation("GetAssignmentsAsync: TotalCount after filters = {TotalCount}", totalCount);

            // Apply sorting - use AssignmentId as fallback to ensure stable ordering
            query = filter.SortBy?.ToLower() switch
            {
                "effectivefrom" => filter.SortDescending ? query.OrderByDescending(a => a.EffectiveFrom) : query.OrderBy(a => a.EffectiveFrom),
                "effectiveuntil" => filter.SortDescending ? query.OrderByDescending(a => a.EffectiveUntil) : query.OrderBy(a => a.EffectiveUntil),
                "assigneddate" => filter.SortDescending ? query.OrderByDescending(a => a.AssignedDate) : query.OrderBy(a => a.AssignedDate),
                "status" => filter.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                "template" => filter.SortDescending ? query.OrderByDescending(a => a.Template.TemplateName) : query.OrderBy(a => a.Template.TemplateName),
                _ => query.OrderByDescending(a => a.AssignmentId) // Default to ID for stable ordering
            };

            // Apply pagination - fetch entities first, then project in memory
            // (GetTargetName and FullName cannot be translated to SQL)
            var skip = (filter.Page - 1) * filter.PageSize;
            _logger.LogInformation("GetAssignmentsAsync: Skip={Skip}, Take={Take}", skip, filter.PageSize);
            
            // DEBUG: Try fetching without pagination first
            var allAssignments = await query.ToListAsync();
            _logger.LogInformation("GetAssignmentsAsync: Total assignments in query (no pagination) = {Count}", allAssignments.Count);
            
            var assignments = allAssignments.Skip(skip).Take(filter.PageSize).ToList();

            _logger.LogInformation("GetAssignmentsAsync: Fetched {Count} assignments from DB", assignments.Count);

            // Project to DTOs in memory (client-side evaluation)
            var items = assignments.Select(a => new AssignmentListDto
            {
                AssignmentId = a.AssignmentId,
                TemplateId = a.TemplateId,
                TemplateName = a.Template?.TemplateName ?? "Unknown",
                TemplateCode = a.Template?.TemplateCode ?? "",
                AssignmentType = a.AssignmentType,
                TargetName = GetTargetName(a),
                TargetDetail = GetTargetDetail(a),
                EffectiveFrom = a.EffectiveFrom,
                EffectiveUntil = a.EffectiveUntil,
                AllowAnonymous = a.AllowAnonymous,
                Status = a.Status,
                AssignedDate = a.AssignedDate,
                AssignedByName = a.AssignedByUser?.FullName ?? "Unknown"
            }).ToList();

            return (items, totalCount);
        }

        public async Task<AssignmentDetailDto?> GetAssignmentByIdAsync(int assignmentId)
        {
            var assignment = await _context.FormTemplateAssignments
                .Include(a => a.Template)
                .Include(a => a.AssignedByUser)
                .Include(a => a.CancelledByUser)
                .Include(a => a.TenantGroup)
                .Include(a => a.Tenant)
                .Include(a => a.Role)
                .Include(a => a.Department)
                .Include(a => a.UserGroup)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null)
                return null;

            var dto = new AssignmentDetailDto
            {
                AssignmentId = assignment.AssignmentId,
                TemplateId = assignment.TemplateId,
                TemplateName = assignment.Template?.TemplateName ?? "Unknown",
                TemplateCode = assignment.Template?.TemplateCode ?? "",
                AssignmentType = assignment.AssignmentType,
                TargetName = GetTargetName(assignment),
                TargetDetail = GetTargetDetail(assignment),
                EffectiveFrom = assignment.EffectiveFrom,
                EffectiveUntil = assignment.EffectiveUntil,
                AllowAnonymous = assignment.AllowAnonymous,
                Status = assignment.Status,
                AssignedDate = assignment.AssignedDate,
                AssignedByName = assignment.AssignedByUser?.FullName ?? "Unknown",
                TenantType = assignment.TenantType,
                TenantGroupId = assignment.TenantGroupId,
                TenantGroupName = assignment.TenantGroup?.GroupName,
                TenantId = assignment.TenantId,
                TenantName = assignment.Tenant?.TenantName,
                RoleId = assignment.RoleId,
                RoleName = assignment.Role?.RoleName,
                DepartmentId = assignment.DepartmentId,
                DepartmentName = assignment.Department?.DepartmentName,
                UserGroupId = assignment.UserGroupId,
                UserGroupName = assignment.UserGroup?.GroupName,
                UserId = assignment.UserId,
                UserName = assignment.User?.FullName,
                Notes = assignment.Notes,
                CancelledBy = assignment.CancelledBy,
                CancelledByName = assignment.CancelledByUser?.FullName,
                CancelledDate = assignment.CancelledDate,
                CancellationReason = assignment.CancellationReason
            };

            // Get target count
            dto.TargetCount = await GetTargetCountAsync(
                assignment.AssignmentType,
                GetTargetId(assignment),
                assignment.TenantType);

            // Get submission statistics (for current period based on frequency)
            var now = DateTime.UtcNow;
            var submissions = await _context.FormTemplateSubmissions
                .Where(s => s.TemplateId == assignment.TemplateId)
                .ToListAsync();

            dto.TotalExpectedSubmissions = dto.TargetCount;
            dto.CompletedSubmissions = submissions.Count(s => s.Status == "Submitted" || s.Status == "Approved");
            dto.PendingSubmissions = dto.TotalExpectedSubmissions - dto.CompletedSubmissions;

            return dto;
        }

        public async Task<AssignmentDetailDto> CreateAssignmentAsync(AssignmentCreateDto dto)
        {
            var userId = _claimsService.GetUserId();
            
            _logger.LogInformation("CreateAssignmentAsync: UserId={UserId}, TemplateId={TemplateId}, AssignmentType={AssignmentType}, TenantType={TenantType}",
                userId, dto.TemplateId, dto.AssignmentType, dto.TenantType);

            var assignment = new FormTemplateAssignment
            {
                TemplateId = dto.TemplateId,
                AssignmentType = dto.AssignmentType,
                TenantType = dto.TenantType,
                TenantGroupId = dto.TenantGroupId,
                TenantId = dto.TenantId,
                RoleId = dto.RoleId,
                DepartmentId = dto.DepartmentId,
                UserGroupId = dto.UserGroupId,
                UserId = dto.UserId,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveUntil = dto.EffectiveUntil,
                AllowAnonymous = dto.AllowAnonymous,
                Status = "Active",
                Notes = dto.Notes,
                AssignedBy = userId,
                AssignedDate = DateTime.UtcNow
            };

            _context.FormTemplateAssignments.Add(assignment);
            var saveResult = await _context.SaveChangesAsync();
            
            _logger.LogInformation("SaveChangesAsync returned {SaveResult}, AssignmentId={AssignmentId}", 
                saveResult, assignment.AssignmentId);

            if (assignment.AssignmentId == 0)
            {
                _logger.LogError("Assignment was not saved - AssignmentId is 0");
                throw new InvalidOperationException("Failed to save assignment to database");
            }

            var result = await GetAssignmentByIdAsync(assignment.AssignmentId);
            
            if (result == null)
            {
                _logger.LogError("GetAssignmentByIdAsync returned null for AssignmentId={AssignmentId}", assignment.AssignmentId);
                throw new InvalidOperationException($"Assignment {assignment.AssignmentId} was created but could not be retrieved");
            }

            return result;
        }

        public async Task<AssignmentDetailDto?> UpdateAssignmentAsync(int assignmentId, AssignmentUpdateDto dto)
        {
            var assignment = await _context.FormTemplateAssignments.FindAsync(assignmentId);
            if (assignment == null)
                return null;

            // Access Period
            if (dto.EffectiveFrom.HasValue) assignment.EffectiveFrom = dto.EffectiveFrom.Value;
            if (dto.EffectiveUntil.HasValue) assignment.EffectiveUntil = dto.EffectiveUntil;
            
            // Access Options
            if (dto.AllowAnonymous.HasValue) assignment.AllowAnonymous = dto.AllowAnonymous.Value;
            
            // Status & Notes
            if (!string.IsNullOrEmpty(dto.Status)) assignment.Status = dto.Status;
            if (dto.Notes != null) assignment.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated assignment {AssignmentId}", assignmentId);

            return await GetAssignmentByIdAsync(assignmentId);
        }

        public async Task<bool> CancelAssignmentAsync(int assignmentId, string reason)
        {
            var assignment = await _context.FormTemplateAssignments.FindAsync(assignmentId);
            if (assignment == null)
                return false;

            var userId = _claimsService.GetUserId();

            assignment.Status = "Revoked";
            assignment.CancelledBy = userId;
            assignment.CancelledDate = DateTime.UtcNow;
            assignment.CancellationReason = reason;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cancelled assignment {AssignmentId}: {Reason}", assignmentId, reason);

            return true;
        }

        public async Task<bool> ExtendEffectivePeriodAsync(int assignmentId, DateTime newEffectiveUntil)
        {
            var assignment = await _context.FormTemplateAssignments.FindAsync(assignmentId);
            if (assignment == null)
                return false;

            var oldEffectiveUntil = assignment.EffectiveUntil;
            assignment.EffectiveUntil = newEffectiveUntil;

            // Reactivate if was revoked due to expiry
            if (assignment.Status == "Revoked" && newEffectiveUntil > DateTime.UtcNow)
                assignment.Status = "Active";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Extended effective period for assignment {AssignmentId} from {OldDate} to {NewDate}", 
                assignmentId, oldEffectiveUntil, newEffectiveUntil);

            return true;
        }

        public async Task<bool> SuspendAssignmentAsync(int assignmentId, string? reason = null)
        {
            var assignment = await _context.FormTemplateAssignments.FindAsync(assignmentId);
            if (assignment == null)
                return false;

            assignment.Status = "Suspended";
            if (!string.IsNullOrEmpty(reason))
                assignment.Notes = $"{assignment.Notes}\n[Suspended: {reason}]".Trim();

            await _context.SaveChangesAsync();

            _logger.LogInformation("Suspended assignment {AssignmentId}: {Reason}", assignmentId, reason);

            return true;
        }

        public async Task<bool> ReactivateAssignmentAsync(int assignmentId)
        {
            var assignment = await _context.FormTemplateAssignments.FindAsync(assignmentId);
            if (assignment == null)
                return false;

            // Only reactivate if not expired
            if (assignment.EffectiveUntil.HasValue && assignment.EffectiveUntil < DateTime.UtcNow)
            {
                _logger.LogWarning("Cannot reactivate expired assignment {AssignmentId}", assignmentId);
                return false;
            }

            assignment.Status = "Active";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reactivated assignment {AssignmentId}", assignmentId);

            return true;
        }

        #endregion

        #region Target Preview

        public async Task<AssignmentTargetPreviewDto> GetTargetPreviewAsync(AssignmentCreateDto dto)
        {
            var preview = new AssignmentTargetPreviewDto
            {
                AssignmentType = dto.AssignmentType
            };

            switch (dto.AssignmentType)
            {
                case "All":
                    var allTenants = await _context.Tenants.Where(t => t.IsActive).ToListAsync();
                    preview.Targets = allTenants.Select(t => new TargetItemDto
                    {
                        Id = t.TenantId,
                        Name = t.TenantName,
                        Type = "Tenant"
                    }).ToList();
                    break;

                case "TenantType":
                    var tenantsByType = await _context.Tenants
                        .Where(t => t.IsActive && t.TenantType == dto.TenantType)
                        .ToListAsync();
                    preview.Targets = tenantsByType.Select(t => new TargetItemDto
                    {
                        Id = t.TenantId,
                        Name = t.TenantName,
                        Type = "Tenant"
                    }).ToList();
                    break;

                case "TenantGroup":
                    var groupMembers = await _context.TenantGroupMembers
                        .Include(m => m.Tenant)
                        .Where(m => m.TenantGroupId == dto.TenantGroupId && m.Tenant.IsActive)
                        .ToListAsync();
                    preview.Targets = groupMembers.Select(m => new TargetItemDto
                    {
                        Id = m.TenantId,
                        Name = m.Tenant.TenantName,
                        Type = "Tenant"
                    }).ToList();
                    break;

                case "SpecificTenant":
                    var tenant = await _context.Tenants.FindAsync(dto.TenantId);
                    if (tenant != null)
                    {
                        preview.Targets.Add(new TargetItemDto
                        {
                            Id = tenant.TenantId,
                            Name = tenant.TenantName,
                            Type = "Tenant"
                        });
                    }
                    break;

                case "Role":
                    var usersByRole = await _context.UserRoles
                        .Include(ur => ur.User)
                        .Where(ur => ur.RoleId == dto.RoleId && ur.User.IsActive)
                        .ToListAsync();
                    preview.Targets = usersByRole.Select(ur => new TargetItemDto
                    {
                        Id = ur.UserId,
                        Name = ur.User.FullName,
                        Type = "User",
                        Email = ur.User.Email
                    }).ToList();
                    break;

                case "Department":
                    var usersByDept = await _context.Users
                        .Where(u => u.DepartmentId == dto.DepartmentId && u.IsActive)
                        .ToListAsync();
                    preview.Targets = usersByDept.Select(u => new TargetItemDto
                    {
                        Id = u.UserId,
                        Name = u.FullName,
                        Type = "User",
                        Email = u.Email
                    }).ToList();
                    break;

                case "UserGroup":
                    var groupUsers = await _context.UserGroupMembers
                        .Include(m => m.User)
                        .Where(m => m.UserGroupId == dto.UserGroupId && m.User.IsActive)
                        .ToListAsync();
                    preview.Targets = groupUsers.Select(m => new TargetItemDto
                    {
                        Id = m.UserId,
                        Name = m.User.FullName,
                        Type = "User",
                        Email = m.User.Email
                    }).ToList();
                    break;

                case "SpecificUser":
                    var user = await _context.Users.FindAsync(dto.UserId);
                    if (user != null)
                    {
                        preview.Targets.Add(new TargetItemDto
                        {
                            Id = user.UserId,
                            Name = user.FullName,
                            Type = "User",
                            Email = user.Email
                        });
                    }
                    break;
            }

            preview.TotalTargets = preview.Targets.Count;
            return preview;
        }

        public async Task<int> GetTargetCountAsync(string assignmentType, int? targetId, string? tenantType = null)
        {
            return assignmentType switch
            {
                "All" => await _context.Tenants.CountAsync(t => t.IsActive),
                "TenantType" => await _context.Tenants.CountAsync(t => t.IsActive && t.TenantType == tenantType),
                "TenantGroup" => await _context.TenantGroupMembers.CountAsync(m => m.TenantGroupId == targetId),
                "SpecificTenant" => 1,
                "Role" => await _context.UserRoles.CountAsync(ur => ur.RoleId == targetId && ur.User.IsActive),
                "Department" => await _context.Users.CountAsync(u => u.DepartmentId == targetId && u.IsActive),
                "UserGroup" => await _context.UserGroupMembers.CountAsync(m => m.UserGroupId == targetId && m.User.IsActive),
                "SpecificUser" => 1,
                _ => 0
            };
        }

        public async Task<List<TargetOptionDto>> GetTargetOptionsAsync(System.Security.Claims.ClaimsPrincipal user, string assignmentType)
        {
            var options = new List<TargetOptionDto>();

            // Use IScopeService to get accessible tenant IDs (handles all scope logic + exceptions)
            var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(user);

            switch (assignmentType.ToLower())
            {
                case "all":
                    // Return count of accessible tenants
                    var allCount = accessibleTenantIds.Count;
                    options.Add(new TargetOptionDto
                    {
                        Id = 0,
                        Value = "all",
                        Label = "All Accessible Tenants",
                        Description = $"Assign to all {allCount} tenants in your scope",
                        Icon = "ri-building-line",
                        Count = allCount
                    });
                    break;

                case "tenanttype":
                    // Return distinct tenant types from accessible tenants
                    var tenantTypes = await _context.Tenants
                        .Where(t => t.IsActive && accessibleTenantIds.Contains(t.TenantId))
                        .GroupBy(t => t.TenantType)
                        .Select(g => new { Type = g.Key, Count = g.Count() })
                        .ToListAsync();

                    foreach (var tt in tenantTypes)
                    {
                        options.Add(new TargetOptionDto
                        {
                            Id = 0,
                            Value = tt.Type,
                            Label = tt.Type,
                            Description = $"{tt.Count} tenants",
                            Icon = GetTenantTypeIcon(tt.Type),
                            Count = tt.Count
                        });
                    }
                    break;

                case "tenantgroup":
                    // Return tenant groups that contain accessible tenants
                    var groups = await _context.TenantGroups
                        .Where(g => g.IsActive && g.Members.Any(m => accessibleTenantIds.Contains(m.TenantId)))
                        .Select(g => new
                        {
                            g.TenantGroupId,
                            g.GroupName,
                            g.Description,
                            MemberCount = g.Members.Count(m => accessibleTenantIds.Contains(m.TenantId))
                        })
                        .OrderBy(g => g.GroupName)
                        .ToListAsync();

                    foreach (var g in groups)
                    {
                        options.Add(new TargetOptionDto
                        {
                            Id = g.TenantGroupId,
                            Value = g.TenantGroupId.ToString(),
                            Label = g.GroupName,
                            Description = g.Description,
                            Icon = "ri-group-line",
                            Count = g.MemberCount
                        });
                    }
                    break;

                case "specifictenant":
                    // Return accessible tenants grouped by region
                    var tenants = await _context.Tenants
                        .Include(t => t.Region)
                        .Where(t => t.IsActive && accessibleTenantIds.Contains(t.TenantId))
                        .OrderBy(t => t.Region.RegionName)
                        .ThenBy(t => t.TenantName)
                        .ToListAsync();

                    foreach (var t in tenants)
                    {
                        options.Add(new TargetOptionDto
                        {
                            Id = t.TenantId,
                            Value = t.TenantId.ToString(),
                            Label = t.TenantName,
                            Description = t.TenantCode,
                            Icon = GetTenantTypeIcon(t.TenantType),
                            Group = t.Region?.RegionName ?? "No Region"
                        });
                    }
                    break;

                case "role":
                    // Return all active roles (roles are global)
                    var roles = await _context.Roles
                        .Where(r => r.IsActive)
                        .OrderBy(r => r.RoleName)
                        .ToListAsync();

                    foreach (var r in roles)
                    {
                        // Count users with this role in accessible tenants
                        var userCount = await _context.UserRoles
                            .CountAsync(ur => ur.RoleId == r.RoleId && 
                                             ur.User.IsActive && 
                                             accessibleTenantIds.Contains(ur.User.TenantId));

                        options.Add(new TargetOptionDto
                        {
                            Id = r.RoleId,
                            Value = r.RoleId.ToString(),
                            Label = r.RoleName,
                            Description = r.Description,
                            Icon = "ri-shield-user-line",
                            Count = userCount
                        });
                    }
                    break;

                case "department":
                    // Return departments from accessible tenants
                    var departments = await _context.Departments
                        .Include(d => d.Tenant)
                        .Where(d => d.IsActive && accessibleTenantIds.Contains(d.TenantId))
                        .OrderBy(d => d.Tenant.TenantName)
                        .ThenBy(d => d.DepartmentName)
                        .ToListAsync();

                    foreach (var d in departments)
                    {
                        var userCount = await _context.Users
                            .CountAsync(u => u.DepartmentId == d.DepartmentId && u.IsActive);

                        options.Add(new TargetOptionDto
                        {
                            Id = d.DepartmentId,
                            Value = d.DepartmentId.ToString(),
                            Label = d.DepartmentName,
                            Description = d.DepartmentCode,
                            Icon = "ri-building-2-line",
                            Group = d.Tenant?.TenantName ?? "Unknown",
                            Count = userCount
                        });
                    }
                    break;

                case "usergroup":
                    // Return user groups with members in accessible tenants
                    var userGroups = await _context.UserGroups
                        .Where(g => g.IsActive && g.Members.Any(m => accessibleTenantIds.Contains(m.User.TenantId)))
                        .Select(g => new
                        {
                            g.UserGroupId,
                            g.GroupName,
                            g.Description,
                            MemberCount = g.Members.Count(m => m.User.IsActive && accessibleTenantIds.Contains(m.User.TenantId))
                        })
                        .OrderBy(g => g.GroupName)
                        .ToListAsync();

                    foreach (var g in userGroups)
                    {
                        options.Add(new TargetOptionDto
                        {
                            Id = g.UserGroupId,
                            Value = g.UserGroupId.ToString(),
                            Label = g.GroupName,
                            Description = g.Description,
                            Icon = "ri-team-line",
                            Count = g.MemberCount
                        });
                    }
                    break;

                case "specificuser":
                    // Return users from accessible tenants
                    // Note: FullName is a computed [NotMapped] property, so we order by FirstName/LastName
                    var users = await _context.Users
                        .Include(u => u.PrimaryTenant)
                        .Include(u => u.Department)
                        .Where(u => u.IsActive && accessibleTenantIds.Contains(u.TenantId))
                        .OrderBy(u => u.PrimaryTenant.TenantName)
                        .ThenBy(u => u.FirstName)
                        .ThenBy(u => u.LastName)
                        .Take(500) // Limit for performance
                        .ToListAsync();

                    foreach (var u in users)
                    {
                        var groupName = u.PrimaryTenant?.TenantName ?? "Unknown";
                        var description = u.Department != null 
                            ? $"{u.Email} • {u.Department.DepartmentName}"
                            : u.Email;

                        options.Add(new TargetOptionDto
                        {
                            Id = u.UserId,
                            Value = u.UserId.ToString(),
                            Label = u.FullName,
                            Description = description,
                            Icon = "ri-user-line",
                            Group = groupName
                        });
                    }
                    break;
            }

            return options;
        }

        private string GetTenantTypeIcon(string tenantType)
        {
            return tenantType?.ToLower() switch
            {
                "headoffice" => "ri-building-4-line",
                "factory" => "ri-building-line",
                "subsidiary" => "ri-building-2-line",
                "warehouse" => "ri-store-2-line",
                _ => "ri-building-line"
            };
        }

        #endregion

        #region Statistics & Reporting

        public async Task<AssignmentStatisticsDto> GetAssignmentStatisticsAsync(int? templateId = null)
        {
            var query = _context.FormTemplateAssignments.AsQueryable();

            if (templateId.HasValue)
                query = query.Where(a => a.TemplateId == templateId.Value);

            var assignments = await query.ToListAsync();
            var now = DateTime.UtcNow;

            return new AssignmentStatisticsDto
            {
                TotalAssignments = assignments.Count,
                ActiveAssignments = assignments.Count(a => a.Status == "Active"),
                SuspendedAssignments = assignments.Count(a => a.Status == "Suspended"),
                RevokedAssignments = assignments.Count(a => a.Status == "Revoked"),
                ExpiredAssignments = assignments.Count(a => a.EffectiveUntil.HasValue && a.EffectiveUntil < now),
                EffectiveAssignments = assignments.Count(a => a.Status == "Active" && 
                                                             a.EffectiveFrom <= now && 
                                                             (!a.EffectiveUntil.HasValue || a.EffectiveUntil >= now)),
                AnonymousAssignments = assignments.Count(a => a.AllowAnonymous),
                ByAssignmentType = assignments.GroupBy(a => a.AssignmentType).ToDictionary(g => g.Key, g => g.Count()),
                ByStatus = assignments.GroupBy(a => a.Status).ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public async Task<AssignmentStatisticsDto> GetComplianceMetricsAsync(AssignmentFilterDto filter)
        {
            // Reuse GetAssignmentStatisticsAsync with filter applied
            return await GetAssignmentStatisticsAsync(filter.TemplateId);
        }

        #endregion

        #region User Access

        public async Task<bool> CheckUserAccessAsync(int userId, int templateId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.GroupMemberships)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return false;

            var now = DateTime.UtcNow;
            var assignments = await _context.FormTemplateAssignments
                .Where(a => a.TemplateId == templateId && 
                           a.Status == "Active" &&
                           a.EffectiveFrom <= now &&
                           (!a.EffectiveUntil.HasValue || a.EffectiveUntil >= now))
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                var hasAccess = assignment.AssignmentType switch
                {
                    "All" => true,
                    "TenantType" => await _context.Tenants.AnyAsync(t => t.TenantId == user.TenantId && t.TenantType == assignment.TenantType),
                    "TenantGroup" => await _context.TenantGroupMembers.AnyAsync(m => m.TenantGroupId == assignment.TenantGroupId && m.TenantId == user.TenantId),
                    "SpecificTenant" => user.TenantId == assignment.TenantId,
                    "Role" => user.UserRoles.Any(ur => ur.RoleId == assignment.RoleId),
                    "Department" => user.DepartmentId == assignment.DepartmentId,
                    "UserGroup" => user.GroupMemberships.Any(m => m.UserGroupId == assignment.UserGroupId),
                    "SpecificUser" => user.UserId == assignment.UserId,
                    _ => false
                };

                if (hasAccess)
                    return true;
            }

            return false;
        }

        public async Task<List<UserAssignmentDto>> GetUserAssignmentsAsync(int userId, int? templateId = null)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.GroupMemberships)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return new List<UserAssignmentDto>();

            var now = DateTime.UtcNow;
            var query = _context.FormTemplateAssignments
                .Include(a => a.Template)
                    .ThenInclude(t => t.Category)
                .Where(a => a.Status == "Active" &&
                           a.EffectiveFrom <= now &&
                           (!a.EffectiveUntil.HasValue || a.EffectiveUntil >= now));

            if (templateId.HasValue)
                query = query.Where(a => a.TemplateId == templateId.Value);

            var assignments = await query.ToListAsync();
            var result = new List<UserAssignmentDto>();

            foreach (var assignment in assignments)
            {
                var hasAccess = await CheckUserAccessForAssignment(user, assignment);
                if (!hasAccess)
                    continue;

                // Check for existing submission
                var submission = await _context.FormTemplateSubmissions
                    .FirstOrDefaultAsync(s => s.TemplateId == assignment.TemplateId &&
                                             s.SubmittedBy == userId);

                result.Add(new UserAssignmentDto
                {
                    AssignmentId = assignment.AssignmentId,
                    TemplateId = assignment.TemplateId,
                    TemplateName = assignment.Template.TemplateName,
                    TemplateCode = assignment.Template.TemplateCode,
                    CategoryName = assignment.Template.Category.CategoryName,
                    EffectiveFrom = assignment.EffectiveFrom,
                    EffectiveUntil = assignment.EffectiveUntil,
                    AllowAnonymous = assignment.AllowAnonymous,
                    Status = assignment.Status,
                    HasSubmission = submission != null,
                    SubmissionId = submission?.SubmissionId,
                    SubmissionStatus = submission?.Status
                });
            }

            return result.OrderBy(a => a.TemplateName).ToList();
        }

        public async Task<List<UserAssignmentDto>> GetUserPendingAssignmentsAsync(int userId)
        {
            var assignments = await GetUserAssignmentsAsync(userId);
            return assignments
                .Where(a => a.IsEffective && (!a.HasSubmission || a.SubmissionStatus == "Draft"))
                .OrderBy(a => a.TemplateName)
                .ToList();
        }

        #endregion

        #region Bulk Operations

        public async Task<int> SendRemindersAsync(List<int> assignmentIds)
        {
            // TODO: Reminders will be handled by FormTemplateSubmissionRule
            // This method is kept for backward compatibility but will be refactored
            _logger.LogInformation("SendRemindersAsync called for {Count} assignments - to be implemented with SubmissionRules", assignmentIds.Count);
            return 0;
        }

        public async Task<int> BulkExtendEffectivePeriodsAsync(List<int> assignmentIds, DateTime newEffectiveUntil)
        {
            var assignments = await _context.FormTemplateAssignments
                .Where(a => assignmentIds.Contains(a.AssignmentId))
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                assignment.EffectiveUntil = newEffectiveUntil;
                // Reactivate if was revoked due to expiry
                if (assignment.Status == "Revoked" && newEffectiveUntil > DateTime.UtcNow)
                    assignment.Status = "Active";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Extended effective periods for {Count} assignments to {NewDate}", assignments.Count, newEffectiveUntil);

            return assignments.Count;
        }

        public async Task<int> BulkCancelAsync(List<int> assignmentIds, string reason)
        {
            var userId = _claimsService.GetUserId();
            var assignments = await _context.FormTemplateAssignments
                .Where(a => assignmentIds.Contains(a.AssignmentId))
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                assignment.Status = "Revoked";
                assignment.CancelledBy = userId;
                assignment.CancelledDate = DateTime.UtcNow;
                assignment.CancellationReason = reason;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk cancelled {Count} assignments: {Reason}", assignments.Count, reason);

            return assignments.Count;
        }

        #endregion

        #region Background Jobs

        public async Task<int> ProcessExpiredAssignmentsAsync()
        {
            var now = DateTime.UtcNow;
            var expiredAssignments = await _context.FormTemplateAssignments
                .Where(a => a.Status == "Active" &&
                           a.EffectiveUntil.HasValue &&
                           a.EffectiveUntil < now)
                .ToListAsync();

            foreach (var assignment in expiredAssignments)
            {
                assignment.Status = "Revoked";
            }

            await _context.SaveChangesAsync();

            if (expiredAssignments.Any())
            {
                _logger.LogInformation("Marked {Count} assignments as revoked due to expiry", expiredAssignments.Count);
            }

            return expiredAssignments.Count;
        }

        public async Task<int> SendDueDateRemindersAsync()
        {
            // TODO: Due date reminders will be handled by FormTemplateSubmissionRule
            // This method is kept for backward compatibility but will be refactored
            _logger.LogInformation("SendDueDateRemindersAsync called - to be implemented with SubmissionRules");
            return 0;
        }

        #endregion

        #region Private Helpers

        private static string GetTargetName(FormTemplateAssignment assignment)
        {
            return assignment.AssignmentType switch
            {
                "All" => "All Tenants",
                "TenantType" => $"All {assignment.TenantType} Tenants",
                "TenantGroup" => assignment.TenantGroup?.GroupName ?? "Unknown Group",
                "SpecificTenant" => assignment.Tenant?.TenantName ?? "Unknown Tenant",
                "Role" => assignment.Role?.RoleName ?? "Unknown Role",
                "Department" => assignment.Department?.DepartmentName ?? "Unknown Department",
                "UserGroup" => assignment.UserGroup?.GroupName ?? "Unknown Group",
                "SpecificUser" => assignment.User?.FullName ?? "Unknown User",
                _ => "Unknown"
            };
        }

        private static int? GetTargetId(FormTemplateAssignment assignment)
        {
            return assignment.AssignmentType switch
            {
                "TenantGroup" => assignment.TenantGroupId,
                "SpecificTenant" => assignment.TenantId,
                "Role" => assignment.RoleId,
                "Department" => assignment.DepartmentId,
                "UserGroup" => assignment.UserGroupId,
                "SpecificUser" => assignment.UserId,
                _ => null
            };
        }

        private static string? GetTargetDetail(FormTemplateAssignment assignment)
        {
            return assignment.AssignmentType switch
            {
                "All" => "All tenants in scope",
                "TenantType" => $"All {assignment.TenantType} tenants",
                "TenantGroup" => assignment.TenantGroup?.Description,
                "SpecificTenant" => assignment.Tenant?.TenantCode,
                "Role" => assignment.Role?.Description,
                "Department" => assignment.Department?.Tenant?.TenantName,
                "UserGroup" => assignment.UserGroup?.Description,
                "SpecificUser" => assignment.User?.Email,
                _ => null
            };
        }

        private async Task<bool> CheckUserAccessForAssignment(Models.Entities.Identity.User user, FormTemplateAssignment assignment)
        {
            return assignment.AssignmentType switch
            {
                "All" => true,
                "TenantType" => await _context.Tenants.AnyAsync(t => t.TenantId == user.TenantId && t.TenantType == assignment.TenantType),
                "TenantGroup" => await _context.TenantGroupMembers.AnyAsync(m => m.TenantGroupId == assignment.TenantGroupId && m.TenantId == user.TenantId),
                "SpecificTenant" => user.TenantId == assignment.TenantId,
                "Role" => user.UserRoles.Any(ur => ur.RoleId == assignment.RoleId),
                "Department" => user.DepartmentId == assignment.DepartmentId,
                "UserGroup" => user.GroupMemberships.Any(m => m.UserGroupId == assignment.UserGroupId),
                "SpecificUser" => user.UserId == assignment.UserId,
                _ => false
            };
        }

        /// <summary>
        /// Validate if a submission can be made based on assignment access period.
        /// Due date validation will be handled by FormTemplateSubmissionRule.
        /// </summary>
        public async Task<SubmissionValidationResult> ValidateSubmissionAsync(int assignmentId, DateTime submissionDate)
        {
            var assignment = await _context.FormTemplateAssignments.FindAsync(assignmentId);
            if (assignment == null)
            {
                return new SubmissionValidationResult
                {
                    CanSubmit = false,
                    BlockReason = "Assignment not found"
                };
            }

            // Check if assignment is effective
            if (assignment.Status != "Active")
            {
                return new SubmissionValidationResult
                {
                    CanSubmit = false,
                    BlockReason = "Assignment is not active"
                };
            }

            if (assignment.EffectiveFrom > submissionDate)
            {
                return new SubmissionValidationResult
                {
                    CanSubmit = false,
                    BlockReason = $"Assignment is not yet effective. Starts on {assignment.EffectiveFrom:MMM d, yyyy}"
                };
            }

            if (assignment.EffectiveUntil.HasValue && assignment.EffectiveUntil < submissionDate)
            {
                return new SubmissionValidationResult
                {
                    CanSubmit = false,
                    BlockReason = $"Assignment has expired on {assignment.EffectiveUntil:MMM d, yyyy}"
                };
            }

            // Assignment is valid - due date validation will be handled by SubmissionRule
            return new SubmissionValidationResult
            {
                CanSubmit = true,
                IsLate = false
            };
        }

        #endregion

        #region Dual-Mode Assignment Validation Methods

        public async Task<bool> CanUserCreateSubmissionAsync(int templateId, int userId)
        {
            var template = await _context.FormTemplates.FindAsync(templateId);
            if (template?.SubmissionMode != Models.Common.SubmissionMode.Individual)
            {
                return false; // Only Individual mode uses assignment-based access control
            }

            // Anonymous access overrides all assignment checks
            if (template.AllowAnonymousAccess)
            {
                return true; // Anonymous forms allow anyone to create submissions
            }

            // Get active assignments for this template
            var activeAssignments = await GetActiveAssignmentsForTemplateAsync(templateId);
            if (!activeAssignments.Any())
            {
                return false; // No assignments = no access (except for anonymous)
            }

            // Check if user matches any assignment
            foreach (var assignment in activeAssignments)
            {
                if (await CheckUserMatchesAssignmentAsync(userId, null, assignment))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<AssignmentCoverageValidationDto> ValidateAssignmentCoverageAsync(int templateId)
        {
            var result = new AssignmentCoverageValidationDto();
            
            var template = await _context.FormTemplates.FindAsync(templateId);
            if (template == null)
            {
                result.CoverageIssues.Add("Template not found");
                return result;
            }

            result.SubmissionMode = template.SubmissionMode.ToString();
            
            var activeAssignments = await GetActiveAssignmentsForTemplateAsync(templateId);
            result.ActiveAssignmentCount = activeAssignments.Count;

            // Build assignment details
            foreach (var assignment in activeAssignments)
            {
                var detail = new AssignmentCoverageDetailDto
                {
                    AssignmentId = assignment.AssignmentId,
                    AssignmentType = assignment.AssignmentType,
                    TargetName = GetAssignmentTargetName(assignment),
                    EstimatedUserCount = await EstimateUserCountForAssignmentAsync(assignment),
                    IsActive = assignment.Status == "Active",
                    EffectiveFrom = assignment.EffectiveFrom,
                    EffectiveUntil = assignment.EffectiveUntil
                };
                result.AssignmentDetails.Add(detail);
                result.PotentialUserCount += detail.EstimatedUserCount;
            }

            // Validate based on submission mode
            if (template.SubmissionMode == Models.Common.SubmissionMode.Individual)
            {
                // Individual mode REQUIRES assignments
                if (!activeAssignments.Any())
                {
                    result.CoverageIssues.Add("Individual mode requires at least one active assignment to control who can create submissions");
                }
                else if (result.PotentialUserCount == 0)
                {
                    result.CoverageIssues.Add("Active assignments do not resolve to any users");
                }

                result.HasSufficientCoverage = !result.CoverageIssues.Any();
            }
            else if (template.SubmissionMode == Models.Common.SubmissionMode.Collaborative)
            {
                // Collaborative mode: assignments are optional
                if (!activeAssignments.Any())
                {
                    result.Warnings.Add("No assignments found. Users may not be able to view submissions unless workflow assignees provide viewing permissions");
                }

                result.HasSufficientCoverage = true; // Always sufficient for Collaborative mode
            }

            return result;
        }

        public async Task<bool> HasSufficientAssignmentsAsync(int templateId)
        {
            var validation = await ValidateAssignmentCoverageAsync(templateId);
            return validation.HasSufficientCoverage;
        }

        public async Task<List<int>> GetUsersWithSubmissionAccessAsync(int templateId)
        {
            var template = await _context.FormTemplates.FindAsync(templateId);
            if (template?.SubmissionMode != Models.Common.SubmissionMode.Individual)
            {
                return new List<int>(); // Only Individual mode uses assignment-based access
            }

            var activeAssignments = await GetActiveAssignmentsForTemplateAsync(templateId);
            var userIds = new HashSet<int>();

            foreach (var assignment in activeAssignments)
            {
                var assignmentUserIds = await ResolveAssignmentToUserIdsAsync(assignment);
                foreach (var userId in assignmentUserIds)
                {
                    userIds.Add(userId);
                }
            }

            return userIds.ToList();
        }

        #region Private Helper Methods for Assignment Validation

        private async Task<List<FormTemplateAssignment>> GetActiveAssignmentsForTemplateAsync(int templateId)
        {
            var now = DateTime.UtcNow;
            return await _context.FormTemplateAssignments
                .Where(a => a.TemplateId == templateId &&
                           a.Status == "Active" &&
                           a.EffectiveFrom <= now &&
                           (a.EffectiveUntil == null || a.EffectiveUntil >= now))
                .ToListAsync();
        }

        private async Task<bool> CheckUserMatchesAnyAssignmentAsync(int userId, object userScope, List<FormTemplateAssignment> assignments)
        {
            foreach (var assignment in assignments)
            {
                if (await CheckUserMatchesAssignmentAsync(userId, userScope, assignment))
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> CheckUserMatchesAssignmentAsync(int userId, object userScope, FormTemplateAssignment assignment)
        {
            return assignment.AssignmentType switch
            {
                "All" => true,
                "SpecificUser" => assignment.UserId == userId,
                "Role" => assignment.RoleId.HasValue && await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == assignment.RoleId.Value),
                "Department" => assignment.DepartmentId.HasValue && await _context.Users.AnyAsync(u => u.UserId == userId && u.DepartmentId == assignment.DepartmentId.Value),
                "UserGroup" => assignment.UserGroupId.HasValue && await _context.UserGroupMembers.AnyAsync(ugm => ugm.UserId == userId && ugm.UserGroupId == assignment.UserGroupId.Value),
                "SpecificTenant" => assignment.TenantId.HasValue && await CheckUserBelongsToTenantAsync(userId, assignment.TenantId.Value),
                "TenantGroup" => assignment.TenantGroupId.HasValue && await CheckUserBelongsToTenantGroupAsync(userId, assignment.TenantGroupId.Value),
                "TenantType" => !string.IsNullOrEmpty(assignment.TenantType) && await CheckUserBelongsToTenantTypeAsync(userId, assignment.TenantType),
                _ => false
            };
        }

        private async Task<int> EstimateUserCountForAssignmentAsync(FormTemplateAssignment assignment)
        {
            return assignment.AssignmentType switch
            {
                "All" => await _context.Users.CountAsync(u => u.IsActive),
                "SpecificUser" => assignment.UserId.HasValue ? 1 : 0,
                "Role" => assignment.RoleId.HasValue ? await _context.UserRoles.CountAsync(ur => ur.RoleId == assignment.RoleId.Value) : 0,
                "Department" => assignment.DepartmentId.HasValue ? await _context.Users.CountAsync(u => u.DepartmentId == assignment.DepartmentId.Value && u.IsActive) : 0,
                "UserGroup" => assignment.UserGroupId.HasValue ? await _context.UserGroupMembers.CountAsync(ugm => ugm.UserGroupId == assignment.UserGroupId.Value) : 0,
                "SpecificTenant" => assignment.TenantId.HasValue ? await _context.Users.CountAsync(u => u.TenantId == assignment.TenantId.Value && u.IsActive) : 0,
                "TenantGroup" => assignment.TenantGroupId.HasValue ? await CountUsersInTenantGroupAsync(assignment.TenantGroupId.Value) : 0,
                "TenantType" => !string.IsNullOrEmpty(assignment.TenantType) ? await CountUsersInTenantTypeAsync(assignment.TenantType) : 0,
                _ => 0
            };
        }

        private async Task<List<int>> ResolveAssignmentToUserIdsAsync(FormTemplateAssignment assignment)
        {
            return assignment.AssignmentType switch
            {
                "All" => await _context.Users.Where(u => u.IsActive).Select(u => u.UserId).ToListAsync(),
                "SpecificUser" => assignment.UserId.HasValue ? new List<int> { assignment.UserId.Value } : new List<int>(),
                "Role" => assignment.RoleId.HasValue ? await _context.UserRoles.Where(ur => ur.RoleId == assignment.RoleId.Value).Select(ur => ur.UserId).ToListAsync() : new List<int>(),
                "Department" => assignment.DepartmentId.HasValue ? await _context.Users.Where(u => u.DepartmentId == assignment.DepartmentId.Value && u.IsActive).Select(u => u.UserId).ToListAsync() : new List<int>(),
                "UserGroup" => assignment.UserGroupId.HasValue ? await _context.UserGroupMembers.Where(ugm => ugm.UserGroupId == assignment.UserGroupId.Value).Select(ugm => ugm.UserId).ToListAsync() : new List<int>(),
                "SpecificTenant" => assignment.TenantId.HasValue ? await _context.Users.Where(u => u.TenantId == assignment.TenantId.Value && u.IsActive).Select(u => u.UserId).ToListAsync() : new List<int>(),
                "TenantGroup" => assignment.TenantGroupId.HasValue ? await GetUsersInTenantGroupAsync(assignment.TenantGroupId.Value) : new List<int>(),
                "TenantType" => !string.IsNullOrEmpty(assignment.TenantType) ? await GetUsersInTenantTypeAsync(assignment.TenantType) : new List<int>(),
                _ => new List<int>()
            };
        }

        private async Task<bool> CheckUserBelongsToTenantAsync(int userId, int tenantId)
        {
            return await _context.Users.AnyAsync(u => u.UserId == userId && u.TenantId == tenantId);
        }

        private async Task<bool> CheckUserBelongsToTenantGroupAsync(int userId, int tenantGroupId)
        {
            return await _context.Users
                .Where(u => u.UserId == userId)
                .Join(_context.TenantGroupMembers, u => u.TenantId, tgm => tgm.TenantId, (u, tgm) => tgm)
                .AnyAsync(tgm => tgm.TenantGroupId == tenantGroupId);
        }

        private async Task<bool> CheckUserBelongsToTenantTypeAsync(int userId, string tenantType)
        {
            return await _context.Users
                .Where(u => u.UserId == userId)
                .Join(_context.Tenants, u => u.TenantId, t => t.TenantId, (u, t) => t)
                .AnyAsync(t => t.TenantType == tenantType);
        }

        private async Task<int> CountUsersInTenantGroupAsync(int tenantGroupId)
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .Join(_context.TenantGroupMembers, u => u.TenantId, tgm => tgm.TenantId, (u, tgm) => tgm)
                .CountAsync(tgm => tgm.TenantGroupId == tenantGroupId);
        }

        private async Task<int> CountUsersInTenantTypeAsync(string tenantType)
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .Join(_context.Tenants, u => u.TenantId, t => t.TenantId, (u, t) => t)
                .CountAsync(t => t.TenantType == tenantType);
        }

        private async Task<List<int>> GetUsersInTenantGroupAsync(int tenantGroupId)
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .Join(_context.TenantGroupMembers, u => u.TenantId, tgm => tgm.TenantId, (u, tgm) => new { u.UserId, tgm.TenantGroupId })
                .Where(x => x.TenantGroupId == tenantGroupId)
                .Select(x => x.UserId)
                .ToListAsync();
        }

        private async Task<List<int>> GetUsersInTenantTypeAsync(string tenantType)
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .Join(_context.Tenants, u => u.TenantId, t => t.TenantId, (u, t) => new { u.UserId, t.TenantType })
                .Where(x => x.TenantType == tenantType)
                .Select(x => x.UserId)
                .ToListAsync();
        }

        private string GetAssignmentTargetName(FormTemplateAssignment assignment)
        {
            return assignment.AssignmentType switch
            {
                "All" => "All Users",
                "SpecificUser" => assignment.User?.FullName ?? "Unknown User",
                "Role" => assignment.Role?.RoleName ?? "Unknown Role",
                "Department" => assignment.Department?.DepartmentName ?? "Unknown Department",
                "UserGroup" => assignment.UserGroup?.GroupName ?? "Unknown User Group",
                "SpecificTenant" => assignment.Tenant?.TenantName ?? "Unknown Tenant",
                "TenantGroup" => assignment.TenantGroup?.GroupName ?? "Unknown Tenant Group",
                "TenantType" => assignment.TenantType ?? "Unknown Tenant Type",
                _ => "Unknown Target"
            };
        }

        private System.Security.Claims.ClaimsPrincipal GetUserClaimsPrincipal(int userId)
        {
            // This is a simplified version - in real implementation, you'd need to build proper claims
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("UserId", userId.ToString())
            };
            return new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(claims));
        }

        #endregion

        #endregion
    }
}

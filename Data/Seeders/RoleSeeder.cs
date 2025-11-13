using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Data.Seeders
{
    /// <summary>
    /// Seeds initial role data with scope-based access control
    /// </summary>
    public static class RoleSeeder
    {
        /// <summary>
        /// Seeds roles if they don't already exist
        /// Requires ScopeLevels to be seeded first
        /// </summary>
        public static void SeedRoles(ApplicationDbContext context)
        {
            // Check if roles already exist
            if (context.Roles.Any())
            {
                return; // Data already seeded
            }

            // Get scope level IDs
            var globalScope = context.ScopeLevels.First(s => s.ScopeCode == "GLOBAL");
            var regionalScope = context.ScopeLevels.First(s => s.ScopeCode == "REGIONAL");
            var tenantScope = context.ScopeLevels.First(s => s.ScopeCode == "TENANT");
            var departmentScope = context.ScopeLevels.First(s => s.ScopeCode == "DEPARTMENT");
            var deptGroupScope = context.ScopeLevels.First(s => s.ScopeCode == "DEPT_GROUP");
            var individualScope = context.ScopeLevels.First(s => s.ScopeCode == "INDIVIDUAL");

            var roles = new List<Role>
            {
                // ============================================================================
                // GLOBAL SCOPE ROLES (Level 1) - Access to entire organization
                // ============================================================================
                new Role
                {
                    RoleName = "Executive",
                    RoleCode = "EXECUTIVE",
                    Description = "C-level executives with global view across all tenants, regions, and departments",
                    ScopeLevelId = globalScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "System Administrator",
                    RoleCode = "SYSTEM_ADMIN",
                    Description = "Full system access for configuration, user management, and technical administration",
                    ScopeLevelId = globalScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Head Office ICT Manager",
                    RoleCode = "HO_ICT_MGR",
                    Description = "Head office ICT leadership with global oversight of all ICT operations",
                    ScopeLevelId = globalScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Head Office HR Manager",
                    RoleCode = "HO_HR_MGR",
                    Description = "Head office HR leadership with global oversight of all HR operations and employee data",
                    ScopeLevelId = globalScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Head Office Finance Manager",
                    RoleCode = "HO_FIN_MGR",
                    Description = "Head office finance leadership with global oversight of all financial operations",
                    ScopeLevelId = globalScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Auditor",
                    RoleCode = "AUDITOR",
                    Description = "Internal/external auditor with read-only access across entire organization",
                    ScopeLevelId = globalScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================================
                // REGIONAL SCOPE ROLES (Level 2) - Access to all tenants in assigned region(s)
                // ============================================================================
                new Role
                {
                    RoleName = "Regional Manager",
                    RoleCode = "REGIONAL_MGR",
                    Description = "Regional leadership with oversight of all factories and operations in assigned region",
                    ScopeLevelId = regionalScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Regional ICT Coordinator",
                    RoleCode = "REGIONAL_ICT",
                    Description = "Regional ICT support and coordination across all factories in region",
                    ScopeLevelId = regionalScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Regional HR Coordinator",
                    RoleCode = "REGIONAL_HR",
                    Description = "Regional HR support and coordination across all factories in region",
                    ScopeLevelId = regionalScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================================
                // TENANT SCOPE ROLES (Level 3) - Access to single factory/subsidiary
                // ============================================================================
                new Role
                {
                    RoleName = "Factory Manager",
                    RoleCode = "FACTORY_MGR",
                    Description = "Factory leadership with full access to all departments and operations in their factory",
                    ScopeLevelId = tenantScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Factory ICT Officer",
                    RoleCode = "FACTORY_ICT",
                    Description = "Factory-level ICT support and administration for assigned factory",
                    ScopeLevelId = tenantScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Factory HR Officer",
                    RoleCode = "FACTORY_HR",
                    Description = "Factory-level HR administration for assigned factory",
                    ScopeLevelId = tenantScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Factory Finance Officer",
                    RoleCode = "FACTORY_FIN",
                    Description = "Factory-level finance administration for assigned factory",
                    ScopeLevelId = tenantScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================================
                // DEPARTMENT SCOPE ROLES (Level 4) - Access to single department
                // ============================================================================
                new Role
                {
                    RoleName = "Head of Department",
                    RoleCode = "HOD",
                    Description = "Department head with full access to their department's data and team members",
                    ScopeLevelId = departmentScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Department Supervisor",
                    RoleCode = "DEPT_SUPERVISOR",
                    Description = "Department supervisor with access to their department's operational data",
                    ScopeLevelId = departmentScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Training Coordinator",
                    RoleCode = "TRAINING_COORD",
                    Description = "Manages training programs, schedules, and records for their department",
                    ScopeLevelId = departmentScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Appraisal Coordinator",
                    RoleCode = "APPRAISAL_COORD",
                    Description = "Manages performance appraisals and reviews for their department",
                    ScopeLevelId = departmentScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================================
                // DEPARTMENT GROUP SCOPE (Level 4) - Cross-tenant department view
                // ============================================================================
                new Role
                {
                    RoleName = "Group Department Manager",
                    RoleCode = "GROUP_DEPT_MGR",
                    Description = "Manages same department across all tenants (e.g., all ICT departments organization-wide)",
                    ScopeLevelId = deptGroupScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================================
                // INDIVIDUAL SCOPE ROLES (Level 6) - Self-service only
                // ============================================================================
                new Role
                {
                    RoleName = "Employee",
                    RoleCode = "EMPLOYEE",
                    Description = "Standard employee with access to own data, forms, and self-service functions",
                    ScopeLevelId = individualScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Viewer",
                    RoleCode = "VIEWER",
                    Description = "Read-only access to own data and assigned resources",
                    ScopeLevelId = individualScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Contractor",
                    RoleCode = "CONTRACTOR",
                    Description = "Temporary/contract worker with limited self-service access",
                    ScopeLevelId = individualScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================================
                // SPECIALIZED FUNCTIONAL ROLES
                // ============================================================================
                new Role
                {
                    RoleName = "Helpdesk Agent",
                    RoleCode = "HELPDESK",
                    Description = "IT support staff handling tickets and user requests within their scope",
                    ScopeLevelId = tenantScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Asset Manager",
                    RoleCode = "ASSET_MGR",
                    Description = "Manages hardware and software assets within their scope",
                    ScopeLevelId = tenantScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Report Analyst",
                    RoleCode = "REPORT_ANALYST",
                    Description = "Creates and analyzes reports within their scope",
                    ScopeLevelId = departmentScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Role
                {
                    RoleName = "Data Entry Clerk",
                    RoleCode = "DATA_ENTRY",
                    Description = "Data entry and form submission within their department",
                    ScopeLevelId = departmentScope.ScopeLevelId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.Roles.AddRange(roles);
            context.SaveChanges();
        }
    }
}

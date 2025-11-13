using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Data.Seeders
{
    /// <summary>
    /// Seeds initial scope level data for hierarchical access control
    /// </summary>
    public static class ScopeLevelSeeder
    {
        /// <summary>
        /// Seeds scope levels if they don't already exist
        /// </summary>
        public static void SeedScopeLevels(ApplicationDbContext context)
        {
            // Check if scope levels already exist
            if (context.ScopeLevels.Any())
            {
                return; // Data already seeded
            }

            var scopeLevels = new List<ScopeLevel>
            {
                new ScopeLevel
                {
                    ScopeName = "Global",
                    ScopeCode = "GLOBAL",
                    Level = 1,
                    Description = "Access to all data across entire organization (all tenants, regions, departments)",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ScopeLevel
                {
                    ScopeName = "Regional",
                    ScopeCode = "REGIONAL",
                    Level = 2,
                    Description = "Access to all data within assigned region(s) - all factories in region",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ScopeLevel
                {
                    ScopeName = "Tenant",
                    ScopeCode = "TENANT",
                    Level = 3,
                    Description = "Access to single tenant only (one factory/subsidiary/head office)",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ScopeLevel
                {
                    ScopeName = "Department",
                    ScopeCode = "DEPARTMENT",
                    Level = 4,
                    Description = "Access to single department within a tenant",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ScopeLevel
                {
                    ScopeName = "Department Group",
                    ScopeCode = "DEPT_GROUP",
                    Level = 4,
                    Description = "Access to same department across all tenants (cross-tenant department view)",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ScopeLevel
                {
                    ScopeName = "Team",
                    ScopeCode = "TEAM",
                    Level = 5,
                    Description = "Access to single team/group within a department",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new ScopeLevel
                {
                    ScopeName = "Individual",
                    ScopeCode = "INDIVIDUAL",
                    Level = 6,
                    Description = "Access to own data only (self-service)",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.ScopeLevels.AddRange(scopeLevels);
            context.SaveChanges();
        }
    }
}

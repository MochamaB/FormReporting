// Data/Seeders/ModuleSeeder.cs
using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Data.Seeders
{
    public static class ModuleSeeder
    {
        public static void SeedModules(ApplicationDbContext context)
        {
            if (context.Modules.Any())
                return;

            var sections = context.MenuSections.ToList();
            
            var modules = new List<Module>
            {
                // MAIN MENU
                new Module
                {
                    MenuSectionId = sections[0].MenuSectionId, // "MAIN MENU"
                    ModuleName = "Dashboards",
                    ModuleCode = "DASHBOARDS",
                    Description = "System dashboards and analytics overview",
                    Icon = "ri-dashboard-2-line",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Module
                {
                    MenuSectionId = sections[0].MenuSectionId, // "MAIN MENU"
                    ModuleName = "Forms & Submissions",
                    ModuleCode = "FORMS",
                    Description = "Form submissions and workflow management",
                    Icon = "ri-file-list-3-line",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Module
                {
                    MenuSectionId = sections[0].MenuSectionId, // "MAIN MENU"
                    ModuleName = "Reports & Analytics",
                    ModuleCode = "REPORTS",
                    Description = "Reporting and business intelligence",
                    Icon = "ri-file-chart-line",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Module
                {
                    MenuSectionId = sections[0].MenuSectionId, // "MAIN MENU"
                    ModuleName = "Support Tickets",
                    ModuleCode = "SUPPORT",
                    Description = "IT support ticket management system",
                    Icon = "ri-ticket-2-line",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ASSET MANAGEMENT SECTION
                new Module
                {
                    MenuSectionId = sections[1].MenuSectionId, // "ASSET MANAGEMENT"
                    ModuleName = "Hardware Inventory",
                    ModuleCode = "HARDWARE",
                    Description = "Physical IT asset tracking and management",
                    Icon = "ri-computer-line",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Module
                {
                    MenuSectionId = sections[1].MenuSectionId, // "ASSET MANAGEMENT"
                    ModuleName = "Software Management",
                    ModuleCode = "SOFTWARE",
                    Description = "Software assets and license management",
                    Icon = "ri-apps-line",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ORGANIZATIONAL MANAGEMENT SECTION
                new Module
                {
                    MenuSectionId = sections[2].MenuSectionId, // "ORGANIZATIONAL MANAGEMENT"
                    ModuleName = "Organizational Structure",
                    ModuleCode = "ORG_STRUCTURE",
                    Description = "Tenant, factory, and regional management",
                    Icon = "ri-building-line",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Module
                {
                    MenuSectionId = sections[2].MenuSectionId, // "ORGANIZATIONAL MANAGEMENT"
                    ModuleName = "Users & Access Management",
                    ModuleCode = "IAM",
                    Description = "User, role, and permission management",
                    Icon = "ri-user-line",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // PERFORMANCE & METRICS SECTION
                new Module
                {
                    MenuSectionId = sections[3].MenuSectionId, // "PERFORMANCE & METRICS"
                    ModuleName = "Metrics & KPI Tracking",
                    ModuleCode = "METRICS",
                    Description = "Performance metrics and KPI monitoring",
                    Icon = "ri-line-chart-line",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // FINANCIAL MANAGEMENT SECTION
                new Module
                {
                    MenuSectionId = sections[4].MenuSectionId, // "FINANCIAL MANAGEMENT"
                    ModuleName = "Financial Tracking",
                    ModuleCode = "FINANCE",
                    Description = "Budget and expenditure tracking",
                    Icon = "ri-money-dollar-circle-line",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // SYSTEM ADMINISTRATION SECTION
                new Module
                {
                    MenuSectionId = sections[5].MenuSectionId, // "SYSTEM ADMINISTRATION"
                    ModuleName = "Notifications",
                    ModuleCode = "NOTIFICATIONS",
                    Description = "Notification and alert management",
                    Icon = "ri-notification-3-line",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Module
                {
                    MenuSectionId = sections[5].MenuSectionId, // "SYSTEM ADMINISTRATION"
                    ModuleName = "Media Management",
                    ModuleCode = "MEDIA",
                    Description = "File and document management",
                    Icon = "ri-folder-line",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Module
                {
                    MenuSectionId = sections[5].MenuSectionId, // "SYSTEM ADMINISTRATION"
                    ModuleName = "System Settings",
                    ModuleCode = "SETTINGS",
                    Description = "System configuration and settings",
                    Icon = "ri-settings-3-line",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.Modules.AddRange(modules);
            context.SaveChanges();
        }
    }
}
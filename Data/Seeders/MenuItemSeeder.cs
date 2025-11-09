// Data/Seeders/MenuItemSeeder.cs
using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Data.Seeders
{
    public static class MenuItemSeeder
    {
        public static void SeedMenuItems(ApplicationDbContext context)
        {
            if (context.MenuItems.Any())
                return;

            var modules = context.Modules
                .AsNoTracking()
                .ToDictionary(m => m.ModuleCode.ToUpper(), m => m.ModuleId);

            var menuItems = new List<MenuItem>();
            int menuItemId = 1;

            // Helper method to add menu items
            int AddMenuItems(int? parentId, int moduleId, string moduleCode, string icon, string controller, List<(string Title, string Code, string Action, int Order, int Level)> items)
            {
                int? firstItemId = null;

                        foreach (var item in items)
                        {
                            var menuItem = new MenuItem
                            {
                               
                                ParentMenuItemId = parentId,
                                ModuleId = moduleId,
                                MenuTitle = item.Title,
                                MenuCode = $"{moduleCode}_{item.Code}",
                                Controller = controller,
                                Action = item.Action,
                                Icon = parentId == null ? icon : null,
                                DisplayOrder = item.Order,
                                Level = item.Level,
                                IsActive = true,
                                IsVisible = true,
                                RequiresAuth = true,
                                CreatedDate = DateTime.UtcNow
                            };

                            // Set firstItemId for parent items
                            if (firstItemId == null && parentId == null)
                            {
                                firstItemId = menuItem.MenuItemId;
                            }

                            menuItems.Add(menuItem);
                        }

                        return firstItemId ?? 0;  // Return 0 if no items were added
            }

            // ===== MAIN MENU SECTION =====

            // Dashboards Module (1)
            if (modules.TryGetValue("DASHBOARDS", out var dashboardsModuleId))
            {
                var parentId = AddMenuItems(null, dashboardsModuleId, "DASH", "ri-dashboard-2-line", "Dashboard", new()
                {
                    ("Analytics Dashboard", "ANALYTICS", "Analytics", 1, 1),
                    ("Regional Overview", "REGIONAL", "Regional", 2, 1),
                    ("Performance Metrics", "PERFORMANCE", "Performance", 3, 1),
                    ("KPI Monitoring", "KPI", "KPI", 4, 1)
                });
            }

            // Forms & Submissions Module (3)
            if (modules.TryGetValue("FORMS", out var formsModuleId))
            {
                var parentId = AddMenuItems(null, formsModuleId, "FORMS", "ri-file-list-3-line", "Forms", new()
                {
                    ("Assigned Forms", "ASSIGNED", "Assigned", 1, 1),
                    ("My Submissions", "MY", "MySubmissions", 2, 1),
                    ("Form Templates Library", "TEMPLATES", "Templates", 3, 1),
                    ("Submission Workflow", "WORKFLOW", "Workflow", 4, 1),
                    ("Approval Queue", "APPROVAL", "Approval", 5, 1)
                });
            }

            // Reports & Analytics Module (9)
            if (modules.TryGetValue("REPORTS", out var reportsModuleId))
            {
                var parentId = AddMenuItems(null, reportsModuleId, "REPORTS", "ri-file-chart-line", "Reports", new()
                {
                    ("All Reports", "ALL", "All", 1, 1),
                    ("Regional Summary", "REGIONAL", "Regional", 2, 1),
                    ("Compliance Reports", "COMPLIANCE", "Compliance", 3, 1),
                    ("Custom Reports", "CUSTOM", "Custom", 4, 1),
                    ("Report Scheduler", "SCHEDULER", "Scheduler", 5, 1),
                    ("Analytics Dashboard", "ANALYTICS", "Analytics", 6, 1)
                });
            }

            // Support Tickets Module (7)
            if (modules.TryGetValue("SUPPORT", out var supportModuleId))
            {
                var parentId = AddMenuItems(null, supportModuleId, "TICKETS", "ri-ticket-2-line", "Tickets", new()
                {
                    ("All Tickets", "ALL", "All", 1, 1),
                    ("My Tickets", "MY", "My", 2, 1),
                    ("Create Ticket", "CREATE", "Create", 3, 1),
                    ("Ticket Categories", "CATEGORIES", "Categories", 4, 1),
                    ("SLA Monitoring", "SLA", "SLA", 5, 1)
                });
            }

            // ===== ASSET MANAGEMENT SECTION =====

            // Hardware Inventory Module (6)
            if (modules.TryGetValue("HARDWARE", out var hardwareModuleId))
            {
                var parentId = AddMenuItems(null, hardwareModuleId, "HARDWARE", "ri-computer-line", "Hardware", new()
                {
                    ("All Hardware Assets", "ALL", "All", 1, 1),
                    ("Asset Assignments", "ASSIGNMENTS", "Assignments", 2, 1),
                    ("Maintenance Records", "MAINTENANCE", "Maintenance", 3, 1),
                    ("Disposal Tracking", "DISPOSAL", "Disposal", 4, 1),
                    ("Hardware Categories", "CATEGORIES", "Categories", 5, 1)
                });
            }

            // Software Management Module (5)
            if (modules.TryGetValue("SOFTWARE", out var softwareModuleId))
            {
                var parentId = AddMenuItems(null, softwareModuleId, "SOFTWARE", "ri-apps-line", "Software", new()
                {
                    ("Software Catalog", "CATALOG", "Catalog", 1, 1),
                    ("License Management", "LICENSES", "Licenses", 2, 1),
                    ("Installations", "INSTALLATIONS", "Installations", 3, 1),
                    ("Version Control", "VERSIONS", "Versions", 4, 1),
                    ("License Allocations", "ALLOCATIONS", "Allocations", 5, 1)
                });
            }

            // ===== ORGANIZATIONAL MANAGEMENT SECTION =====

            // Organizational Structure Module (1)
            if (modules.TryGetValue("ORG_STRUCTURE", out var orgStructureModuleId))
            {
                var parentId = AddMenuItems(null, orgStructureModuleId, "ORG", "ri-building-line", "Organization", new()
                {
                    ("All Tenants", "TENANTS", "Tenants", 1, 1),
                    ("Factories", "FACTORIES", "Factories", 2, 1),
                    ("Subsidiaries", "SUBSIDIARIES", "Subsidiaries", 3, 1),
                    ("Regions", "REGIONS", "Regions", 4, 1),
                    ("Tenant Groups", "GROUPS", "Groups", 5, 1),
                    ("Departments", "DEPARTMENTS", "Departments", 6, 1)
                });
            }

            // Users & Access Management Module (2)
            if (modules.TryGetValue("IAM", out var iamModuleId))
            {
                var parentId = AddMenuItems(null, iamModuleId, "IAM", "ri-user-line", "IAM", new()
                {
                    ("Users", "USERS", "Users", 1, 1),
                    ("Roles", "ROLES", "Roles", 2, 1),
                    ("Permissions", "PERMISSIONS", "Permissions", 3, 1),
                    ("User Groups", "GROUPS", "Groups", 4, 1),
                    ("Access Control", "ACCESS", "Access", 5, 1),
                    ("Audit Logs", "AUDIT", "Audit", 6, 1)
                });
            }

            // ===== PERFORMANCE & METRICS SECTION =====

            // Metrics & KPI Tracking Module (4)
            if (modules.TryGetValue("METRICS", out var metricsModuleId))
            {
                var parentId = AddMenuItems(null, metricsModuleId, "METRICS", "ri-line-chart-line", "Metrics", new()
                {
                    ("Metric Definitions", "DEFINITIONS", "Definitions", 1, 1),
                    ("Performance Targets", "TARGETS", "Targets", 2, 1),
                    ("Metric Values", "VALUES", "Values", 3, 1),
                    ("Dashboard Widgets", "WIDGETS", "Widgets", 4, 1),
                    ("KPI Monitoring", "KPI", "KPI", 5, 1)
                });
            }

            // ===== FINANCIAL MANAGEMENT SECTION =====

            // Financial Tracking Module (8)
            if (modules.TryGetValue("FINANCE", out var financeModuleId))
            {
                var parentId = AddMenuItems(null, financeModuleId, "FINANCE", "ri-money-dollar-circle-line", "Finance", new()
                {
                    ("Budgets", "BUDGETS", "Budgets", 1, 1),
                    ("Expenditures", "EXPENDITURES", "Expenditures", 2, 1),
                    ("Cost Centers", "COSTCENTERS", "CostCenters", 3, 1),
                    ("Vendors", "VENDORS", "Vendors", 4, 1),
                    ("Financial Reports", "REPORTS", "Reports", 5, 1)
                });
            }

            // ===== SYSTEM ADMINISTRATION SECTION =====

            // Notifications Module (10)
            if (modules.TryGetValue("NOTIFICATIONS", out var notificationsModuleId))
            {
                var parentId = AddMenuItems(null, notificationsModuleId, "NOTIF", "ri-notification-3-line", "Notifications", new()
                {
                    ("All Notifications", "ALL", "All", 1, 1),
                    ("Notification Templates", "TEMPLATES", "Templates", 2, 1),
                    ("Notification Queue", "QUEUE", "Queue", 3, 1),
                    ("User Preferences", "PREFERENCES", "Preferences", 4, 1),
                    ("Alert Management", "ALERTS", "Alerts", 5, 1)
                });
            }

            // Media Management Module (11)
            if (modules.TryGetValue("MEDIA", out var mediaModuleId))
            {
                var parentId = AddMenuItems(null, mediaModuleId, "MEDIA", "ri-folder-line", "Media", new()
                {
                    ("Media Files", "FILES", "Files", 1, 1),
                    ("Folder Management", "FOLDERS", "Folders", 2, 1),
                    ("Access Control", "ACCESS", "Access", 3, 1)
                });
            }

            // System Settings Module (12)
            if (modules.TryGetValue("SETTINGS", out var settingsModuleId))
            {
                var parentId = AddMenuItems(null, settingsModuleId, "SETTINGS", "ri-settings-3-line", "Settings", new()
                {
                    ("General Settings", "GENERAL", "General", 1, 1),
                    ("System Configuration", "SYSTEM", "System", 2, 1),
                    ("Audit Logs", "AUDIT", "Audit", 3, 1),
                    ("Data Change Logs", "DATALOGS", "DataLogs", 4, 1),
                    ("Login History", "LOGINHISTORY", "LoginHistory", 5, 1)
                });
            }

            context.MenuItems.AddRange(menuItems);
            context.SaveChanges();
        }
    }
}
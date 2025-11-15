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
            int AddMenuItems(int? parentId, int moduleId, string moduleCode, string icon, List<(string Title, string Code, string Controller, int Order, int Level)> items)
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
                                Controller = item.Controller,
                                Action = "Index",  // All actions default to Index
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
                var parentId = AddMenuItems(null, dashboardsModuleId, "DASH", "ri-dashboard-2-line", new()
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
                var parentId = AddMenuItems(null, formsModuleId, "FORMS", "ri-file-list-3-line", new()
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
                var parentId = AddMenuItems(null, reportsModuleId, "REPORTS", "ri-file-chart-line", new()
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
                var parentId = AddMenuItems(null, supportModuleId, "TICKETS", "ri-ticket-2-line", new()
                {
                    ("All Tickets", "ALL", "Ticket", 1, 1),
                    ("My Tickets", "MY", "MyTicket", 2, 1),
                    ("Create Ticket", "CREATE", "Ticket", 3, 1),
                    ("Ticket Categories", "CATEGORIES", "TicketCategory", 4, 1),
                    ("SLA Monitoring", "SLA", "SLA", 5, 1)
                });
            }

            // ===== ASSET MANAGEMENT SECTION =====

            // Hardware Inventory Module (6)
            if (modules.TryGetValue("HARDWARE", out var hardwareModuleId))
            {
                var parentId = AddMenuItems(null, hardwareModuleId, "HARDWARE", "ri-computer-line", new()
                {
                    ("All Hardware Assets", "ALL", "Hardware", 1, 1),
                    ("Asset Assignments", "ASSIGNMENTS", "HardwareAssignment", 2, 1),
                    ("Maintenance Records", "MAINTENANCE", "Maintenance", 3, 1),
                    ("Disposal Tracking", "DISPOSAL", "Disposal", 4, 1),
                    ("Hardware Categories", "CATEGORIES", "HardwareCategory", 5, 1)
                });
            }

            // Software Management Module (5)
            if (modules.TryGetValue("SOFTWARE", out var softwareModuleId))
            {
                var parentId = AddMenuItems(null, softwareModuleId, "SOFTWARE", "ri-apps-line", new()
                {
                    ("Software Catalog", "CATALOG", "Software", 1, 1),
                    ("License Management", "LICENSES", "License", 2, 1),
                    ("Installations", "INSTALLATIONS", "Installation", 3, 1),
                    ("Version Control", "VERSIONS", "SoftwareVersion", 4, 1),
                    ("License Allocations", "ALLOCATIONS", "LicenseAllocation", 5, 1)
                });
            }

            // ===== ORGANIZATIONAL MANAGEMENT SECTION =====

            // Organizational Structure Module (1)
            if (modules.TryGetValue("ORG_STRUCTURE", out var orgStructureModuleId))
            {
                var parentId = AddMenuItems(null, orgStructureModuleId, "ORG", "ri-building-line", new()
                {
                    ("All Tenants", "TENANTS", "Tenant", 1, 1),
                    ("Factories", "FACTORIES", "Factory", 2, 1),
                    ("Subsidiaries", "SUBSIDIARIES", "Subsidiary", 3, 1),
                    ("Regions", "REGIONS", "Region", 4, 1),
                    ("Tenant Groups", "GROUPS", "TenantGroup", 5, 1),
                    ("Departments", "DEPARTMENTS", "Department", 6, 1)
                });
            }

            // Users & Access Management Module (2)
            if (modules.TryGetValue("IAM", out var iamModuleId))
            {
                var parentId = AddMenuItems(null, iamModuleId, "IAM", "ri-user-line", new()
                {
                    ("Users", "USERS", "User", 1, 1),
                    ("Roles", "ROLES", "Role", 2, 1),
                    ("Permissions", "PERMISSIONS", "Permission", 3, 1),
                    ("User Groups", "GROUPS", "UserGroup", 4, 1),
                    ("Access Control", "ACCESS", "AccessControl", 5, 1),
                    ("Audit Logs", "AUDIT", "AuditLog", 6, 1)
                });
            }

            // ===== PERFORMANCE & METRICS SECTION =====

            // Metrics & KPI Tracking Module (4)
            if (modules.TryGetValue("METRICS", out var metricsModuleId))
            {
                var parentId = AddMenuItems(null, metricsModuleId, "METRICS", "ri-line-chart-line", new()
                {
                    ("Metric Definitions", "DEFINITIONS", "MetricDefinition", 1, 1),
                    ("Performance Targets", "TARGETS", "PerformanceTarget", 2, 1),
                    ("Metric Values", "VALUES", "MetricValue", 3, 1),
                    ("Dashboard Widgets", "WIDGETS", "DashboardWidget", 4, 1),
                    ("KPI Monitoring", "KPI", "KPI", 5, 1)
                });
            }

            // ===== FINANCIAL MANAGEMENT SECTION =====

            // Financial Tracking Module (8)
            if (modules.TryGetValue("FINANCE", out var financeModuleId))
            {
                var parentId = AddMenuItems(null, financeModuleId, "FINANCE", "ri-money-dollar-circle-line", new()
                {
                    ("Budgets", "BUDGETS", "Budget", 1, 1),
                    ("Expenditures", "EXPENDITURES", "Expenditure", 2, 1),
                    ("Cost Centers", "COSTCENTERS", "CostCenter", 3, 1),
                    ("Vendors", "VENDORS", "Vendor", 4, 1),
                    ("Financial Reports", "REPORTS", "FinancialReport", 5, 1)
                });
            }

            // ===== SYSTEM ADMINISTRATION SECTION =====

            // Notifications Module (10)
            if (modules.TryGetValue("NOTIFICATIONS", out var notificationsModuleId))
            {
                var parentId = AddMenuItems(null, notificationsModuleId, "NOTIF", "ri-notification-3-line", new()
                {
                    ("All Notifications", "ALL", "Notification", 1, 1),
                    ("Notification Templates", "TEMPLATES", "NotificationTemplate", 2, 1),
                    ("Notification Queue", "QUEUE", "NotificationQueue", 3, 1),
                    ("User Preferences", "PREFERENCES", "NotificationPreference", 4, 1),
                    ("Alert Management", "ALERTS", "Alert", 5, 1)
                });
            }

            // Media Management Module (11)
            if (modules.TryGetValue("MEDIA", out var mediaModuleId))
            {
                var parentId = AddMenuItems(null, mediaModuleId, "MEDIA", "ri-folder-line", new()
                {
                    ("Media Files", "FILES", "MediaFile", 1, 1),
                    ("Folder Management", "FOLDERS", "MediaFolder", 2, 1),
                    ("Access Control", "ACCESS", "MediaAccess", 3, 1)
                });
            }

            // System Settings Module (12)
            if (modules.TryGetValue("SETTINGS", out var settingsModuleId))
            {
                var parentId = AddMenuItems(null, settingsModuleId, "SETTINGS", "ri-settings-3-line", new()
                {
                    ("General Settings", "GENERAL", "GeneralSetting", 1, 1),
                    ("System Configuration", "SYSTEM", "SystemConfiguration", 2, 1),
                    ("Menu Management", "MENU", "MenuManagement", 3, 1), 
                    ("Audit Logs", "AUDIT", "AuditLog", 3, 1),
                    ("Data Change Logs", "DATALOGS", "DataChangeLog", 4, 1),
                    ("Login History", "LOGINHISTORY", "LoginHistory", 5, 1)
                });
            }

            context.MenuItems.AddRange(menuItems);
            context.SaveChanges();
        }
    }
}
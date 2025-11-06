# RBAC & Dynamic Menu System - Implementation Guide

## Overview
This document explains the complete Role-Based Access Control (RBAC) and Dynamic Menu system for the KTDA ICT Reporting System.

---

## 1. RBAC Architecture

### **Multi-Layer Permission Model**

```
┌───────────────────────────────────────────────────────────────┐
│                    PERMISSION HIERARCHY                       │
├───────────────────────────────────────────────────────────────┤
│                                                               │
│  User ──────────┐                                            │
│                  │                                            │
│                  ├──> Role ──> Role Permissions ─┐            │
│                  │                                │           │
│                  └──> User Permissions (Override) ┘           │
│                                    │                          │
│                                    ↓                          │
│                         Effective Permissions                │
│                                    │                          │
│                                    ↓                          │
│                          Menu Items Visibility               │
│                                                               │
└───────────────────────────────────────────────────────────────┘
```

### **Key Components**

| Component | Purpose | Example |
|-----------|---------|---------|
| **Modules** | High-level feature areas | Forms, Reports, Assets, Tenants |
| **Permissions** | Specific actions | Forms.Create, Reports.View, Assets.Edit |
| **Roles** | Job functions | Factory Manager, IT Admin, Regional Manager |
| **RolePermissions** | Permissions assigned to roles | Factory Manager → Forms.Create |
| **UserPermissions** | User-specific overrides | John Doe → Reports.Export (temporary) |
| **MenuItems** | Sidebar navigation items | Dashboard, Monthly Forms, Settings |
| **RoleMenuItems** | Menu visibility by role | IT Admin can see Settings |

---

## 2. How Level + Tenant Logic Works

### **Role Levels Explained**

```
Level 1: HEAD OFFICE (Global Scope)
┌─────────────────────────────────────────────────────┐
│  • Can access ALL tenants by default                │
│  • User.TenantAccess can restrict if needed         │
│  • Examples: CEO, IT Administrator, Audit Officer   │
└─────────────────────────────────────────────────────┘

Level 2: REGIONAL (Regional Scope)
┌─────────────────────────────────────────────────────┐
│  • Can access tenants in their assigned region      │
│  • User.TenantAccess defines which tenants          │
│  • Examples: Regional Manager, Regional IT Officer  │
└─────────────────────────────────────────────────────┘

Level 3: FACTORY/SUBSIDIARY (Single Tenant Scope)
┌─────────────────────────────────────────────────────┐
│  • Can access ONLY their assigned tenant            │
│  • User.TenantAccess must specify ONE tenant        │
│  • Examples: Factory Manager, Data Entry Clerk      │
└─────────────────────────────────────────────────────┘
```

### **Combined Logic Flow**

```sql
-- Pseudocode for checking user access to tenant
FUNCTION CanUserAccessTenant(userId, tenantId):
    user = GetUser(userId)
    userRoles = GetUserRoles(userId)
    
    -- Check if any role has Level 1 (Head Office)
    IF ANY(userRoles.Level == 1):
        RETURN TRUE  -- Head Office can access all
    
    -- Check UserTenantAccess
    tenantAccess = GetUserTenantAccess(userId, tenantId)
    IF tenantAccess EXISTS:
        RETURN TRUE
    
    -- Check if role level matches tenant hierarchy
    FOR role IN userRoles:
        IF role.Level == 2:  -- Regional
            IF tenant.RegionId IN user.AllowedRegions:
                RETURN TRUE
        
        IF role.Level == 3:  -- Factory
            IF tenant.Id == user.PrimaryTenantId:
                RETURN TRUE
    
    RETURN FALSE
```

---

## 3. Permission Structure Examples

### **Sample Modules**

| ModuleId | ModuleName | ModuleCode | Icon |
|----------|------------|------------|------|
| 1 | Dashboards | dashboard | ri-dashboard-line |
| 2 | Forms & Checklists | forms | ri-file-list-3-line |
| 3 | Reports | reports | ri-file-chart-line |
| 4 | Support Tickets | tickets | ri-ticket-2-line |
| 5 | Tenants | tenants | ri-building-line |
| 6 | Users & Roles | users | ri-user-line |
| 7 | Form Templates | templates | ri-file-text-line |
| 8 | Assets | assets | ri-computer-line |
| 9 | Settings | settings | ri-settings-3-line |

### **Sample Permissions**

| PermissionCode | ModuleId | PermissionType | Description |
|----------------|----------|----------------|-------------|
| Forms.View | 2 | View | View all forms |
| Forms.Create | 2 | Create | Create new form submissions |
| Forms.Edit | 2 | Edit | Edit form submissions |
| Forms.Delete | 2 | Delete | Delete form submissions |
| Forms.Approve | 2 | Approve | Approve/reject form submissions |
| Reports.View | 3 | View | View reports |
| Reports.Export | 3 | Export | Export reports to Excel/PDF |
| Tenants.Manage | 5 | Custom | Manage tenant settings |
| Users.Manage | 6 | Custom | Manage user accounts |
| Settings.Modify | 9 | Edit | Modify system settings |

---

## 4. Dynamic Sidebar Implementation

### **Menu Item Structure**

```sql
-- Example Menu Items
INSERT INTO MenuItems (ParentMenuItemId, ModuleId, MenuTitle, MenuCode, Icon, Route, DisplayOrder, Level, RequiredPermissionCode)
VALUES
-- Top Level
(NULL, 1, 'Dashboards', 'menu-dashboards', 'ri-dashboard-2-line', NULL, 1, 1, NULL),
(NULL, 2, 'Forms & Checklists', 'menu-forms', 'ri-file-list-3-line', NULL, 2, 1, 'Forms.View'),
(NULL, 3, 'Reports', 'menu-reports', 'ri-file-chart-line', NULL, 3, 1, 'Reports.View'),

-- Dashboard Submenu
(1, 1, 'Analytics', 'menu-dashboard-analytics', NULL, '/Home/Index', 1, 2, NULL),
(1, 1, 'Regional Overview', 'menu-dashboard-regional', NULL, '/Dashboard/Regional', 2, 2, NULL),

-- Forms Submenu
(2, 2, 'Assigned Forms', 'menu-forms-assigned', NULL, '/Forms/Assigned', 1, 2, 'Forms.View'),
(2, 2, 'Monthly Forms', 'menu-forms-monthly', NULL, '/Forms/Monthly', 2, 2, 'Forms.View'),
(2, 2, 'My Submissions', 'menu-forms-submissions', NULL, '/Forms/MySubmissions', 3, 2, 'Forms.View'),

-- Reports Submenu
(3, 3, 'All Reports', 'menu-reports-all', NULL, '/Reports/Index', 1, 2, 'Reports.View'),
(3, 3, 'Regional Summary', 'menu-reports-regional', NULL, '/Reports/Regional', 2, 2, 'Reports.View'),
(3, 3, 'Custom Reports', 'menu-reports-custom', NULL, '/Reports/Custom', 3, 2, 'Reports.Export');
```

### **Role-Menu Assignments**

```sql
-- Example: IT Administrator (RoleId=1) sees all menus
INSERT INTO RoleMenuItems (RoleId, MenuItemId, IsVisible)
SELECT 1, MenuItemId, 1 FROM MenuItems;

-- Example: Factory Manager (RoleId=3) sees limited menus
INSERT INTO RoleMenuItems (RoleId, MenuItemId, IsVisible)
SELECT 3, MenuItemId, 1 FROM MenuItems
WHERE MenuCode IN ('menu-dashboards', 'menu-dashboard-analytics', 
                   'menu-forms', 'menu-forms-assigned', 'menu-forms-monthly', 'menu-forms-submissions',
                   'menu-reports', 'menu-reports-all');
```

---

## 5. C# Implementation

### **5.1 Permission Service Interface**

```csharp
public interface IPermissionService
{
    Task<bool> UserHasPermissionAsync(int userId, string permissionCode);
    Task<List<string>> GetUserPermissionsAsync(int userId);
    Task<List<MenuItem>> GetUserAccessibleMenuItemsAsync(int userId);
    Task<bool> CanAccessTenantAsync(int userId, int tenantId);
}
```

### **5.2 Permission Service Implementation**

```csharp
public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public PermissionService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<bool> UserHasPermissionAsync(int userId, string permissionCode)
    {
        var cacheKey = $"UserPermissions_{userId}";
        
        var permissions = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30);
            
            return await _context.Set<VwUserEffectivePermission>()
                .Where(p => p.UserId == userId && p.IsGranted)
                .Select(p => p.PermissionCode)
                .ToListAsync();
        });

        return permissions.Contains(permissionCode);
    }

    public async Task<List<MenuItem>> GetUserAccessibleMenuItemsAsync(int userId)
    {
        var cacheKey = $"UserMenuItems_{userId}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30);
            
            return await _context.Set<VwUserAccessibleMenuItem>()
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.DisplayOrder)
                .Select(m => new MenuItem
                {
                    MenuItemId = m.MenuItemId,
                    ParentMenuItemId = m.ParentMenuItemId,
                    MenuTitle = m.MenuTitle,
                    MenuCode = m.MenuCode,
                    Icon = m.Icon,
                    Route = m.Route,
                    Controller = m.Controller,
                    Action = m.Action,
                    DisplayOrder = m.DisplayOrder,
                    Level = m.Level
                })
                .ToListAsync();
        });
    }

    public async Task<bool> CanAccessTenantAsync(int userId, int tenantId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return false;

        // Check if user has Level 1 role (Head Office - access all tenants)
        if (user.UserRoles.Any(ur => ur.Role.Level == 1 && ur.Role.IsActive))
            return true;

        // Check UserTenantAccess
        return await _context.UserTenantAccess
            .AnyAsync(uta => uta.UserId == userId && uta.TenantId == tenantId && uta.CanRead);
    }
}
```

### **5.3 Authorization Attribute**

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permissionCode;

    public RequirePermissionAttribute(string permissionCode)
    {
        _permissionCode = permissionCode;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var permissionService = context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();

        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var hasPermission = permissionService.UserHasPermissionAsync(userId, _permissionCode).Result;

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}
```

### **5.4 Usage in Controllers**

```csharp
[RequirePermission("Forms.View")]
public class FormsController : Controller
{
    [RequirePermission("Forms.Create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("Forms.Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        // Approve logic
        return RedirectToAction("Index");
    }
}
```

### **5.5 Dynamic Sidebar View Component**

```csharp
public class SidebarMenuViewComponent : ViewComponent
{
    private readonly IPermissionService _permissionService;

    public SidebarMenuViewComponent(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return View(new List<MenuItem>());
        }

        var menuItems = await _permissionService.GetUserAccessibleMenuItemsAsync(userId);
        
        // Build hierarchical menu structure
        var menuTree = BuildMenuTree(menuItems);

        return View(menuTree);
    }

    private List<MenuItemViewModel> BuildMenuTree(List<MenuItem> flatItems)
    {
        var lookup = flatItems.ToLookup(m => m.ParentMenuItemId);

        List<MenuItemViewModel> BuildChildren(int? parentId)
        {
            return lookup[parentId]
                .OrderBy(m => m.DisplayOrder)
                .Select(m => new MenuItemViewModel
                {
                    MenuItem = m,
                    Children = BuildChildren(m.MenuItemId)
                })
                .ToList();
        }

        return BuildChildren(null);
    }
}
```

### **5.6 Update _Sidebar.cshtml to Use View Component**

```cshtml
@await Component.InvokeAsync("SidebarMenu")
```

---

## 6. Implementation Steps

### **Phase 1: Database Setup**
1. ✅ Run `Enhanced_RBAC_Schema.sql`
2. ✅ Seed Modules data
3. ✅ Seed Permissions data
4. ✅ Seed MenuItems data
5. ✅ Create sample Roles with RolePermissions
6. ✅ Assign RoleMenuItems

### **Phase 2: C# Models & DbContext**
1. Create Entity models for new tables
2. Add DbSets to ApplicationDbContext
3. Configure relationships in OnModelCreating
4. Run migrations

### **Phase 3: Services**
1. Implement IPermissionService
2. Create PermissionService
3. Register in Program.cs
4. Add caching layer

### **Phase 4: Authorization**
1. Create RequirePermissionAttribute
2. Create TenantAccessAttribute
3. Update controllers with attributes

### **Phase 5: UI**
1. Create SidebarMenuViewComponent
2. Update _Sidebar.cshtml
3. Test with different user roles
4. Add permission checks in views (@if permission)

---

## 7. Seed Data Script

See separate file: `SeedData_RBAC.sql`

---

## Summary

This enhanced RBAC system provides:

✅ **Granular Permissions** - Module-based, action-level control  
✅ **Role Hierarchy** - Head Office, Regional, Factory levels  
✅ **Multi-Tenancy Support** - UserTenantAccess controls scope  
✅ **Dynamic Menus** - Role-based sidebar visibility  
✅ **Permission Overrides** - User-specific exceptions  
✅ **Audit Trail** - All permission changes logged  
✅ **Cached Performance** - Fast permission checks  
✅ **Flexible** - Easy to extend with new modules/permissions  


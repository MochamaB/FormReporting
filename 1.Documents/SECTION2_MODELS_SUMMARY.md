# Section 2: Identity & Access Management - Models Implementation Summary

## ✅ Completed Tasks

### **1. Entity Models Created** (11 models)

| Model | File Path | Description |
|-------|-----------|-------------|
| Role | `Models/Entities/Identity/Role.cs` | Roles with hierarchical levels (1=HeadOffice, 2=Regional, 3=Factory) |
| User | `Models/Entities/Identity/User.cs` | ASP.NET Identity compatible user accounts |
| UserRole | `Models/Entities/Identity/UserRole.cs` | Many-to-many user-role assignments |
| Module | `Models/Entities/Identity/Module.cs` | High-level application feature areas |
| Permission | `Models/Entities/Identity/Permission.cs` | Granular functional permissions |
| RolePermission | `Models/Entities/Identity/RolePermission.cs` | Role-permission assignments |
| MenuItem | `Models/Entities/Identity/MenuItem.cs` | Dynamic hierarchical sidebar navigation |
| RoleMenuItem | `Models/Entities/Identity/RoleMenuItem.cs` | Role-based menu visibility control |
| UserTenantAccess | `Models/Entities/Identity/UserTenantAccess.cs` | Explicit tenant access exceptions |
| UserGroup | `Models/Entities/Identity/UserGroup.cs` | User groups (training, projects, committees) |
| UserGroupMember | `Models/Entities/Identity/UserGroupMember.cs` | User group memberships |

### **2. EF Core Configurations Created** (11 configurations)

| Configuration | File Path | Features |
|---------------|-----------|----------|
| RoleConfiguration | `Data/Configurations/Identity/RoleConfiguration.cs` | Unique constraints, cascade delete for permissions |
| UserConfiguration | `Data/Configurations/Identity/UserConfiguration.cs` | ASP.NET Identity fields, indexes, unique constraints |
| UserRoleConfiguration | `Data/Configurations/Identity/UserRoleConfiguration.cs` | Composite unique constraint (UserId, RoleId) |
| ModuleConfiguration | `Data/Configurations/Identity/ModuleConfiguration.cs` | Unique constraints, display order index |
| PermissionConfiguration | `Data/Configurations/Identity/PermissionConfiguration.cs` | Check constraint for PermissionType, multiple indexes |
| RolePermissionConfiguration | `Data/Configurations/Identity/RolePermissionConfiguration.cs` | Composite unique constraint, IsGranted support |
| MenuItemConfiguration | `Data/Configurations/Identity/MenuItemConfiguration.cs` | Self-referencing FK, hierarchical structure |
| RoleMenuItemConfiguration | `Data/Configurations/Identity/RoleMenuItemConfiguration.cs` | Composite unique constraint, cascade delete |
| UserTenantAccessConfiguration | `Data/Configurations/Identity/UserTenantAccessConfiguration.cs` | Filtered index on ExpiryDate, unique constraint |
| UserGroupConfiguration | `Data/Configurations/Identity/UserGroupConfiguration.cs` | Unique GroupCode, optional TenantId |
| UserGroupMemberConfiguration | `Data/Configurations/Identity/UserGroupMemberConfiguration.cs` | Composite unique constraint, cascade delete |

### **3. ApplicationDbContext Updated**

**Changes:**
- ✅ Added 11 DbSet properties for Section 2 entities
- ✅ Applied all 11 configurations in OnModelCreating
- ✅ Updated using statements to include Identity namespace

### **4. Section 1 Entities Updated**

**Fixed namespace references:**
- ✅ Region.cs - Added `using FormReporting.Models.Entities.Identity;`
- ✅ Tenant.cs - Added `using FormReporting.Models.Entities.Identity;`
- ✅ TenantGroup.cs - Added `using FormReporting.Models.Entities.Identity;`
- ✅ TenantGroupMember.cs - Added `using FormReporting.Models.Entities.Identity;`
- ✅ Department.cs - Added `using FormReporting.Models.Entities.Identity;`

---

## 📋 Database Schema Features Implemented

### **Constraints**

| Entity | Constraint Type | Description |
|--------|----------------|-------------|
| Role | Unique | RoleName must be unique |
| Role | Unique | RoleCode must be unique |
| User | Unique | UserName must be unique |
| User | Unique | EmployeeNumber must be unique (if not null) |
| UserRole | Unique | Composite (UserId, RoleId) - user can't have same role twice |
| Module | Unique | ModuleName must be unique |
| Module | Unique | ModuleCode must be unique |
| Permission | Unique | PermissionCode must be unique |
| Permission | Unique | Composite (ModuleId, PermissionCode) |
| Permission | Check | PermissionType IN ('View', 'Create', 'Edit', 'Delete', 'Approve', 'Export', 'Manage', 'Custom') |
| RolePermission | Unique | Composite (RoleId, PermissionId) |
| MenuItem | Unique | MenuCode must be unique |
| RoleMenuItem | Unique | Composite (RoleId, MenuItemId) |
| UserTenantAccess | Unique | Composite (UserId, TenantId) |
| UserGroup | Unique | GroupCode must be unique |
| UserGroupMember | Unique | Composite (UserGroupId, UserId) |

### **Indexes**

| Entity | Index | Columns | Special Features |
|--------|-------|---------|------------------|
| User | IX_Users_Email | NormalizedEmail | - |
| User | IX_Users_Username | NormalizedUserName | - |
| User | IX_User_Department | DepartmentId | - |
| UserRole | IX_UserRoles_User | UserId | - |
| UserRole | IX_UserRoles_Role | RoleId | - |
| Module | IX_Modules_Active | IsActive, DisplayOrder | Composite |
| Module | IX_Modules_Code | ModuleCode | - |
| Permission | IX_Permission_Module | ModuleId, IsActive | Composite |
| Permission | IX_Permission_Code | PermissionCode | - |
| Permission | IX_Permission_Type | PermissionType, IsActive | Composite |
| Permission | IX_Permission_Active | IsActive | - |
| RolePermission | IX_RolePermission_Role | RoleId, IsGranted | Composite |
| RolePermission | IX_RolePermission_Permission | PermissionId | - |
| MenuItem | IX_MenuItem_Parent | ParentMenuItemId, DisplayOrder | Composite |
| MenuItem | IX_MenuItem_Module | ModuleId | - |
| MenuItem | IX_MenuItem_Active | IsActive, IsVisible | Composite |
| MenuItem | IX_MenuItem_Order | DisplayOrder | - |
| RoleMenuItem | IX_RoleMenuItem_Role | RoleId, IsVisible | Composite |
| RoleMenuItem | IX_RoleMenuItem_MenuItem | MenuItemId | - |
| UserTenantAccess | IX_UserTenantAccess_User | UserId, IsActive | Composite |
| UserTenantAccess | IX_UserTenantAccess_Tenant | TenantId, IsActive | Composite |
| UserTenantAccess | IX_UserTenantAccess_Expiry | ExpiryDate | Filtered: WHERE ExpiryDate IS NOT NULL AND IsActive = 1 |
| UserGroup | IX_UserGroup_Tenant | TenantId | - |
| UserGroup | IX_UserGroup_Code | GroupCode | - |
| UserGroup | IX_UserGroup_Active | IsActive | - |
| UserGroupMember | IX_UserGroupMember_Group | UserGroupId | - |
| UserGroupMember | IX_UserGroupMember_User | UserId | - |

### **Relationships**

| Parent | Child | Relationship Type | Delete Behavior |
|--------|-------|-------------------|-----------------|
| Role | UserRole | One-to-Many | Restrict |
| Role | RolePermission | One-to-Many | Cascade |
| Role | RoleMenuItem | One-to-Many | Cascade |
| User | UserRole | One-to-Many | Restrict |
| User | UserTenantAccess | One-to-Many | Restrict |
| User | UserGroupMember | One-to-Many | Cascade |
| Department | User | One-to-Many | SetNull |
| Module | Permission | One-to-Many | Restrict |
| Module | MenuItem | One-to-Many | SetNull |
| Permission | RolePermission | One-to-Many | Cascade |
| MenuItem | MenuItem (self) | One-to-Many | Restrict |
| MenuItem | RoleMenuItem | One-to-Many | Cascade |
| Tenant | UserTenantAccess | One-to-Many | Restrict |
| Tenant | UserGroup | One-to-Many | SetNull |
| UserGroup | UserGroupMember | One-to-Many | Cascade |

---

## 🎯 RBAC Design Features

### **1. Role Hierarchy (Level-Based Tenant Access)**

```csharp
public enum RoleLevel
{
    HeadOffice = 1,    // Access ALL tenants automatically
    Regional = 2,      // Access region tenants automatically
    Factory = 3        // Access ONLY own tenant automatically
}
```

**Automatic Access Logic:**
- **Level 1 Users**: See data from all 80+ tenants (CEO, IT Admin, Compliance)
- **Level 2 Users**: See data from tenants in their assigned region (Regional Managers)
- **Level 3 Users**: See data from ONLY their tenant (Factory Managers, Clerks)

**UserTenantAccess for Exceptions:**
- Temporary access (audits, projects) with `ExpiryDate`
- Cross-region assignments with `Reason` tracking
- Granted by another user (`GrantedBy`) for audit trail

### **2. Permission Model**

**Permission Structure:**
```
Module (Forms) → Permission (Forms.Submit, Forms.Approve, Forms.Export)
    ↓
RolePermission (Factory Manager has Forms.Submit)
    ↓
User Claims (loaded at login, cached)
```

**Permission Types:**
- `View` - Read access
- `Create` - Create new records
- `Edit` - Modify existing records
- `Delete` - Delete records
- `Approve` - Approve/reject workflow
- `Export` - Export data
- `Manage` - Administrative access
- `Custom` - Special actions

**Permission Codes:** `{Module}.{Action}` format
- `Forms.Submit`
- `Forms.Approve`
- `Reports.Export`
- `Templates.Design`
- `Users.Manage`

### **3. Dynamic Menu System**

**Menu Visibility Flow:**
```
User → UserRoles → Role → RoleMenuItems → MenuItems
```

**MenuItem Features:**
- Hierarchical structure (ParentMenuItemId)
- Supports 3 levels deep (Level 1/2/3)
- Links to routes (Controller/Action/Area)
- Links to Modules
- Display order control

**RoleMenuItem Controls:**
- Which roles can see which menus
- Simple visibility flag (IsVisible)
- Cascade delete when role or menu deleted

---

## 🚀 Next Steps

### **1. Create Migration**

```powershell
# Add migration for Sections 1 & 2
dotnet ef migrations add InitialCreate_Sections1And2 --output-dir Data/Migrations

# Review the generated migration file
# Verify all constraints, indexes, and relationships
```

### **2. Update Database**

```powershell
# Apply the migration
dotnet ef database update

# Verify tables were created
```

### **3. Verify Schema**

**Expected Tables (16 total):**

**Section 1 (5 tables):**
- ✅ Regions
- ✅ Tenants
- ✅ TenantGroups
- ✅ TenantGroupMembers
- ✅ Departments

**Section 2 (11 tables):**
- ✅ Roles
- ✅ Users
- ✅ UserRoles
- ✅ Modules
- ✅ Permissions
- ✅ RolePermissions
- ✅ MenuItems
- ✅ RoleMenuItems
- ✅ UserTenantAccess
- ✅ UserGroups
- ✅ UserGroupMembers

**Verification Queries:**
```sql
-- Check all tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_NAME NOT LIKE '__EF%'
ORDER BY TABLE_NAME;

-- Check constraints
SELECT TABLE_NAME, CONSTRAINT_NAME, CONSTRAINT_TYPE
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE TABLE_NAME IN ('Roles', 'Users', 'Permissions', 'MenuItems')
ORDER BY TABLE_NAME, CONSTRAINT_TYPE;

-- Check indexes
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('Users', 'Permissions', 'RolePermissions', 'MenuItems')
ORDER BY t.name, i.name;
```

---

## 📁 File Structure Created

```
FormReporting/
├── Models/
│   └── Entities/
│       ├── Organizational/
│       │   ├── Region.cs ✅ (updated namespaces)
│       │   ├── Tenant.cs ✅ (updated namespaces)
│       │   ├── TenantGroup.cs ✅ (updated namespaces)
│       │   ├── TenantGroupMember.cs ✅ (updated namespaces)
│       │   └── Department.cs ✅ (updated namespaces)
│       │
│       └── Identity/
│           ├── Role.cs ✅
│           ├── User.cs ✅ (replaced placeholder)
│           ├── UserRole.cs ✅
│           ├── Module.cs ✅
│           ├── Permission.cs ✅
│           ├── RolePermission.cs ✅
│           ├── MenuItem.cs ✅
│           ├── RoleMenuItem.cs ✅
│           ├── UserTenantAccess.cs ✅
│           ├── UserGroup.cs ✅
│           └── UserGroupMember.cs ✅
│
└── Data/
    ├── ApplicationDbContext.cs ✅ (updated with Section 2)
    │
    └── Configurations/
        ├── Organizational/
        │   ├── RegionConfiguration.cs ✅
        │   ├── TenantConfiguration.cs ✅
        │   ├── TenantGroupConfiguration.cs ✅
        │   ├── TenantGroupMemberConfiguration.cs ✅
        │   └── DepartmentConfiguration.cs ✅
        │
        └── Identity/
            ├── RoleConfiguration.cs ✅
            ├── UserConfiguration.cs ✅
            ├── UserRoleConfiguration.cs ✅
            ├── ModuleConfiguration.cs ✅
            ├── PermissionConfiguration.cs ✅
            ├── RolePermissionConfiguration.cs ✅
            ├── MenuItemConfiguration.cs ✅
            ├── RoleMenuItemConfiguration.cs ✅
            ├── UserTenantAccessConfiguration.cs ✅
            ├── UserGroupConfiguration.cs ✅
            └── UserGroupMemberConfiguration.cs ✅
```

---

## 🎯 What's Next?

1. **Run the migration commands** to create both Section 1 & 2 tables
2. **Create seed data** for initial Roles, Modules, Permissions, MenuItems
3. **Test RBAC logic** with sample users and role assignments
4. **Proceed to Section 3** (Metrics & KPI Tracking - 3 tables) when ready
5. **Continue scaffolding** remaining 10 sections

---

## 💡 Key Design Decisions

### **✅ Simplified RBAC (From Enhanced_RBAC_Schema.sql)**

**What was REMOVED from original design:**
- ❌ UserPermissions table (no user-level permission overrides)
- ❌ RequiredPermissionCode in MenuItems (using RoleMenuItems instead)

**What was KEPT:**
- ✅ Role-based permissions only (simpler to manage)
- ✅ Level-based automatic tenant access (1=All, 2=Region, 3=Own)
- ✅ UserTenantAccess for exceptions only (with ExpiryDate, Reason, GrantedBy)
- ✅ RoleMenuItems for role-based menu visibility
- ✅ IsGranted flag in RolePermissions (allow explicit deny)

### **Benefits of This Approach:**
1. **Simpler to manage** - No per-user permission configurations
2. **Easier to audit** - All permissions through roles
3. **Clearer access rules** - Level determines tenant access automatically
4. **Exception tracking** - UserTenantAccess has reason and expiry
5. **Consistent RBAC** - Roles control everything (permissions + menus)

---

**Status:** ✅ Section 2 Complete - Ready for Migration  
**Last Updated:** November 2025  
**Next Section:** Section 3 - Metrics & KPI Tracking (3 tables)  
**Total Progress:** 16/78 tables (20.5%)

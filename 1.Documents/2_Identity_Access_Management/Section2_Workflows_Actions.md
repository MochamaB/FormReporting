# SECTION 2: IDENTITY & ACCESS MANAGEMENT - Workflows & Actions

**Module:** User Authentication, Authorization & RBAC
**Tables:** 11 (Roles, Users, UserRoles, Modules, Permissions, RolePermissions, MenuItems, RoleMenuItems, UserTenantAccess, UserGroups, UserGroupMembers)

---

## 1. ROLES

### **CRUD Operations:**
- **CREATE** Role
- **READ** Role (Single)
- **READ** All Roles (List with filter by Level)
- **UPDATE** Role Details
- **DELETE** Role (Check for associated users first)

### **Business Rules:**
- RoleCode must be unique
- RoleName must be unique
- Level must be 1 (HeadOffice), 2 (Regional), or 3 (Factory)
- Cannot delete Role if users exist (FK from UserRoles)
- Cannot delete system roles (SYSADMIN, HO_ICT_MGR, etc.)

### **Workflows:**

#### **WF-2.1: Create Role**
```
Trigger: Admin creates new Role
Steps:
  1. Validate RoleCode uniqueness
  2. Validate RoleName uniqueness
  3. Set Level based on intended scope:
     - Level 1: Access all tenants (HeadOffice staff)
     - Level 2: Access region tenants (Regional managers)
     - Level 3: Access single tenant (Factory staff)
  4. Save Role with IsActive = 1
  5. Redirect to "Assign Permissions" screen
  6. Log action in PermissionAuditLog
```

#### **WF-2.2: Role Hierarchy Validation**
```
Business Logic:
  Level 1 (HeadOffice) Automatic Access:
    → Access ALL 80 tenants
    → Typically: SYSADMIN, HO_ICT_MGR, AUDITOR

  Level 2 (Regional) Automatic Access:
    → User assigned to Region X
    → Access all tenants WHERE RegionId = X
    → Typically: REGIONAL_MGR, REGIONAL_ICT

  Level 3 (Factory) Automatic Access:
    → User belongs to Tenant Y (via DepartmentId)
    → Access ONLY TenantId = Y
    → Typically: FACTORY_ICT, FACTORY_MGR, VIEWER
```

#### **WF-2.3: Delete Role with Users Check**
```
Trigger: Admin attempts to delete Role
Steps:
  1. Check if Role is system role (IsSystemRole flag or hardcoded list)
     - If yes → Error: "Cannot delete system role"
  2. Query: SELECT COUNT(*) FROM UserRoles WHERE RoleId = X
  3. If count > 0:
     - Error: "Cannot delete. X users have this role"
     - Suggest: "Reassign users first or deactivate role"
  4. If count = 0:
     - Confirm: "Delete {RoleName}? This will also remove all associated permissions and menu items."
     - CASCADE DELETE RolePermissions
     - CASCADE DELETE RoleMenuItems
     - DELETE Role
  5. Log in PermissionAuditLog
```

---

## 2. USERS

### **CRUD Operations:**
- **CREATE** User (with ASP.NET Identity)
- **READ** User (Single, with roles and permissions)
- **READ** All Users (List with filters by Tenant, Department, Role)
- **UPDATE** User Profile
- **UPDATE** Password (reset/change)
- **DELETE** User (Soft delete - sets IsActive = 0)
- **LOCK/UNLOCK** User Account

### **Business Rules:**
- UserName must be unique
- Email must be unique and valid format
- EmployeeNumber must be unique (if provided)
- Must have at least one Role assigned
- DepartmentId required (determines primary Tenant)
- Password must meet complexity requirements (ASP.NET Identity)
- Account locks after 5 failed login attempts

### **Workflows:**

#### **WF-2.4: User Registration (Admin Creates User)**
```
Trigger: Admin selects "Add New User"
Steps:
  1. Step 1: Basic Details
     - FirstName, LastName, Email, UserName
     - EmployeeNumber (optional)
     - Department (dropdown: filters by selected Tenant)

  2. Step 2: Role Assignment
     - Show available Roles
     - Multi-select (user can have multiple roles)
     - Validate: At least one role required

  3. Step 3: Tenant Access
     - Auto-determined by Role.Level
     - Option to add exceptions via UserTenantAccess
     - Example: Factory user needs temporary access to subsidiary for project

  4. Step 4: Generate Credentials
     - Auto-generate temporary password
     - Option: Send email with credentials
     - User must change password on first login

  5. Post-Creation:
     - Hash password (ASP.NET Identity)
     - INSERT INTO Users
     - INSERT INTO UserRoles for each assigned role
     - Send welcome email with credentials
     - Log in AuditLogs
```

#### **WF-2.5: User Self-Registration (If Enabled)**
```
Trigger: New user visits registration page
Steps:
  1. User fills form:
     - Email, UserName, FirstName, LastName
     - Password + Confirm Password
     - EmployeeNumber

  2. Email Verification:
     - Send verification link to email
     - User clicks link to activate account

  3. Admin Approval:
     - Notification to tenant admin
     - Admin assigns Department + Roles
     - Admin approves account

  4. Activation:
     - Set IsActive = 1
     - Send activation email to user
     - User can now login
```

#### **WF-2.6: User Login**
```
Trigger: User submits login form
Steps:
  1. Validate credentials (UserName/Email + Password)
  2. Check IsActive = 1
  3. Check IsLocked = 0
  4. If credentials invalid:
     - Increment AccessFailedCount
     - If AccessFailedCount >= 5:
       * Set IsLocked = 1
       * Set LockoutEnd = NOW() + 30 minutes
       * Log in AuditLogs (Action: AccountLocked)
     - Show error: "Invalid credentials"

  5. If credentials valid:
     - Reset AccessFailedCount = 0
     - Update LastLoginDate = NOW()
     - Create authentication cookie
     - Build user claims (see WF-2.7)
     - Log in AuditLogs (Action: Login)
     - Redirect to Dashboard
```

#### **WF-2.7: Build User Claims (On Login)**
```
Trigger: Successful authentication
Claims to Include:
  1. Identity Claims:
     - UserId
     - UserName
     - Email
     - FullName (FirstName + LastName)

  2. Role Claims:
     - Get all roles from UserRoles
     - Add claim for each: ClaimType = "Role", Value = RoleName

  3. Permission Claims:
     - Get all permissions via: UserRoles → RolePermissions → Permissions
     - Add claim for each: ClaimType = "Permission", Value = PermissionCode
     - Example: "Forms.Submit", "Forms.Approve", "Reports.Export"

  4. Tenant Access Claims:
     - Based on Role.Level:
       * Level 1 → Claim: "TenantAccess", Value = "*" (all tenants)
       * Level 2 → Get user's region, claim: "TenantAccess", Value = "Region:3"
       * Level 3 → Get user's tenant, claim: "TenantAccess", Value = "Tenant:50"
     - Plus UserTenantAccess exceptions

  5. Department/Tenant Context:
     - PrimaryTenantId (from User.DepartmentId → Department.TenantId)
     - DepartmentId
     - DepartmentName

Store in: HttpContext.User.Claims
Cache Duration: 30 minutes (refresh on permission/role change)
```

#### **WF-2.8: Password Reset**
```
Trigger: User clicks "Forgot Password"
Steps:
  1. User enters Email
  2. System validates email exists
  3. Generate password reset token (ASP.NET Identity)
  4. Send email with reset link (token expires in 24 hours)
  5. User clicks link
  6. User enters new password + confirm
  7. Validate password complexity
  8. Hash and save new password
  9. Invalidate reset token
  10. Send confirmation email
  11. Log in AuditLogs
```

#### **WF-2.9: User Deactivation**
```
Trigger: Admin deactivates user
Steps:
  1. Check if user has pending tasks:
     - Pending form approvals
     - Open tickets assigned
     - Active projects
  2. If pending tasks:
     - Prompt: "User has X pending tasks. Reassign first?"
  3. Set IsActive = 0
  4. Invalidate all sessions (logout user)
  5. Remove from UserGroups (optional)
  6. Keep in UserTenantAccess (audit trail)
  7. Log in AuditLogs
  8. Notify user via email
```

---

## 3. USER ROLES

### **CRUD Operations:**
- **CREATE** User-Role Assignment
- **READ** Roles for User
- **READ** Users for Role
- **DELETE** User-Role Assignment

### **Business Rules:**
- Unique constraint: (UserId, RoleId) - same role cannot be assigned twice
- User must have at least one active role
- AssignedBy tracks who granted the role

### **Workflows:**

#### **WF-2.10: Assign Role to User**
```
Trigger: Admin selects "Manage Roles" for user
Steps:
  1. Show current roles (with "Remove" button)
  2. Show available roles dropdown
  3. Admin selects role and clicks "Add"
  4. Validate role not already assigned
  5. INSERT INTO UserRoles (UserId, RoleId, AssignedBy, AssignedDate)
  6. Invalidate user's claims cache (force re-login or refresh)
  7. Log in PermissionAuditLog (Action: RoleGranted)
  8. Show success: "{RoleName} assigned to {UserName}"
```

#### **WF-2.11: Remove Role from User**
```
Trigger: Admin clicks "Remove" next to role
Steps:
  1. Check if last remaining role:
     - If yes → Error: "User must have at least one role"
  2. Confirm: "Remove {RoleName} from {UserName}?"
  3. DELETE FROM UserRoles WHERE UserId = X AND RoleId = Y
  4. Invalidate user's claims cache
  5. Log in PermissionAuditLog (Action: RoleRevoked)
  6. Show success: "{RoleName} removed"
```

#### **WF-2.12: Bulk Role Assignment**
```
Trigger: Admin selects "Bulk Assign Role"
Steps:
  1. Select Role from dropdown
  2. Select multiple users (checkbox list with filters)
  3. Preview: "Assign {RoleName} to X users?"
  4. For each user:
     - Check if already has role (skip if exists)
     - INSERT INTO UserRoles
  5. Invalidate all affected users' claims
  6. Show summary: "{RoleName} assigned to X users"
  7. Log in PermissionAuditLog (bulk action)
```

---

## 4. MODULES

### **CRUD Operations:**
- **CREATE** Module
- **READ** Module (Single)
- **READ** All Modules (List, ordered by DisplayOrder)
- **UPDATE** Module Details
- **DELETE** Module (Check for associated permissions/menu items first)

### **Business Rules:**
- ModuleCode must be unique
- ModuleName must be unique
- DisplayOrder determines order in UI
- Cannot delete Module if Permissions or MenuItems exist

### **Workflows:**

#### **WF-2.13: Create Module**
```
Trigger: Admin creates new application module
Steps:
  1. Enter ModuleName (e.g., "Forms Management", "Asset Tracking")
  2. Enter ModuleCode (e.g., "FORMS", "ASSETS")
  3. Select Icon (icon picker from available icons)
  4. Set DisplayOrder (for sidebar ordering)
  5. Save Module with IsActive = 1
  6. Redirect to "Add Permissions" screen
```

#### **WF-2.14: Module Reordering**
```
Trigger: Admin drags modules to reorder
Steps:
  1. Display modules in current order (drag handles)
  2. Admin drags module to new position
  3. Update DisplayOrder for all affected modules
  4. Save changes
  5. Sidebar updates on next page load
```

**Typical Modules:**
```
1. Dashboard (Icon: ri-dashboard-line, Order: 1)
2. Forms (Icon: ri-file-list-line, Order: 2)
3. Reports (Icon: ri-bar-chart-line, Order: 3)
4. Assets (Icon: ri-computer-line, Order: 4)
5. Tickets (Icon: ri-customer-service-line, Order: 5)
6. Users (Icon: ri-user-line, Order: 6)
7. Tenants (Icon: ri-building-line, Order: 7)
8. Settings (Icon: ri-settings-line, Order: 10)
```

---

## 5. PERMISSIONS

### **CRUD Operations:**
- **CREATE** Permission
- **READ** Permission (Single)
- **READ** Permissions by Module
- **READ** All Permissions (List grouped by Module)
- **UPDATE** Permission Details
- **DELETE** Permission (Check for RolePermissions first)

### **Business Rules:**
- PermissionCode must be unique globally
- Convention: `{Module}.{Action}` (e.g., "Forms.Submit", "Reports.Export")
- PermissionType must be: View, Create, Edit, Delete, Approve, Export, Manage, Custom
- Cannot delete Permission if assigned to roles

### **Workflows:**

#### **WF-2.15: Create Permission**
```
Trigger: Admin creates new permission
Steps:
  1. Select Module (dropdown)
  2. Enter PermissionName (e.g., "Submit Forms", "Approve Budgets")
  3. Enter PermissionCode (e.g., "Forms.Submit", "Budgets.Approve")
     - Validate format: ModuleName.ActionVerb
  4. Select PermissionType (dropdown):
     - View, Create, Edit, Delete, Approve, Export, Manage, Custom
  5. Enter Description (optional)
  6. Save Permission with IsActive = 1
  7. Show success: "Permission created. Assign to roles now?"
```

#### **WF-2.16: Permission Naming Convention**
```
Standard Format: {Module}.{Action}

Examples:
  Templates.Design    → Design form templates (use form builder)
  Templates.Publish   → Publish templates to users
  Forms.Submit        → Submit/fill form instances
  Forms.View          → View own form submissions
  Forms.ViewAll       → View all submissions (admin)
  Forms.Approve       → Approve/reject submissions
  Forms.Delete        → Delete form submissions
  Reports.View        → View reports
  Reports.Export      → Export reports to Excel/PDF
  Assets.Manage       → Full CRUD on assets
  Tenants.Manage      → Full CRUD on tenants
  Users.Manage        → Full CRUD on users
  Settings.Modify     → Change system settings
```

**Typical Permission Set for Each Module:**
```
Forms Module:
  - Forms.ViewOwn      (View own submissions)
  - Forms.ViewAll      (View all submissions in scope)
  - Forms.Submit       (Fill and submit forms)
  - Forms.Edit         (Edit draft submissions)
  - Forms.Delete       (Delete submissions)
  - Forms.Approve      (Approve submissions)
  - Templates.Design   (Use form builder)
  - Templates.Publish  (Make templates live)
  - Templates.Assign   (Assign templates to users)
```

---

## 6. ROLE PERMISSIONS

### **CRUD Operations:**
- **CREATE** Role-Permission Assignment
- **READ** Permissions for Role
- **READ** Roles for Permission
- **DELETE** Role-Permission Assignment

### **Business Rules:**
- Unique constraint: (RoleId, PermissionId)
- IsGranted flag: 1 = Allow, 0 = Explicitly Deny
- AssignedBy tracks who granted permission

### **Workflows:**

#### **WF-2.17: Assign Permissions to Role**
```
Trigger: Admin configures role permissions
Steps:
  1. Select Role
  2. Display permissions grouped by Module:

     Forms Module:
       ☐ Forms.ViewOwn
       ☐ Forms.Submit
       ☐ Forms.Approve

     Reports Module:
       ☐ Reports.View
       ☐ Reports.Export

  3. Admin checks desired permissions
  4. Click "Save Permissions"
  5. For each checked permission:
     - If not exists → INSERT INTO RolePermissions (RoleId, PermissionId, IsGranted=1)
     - If exists → UPDATE IsGranted = 1
  6. For each unchecked permission:
     - If exists → DELETE FROM RolePermissions
  7. Invalidate claims cache for all users with this role
  8. Log in PermissionAuditLog
  9. Show success: "Permissions updated for {RoleName}"
```

#### **WF-2.18: Permission Matrix View**
```
Trigger: Admin views "Permission Matrix" report
Display: Grid showing Roles × Permissions

                    | SYSADMIN | HO_ICT_MGR | REGIONAL_MGR | FACTORY_ICT |
--------------------|----------|------------|--------------|-------------|
Forms.Submit        |    ✓     |     ✓      |      ✓       |      ✓      |
Forms.Approve       |    ✓     |     ✓      |      ✓       |      ✗      |
Forms.Delete        |    ✓     |     ✓      |      ✗       |      ✗      |
Templates.Design    |    ✓     |     ✓      |      ✗       |      ✗      |
Reports.Export      |    ✓     |     ✓      |      ✓       |      ✗      |
Users.Manage        |    ✓     |     ✓      |      ✗       |      ✗      |

Actions:
  - Click cell to toggle permission
  - Export matrix to Excel
```

#### **WF-2.19: Role Templates (Permission Presets)**
```
Trigger: Admin creates new role from template
Steps:
  1. Select "Create Role from Template"
  2. Choose template:
     - "Factory ICT Staff" → Pre-checked: Forms.Submit, Forms.View, Reports.View
     - "Regional Manager" → Pre-checked: Forms.Approve, Forms.ViewAll, Reports.Export
     - "Auditor" → Pre-checked: Forms.ViewAll (read-only), Reports.Export
  3. Customize permissions if needed
  4. Save role with selected permissions
  5. All RolePermissions INSERTed in one transaction
```

---

## 7. MENU ITEMS

### **CRUD Operations:**
- **CREATE** Menu Item
- **READ** Menu Item (Single)
- **READ** Menu Hierarchy (Tree structure)
- **UPDATE** Menu Item Details
- **DELETE** Menu Item (Cascade deletes child items)

### **Business Rules:**
- MenuCode must be unique
- ParentMenuItemId NULL = top-level menu
- Level indicates nesting depth (1=top, 2=submenu, 3=nested submenu)
- Route can be NULL for parent menus (no direct link)
- RequiresAuth = 1 means must be logged in to see

### **Workflows:**

#### **WF-2.20: Create Menu Item**
```
Trigger: Admin creates new menu item
Steps:
  1. Enter MenuTitle (display name, e.g., "My Forms")
  2. Enter MenuCode (unique code, e.g., "FORMS_ASSIGNED")
  3. Select ParentMenuItem (dropdown, NULL = top-level)
  4. Select Module (links to Module table)
  5. Enter Route (e.g., "/Forms/Assigned", "/Reports/Dashboard")
  6. Select Icon (icon picker)
  7. Set DisplayOrder (ordering within parent)
  8. Set Level (auto-calculated based on parent)
  9. RequiresAuth checkbox (default: checked)
  10. IsVisible checkbox (default: checked)
  11. Save MenuItem
  12. Redirect to "Assign to Roles" screen
```

#### **WF-2.21: Menu Hierarchy Builder**
```
Visual Tree Editor:
  Dashboard (Level 1, Order 1)

  Forms (Level 1, Order 2)
    ├─ Assigned Forms (Level 2, Order 1) → /Forms/Assigned
    ├─ My Submissions (Level 2, Order 2) → /Forms/MySubmissions
    ├─ Pending Approvals (Level 2, Order 3) → /Forms/PendingApprovals
    └─ Form Templates (Level 2, Order 4) → /Templates/Index

  Reports (Level 1, Order 3)
    ├─ Dashboard (Level 2, Order 1) → /Reports/Dashboard
    ├─ Monthly Reports (Level 2, Order 2) → /Reports/Monthly
    └─ Custom Reports (Level 2, Order 3) → /Reports/Custom

  Assets (Level 1, Order 4)
    ├─ Hardware (Level 2, Order 1)
    │   ├─ Computers (Level 3, Order 1) → /Hardware/Computers
    │   ├─ Printers (Level 3, Order 2) → /Hardware/Printers
    │   └─ Network Equipment (Level 3, Order 3) → /Hardware/Network
    └─ Software (Level 2, Order 2) → /Software/Index

Actions:
  - Drag to reorder (updates DisplayOrder)
  - Drag to change parent (updates ParentMenuItemId, Level)
  - Click to edit
  - Delete (prompts if has children)
```

#### **WF-2.22: Menu Item Reordering**
```
Trigger: Admin drags menu item to new position
Steps:
  1. Capture drop position
  2. If changing parent:
     - Update ParentMenuItemId
     - Recalculate Level (parent.Level + 1)
  3. Update DisplayOrder for all siblings
  4. Save changes
  5. Menu updates on next page load
```

---

## 8. ROLE MENU ITEMS

### **CRUD Operations:**
- **CREATE** Role-MenuItem Assignment
- **READ** MenuItems for Role
- **READ** Roles for MenuItem
- **DELETE** Role-MenuItem Assignment

### **Business Rules:**
- Unique constraint: (RoleId, MenuItemId)
- IsVisible flag: Show/hide menu item for role
- Cascade considerations: If parent menu hidden, children also hidden

### **Workflows:**

#### **WF-2.23: Assign Menu Items to Role**
```
Trigger: Admin configures role menu visibility
Steps:
  1. Select Role
  2. Display menu hierarchy with checkboxes:

     ☑ Dashboard
     ☑ Forms
       ☑ Assigned Forms
       ☑ My Submissions
       ☑ Pending Approvals
       ☐ Form Templates (unchecked = hidden for this role)
     ☑ Reports
       ☑ Dashboard
       ☐ Custom Reports (hidden)
     ☐ Assets (entire module hidden)
     ☑ Settings

  3. Admin checks/unchecks items
  4. Click "Save Menu Configuration"
  5. For each checked item:
     - INSERT OR UPDATE RoleMenuItems (RoleId, MenuItemId, IsVisible=1)
  6. For each unchecked item:
     - DELETE FROM RoleMenuItems (or UPDATE IsVisible=0)
  7. Show success: "Menu configuration saved for {RoleName}"
```

#### **WF-2.24: Generate User Sidebar (On Page Load)**
```
Trigger: User loads any page
Query Logic:
  1. Get user's roles from UserRoles
  2. Get visible menu items:
     SELECT DISTINCT mi.*
     FROM MenuItems mi
     INNER JOIN RoleMenuItems rmi ON mi.MenuItemId = rmi.MenuItemId
     INNER JOIN UserRoles ur ON rmi.RoleId = ur.RoleId
     WHERE ur.UserId = @UserId
       AND rmi.IsVisible = 1
       AND mi.IsActive = 1
       AND mi.IsVisible = 1
     ORDER BY mi.ParentMenuItemId, mi.DisplayOrder

  3. Build tree structure (parent-child relationships)
  4. Render sidebar HTML with:
     - Top-level items as main menu
     - Children as collapsible submenus
     - Icons from MenuItem.Icon
     - Links to MenuItem.Route

  5. Highlight active menu item (based on current URL)
  6. Cache for 30 minutes (invalidate on role/menu changes)
```

#### **WF-2.25: Bulk Menu Assignment**
```
Trigger: Admin wants to copy menu config from one role to another
Steps:
  1. Select "Copy From" role (source)
  2. Select "Copy To" role(s) (destination, multi-select)
  3. Preview: "Copy menu configuration from {SourceRole} to X roles?"
  4. Confirm
  5. For each destination role:
     - DELETE existing RoleMenuItems
     - INSERT INTO RoleMenuItems (SELECT from source role)
  6. Show success: "Menu configuration copied to X roles"
```

---

## 9. USER TENANT ACCESS

### **CRUD Operations:**
- **CREATE** Tenant Access Exception
- **READ** Tenant Access for User
- **READ** Users with Access to Tenant
- **UPDATE** Extend/Modify Access (ExpiryDate, Reason)
- **DELETE** Revoke Access

### **Business Rules:**
- Unique constraint: (UserId, TenantId)
- GrantedBy required (who granted access)
- Reason required (audit trail)
- ExpiryDate optional (NULL = permanent)
- IsActive for soft delete
- This table is for **exceptions only** (automatic access via Role.Level)

### **Workflows:**

#### **WF-2.26: Grant Temporary Tenant Access**
```
Trigger: Admin needs to give user temporary access to tenant outside their normal scope
Example: Factory user needs access to subsidiary for 3-month project

Steps:
  1. Select User
  2. Display user's current access:
     - Automatic: "Level 3 (Factory) → Access to Kiambu Factory only"
     - Exceptions: (list any existing UserTenantAccess records)

  3. Click "Grant Additional Access"
  4. Select Tenant (dropdown: show tenants user doesn't already have access to)
  5. Enter Reason (required):
     - "ERP Implementation Project"
     - "Q4 2025 Audit"
     - "Training Assignment"
  6. Set ExpiryDate (optional):
     - DatePicker or "No Expiry"
  7. Confirm: "Grant {UserName} access to {TenantName} until {ExpiryDate}?"
  8. INSERT INTO UserTenantAccess (UserId, TenantId, GrantedBy, ExpiryDate, Reason)
  9. Invalidate user's claims cache
  10. Send notification to user
  11. Log in AuditLogs
  12. Show success: "Access granted"
```

#### **WF-2.27: Revoke Tenant Access**
```
Trigger: Admin revokes exception access
Steps:
  1. View user's UserTenantAccess records
  2. Click "Revoke" next to record
  3. Confirm: "Revoke {UserName}'s access to {TenantName}?"
  4. Option: Add revocation reason
  5. Set IsActive = 0 (soft delete for audit trail)
  6. Invalidate user's claims cache
  7. Notify user
  8. Log in AuditLogs
```

#### **WF-2.28: Auto-Expire Access (Scheduled Job)**
```
Trigger: Daily Hangfire job at 2 AM
Steps:
  1. Query: SELECT * FROM UserTenantAccess
            WHERE ExpiryDate < NOW() AND IsActive = 1
  2. For each expired access:
     - Set IsActive = 0
     - Send notification to user: "Your access to {TenantName} has expired"
     - Log in AuditLogs
  3. Summary email to admins: "X tenant accesses expired today"
```

#### **WF-2.29: Tenant Access Report**
```
Trigger: Admin runs "User Access Audit" report
Query Logic:
  For each user:
    - Show Role.Level automatic access
    - Show UserTenantAccess exceptions
    - Flag: Active vs Expired
    - Show GrantedBy and Reason

Output Example:
  User: John Doe (FACTORY_ICT role, Level 3)
    Automatic Access:
      ✓ Kiambu Factory (via role level)

    Exception Access:
      ✓ Chai Trading Co. (Subsidiary) - Active until 2025-12-31
        Reason: "ERP Implementation Project"
        Granted by: Admin User on 2025-10-01

      ✗ Thika Factory - EXPIRED (2025-09-30)
        Reason: "Temporary Support Assignment"
```

---

## 10. USER GROUPS

### **CRUD Operations:**
- **CREATE** User Group
- **READ** User Group (Single)
- **READ** All User Groups (List, filter by TenantId)
- **UPDATE** User Group Details
- **DELETE** User Group (Cascade deletes UserGroupMembers)

### **Business Rules:**
- GroupCode must be unique
- TenantId optional (NULL = cross-tenant group)
- GroupType examples: 'Training', 'Project', 'Committee', 'Custom'
- Cannot delete if group is referenced in FormTemplateAssignments

### **Workflows:**

#### **WF-2.30: Create User Group**
```
Trigger: Admin creates new user group
Steps:
  1. Enter GroupName (e.g., "Q1 2025 Trainees", "ERP Implementation Team")
  2. Enter GroupCode (e.g., "Q1_TRAIN_2025", "ERP_TEAM")
  3. Select GroupType (dropdown):
     - Training
     - Project
     - Committee
     - Department
     - Custom
  4. Select Tenant (optional):
     - NULL = Cross-tenant group
     - Specific TenantId = Scoped to that tenant
  5. Enter Description
  6. Save UserGroup with IsActive = 1
  7. Redirect to "Add Members" screen
```

#### **WF-2.31: Add Users to Group**
```
Trigger: Admin selects "Manage Members" for group
Steps:
  1. Display current members (with "Remove" button)
  2. Click "Add Members"
  3. Show user selection modal:
     - List all active users
     - Filters: By Tenant, Department, Role
     - Multi-select checkboxes
     - Exclude users already in group
  4. User selects multiple users
  5. Click "Add Selected"
  6. For each selected user:
     - INSERT INTO UserGroupMembers (UserGroupId, UserId, AddedBy, AddedDate)
  7. Refresh members list
  8. Show success: "X users added to {GroupName}"
```

#### **WF-2.32: Remove User from Group**
```
Trigger: Admin clicks "Remove" next to member
Steps:
  1. Confirm: "Remove {UserName} from {GroupName}?"
  2. DELETE FROM UserGroupMembers WHERE UserGroupId = X AND UserId = Y
  3. Refresh list
  4. Show success: "{UserName} removed from group"
```

#### **WF-2.33: User Group Use Cases**

**Training Cohort:**
```
Group: "Q1 2025 Trainees"
Purpose: Give temporary access to training forms
Duration: 3 months
Members: 15 new factory staff
Assignment:
  - Training form templates assigned to this UserGroup
  - Access expires after training period
```

**Project Team:**
```
Group: "ERP Implementation Team"
Purpose: Cross-functional team needing specific module access
Duration: 6 months
Members: 8 staff from different departments/tenants
Assignment:
  - ERP-related forms assigned to group
  - Access to specific tenant group for testing
```

**Committee:**
```
Group: "Budget Review Committee"
Purpose: Quarterly budget approval
Duration: Permanent (membership changes)
Members: CFO + 4 managers
Assignment:
  - Budget-related forms/reports assigned to group
  - Rotates members as needed
```

---

## 11. USER GROUP MEMBERS

### **CRUD Operations:**
- **CREATE** Group Member (via WF-2.31)
- **READ** Group Members (List for specific group)
- **READ** Groups for User
- **DELETE** Group Member (via WF-2.32)

### **Business Rules:**
- Unique constraint: (UserGroupId, UserId)
- CASCADE DELETE when UserGroup is deleted
- AddedBy tracks who added the user

### **Workflows:**
See WF-2.31, WF-2.32 above (managed through UserGroups UI)

#### **WF-2.34: Bulk Group Assignment**
```
Trigger: Admin wants to add all users from a department to a group
Steps:
  1. Select UserGroup
  2. Click "Bulk Add"
  3. Choose criteria:
     - All users in Department X
     - All users with Role Y
     - All users in Tenant Z
  4. Preview: "Add X users to {GroupName}?"
  5. Confirm
  6. For each user matching criteria:
     - Check if already in group (skip if exists)
     - INSERT INTO UserGroupMembers
  7. Show summary: "X users added to group"
```

---

## CROSS-TABLE WORKFLOWS

### **WF-2.35: Complete User Setup Wizard**
```
Trigger: Admin selects "Add New User" (complete setup)
Steps:
  1. Step 1: Basic Info
     - Name, Email, UserName, EmployeeNumber
     - Department (determines primary Tenant)

  2. Step 2: Role Assignment
     - Multi-select roles
     - Show role descriptions and Level

  3. Step 3: Tenant Access (if needed)
     - Show automatic access based on Role.Level
     - Option to add exceptions via UserTenantAccess

  4. Step 4: User Groups (optional)
     - Multi-select existing UserGroups
     - Or create new group on the fly

  5. Step 5: Credentials
     - Auto-generate password
     - Send welcome email checkbox

  6. Summary & Confirm
     - Review all selections
     - Click "Create User"

  7. Post-Creation:
     - INSERT INTO Users (hashed password)
     - INSERT INTO UserRoles (for each role)
     - INSERT INTO UserTenantAccess (for each exception)
     - INSERT INTO UserGroupMembers (for each group)
     - Send welcome email with credentials
     - Log in AuditLogs
     - Redirect to User Details page
```

### **WF-2.36: Role Configuration Wizard**
```
Trigger: Admin selects "Create New Role" (complete setup)
Steps:
  1. Step 1: Basic Details
     - RoleName, RoleCode
     - Select Level (1/2/3)
     - Description

  2. Step 2: Assign Permissions
     - Checkbox list grouped by Module
     - Or select "Use Template" (copy from existing role)

  3. Step 3: Configure Menu Visibility
     - Tree view of all MenuItems
     - Check items visible to this role

  4. Summary & Confirm
     - Review role details, permissions, menu items
     - Click "Create Role"

  5. Post-Creation:
     - INSERT INTO Roles
     - INSERT INTO RolePermissions (for each permission)
     - INSERT INTO RoleMenuItems (for each menu item)
     - Log in PermissionAuditLog
     - Redirect to "Assign Users" screen
```

### **WF-2.37: Permission Audit Report**
```
Trigger: Admin/Auditor runs "Permission Audit" report
Queries:
  1. User Permission Summary:
     - User → Roles → Permissions
     - Show all effective permissions per user

  2. Role Permission Matrix:
     - Roles × Permissions grid (checkmarks)

  3. Permission Change Log:
     - Query PermissionAuditLog
     - Who granted/revoked permissions, when, why

  4. Tenant Access Audit:
     - Automatic access (via Role.Level)
     - Exception access (via UserTenantAccess)
     - Expired access

  5. Menu Visibility Audit:
     - Which roles see which menus
     - Users with access to specific menu item

Export Options:
  - Excel
  - PDF
  - CSV
```

### **WF-2.38: Claims Refresh Trigger**
```
Events that Require Claims Cache Invalidation:
  1. User assigned new role → Invalidate user's claims
  2. User removed from role → Invalidate user's claims
  3. Role's permissions changed → Invalidate all users with that role
  4. User granted tenant access → Invalidate user's claims
  5. User added to UserGroup → Invalidate user's claims (if groups affect permissions)
  6. Menu items reassigned to role → Invalidate all users with that role

Implementation:
  - Set cache key: "UserClaims_{UserId}"
  - On invalidation: Remove from cache
  - On next request: Rebuild claims (WF-2.7)
  - Force re-login option (invalidate auth cookie)
```

### **WF-2.39: Tenant Access Resolution (Runtime)**
```
Function: CanUserAccessTenant(int userId, int tenantId)

Logic:
  1. Get user's roles and their levels
  2. Check Level 1 roles:
     - If exists → ALLOW (access all tenants)

  3. Check Level 2 roles:
     - Get user's assigned region(s)
     - Get tenant's region
     - If match → ALLOW

  4. Check Level 3 roles:
     - Get user's primary tenant (via DepartmentId)
     - If tenantId == user's tenant → ALLOW

  5. Check UserTenantAccess exceptions:
     - Query: WHERE UserId = X AND TenantId = Y AND IsActive = 1
              AND (ExpiryDate IS NULL OR ExpiryDate > NOW())
     - If exists → ALLOW

  6. Otherwise → DENY

Cache Result: 30 minutes per user-tenant combination
```

---

## SUMMARY

### **Total Operations:**
- **CRUD Actions:** 55+ operations across 11 tables
- **Workflows:** 39 defined workflows
- **Business Rules:** 30+ validation rules

### **Key Integration Points:**
1. **Users ↔ Departments/Tenants** → Primary tenant determination
2. **UserRoles ↔ RolePermissions** → Effective permissions
3. **RoleMenuItems** → Dynamic sidebar generation
4. **UserTenantAccess** → Exception-based multi-tenant access
5. **UserGroups ↔ FormTemplateAssignments** → Form access control
6. **Role.Level** → Automatic tenant access (1=all, 2=region, 3=single)
7. **Claims-based authentication** → HttpContext.User.Claims
8. **PermissionAuditLog** → Complete audit trail

### **Permissions Required:**
```
System Configuration (Modules, Permissions, MenuItems):
  - System.Configure → SYSADMIN only

Role Management:
  - Roles.Manage → SYSADMIN, HO_ICT_MGR
  - Roles.AssignPermissions → SYSADMIN, HO_ICT_MGR

User Management:
  - Users.Create → SYSADMIN, HO_ICT_MGR, REGIONAL_MGR (own region)
  - Users.Edit → SYSADMIN, HO_ICT_MGR, REGIONAL_MGR (own region)
  - Users.Delete → SYSADMIN only
  - Users.AssignRoles → SYSADMIN, HO_ICT_MGR
  - Users.GrantTenantAccess → SYSADMIN, HO_ICT_MGR
  - Users.ViewAll → SYSADMIN, HO_ICT_MGR, REGIONAL_MGR (own region)

User Groups:
  - UserGroups.Manage → SYSADMIN, HO_ICT_MGR, REGIONAL_MGR (own region)
  - UserGroups.ManageMembers → SYSADMIN, HO_ICT_MGR

Audit:
  - Audit.ViewPermissionLogs → SYSADMIN, AUDITOR
```

### **Security Considerations:**
1. **Password Complexity:** Enforced by ASP.NET Identity (min length, special chars, etc.)
2. **Account Lockout:** 5 failed attempts → 30-minute lockout
3. **Claims Caching:** 30-minute TTL, invalidate on role/permission changes
4. **Audit Trail:** All permission changes logged in PermissionAuditLog
5. **Tenant Isolation:** Level-based automatic access + exception tracking
6. **Session Management:** Secure cookies, HTTPS only
7. **Role Hierarchy:** Level 1 > Level 2 > Level 3 (privilege escalation prevention)

---

**End of Section 2 Workflows**

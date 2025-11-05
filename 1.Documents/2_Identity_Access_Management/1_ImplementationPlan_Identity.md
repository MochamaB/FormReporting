# Identity & Access Management - Implementation Plan

**Version:** 1.0
**Date:** October 30, 2025
**Phase:** Phase 1 - Week 3 (Identity & Access Management)
**Duration:** 5 Working Days
**Section:** 2 - Identity & Access Management

---

## Table of Contents

1. [Implementation Overview](#overview)
2. [Week 3 Day-by-Day Breakdown](#daily-breakdown)
3. [Database Schema Implementation](#database-schema)
4. [ASP.NET Core Identity Setup](#identity-setup)
5. [Authentication Implementation](#authentication)
6. [Authorization Implementation](#authorization)
7. [UI Components](#ui-components)
8. [Seed Data Strategy](#seed-data)
9. [Testing Strategy](#testing)
10. [Success Criteria](#success-criteria)

---

## <a name="overview"></a>Implementation Overview

### Purpose

Build the **Identity & Access Management** system that provides secure authentication and authorization for all users. This includes:
- ASP.NET Core Identity setup
- User authentication (login/logout/password reset)
- User management (CRUD operations)
- Role-based access control (6 standard roles)
- User role assignments
- User tenant access control
- Authorization policies

### Goals for Week 3

1. ✅ Set up ASP.NET Core Identity with custom user model
2. ✅ Create database tables: Users, Roles, UserRoles, UserTenantAccess
3. ✅ Implement authentication (login, logout, password reset)
4. ✅ Build user management UI (CRUD operations)
5. ✅ Implement role management and user role assignment
6. ✅ Build user tenant access management
7. ✅ Create authorization policies
8. ✅ Seed 6 standard roles
9. ✅ Test complete authentication and authorization flow

### Technology Stack

- **Authentication:** ASP.NET Core Identity 8.0
- **Password Hashing:** PBKDF2 (Identity default)
- **Backend:** ASP.NET Core 8.0 MVC + Razor Pages + C# 12
- **Database:** SQL Server 2022 Standard Edition
- **Frontend:** Bootstrap 5 + jQuery + DataTables.js
- **Email:** SMTP service for password resets

### Prerequisites

- ✅ Phase 0 completed (environment setup)
- ✅ Week 2 completed (Organizational Structure with Tenants and Regions)
- ✅ SQL Server 2022 operational
- ✅ SMTP server configured (or use development mode)

---

## <a name="daily-breakdown"></a>Week 3: Day-by-Day Breakdown

### Day 1 (Monday): ASP.NET Core Identity Setup

**Focus:** Configure Identity and create database tables

**Morning (4 hours):**
1. Install NuGet packages:
   - Microsoft.AspNetCore.Identity.EntityFrameworkCore
   - Microsoft.AspNetCore.Identity.UI
2. Create custom `ApplicationUser` class extending `IdentityUser`
3. Add custom properties: FirstName, LastName, EmployeeNumber, PrimaryTenantId, Status
4. Update `ApplicationDbContext` to inherit from `IdentityDbContext<ApplicationUser>`
5. Configure Identity in Program.cs (password policies, lockout settings)

**Afternoon (4 hours):**
1. Create `Roles` table (custom, not using IdentityRole)
2. Create `UserRoles` table (custom many-to-many)
3. Create `UserTenantAccess` table
4. Create EF Core configurations for custom tables
5. Generate migration: `Add-Migration AddIdentityTables`
6. Apply migration: `Update-Database`
7. Verify all Identity tables created (AspNetUsers, AspNetUserClaims, etc.)

**Deliverables:**
- ✅ ASP.NET Core Identity configured
- ✅ Custom ApplicationUser class created
- ✅ All Identity tables created (9 tables total)
- ✅ Custom tables: Roles, UserRoles, UserTenantAccess
- ✅ Migration applied successfully

**Testing:**
- Verify AspNetUsers table has custom columns
- Verify all foreign key relationships work
- Test unique constraints on Roles.RoleCode

---

### Day 2 (Tuesday): Authentication & User Management Backend

**Focus:** Implement authentication logic and user service layer

**Morning (4 hours):**
1. Create authentication service: `IAuthenticationService`
   - Login(username, password)
   - Logout()
   - ResetPassword(email)
   - ChangePassword(userId, oldPassword, newPassword)
   - ValidateUser(username, password)
2. Implement `AuthenticationService` using UserManager and SignInManager
3. Configure cookie authentication settings
4. Implement password validation rules
5. Implement account lockout logic (5 failed attempts)

**Afternoon (4 hours):**
1. Create user service: `IUserService`
   - CreateUser(dto)
   - UpdateUser(userId, dto)
   - GetUserById(userId)
   - GetAllUsers()
   - ActivateUser(userId)
   - DeactivateUser(userId)
   - ResetUserPassword(userId)
2. Implement `UserService` class
3. Create DTOs: CreateUserDto, UpdateUserDto, UserDto
4. Create validators using FluentValidation
5. Add AutoMapper profiles for user mappings

**Deliverables:**
- ✅ IAuthenticationService and implementation
- ✅ IUserService and implementation
- ✅ User DTOs and validators
- ✅ AutoMapper profiles
- ✅ Password policy enforced

**Testing:**
- Unit tests for authentication service
- Unit tests for user service validation
- Test password hashing works correctly
- Test account lockout logic

---

### Day 3 (Wednesday): Role Management & User Role Assignment

**Focus:** Implement role management and role assignment logic

**Morning (4 hours):**
1. Create role service: `IRoleService`
   - CreateRole(dto) - Admin only
   - UpdateRole(roleId, dto)
   - GetAllRoles()
   - GetRoleById(roleId)
   - ActivateRole(roleId)
   - DeactivateRole(roleId)
2. Implement `RoleService` class
3. Create DTOs: CreateRoleDto, UpdateRoleDto, RoleDto
4. Add validation for role creation

**Afternoon (4 hours):**
1. Create user role service: `IUserRoleService`
   - AssignRoleToUser(userId, roleId)
   - RemoveRoleFromUser(userId, roleId)
   - GetUserRoles(userId)
   - GetUsersInRole(roleId)
   - SetPrimaryRole(userId, roleId)
2. Implement `UserRoleService` class
3. Create DTOs: UserRoleDto, AssignRoleDto
4. Add business logic:
   - Prevent duplicate role assignments
   - Validate role level compatibility
   - Track who assigned the role

**Deliverables:**
- ✅ IRoleService and implementation
- ✅ IUserRoleService and implementation
- ✅ Role and UserRole DTOs
- ✅ Business validation rules
- ✅ Audit tracking for role assignments

**Testing:**
- Unit tests for role service
- Unit tests for user role assignment logic
- Test duplicate role prevention
- Test primary role switching

---

### Day 4 (Thursday): User Tenant Access & Authorization Policies

**Focus:** Implement tenant access control and authorization

**Morning (4 hours):**
1. Create user tenant access service: `IUserTenantAccessService`
   - GrantTenantAccess(userId, tenantId, permissions)
   - RevokeTenantAccess(userId, tenantId)
   - GetUserTenants(userId)
   - GetTenantUsers(tenantId)
   - UpdateTenantPermissions(userId, tenantId, permissions)
   - SetPrimaryTenant(userId, tenantId)
2. Implement `UserTenantAccessService` class
3. Create DTOs: UserTenantAccessDto, GrantAccessDto
4. Add business logic for auto-assignment based on role:
   - FACTORY_ICT → Auto-assign to one factory
   - REGIONAL_MGR → Auto-assign to all factories in region
   - HO_ICT_MGR/SYSADMIN → Auto-assign to all tenants

**Afternoon (4 hours):**
1. Create authorization handlers:
   - `TenantAccessHandler` - Checks UserTenantAccess table
   - `RoleAuthorizationHandler` - Checks user roles
2. Create authorization requirements:
   - `TenantAccessRequirement`
   - `AdminRequirement`
3. Define authorization policies in Program.cs:
   - "AdminOnly" - SYSADMIN or HO_ICT_MGR
   - "CanApproveSubmissions" - HO_ICT_MGR, REGIONAL_MGR, FACTORY_MGR
   - "CanManageUsers" - SYSADMIN or HO_ICT_MGR
   - "TenantAccess" - Custom tenant access check
4. Add claims transformation to include user's tenants in claims
5. Integrate tenant context service with UserTenantAccess

**Deliverables:**
- ✅ IUserTenantAccessService and implementation
- ✅ Authorization handlers and requirements
- ✅ 4 authorization policies configured
- ✅ Claims transformation implemented
- ✅ Auto-assignment logic based on roles

**Testing:**
- Unit tests for tenant access service
- Integration tests for authorization policies
- Test auto-assignment for different role types
- Test tenant-based data filtering

---

### Day 5 (Friday): UI Components, Seed Data & Testing

**Focus:** Build UI for authentication and user management, seed roles, test everything

**Morning (4 hours):**
1. Create authentication UI:
   - Login page (`/Account/Login`)
   - Logout functionality
   - Forgot password page (`/Account/ForgotPassword`)
   - Reset password page (`/Account/ResetPassword`)
   - Change password page (`/Account/ChangePassword`)
2. Add client-side validation for login and password forms
3. Implement "Remember Me" functionality
4. Add account lockout message display
5. Create seed data service: `IdentitySeedService`
6. Seed 6 standard roles
7. Create default admin user (SYSADMIN)

**Afternoon (4 hours):**
1. Create user management UI:
   - Users list page (`/Admin/Users/Index`)
   - Create user page (`/Admin/Users/Create`)
   - Edit user page (`/Admin/Users/Edit`)
   - User details page (`/Admin/Users/Details`)
   - Assign roles modal
   - Assign tenant access modal
2. Test complete authentication flow:
   - Login with valid credentials
   - Login with invalid credentials (test lockout)
   - Password reset via email
   - Logout
3. Test user management:
   - Create user with role assignment
   - Edit user details
   - Activate/deactivate user
   - Assign multiple roles
   - Assign tenant access
4. Test authorization:
   - Verify role-based page access
   - Verify tenant-based data filtering
   - Test policy enforcement

**Deliverables:**
- ✅ Complete authentication UI (5 pages)
- ✅ User management UI (4 pages)
- ✅ 6 standard roles seeded
- ✅ Default admin user created
- ✅ All authentication flows tested
- ✅ All authorization policies tested

**Testing:**
- Manual testing checklist completed
- Test all user CRUD operations
- Test role assignment UI
- Test tenant access assignment UI
- Verify email sending works (or mock in dev)

---

## <a name="database-schema"></a>Database Schema Implementation

### ASP.NET Core Identity Tables (Auto-Generated)

**Tables Created by Identity:**

1. **AspNetUsers** - User accounts
2. **AspNetRoles** - NOT USED (we use custom Roles table)
3. **AspNetUserRoles** - NOT USED (we use custom UserRoles table)
4. **AspNetUserClaims** - User claims
5. **AspNetUserLogins** - External login providers (OAuth)
6. **AspNetUserTokens** - Authentication tokens
7. **AspNetRoleClaims** - NOT USED
8. **AspNetUserTokens** - Password reset tokens

**Note:** We use custom `Roles` and `UserRoles` tables instead of Identity's built-in role tables to support custom fields like Level, Description, and assignment tracking.

---

### Custom Table 1: ApplicationUser (Extends AspNetUsers)

**Purpose:** Store user accounts with custom properties

**SQL Schema:**
```sql
-- ASP.NET Core Identity creates AspNetUsers table automatically
-- We add custom columns via ApplicationUser entity

ALTER TABLE AspNetUsers ADD
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    EmployeeNumber NVARCHAR(20) UNIQUE,
    PrimaryTenantId INT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(100);

ALTER TABLE AspNetUsers ADD
    CONSTRAINT FK_AspNetUsers_Tenants FOREIGN KEY (PrimaryTenantId)
        REFERENCES Tenants(TenantId) ON DELETE SET NULL;

CREATE INDEX IX_AspNetUsers_PrimaryTenantId ON AspNetUsers(PrimaryTenantId);
CREATE INDEX IX_AspNetUsers_Status ON AspNetUsers(Status);
CREATE INDEX IX_AspNetUsers_EmployeeNumber ON AspNetUsers(EmployeeNumber);
```

**Sample Data:**
```
┌──────────────────────┬───────────┬──────────┬────────────────┬───────────────┬────────┐
│ UserId (Email)       │ FirstName │ LastName │ EmployeeNumber │ PrimaryTenant │ Status │
├──────────────────────┼───────────┼──────────┼────────────────┼───────────────┼────────┤
│ admin@ktda.com       │ System    │ Admin    │ EMP001         │ 1 (HO)        │ Active │
│ elizabeth.ndegwa@... │ Elizabeth │ Ndegwa   │ EMP038         │ 38 (Kangaita) │ Active │
│ eric.kinyeki@...     │ Eric      │ Kinyeki  │ EMP203         │ 1 (HO)        │ Active │
│ martin.mwarangu@...  │ Martin    │ Mwarangu │ EMP100         │ 1 (HO)        │ Active │
└──────────────────────┴───────────┴──────────┴────────────────┴───────────────┴────────┘
```

---

### Custom Table 2: Roles

**Purpose:** Define system roles with hierarchical levels

**SQL Schema:**
```sql
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(100) NOT NULL,
    RoleCode NVARCHAR(20) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    Level INT NOT NULL,  -- 1=HeadOffice, 2=Regional, 3=Factory
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(100),

    CONSTRAINT CK_Roles_Level CHECK (Level BETWEEN 1 AND 3)
);

CREATE INDEX IX_Roles_RoleCode ON Roles(RoleCode);
CREATE INDEX IX_Roles_Level ON Roles(Level);
```

**6 Standard Roles (Seed Data):**
```sql
INSERT INTO Roles (RoleName, RoleCode, Description, Level) VALUES
('System Administrator', 'SYSADMIN', 'Full system access and management', 1),
('Head Office ICT Manager', 'HO_ICT_MGR', 'Head office ICT management', 1),
('Regional ICT Manager', 'REGIONAL_MGR', 'Regional ICT oversight', 2),
('Factory ICT Support', 'FACTORY_ICT', 'Factory-level ICT support', 3),
('Factory Manager', 'FACTORY_MGR', 'Factory management with approval rights', 3),
('Report Viewer', 'VIEWER', 'Read-only access to reports', 1);
```

**Roles Table:**
```
┌────────┬───────────────────────────┬──────────────┬─────────────┬────────┐
│ RoleId │ RoleName                  │ RoleCode     │ Description │ Level  │
├────────┼───────────────────────────┼──────────────┼─────────────┼────────┤
│ 1      │ System Administrator      │ SYSADMIN     │ Full access │ 1      │
│ 2      │ Head Office ICT Manager   │ HO_ICT_MGR   │ HO ICT mgmt │ 1      │
│ 3      │ Regional ICT Manager      │ REGIONAL_MGR │ Region mgmt │ 2      │
│ 4      │ Factory ICT Support       │ FACTORY_ICT  │ Factory ICT │ 3      │
│ 5      │ Factory Manager           │ FACTORY_MGR  │ Factory mgr │ 3      │
│ 6      │ Report Viewer             │ VIEWER       │ Read-only   │ 1      │
└────────┴───────────────────────────┴──────────────┴─────────────┴────────┘
```

---

### Custom Table 3: UserRoles (Many-to-Many)

**Purpose:** Assign roles to users (users can have multiple roles)

**SQL Schema:**
```sql
CREATE TABLE UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(450) NOT NULL,
    RoleId INT NOT NULL,
    IsPrimaryRole BIT NOT NULL DEFAULT 0,
    AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AssignedBy NVARCHAR(100),
    RevokedAt DATETIME2 NULL,
    RevokedBy NVARCHAR(100) NULL,

    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId)
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId)
        REFERENCES Roles(RoleId) ON DELETE CASCADE,
    CONSTRAINT UQ_UserRoles_User_Role UNIQUE (UserId, RoleId)
);

CREATE INDEX IX_UserRoles_UserId ON UserRoles(UserId);
CREATE INDEX IX_UserRoles_RoleId ON UserRoles(RoleId);
CREATE INDEX IX_UserRoles_IsPrimaryRole ON UserRoles(IsPrimaryRole);
```

**Sample Data:**
```
┌────────────┬──────────────────────┬────────┬───────────────┬────────────┐
│ UserRoleId │ UserId               │ RoleId │ IsPrimaryRole │ AssignedBy │
├────────────┼──────────────────────┼────────┼───────────────┼────────────┤
│ 1          │ admin@ktda.com       │ 1      │ 1 (Primary)   │ SYSTEM     │
│ 2          │ elizabeth.ndegwa@... │ 4      │ 1 (Primary)   │ admin      │
│ 3          │ eric.kinyeki@...     │ 3      │ 1 (Primary)   │ admin      │
│ 4          │ martin.mwarangu@...  │ 2      │ 1 (Primary)   │ admin      │
└────────────┴──────────────────────┴────────┴───────────────┴────────────┘
```

**Business Rules:**
- User can have multiple roles
- Only one role can be marked IsPrimaryRole = 1
- Primary role determines default dashboard view
- Cannot remove primary role without assigning a new one
- Track who assigned the role (AssignedBy)
- Support role revocation with RevokedAt timestamp

---

### Custom Table 4: UserTenantAccess

**Purpose:** Control which tenants each user can access

**SQL Schema:**
```sql
CREATE TABLE UserTenantAccess (
    UserTenantAccessId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(450) NOT NULL,
    TenantId INT NOT NULL,
    IsPrimaryTenant BIT NOT NULL DEFAULT 0,
    CanRead BIT NOT NULL DEFAULT 1,
    CanWrite BIT NOT NULL DEFAULT 0,
    CanApprove BIT NOT NULL DEFAULT 0,
    GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    GrantedBy NVARCHAR(100),
    RevokedAt DATETIME2 NULL,
    RevokedBy NVARCHAR(100) NULL,

    CONSTRAINT FK_UserTenantAccess_Users FOREIGN KEY (UserId)
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserTenantAccess_Tenants FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId) ON DELETE CASCADE,
    CONSTRAINT UQ_UserTenantAccess_User_Tenant UNIQUE (UserId, TenantId)
);

CREATE INDEX IX_UserTenantAccess_UserId ON UserTenantAccess(UserId);
CREATE INDEX IX_UserTenantAccess_TenantId ON UserTenantAccess(TenantId);
CREATE INDEX IX_UserTenantAccess_IsPrimaryTenant ON UserTenantAccess(IsPrimaryTenant);
```

**Sample Data:**
```
┌─────────────────────┬──────────────────────┬──────────┬─────────────┬────────┬──────────┬───────────┐
│ UserTenantAccessId  │ UserId               │ TenantId │ IsPrimary   │ CanRead│ CanWrite │ CanApprove│
├─────────────────────┼──────────────────────┼──────────┼─────────────┼────────┼──────────┼───────────┤
│ 1                   │ elizabeth.ndegwa@... │ 38       │ 1 (Yes)     │ 1      │ 1        │ 0         │
│ 2                   │ eric.kinyeki@...     │ 38       │ 0 (No)      │ 1      │ 0        │ 1         │
│ 3                   │ eric.kinyeki@...     │ 39       │ 0 (No)      │ 1      │ 0        │ 1         │
│ 4                   │ eric.kinyeki@...     │ 40       │ 0 (No)      │ 1      │ 0        │ 1         │
│ ...                 │ ...                  │ ...      │ ...         │ ...    │ ...      │ ...       │
│ 10                  │ eric.kinyeki@...     │ 45       │ 0 (No)      │ 1      │ 0        │ 1         │
└─────────────────────┴──────────────────────┴──────────┴─────────────┴────────┴──────────┴───────────┘
```

**Permission Levels:**
- **CanRead**: User can view data for this tenant
- **CanWrite**: User can create/edit data for this tenant
- **CanApprove**: User can approve submissions for this tenant
- **IsPrimaryTenant**: User's home tenant (only one per user)

**Access Patterns:**
- FACTORY_ICT: Access to 1 factory (CanRead=1, CanWrite=1, CanApprove=0)
- REGIONAL_MGR: Access to all factories in region (CanRead=1, CanWrite=0, CanApprove=1)
- HO_ICT_MGR: Access to all 80 tenants (CanRead=1, CanWrite=1, CanApprove=1)
- VIEWER: Access to all 80 tenants (CanRead=1, CanWrite=0, CanApprove=0)

---

## <a name="identity-setup"></a>ASP.NET Core Identity Setup

### ApplicationUser Entity

**File:** `Core/Entities/ApplicationUser.cs`

**Properties:**
```
Inherited from IdentityUser:
- Id (string, primary key)
- UserName (string, unique)
- NormalizedUserName
- Email (string)
- NormalizedEmail
- EmailConfirmed (bool)
- PasswordHash (string)
- SecurityStamp
- ConcurrencyStamp
- PhoneNumber
- PhoneNumberConfirmed
- TwoFactorEnabled
- LockoutEnd (DateTimeOffset?)
- LockoutEnabled
- AccessFailedCount (int)

Custom properties:
- FirstName (string, required)
- LastName (string, required)
- EmployeeNumber (string, unique)
- PrimaryTenantId (int?, FK to Tenants)
- Status (string: Active, Inactive, Locked)
- CreatedAt (DateTime)
- CreatedBy (string)
- UpdatedAt (DateTime?)
- UpdatedBy (string?)

Navigation properties:
- Tenant? PrimaryTenant
- ICollection<UserRole> UserRoles
- ICollection<UserTenantAccess> TenantAccess
```

---

### Identity Configuration in Program.cs

**Password Policy:**
```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;  // Set to true in production
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

**Cookie Settings:**
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);  // Absolute timeout
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;  // Reset timeout on activity
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // HTTPS only
});
```

---

### Authorization Policies

**Configure in Program.cs:**
```csharp
builder.Services.AddAuthorization(options =>
{
    // AdminOnly: SYSADMIN or HO_ICT_MGR
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
        {
            var userRoles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            return userRoles.Contains("SYSADMIN") || userRoles.Contains("HO_ICT_MGR");
        }));

    // CanApproveSubmissions: HO_ICT_MGR, REGIONAL_MGR, or FACTORY_MGR
    options.AddPolicy("CanApproveSubmissions", policy =>
        policy.RequireAssertion(context =>
        {
            var userRoles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            return userRoles.Any(r => new[] { "HO_ICT_MGR", "REGIONAL_MGR", "FACTORY_MGR" }.Contains(r));
        }));

    // CanManageUsers: SYSADMIN or HO_ICT_MGR
    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireAssertion(context =>
        {
            var userRoles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            return userRoles.Contains("SYSADMIN") || userRoles.Contains("HO_ICT_MGR");
        }));

    // TenantAccess: Custom requirement to check UserTenantAccess table
    options.AddPolicy("TenantAccess", policy =>
        policy.Requirements.Add(new TenantAccessRequirement()));
});

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, TenantAccessHandler>();
```

---

## <a name="authentication"></a>Authentication Implementation

### Login Flow

**UI Mockup - Login Page:**
```
┌─────────────────────────────────────────────────────────────┐
│                  KTDA ICT Reporting System                  │
│                        Login                                 │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Email Address *                                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ username@ktda.com                                     │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  Password *                                                 │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ ••••••••                                              │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  ☐ Remember Me                                              │
│                                                             │
│  [        Login        ]                                    │
│                                                             │
│  Forgot your password? [Reset Password]                    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Login Process:**
```
1. User enters email and password
   ↓
2. Submit to /Account/Login (POST)
   ↓
3. AuthenticationService.Login(email, password)
   ↓
4. SignInManager.PasswordSignInAsync()
   ↓
5. If successful:
   - Load user's roles from UserRoles table
   - Load user's tenants from UserTenantAccess table
   - Add roles and tenants to user claims
   - Create authentication cookie
   - Redirect to role-based dashboard
   ↓
6. If failed:
   - Increment AccessFailedCount
   - If count >= 5, lock account for 30 minutes
   - Display error message
```

---

### Password Reset Flow

**UI Mockup - Forgot Password:**
```
┌─────────────────────────────────────────────────────────────┐
│                    Forgot Password                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Enter your email address and we'll send you instructions  │
│  to reset your password.                                    │
│                                                             │
│  Email Address *                                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ username@ktda.com                                     │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  [  Send Reset Link  ]    [Cancel]                         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Password Reset Process:**
```
1. User enters email on "Forgot Password" page
   ↓
2. System generates password reset token (valid for 24 hours)
   ↓
3. Send email with reset link:
   https://app.ktda.com/Account/ResetPassword?token={token}&email={email}
   ↓
4. User clicks link → Reset Password page
   ↓
5. User enters new password (twice for confirmation)
   ↓
6. System validates token and updates password
   ↓
7. Password reset successful → Redirect to login
```

**UI Mockup - Reset Password:**
```
┌─────────────────────────────────────────────────────────────┐
│                    Reset Password                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Email: elizabeth.ndegwa@ktda.com                           │
│                                                             │
│  New Password *                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ ••••••••                                              │  │
│  └──────────────────────────────────────────────────────┘  │
│  Password must contain:                                     │
│  ☑ At least 8 characters                                   │
│  ☑ One uppercase letter                                    │
│  ☑ One lowercase letter                                    │
│  ☑ One number                                              │
│  ☐ One special character                                   │
│                                                             │
│  Confirm Password *                                         │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ ••••••••                                              │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  [  Reset Password  ]    [Cancel]                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## <a name="authorization"></a>Authorization Implementation

### Claims Transformation

**Add user's roles and tenants to claims on login:**

```csharp
public class ClaimsTransformation : IClaimsTransformation
{
    private readonly IUserRoleService _userRoleService;
    private readonly IUserTenantAccessService _tenantAccessService;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId != null)
        {
            // Add user roles as claims
            var userRoles = await _userRoleService.GetUserRoles(userId);
            foreach (var role in userRoles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role.RoleCode));
            }

            // Add accessible tenant IDs as claims
            var userTenants = await _tenantAccessService.GetUserTenants(userId);
            foreach (var tenant in userTenants)
            {
                identity.AddClaim(new Claim("TenantAccess", tenant.TenantId.ToString()));
            }

            // Add primary tenant as claim
            var primaryTenant = userTenants.FirstOrDefault(t => t.IsPrimaryTenant);
            if (primaryTenant != null)
            {
                identity.AddClaim(new Claim("PrimaryTenant", primaryTenant.TenantId.ToString()));
            }
        }

        return principal;
    }
}
```

---

### Tenant Access Authorization Handler

**Check if user has access to requested tenant:**

```csharp
public class TenantAccessHandler : AuthorizationHandler<TenantAccessRequirement, int>
{
    private readonly IUserTenantAccessService _tenantAccessService;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantAccessRequirement requirement,
        int tenantId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            context.Fail();
            return;
        }

        // Check if user has access to this tenant
        var hasAccess = await _tenantAccessService.CanAccessTenant(userId, tenantId);

        if (hasAccess)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
```

**Usage in Controllers:**
```csharp
[Authorize(Policy = "TenantAccess")]
public async Task<IActionResult> ViewTenantData(int tenantId)
{
    // User's access to tenantId already verified by policy
    var data = await _service.GetDataForTenant(tenantId);
    return View(data);
}
```

---

## <a name="ui-components"></a>UI Components

### Component 1: User List with DataTables

**Page:** `/Admin/Users/Index`

**UI Mockup:**
```
┌─────────────────────────────────────────────────────────────────────────────────┐
│ User Management                                             [+ Create New User] │
├─────────────────────────────────────────────────────────────────────────────────┤
│ Search: [         ]  Filter by Status: [All ▼]  Filter by Role: [All ▼]        │
├──────────────┬────────────┬─────────────┬────────────┬────────────┬────────┬───┤
│ Email        │ Name       │ Employee #  │ Role       │ Tenant     │ Status │   │
├──────────────┼────────────┼─────────────┼────────────┼────────────┼────────┼───┤
│ admin@...    │ System     │ EMP001      │ SYSADMIN   │ Head Office│ Active │[E]│
│              │ Admin      │             │            │            │        │   │
│ elizabeth... │ Elizabeth  │ EMP038      │ FACTORY_ICT│ Kangaita   │ Active │[E]│
│              │ Ndegwa     │             │            │            │        │   │
│ eric.        │ Eric       │ EMP203      │ REGIONAL_  │ Region 3   │ Active │[E]│
│ kinyeki@...  │ Kinyeki    │             │ MGR        │            │        │   │
│ martin.      │ Martin     │ EMP100      │ HO_ICT_MGR │ Head Office│ Active │[E]│
│ mwarangu@... │ Mwarangu   │             │            │            │        │   │
├──────────────┴────────────┴─────────────┴────────────┴────────────┴────────┴───┤
│ Showing 1 to 50 of 120 entries                    [Prev] [1] [2] [3] [Next]    │
└─────────────────────────────────────────────────────────────────────────────────┘

[E] = Edit
```

---

### Component 2: Create/Edit User Form

**UI Mockup - Create User:**
```
┌─────────────────────────────────────────────────────────────┐
│ Create New User                                       [X]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ Personal Information                                        │
│ ─────────────────────                                       │
│                                                             │
│ First Name *            Last Name *                         │
│ ┌────────────────────┐  ┌────────────────────┐            │
│ │ Elizabeth          │  │ Ndegwa             │            │
│ └────────────────────┘  └────────────────────┘            │
│                                                             │
│ Employee Number *                                           │
│ ┌──────────────────────────────────────────────────────┐  │
│ │ EMP038                                                │  │
│ └──────────────────────────────────────────────────────┘  │
│                                                             │
│ Contact Information                                         │
│ ────────────────────                                        │
│                                                             │
│ Email Address * (used as username)                          │
│ ┌──────────────────────────────────────────────────────┐  │
│ │ elizabeth.ndegwa@ktda.com                             │  │
│ └──────────────────────────────────────────────────────┘  │
│                                                             │
│ Phone Number                                                │
│ ┌──────────────────────────────────────────────────────┐  │
│ │ +254 712 345 678                                      │  │
│ └──────────────────────────────────────────────────────┘  │
│                                                             │
│ Account Settings                                            │
│ ────────────────                                            │
│                                                             │
│ Primary Role *                                              │
│ ┌──────────────────────────────────────────────────────┐  │
│ │ Factory ICT Support                           [▼]    │  │
│ └──────────────────────────────────────────────────────┘  │
│                                                             │
│ Primary Tenant *                                            │
│ ┌──────────────────────────────────────────────────────┐  │
│ │ Kangaita Tea Factory (Region 3)               [▼]    │  │
│ └──────────────────────────────────────────────────────┘  │
│                                                             │
│ Temporary Password * (user will be asked to change)        │
│ ┌──────────────────────────────────────────────────────┐  │
│ │ ••••••••••••                                          │  │
│ └──────────────────────────────────────────────────────┘  │
│                                                             │
│ ☐ Send welcome email with login instructions               │
│ ☑ Require password change on first login                   │
│                                                             │
│                    [Cancel]          [Create User]          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

### Component 3: Assign Roles Modal

**UI Mockup:**
```
┌─────────────────────────────────────────────────────────────┐
│ Assign Roles to Elizabeth Ndegwa                      [X]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ Current Roles:                                              │
│ ┌─────────────────────────────────────────────────────┐    │
│ │ ● Factory ICT Support (Primary)      [Remove]      │    │
│ └─────────────────────────────────────────────────────┘    │
│                                                             │
│ Add New Role:                                               │
│ ┌──────────────────────────────────────────────────────┐  │
│ │ Select a role...                              [▼]    │  │
│ └──────────────────────────────────────────────────────┘  │
│                                                             │
│ Available Roles:                                            │
│ ☐ System Administrator                                     │
│ ☐ Head Office ICT Manager                                  │
│ ☐ Regional ICT Manager                                     │
│ ☑ Factory ICT Support                                      │
│ ☐ Factory Manager                                          │
│ ☐ Report Viewer                                            │
│                                                             │
│ ⓘ User can have multiple roles. One role must be marked   │
│   as primary, which determines the default dashboard.      │
│                                                             │
│                    [Cancel]          [Save Changes]         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

### Component 4: Assign Tenant Access Modal

**UI Mockup:**
```
┌─────────────────────────────────────────────────────────────┐
│ Manage Tenant Access for Eric Kinyeki                [X]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ Role: Regional ICT Manager (Region 3)                      │
│                                                             │
│ Current Tenant Access: (8 factories)                       │
│ ┌─────────────────────────────────────────────────────────┐│
│ │ Tenant Name      │ Read │ Write │ Approve │ Primary │  ││
│ ├──────────────────┼──────┼───────┼─────────┼─────────┼──││
│ │ Kangaita Factory │  ☑   │  ☐    │   ☑     │   ☐     │  ││
│ │ Kathangariri     │  ☑   │  ☐    │   ☑     │   ☐     │  ││
│ │ Kimunye Factory  │  ☑   │  ☐    │   ☑     │   ☐     │  ││
│ │ Mununga Factory  │  ☑   │  ☐    │   ☑     │   ☐     │  ││
│ │ Mungania Factory │  ☑   │  ☐    │   ☑     │   ☐     │  ││
│ │ Ndima Factory    │  ☑   │  ☐    │   ☑     │   ☐     │  ││
│ │ Rukuriri Factory │  ☑   │  ☐    │   ☑     │   ☐     │  ││
│ │ Thumaita Factory │  ☑   │  ☐    │   ☑     │   ☐     │  ││
│ └──────────────────┴──────┴───────┴─────────┴─────────┴──┘│
│                                                             │
│ Add Additional Tenant Access:                               │
│ ┌──────────────────────────────────────────────────────┐  │
│ │ Select tenant...                              [▼]    │  │
│ └──────────────────────────────────────────────────────┘  │
│                                                             │
│ Permissions for selected tenant:                           │
│ ☑ Can Read    ☐ Can Write    ☐ Can Approve                │
│                                                             │
│ [Add Tenant Access]                                        │
│                                                             │
│ ⓘ Regional Managers automatically get access to all       │
│   factories in their region.                               │
│                                                             │
│                    [Cancel]          [Save Changes]         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## <a name="seed-data"></a>Seed Data Strategy

### IdentitySeedService Implementation

**Class:** `IdentitySeedService`

**Methods:**

1. **SeedRoles()**
   - Create 6 standard roles
   - Check if roles already exist before inserting
   - Log role creation

2. **SeedDefaultAdmin()**
   - Create default SYSADMIN user
   - Email: admin@ktda.com
   - Password: Admin@12345 (must change on first login)
   - Assign SYSADMIN role
   - Grant access to all tenants

3. **SeedSampleUsers()** (Development only)
   - Create sample users for testing:
     - Elizabeth Ndegwa (FACTORY_ICT, Kangaita)
     - Eric Kinyeki (REGIONAL_MGR, Region 3)
     - Martin Mwarangu (HO_ICT_MGR, Head Office)
   - Assign appropriate roles and tenant access

---

### Seed Data Execution Order

**Program.cs:**
```
1. Week 2 seed data (Regions, Tenants) - Already completed
   ↓
2. Week 3 seed data (Roles, Users)
   ↓
3. Call IdentitySeedService.SeedRoles()
   ↓
4. Call IdentitySeedService.SeedDefaultAdmin()
   ↓
5. If Development environment:
   - Call IdentitySeedService.SeedSampleUsers()
   ↓
6. Log completion
```

---

## <a name="testing"></a>Testing Strategy

### Unit Tests

**Test Class:** `AuthenticationServiceTests`

**Test Cases:**
1. `Login_ValidCredentials_Success()` - Login with valid email/password
2. `Login_InvalidPassword_ReturnsError()` - Login with wrong password
3. `Login_FiveFailedAttempts_LocksAccount()` - Account lockout after 5 attempts
4. `ResetPassword_ValidToken_Success()` - Password reset with valid token
5. `ResetPassword_ExpiredToken_Failure()` - Password reset with expired token
6. `ChangePassword_ValidOldPassword_Success()` - User changes password
7. `ChangePassword_InvalidOldPassword_Failure()` - Wrong old password

**Test Class:** `UserServiceTests`

**Test Cases:**
1. `CreateUser_ValidData_Success()` - Create user with valid data
2. `CreateUser_DuplicateEmail_ThrowsException()` - Duplicate email validation
3. `CreateUser_InvalidEmail_ThrowsException()` - Email format validation
4. `CreateUser_WeakPassword_ThrowsException()` - Password policy enforcement
5. `UpdateUser_ValidData_Success()` - Update user details
6. `DeactivateUser_Success()` - Deactivate user account
7. `ActivateUser_Success()` - Reactivate user account

**Test Class:** `UserRoleServiceTests`

**Test Cases:**
1. `AssignRole_ValidUserAndRole_Success()` - Assign role to user
2. `AssignRole_DuplicateAssignment_ThrowsException()` - Prevent duplicate role
3. `RemoveRole_Success()` - Remove role from user
4. `SetPrimaryRole_Success()` - Change primary role
5. `GetUserRoles_ReturnsAllRoles()` - Get all roles for user

**Test Class:** `UserTenantAccessServiceTests`

**Test Cases:**
1. `GrantAccess_ValidData_Success()` - Grant tenant access
2. `GrantAccess_DuplicateAccess_ThrowsException()` - Prevent duplicate access
3. `RevokeAccess_Success()` - Revoke tenant access
4. `AutoAssignTenants_FactoryICT_AssignsOneFactory()` - Auto-assign for FACTORY_ICT
5. `AutoAssignTenants_RegionalMgr_AssignsAllRegionFactories()` - Auto-assign for REGIONAL_MGR
6. `UpdatePermissions_Success()` - Update tenant permissions

---

### Integration Tests

**Test Class:** `IdentityIntegrationTests`

**Test Cases:**
1. `Login_CreatesCookie_Success()` - Login creates authentication cookie
2. `Logout_RemovesCookie_Success()` - Logout removes cookie
3. `AuthorizeAttribute_UnauthenticatedUser_RedirectsToLogin()` - Authorization redirect
4. `AuthorizationPolicy_AdminOnly_RestrictsNonAdmins()` - Policy enforcement
5. `TenantAccessPolicy_UserWithoutAccess_Denies()` - Tenant access denial
6. `TenantAccessPolicy_UserWithAccess_Allows()` - Tenant access granted

---

### Manual Testing Checklist

**Authentication:**
- [ ] Can login with valid credentials
- [ ] Cannot login with invalid password
- [ ] Account locks after 5 failed attempts
- [ ] Locked account shows appropriate message
- [ ] "Remember Me" keeps session across browser restarts
- [ ] Session expires after 8 hours
- [ ] Session timeout warning appears at 55 minutes
- [ ] Can logout successfully
- [ ] Forgot password sends email with reset link
- [ ] Reset password link works within 24 hours
- [ ] Reset password link expires after 24 hours
- [ ] Can change password from profile page
- [ ] New password must meet policy requirements

**User Management:**
- [ ] Admin can create new user
- [ ] Cannot create user with duplicate email
- [ ] Cannot create user with weak password
- [ ] Welcome email sent on user creation
- [ ] Can edit user details (name, phone, etc.)
- [ ] Cannot edit user email (identifier)
- [ ] Can activate/deactivate user
- [ ] Deactivated user cannot login
- [ ] Can reset user's password (admin only)
- [ ] User list shows all users with filters
- [ ] DataTables search/filter works correctly

**Role Assignment:**
- [ ] Can assign role to user
- [ ] User can have multiple roles
- [ ] Cannot assign same role twice
- [ ] Can set primary role
- [ ] Can remove non-primary role
- [ ] Cannot remove primary role without assigning new one
- [ ] Role assignment tracked with timestamp and assigner

**Tenant Access:**
- [ ] FACTORY_ICT auto-assigned to one factory
- [ ] REGIONAL_MGR auto-assigned to all region factories
- [ ] HO_ICT_MGR auto-assigned to all 80 tenants
- [ ] Can manually grant additional tenant access
- [ ] Can update tenant permissions (Read/Write/Approve)
- [ ] Can revoke tenant access
- [ ] Tenant access controls data visibility correctly

**Authorization:**
- [ ] SYSADMIN can access all admin pages
- [ ] HO_ICT_MGR can access admin pages except user management
- [ ] REGIONAL_MGR cannot access admin pages
- [ ] FACTORY_ICT cannot access admin pages
- [ ] "AdminOnly" policy works correctly
- [ ] "CanApproveSubmissions" policy works correctly
- [ ] "CanManageUsers" policy works correctly
- [ ] "TenantAccess" policy works correctly

---

## <a name="success-criteria"></a>Success Criteria

### Week 3 Success Criteria

**Database & Identity:**
- ✅ ASP.NET Core Identity configured
- ✅ All Identity tables created (9 tables)
- ✅ Custom tables created: Roles, UserRoles, UserTenantAccess
- ✅ ApplicationUser with custom properties
- ✅ Migrations applied successfully

**Authentication:**
- ✅ Login/logout functionality working
- ✅ Password reset via email working
- ✅ Change password functionality working
- ✅ Account lockout after 5 failed attempts
- ✅ Session management (timeout, sliding expiration)
- ✅ Cookie-based authentication

**User Management:**
- ✅ Create user functionality (admin only)
- ✅ Edit user functionality
- ✅ Activate/deactivate user
- ✅ Reset user password (admin only)
- ✅ User list with search/filter
- ✅ DataTables integration

**Role Management:**
- ✅ 6 standard roles seeded
- ✅ Assign role to user
- ✅ Remove role from user
- ✅ Set primary role
- ✅ Users can have multiple roles
- ✅ Role assignment audit trail

**Tenant Access:**
- ✅ Grant tenant access to user
- ✅ Revoke tenant access
- ✅ Update tenant permissions
- ✅ Set primary tenant
- ✅ Auto-assignment based on role
- ✅ Tenant access controls data visibility

**Authorization:**
- ✅ 4 authorization policies configured
- ✅ TenantAccessHandler implemented
- ✅ Claims transformation adds roles and tenants
- ✅ Policy enforcement working
- ✅ Role-based page access control

**Seed Data:**
- ✅ 6 standard roles created
- ✅ Default admin user created
- ✅ Sample users created (dev environment)
- ✅ Roles assigned to sample users
- ✅ Tenant access assigned to sample users

**Testing:**
- ✅ 20+ unit tests passing
- ✅ 6+ integration tests passing
- ✅ Manual testing checklist completed
- ✅ All authentication flows tested
- ✅ All authorization policies tested

---

## Week 3 Deliverables Summary

### Database Artifacts
- ✅ ASP.NET Core Identity tables (9 tables)
- ✅ Custom tables: Roles, UserRoles, UserTenantAccess
- ✅ ApplicationUser with custom columns
- ✅ Seed data: 6 roles + default admin user

### Code Artifacts
- ✅ ApplicationUser entity
- ✅ Role, UserRole, UserTenantAccess entities
- ✅ EF Core configurations for custom tables
- ✅ IAuthenticationService and implementation
- ✅ IUserService and implementation
- ✅ IRoleService and implementation
- ✅ IUserRoleService and implementation
- ✅ IUserTenantAccessService and implementation
- ✅ DTOs and validators
- ✅ AutoMapper profiles
- ✅ TenantAccessHandler
- ✅ ClaimsTransformation
- ✅ 4 authorization policies

### UI Artifacts
- ✅ 5 authentication pages (Login, Logout, ForgotPassword, ResetPassword, ChangePassword)
- ✅ 4 user management pages (Index, Create, Edit, Details)
- ✅ Assign roles modal
- ✅ Assign tenant access modal
- ✅ DataTables integration
- ✅ Client-side validation

### Documentation
- ✅ This implementation plan
- ✅ User management implementation flow (next document)
- ✅ Role management implementation flow (next document)
- ✅ User roles assignment flow (next document)
- ✅ User tenant access flow (next document)

---

## Related Documents

- **Parent Plan:** [ImplementationPlan.md](../ImplementationPlan.md)
- **Overview:** [0_Identity_Overview.md](0_Identity_Overview.md)
- **Previous Phase:** [1_Organizational_Structure/1_ImplementationPlan_OrgStructure.md](../1_Organizational_Structure/1_ImplementationPlan_OrgStructure.md)
- **Detailed Flows:**
  - [2_UserManagement_Implementation.md](2_UserManagement_Implementation.md)
  - [3_RoleManagement_Implementation.md](3_RoleManagement_Implementation.md)
  - [4_UserRoles_Implementation.md](4_UserRoles_Implementation.md)
  - [5_UserTenantAccess_Implementation.md](5_UserTenantAccess_Implementation.md)

---

**Document Version:** 1.0
**Last Updated:** October 30, 2025
**Estimated Implementation Time:** 5 days (Week 3 of Phase 1)
**Complexity:** High
**Dependencies:** Week 2 (Organizational Structure) must be completed
**Next Steps:** Proceed to Week 4 (Reference Data Management)

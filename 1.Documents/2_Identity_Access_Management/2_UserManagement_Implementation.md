# User Management - Implementation Flow

**Version:** 1.0
**Date:** October 30, 2025
**Section:** 2 - Identity & Access Management
**Component:** User Management (CRUD Operations)

---

## Table of Contents

1. [Overview](#overview)
2. [User Entity Structure](#entity-structure)
3. [Create User Flow](#create-user)
4. [Edit User Flow](#edit-user)
5. [View User Details Flow](#view-user)
6. [Activate/Deactivate User Flow](#activate-deactivate)
7. [Reset Password Flow](#reset-password)
8. [User List & Search Flow](#user-list)
9. [Service Layer Implementation](#service-layer)
10. [Validation Rules](#validation)
11. [UI Components](#ui-components)

---

## <a name="overview"></a>Overview

### Purpose

The User Management component allows administrators to create, edit, view, and manage user accounts in the KTDA ICT Reporting System.

### Key Features

- Create new user accounts
- Edit user profile information
- View user details and activity
- Activate/deactivate user accounts
- Reset user passwords (admin function)
- Search and filter users
- Assign primary tenant to users

### Access Control

**Who can manage users:**
- **SYSADMIN**: Full access to all user management functions
- **HO_ICT_MGR**: Can create/edit users but cannot manage other HO_ICT_MGR or SYSADMIN users
- **Others**: Read-only access to their own profile

### Related Components

User Management is closely integrated with:
- **Role Management**: Assign roles to users (see 3_RoleManagement_Implementation.md)
- **Tenant Access**: Assign tenant access to users (see 5_UserTenantAccess_Implementation.md)

---

## <a name="entity-structure"></a>User Entity Structure

### ApplicationUser Entity

**Database Table:** AspNetUsers (with custom columns)

**Properties:**

```
Inherited from IdentityUser:
┌────────────────────────┬──────────────┬──────────────────────────┐
│ Property               │ Type         │ Description              │
├────────────────────────┼──────────────┼──────────────────────────┤
│ Id                     │ string       │ Primary key (GUID)       │
│ UserName               │ string       │ Email (unique)           │
│ NormalizedUserName     │ string       │ Uppercase email          │
│ Email                  │ string       │ Email address            │
│ NormalizedEmail        │ string       │ Uppercase email          │
│ EmailConfirmed         │ bool         │ Email confirmed flag     │
│ PasswordHash           │ string       │ Hashed password          │
│ SecurityStamp          │ string       │ Security token           │
│ PhoneNumber            │ string       │ Phone number             │
│ PhoneNumberConfirmed   │ bool         │ Phone confirmed flag     │
│ TwoFactorEnabled       │ bool         │ 2FA enabled flag         │
│ LockoutEnd             │ DateTimeOffset? Lockout expiry time   │
│ LockoutEnabled         │ bool         │ Lockout enabled flag     │
│ AccessFailedCount      │ int          │ Failed login attempts    │
└────────────────────────┴──────────────┴──────────────────────────┘

Custom properties:
┌────────────────────────┬──────────────┬──────────────────────────┐
│ Property               │ Type         │ Description              │
├────────────────────────┼──────────────┼──────────────────────────┤
│ FirstName              │ string       │ User's first name        │
│ LastName               │ string       │ User's last name         │
│ EmployeeNumber         │ string       │ Unique employee ID       │
│ PrimaryTenantId        │ int?         │ FK to Tenants table      │
│ Status                 │ string       │ Active/Inactive/Locked   │
│ CreatedAt              │ DateTime     │ Creation timestamp       │
│ CreatedBy              │ string       │ Created by user ID       │
│ UpdatedAt              │ DateTime?    │ Last update timestamp    │
│ UpdatedBy              │ string       │ Updated by user ID       │
└────────────────────────┴──────────────┴──────────────────────────┘

Navigation properties:
┌────────────────────────┬───────────────────────────────────────────┐
│ Property               │ Description                               │
├────────────────────────┼───────────────────────────────────────────┤
│ PrimaryTenant          │ Tenant? (many-to-one)                    │
│ UserRoles              │ ICollection<UserRole> (one-to-many)      │
│ TenantAccess           │ ICollection<UserTenantAccess> (one-many) │
└────────────────────────┴───────────────────────────────────────────┘
```

### DTOs (Data Transfer Objects)

**CreateUserDto:**
```csharp
public class CreateUserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmployeeNumber { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public int PrimaryTenantId { get; set; }
    public int PrimaryRoleId { get; set; }  // Initial role assignment
    public string TemporaryPassword { get; set; }
    public bool SendWelcomeEmail { get; set; }
    public bool RequirePasswordChange { get; set; }
}
```

**UpdateUserDto:**
```csharp
public class UpdateUserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public int PrimaryTenantId { get; set; }
    // Note: Cannot change Email, EmployeeNumber after creation
}
```

**UserDto (for display):**
```csharp
public class UserDto
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string EmployeeNumber { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public int? PrimaryTenantId { get; set; }
    public string PrimaryTenantName { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; }
}
```

---

## <a name="create-user"></a>Create User Flow

### Process Overview

```
┌─────────────────────────────────────────────────────────────┐
│ Step 1: Admin navigates to /Admin/Users/Create             │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 2: Fill in user details form                          │
│ - Personal info (first name, last name, employee number)   │
│ - Contact info (email, phone)                              │
│ - Primary tenant selection                                  │
│ - Primary role selection                                    │
│ - Temporary password                                        │
│ - Options (send email, require password change)            │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 3: Client-side validation                             │
│ - Check required fields                                     │
│ - Validate email format                                     │
│ - Validate password meets policy                            │
│ - Show validation errors inline                            │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 4: Submit form (POST /Admin/Users/Create)             │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 5: Server-side validation (UserService)               │
│ - Validate DTO using FluentValidation                      │
│ - Check email uniqueness                                    │
│ - Check employee number uniqueness                          │
│ - Validate password policy                                  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 6: Create user in database                            │
│ - Call UserManager.CreateAsync(user, password)             │
│ - Set custom properties (FirstName, LastName, etc.)        │
│ - Set Status = "Active"                                    │
│ - Set CreatedAt = Now, CreatedBy = CurrentUser             │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 7: Assign primary role                                │
│ - Insert into UserRoles table                              │
│ - Set IsPrimaryRole = true                                 │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 8: Auto-assign tenant access based on role            │
│ - If FACTORY_ICT: Assign to PrimaryTenantId only           │
│ - If REGIONAL_MGR: Assign to all factories in region       │
│ - If HO_ICT_MGR: Assign to all 80 tenants                  │
│ - Set appropriate permissions (Read/Write/Approve)          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 9: Send welcome email (if option selected)            │
│ - Generate email with login instructions                   │
│ - Include temporary password                               │
│ - Include link to login page                               │
│ - Send via SMTP service                                    │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 10: Redirect to user list with success message        │
│ - Show toast notification: "User created successfully"     │
│ - Redirect to /Admin/Users                                 │
└─────────────────────────────────────────────────────────────┘
```

---

### UI Mockup - Create User Page

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Create New User                                        [Back to User List]  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Personal Information                                                    │ │
│ │ ───────────────────────                                                 │ │
│ │                                                                         │ │
│ │ First Name *                      Last Name *                           │ │
│ │ ┌────────────────────────────┐    ┌────────────────────────────────┐   │ │
│ │ │ Elizabeth                  │    │ Ndegwa                         │   │ │
│ │ └────────────────────────────┘    └────────────────────────────────┘   │ │
│ │                                                                         │ │
│ │ Employee Number *                                                       │ │
│ │ ┌─────────────────────────────────────────────────────────────────┐   │ │
│ │ │ EMP038                                                          │   │ │
│ │ └─────────────────────────────────────────────────────────────────┘   │ │
│ │ Must be unique. Format: EMP followed by numbers                        │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Contact Information                                                     │ │
│ │ ──────────────────────                                                  │ │
│ │                                                                         │ │
│ │ Email Address * (will be used as username)                              │ │
│ │ ┌─────────────────────────────────────────────────────────────────┐   │ │
│ │ │ elizabeth.ndegwa@ktda.com                                       │   │ │
│ │ └─────────────────────────────────────────────────────────────────┘   │ │
│ │ Must be unique and valid email format                                   │ │
│ │                                                                         │ │
│ │ Phone Number                                                            │ │
│ │ ┌─────────────────────────────────────────────────────────────────┐   │ │
│ │ │ +254 712 345 678                                                │   │ │
│ │ └─────────────────────────────────────────────────────────────────┘   │ │
│ │ Format: +254 XXX XXX XXX                                               │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Account Settings                                                        │ │
│ │ ───────────────                                                         │ │
│ │                                                                         │ │
│ │ Primary Role *                                                          │ │
│ │ ┌─────────────────────────────────────────────────────────────────┐   │ │
│ │ │ Factory ICT Support                                      [▼]   │   │ │
│ │ └─────────────────────────────────────────────────────────────────┘   │ │
│ │   ┌─────────────────────────────────────────────────────────────┐     │ │
│ │   │ System Administrator                                        │     │ │
│ │   │ Head Office ICT Manager                                     │     │ │
│ │   │ Regional ICT Manager                                        │     │ │
│ │   │ Factory ICT Support                                    ✓    │     │ │
│ │   │ Factory Manager                                             │     │ │
│ │   │ Report Viewer                                               │     │ │
│ │   └─────────────────────────────────────────────────────────────┘     │ │
│ │                                                                         │ │
│ │ Primary Tenant *                                                        │ │
│ │ ┌─────────────────────────────────────────────────────────────────┐   │ │
│ │ │ Kangaita Tea Factory (Region 3)                          [▼]   │   │ │
│ │ └─────────────────────────────────────────────────────────────────┘   │ │
│ │ Tenant access will be auto-assigned based on role                      │ │
│ │                                                                         │ │
│ │ Temporary Password *                                                    │ │
│ │ ┌─────────────────────────────────────────────────────────────────┐   │ │
│ │ │ ••••••••••••                                    [Show] [Generate]│   │ │
│ │ └─────────────────────────────────────────────────────────────────┘   │ │
│ │ Password must contain:                                                  │ │
│ │ ☑ At least 8 characters                                                │ │
│ │ ☑ One uppercase letter                                                 │ │
│ │ ☑ One lowercase letter                                                 │ │
│ │ ☑ One number                                                           │ │
│ │ ☑ One special character                                                │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Additional Options                                                      │ │
│ │ ─────────────────                                                       │ │
│ │                                                                         │ │
│ │ ☑ Send welcome email with login instructions                           │ │
│ │ ☑ Require password change on first login                               │ │
│ │ ☐ Enable two-factor authentication                                     │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ ⓘ After creation:                                                      │ │
│ │ • User will be able to login with email and temporary password         │ │
│ │ • Tenant access will be automatically assigned based on role           │ │
│ │ • Additional roles can be assigned from the user details page          │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│                       [Cancel]                    [Create User]             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

### Validation Rules - Create User

**Client-Side Validation (jQuery Validation):**

```javascript
$('#createUserForm').validate({
    rules: {
        firstName: {
            required: true,
            minlength: 2,
            maxlength: 100
        },
        lastName: {
            required: true,
            minlength: 2,
            maxlength: 100
        },
        employeeNumber: {
            required: true,
            pattern: /^EMP\d+$/,
            remote: {
                url: '/api/users/check-employee-number',
                type: 'post'
            }
        },
        email: {
            required: true,
            email: true,
            remote: {
                url: '/api/users/check-email',
                type: 'post'
            }
        },
        phoneNumber: {
            pattern: /^\+254\s\d{3}\s\d{3}\s\d{3}$/
        },
        primaryRoleId: {
            required: true
        },
        primaryTenantId: {
            required: true
        },
        temporaryPassword: {
            required: true,
            minlength: 8,
            passwordPolicy: true  // Custom validator
        }
    },
    messages: {
        employeeNumber: {
            pattern: 'Employee number must start with EMP followed by numbers',
            remote: 'This employee number is already in use'
        },
        email: {
            remote: 'This email is already registered'
        },
        phoneNumber: {
            pattern: 'Phone format: +254 XXX XXX XXX'
        }
    }
});

// Custom password policy validator
$.validator.addMethod('passwordPolicy', function(value) {
    return /[A-Z]/.test(value) &&  // Uppercase
           /[a-z]/.test(value) &&  // Lowercase
           /\d/.test(value) &&     // Digit
           /[@#$%^&*()_+\-=\[\]{}|;:,.<>?]/.test(value);  // Special char
}, 'Password must contain uppercase, lowercase, number, and special character');
```

**Server-Side Validation (FluentValidation):**

```csharp
public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    private readonly IUserService _userService;

    public CreateUserDtoValidator(IUserService userService)
    {
        _userService = userService;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .Length(2, 100).WithMessage("First name must be 2-100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .Length(2, 100).WithMessage("Last name must be 2-100 characters");

        RuleFor(x => x.EmployeeNumber)
            .NotEmpty().WithMessage("Employee number is required")
            .Matches(@"^EMP\d+$").WithMessage("Employee number must start with EMP")
            .MustAsync(async (empNum, cancellation) =>
            {
                return !await _userService.EmployeeNumberExistsAsync(empNum);
            }).WithMessage("Employee number already exists");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MustAsync(async (email, cancellation) =>
            {
                return !await _userService.EmailExistsAsync(email);
            }).WithMessage("Email already registered");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+254\s\d{3}\s\d{3}\s\d{3}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone format: +254 XXX XXX XXX");

        RuleFor(x => x.PrimaryRoleId)
            .GreaterThan(0).WithMessage("Primary role is required");

        RuleFor(x => x.PrimaryTenantId)
            .GreaterThan(0).WithMessage("Primary tenant is required");

        RuleFor(x => x.TemporaryPassword)
            .NotEmpty().WithMessage("Temporary password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain lowercase letter")
            .Matches(@"\d").WithMessage("Password must contain a digit")
            .Matches(@"[@#$%^&*()_+\-=\[\]{}|;:,.<>?]")
            .WithMessage("Password must contain a special character");
    }
}
```

---

### Auto-Assignment Logic Based on Role

**When creating a user, automatically assign tenant access based on their primary role:**

```csharp
public async Task AutoAssignTenantAccessAsync(string userId, int primaryRoleId, int primaryTenantId)
{
    var role = await _roleService.GetRoleByIdAsync(primaryRoleId);

    switch (role.RoleCode)
    {
        case "FACTORY_ICT":
            // Assign access to ONE factory (the primary tenant)
            await _userTenantAccessService.GrantTenantAccessAsync(new GrantAccessDto
            {
                UserId = userId,
                TenantId = primaryTenantId,
                IsPrimaryTenant = true,
                CanRead = true,
                CanWrite = true,
                CanApprove = false
            });
            break;

        case "FACTORY_MGR":
            // Assign read-only and approve access to ONE factory
            await _userTenantAccessService.GrantTenantAccessAsync(new GrantAccessDto
            {
                UserId = userId,
                TenantId = primaryTenantId,
                IsPrimaryTenant = true,
                CanRead = true,
                CanWrite = false,
                CanApprove = true
            });
            break;

        case "REGIONAL_MGR":
            // Assign access to ALL factories in the region
            var tenant = await _tenantService.GetTenantByIdAsync(primaryTenantId);
            var regionFactories = await _tenantService.GetFactoriesByRegionAsync(tenant.RegionId.Value);

            foreach (var factory in regionFactories)
            {
                await _userTenantAccessService.GrantTenantAccessAsync(new GrantAccessDto
                {
                    UserId = userId,
                    TenantId = factory.TenantId,
                    IsPrimaryTenant = factory.TenantId == primaryTenantId,
                    CanRead = true,
                    CanWrite = false,
                    CanApprove = true
                });
            }
            break;

        case "HO_ICT_MGR":
        case "SYSADMIN":
            // Assign access to ALL 80 tenants
            var allTenants = await _tenantService.GetAllTenantsAsync();

            foreach (var tenant in allTenants)
            {
                await _userTenantAccessService.GrantTenantAccessAsync(new GrantAccessDto
                {
                    UserId = userId,
                    TenantId = tenant.TenantId,
                    IsPrimaryTenant = tenant.TenantId == primaryTenantId,
                    CanRead = true,
                    CanWrite = true,
                    CanApprove = true
                });
            }
            break;

        case "VIEWER":
            // Assign read-only access to ALL 80 tenants
            var allTenantsViewer = await _tenantService.GetAllTenantsAsync();

            foreach (var tenant in allTenantsViewer)
            {
                await _userTenantAccessService.GrantTenantAccessAsync(new GrantAccessDto
                {
                    UserId = userId,
                    TenantId = tenant.TenantId,
                    IsPrimaryTenant = tenant.TenantId == primaryTenantId,
                    CanRead = true,
                    CanWrite = false,
                    CanApprove = false
                });
            }
            break;
    }
}
```

---

## <a name="edit-user"></a>Edit User Flow

### Process Overview

```
┌─────────────────────────────────────────────────────────────┐
│ Step 1: Admin navigates to /Admin/Users/Edit/{userId}      │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 2: Load user data into form                           │
│ - Fetch user by ID from database                           │
│ - Populate form fields with existing data                  │
│ - Email and EmployeeNumber fields are read-only            │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 3: Admin modifies allowed fields                      │
│ - First name, last name                                    │
│ - Phone number                                              │
│ - Primary tenant (with caution warning)                    │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 4: Client-side validation                             │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 5: Submit form (POST /Admin/Users/Edit/{userId})      │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 6: Server-side validation                             │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 7: Update user in database                            │
│ - Update allowed fields only                               │
│ - Set UpdatedAt = Now, UpdatedBy = CurrentUser             │
│ - Do NOT change: Email, EmployeeNumber, PasswordHash       │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 8: Redirect to user details with success message      │
└─────────────────────────────────────────────────────────────┘
```

---

### UI Mockup - Edit User Page

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Edit User: Elizabeth Ndegwa                        [Back to User Details]   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Personal Information                                                    │ │
│ │ ───────────────────────                                                 │ │
│ │                                                                         │ │
│ │ First Name *                      Last Name *                           │ │
│ │ ┌────────────────────────────┐    ┌────────────────────────────────┐   │ │
│ │ │ Elizabeth                  │    │ Ndegwa                         │   │ │
│ │ └────────────────────────────┘    └────────────────────────────────┘   │ │
│ │                                                                         │ │
│ │ Employee Number (cannot be changed)                                     │ │
│ │ ┌─────────────────────────────────────────────────────────────────┐   │ │
│ │ │ EMP038                                                    [🔒]   │   │ │
│ │ └─────────────────────────────────────────────────────────────────┘   │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Contact Information                                                     │ │
│ │ ──────────────────────                                                  │ │
│ │                                                                         │ │
│ │ Email Address (cannot be changed)                                       │ │
│ │ ┌─────────────────────────────────────────────────────────────────┐   │ │
│ │ │ elizabeth.ndegwa@ktda.com                                 [🔒]   │   │ │
│ │ └─────────────────────────────────────────────────────────────────┘   │ │
│ │                                                                         │ │
│ │ Phone Number                                                            │ │
│ │ ┌─────────────────────────────────────────────────────────────────┐   │ │
│ │ │ +254 712 345 678                                                │   │ │
│ │ └─────────────────────────────────────────────────────────────────┘   │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Account Settings                                                        │ │
│ │ ───────────────                                                         │ │
│ │                                                                         │ │
│ │ Primary Tenant                                                          │ │
│ │ ┌─────────────────────────────────────────────────────────────────┐   │ │
│ │ │ Kangaita Tea Factory (Region 3)                          [▼]   │   │ │
│ │ └─────────────────────────────────────────────────────────────────┘   │ │
│ │ ⚠️  Changing primary tenant will affect data visibility and access     │ │
│ │                                                                         │ │
│ │ Status: ┌────────┐                                                      │ │
│ │         │ Active │  (Green badge)                                      │ │
│ │         └────────┘                                                      │ │
│ │                                                                         │ │
│ │ Primary Role: Factory ICT Support                                       │ │
│ │ [Manage Roles] - Click to view/modify user roles                       │ │
│ │                                                                         │ │
│ │ Tenant Access: 1 tenant assigned                                        │ │
│ │ [Manage Tenant Access] - Click to view/modify tenant assignments       │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Account Information                                                     │ │
│ │ ──────────────────                                                      │ │
│ │ Created: January 15, 2025 by admin@ktda.com                            │ │
│ │ Last Updated: October 20, 2025 by martin.mwarangu@ktda.com             │ │
│ │ Last Login: October 30, 2025 at 08:45 AM                               │ │
│ │ Failed Login Attempts: 0                                                │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│                       [Cancel]                    [Save Changes]            │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

### Fields That Cannot Be Edited

**Immutable Fields:**
- **Email**: Used as username, changing would break authentication
- **EmployeeNumber**: Business identifier, should remain constant
- **PasswordHash**: Use separate "Reset Password" function
- **Status**: Use separate "Activate/Deactivate" buttons
- **Roles**: Use separate "Manage Roles" modal
- **Tenant Access**: Use separate "Manage Tenant Access" modal

**Rationale:**
- Separating these functions into dedicated workflows prevents accidental changes
- Provides better audit trail
- Enforces business rules and validation

---

## <a name="view-user"></a>View User Details Flow

### UI Mockup - User Details Page

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ User Details: Elizabeth Ndegwa                     [Edit] [Reset Password]  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ ┌────────────────────────┬────────────────────────────────────────────────┐ │
│ │  PERSONAL INFO         │  ACCOUNT STATUS                                │ │
│ ├────────────────────────┼────────────────────────────────────────────────┤ │
│ │ Full Name:             │  Status:  ┌────────┐                           │ │
│ │ Elizabeth Ndegwa       │           │ Active │  (Green)                  │ │
│ │                        │           └────────┘                           │ │
│ │ Employee Number:       │                                                │ │
│ │ EMP038                 │  Account Locked: No                            │ │
│ │                        │                                                │ │
│ │ Email:                 │  Failed Logins: 0                              │ │
│ │ elizabeth.ndegwa@...   │                                                │ │
│ │                        │  Last Login:                                   │ │
│ │ Phone:                 │  Oct 30, 2025 08:45 AM                         │ │
│ │ +254 712 345 678       │                                                │ │
│ └────────────────────────┴────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Assigned Roles (1)                                  [Manage Roles]      │ │
│ ├─────────────────────────────────────────────────────────────────────────┤ │
│ │ Role Name                │ Level    │ Primary │ Assigned Date         │ │
│ ├──────────────────────────┼──────────┼─────────┼───────────────────────┤ │
│ │ Factory ICT Support      │ Level 3  │   ●     │ Jan 15, 2025          │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Tenant Access (1)                              [Manage Tenant Access]   │ │
│ ├─────────────────────────────────────────────────────────────────────────┤ │
│ │ Tenant Name      │ Type    │ Primary │ Read │ Write │ Approve │ Date   │ │
│ ├──────────────────┼─────────┼─────────┼──────┼───────┼─────────┼────────┤ │
│ │ Kangaita Factory │ Factory │   ●     │  ✓   │   ✓   │    ✗    │ Jan 15 │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Recent Activity                                         [View All]      │ │
│ ├─────────────────────────────────────────────────────────────────────────┤ │
│ │ Date/Time           │ Action                     │ Details              │ │
│ ├─────────────────────┼────────────────────────────┼──────────────────────┤ │
│ │ Oct 30, 08:45 AM    │ Login                      │ Success              │ │
│ │ Oct 29, 04:30 PM    │ Submitted Checklist        │ Monthly Report       │ │
│ │ Oct 29, 02:15 PM    │ Updated Hardware           │ Server RAM Upgrade   │ │
│ │ Oct 29, 09:00 AM    │ Login                      │ Success              │ │
│ │ Oct 28, 03:45 PM    │ Created Ticket             │ Network Issue        │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Audit Trail                                                             │ │
│ ├─────────────────────────────────────────────────────────────────────────┤ │
│ │ Created: January 15, 2025 by admin@ktda.com                            │ │
│ │ Last Modified: October 20, 2025 by martin.mwarangu@ktda.com            │ │
│ │ Password Last Changed: September 1, 2025                                │ │
│ │ Password Expires: November 30, 2025 (31 days remaining)                │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Actions                                                                 │ │
│ ├─────────────────────────────────────────────────────────────────────────┤ │
│ │ [Edit User Info]  [Reset Password]  [Deactivate Account]  [View Logs]  │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│                              [Back to User List]                            │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## <a name="activate-deactivate"></a>Activate/Deactivate User Flow

### Deactivate User Process

```
┌─────────────────────────────────────────────────────────────┐
│ Step 1: Admin clicks "Deactivate" button on user details   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 2: Confirmation modal appears                         │
│ "Are you sure you want to deactivate this user?"           │
│ "User will not be able to login until reactivated"         │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 3: Admin confirms deactivation                        │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 4: POST /Admin/Users/Deactivate/{userId}              │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 5: Update user status in database                     │
│ - Set Status = "Inactive"                                  │
│ - Set LockoutEnd = DateTime.MaxValue (prevent login)       │
│ - Set UpdatedAt = Now, UpdatedBy = CurrentUser             │
│ - Log action in audit trail                                │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 6: If user is currently logged in                     │
│ - Invalidate all active sessions                           │
│ - User will be logged out immediately                      │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 7: Refresh page with success message                  │
│ - Status badge changes to "Inactive" (gray)                │
│ - "Deactivate" button changes to "Activate" button         │
└─────────────────────────────────────────────────────────────┘
```

---

### UI Mockup - Deactivation Confirmation Modal

```
┌─────────────────────────────────────────────────────────────┐
│ ⚠️  Confirm Deactivation                              [X]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ Are you sure you want to deactivate this user account?     │
│                                                             │
│ User: Elizabeth Ndegwa (elizabeth.ndegwa@ktda.com)          │
│ Employee Number: EMP038                                     │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ Effects of deactivation:                                │ │
│ │                                                         │ │
│ │ • User will not be able to login                       │ │
│ │ • All active sessions will be terminated immediately   │ │
│ │ • User's data and history will be preserved            │ │
│ │ • Tenant access will remain but be inaccessible        │ │
│ │ • Account can be reactivated at any time               │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ Reason for deactivation (optional):                        │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │                                                         │ │
│ │                                                         │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│                    [Cancel]          [Confirm Deactivation] │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

### Activate User Process

**Simpler process, mirror of deactivation:**

```
1. Admin clicks "Activate" button
   ↓
2. Confirmation modal (simpler than deactivation)
   ↓
3. Update user status:
   - Set Status = "Active"
   - Set LockoutEnd = null (allow login)
   - Clear AccessFailedCount
   ↓
4. User can now login normally
```

---

## <a name="reset-password"></a>Reset Password Flow (Admin Function)

### Process Overview

```
┌─────────────────────────────────────────────────────────────┐
│ Step 1: Admin clicks "Reset Password" on user details page │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 2: Reset Password modal appears                       │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 3: Admin chooses reset method:                        │
│ Option A: Generate temporary password                      │
│ Option B: Send password reset link via email               │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 4: If Option A (Generate temporary password):         │
│ - Generate random password meeting policy                  │
│ - Update user's password in database                       │
│ - Force password change on next login                      │
│ - Display temporary password to admin (copy button)        │
│ - Admin manually sends password to user                    │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 5: If Option B (Email reset link):                    │
│ - Generate password reset token (24-hour expiry)           │
│ - Send email to user with reset link                       │
│ - User clicks link and sets new password                   │
└─────────────────────────────────────────────────────────────┘
```

---

### UI Mockup - Reset Password Modal

```
┌─────────────────────────────────────────────────────────────┐
│ Reset Password for Elizabeth Ndegwa                   [X]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ Choose password reset method:                               │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ ⦿ Generate temporary password                           │ │
│ │   Admin generates password and shares it with user      │ │
│ │                                                         │ │
│ │   [Generate Password]                                   │ │
│ │                                                         │ │
│ │   Temporary Password:                                   │ │
│ │   ┌────────────────────────────────────┬──────────────┐ │ │
│ │   │ Ktda#2025Temp!                     │  [Copy]      │ │ │
│ │   └────────────────────────────────────┴──────────────┘ │ │
│ │                                                         │ │
│ │   ☑ User must change password on first login           │ │
│ │   ☐ Send temporary password to user via email          │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ ○ Send password reset link via email                   │ │
│ │   User receives email and sets own password             │ │
│ │                                                         │ │
│ │   Reset link will be sent to:                          │ │
│ │   elizabeth.ndegwa@ktda.com                             │ │
│ │                                                         │ │
│ │   ⓘ Link expires in 24 hours                           │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ⚠️  User's current password will be invalidated           │
│                                                             │
│                    [Cancel]          [Reset Password]       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## <a name="user-list"></a>User List & Search Flow

### UI Mockup - User List Page

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│ User Management                                                [+ Create New User]  │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│ ┌──────────────────────────────────────────────────────────────────────────────┐   │
│ │ Search: [                    ]  🔍                                           │   │
│ │                                                                              │   │
│ │ Filter by Role:   [All Roles ▼]                                             │   │
│ │ Filter by Status: [All Status ▼]                                            │   │
│ │ Filter by Tenant: [All Tenants ▼]                                           │   │
│ │                                                                              │   │
│ │ [Export to Excel]  [Export to PDF]                                          │   │
│ └──────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                     │
│ ┌──────────────┬─────────────┬──────────────┬─────────────┬───────────┬────────┬──┤
│ │ Employee #   │ Name        │ Email        │ Role        │ Tenant    │ Status │  │
│ ├──────────────┼─────────────┼──────────────┼─────────────┼───────────┼────────┼──┤
│ │ EMP001       │ System      │ admin@...    │ SYSADMIN    │ Head      │ Active │✓ │
│ │              │ Admin       │              │             │ Office    │        │  │
│ ├──────────────┼─────────────┼──────────────┼─────────────┼───────────┼────────┼──┤
│ │ EMP038       │ Elizabeth   │ elizabeth... │ FACTORY_ICT │ Kangaita  │ Active │✓ │
│ │              │ Ndegwa      │              │             │           │        │  │
│ ├──────────────┼─────────────┼──────────────┼─────────────┼───────────┼────────┼──┤
│ │ EMP100       │ Martin      │ martin...    │ HO_ICT_MGR  │ Head      │ Active │✓ │
│ │              │ Mwarangu    │              │             │ Office    │        │  │
│ ├──────────────┼─────────────┼──────────────┼─────────────┼───────────┼────────┼──┤
│ │ EMP203       │ Eric        │ eric...      │ REGIONAL_   │ Region 3  │ Active │✓ │
│ │              │ Kinyeki     │              │ MGR         │           │        │  │
│ ├──────────────┼─────────────┼──────────────┼─────────────┼───────────┼────────┼──┤
│ │ EMP205       │ Peter       │ peter...     │ REGIONAL_   │ Region 1  │ Inactive│✓│
│ │              │ Kibe        │              │ MGR         │           │        │  │
│ └──────────────┴─────────────┴──────────────┴─────────────┴───────────┴────────┴──┘
│                                                                                     │
│ Showing 1 to 50 of 120 users            [◀ Previous] [1] [2] [3] [Next ▶]         │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘

✓ = Actions menu (Edit | Details | Reset Password | Deactivate)
```

---

### DataTables Configuration

**JavaScript implementation:**

```javascript
$('#usersTable').DataTable({
    processing: true,
    serverSide: true,
    ajax: {
        url: '/api/users/datatable',
        type: 'POST',
        data: function(d) {
            d.roleFilter = $('#roleFilter').val();
            d.statusFilter = $('#statusFilter').val();
            d.tenantFilter = $('#tenantFilter').val();
        }
    },
    columns: [
        { data: 'employeeNumber', title: 'Employee #' },
        { data: 'fullName', title: 'Name' },
        { data: 'email', title: 'Email' },
        {
            data: 'primaryRole',
            title: 'Role',
            render: function(data) {
                return `<span class="badge badge-info">${data}</span>`;
            }
        },
        { data: 'primaryTenantName', title: 'Tenant' },
        {
            data: 'status',
            title: 'Status',
            render: function(data) {
                const badgeClass = data === 'Active' ? 'badge-success' :
                                   data === 'Inactive' ? 'badge-secondary' :
                                   'badge-danger';
                return `<span class="badge ${badgeClass}">${data}</span>`;
            }
        },
        {
            data: 'id',
            title: 'Actions',
            orderable: false,
            render: function(data, type, row) {
                return `
                    <div class="btn-group">
                        <button class="btn btn-sm btn-primary" onclick="viewUser('${data}')">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-sm btn-warning" onclick="editUser('${data}')">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-sm btn-info" onclick="resetPassword('${data}')">
                            <i class="fas fa-key"></i>
                        </button>
                        ${row.status === 'Active' ?
                            `<button class="btn btn-sm btn-danger" onclick="deactivateUser('${data}')">
                                <i class="fas fa-ban"></i>
                            </button>` :
                            `<button class="btn btn-sm btn-success" onclick="activateUser('${data}')">
                                <i class="fas fa-check"></i>
                            </button>`
                        }
                    </div>
                `;
            }
        }
    ],
    order: [[1, 'asc']],  // Sort by name
    pageLength: 50,
    lengthMenu: [[25, 50, 100, -1], [25, 50, 100, "All"]],
    dom: 'Bfrtip',
    buttons: [
        'excelHtml5',
        'pdfHtml5',
        'print'
    ]
});

// Reload table when filters change
$('#roleFilter, #statusFilter, #tenantFilter').on('change', function() {
    $('#usersTable').DataTable().ajax.reload();
});
```

---

## <a name="service-layer"></a>Service Layer Implementation

### IUserService Interface

```csharp
public interface IUserService
{
    // Create
    Task<ResultDto<string>> CreateUserAsync(CreateUserDto dto);

    // Read
    Task<UserDto> GetUserByIdAsync(string userId);
    Task<UserDto> GetUserByEmailAsync(string email);
    Task<UserDto> GetUserByEmployeeNumberAsync(string employeeNumber);
    Task<PagedResultDto<UserDto>> GetUsersAsync(UserFilterDto filter);

    // Update
    Task<ResultDto> UpdateUserAsync(string userId, UpdateUserDto dto);
    Task<ResultDto> ActivateUserAsync(string userId);
    Task<ResultDto> DeactivateUserAsync(string userId, string reason);

    // Password
    Task<ResultDto> ResetUserPasswordAsync(string userId, string newPassword);
    Task<ResultDto<string>> GenerateTemporaryPasswordAsync();
    Task<ResultDto> SendPasswordResetEmailAsync(string userId);

    // Validation
    Task<bool> EmailExistsAsync(string email);
    Task<bool> EmployeeNumberExistsAsync(string employeeNumber);

    // Statistics
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveUsersCountAsync();
    Task<Dictionary<string, int>> GetUsersByRoleCountAsync();
}
```

---

### UserService Implementation (Key Methods)

**Create User:**

```csharp
public async Task<ResultDto<string>> CreateUserAsync(CreateUserDto dto)
{
    // Validate DTO
    var validator = new CreateUserDtoValidator(_userService);
    var validationResult = await validator.ValidateAsync(dto);

    if (!validationResult.IsValid)
    {
        return ResultDto<string>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
    }

    try
    {
        // Create ApplicationUser
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmployeeNumber = dto.EmployeeNumber,
            PhoneNumber = dto.PhoneNumber,
            PrimaryTenantId = dto.PrimaryTenantId,
            Status = "Active",
            EmailConfirmed = false,  // Will confirm via email
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId
        };

        // Create user with password
        var result = await _userManager.CreateAsync(user, dto.TemporaryPassword);

        if (!result.Succeeded)
        {
            return ResultDto<string>.Failure(result.Errors.Select(e => e.Description).ToList());
        }

        // Assign primary role
        await _userRoleService.AssignRoleToUserAsync(user.Id, dto.PrimaryRoleId, isPrimary: true);

        // Auto-assign tenant access based on role
        await AutoAssignTenantAccessAsync(user.Id, dto.PrimaryRoleId, dto.PrimaryTenantId);

        // Require password change on first login
        if (dto.RequirePasswordChange)
        {
            await _userManager.AddClaimAsync(user, new Claim("RequirePasswordChange", "true"));
        }

        // Send welcome email
        if (dto.SendWelcomeEmail)
        {
            await SendWelcomeEmailAsync(user, dto.TemporaryPassword);
        }

        // Log action
        await _auditService.LogAsync("User Created", $"User {user.Email} created by {_currentUserService.Email}");

        return ResultDto<string>.Success(user.Id, "User created successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating user");
        return ResultDto<string>.Failure("An error occurred while creating the user");
    }
}
```

**Update User:**

```csharp
public async Task<ResultDto> UpdateUserAsync(string userId, UpdateUserDto dto)
{
    var user = await _userManager.FindByIdAsync(userId);

    if (user == null)
    {
        return ResultDto.Failure("User not found");
    }

    // Update allowed fields only
    user.FirstName = dto.FirstName;
    user.LastName = dto.LastName;
    user.PhoneNumber = dto.PhoneNumber;
    user.PrimaryTenantId = dto.PrimaryTenantId;
    user.UpdatedAt = DateTime.UtcNow;
    user.UpdatedBy = _currentUserService.UserId;

    var result = await _userManager.UpdateAsync(user);

    if (!result.Succeeded)
    {
        return ResultDto.Failure(result.Errors.Select(e => e.Description).ToList());
    }

    // Log action
    await _auditService.LogAsync("User Updated", $"User {user.Email} updated by {_currentUserService.Email}");

    return ResultDto.Success("User updated successfully");
}
```

**Deactivate User:**

```csharp
public async Task<ResultDto> DeactivateUserAsync(string userId, string reason)
{
    var user = await _userManager.FindByIdAsync(userId);

    if (user == null)
    {
        return ResultDto.Failure("User not found");
    }

    // Update status
    user.Status = "Inactive";
    user.LockoutEnd = DateTimeOffset.MaxValue;  // Prevent login
    user.UpdatedAt = DateTime.UtcNow;
    user.UpdatedBy = _currentUserService.UserId;

    var result = await _userManager.UpdateAsync(user);

    if (!result.Succeeded)
    {
        return ResultDto.Failure(result.Errors.Select(e => e.Description).ToList());
    }

    // Invalidate all active sessions
    await _userManager.UpdateSecurityStampAsync(user);

    // Log action
    await _auditService.LogAsync("User Deactivated",
        $"User {user.Email} deactivated by {_currentUserService.Email}. Reason: {reason}");

    return ResultDto.Success("User deactivated successfully");
}
```

---

## <a name="validation"></a>Validation Rules Summary

### Field Validation Rules

| Field | Required | Min Length | Max Length | Pattern/Format | Uniqueness |
|-------|----------|-----------|-----------|----------------|------------|
| FirstName | Yes | 2 | 100 | Alpha + spaces | No |
| LastName | Yes | 2 | 100 | Alpha + spaces | No |
| EmployeeNumber | Yes | 3 | 20 | EMP + digits | Yes |
| Email | Yes | - | 100 | Valid email | Yes |
| PhoneNumber | No | - | 20 | +254 XXX XXX XXX | No |
| PrimaryTenantId | Yes | - | - | Valid tenant ID | No |
| PrimaryRoleId | Yes | - | - | Valid role ID | No |
| TemporaryPassword | Yes | 8 | 100 | Password policy | No |

---

## <a name="ui-components"></a>UI Components Summary

### Component List

1. **User List Table**: DataTables with search, filter, pagination, export
2. **Create User Form**: Multi-section form with validation
3. **Edit User Form**: Similar to create but with read-only fields
4. **User Details View**: Read-only display with action buttons
5. **Activate/Deactivate Modal**: Confirmation dialog
6. **Reset Password Modal**: Choose reset method
7. **Assign Roles Modal**: (See 4_UserRoles_Implementation.md)
8. **Assign Tenant Access Modal**: (See 5_UserTenantAccess_Implementation.md)
9. **Status Badge**: Visual indicator (Green/Gray/Red)
10. **Action Dropdown Menu**: Context menu for user actions

---

## Related Documents

- **Parent Plan:** [1_ImplementationPlan_Identity.md](1_ImplementationPlan_Identity.md)
- **Next Flow:** [3_RoleManagement_Implementation.md](3_RoleManagement_Implementation.md)
- **Related Flow:** [4_UserRoles_Implementation.md](4_UserRoles_Implementation.md)
- **Related Flow:** [5_UserTenantAccess_Implementation.md](5_UserTenantAccess_Implementation.md)

---

**Document Version:** 1.0
**Last Updated:** October 30, 2025
**Implementation Day:** Day 2 (User Management Backend) + Day 5 (UI)
**Complexity:** Medium-High

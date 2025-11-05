# Identity & Access Management - Overview

**Version:** 1.0
**Date:** October 30, 2025
**Phase:** Phase 1 - Week 3
**Section:** 2 - Identity & Access Management

---

## Table of Contents

1. [Purpose](#purpose)
2. [Core Concepts](#core-concepts)
3. [System Roles](#system-roles)
4. [Access Control Model](#access-control-model)
5. [Key Components](#key-components)
6. [Security Architecture](#security-architecture)

---

## <a name="purpose"></a>Purpose

The Identity & Access Management (IAM) system provides secure authentication and authorization for the KTDA ICT Reporting System. It manages:

- **User Authentication**: Login/logout, password management
- **User Management**: Create, edit, activate/deactivate users
- **Role Management**: Define system roles with permissions
- **User Role Assignment**: Assign roles to users
- **Tenant Access Control**: Control which tenants users can access

---

## <a name="core-concepts"></a>Core Concepts

### Authentication vs. Authorization

**Authentication** (Week 3)
- Verifies WHO the user is
- Handled by ASP.NET Core Identity
- Includes login, logout, password reset
- Enforces password policies
- Implements account lockout

**Authorization** (Week 3)
- Determines WHAT the user can do
- Role-Based Access Control (RBAC)
- Policy-based authorization
- Tenant-level data access control

### Two-Layer Security Model

```
┌─────────────────────────────────────────────────────────────┐
│ Layer 1: Role-Based Access (What can user do?)             │
│ - Defines functional permissions                            │
│ - Managed via Roles and UserRoles tables                   │
│ - Examples: Can create templates? Can approve submissions?  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ Layer 2: Tenant-Based Access (What data can user see?)     │
│ - Defines data visibility scope                             │
│ - Managed via UserTenantAccess table                       │
│ - Examples: Which factories? Which regions?                 │
└─────────────────────────────────────────────────────────────┘
```

**Example:**
- **Eric Kinyeki** has role: **REGIONAL_MGR** (Layer 1)
  - Can approve checklist submissions (functional permission)
- **Eric Kinyeki** has tenant access: **Region 3 factories** (Layer 2)
  - Can only see data from his 8 factories (data permission)

---

## <a name="system-roles"></a>System Roles

The system supports **6 standard roles** with hierarchical access levels:

### Role Hierarchy

```
Level 1 (Head Office Level)
┌────────────────────────────────────────────────────────────────┐
│ SYSADMIN           - Full system access (all CRUD operations) │
│ HO_ICT_MGR         - Head Office ICT Manager                  │
│ VIEWER             - Read-only access to all reports          │
└────────────────────────────────────────────────────────────────┘
                            ↓
Level 2 (Regional Level)
┌────────────────────────────────────────────────────────────────┐
│ REGIONAL_MGR       - Regional ICT Manager (Region access)     │
└────────────────────────────────────────────────────────────────┘
                            ↓
Level 3 (Factory/Tenant Level)
┌────────────────────────────────────────────────────────────────┐
│ FACTORY_ICT        - Factory ICT Support (Single factory)     │
│ FACTORY_MGR        - Factory Manager (Read-only)              │
└────────────────────────────────────────────────────────────────┘
```

### Role Permissions Matrix

| Permission                | SYSADMIN | HO_ICT_MGR | REGIONAL_MGR | FACTORY_ICT | FACTORY_MGR | VIEWER |
|---------------------------|----------|------------|--------------|-------------|-------------|--------|
| **Data Visibility**       | All      | All        | Region Only  | Factory Only| Factory Only| All    |
| **Create/Edit Templates** | ✅       | ✅         | ❌           | ❌          | ❌          | ❌     |
| **Submit Checklists**     | ✅       | ✅         | ✅           | ✅          | ❌          | ❌     |
| **Approve Submissions**   | ✅       | ✅         | ✅           | ❌          | ✅          | ❌     |
| **Manage Users**          | ✅       | ✅         | ❌           | ❌          | ❌          | ❌     |
| **Manage Hardware**       | ✅       | ✅         | ✅           | ✅          | ❌          | ❌     |
| **Create Tickets**        | ✅       | ✅         | ✅           | ✅          | ✅          | ❌     |
| **View Reports**          | ✅       | ✅         | ✅           | ✅          | ✅          | ✅     |

---

## <a name="access-control-model"></a>Access Control Model

### Tenant Access Patterns

**Pattern 1: Single Factory Access (FACTORY_ICT)**
```
User: Elizabeth Ndegwa
Role: FACTORY_ICT
Tenant Access: Kangaita Factory (TenantId = 38) ONLY

Result: Can ONLY see/edit data for Kangaita Factory
```

**Pattern 2: Regional Access (REGIONAL_MGR)**
```
User: Eric Kinyeki
Role: REGIONAL_MGR
Tenant Access: All 8 factories in Region 3

Factories:
- Kangaita (38)
- Kathangariri (39)
- Kimunye (40)
- Mununga (41)
- Mungania (42)
- Ndima (43)
- Rukuriri (44)
- Thumaita (45)

Result: Can see/approve submissions from all 8 factories in Region 3
```

**Pattern 3: Global Access (SYSADMIN, HO_ICT_MGR)**
```
User: Martin Mwarangu
Role: HO_ICT_MGR
Tenant Access: ALL 80 tenants (71 factories + 9 subsidiaries + Head Office)

Result: Can see all data across entire organization
```

---

## <a name="key-components"></a>Key Components

### 1. Authentication Components

**ASP.NET Core Identity**
- User accounts and password management
- Password hashing (PBKDF2)
- Account lockout after 5 failed attempts
- Password expiry (90 days)
- Two-factor authentication support (optional)

**Login Flow**
```
User enters credentials
   ↓
Identity validates credentials
   ↓
If valid → Generate authentication cookie
   ↓
Load user's roles from UserRoles table
   ↓
Load user's tenants from UserTenantAccess table
   ↓
Store in user claims for session
   ↓
Redirect to role-based dashboard
```

---

### 2. User Management Components

**User CRUD Operations**
- Create user (admin only)
- Edit user profile
- Activate/deactivate user
- Reset user password
- View user activity log

**User Properties**
- Username (email format)
- Email address
- First name, last name
- Employee number
- Phone number
- Status (Active, Inactive, Locked)
- Primary tenant assignment

---

### 3. Role Management Components

**Role CRUD Operations**
- Create custom role (SYSADMIN only)
- Edit role description
- Activate/deactivate role
- View role assignments

**Standard Roles** (Pre-seeded)
- 6 system roles created during seed data
- Cannot delete standard roles
- Can modify descriptions only

---

### 4. User Role Assignment

**Assigning Roles to Users**
- Users can have multiple roles
- Primary role determines default dashboard
- Role assignments tracked with audit trail
- Can revoke role assignments

**Assignment Rules**
- SYSADMIN can assign any role
- HO_ICT_MGR can assign roles except SYSADMIN
- Regional and factory users cannot assign roles

---

### 5. User Tenant Access

**Assigning Tenant Access**
- Defines which tenants user can access
- Permission levels:
  - **IsPrimaryTenant**: User's main tenant
  - **CanRead**: View data
  - **CanWrite**: Create/edit data
  - **CanApprove**: Approve submissions

**Access Levels**
```
Read-Only Access (CanRead = true, CanWrite = false)
  → User can view data but cannot modify

Full Access (CanRead = true, CanWrite = true)
  → User can view and modify data

Approval Access (CanRead = true, CanApprove = true)
  → User can approve submissions
```

**Auto-Assignment Rules**
- **FACTORY_ICT**: Automatically get access to their factory (IsPrimaryTenant = true)
- **REGIONAL_MGR**: Automatically get access to all factories in their region
- **SYSADMIN/HO_ICT_MGR**: Automatically get access to all 80 tenants
- **VIEWER**: Get read-only access to all tenants

---

## <a name="security-architecture"></a>Security Architecture

### Password Policy

**Requirements:**
- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 digit
- At least 1 special character (@, #, $, etc.)

**Password Management:**
- Passwords hashed using PBKDF2 (ASP.NET Core Identity default)
- Password expiry: 90 days
- Cannot reuse last 5 passwords
- Password reset link expires in 24 hours

### Account Lockout

**Policy:**
- Lock account after 5 failed login attempts
- Lockout duration: 30 minutes
- Admin can manually unlock accounts
- Audit log tracks all failed login attempts

### Session Management

**Settings:**
- Session timeout: 60 minutes of inactivity
- Absolute timeout: 8 hours (requires re-login)
- Single session per user (optional)
- Automatic logout on browser close

### Authorization Policies

**Pre-defined Policies:**

1. **AdminOnly** - Requires SYSADMIN or HO_ICT_MGR role
2. **CanApproveSubmissions** - Requires HO_ICT_MGR, REGIONAL_MGR, or FACTORY_MGR role
3. **CanManageUsers** - Requires SYSADMIN or HO_ICT_MGR role
4. **TenantAccess** - Custom requirement to check UserTenantAccess table

**Usage in Code:**
```
Apply [Authorize(Policy = "AdminOnly")] attribute to controllers/pages
```

---

## Database Tables

### Users Table (ASP.NET Core Identity)

**Key Fields:**
- UserId (string, primary key)
- UserName (email format)
- Email
- PasswordHash
- SecurityStamp
- PhoneNumber
- TwoFactorEnabled
- LockoutEnabled
- LockoutEnd
- AccessFailedCount

**Additional Fields (Custom):**
- FirstName
- LastName
- EmployeeNumber
- Status (Active, Inactive, Locked)
- PrimaryTenantId (FK to Tenants)
- CreatedAt
- CreatedBy
- UpdatedAt
- UpdatedBy

---

### Roles Table

**Schema:**
```sql
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(100) NOT NULL,
    RoleCode NVARCHAR(20) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    Level INT NOT NULL,  -- 1=HeadOffice, 2=Regional, 3=Factory
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

**6 Standard Roles:**
1. SYSADMIN - System Administrator (Level 1)
2. HO_ICT_MGR - Head Office ICT Manager (Level 1)
3. REGIONAL_MGR - Regional ICT Manager (Level 2)
4. FACTORY_ICT - Factory ICT Support (Level 3)
5. FACTORY_MGR - Factory Manager (Level 3)
6. VIEWER - Report Viewer (Level 1)

---

### UserRoles Table (Many-to-Many)

**Schema:**
```sql
CREATE TABLE UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(450) NOT NULL,
    RoleId INT NOT NULL,
    IsPrimaryRole BIT NOT NULL DEFAULT 0,
    AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AssignedBy NVARCHAR(100),

    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId)
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId)
        REFERENCES Roles(RoleId) ON DELETE CASCADE,
    CONSTRAINT UQ_UserRoles_User_Role UNIQUE (UserId, RoleId)
);
```

---

### UserTenantAccess Table

**Schema:**
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
```

---

## Real-World Examples

### Example 1: Field Systems Administrator

**User:** Elizabeth Ndegwa
**Factory:** Kangaita Tea Factory (Region 3)
**Role:** FACTORY_ICT

**Setup:**
```sql
-- User created in AspNetUsers table
-- Role assignment
INSERT INTO UserRoles (UserId, RoleId, IsPrimaryRole)
VALUES ('elizabeth.ndegwa@ktda.com', 4, 1);  -- FACTORY_ICT

-- Tenant access (only Kangaita)
INSERT INTO UserTenantAccess (UserId, TenantId, IsPrimaryTenant, CanRead, CanWrite)
VALUES ('elizabeth.ndegwa@ktda.com', 38, 1, 1, 1);
```

**Access:**
- Can submit checklists for Kangaita Factory
- Can manage hardware/software for Kangaita
- Can create tickets
- **Cannot** see data from other factories
- **Cannot** approve submissions

---

### Example 2: Regional ICT Manager

**User:** Eric Kinyeki
**Region:** Region 3 (Kirinyaga & Embu)
**Role:** REGIONAL_MGR

**Setup:**
```sql
-- Role assignment
INSERT INTO UserRoles (UserId, RoleId, IsPrimaryRole)
VALUES ('eric.kinyeki@ktda.com', 3, 1);  -- REGIONAL_MGR

-- Tenant access (all 8 factories in Region 3)
INSERT INTO UserTenantAccess (UserId, TenantId, IsPrimaryTenant, CanRead, CanApprove)
VALUES
('eric.kinyeki@ktda.com', 38, 0, 1, 1),  -- Kangaita
('eric.kinyeki@ktda.com', 39, 0, 1, 1),  -- Kathangariri
('eric.kinyeki@ktda.com', 40, 0, 1, 1),  -- Kimunye
('eric.kinyeki@ktda.com', 41, 0, 1, 1),  -- Mununga
('eric.kinyeki@ktda.com', 42, 0, 1, 1),  -- Mungania
('eric.kinyeki@ktda.com', 43, 0, 1, 1),  -- Ndima
('eric.kinyeki@ktda.com', 44, 0, 1, 1),  -- Rukuriri
('eric.kinyeki@ktda.com', 45, 0, 1, 1);  -- Thumaita
```

**Access:**
- Can view submissions from all 8 factories
- Can approve submissions from all 8 factories
- Can see regional reports and analytics
- **Cannot** see data from other regions
- **Cannot** manage users or templates

---

### Example 3: Head Office ICT Manager

**User:** Martin Mwarangu
**Department:** Head Office ICT
**Role:** HO_ICT_MGR

**Setup:**
```sql
-- Role assignment
INSERT INTO UserRoles (UserId, RoleId, IsPrimaryRole)
VALUES ('martin.mwarangu@ktda.com', 2, 1);  -- HO_ICT_MGR

-- Tenant access (all 80 tenants)
INSERT INTO UserTenantAccess (UserId, TenantId, IsPrimaryTenant, CanRead, CanWrite, CanApprove)
SELECT 'martin.mwarangu@ktda.com', TenantId,
       CASE WHEN TenantCode = 'HO' THEN 1 ELSE 0 END,
       1, 1, 1
FROM Tenants;
```

**Access:**
- Can view all data across all 80 tenants
- Can approve submissions from any tenant
- Can create/edit checklist templates
- Can manage users and assign roles
- Can generate system-wide reports

---

## Implementation Sequence

The Identity & Access Management implementation follows this sequence:

**Week 3 (Phase 1):**

1. **Day 1:** ASP.NET Core Identity setup + database tables
2. **Day 2:** User management (CRUD operations, password reset)
3. **Day 3:** Role management + user role assignment
4. **Day 4:** User tenant access + authorization policies
5. **Day 5:** Login/logout UI, testing, seed data

**Detailed Implementation Plans:**
- [1_ImplementationPlan_Identity.md](1_ImplementationPlan_Identity.md) - Week 3 overall plan
- [2_UserManagement_Implementation.md](2_UserManagement_Implementation.md) - User CRUD flows
- [3_RoleManagement_Implementation.md](3_RoleManagement_Implementation.md) - Role management
- [4_UserRoles_Implementation.md](4_UserRoles_Implementation.md) - Role assignment flows
- [5_UserTenantAccess_Implementation.md](5_UserTenantAccess_Implementation.md) - Tenant access flows

---

## Success Criteria

- ✅ Users can login with username/password
- ✅ Account lockout works after 5 failed attempts
- ✅ Password reset via email works
- ✅ Admins can create/edit/deactivate users
- ✅ 6 standard roles created and functional
- ✅ Can assign multiple roles to users
- ✅ Tenant access controls data visibility correctly
- ✅ Regional managers only see their region's data
- ✅ Factory users only see their factory's data
- ✅ Authorization policies enforce permissions

---

## Related Documents

- **Parent Plan:** [ImplementationPlan.md](../ImplementationPlan.md)
- **Previous Phase:** [1_Organizational_Structure/1_ImplementationPlan_OrgStructure.md](../1_Organizational_Structure/1_ImplementationPlan_OrgStructure.md)
- **Database Schema:** [KTDA_Enhanced_Database_Schema.sql](../KTDA_Enhanced_Database_Schema.sql)
- **Technology Stack:** [TechStack.md](../TechStack.md)

---

**Document Version:** 1.0
**Last Updated:** October 30, 2025
**Estimated Implementation Time:** 5 days (Week 3 of Phase 1)
**Complexity:** Medium-High
**Dependencies:** Week 2 (Organizational Structure) must be completed

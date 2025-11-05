# Organizational Structure - Implementation Plan

**Version:** 1.0
**Date:** October 30, 2025
**Phase:** Phase 1 - Week 2 (Database Foundation)
**Duration:** 5 Working Days
**Section:** 1 - Organizational Structure

---

## Table of Contents

1. [Implementation Overview](#overview)
2. [Week 2 Day-by-Day Breakdown](#daily-breakdown)
3. [Database Schema Implementation](#database-schema)
4. [Entity Framework Core Setup](#ef-core-setup)
5. [Multi-Tenancy Implementation](#multi-tenancy)
6. [CRUD Operations](#crud-operations)
7. [UI Components](#ui-components)
8. [Seed Data Strategy](#seed-data)
9. [Testing Strategy](#testing)
10. [Success Criteria](#success-criteria)

---

## <a name="overview"></a>Implementation Overview

### Purpose

Build the **foundational multi-tenant organizational structure** that will support all other system features. This includes:
- 71 tea factories across 7 regions
- 9 KTDA subsidiaries
- Head Office tenant
- Multi-tenant data isolation with TenantId filtering

### Goals for Week 2

1. ✅ Create database tables: `Regions`, `Tenants`, `UserTenantAccess`
2. ✅ Implement Entity Framework Core entities and configurations
3. ✅ Build CRUD operations for managing tenants and regions
4. ✅ Implement automatic TenantId filtering at application layer
5. ✅ Seed all 71 factories, 7 regions, 9 subsidiaries, and Head Office
6. ✅ Create admin UI for tenant management
7. ✅ Test multi-tenant data isolation

### Technology Stack Reminder

- **Backend:** ASP.NET Core 8.0 MVC + Razor Pages + C# 12
- **ORM:** Entity Framework Core 8.0
- **Database:** SQL Server 2022 Standard Edition
- **Frontend:** Bootstrap 5 + jQuery + DataTables.js
- **Architecture:** Clean Architecture (4 layers: Web, Application, Core, Infrastructure)

### Prerequisites

- ✅ Phase 0 completed (environment setup, solution structure created)
- ✅ SQL Server 2022 installed and configured
- ✅ Development environment operational
- ✅ Git repository initialized

---

## <a name="daily-breakdown"></a>Week 2: Day-by-Day Breakdown

### Day 1 (Monday): Database Schema Design & Creation

**Focus:** Create the foundational database tables

**Morning (4 hours):**
1. Design database schema for `Regions`, `Tenants`, `UserTenantAccess` tables
2. Write SQL migration script
3. Review schema with DBA
4. Document table relationships and constraints

**Afternoon (4 hours):**
1. Create EF Core migration: `Add-Migration CreateOrganizationalStructure`
2. Review migration SQL output
3. Apply migration to development database: `Update-Database`
4. Verify tables created correctly using SSMS
5. Test check constraints (especially for `Tenants.RegionId` validation)

**Deliverables:**
- ✅ SQL migration script
- ✅ Three tables created in database
- ✅ Foreign key relationships established
- ✅ Check constraints validated

**Testing:**
- Verify factories MUST have RegionId (not null)
- Verify Head Office and Subsidiaries CANNOT have RegionId (must be null)
- Verify cascade delete relationships work correctly

---

### Day 2 (Tuesday): Entity Classes & EF Core Configuration

**Focus:** Create domain entities and configure EF Core mappings

**Morning (4 hours):**
1. Create entity classes in `Core` project:
   - `Region.cs`
   - `Tenant.cs`
   - `UserTenantAccess.cs`
2. Add entity properties, navigation properties, and domain logic
3. Create enums: `TenantType`, `TenantStatus`

**Afternoon (4 hours):**
1. Create EF Core configurations in `Infrastructure` project:
   - `RegionConfiguration.cs`
   - `TenantConfiguration.cs`
   - `UserTenantAccessConfiguration.cs`
2. Configure table mappings, indexes, and relationships
3. Register configurations in `ApplicationDbContext`
4. Add global query filter for TenantId (multi-tenancy)
5. Create `ITenantContext` interface for tenant resolution

**Deliverables:**
- ✅ Three entity classes with properties and navigation properties
- ✅ Three EF Core configuration classes
- ✅ Global query filter for automatic TenantId filtering
- ✅ ITenantContext interface

**Testing:**
- Build solution and verify no compilation errors
- Run migrations again to ensure idempotency
- Test that navigation properties load correctly

---

### Day 3 (Wednesday): Repository & Service Layer

**Focus:** Implement business logic and data access layers

**Morning (4 hours):**
1. Create repository interfaces in `Core` project:
   - `IRegionRepository`
   - `ITenantRepository`
2. Implement repositories in `Infrastructure` project:
   - `RegionRepository.cs`
   - `TenantRepository.cs`
3. Implement `IUnitOfWork` pattern
4. Add repository methods: GetAll, GetById, Add, Update, Delete, GetByRegion, etc.

**Afternoon (4 hours):**
1. Create service interfaces in `Application` project:
   - `IRegionService`
   - `ITenantService`
2. Implement services:
   - `RegionService.cs`
   - `TenantService.cs`
3. Add business logic:
   - Validate tenant creation rules (RegionId constraints)
   - Check for duplicate tenant names
   - Soft delete implementation
   - Tenant activation/deactivation
4. Create DTOs and ViewModels
5. Configure AutoMapper profiles

**Deliverables:**
- ✅ Repository interfaces and implementations
- ✅ Service interfaces and implementations
- ✅ DTOs for Region, Tenant, UserTenantAccess
- ✅ AutoMapper profiles
- ✅ Unit of Work implementation

**Testing:**
- Write unit tests for service layer validation logic
- Test repository methods with in-memory database
- Verify AutoMapper mappings work correctly

---

### Day 4 (Thursday): Admin UI - Tenant & Region Management

**Focus:** Build Razor Pages for CRUD operations

**Morning (4 hours):**
1. Create Razor Pages in `Pages/Admin/` folder:
   - `Regions/Index.cshtml` (list all regions)
   - `Regions/Create.cshtml`
   - `Regions/Edit.cshtml`
   - `Tenants/Index.cshtml` (list all tenants)
   - `Tenants/Create.cshtml`
   - `Tenants/Edit.cshtml`
   - `Tenants/Details.cshtml`
2. Create corresponding PageModel classes
3. Inject services into PageModels

**Afternoon (4 hours):**
1. Implement form validation using FluentValidation
2. Add client-side validation (jQuery Validation)
3. Implement DataTables.js for tenant/region listing with:
   - Search functionality
   - Sorting by columns
   - Pagination
   - Export to Excel
4. Add modal dialogs for delete confirmation
5. Implement success/error notifications (toastr.js)

**Deliverables:**
- ✅ Region management UI (Create, Edit, List)
- ✅ Tenant management UI (Create, Edit, List, Details)
- ✅ DataTables integration for listing pages
- ✅ Form validation (client and server-side)
- ✅ Delete confirmation modals

**Testing:**
- Test all CRUD operations through UI
- Verify validation messages display correctly
- Test DataTables search, sort, pagination
- Test form submission with invalid data
- Verify only System Admin can access these pages

---

### Day 5 (Friday): Seed Data & Multi-Tenancy Testing

**Focus:** Load production data and test data isolation

**Morning (4 hours):**
1. Create seed data service: `OrganizationalStructureSeedService.cs`
2. Implement seed data methods:
   - `SeedRegions()` - Create 7 regions
   - `SeedFactories()` - Create 71 factories
   - `SeedSubsidiaries()` - Create 9 subsidiaries
   - `SeedHeadOffice()` - Create Head Office tenant
3. Add seed data to `Program.cs` startup
4. Run application and verify all 80 tenants created
5. Verify TenantId auto-assignment works correctly

**Afternoon (4 hours):**
1. Implement tenant context service: `TenantContextService.cs`
2. Add middleware to resolve current tenant from user claims
3. Test multi-tenant data isolation:
   - Create test data for multiple tenants
   - Verify queries automatically filter by TenantId
   - Test that users cannot access other tenant's data
4. Create test scenarios for all three tenant types
5. Document multi-tenancy implementation

**Deliverables:**
- ✅ Seed data service with all 80 tenants
- ✅ All 7 regions created
- ✅ All 71 factories assigned to correct regions
- ✅ All 9 subsidiaries created
- ✅ Head Office tenant created
- ✅ Tenant context resolution working
- ✅ Multi-tenant data isolation verified

**Testing:**
- Verify all 80 tenants loaded correctly
- Verify regional assignments are accurate
- Test TenantId filtering with real queries
- Test tenant context switching
- Verify no cross-tenant data leakage

---

## <a name="database-schema"></a>Database Schema Implementation

### Table 1: Regions

**Purpose:** Store the 7 KTDA regional divisions

**SQL Schema:**
```sql
CREATE TABLE Regions (
    RegionId INT PRIMARY KEY IDENTITY(1,1),
    RegionCode NVARCHAR(10) NOT NULL UNIQUE,
    RegionName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(100),

    CONSTRAINT CK_Regions_RegionCode CHECK (RegionCode LIKE 'R[1-7]')
);

CREATE INDEX IX_Regions_RegionCode ON Regions(RegionCode);
```

**Sample Data:**
```
┌──────────┬────────────┬─────────────┬────────────────────────────────────┐
│ RegionId │ RegionCode │ RegionName  │ Description                        │
├──────────┼────────────┼─────────────┼────────────────────────────────────┤
│ 1        │ R1         │ Region 1    │ Kiambu & Murang'a Counties         │
│ 2        │ R2         │ Region 2    │ Murang'a & Nyeri Counties          │
│ 3        │ R3         │ Region 3    │ Kirinyaga & Embu Counties          │
│ 4        │ R4         │ Region 4    │ Meru & Tharaka Nithi Counties      │
│ 5        │ R5         │ Region 5    │ Kericho & Bomet Counties           │
│ 6        │ R6         │ Region 6    │ Kisii & Nyamira Counties           │
│ 7        │ R7         │ Region 7    │ Nandi, Trans Nzoia, Vihiga Counties│
└──────────┴────────────┴─────────────┴────────────────────────────────────┘
```

---

### Table 2: Tenants

**Purpose:** Store all 80 tenants (71 factories + 9 subsidiaries + Head Office)

**SQL Schema:**
```sql
CREATE TABLE Tenants (
    TenantId INT PRIMARY KEY IDENTITY(1,1),
    TenantCode NVARCHAR(20) NOT NULL UNIQUE,
    TenantName NVARCHAR(200) NOT NULL,
    TenantType NVARCHAR(20) NOT NULL,
    RegionId INT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    Address NVARCHAR(500),
    PhoneNumber NVARCHAR(20),
    EmailAddress NVARCHAR(100),
    ManagerName NVARCHAR(100),
    ManagerEmail NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(100),

    CONSTRAINT FK_Tenants_Regions FOREIGN KEY (RegionId)
        REFERENCES Regions(RegionId) ON DELETE SET NULL,

    CONSTRAINT CK_Tenants_TenantType
        CHECK (TenantType IN ('HeadOffice', 'Factory', 'Subsidiary')),

    CONSTRAINT CK_Tenants_Status
        CHECK (Status IN ('Active', 'Inactive', 'Suspended')),

    -- Business Rule: Factories MUST have RegionId, others MUST NOT
    CONSTRAINT CK_Tenants_RegionId_Factory
        CHECK (
            (TenantType = 'Factory' AND RegionId IS NOT NULL) OR
            (TenantType IN ('HeadOffice', 'Subsidiary') AND RegionId IS NULL)
        )
);

CREATE INDEX IX_Tenants_RegionId ON Tenants(RegionId);
CREATE INDEX IX_Tenants_TenantType ON Tenants(TenantType);
CREATE INDEX IX_Tenants_Status ON Tenants(Status);
CREATE INDEX IX_Tenants_TenantCode ON Tenants(TenantCode);
```

**Sample Data:**
```
┌──────────┬─────────────┬───────────────────────┬──────────────┬──────────┬────────┐
│ TenantId │ TenantCode  │ TenantName            │ TenantType   │ RegionId │ Status │
├──────────┼─────────────┼───────────────────────┼──────────────┼──────────┼────────┤
│ 1        │ HO          │ Head Office           │ HeadOffice   │ NULL     │ Active │
│ 2        │ R1-GACH     │ Gacharage Factory     │ Factory      │ 1        │ Active │
│ 3        │ R1-GCHE     │ Gachege Factory       │ Factory      │ 1        │ Active │
│ 4        │ R1-IKUM     │ Ikumbi Factory        │ Factory      │ 1        │ Active │
│ ...      │ ...         │ ...                   │ ...          │ ...      │ ...    │
│ 72       │ SUB-KETEPA  │ KETEPA Limited        │ Subsidiary   │ NULL     │ Active │
│ 73       │ SUB-CHAI    │ Chai Trading Company  │ Subsidiary   │ NULL     │ Active │
│ ...      │ ...         │ ...                   │ ...          │ ...      │ ...    │
└──────────┴─────────────┴───────────────────────┴──────────────┴──────────┴────────┘
```

**Factory Count by Region:**
```
Region 1: 12 factories (Kiambu & Murang'a)
Region 2: 9 factories (Murang'a & Nyeri)
Region 3: 8 factories (Kirinyaga & Embu)
Region 4: 8 factories (Meru & Tharaka Nithi)
Region 5: 16 factories (Kericho & Bomet)
Region 6: 14 factories (Kisii & Nyamira)
Region 7: 4 factories (Nandi, Trans Nzoia, Vihiga)
```

---

### Table 3: UserTenantAccess

**Purpose:** Control which users can access which tenants

**SQL Schema:**
```sql
CREATE TABLE UserTenantAccess (
    UserTenantAccessId INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(450) NOT NULL,
    TenantId INT NOT NULL,
    IsPrimaryTenant BIT NOT NULL DEFAULT 0,
    GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    GrantedBy NVARCHAR(100),
    RevokedAt DATETIME2 NULL,
    RevokedBy NVARCHAR(100) NULL,

    CONSTRAINT FK_UserTenantAccess_Users FOREIGN KEY (UserId)
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE,

    CONSTRAINT FK_UserTenantAccess_Tenants FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId) ON DELETE CASCADE,

    CONSTRAINT UQ_UserTenantAccess_User_Tenant
        UNIQUE (UserId, TenantId)
);

CREATE INDEX IX_UserTenantAccess_UserId ON UserTenantAccess(UserId);
CREATE INDEX IX_UserTenantAccess_TenantId ON UserTenantAccess(TenantId);
```

**Sample Data:**
```
┌─────────────────────┬────────┬──────────┬─────────────────┐
│ UserTenantAccessId  │ UserId │ TenantId │ IsPrimaryTenant │
├─────────────────────┼────────┼──────────┼─────────────────┤
│ 1                   │ user123│ 38       │ 1 (Primary)     │
│ 2                   │ user456│ 34       │ 1 (Primary)     │
│ 3                   │ user789│ 1        │ 1 (Primary)     │
│ 4                   │ user789│ 2        │ 0 (Secondary)   │
│ 5                   │ user789│ 3        │ 0 (Secondary)   │
└─────────────────────┴────────┴──────────┴─────────────────┘
```

**Access Rules:**
- **Field Systems Administrator:** Has access to ONE factory (IsPrimaryTenant = 1)
- **Regional ICT Manager:** Has access to ALL factories in their region
- **Head Office ICT Manager:** Has access to ALL tenants (80 total)
- **System Admin:** Has access to ALL tenants (80 total)

---

## <a name="ef-core-setup"></a>Entity Framework Core Setup

### Entity Class: Region

**File:** `Core/Entities/Region.cs`

**Properties:**
- RegionId (int, Primary Key)
- RegionCode (string, required, unique, max 10 chars)
- RegionName (string, required, max 100 chars)
- Description (string, max 500 chars)
- CreatedAt (DateTime)
- CreatedBy (string)
- UpdatedAt (DateTime?)
- UpdatedBy (string?)

**Navigation Properties:**
- `ICollection<Tenant> Factories` (one-to-many relationship)

**Domain Methods:**
- `GetFactoryCount()` - Returns number of factories in region
- `IsActive()` - Returns true if region has active factories

---

### Entity Class: Tenant

**File:** `Core/Entities/Tenant.cs`

**Properties:**
- TenantId (int, Primary Key)
- TenantCode (string, required, unique, max 20 chars)
- TenantName (string, required, max 200 chars)
- TenantType (TenantType enum: HeadOffice, Factory, Subsidiary)
- RegionId (int?, nullable)
- Status (TenantStatus enum: Active, Inactive, Suspended)
- Address (string, max 500 chars)
- PhoneNumber (string, max 20 chars)
- EmailAddress (string, max 100 chars)
- ManagerName (string, max 100 chars)
- ManagerEmail (string, max 100 chars)
- CreatedAt (DateTime)
- CreatedBy (string)
- UpdatedAt (DateTime?)
- UpdatedBy (string?)

**Navigation Properties:**
- `Region? Region` (many-to-one relationship)
- `ICollection<UserTenantAccess> UserAccess` (one-to-many)

**Domain Methods:**
- `IsFactory()` - Returns true if TenantType is Factory
- `RequiresRegion()` - Returns true if tenant type requires RegionId
- `CanActivate()` - Validates if tenant can be activated
- `CanDeactivate()` - Validates if tenant can be deactivated

---

### Entity Configuration: RegionConfiguration

**File:** `Infrastructure/Data/Configurations/RegionConfiguration.cs`

**Implements:** `IEntityTypeConfiguration<Region>`

**Configuration Steps:**
1. Configure primary key: `HasKey(r => r.RegionId)`
2. Configure properties:
   - RegionCode: Required, MaxLength(10), Unique index
   - RegionName: Required, MaxLength(100)
   - Description: MaxLength(500)
3. Configure relationships:
   - `HasMany(r => r.Factories).WithOne(t => t.Region).HasForeignKey(t => t.RegionId)`
4. Configure table name: `ToTable("Regions")`
5. Add seed data for 7 regions

---

### Entity Configuration: TenantConfiguration

**File:** `Infrastructure/Data/Configurations/TenantConfiguration.cs`

**Implements:** `IEntityTypeConfiguration<Tenant>`

**Configuration Steps:**
1. Configure primary key: `HasKey(t => t.TenantId)`
2. Configure properties:
   - TenantCode: Required, MaxLength(20), Unique index
   - TenantName: Required, MaxLength(200)
   - TenantType: Convert enum to string
   - Status: Convert enum to string, default value "Active"
3. Configure relationships:
   - `HasOne(t => t.Region).WithMany(r => r.Factories).HasForeignKey(t => t.RegionId).OnDelete(DeleteBehavior.SetNull)`
4. Configure indexes:
   - Index on RegionId
   - Index on TenantType
   - Index on Status
5. Configure check constraints (via raw SQL in migration)
6. Add seed data for Head Office

---

### Global Query Filter for Multi-Tenancy

**File:** `Infrastructure/Data/ApplicationDbContext.cs`

**Implementation Steps:**

1. Add `ITenantContext` interface to resolve current tenant
2. Inject `ITenantContext` into ApplicationDbContext
3. Override `OnModelCreating()` method
4. Apply global query filter to all tenant-specific entities

**Filter Logic:**
```
For entities with TenantId property:
- Automatically append WHERE TenantId = @CurrentTenantId to ALL queries
- Bypass filter for System Admins and Head Office users
- Allow Regional ICT Managers to see all factories in their region
```

**Important:** Global query filters apply to:
- All future tables: TenantHardware, TenantSoftware, ChecklistSubmissions, Tickets, etc.
- NOT applied to: Regions, AspNetUsers, AspNetRoles (system-wide tables)

---

## <a name="multi-tenancy"></a>Multi-Tenancy Implementation

### Tenant Context Resolution

**Interface:** `ITenantContext`

**Methods:**
- `int? GetCurrentTenantId()` - Returns current user's primary tenant
- `List<int> GetAccessibleTenantIds()` - Returns all tenants user can access
- `bool CanAccessTenant(int tenantId)` - Checks if user can access specific tenant
- `TenantType GetCurrentTenantType()` - Returns type of current tenant
- `bool IsSystemAdmin()` - Returns true if user is System Admin
- `bool IsHeadOfficeUser()` - Returns true if user belongs to Head Office

**Implementation:** `TenantContextService`

**Tenant Resolution Flow:**
```
1. User logs in
   ↓
2. System reads user's claims (includes UserId)
   ↓
3. Query UserTenantAccess table for user's tenant assignments
   ↓
4. Find IsPrimaryTenant = 1 row (user's main tenant)
   ↓
5. Store TenantId in HttpContext.Items for current request
   ↓
6. All queries automatically filter by this TenantId
```

---

### Automatic TenantId Filtering

**How It Works:**

**Example 1: Field Systems Administrator Query**
```
User: Elizabeth Ndegwa (Kangaita Factory, TenantId = 38)

User writes query:
  var hardware = dbContext.TenantHardware.ToList();

EF Core automatically converts to:
  SELECT * FROM TenantHardware WHERE TenantId = 38;

Result: Elizabeth only sees Kangaita Factory's hardware
```

**Example 2: Regional ICT Manager Query**
```
User: Eric Kinyeki (Region 3 ICT Manager)

User writes query:
  var submissions = dbContext.ChecklistSubmissions.ToList();

EF Core automatically converts to:
  SELECT * FROM ChecklistSubmissions cs
  WHERE cs.TenantId IN (38, 39, 40, 41, 42, 43, 44, 45)  -- All Region 3 factories

Result: Eric sees all submissions from his 8 factories
```

**Example 3: System Admin / Head Office Query**
```
User: System Admin or Head Office ICT Manager

Query filter is BYPASSED using .IgnoreQueryFilters()

User writes query:
  var allSubmissions = dbContext.ChecklistSubmissions
                                .IgnoreQueryFilters()
                                .ToList();

Result: User sees ALL 80 tenants' data
```

---

### Data Isolation Architecture

**5-Layer Security Model:**

```
┌────────────────────────────────────────────────────────────┐
│ Layer 1: Application Layer (EF Core Global Query Filter)  │
│ - Automatic WHERE TenantId = X appended to ALL queries    │
└────────────────────────────────────────────────────────────┘
                            ↓
┌────────────────────────────────────────────────────────────┐
│ Layer 2: Service Layer (Business Logic Validation)        │
│ - Validate user has permission to access requested tenant │
│ - Check UserTenantAccess before any operation             │
└────────────────────────────────────────────────────────────┘
                            ↓
┌────────────────────────────────────────────────────────────┐
│ Layer 3: Authorization Layer (Role-Based Access Control)  │
│ - [Authorize] attributes on controllers/pages             │
│ - Role checks: SystemAdmin, HeadOfficeICTManager, etc.    │
└────────────────────────────────────────────────────────────┘
                            ↓
┌────────────────────────────────────────────────────────────┐
│ Layer 4: Database Layer (Foreign Key Constraints)         │
│ - All tenant-specific tables have FK to Tenants table     │
│ - Cannot insert data with invalid TenantId                │
└────────────────────────────────────────────────────────────┘
                            ↓
┌────────────────────────────────────────────────────────────┐
│ Layer 5: Audit Layer (Logging & Change Tracking)          │
│ - All data access logged with UserId + TenantId           │
│ - Audit trail for cross-tenant access attempts            │
└────────────────────────────────────────────────────────────┘
```

---

### Bypassing Query Filters (Authorized Scenarios)

**When to Use `.IgnoreQueryFilters()`:**

✅ **Approved Scenarios:**
1. System Admin performing system-wide operations
2. Head Office ICT Manager viewing all tenant reports
3. Regional ICT Manager viewing all factories in their region
4. Generating cross-tenant analytics/dashboards
5. Data export operations for authorized users

❌ **Prohibited Scenarios:**
1. Regular users (Field Systems Administrators)
2. Any query without explicit authorization check
3. External API calls without proper authentication

**Implementation Pattern:**
```
Service Method: GetAllTenantsData()
  ↓
1. Check if user is SystemAdmin OR HeadOfficeICTManager
  ↓
2. If NO → Throw UnauthorizedAccessException
  ↓
3. If YES → Use .IgnoreQueryFilters() and log the access
  ↓
4. Audit log: "User X accessed all tenant data at [timestamp]"
```

---

## <a name="crud-operations"></a>CRUD Operations

### Region Management

#### Create Region

**Steps:**
1. Navigate to `/Admin/Regions/Create`
2. Fill in form fields:
   - Region Code (e.g., "R1", "R2", ... "R7")
   - Region Name (e.g., "Region 1")
   - Description (e.g., "Kiambu & Murang'a Counties")
3. Click "Save" button
4. Validate:
   - Region Code must be unique
   - Region Code must match pattern "R[1-7]"
   - Region Name is required
5. If valid, save to database
6. Redirect to Regions list with success message

**Validation Rules:**
- Region Code: Required, 2-10 characters, unique, matches regex "^R[1-7]$"
- Region Name: Required, 3-100 characters
- Description: Optional, max 500 characters

---

#### Edit Region

**Steps:**
1. Navigate to `/Admin/Regions/Edit/{regionId}`
2. Load existing region data into form
3. Allow editing Region Name and Description only (not Region Code)
4. Click "Save" button
5. Validate changes
6. Update database with UpdatedAt and UpdatedBy
7. Redirect to Regions list with success message

**Business Rules:**
- Cannot edit Region Code (identifier should be immutable)
- Cannot delete region if it has active factories
- Can deactivate region (soft delete)

---

#### List Regions

**Page:** `/Admin/Regions/Index`

**Features:**
- DataTables integration with:
  - Search by region code or name
  - Sort by any column
  - Pagination (25 per page)
  - Export to Excel
- Display columns:
  - Region Code
  - Region Name
  - Description
  - Number of Factories
  - Created Date
  - Status
  - Actions (Edit, View Details)

---

### Tenant Management

#### Create Tenant

**Steps:**
1. Navigate to `/Admin/Tenants/Create`
2. Select Tenant Type from dropdown:
   - Head Office
   - Factory
   - Subsidiary
3. Fill in form fields:
   - Tenant Code (e.g., "R1-GACH" for factories)
   - Tenant Name (e.g., "Gacharage Factory")
   - Region (dropdown, enabled only if Tenant Type = Factory)
   - Contact Information (address, phone, email)
   - Manager Details (name, email)
4. Click "Save" button
5. Validate:
   - If Factory → Region is required
   - If Head Office or Subsidiary → Region must be null
   - Tenant Code must be unique
6. If valid, save to database with Status = Active
7. Redirect to Tenants list with success message

**Validation Rules:**
- Tenant Code: Required, 3-20 characters, unique, alphanumeric with hyphens
- Tenant Name: Required, 3-200 characters, unique
- Tenant Type: Required, must be one of enum values
- RegionId: Required if TenantType = Factory, must be null otherwise
- Email: Optional, valid email format
- Phone: Optional, valid phone format

**Dynamic Form Behavior:**
```
When Tenant Type changes:
  ↓
If "Factory" selected:
  - Show Region dropdown (enabled)
  - Mark Region as required
  - Show factory-specific fields

If "Head Office" or "Subsidiary" selected:
  - Hide Region dropdown
  - Clear any selected region
  - Show subsidiary-specific fields (if applicable)
```

---

#### Edit Tenant

**Steps:**
1. Navigate to `/Admin/Tenants/Edit/{tenantId}`
2. Load existing tenant data into form
3. Allow editing:
   - Tenant Name
   - Contact Information
   - Manager Details
   - Status (Active, Inactive, Suspended)
4. Do NOT allow editing:
   - Tenant Code (immutable identifier)
   - Tenant Type (cannot change factory to subsidiary)
   - Region (should be changed via separate "Transfer" operation)
5. Click "Save" button
6. Validate changes
7. Update database with UpdatedAt and UpdatedBy
8. Redirect to Tenants list with success message

**Business Rules:**
- Cannot deactivate tenant if it has pending submissions
- Cannot delete tenant (only deactivate)
- Cannot change tenant type after creation
- Changing status to "Suspended" disables all user logins for that tenant

---

#### View Tenant Details

**Page:** `/Admin/Tenants/Details/{tenantId}`

**Information Displayed:**
- Basic Information:
  - Tenant Code, Name, Type, Status
  - Region (if factory)
  - Contact details
  - Manager information
- Statistics:
  - Number of users assigned
  - Number of active submissions
  - Last activity date
  - Date created
- Related Data:
  - List of users with access (from UserTenantAccess)
  - Recent submissions
  - Hardware count
  - Software installations count
- Actions:
  - Edit Tenant
  - Activate/Deactivate
  - Manage User Access

---

#### List Tenants

**Page:** `/Admin/Tenants/Index`

**Features:**
- DataTables integration with:
  - Search by tenant code or name
  - Filter by Tenant Type (All, Head Office, Factory, Subsidiary)
  - Filter by Region (dropdown)
  - Filter by Status (All, Active, Inactive, Suspended)
  - Sort by any column
  - Pagination (50 per page)
  - Export to Excel
- Display columns:
  - Tenant Code
  - Tenant Name
  - Tenant Type
  - Region (for factories)
  - Manager Name
  - Status
  - Created Date
  - Actions (Edit, Details, Manage Access)

**UI Mockup:**
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Tenants Management                                          [+ Create Tenant]│
├─────────────────────────────────────────────────────────────────────────────┤
│ Filter by Type: [All ▼]  Region: [All ▼]  Status: [Active ▼]  [Export Excel]│
├──────┬───────────┬──────────────────────┬──────────┬─────────┬────────┬─────┤
│ Code │ Name      │ Type                 │ Region   │ Manager │ Status │ ... │
├──────┼───────────┼──────────────────────┼──────────┼─────────┼────────┼─────┤
│ HO   │ Head Office│ HeadOffice          │ -        │ Martin  │ Active │ [E] │
│ R1-  │ Gacharage │ Factory              │ Region 1 │ -       │ Active │ [E] │
│ GACH │ Factory   │                      │          │         │        │     │
│ R1-  │ Gachege   │ Factory              │ Region 1 │ -       │ Active │ [E] │
│ GCHE │ Factory   │                      │          │         │        │     │
│ SUB- │ KETEPA    │ Subsidiary           │ -        │ -       │ Active │ [E] │
│ KETEPA Limited   │                      │          │         │        │     │
├──────┴───────────┴──────────────────────┴──────────┴─────────┴────────┴─────┤
│ Showing 1 to 50 of 80 entries                        [< Prev] [1] [2] [Next >]│
└─────────────────────────────────────────────────────────────────────────────┘

[E] = Edit button
```

---

## <a name="ui-components"></a>UI Components

### Component 1: Tenant Type Selector

**Purpose:** Allow admin to select tenant type with visual indicators

**UI Mockup:**
```
┌──────────────────────────────────────────────────────────────────┐
│ Select Tenant Type *                                             │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │   🏢         │  │   🏭         │  │   🏪         │          │
│  │ Head Office  │  │   Factory    │  │ Subsidiary   │          │
│  │              │  │              │  │              │          │
│  │ [ Select ]   │  │ [ Select ]   │  │ [ Select ]   │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                  │
│  Currently selected: Factory                                     │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

**Behavior:**
- Three clickable cards for each tenant type
- Selected card gets highlighted border (blue)
- Shows different form fields based on selection
- Visual icons help users identify tenant type quickly

---

### Component 2: Region Selector (Conditional)

**Purpose:** Show region dropdown only when Factory is selected

**UI Mockup - Factory Selected:**
```
┌──────────────────────────────────────────────────────────────────┐
│ Tenant Type: Factory                                             │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│ Tenant Code *                                                    │
│ ┌────────────────────────────────────────────────────────────┐  │
│ │ R1-GACH                                                     │  │
│ └────────────────────────────────────────────────────────────┘  │
│ Format: R{RegionNumber}-{Code} (e.g., R1-GACH)                  │
│                                                                  │
│ Tenant Name *                                                    │
│ ┌────────────────────────────────────────────────────────────┐  │
│ │ Gacharage Tea Factory                                       │  │
│ └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│ Region *                                                         │
│ ┌────────────────────────────────────────────────────────────┐  │
│ │ Region 1 - Kiambu & Murang'a Counties              [▼]     │  │
│ └────────────────────────────────────────────────────────────┘  │
│ Required for factory tenants                                     │
│                                                                  │
│ [Cancel]                                      [Save Tenant]      │
└──────────────────────────────────────────────────────────────────┘
```

**UI Mockup - Subsidiary Selected:**
```
┌──────────────────────────────────────────────────────────────────┐
│ Tenant Type: Subsidiary                                          │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│ Tenant Code *                                                    │
│ ┌────────────────────────────────────────────────────────────┐  │
│ │ SUB-KETEPA                                                  │  │
│ └────────────────────────────────────────────────────────────┘  │
│ Format: SUB-{Code}                                               │
│                                                                  │
│ Tenant Name *                                                    │
│ ┌────────────────────────────────────────────────────────────┐  │
│ │ KETEPA Limited                                              │  │
│ └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│ ⓘ Note: Subsidiaries are not assigned to any region            │
│                                                                  │
│ [Cancel]                                      [Save Tenant]      │
└──────────────────────────────────────────────────────────────────┘
```

---

### Component 3: Tenant Status Badge

**Purpose:** Visual indicator of tenant status

**UI Mockup:**
```
Status:  ┌──────────┐
         │  Active  │  (Green background)
         └──────────┘

Status:  ┌──────────┐
         │ Inactive │  (Gray background)
         └──────────┘

Status:  ┌───────────┐
         │ Suspended │  (Red background)
         └───────────┘
```

**CSS Classes:**
- `.badge-success` for Active (green)
- `.badge-secondary` for Inactive (gray)
- `.badge-danger` for Suspended (red)

---

### Component 4: DataTable with Export

**Purpose:** Display tenant list with search, filter, pagination, and export

**UI Mockup:**
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Search: [         ]                                      [Export to Excel]  │
├──────┬────────────────┬──────────┬─────────┬────────┬────────────┬─────────┤
│ Code │ Name           │ Type     │ Region  │ Status │ Created    │ Actions │
├──────┼────────────────┼──────────┼─────────┼────────┼────────────┼─────────┤
│ HO   │ Head Office    │ HO       │ -       │ Active │ 2025-01-01 │ [E] [V] │
│ R1-  │ Gacharage      │ Factory  │ Region 1│ Active │ 2025-01-01 │ [E] [V] │
│ GACH │ Factory        │          │         │        │            │         │
│ R1-  │ Gachege        │ Factory  │ Region 1│ Active │ 2025-01-01 │ [E] [V] │
│ GCHE │ Factory        │          │         │        │            │         │
│ ...  │ ...            │ ...      │ ...     │ ...    │ ...        │ ...     │
├──────┴────────────────┴──────────┴─────────┴────────┴────────────┴─────────┤
│ Showing 1 to 50 of 80 entries                  [Prev] [1] [2] [Next]       │
└─────────────────────────────────────────────────────────────────────────────┘

[E] = Edit  [V] = View Details
```

**JavaScript Libraries:**
- DataTables.js for table functionality
- Buttons extension for Excel export
- Bootstrap styling

---

### Component 5: Confirmation Modal

**Purpose:** Confirm destructive actions like deactivation or deletion

**UI Mockup:**
```
┌─────────────────────────────────────────────────────────────┐
│ ⚠️  Confirm Action                                    [X]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ Are you sure you want to deactivate this tenant?            │
│                                                             │
│ Tenant: Gacharage Factory (R1-GACH)                        │
│                                                             │
│ This will:                                                  │
│  • Prevent users from accessing this tenant                │
│  • Disable all checklist submissions                       │
│  • Hide tenant from reports                                │
│                                                             │
│ This action can be reversed by reactivating the tenant.    │
│                                                             │
│                    [Cancel]          [Confirm Deactivate]  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Implementation:**
- Bootstrap modal component
- AJAX call to deactivate endpoint
- Success/error toast notification after action

---

## <a name="seed-data"></a>Seed Data Strategy

### Seed Data Service Implementation

**Class:** `OrganizationalStructureSeedService`

**Methods:**

1. **SeedRegions()**
   - Create 7 regions with accurate data
   - Region 1: Kiambu & Murang'a Counties
   - Region 2: Murang'a & Nyeri Counties
   - Region 3: Kirinyaga & Embu Counties
   - Region 4: Meru & Tharaka Nithi Counties
   - Region 5: Kericho & Bomet Counties
   - Region 6: Kisii & Nyamira Counties
   - Region 7: Nandi, Trans Nzoia, Vihiga Counties

2. **SeedHeadOffice()**
   - Create single Head Office tenant
   - TenantCode: "HO"
   - TenantType: HeadOffice
   - RegionId: NULL
   - Status: Active

3. **SeedFactories()**
   - Create all 71 factories with accurate data
   - Assign each factory to correct region
   - Generate tenant codes (format: R{RegionId}-{Code})
   - Set Status: Active for all

4. **SeedSubsidiaries()**
   - Create all 9 subsidiaries:
     1. KTDA Management Services (SUB-KTDAMS)
     2. KETEPA Limited (SUB-KETEPA)
     3. Chai Trading Company (SUB-CHAI)
     4. Greenland Fedha Limited (SUB-GREENLAND)
     5. Majani Insurance Brokers (SUB-MAJANI)
     6. KTDA Power Company (SUB-POWER)
     7. TEMEC Limited (SUB-TEMEC)
     8. KTDA Foundation (SUB-FOUNDATION)
     9. Chai Logistics Centre (SUB-LOGISTICS)
   - TenantType: Subsidiary
   - RegionId: NULL
   - Status: Active

---

### Factory Seed Data by Region

#### Region 1 Factories (12 total)

```
1.  R1-GACH    → Gacharage Tea Factory
2.  R1-GCHE    → Gachege Tea Factory
3.  R1-IKUM    → Ikumbi Tea Factory
4.  R1-KAMB    → Kambaa Tea Factory
5.  R1-KAGW    → Kagwe Tea Factory
6.  R1-MATA    → Mataara Tea Factory
7.  R1-NDAR    → Ndarugu Tea Factory
8.  R1-NDUT    → Nduti Tea Factory
9.  R1-NGER    → Ngere Tea Factory
10. R1-NJUN    → Njunu Tea Factory
11. R1-THET    → Theta Tea Factory
12. R1-MAKO    → Makomboki Tea Factory
```

#### Region 2 Factories (9 total)

```
1. R2-CHIN    → Chinga Tea Factory
2. R2-GATH    → Gathuthi Tea Factory
3. R2-GATU    → Gatunguru Tea Factory
4. R2-GITH    → Githambo Tea Factory
5. R2-GITU    → Gitugi Tea Factory
6. R2-IRIA    → Iriaini Tea Factory
7. R2-KANY    → Kanyenyaini Tea Factory
8. R2-KIRU    → Kiru Tea Factory
9. R2-RAGA    → Ragati Tea Factory
```

#### Region 3 Factories (8 total)

```
1. R3-KANG    → Kangaita Tea Factory
2. R3-KATH    → Kathangariri Tea Factory
3. R3-KIMU    → Kimunye Tea Factory
4. R3-MUNU    → Mununga Tea Factory
5. R3-MUNG    → Mungania Tea Factory
6. R3-NDIM    → Ndima Tea Factory
7. R3-RUKU    → Rukuriri Tea Factory
8. R3-THUM    → Thumaita Tea Factory
```

#### Region 4 Factories (8 total)

```
1. R4-GITH    → Githongo Tea Factory
2. R4-IGEM    → Igembe Tea Factory
3. R4-IMEN    → Imenti Tea Factory
4. R4-KIEG    → Kiegoi Tea Factory
5. R4-KINO    → Kinoro Tea Factory
6. R4-KION    → Kionyo Tea Factory
7. R4-MICH    → Michimikuru Tea Factory
8. R4-WERU    → Weru Tea Factory
```

#### Region 5 Factories (16 total)

```
1.  R5-BOIT    → Boito Tea Factory
2.  R5-KAPK    → Kapkatet Tea Factory
3.  R5-KAPO    → Kapkoros Tea Factory
4.  R5-KAPS    → Kapset Tea Factory
5.  R5-KOBE    → Kobel Tea Factory
6.  R5-LITE    → Litein Tea Factory
7.  R5-MOGO    → Mogogosiek Tea Factory
8.  R5-MOMU    → Momul Tea Factory
9.  R5-MOTI    → Motigo Tea Factory
10. R5-OLEN    → Olenguruone Tea Factory
11. R5-RORO    → Rorok Tea Factory
12. R5-TEBE    → Tebesonik Tea Factory
13. R5-TEGA    → Tegat Tea Factory
14. R5-TIRG    → Tirgaga Tea Factory
15. R5-TORO    → Toror Tea Factory
16. R5-CHEL    → Chelal Tea Factory
```

#### Region 6 Factories (14 total)

```
1.  R6-EBER    → Eberege Tea Factory
2.  R6-GIAN    → Gianchore Tea Factory
3.  R6-ITUNG   → Itumbe Tea Factory
4.  R6-KEBI    → Kebirigo Tea Factory
5.  R6-KIAM    → Kiamokama Tea Factory
6.  R6-MATU    → Matunwa Tea Factory
7.  R6-NYAM    → Nyamache Tea Factory
8.  R6-NYAN    → Nyankoba Tea Factory
9.  R6-NYAS    → Nyansiongo Tea Factory
10. R6-OGEM    → Ogembo Tea Factory
11. R6-RIAN    → Rianyamwamu Tea Factory
12. R6-SANG    → Sanganyi Tea Factory
13. R6-SOMB    → Sombogo Tea Factory
14. R6-TOMB    → Tombe Tea Factory
```

#### Region 7 Factories (4 total)

```
1. R7-CHEB    → Chebut Tea Factory
2. R7-KAPS    → Kapsara Tea Factory
3. R7-KAPT    → Kaptumo Tea Factory
4. R7-MUDE    → Mudete Tea Factory
```

---

### Seed Data Execution

**Approach:** Run seed data on application startup (development only)

**Program.cs Implementation:**

```
1. After building the WebApplication app
   ↓
2. Create a service scope
   ↓
3. Resolve ApplicationDbContext and OrganizationalStructureSeedService
   ↓
4. Check if database already has data (prevent duplicate seeding)
   ↓
5. If no data exists:
   - Call SeedRegions()
   - Call SeedHeadOffice()
   - Call SeedFactories()
   - Call SeedSubsidiaries()
   ↓
6. Log seed data results
   ↓
7. Dispose service scope
```

**Important:**
- Only run seed data in Development environment
- Add check to prevent duplicate seeding
- Log all seed operations
- Handle errors gracefully (rollback if any seed fails)

---

## <a name="testing"></a>Testing Strategy

### Unit Tests

**Test Class:** `TenantServiceTests`

**Test Cases:**

1. **CreateTenant_Factory_WithRegion_Success()**
   - Arrange: Create factory tenant with valid RegionId
   - Act: Call tenantService.CreateTenant()
   - Assert: Tenant created successfully, RegionId assigned correctly

2. **CreateTenant_Factory_WithoutRegion_ThrowsException()**
   - Arrange: Create factory tenant without RegionId
   - Act: Call tenantService.CreateTenant()
   - Assert: ValidationException thrown

3. **CreateTenant_Subsidiary_WithRegion_ThrowsException()**
   - Arrange: Create subsidiary tenant with RegionId
   - Act: Call tenantService.CreateTenant()
   - Assert: ValidationException thrown

4. **CreateTenant_DuplicateCode_ThrowsException()**
   - Arrange: Create tenant with code "R1-GACH", then try to create another with same code
   - Act: Call tenantService.CreateTenant() twice
   - Assert: DuplicateException thrown on second call

5. **GetTenantsByRegion_ReturnsOnlyFactoriesInRegion()**
   - Arrange: Seed database with 3 factories in Region 1, 2 in Region 2
   - Act: Call tenantService.GetTenantsByRegion(1)
   - Assert: Returns 3 tenants, all with RegionId = 1

6. **DeactivateTenant_WithPendingSubmissions_ThrowsException()**
   - Arrange: Create tenant with pending checklist submissions
   - Act: Call tenantService.DeactivateTenant()
   - Assert: BusinessRuleException thrown

---

### Integration Tests

**Test Class:** `TenantRepositoryIntegrationTests`

**Test Cases:**

1. **InsertFactory_CheckConstraint_ValidatesRegionId()**
   - Arrange: Try to insert factory with RegionId = NULL
   - Act: Call SaveChanges()
   - Assert: SqlException thrown with check constraint violation

2. **InsertSubsidiary_CheckConstraint_ValidatesRegionIdNull()**
   - Arrange: Try to insert subsidiary with RegionId = 1
   - Act: Call SaveChanges()
   - Assert: SqlException thrown with check constraint violation

3. **QueryWithTenantFilter_ReturnsOnlyCurrentTenantData()**
   - Arrange: Seed database with data for TenantId 1 and TenantId 2
   - Act: Set current tenant to 1, query TenantHardware table
   - Assert: Only returns data where TenantId = 1

4. **QueryWithIgnoreFilters_ReturnsAllTenantsData()**
   - Arrange: Seed database with data for multiple tenants
   - Act: Query with .IgnoreQueryFilters()
   - Assert: Returns data for all tenants

---

### Manual Testing Checklist

**Day 4 Testing:**

- [ ] Can create a new region through UI
- [ ] Can edit region name and description
- [ ] Cannot create region with duplicate code
- [ ] Can create Head Office tenant
- [ ] Can create Factory tenant (region required)
- [ ] Can create Subsidiary tenant (region must be null)
- [ ] Cannot create factory without selecting region
- [ ] Cannot create subsidiary with region selected
- [ ] Tenant list shows all 80 tenants after seed data
- [ ] DataTables search works correctly
- [ ] Can filter tenants by type
- [ ] Can filter factories by region
- [ ] Export to Excel works
- [ ] Edit tenant form loads correctly
- [ ] Cannot change tenant code or type after creation
- [ ] Delete confirmation modal appears
- [ ] Success notifications display after save
- [ ] Error notifications display on validation failure

**Day 5 Testing:**

- [ ] Seed data creates exactly 7 regions
- [ ] Seed data creates exactly 80 tenants (1 HO + 71 factories + 9 subsidiaries + Head Office)
- [ ] All factories assigned to correct regions
- [ ] All subsidiaries have RegionId = NULL
- [ ] No duplicate tenant codes
- [ ] TenantId filtering works (query only returns current tenant's data)
- [ ] Regional ICT Manager can see all factories in their region
- [ ] Field Systems Administrator can only see their own factory
- [ ] System Admin can see all tenants using .IgnoreQueryFilters()
- [ ] Audit log captures all tenant access attempts

---

## <a name="success-criteria"></a>Success Criteria

### Week 2 Success Criteria

**Database & Schema:**
- ✅ Three tables created: Regions, Tenants, UserTenantAccess
- ✅ All foreign keys and indexes created
- ✅ Check constraints enforce business rules (factory must have region)
- ✅ Migrations run successfully without errors

**Entity Framework Core:**
- ✅ Three entity classes created with navigation properties
- ✅ Three EF Core configurations created
- ✅ Global query filter for TenantId implemented
- ✅ ApplicationDbContext configured correctly
- ✅ ITenantContext interface created

**Business Logic:**
- ✅ Repository pattern implemented for Regions and Tenants
- ✅ Service layer with validation logic
- ✅ AutoMapper profiles created
- ✅ Unit tests for service layer (80%+ coverage)

**User Interface:**
- ✅ Region management UI (Create, Edit, List)
- ✅ Tenant management UI (Create, Edit, List, Details)
- ✅ DataTables integration working
- ✅ Form validation (client and server)
- ✅ Modal confirmations working
- ✅ Export to Excel working

**Seed Data:**
- ✅ All 7 regions created
- ✅ All 71 factories created and assigned to correct regions
- ✅ All 9 subsidiaries created
- ✅ Head Office tenant created
- ✅ Total: 80 tenants in database
- ✅ No duplicate codes
- ✅ All tenants have Status = Active

**Multi-Tenancy:**
- ✅ TenantId filtering works automatically
- ✅ Tenant context resolution working
- ✅ Regional ICT Manager can access all factories in their region
- ✅ Field Systems Administrator can only access their factory
- ✅ System Admin can bypass filter with .IgnoreQueryFilters()
- ✅ No cross-tenant data leakage

**Testing:**
- ✅ 15+ unit tests passing
- ✅ 5+ integration tests passing
- ✅ Manual testing checklist completed
- ✅ All CRUD operations tested through UI
- ✅ Multi-tenant isolation tested

---

## Phase 1 Week 2 Deliverables Summary

### Database Artifacts
- ✅ SQL migration script
- ✅ Three tables: Regions, Tenants, UserTenantAccess
- ✅ Seed data for 80 tenants

### Code Artifacts
- ✅ 3 entity classes (Region, Tenant, UserTenantAccess)
- ✅ 3 EF Core configurations
- ✅ 2 repository interfaces and implementations
- ✅ 2 service interfaces and implementations
- ✅ DTOs and AutoMapper profiles
- ✅ ITenantContext interface and implementation
- ✅ Global query filter for multi-tenancy

### UI Artifacts
- ✅ 6 Razor Pages (Region Create/Edit/List, Tenant Create/Edit/List/Details)
- ✅ 6 PageModel classes
- ✅ DataTables integration
- ✅ Form validation
- ✅ Modal dialogs

### Documentation
- ✅ This implementation plan
- ✅ Database schema documentation
- ✅ Multi-tenancy architecture documentation
- ✅ Testing strategy documentation

---

## Related Documents

- **Parent Plan:** [ImplementationPlan.md](../ImplementationPlan.md)
- **Overview Document:** [0_OrganizationalStructure_Overview.md](0_OrganizationalStructure_Overview.md)
- **Next Phase:** Week 3 - Identity & Access Management (Authentication)
- **Database Schema:** [KTDA_Enhanced_Database_Schema.sql](../KTDA_Enhanced_Database_Schema.sql)
- **Technology Stack:** [TechStack.md](../TechStack.md)

---

**Document Version:** 1.0
**Last Updated:** October 30, 2025
**Estimated Implementation Time:** 5 days (1 week)
**Complexity:** Medium
**Dependencies:** Phase 0 (Environment Setup) must be completed
**Next Steps:** Proceed to Week 3 (Authentication & Authorization)

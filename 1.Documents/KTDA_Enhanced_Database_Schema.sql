-- ============================================================================
-- KTDA ICT REPORTING SYSTEM - ENHANCED DATABASE SCHEMA
-- Version: 3.0 (Enterprise-Wide Forms Edition)
-- Date: October 31, 2025
-- Database: Microsoft SQL Server 2022
-- Purpose: Complete database schema with multi-tenant, multi-workflow support
-- ============================================================================

-- ============================================================================
-- TABLE OF CONTENTS
-- ============================================================================
-- SECTION 1: ORGANIZATIONAL STRUCTURE (Multi-Tenancy) - 5 Tables
--   - Regions
--   - Tenants (HeadOffice, Factories, Subsidiaries)
--   - TenantGroups (Custom tenant groupings)
--   - TenantGroupMembers
--   - Departments (Tenant-scoped organizational units)
--
-- SECTION 2: IDENTITY & ACCESS MANAGEMENT - 6 Tables
--   - Roles
--   - Users (Enhanced with DepartmentId)
--   - UserRoles
--   - UserTenantAccess
--   - UserGroups (Training cohorts, project teams, committees)
--   - UserGroupMembers
--
-- SECTION 3: METRICS & KPI TRACKING - 3 Tables
--   - MetricDefinitions (Enhanced with SourceType, ExpectedValue, ComplianceRule)
--   - TenantMetrics (Enhanced with SourceType, SourceReferenceId)
--   - SystemMetricLogs (Hangfire job tracking)
--
-- SECTION 4: FORM TEMPLATES & SUBMISSIONS - 18 Tables
--   - FormCategories (Template categorization)
--   - FieldLibrary (Reusable field definitions)
--   - FormTemplates (Core template definition with publish workflow)
--   - FormTemplateSections (Template sections/pages)
--   - FormTemplateItems (Individual form fields/questions)
--       • FormItemOptions (Field dropdown/radio/multi-select options)
--       • FormItemConfiguration (Field-specific settings)
--       • FormItemValidations (Validation rules per field)
--       • FormItemCalculations (Auto-calculated fields)
--       • FormItemMetricMappings (Link fields to KPI metrics)
--       • MetricPopulationLog (Metric calculation audit trail)
--   - FormTemplateSubmissions (User form submissions)
--       • FormTemplateResponses (Individual field answers - EAV pattern)
--   - SubmissionWorkflowProgress (Multi-level approval tracking)
--   - WorkflowDefinitions (Reusable approval workflows)
--   - WorkflowSteps (Individual workflow approval steps)
--   - SectionRouting (Skip logic / branching)
--   - FormAnalytics (User behavior tracking)
--   - FormTemplateAssignments (8 assignment types: tenant + user based)
--
-- SECTION 5: SOFTWARE MANAGEMENT - 5 Tables
--   - SoftwareProducts
--   - SoftwareVersions
--   - SoftwareLicenses (License tracking)
--   - TenantSoftwareInstallations (Installations per tenant)
--   - SoftwareInstallationHistory (Installation audit trail)
--
-- SECTION 6: HARDWARE INVENTORY - 4 Tables
--   - HardwareCategories (Hierarchical categories)
--   - HardwareItems (Master hardware catalog)
--   - TenantHardware (Actual inventory per tenant)
--   - HardwareMaintenanceLog (Maintenance history)
--
-- SECTION 7: SUPPORT TICKETS - 3 Tables
--   - TicketCategories (Hierarchical categories with SLA)
--   - Tickets (Enhanced with external system integration)
--   - TicketComments
--
-- SECTION 8: FINANCIAL TRACKING - 3 Tables
--   - BudgetCategories (Hierarchical budget organization)
--   - TenantBudgets
--   - TenantExpenses
--
-- SECTION 9: UNIFIED NOTIFICATION SYSTEM - 8 Tables
--   - NotificationChannels (Channel configuration: Email, SMS, Push, InApp)
--   - Notifications (Central notification inbox for all notification types)
--   - NotificationRecipients (Who receives each notification)
--   - NotificationDelivery (Multi-channel delivery tracking with retry)
--   - NotificationTemplates (Reusable message templates)
--   - UserNotificationPreferences (Per-user channel & frequency settings)
--   - AlertDefinitions (Automated alert rules - creates notifications)
--   - AlertHistory (Alert trigger log with acknowledge/resolve workflow)
--
-- SECTION 10: REPORTING & ANALYTICS - 12 Tables
--   - TenantPerformanceSnapshot (Pre-aggregated metrics for dashboards)
--   - RegionalMonthlySnapshot (Regional aggregations)
--   - ReportDefinitions (User-created custom reports)
--       • ReportFields (Columns/fields to include)
--       • ReportFilters (WHERE conditions)
--       • ReportGroupings (GROUP BY logic)
--       • ReportSorting (ORDER BY logic)
--   - ReportSchedules (Automated report generation)
--   - ReportCache (Pre-generated results for performance)
--   - ReportAccessControl (Permission management)
--   - ReportExecutionLog (Audit trail of report runs)
--
-- SECTION 11: MEDIA MANAGEMENT - 3 Tables
--   - MediaFiles (Centralized file storage with deduplication)
--   - EntityMediaFiles (Polymorphic file associations)
--   - FileAccessLog (Security audit trail)
--
-- SECTION 12: AUDIT & LOGGING - 2 Tables
--   - AuditLogs (Data change tracking)
--   - UserActivityLog (User action tracking)
--
-- TOTAL: 72 TABLES
-- ============================================================================

-- Drop existing database if needed (USE WITH CAUTION)
-- DROP DATABASE IF EXISTS KTDA_ICT_Reporting;

CREATE DATABASE KTDA_ICT_Reporting;
GO

USE KTDA_ICT_Reporting;
GO

-- ============================================================================
-- SECTION 1: ORGANIZATIONAL STRUCTURE (MULTI-TENANCY ENABLED)
-- ============================================================================
-- Design: Unified tenant model supporting HeadOffice, Factories, and Subsidiaries
-- HeadOffice: Central operations, not under any region
-- Factories: Grouped under regions
-- Subsidiaries: Independent stations/units, not under regions
-- ============================================================================

-- Regions (for grouping factories only)
CREATE TABLE Regions (
    RegionId INT PRIMARY KEY IDENTITY(1,1),
    RegionNumber INT NOT NULL UNIQUE,
    RegionName NVARCHAR(100) NOT NULL,
    RegionCode NVARCHAR(20) UNIQUE NOT NULL,
    RegionalManagerUserId INT NULL, -- FK added later after Users table
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE()
);

-- Unified Tenants Table (replaces separate Factories, HeadOffice, Subsidiaries tables)
CREATE TABLE Tenants (
    TenantId INT PRIMARY KEY IDENTITY(1,1),
    TenantType NVARCHAR(20) NOT NULL CHECK (TenantType IN ('HeadOffice', 'Factory', 'Subsidiary')),
    TenantCode NVARCHAR(50) UNIQUE NOT NULL,
    TenantName NVARCHAR(200) NOT NULL,

    -- Regional association (only for Factories)
    RegionId INT NULL, -- Populated only when TenantType = 'Factory'

    -- Location details
    Location NVARCHAR(500),
    Latitude DECIMAL(10, 7),
    Longitude DECIMAL(10, 7),

    -- Contact information
    ContactPhone NVARCHAR(50),
    ContactEmail NVARCHAR(200),

    -- Management
    ManagerUserId INT NULL, -- FK added later
    ICTSupportUserId INT NULL, -- FK added later

    -- Status
    IsActive BIT DEFAULT 1,
    CreatedBy INT NULL,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME2 DEFAULT GETDATE(),

    CONSTRAINT FK_Tenant_Region FOREIGN KEY (RegionId)
        REFERENCES Regions(RegionId),
    CONSTRAINT FK_Tenant_Creator FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Tenant_Modifier FOREIGN KEY (ModifiedBy)
        REFERENCES Users(UserId),

    -- Business rule: Factories must have RegionId, HeadOffice and Subsidiaries must not
    CONSTRAINT CHK_Tenant_Region CHECK (
        (TenantType = 'Factory' AND RegionId IS NOT NULL) OR
        (TenantType IN ('HeadOffice', 'Subsidiary') AND RegionId IS NULL)
    )
);

-- Indexes for multi-tenant queries
CREATE INDEX IX_Tenants_Type ON Tenants(TenantType, IsActive) INCLUDE (TenantName);
CREATE INDEX IX_Tenants_Region ON Tenants(RegionId, IsActive) WHERE TenantType = 'Factory';
CREATE INDEX IX_Tenants_Code ON Tenants(TenantCode);
CREATE INDEX IX_Tenants_Active ON Tenants(IsActive, TenantType);

-- Tenant Groups (custom groupings for flexible e assignment)
CREATE TABLE TenantGroups (
    TenantGroupId INT PRIMARY KEY IDENTITY(1,1),
    GroupName NVARCHAR(100) NOT NULL UNIQUE,
    GroupCode NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_TenantGroup_Creator FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserId)
);

CREATE INDEX IX_TenantGroups_Active ON TenantGroups(IsActive);
CREATE INDEX IX_TenantGroups_Code ON TenantGroups(GroupCode);

-- Tenant Group Members (which tenants belong to which groups)
CREATE TABLE TenantGroupMembers (
    GroupMemberId INT PRIMARY KEY IDENTITY(1,1),
    TenantGroupId INT NOT NULL,
    TenantId INT NOT NULL,
    AddedBy INT NOT NULL,
    AddedDate DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_GroupMember_Group FOREIGN KEY (TenantGroupId)
        REFERENCES TenantGroups(TenantGroupId) ON DELETE CASCADE,
    CONSTRAINT FK_GroupMember_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId) ON DELETE CASCADE,
    CONSTRAINT FK_GroupMember_AddedBy FOREIGN KEY (AddedBy)
        REFERENCES Users(UserId),
    CONSTRAINT UQ_GroupMember_Group_Tenant UNIQUE (TenantGroupId, TenantId)
);

CREATE INDEX IX_GroupMembers_Group ON TenantGroupMembers(TenantGroupId);
CREATE INDEX IX_GroupMembers_Tenant ON TenantGroupMembers(TenantId);

-- Departments (tenant-scoped organizational units)
CREATE TABLE Departments (
    DepartmentId INT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    DepartmentName NVARCHAR(100) NOT NULL,
    DepartmentCode NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500),
    ParentDepartmentId INT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Department_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_Department_Parent FOREIGN KEY (ParentDepartmentId)
        REFERENCES Departments(DepartmentId),
    CONSTRAINT UQ_Department_Tenant_Code UNIQUE (TenantId, DepartmentCode)
);

CREATE INDEX IX_Department_Tenant ON Departments(TenantId);
CREATE INDEX IX_Department_Active ON Departments(IsActive);

-- ============================================================================
-- SECTION 2: IDENTITY & ACCESS MANAGEMENT
-- ============================================================================

-- Roles
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(100) UNIQUE NOT NULL,
    RoleCode NVARCHAR(50) UNIQUE NOT NULL,
    Description NVARCHAR(500),
    Level INT NOT NULL, -- 1=HeadOffice, 2=Regional, 3=Factory
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

-- Users (ASP.NET Core Identity compatible)
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    UserName NVARCHAR(256) UNIQUE NOT NULL,
    NormalizedUserName NVARCHAR(256) UNIQUE NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    NormalizedEmail NVARCHAR(256) NOT NULL,
    EmailConfirmed BIT DEFAULT 0,
    PasswordHash NVARCHAR(MAX),
    SecurityStamp NVARCHAR(MAX),
    PhoneNumber NVARCHAR(50),
    PhoneNumberConfirmed BIT DEFAULT 0,
    TwoFactorEnabled BIT DEFAULT 0,
    LockoutEnd DATETIMEOFFSET,
    LockoutEnabled BIT DEFAULT 1,
    AccessFailedCount INT DEFAULT 0,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    EmployeeNumber NVARCHAR(50) UNIQUE,
    DepartmentId INT NULL,
    IsActive BIT DEFAULT 1,
    LastLoginDate DATETIME2,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_User_Department FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId)
);

CREATE INDEX IX_Users_Email ON Users(NormalizedEmail);
CREATE INDEX IX_Users_Username ON Users(NormalizedUserName);
CREATE INDEX IX_User_Department ON Users(DepartmentId);

-- User Roles (Many-to-Many)
CREATE TABLE UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedDate DATETIME2 DEFAULT GETDATE(),
    AssignedBy INT,
    CONSTRAINT FK_UserRole_User FOREIGN KEY (UserId)
        REFERENCES Users(UserId),
    CONSTRAINT FK_UserRole_Role FOREIGN KEY (RoleId)
        REFERENCES Roles(RoleId),
    CONSTRAINT UQ_UserRole UNIQUE (UserId, RoleId)
);

-- User Tenant Access (Multi-tenancy)
CREATE TABLE UserTenantAccess (
    AccessId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    TenantId INT NOT NULL,
    CanRead BIT DEFAULT 1,
    CanWrite BIT DEFAULT 0,
    CanApprove BIT DEFAULT 0,
    AssignedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_UserAccess_User FOREIGN KEY (UserId)
        REFERENCES Users(UserId),
    CONSTRAINT FK_UserAccess_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT UQ_UserTenant UNIQUE (UserId, TenantId)
);

CREATE INDEX IX_UserTenantAccess_User ON UserTenantAccess(UserId);
CREATE INDEX IX_UserTenantAccess_Tenant ON UserTenantAccess(TenantId);

-- User Groups (for training cohorts, project teams, committees, etc.)
CREATE TABLE UserGroups (
    UserGroupId INT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NULL,
    GroupName NVARCHAR(100) NOT NULL,
    GroupCode NVARCHAR(50) NOT NULL UNIQUE,
    GroupType NVARCHAR(50),
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_UserGroup_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_UserGroup_Creator FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserId)
);

CREATE INDEX IX_UserGroup_Tenant ON UserGroups(TenantId);
CREATE INDEX IX_UserGroup_Code ON UserGroups(GroupCode);
CREATE INDEX IX_UserGroup_Active ON UserGroups(IsActive);

-- User Group Members (which users belong to which groups)
CREATE TABLE UserGroupMembers (
    UserGroupMemberId INT PRIMARY KEY IDENTITY(1,1),
    UserGroupId INT NOT NULL,
    UserId INT NOT NULL,
    AddedBy INT NOT NULL,
    AddedDate DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_UserGroupMember_Group FOREIGN KEY (UserGroupId)
        REFERENCES UserGroups(UserGroupId) ON DELETE CASCADE,
    CONSTRAINT FK_UserGroupMember_User FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_UserGroupMember_AddedBy FOREIGN KEY (AddedBy)
        REFERENCES Users(UserId),
    CONSTRAINT UQ_UserGroupMember_Group_User UNIQUE (UserGroupId, UserId)
);

CREATE INDEX IX_UserGroupMember_Group ON UserGroupMembers(UserGroupId);
CREATE INDEX IX_UserGroupMember_User ON UserGroupMembers(UserId);

-- ============================================================================
-- SECTION 3: METRICS & KPI TRACKING (NEW)
-- ============================================================================

-- Metric Definitions (Enhanced with source tracking and compliance rules)
CREATE TABLE MetricDefinitions (
    MetricId INT PRIMARY KEY IDENTITY(1,1),
    MetricCode NVARCHAR(50) UNIQUE NOT NULL,
    MetricName NVARCHAR(200) NOT NULL,
    Category NVARCHAR(100), -- Infrastructure, Software, Hardware, Performance, Compliance

    -- Source configuration
    SourceType NVARCHAR(30) NOT NULL, -- 'UserInput', 'SystemCalculated', 'ExternalSystem', 'ComplianceTracking', 'AutomatedCheck'

    DataType NVARCHAR(20) NOT NULL, -- Integer, Decimal, Percentage, Boolean, Text, Duration
    Unit NVARCHAR(50), -- Count, Percentage, Version, Status, Days, Hours
    AggregationType NVARCHAR(20), -- SUM, AVG, MAX, MIN, LAST_VALUE, COUNT

    -- KPI thresholds
    IsKPI BIT DEFAULT 0,
    ThresholdGreen DECIMAL(18,4), -- Target/Good
    ThresholdYellow DECIMAL(18,4), -- Warning
    ThresholdRed DECIMAL(18,4), -- Critical

    -- Expected value for binary/compliance metrics
    ExpectedValue NVARCHAR(100), -- 'TRUE', 'Yes', '100%', etc.

    -- Compliance rules (JSON for deadline tracking, validation rules)
    ComplianceRule NVARCHAR(MAX), -- JSON: {"type": "deadline", "daysAfterPeriodEnd": 2}

    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    
    -- Constraints
    CONSTRAINT CK_Metric_SourceType CHECK (
        SourceType IN ('UserInput', 'SystemCalculated', 'ExternalSystem', 'ComplianceTracking', 'AutomatedCheck')
    ),
    CONSTRAINT CK_Metric_DataType CHECK (
        DataType IN ('Integer', 'Decimal', 'Percentage', 'Boolean', 'Text', 'Duration', 'Date', 'DateTime')
    ),
    CONSTRAINT CK_Metric_Unit CHECK (
        Unit IS NULL OR Unit IN ('Count', 'Percentage', 'Version', 'Status', 'Days', 'Hours', 'Minutes', 'Seconds', 'GB', 'MB', 'KB', 'TB', 'Bytes', 'None')
    ),
    CONSTRAINT CK_Metric_AggregationType CHECK (
        AggregationType IS NULL OR AggregationType IN ('SUM', 'AVG', 'MAX', 'MIN', 'LAST_VALUE', 'COUNT', 'NONE')
    )
);

CREATE INDEX IX_Metrics_Category ON MetricDefinitions(Category, IsKPI, IsActive);
CREATE INDEX IX_Metrics_SourceType ON MetricDefinitions(SourceType, IsActive);

-- Tenant Metrics (Time-Series Data for all tenants with source tracking)
CREATE TABLE TenantMetrics (
    MetricValueId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    MetricId INT NOT NULL,
    ReportingPeriod DATE NOT NULL,
    NumericValue DECIMAL(18,4),
    TextValue NVARCHAR(MAX),

    -- Track source of metric value
    SourceType NVARCHAR(30), -- 'UserInput', 'SystemCalculated', 'HangfireJob', 'ExternalAPI'
    SourceReferenceId INT, -- SubmissionId if from form, LogId if from SystemMetricLogs
    
    CONSTRAINT CK_TenantMetric_SourceType CHECK (
        SourceType IS NULL OR SourceType IN ('UserInput', 'SystemCalculated', 'HangfireJob', 'ExternalAPI', 'Manual', 'Import')
    ),

    CapturedDate DATETIME2 DEFAULT GETDATE(),
    CapturedBy INT,
    CONSTRAINT FK_TenantMetrics_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_TenantMetrics_Metric FOREIGN KEY (MetricId)
        REFERENCES MetricDefinitions(MetricId),
    CONSTRAINT UQ_TenantMetricPeriod UNIQUE (TenantId, MetricId, ReportingPeriod)
);

-- Critical for time-series queries
CREATE INDEX IX_TenantMetrics_TimeSeries
    ON TenantMetrics(ReportingPeriod DESC, TenantId, MetricId)
    INCLUDE (NumericValue, TextValue);

CREATE INDEX IX_TenantMetrics_Tenant
    ON TenantMetrics(TenantId, MetricId, ReportingPeriod DESC);

CREATE INDEX IX_TenantMetrics_Period
    ON TenantMetrics(ReportingPeriod DESC) INCLUDE (TenantId, NumericValue);

CREATE INDEX IX_TenantMetrics_Source
    ON TenantMetrics(SourceType, SourceReferenceId);

-- System Metric Logs (for Hangfire jobs and automated checks)
CREATE TABLE SystemMetricLogs (
    LogId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    MetricId INT NOT NULL,
    CheckDate DATETIME2 NOT NULL,
    Status NVARCHAR(20), -- 'Success', 'Failed', 'Warning'
    NumericValue DECIMAL(18,4),
    TextValue NVARCHAR(MAX),
    Details NVARCHAR(MAX), -- JSON with additional info
    JobName NVARCHAR(100),
    ExecutionDuration INT, -- milliseconds
    ErrorMessage NVARCHAR(MAX),
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_SystemMetricLog_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_SystemMetricLog_Metric FOREIGN KEY (MetricId)
        REFERENCES MetricDefinitions(MetricId)
);

CREATE INDEX IX_SystemMetricLogs_Tenant_Date
    ON SystemMetricLogs(TenantId, CheckDate DESC);

CREATE INDEX IX_SystemMetricLogs_Metric_Date
    ON SystemMetricLogs(MetricId, CheckDate DESC);

CREATE INDEX IX_SystemMetricLogs_Status
    ON SystemMetricLogs(Status, CheckDate DESC);

-- ============================================================================
-- SECTION 4: FORM TEMPLATES & SUBMISSIONS (DYNAMIC FORM BUILDER)
-- ============================================================================

-- Form Categories (groups templates by operational area)
CREATE TABLE FormCategories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    CategoryCode NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    DisplayOrder INT DEFAULT 0,
    IconClass NVARCHAR(50), -- e.g., 'fa-network-wired', 'fa-server', 'fa-desktop'
    Color NVARCHAR(20), -- For UI visual grouping
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE()
);

CREATE INDEX IX_Categories_Active ON FormCategories(IsActive, DisplayOrder);

-- Field Library (reusable field definitions across templates)
CREATE TABLE FieldLibrary (
    LibraryFieldId INT PRIMARY KEY IDENTITY(1,1),
    FieldName NVARCHAR(200) NOT NULL,
    FieldCode NVARCHAR(50) UNIQUE NOT NULL,
    FieldType NVARCHAR(50) NOT NULL, -- Text, Number, Boolean, Date, Dropdown, etc.
    Category NVARCHAR(100), -- 'Common', 'HR', 'ICT', 'Finance'
    Description NVARCHAR(1000),
    DefaultConfiguration NVARCHAR(MAX), -- JSON: complete field setup (validations, options, etc.)
    IsActive BIT DEFAULT 1,
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_LibraryField_Creator FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserId)
);

CREATE INDEX IX_FieldLibrary_Category ON FieldLibrary(Category, IsActive);
CREATE INDEX IX_FieldLibrary_Type ON FieldLibrary(FieldType, IsActive);
CREATE INDEX IX_FieldLibrary_Code ON FieldLibrary(FieldCode);

-- ============================================================================
-- 1. FORM CATEGORIES (Template Categorization)
-- ============================================================================

-- (FormCategories is already above, keeping this as section marker)

-- ============================================================================
-- 2. FORM TEMPLATES (Core Template Definition)
-- ============================================================================

-- Form Templates
CREATE TABLE FormTemplates (
    TemplateId INT PRIMARY KEY IDENTITY(1,1),
    CategoryId INT NOT NULL,
    TemplateName NVARCHAR(200) NOT NULL,
    TemplateCode NVARCHAR(50) UNIQUE NOT NULL,
    Description NVARCHAR(1000),
    TemplateType NVARCHAR(50) NOT NULL, -- Daily, Weekly, Monthly, Quarterly, Annual
    Version INT DEFAULT 1,
    IsActive BIT DEFAULT 1,
    RequiresApproval BIT DEFAULT 1,
    WorkflowId INT NULL, -- Link to approval workflow
    
    -- Publish Status Workflow
    PublishStatus NVARCHAR(20) DEFAULT 'Draft' NOT NULL, -- Draft, Published, Archived, Deprecated
    PublishedDate DATETIME2 NULL,
    PublishedBy INT NULL,
    ArchivedDate DATETIME2 NULL,
    ArchivedBy INT NULL,
    ArchivedReason NVARCHAR(500) NULL,
    
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Template_Category FOREIGN KEY (CategoryId)
        REFERENCES FormCategories(CategoryId),
    CONSTRAINT FK_Template_Workflow FOREIGN KEY (WorkflowId)
        REFERENCES WorkflowDefinitions(WorkflowId),
    CONSTRAINT FK_Template_Creator FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Template_Modifier FOREIGN KEY (ModifiedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Template_Publisher FOREIGN KEY (PublishedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Template_Archiver FOREIGN KEY (ArchivedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_Template_Approval CHECK (
        (RequiresApproval = 0 AND WorkflowId IS NULL) OR
        (RequiresApproval = 1 AND WorkflowId IS NOT NULL) OR
        (PublishStatus = 'Draft') -- Drafts can have incomplete workflow config
    ),
    CONSTRAINT CK_Template_PublishStatus CHECK (
        PublishStatus IN ('Draft', 'Published', 'Archived', 'Deprecated')
    )
);

CREATE INDEX IX_Templates_Category ON FormTemplates(CategoryId, IsActive);
CREATE INDEX IX_Template_Workflow ON FormTemplates(WorkflowId);
CREATE INDEX IX_Templates_PublishStatus ON FormTemplates(PublishStatus, IsActive);

-- ============================================================================
-- 3. FORM TEMPLATE SECTIONS (Template Pages/Grouping)
-- ============================================================================

-- Form Template Sections (groups questions within a template)
CREATE TABLE FormTemplateSections (
    SectionId INT PRIMARY KEY IDENTITY(1,1),
    TemplateId INT NOT NULL,
    SectionName NVARCHAR(100) NOT NULL,
    SectionDescription NVARCHAR(500),
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsCollapsible BIT DEFAULT 1,
    IsCollapsedByDefault BIT DEFAULT 0,
    IconClass NVARCHAR(50), -- e.g., 'fa-desktop', 'fa-network-wired', 'fa-cube'
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_TemplateSection_Template FOREIGN KEY (TemplateId)
        REFERENCES FormTemplates(TemplateId) ON DELETE CASCADE,
    CONSTRAINT UQ_TemplateSection_Name UNIQUE (TemplateId, SectionName)
);

CREATE INDEX IX_TemplateSections_Template ON FormTemplateSections(TemplateId, DisplayOrder);

-- ============================================================================
-- 4. FORM TEMPLATE ITEMS & RELATED CONFIGURATION
-- ============================================================================

-- Form Template Items (individual fields/questions within sections)
CREATE TABLE FormTemplateItems (
    ItemId INT PRIMARY KEY IDENTITY(1,1),
    TemplateId INT NOT NULL,
    SectionId INT NOT NULL,
    ItemCode NVARCHAR(50) NOT NULL,
    ItemName NVARCHAR(200) NOT NULL,
    ItemDescription NVARCHAR(1000),
    DisplayOrder INT DEFAULT 0,
    DataType NVARCHAR(50), -- Text, Number, Boolean, Date, Dropdown, TextArea, Rating, MultiSelect, FileUpload, Signature
    IsRequired BIT DEFAULT 0,
    DefaultValue NVARCHAR(500),
    
    -- UI Enhancement Fields
    PlaceholderText NVARCHAR(200) NULL, -- Hint text shown in empty field
    HelpText NVARCHAR(1000) NULL, -- Explanation text below field
    PrefixText NVARCHAR(50) NULL, -- Text before input (e.g., "$", "KES")
    SuffixText NVARCHAR(50) NULL, -- Text after input (e.g., "%", "kg")
    
    -- Conditional Logic
    ConditionalLogic NVARCHAR(MAX) NULL, -- JSON: {"action": "show", "rules": [{"itemId": 45, "operator": "equals", "value": "Yes"}]}
    
    -- Matrix/Grid Layout Support
    LayoutType NVARCHAR(30) DEFAULT 'Single', -- Single, Matrix, Grid, Inline
    MatrixGroupId INT NULL, -- Groups items in same matrix
    MatrixRowLabel NVARCHAR(200) NULL, -- Row label in matrix layout
    
    -- Field Library Integration
    LibraryFieldId INT NULL, -- Link to reusable field definition
    IsLibraryOverride BIT DEFAULT 0, -- True if admin customized a library field
    
    Version INT DEFAULT 1,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_TemplateItem_Template FOREIGN KEY (TemplateId)
        REFERENCES FormTemplates(TemplateId),
    CONSTRAINT FK_TemplateItem_Section FOREIGN KEY (SectionId)
        REFERENCES FormTemplateSections(SectionId) ON DELETE NO ACTION,
    CONSTRAINT FK_Item_LibraryField FOREIGN KEY (LibraryFieldId)
        REFERENCES FieldLibrary(LibraryFieldId),
    CONSTRAINT UQ_TemplateItem UNIQUE (TemplateId, ItemCode, Version),
    CONSTRAINT CK_Item_LayoutType CHECK (LayoutType IN ('Single', 'Matrix', 'Grid', 'Inline'))
);

CREATE INDEX IX_TemplateItems_Template ON FormTemplateItems(TemplateId, DisplayOrder, IsActive);
CREATE INDEX IX_TemplateItems_Section ON FormTemplateItems(SectionId, DisplayOrder);
CREATE INDEX IX_TemplateItems_Library ON FormTemplateItems(LibraryFieldId);
CREATE INDEX IX_TemplateItems_Layout ON FormTemplateItems(LayoutType, MatrixGroupId);

-- Form Item Options (dropdown/multi-select/radio options)
CREATE TABLE FormItemOptions (
    OptionId INT PRIMARY KEY IDENTITY(1,1),
    ItemId INT NOT NULL,
    OptionValue NVARCHAR(200) NOT NULL,
    OptionLabel NVARCHAR(200) NOT NULL,
    DisplayOrder INT DEFAULT 0,
    IsDefault BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    ParentOptionId INT NULL, -- For cascading dropdowns
    CONSTRAINT FK_ItemOption_Item FOREIGN KEY (ItemId)
        REFERENCES FormTemplateItems(ItemId) ON DELETE CASCADE,
    CONSTRAINT FK_ItemOption_Parent FOREIGN KEY (ParentOptionId)
        REFERENCES FormItemOptions(OptionId),
    CONSTRAINT UQ_ItemOption UNIQUE (ItemId, OptionValue)
);

CREATE INDEX IX_ItemOptions_Item ON FormItemOptions(ItemId, DisplayOrder, IsActive);
CREATE INDEX IX_ItemOptions_Parent ON FormItemOptions(ParentOptionId);

-- Form Item Configuration (field-specific settings like min/max, file types, etc.)
CREATE TABLE FormItemConfiguration (
    ConfigId INT PRIMARY KEY IDENTITY(1,1),
    ItemId INT NOT NULL,
    ConfigKey NVARCHAR(100) NOT NULL, -- 'minValue', 'maxValue', 'allowedFileTypes', 'ratingMax', etc.
    ConfigValue NVARCHAR(500),
    CONSTRAINT FK_ItemConfig_Item FOREIGN KEY (ItemId)
        REFERENCES FormTemplateItems(ItemId) ON DELETE CASCADE,
    CONSTRAINT UQ_ItemConfig UNIQUE (ItemId, ConfigKey)
);

CREATE INDEX IX_ItemConfig_Item ON FormItemConfiguration(ItemId);
CREATE INDEX IX_ItemConfig_Key ON FormItemConfiguration(ConfigKey);

-- Form Item Validations (self-contained validation rules per field)
CREATE TABLE FormItemValidations (
    ItemValidationId INT PRIMARY KEY IDENTITY(1,1),
    ItemId INT NOT NULL,
    ValidationType NVARCHAR(50) NOT NULL, -- Type of validation to apply
    
    -- Inline validation parameters (no external rule table needed)
    MinValue DECIMAL(18,4) NULL, -- For numeric range validation
    MaxValue DECIMAL(18,4) NULL, -- For numeric range validation
    MinLength INT NULL, -- For text length validation
    MaxLength INT NULL, -- For text length validation
    RegexPattern NVARCHAR(500) NULL, -- For pattern matching
    CustomExpression NVARCHAR(MAX) NULL, -- For complex custom validation logic
    
    ValidationOrder INT DEFAULT 0, -- Order of validation execution
    ErrorMessage NVARCHAR(500) NOT NULL, -- Error message to display
    Severity NVARCHAR(20) DEFAULT 'Error', -- Error, Warning, Info
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_ItemValidation_Item FOREIGN KEY (ItemId)
        REFERENCES FormTemplateItems(ItemId) ON DELETE CASCADE,
    CONSTRAINT CK_ValidationType CHECK (
        ValidationType IN ('Required', 'Email', 'Phone', 'URL', 'Range', 
                          'MinLength', 'MaxLength', 'Pattern', 'Custom', 'CrossField', 
                          'Date', 'Number', 'Integer', 'Decimal')
    ),
    CONSTRAINT CK_ValidationSeverity CHECK (Severity IN ('Error', 'Warning', 'Info'))
);

CREATE INDEX IX_ItemValidations_Item ON FormItemValidations(ItemId, ValidationOrder, IsActive);
CREATE INDEX IX_ItemValidations_Type ON FormItemValidations(ValidationType, IsActive);

-- Form Item Calculations (auto-calculated fields based on other field values)
CREATE TABLE FormItemCalculations (
    CalculationId INT PRIMARY KEY IDENTITY(1,1),
    TargetItemId INT NOT NULL, -- Field that displays calculated result
    CalculationFormula NVARCHAR(MAX) NOT NULL, -- JSON: {"formula": "(item1 * item2)", "sourceItems": [1, 2], "roundTo": 2}
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_ItemCalc_Target FOREIGN KEY (TargetItemId)
        REFERENCES FormTemplateItems(ItemId) ON DELETE CASCADE
);

CREATE INDEX IX_ItemCalculations_Target ON FormItemCalculations(TargetItemId, IsActive);

-- Form Item Metric Mappings (links template items to metrics for automatic KPI tracking)
CREATE TABLE FormItemMetricMappings (
    MappingId INT PRIMARY KEY IDENTITY(1,1),
    ItemId INT NOT NULL,
    MetricId INT NOT NULL,

    -- Mapping types
    MappingType NVARCHAR(30) NOT NULL, -- 'Direct', 'Calculated', 'BinaryCompliance', 'Derived'

    -- For calculated metrics (e.g., availability% = operational/total * 100)
    TransformationLogic NVARCHAR(MAX), -- JSON: {"formula": "(item21 / item20) * 100", "items": [21, 20]}

    -- For binary compliance metrics (e.g., Is LAN working? Expected: Yes)
    ExpectedValue NVARCHAR(100), -- 'TRUE', 'Yes', '100%', etc.

    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT FK_ItemMetricMap_Item FOREIGN KEY (ItemId)
        REFERENCES FormTemplateItems(ItemId) ON DELETE CASCADE,
    CONSTRAINT FK_ItemMetricMap_Metric FOREIGN KEY (MetricId)
        REFERENCES MetricDefinitions(MetricId),
    CONSTRAINT UQ_ItemMetricMap UNIQUE (ItemId, MetricId)
);

CREATE INDEX IX_ItemMetricMap_Item ON FormItemMetricMappings(ItemId, IsActive);
CREATE INDEX IX_ItemMetricMap_Metric ON FormItemMetricMappings(MetricId, IsActive);
CREATE INDEX IX_ItemMetricMap_Type ON FormItemMetricMappings(MappingType, IsActive);

-- Metric Population Log (tracks auto-population of metrics from form submissions)
CREATE TABLE MetricPopulationLog (
    LogId BIGINT PRIMARY KEY IDENTITY(1,1),
    SubmissionId INT NOT NULL,
    MetricId INT NOT NULL,
    MappingId INT NOT NULL,
    SourceItemId INT NOT NULL,
    
    -- Source and calculated values
    SourceValue NVARCHAR(MAX), -- Raw value from form response
    CalculatedValue DECIMAL(18,4), -- Final calculated value for metric
    CalculationFormula NVARCHAR(MAX), -- Audit trail of calculation used
    
    -- Processing metadata
    PopulatedDate DATETIME2 DEFAULT GETUTCDATE(),
    PopulatedBy INT NULL, -- NULL if system/automated, UserId if manual override
    Status NVARCHAR(20) NOT NULL, -- Success, Failed, Skipped, Pending
    ErrorMessage NVARCHAR(500),
    ProcessingTimeMs INT, -- Performance tracking
    
    CONSTRAINT FK_MetricLog_Submission FOREIGN KEY (SubmissionId)
        REFERENCES FormTemplateSubmissions(SubmissionId),
    CONSTRAINT FK_MetricLog_Metric FOREIGN KEY (MetricId)
        REFERENCES MetricDefinitions(MetricId),
    CONSTRAINT FK_MetricLog_Mapping FOREIGN KEY (MappingId)
        REFERENCES FormItemMetricMappings(MappingId),
    CONSTRAINT FK_MetricLog_Item FOREIGN KEY (SourceItemId)
        REFERENCES FormTemplateItems(ItemId),
    CONSTRAINT FK_MetricLog_User FOREIGN KEY (PopulatedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_MetricLog_Status CHECK (
        Status IN ('Success', 'Failed', 'Skipped', 'Pending')
    )
);

CREATE INDEX IX_MetricLog_Submission ON MetricPopulationLog(SubmissionId, Status);
CREATE INDEX IX_MetricLog_Date ON MetricPopulationLog(PopulatedDate DESC);
CREATE INDEX IX_MetricLog_Status ON MetricPopulationLog(Status, PopulatedDate DESC);
CREATE INDEX IX_MetricLog_Metric ON MetricPopulationLog(MetricId, PopulatedDate DESC);

-- ============================================================================
-- 5. FORM TEMPLATE SUBMISSIONS & RESPONSES
-- ============================================================================

-- Form Template Submissions (instances of template completion)
CREATE TABLE FormTemplateSubmissions (
    SubmissionId INT PRIMARY KEY IDENTITY(1,1),
    TemplateId INT NOT NULL,
    TenantId INT NULL, -- Nullable for non-location forms (appraisals, training feedback)
    ReportingYear INT NOT NULL,
    ReportingMonth TINYINT NOT NULL,
    ReportingPeriod DATE NOT NULL,
    SnapshotDate DATE NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft', -- Draft, Submitted, InApproval, Approved, Rejected
    SubmittedBy INT NOT NULL,
    SubmittedDate DATETIME2,
    ReviewedBy INT, -- For simple single-level approvals
    ReviewedDate DATETIME2,
    ApprovalComments NVARCHAR(MAX),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_TemplateSubmission_Template FOREIGN KEY (TemplateId)
        REFERENCES FormTemplates(TemplateId),
    CONSTRAINT FK_TemplateSubmission_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_TemplateSubmission_Submitter FOREIGN KEY (SubmittedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_TemplateSubmission_Reviewer FOREIGN KEY (ReviewedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_TemplateSubmission_Modifier FOREIGN KEY (ModifiedBy)
        REFERENCES Users(UserId)
);

CREATE INDEX IX_TemplateSubmissions_TimeSeries
    ON FormTemplateSubmissions(ReportingPeriod DESC, TenantId, TemplateId)
    INCLUDE (Status, SubmittedBy);

CREATE INDEX IX_TemplateSubmissions_Status
    ON FormTemplateSubmissions(Status, TenantId);

CREATE INDEX IX_TemplateSubmissions_Tenant_Recent
    ON FormTemplateSubmissions(TenantId, ReportingPeriod DESC)
    WHERE Status IN ('Submitted', 'Approved');

CREATE INDEX IX_Submission_User_Period
    ON FormTemplateSubmissions(SubmittedBy, ReportingPeriod);

-- Filtered unique indexes for location-based vs user-based forms
CREATE UNIQUE INDEX IX_Submission_Location_Unique
    ON FormTemplateSubmissions(TenantId, TemplateId, ReportingPeriod)
    WHERE TenantId IS NOT NULL AND Status <> 'Draft';

CREATE UNIQUE INDEX IX_Submission_User_Unique
    ON FormTemplateSubmissions(SubmittedBy, TemplateId, ReportingPeriod)
    WHERE TenantId IS NULL AND Status <> 'Draft';

-- Form Template Responses (EAV Pattern for flexible data storage)
CREATE TABLE FormTemplateResponses (
    ResponseId BIGINT PRIMARY KEY IDENTITY(1,1),
    SubmissionId INT NOT NULL,
    ItemId INT NOT NULL,
    TextValue NVARCHAR(MAX),
    NumericValue DECIMAL(18,4),
    DateValue DATE,
    BooleanValue BIT,
    Remarks NVARCHAR(1000),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_TemplateResponse_Submission FOREIGN KEY (SubmissionId)
        REFERENCES FormTemplateSubmissions(SubmissionId) ON DELETE CASCADE,
    CONSTRAINT FK_TemplateResponse_Item FOREIGN KEY (ItemId)
        REFERENCES FormTemplateItems(ItemId),
    CONSTRAINT UQ_SubmissionItem UNIQUE (SubmissionId, ItemId)
);

CREATE INDEX IX_TemplateResponses_Submission ON FormTemplateResponses(SubmissionId);

-- NOTE: Response Attachments now handled by Section 13: Media Management
-- Use MediaFiles + EntityMediaFiles(EntityType='FormResponse', ResponseId=...)

-- Submission Workflow Progress (track multi-level approval progress)
CREATE TABLE SubmissionWorkflowProgress (
    ProgressId INT PRIMARY KEY IDENTITY(1,1),
    SubmissionId INT NOT NULL,
    StepId INT NOT NULL,
    StepOrder INT NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending', -- Pending, Approved, Rejected, Skipped
    ReviewedBy INT NULL,
    ReviewedDate DATETIME2 NULL,
    Comments NVARCHAR(MAX),
    
    -- Delegation and due date tracking
    DueDate DATETIME2 NULL, -- Calculated from submission date + WorkflowSteps.DueDays
    DelegatedTo INT NULL, -- User who received delegated approval authority
    DelegatedDate DATETIME2 NULL, -- When delegation occurred
    DelegatedBy INT NULL, -- Original approver who delegated
    
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Progress_Submission FOREIGN KEY (SubmissionId)
        REFERENCES FormTemplateSubmissions(SubmissionId) ON DELETE CASCADE,
    CONSTRAINT FK_Progress_Step FOREIGN KEY (StepId)
        REFERENCES WorkflowSteps(StepId),
    CONSTRAINT FK_Progress_Reviewer FOREIGN KEY (ReviewedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Progress_DelegatedTo FOREIGN KEY (DelegatedTo)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Progress_DelegatedBy FOREIGN KEY (DelegatedBy)
        REFERENCES Users(UserId),
    CONSTRAINT UQ_Progress_Submission_Step UNIQUE (SubmissionId, StepId)
);

CREATE INDEX IX_Progress_Submission_Order ON SubmissionWorkflowProgress(SubmissionId, StepOrder);
CREATE INDEX IX_Progress_Status ON SubmissionWorkflowProgress(Status);
CREATE INDEX IX_Progress_DueDate ON SubmissionWorkflowProgress(DueDate) WHERE Status = 'Pending';
CREATE INDEX IX_Progress_Delegated ON SubmissionWorkflowProgress(DelegatedTo) WHERE DelegatedTo IS NOT NULL;

-- Workflow Definitions (multi-level approval workflows)
CREATE TABLE WorkflowDefinitions (
    WorkflowId INT PRIMARY KEY IDENTITY(1,1),
    WorkflowName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Workflow_Creator FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserId)
);

CREATE INDEX IX_Workflow_Active ON WorkflowDefinitions(IsActive);

-- Workflow Steps (individual approval steps within a workflow)
CREATE TABLE WorkflowSteps (
    StepId INT PRIMARY KEY IDENTITY(1,1),
    WorkflowId INT NOT NULL,
    StepOrder INT NOT NULL,
    StepName NVARCHAR(100) NOT NULL,
    ApproverRoleId INT NULL,
    ApproverUserId INT NULL,
    IsMandatory BIT DEFAULT 1,
    ConditionLogic NVARCHAR(MAX),
    
    -- Advanced workflow features
    IsParallel BIT DEFAULT 0, -- Can this step run in parallel with other steps of same StepOrder?
    DueDays INT NULL, -- Days to complete step from submission date
    EscalationRoleId INT NULL, -- Escalate to this role if overdue
    AutoApproveCondition NVARCHAR(MAX) NULL, -- JSON: {"field": "amount", "operator": "<", "value": 1000}
    
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_WorkflowStep_Workflow FOREIGN KEY (WorkflowId)
        REFERENCES WorkflowDefinitions(WorkflowId) ON DELETE CASCADE,
    CONSTRAINT FK_WorkflowStep_Role FOREIGN KEY (ApproverRoleId)
        REFERENCES Roles(RoleId),
    CONSTRAINT FK_WorkflowStep_User FOREIGN KEY (ApproverUserId)
        REFERENCES Users(UserId),
    CONSTRAINT FK_WorkflowStep_EscalationRole FOREIGN KEY (EscalationRoleId)
        REFERENCES Roles(RoleId),
    CONSTRAINT CK_WorkflowStep_Approver CHECK (
        (ApproverRoleId IS NOT NULL AND ApproverUserId IS NULL) OR
        (ApproverRoleId IS NULL AND ApproverUserId IS NOT NULL)
    ),
    CONSTRAINT UQ_WorkflowStep_Order UNIQUE (WorkflowId, StepOrder)
);

CREATE INDEX IX_WorkflowStep_Workflow ON WorkflowSteps(WorkflowId, StepOrder);
CREATE INDEX IX_WorkflowStep_Escalation ON WorkflowSteps(EscalationRoleId);

-- ============================================================================
-- SUPPORTING TABLES (Section Routing, Analytics, Field Library)
-- ============================================================================

-- Section Routing (skip logic - jump to different sections based on answers)
CREATE TABLE SectionRouting (
    RoutingId INT PRIMARY KEY IDENTITY(1,1),
    SourceSectionId INT NOT NULL,
    SourceItemId INT NOT NULL, -- Question that triggers routing
    TargetSectionId INT NULL, -- NULL = end form (go to submit)
    ConditionType NVARCHAR(20) NOT NULL, -- 'equals', 'not_equals', 'contains', 'greater_than', 'less_than', 'is_empty'
    ConditionValue NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_Routing_SourceSection FOREIGN KEY (SourceSectionId)
        REFERENCES FormTemplateSections(SectionId),
    CONSTRAINT FK_Routing_SourceItem FOREIGN KEY (SourceItemId)
        REFERENCES FormTemplateItems(ItemId),
    CONSTRAINT FK_Routing_TargetSection FOREIGN KEY (TargetSectionId)
        REFERENCES FormTemplateSections(SectionId),
    CONSTRAINT CK_Routing_Condition CHECK (
        ConditionType IN ('equals', 'not_equals', 'contains', 'greater_than', 'less_than', 'is_empty')
    )
);

CREATE INDEX IX_SectionRouting_Source ON SectionRouting(SourceSectionId, IsActive);
CREATE INDEX IX_SectionRouting_Item ON SectionRouting(SourceItemId, IsActive);
CREATE INDEX IX_SectionRouting_Target ON SectionRouting(TargetSectionId);

-- Form Analytics (track user behavior and form performance)
CREATE TABLE FormAnalytics (
    AnalyticId BIGINT PRIMARY KEY IDENTITY(1,1),
    TemplateId INT NOT NULL,
    UserId INT NOT NULL,
    SubmissionId INT NULL, -- NULL if form abandoned before creating submission
    EventType NVARCHAR(50) NOT NULL, -- 'FormOpened', 'SectionStarted', 'SectionCompleted', 'FieldFilled', 'FormAbandoned', 'FormSubmitted', 'FormSaved'
    EventData NVARCHAR(MAX), -- JSON with context: {"sectionId": 5, "timeSpentSeconds": 45}
    EventDate DATETIME2 DEFAULT GETUTCDATE(),
    SessionId NVARCHAR(100), -- Browser session to track single user journey
    CONSTRAINT FK_Analytics_Template FOREIGN KEY (TemplateId)
        REFERENCES FormTemplates(TemplateId),
    CONSTRAINT FK_Analytics_User FOREIGN KEY (UserId)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Analytics_Submission FOREIGN KEY (SubmissionId)
        REFERENCES FormTemplateSubmissions(SubmissionId),
    CONSTRAINT CK_Analytics_EventType CHECK (
        EventType IN ('FormOpened', 'SectionStarted', 'SectionCompleted', 'FieldFilled', 'FormAbandoned', 'FormSubmitted', 'FormSaved')
    )
);

CREATE INDEX IX_Analytics_Template_Date ON FormAnalytics(TemplateId, EventDate DESC);
CREATE INDEX IX_Analytics_User_Date ON FormAnalytics(UserId, EventDate DESC);
CREATE INDEX IX_Analytics_Session ON FormAnalytics(SessionId, EventDate);
CREATE INDEX IX_Analytics_EventType ON FormAnalytics(EventType, EventDate DESC);
CREATE INDEX IX_Analytics_Submission ON FormAnalytics(SubmissionId);

-- ============================================================================
-- 6. FORM TEMPLATE ASSIGNMENTS (Access Control - Who Can Use Forms)
-- ============================================================================

-- Form Template Assignments (8 assignment types: tenant-based + user-based)
CREATE TABLE FormTemplateAssignments (
    AssignmentId INT PRIMARY KEY IDENTITY(1,1),
    TemplateId INT NOT NULL,

    -- Assignment Type (8 types total)
    AssignmentType NVARCHAR(50) NOT NULL,
    -- Tenant-based: 'All', 'TenantType', 'TenantGroup', 'SpecificTenant'
    -- User-based: 'Role', 'Department', 'UserGroup', 'SpecificUser'

    -- Tenant-based assignment targets
    TenantType NVARCHAR(20) NULL,
    TenantGroupId INT NULL,
    TenantId INT NULL,

    -- User-based assignment targets
    RoleId INT NULL,
    DepartmentId INT NULL,
    UserGroupId INT NULL,
    UserId INT NULL,

    -- Metadata
    AssignedBy INT NOT NULL,
    AssignedDate DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT DEFAULT 1,
    Notes NVARCHAR(500),

    CONSTRAINT FK_TemplateAssignment_Template FOREIGN KEY (TemplateId)
        REFERENCES FormTemplates(TemplateId) ON DELETE CASCADE,
    CONSTRAINT FK_TemplateAssignment_TenantGroup FOREIGN KEY (TenantGroupId)
        REFERENCES TenantGroups(TenantGroupId),
    CONSTRAINT FK_TemplateAssignment_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_TemplateAssignment_Role FOREIGN KEY (RoleId)
        REFERENCES Roles(RoleId),
    CONSTRAINT FK_TemplateAssignment_Department FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId),
    CONSTRAINT FK_TemplateAssignment_UserGroup FOREIGN KEY (UserGroupId)
        REFERENCES UserGroups(UserGroupId),
    CONSTRAINT FK_TemplateAssignment_User FOREIGN KEY (UserId)
        REFERENCES Users(UserId),
    CONSTRAINT FK_TemplateAssignment_AssignedBy FOREIGN KEY (AssignedBy)
        REFERENCES Users(UserId),

    CONSTRAINT CK_TemplateAssignment_Type CHECK (
        AssignmentType IN ('All', 'TenantType', 'TenantGroup', 'SpecificTenant',
                           'Role', 'Department', 'UserGroup', 'SpecificUser')
    ),

    -- Business rule: Exactly one assignment target must be set based on type
    CONSTRAINT CK_TemplateAssignment_Target CHECK (
        -- Tenant-based (4 types)
        (AssignmentType = 'All'
         AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL
         AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR
        (AssignmentType = 'TenantType'
         AND TenantType IS NOT NULL
         AND TenantGroupId IS NULL AND TenantId IS NULL
         AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR
        (AssignmentType = 'TenantGroup'
         AND TenantGroupId IS NOT NULL
         AND TenantType IS NULL AND TenantId IS NULL
         AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR
        (AssignmentType = 'SpecificTenant'
         AND TenantId IS NOT NULL
         AND TenantType IS NULL AND TenantGroupId IS NULL
         AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR
        -- User-based (4 types)
        (AssignmentType = 'Role'
         AND RoleId IS NOT NULL
         AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL
         AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR
        (AssignmentType = 'Department'
         AND DepartmentId IS NOT NULL
         AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL
         AND RoleId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR
        (AssignmentType = 'UserGroup'
         AND UserGroupId IS NOT NULL
         AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL
         AND RoleId IS NULL AND DepartmentId IS NULL AND UserId IS NULL) OR
        (AssignmentType = 'SpecificUser'
         AND UserId IS NOT NULL
         AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL
         AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL)
    )
);

CREATE INDEX IX_TemplateAssignments_Template ON FormTemplateAssignments(TemplateId, IsActive);
CREATE INDEX IX_TemplateAssignments_Type ON FormTemplateAssignments(AssignmentType, IsActive);
CREATE INDEX IX_TemplateAssignments_TenantType ON FormTemplateAssignments(TenantType, IsActive);
CREATE INDEX IX_TemplateAssignments_TenantGroup ON FormTemplateAssignments(TenantGroupId, IsActive);
CREATE INDEX IX_TemplateAssignments_Tenant ON FormTemplateAssignments(TenantId, IsActive);
CREATE INDEX IX_TemplateAssignment_Role ON FormTemplateAssignments(RoleId);
CREATE INDEX IX_TemplateAssignment_Department ON FormTemplateAssignments(DepartmentId);
CREATE INDEX IX_TemplateAssignment_UserGroup ON FormTemplateAssignments(UserGroupId);
CREATE INDEX IX_TemplateAssignment_User ON FormTemplateAssignments(UserId);

-- ============================================================================
-- SECTION 5: SOFTWARE MANAGEMENT (ENHANCED)
-- ============================================================================

-- Software Products Catalog
CREATE TABLE SoftwareProducts (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    ProductCode NVARCHAR(50) UNIQUE NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    Vendor NVARCHAR(100),
    ProductCategory NVARCHAR(100), -- System, Application, Utility, Security
    LicenseModel NVARCHAR(50), -- 'PerUser', 'PerDevice', 'Enterprise', 'Subscription', 'Open Source'
    IsKTDAProduct BIT DEFAULT 0,
    RequiresLicense BIT DEFAULT 0,
    Description NVARCHAR(1000),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT CK_Product_LicenseModel CHECK (
        LicenseModel IN ('PerUser', 'PerDevice', 'Enterprise', 'Subscription', 'OpenSource', 'Concurrent') OR LicenseModel IS NULL
    )
);

CREATE INDEX IX_Products_Category ON SoftwareProducts(ProductCategory, IsActive);
CREATE INDEX IX_Products_Vendor ON SoftwareProducts(Vendor);

-- Software Version Registry
CREATE TABLE SoftwareVersions (
    VersionId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    VersionNumber NVARCHAR(50) NOT NULL,
    
    -- Version comparison support
    MajorVersion INT, -- e.g., 10 from "10.2.5"
    MinorVersion INT, -- e.g., 2 from "10.2.5"
    PatchVersion INT, -- e.g., 5 from "10.2.5"
    
    ReleaseDate DATE,
    EndOfLifeDate DATE,
    IsCurrentVersion BIT DEFAULT 0,
    IsSupported BIT DEFAULT 1,
    SecurityLevel NVARCHAR(20) DEFAULT 'Stable', -- 'Critical', 'Stable', 'Vulnerable', 'Unsupported'
    MinimumSupportedVersion BIT DEFAULT 0,
    ReleaseNotes NVARCHAR(MAX),
    DownloadUrl NVARCHAR(500),
    FileSize BIGINT, -- Download size in bytes
    ChecksumSHA256 NVARCHAR(64), -- For integrity verification
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Version_Product FOREIGN KEY (ProductId)
        REFERENCES SoftwareProducts(ProductId),
    CONSTRAINT UQ_ProductVersion UNIQUE (ProductId, VersionNumber),
    CONSTRAINT CK_Version_SecurityLevel CHECK (
        SecurityLevel IN ('Critical', 'Stable', 'Vulnerable', 'Unsupported')
    )
);

CREATE INDEX IX_Versions_Product ON SoftwareVersions(ProductId, MajorVersion DESC, MinorVersion DESC, PatchVersion DESC);
CREATE INDEX IX_Versions_Security ON SoftwareVersions(SecurityLevel, EndOfLifeDate) WHERE IsSupported = 1;
CREATE INDEX IX_Versions_EOL ON SoftwareVersions(EndOfLifeDate) WHERE EndOfLifeDate IS NOT NULL;

-- Software Licenses (centralized license management)
CREATE TABLE SoftwareLicenses (
    LicenseId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    LicenseKey NVARCHAR(500) NOT NULL,
    LicenseType NVARCHAR(50) NOT NULL, -- 'Perpetual', 'Subscription', 'Trial', 'Volume', 'Academic'
    
    -- Purchase information
    PurchaseDate DATE,
    ExpiryDate DATE,
    QuantityPurchased INT DEFAULT 1,
    QuantityUsed INT DEFAULT 0,
    PurchaseOrderNumber NVARCHAR(100),
    Vendor NVARCHAR(200),
    Cost DECIMAL(18,2),
    Currency NVARCHAR(3) DEFAULT 'KES',
    
    -- Contact and support
    SupportContact NVARCHAR(200),
    SupportPhone NVARCHAR(50),
    SupportEmail NVARCHAR(200),
    
    Notes NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_License_Product FOREIGN KEY (ProductId)
        REFERENCES SoftwareProducts(ProductId),
    CONSTRAINT CK_License_Type CHECK (
        LicenseType IN ('Perpetual', 'Subscription', 'Trial', 'Volume', 'Academic', 'OEM')
    ),
    CONSTRAINT CK_License_Quantity CHECK (QuantityUsed <= QuantityPurchased)
);

CREATE INDEX IX_Licenses_Product ON SoftwareLicenses(ProductId, IsActive);
CREATE INDEX IX_Licenses_Expiry ON SoftwareLicenses(ExpiryDate) WHERE IsActive = 1 AND ExpiryDate IS NOT NULL;
CREATE INDEX IX_Licenses_Available ON SoftwareLicenses(ProductId) WHERE (QuantityPurchased - QuantityUsed) > 0 AND IsActive = 1;

-- Tenant Software Installations (enhanced with license tracking)
CREATE TABLE TenantSoftwareInstallations (
    InstallationId INT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    ProductId INT NOT NULL,
    VersionId INT NOT NULL,
    LicenseId INT NULL, -- Link to centralized license
    
    InstallationDate DATE,
    LastVerifiedDate DATE,
    Status NVARCHAR(50) DEFAULT 'Active', -- 'Active', 'Deprecated', 'NeedsUpgrade', 'EndOfLife', 'Uninstalled'
    InstallationType NVARCHAR(30), -- 'Server', 'Workstation', 'Cloud', 'Virtual'
    InstallationPath NVARCHAR(500), -- Where software is installed
    
    -- Machine/Instance details
    MachineName NVARCHAR(100),
    IPAddress NVARCHAR(50),
    
    -- Verification
    VerifiedBy INT NULL,
    
    Notes NVARCHAR(MAX),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT FK_Install_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_Install_Product FOREIGN KEY (ProductId)
        REFERENCES SoftwareProducts(ProductId),
    CONSTRAINT FK_Install_Version FOREIGN KEY (VersionId)
        REFERENCES SoftwareVersions(VersionId),
    CONSTRAINT FK_Install_License FOREIGN KEY (LicenseId)
        REFERENCES SoftwareLicenses(LicenseId),
    CONSTRAINT FK_Install_Verifier FOREIGN KEY (VerifiedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Install_Modifier FOREIGN KEY (ModifiedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_Install_Status CHECK (
        Status IN ('Active', 'Deprecated', 'NeedsUpgrade', 'EndOfLife', 'Uninstalled')
    ),
    CONSTRAINT CK_Install_Type CHECK (
        InstallationType IN ('Server', 'Workstation', 'Cloud', 'Virtual', 'Container') OR InstallationType IS NULL
    ),
    CONSTRAINT UQ_Install_Tenant_Product_Type UNIQUE (TenantId, ProductId, InstallationType, MachineName)
);

CREATE INDEX IX_Installations_Tenant ON TenantSoftwareInstallations(TenantId, Status);
CREATE INDEX IX_Installations_Product ON TenantSoftwareInstallations(ProductId, Status);
CREATE INDEX IX_Installations_Version ON TenantSoftwareInstallations(VersionId);
CREATE INDEX IX_Installations_License ON TenantSoftwareInstallations(LicenseId) WHERE LicenseId IS NOT NULL;
CREATE INDEX IX_Installations_Verification ON TenantSoftwareInstallations(LastVerifiedDate) WHERE Status = 'Active';

-- Software Installation History (audit trail of installations/upgrades)
CREATE TABLE SoftwareInstallationHistory (
    HistoryId INT PRIMARY KEY IDENTITY(1,1),
    InstallationId INT NOT NULL,
    FromVersionId INT NULL,
    ToVersionId INT NOT NULL,
    ChangeType NVARCHAR(20) NOT NULL, -- 'Install', 'Upgrade', 'Downgrade', 'Uninstall', 'Reinstall', 'Patch'
    ChangeDate DATETIME2 DEFAULT GETUTCDATE(),
    ChangedBy INT NOT NULL,
    Reason NVARCHAR(500),
    SuccessStatus BIT DEFAULT 1, -- Did the change succeed?
    ErrorMessage NVARCHAR(MAX),
    Notes NVARCHAR(MAX),
    
    CONSTRAINT FK_InstallHistory_Installation FOREIGN KEY (InstallationId)
        REFERENCES TenantSoftwareInstallations(InstallationId),
    CONSTRAINT FK_InstallHistory_FromVersion FOREIGN KEY (FromVersionId)
        REFERENCES SoftwareVersions(VersionId),
    CONSTRAINT FK_InstallHistory_ToVersion FOREIGN KEY (ToVersionId)
        REFERENCES SoftwareVersions(VersionId),
    CONSTRAINT FK_InstallHistory_User FOREIGN KEY (ChangedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_History_ChangeType CHECK (
        ChangeType IN ('Install', 'Upgrade', 'Downgrade', 'Uninstall', 'Reinstall', 'Patch')
    )
);

CREATE INDEX IX_InstallHistory_Installation ON SoftwareInstallationHistory(InstallationId, ChangeDate DESC);
CREATE INDEX IX_InstallHistory_Date ON SoftwareInstallationHistory(ChangeDate DESC);
CREATE INDEX IX_InstallHistory_Type ON SoftwareInstallationHistory(ChangeType, ChangeDate DESC);
CREATE INDEX IX_InstallHistory_User ON SoftwareInstallationHistory(ChangedBy);

-- ============================================================================
-- SECTION 6: HARDWARE INVENTORY
-- ============================================================================

-- Hardware Categories
CREATE TABLE HardwareCategories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryCode NVARCHAR(50) UNIQUE NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    ParentCategoryId INT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_Category_Parent FOREIGN KEY (ParentCategoryId)
        REFERENCES HardwareCategories(CategoryId)
);

-- Hardware Items (Master List)
CREATE TABLE HardwareItems (
    HardwareItemId INT PRIMARY KEY IDENTITY(1,1),
    CategoryId INT NOT NULL,
    ItemCode NVARCHAR(50) UNIQUE NOT NULL,
    ItemName NVARCHAR(200) NOT NULL,
    Manufacturer NVARCHAR(100),
    Model NVARCHAR(100),
    Specifications NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Item_Category FOREIGN KEY (CategoryId)
        REFERENCES HardwareCategories(CategoryId)
);

-- Tenant Hardware (Actual Inventory)
CREATE TABLE TenantHardware (
    TenantHardwareId INT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    HardwareItemId INT NOT NULL,
    AssetTag NVARCHAR(50),
    SerialNumber NVARCHAR(100),
    Location NVARCHAR(200), -- Server Room, Office, etc.
    PurchaseDate DATE,
    WarrantyExpiryDate DATE,
    Status NVARCHAR(50), -- Operational, Faulty, Under Repair, Retired
    Quantity INT DEFAULT 1,
    Remarks NVARCHAR(1000),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_TenantHw_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_TenantHw_Item FOREIGN KEY (HardwareItemId)
        REFERENCES HardwareItems(HardwareItemId),
    CONSTRAINT CK_TenantHw_Status CHECK (
        Status IS NULL OR Status IN ('Operational', 'Faulty', 'UnderRepair', 'Retired', 'InStorage', 'PendingDeployment', 'Disposed')
    )
);

CREATE INDEX IX_TenantHw_Tenant ON TenantHardware(TenantId, Status);
CREATE INDEX IX_TenantHw_Status ON TenantHardware(Status) WHERE Status != 'Retired';

-- Hardware Maintenance Log
CREATE TABLE HardwareMaintenanceLog (
    LogId INT PRIMARY KEY IDENTITY(1,1),
    TenantHardwareId INT NOT NULL,
    MaintenanceDate DATE NOT NULL,
    MaintenanceType NVARCHAR(50), -- Preventive, Corrective, Upgrade
    Description NVARCHAR(MAX),
    PerformedBy INT,
    Cost DECIMAL(18,2),
    NextMaintenanceDate DATE,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Maintenance_Hardware FOREIGN KEY (TenantHardwareId)
        REFERENCES TenantHardware(TenantHardwareId),
    CONSTRAINT CK_Maintenance_Type CHECK (
        MaintenanceType IS NULL OR MaintenanceType IN ('Preventive', 'Corrective', 'Upgrade', 'Emergency', 'Calibration', 'Inspection')
    )
);

-- ============================================================================
-- SECTION 7: SUPPORT TICKETS
-- ============================================================================

-- Ticket Categories (supports standardized categorization across internal and external systems)
CREATE TABLE TicketCategories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryCode NVARCHAR(50) UNIQUE NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    ParentCategoryId INT NULL, -- Support hierarchical categories
    
    -- SLA Configuration
    SLAHours INT, -- Resolution time target
    EscalationHours INT, -- When to escalate if not resolved
    
    GenericCategoryMapping NVARCHAR(MAX), -- JSON: {"system": "category"} for other systems
    
    -- Display and Organization
    DisplayOrder INT DEFAULT 0,
    IconClass NVARCHAR(50), -- UI icon class
    ColorCode NVARCHAR(20), -- Hex color for UI
    IsActive BIT DEFAULT 1,
    
    CONSTRAINT FK_TicketCategory_Parent FOREIGN KEY (ParentCategoryId)
        REFERENCES TicketCategories(CategoryId)
);

CREATE INDEX IX_TicketCategories_Parent ON TicketCategories(ParentCategoryId) WHERE ParentCategoryId IS NOT NULL;
CREATE INDEX IX_TicketCategories_Active ON TicketCategories(IsActive, DisplayOrder);

-- Support Tickets (supports both internal tickets and external system integration)
CREATE TABLE Tickets (
    TicketId INT PRIMARY KEY IDENTITY(1,1),
    TicketNumber NVARCHAR(50) UNIQUE NOT NULL, -- e.g., TKT-2025-00123 or synced external ID
    TenantId INT NOT NULL,
    CategoryId INT NOT NULL,
    
    -- Basic Information
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Priority NVARCHAR(20) NOT NULL DEFAULT 'Medium', -- Low, Medium, High, Critical
    Status NVARCHAR(50) NOT NULL DEFAULT 'Open', -- Open, InProgress, Escalated, Resolved, Closed, Cancelled
    
    -- People
    ReportedBy INT NOT NULL,
    ReportedDate DATETIME2 DEFAULT GETDATE(),
    AssignedTo INT,
    AssignedDate DATETIME2,
    EscalatedTo INT,
    EscalatedDate DATETIME2,
    ResolvedBy INT,
    ResolvedDate DATETIME2,
    ClosedDate DATETIME2,
    
    -- Resolution
    ResolutionNotes NVARCHAR(MAX),
    ResolutionTime INT, -- Minutes to resolve (calculated)
    
    -- SLA Tracking
    SLADueDate DATETIME2,
    IsSLABreached BIT DEFAULT 0,
    
    -- External System Integration
    IsExternal BIT DEFAULT 0, -- Is this from external ticketing system?
    ExternalSystem NVARCHAR(50), -- 'Jira', 'ServiceNow', 'Zendesk', 'Freshdesk', 'Internal'
    ExternalTicketId NVARCHAR(100), -- ID in external system (e.g., JIRA-12345)
    ExternalTicketUrl NVARCHAR(500), -- Deep link to external ticket
    LastSyncDate DATETIME2, -- When was this last synced from external system?
    SyncStatus NVARCHAR(20) DEFAULT 'Synced', -- 'Synced', 'Pending', 'Failed'
    SyncError NVARCHAR(500), -- Error message if sync failed
    
    -- Asset Linkage
    RelatedHardwareId INT NULL, -- Link to specific hardware if applicable
    RelatedSoftwareId INT NULL, -- Link to specific software installation if applicable
    
    -- Metadata
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT FK_Ticket_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_Ticket_Category FOREIGN KEY (CategoryId)
        REFERENCES TicketCategories(CategoryId),
    CONSTRAINT FK_Ticket_Reporter FOREIGN KEY (ReportedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Ticket_Assigned FOREIGN KEY (AssignedTo)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Ticket_Escalated FOREIGN KEY (EscalatedTo)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Ticket_Resolver FOREIGN KEY (ResolvedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Ticket_Hardware FOREIGN KEY (RelatedHardwareId)
        REFERENCES TenantHardware(TenantHardwareId),
    CONSTRAINT FK_Ticket_Software FOREIGN KEY (RelatedSoftwareId)
        REFERENCES TenantSoftwareInstallations(InstallationId),
    CONSTRAINT CK_Ticket_Priority CHECK (
        Priority IN ('Low', 'Medium', 'High', 'Critical')
    ),
    CONSTRAINT CK_Ticket_Status CHECK (
        Status IN ('Open', 'InProgress', 'Escalated', 'Resolved', 'Closed', 'Cancelled')
    ),
    CONSTRAINT CK_Ticket_ExternalSystem CHECK (
        ExternalSystem IN ('Internal', 'Jira', 'ServiceNow', 'Zendesk', 'Freshdesk', 'BMC', 'ManageEngine', 'Other') OR ExternalSystem IS NULL
    ),
    CONSTRAINT CK_Ticket_SyncStatus CHECK (
        SyncStatus IN ('Synced', 'Pending', 'Failed', 'NotApplicable')
    )
);

CREATE INDEX IX_Tickets_Status ON Tickets(Status, TenantId);
CREATE INDEX IX_Tickets_Tenant ON Tickets(TenantId, Status);
CREATE INDEX IX_Tickets_Priority ON Tickets(Priority, Status);
CREATE INDEX IX_Tickets_SLA ON Tickets(SLADueDate, Status) WHERE Status NOT IN ('Resolved', 'Closed');
CREATE INDEX IX_Tickets_External ON Tickets(ExternalSystem, ExternalTicketId) WHERE IsExternal = 1;
CREATE INDEX IX_Tickets_ExternalId ON Tickets(ExternalTicketId) WHERE ExternalTicketId IS NOT NULL;
CREATE INDEX IX_Tickets_Sync ON Tickets(LastSyncDate, SyncStatus) WHERE IsExternal = 1;
CREATE INDEX IX_Tickets_SyncFailed ON Tickets(SyncStatus, LastSyncDate) WHERE SyncStatus = 'Failed';
CREATE INDEX IX_Tickets_Hardware ON Tickets(RelatedHardwareId) WHERE RelatedHardwareId IS NOT NULL;
CREATE INDEX IX_Tickets_Software ON Tickets(RelatedSoftwareId) WHERE RelatedSoftwareId IS NOT NULL;
CREATE INDEX IX_Tickets_Assigned ON Tickets(AssignedTo, Status) WHERE AssignedTo IS NOT NULL;
CREATE FULLTEXT INDEX ON Tickets(Title, Description);

-- Ticket Comments
CREATE TABLE TicketComments (
    CommentId INT PRIMARY KEY IDENTITY(1,1),
    TicketId INT NOT NULL,
    UserId INT NOT NULL,
    Comment NVARCHAR(MAX) NOT NULL,
    IsInternal BIT DEFAULT 0, -- Internal notes vs customer-visible
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Comment_Ticket FOREIGN KEY (TicketId)
        REFERENCES Tickets(TicketId) ON DELETE CASCADE,
    CONSTRAINT FK_Comment_User FOREIGN KEY (UserId)
        REFERENCES Users(UserId)
);

CREATE INDEX IX_Comments_Ticket ON TicketComments(TicketId, CreatedDate);

-- ============================================================================
-- SECTION 8: FINANCIAL TRACKING (NEW)
-- ============================================================================

-- Budget Categories
CREATE TABLE BudgetCategories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryCode NVARCHAR(50) UNIQUE NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    ParentCategoryId INT,
    IsCapital BIT DEFAULT 0, -- Capital vs Recurrent
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_BudgetCategory_Parent FOREIGN KEY (ParentCategoryId)
        REFERENCES BudgetCategories(CategoryId)
);

-- Tenant Budgets
CREATE TABLE TenantBudgets (
    BudgetId INT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    FiscalYear INT NOT NULL,
    CategoryId INT NOT NULL,
    BudgetedAmount DECIMAL(18,2) NOT NULL,
    ApprovedDate DATE,
    ApprovedBy INT,
    Notes NVARCHAR(MAX),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Budget_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_Budget_Category FOREIGN KEY (CategoryId)
        REFERENCES BudgetCategories(CategoryId),
    CONSTRAINT UQ_TenantBudget UNIQUE (TenantId, FiscalYear, CategoryId)
);

-- Tenant Expenses
CREATE TABLE TenantExpenses (
    ExpenseId INT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    CategoryId INT NOT NULL,
    ExpenseDate DATE NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500),
    ReferenceNumber NVARCHAR(50),
    
    -- Expense Classification
    ExpenseType NVARCHAR(30) NOT NULL DEFAULT 'Purchase', -- Type of expense
    IsCapital BIT DEFAULT 0, -- Capital vs Recurrent (CAPEX vs OPEX)
    
    -- Vendor Information
    VendorName NVARCHAR(200), -- NULL for internal expenses
    
    -- Attachments (deprecated - use MediaFiles table instead)
    AttachmentPath NVARCHAR(500), -- Legacy field, migrate to MediaFiles
    
    -- Approval
    ApprovedBy INT,
    ApprovedDate DATE,
    
    -- Audit
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    
    CONSTRAINT FK_Expense_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_Expense_Category FOREIGN KEY (CategoryId)
        REFERENCES BudgetCategories(CategoryId),
    CONSTRAINT FK_Expense_Creator FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_Expense_Type CHECK (
        ExpenseType IN ('Purchase', 'Subscription', 'Maintenance', 'Service', 'Internal', 'Utility', 'Other')
    )
);

CREATE INDEX IX_Expenses_Tenant ON TenantExpenses(TenantId, ExpenseDate DESC);
CREATE INDEX IX_Expenses_Date ON TenantExpenses(ExpenseDate, TenantId);
CREATE INDEX IX_Expenses_Type ON TenantExpenses(ExpenseType, IsCapital);

-- ============================================================================
-- SECTION 9: UNIFIED NOTIFICATION SYSTEM
-- ============================================================================
-- Comprehensive notification system supporting:
--   - Multi-channel delivery (Email, SMS, Push, In-App, Webhook)
--   - Centralized notification inbox
--   - Delivery tracking and retry logic
--   - User preferences and quiet hours
--   - Template management
--   - Integration with Alerts, Reports, Workflows, Forms
-- ============================================================================

-- 1. Notification Channels (System-wide configuration)
CREATE TABLE NotificationChannels (
    ChannelId INT PRIMARY KEY IDENTITY(1,1),
    ChannelType NVARCHAR(20) NOT NULL, -- 'Email', 'SMS', 'Push', 'InApp', 'Webhook'
    ChannelName NVARCHAR(100) NOT NULL,
    IsEnabled BIT DEFAULT 1,

    -- Channel-specific configuration (JSON)
    Configuration NVARCHAR(MAX), -- SMTP settings, Twilio API, Firebase config, etc.

    -- Rate limiting
    MaxMessagesPerMinute INT DEFAULT 100,
    MaxMessagesPerDay INT DEFAULT 10000,

    -- Retry configuration
    RetryAttempts INT DEFAULT 3,
    RetryDelayMinutes INT DEFAULT 5,

    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT CK_Channel_Type CHECK (
        ChannelType IN ('Email', 'SMS', 'Push', 'InApp', 'Webhook')
    )
);

CREATE INDEX IX_Channel_Type ON NotificationChannels(ChannelType, IsEnabled);

-- 2. Notifications (Central notification inbox - all notification types)
CREATE TABLE Notifications (
    NotificationId BIGINT PRIMARY KEY IDENTITY(1,1),

    -- Notification type and priority
    NotificationType NVARCHAR(50) NOT NULL,
    -- Types: 'Alert', 'FormApproval', 'FormSubmitted', 'FormRejected', 'ReportReady',
    --        'WorkflowStep', 'TicketAssigned', 'TicketUpdated', 'CommentAdded',
    --        'LicenseExpiring', 'BudgetExceeded', 'SystemMaintenance', 'UserMention'
    Priority NVARCHAR(20) NOT NULL DEFAULT 'Normal', -- 'Critical', 'High', 'Normal', 'Low'

    -- Message content
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,

    -- Rich content (optional)
    ActionUrl NVARCHAR(500), -- Deep link: /forms/submissions/123, /tickets/456
    ActionLabel NVARCHAR(50), -- "View Submission", "Approve Now", "View Details"
    IconClass NVARCHAR(50), -- FontAwesome: 'fa-check-circle', 'fa-exclamation-triangle'
    ColorClass NVARCHAR(20), -- Bootstrap: 'success', 'warning', 'danger', 'info', 'primary'

    -- Context (what triggered this notification?)
    SourceType NVARCHAR(50), -- 'Alert', 'Workflow', 'Schedule', 'UserAction', 'System', 'Form'
    SourceId INT, -- AlertId, WorkflowStepId, ScheduleId, UserId, etc.
    RelatedEntityType NVARCHAR(100), -- 'FormSubmission', 'Ticket', 'Report', 'Hardware', 'License'
    RelatedEntityId INT,

    -- Additional metadata (JSON)
    Metadata NVARCHAR(MAX), -- Custom data: {"department": "ICT", "region": "Region1"}

    -- Tenant context (for multi-tenant filtering)
    TenantId INT NULL,

    -- Lifecycle
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ExpiryDate DATETIME2, -- Auto-delete after this date (e.g., 90 days)

    CONSTRAINT FK_Notification_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT CK_Notification_Priority CHECK (
        Priority IN ('Critical', 'High', 'Normal', 'Low')
    )
);

CREATE INDEX IX_Notification_Type ON Notifications(NotificationType, CreatedDate DESC);
CREATE INDEX IX_Notification_Priority ON Notifications(Priority, CreatedDate DESC);
CREATE INDEX IX_Notification_Source ON Notifications(SourceType, SourceId);
CREATE INDEX IX_Notification_Entity ON Notifications(RelatedEntityType, RelatedEntityId);
CREATE INDEX IX_Notification_Tenant ON Notifications(TenantId, CreatedDate DESC) WHERE TenantId IS NOT NULL;
CREATE INDEX IX_Notification_Expiry ON Notifications(ExpiryDate) WHERE ExpiryDate IS NOT NULL;

-- 3. Notification Recipients (Who gets each notification - many-to-many)
CREATE TABLE NotificationRecipients (
    RecipientId BIGINT PRIMARY KEY IDENTITY(1,1),
    NotificationId BIGINT NOT NULL,
    UserId INT NOT NULL,

    -- Delivery preferences (per recipient, can be overridden by user preferences)
    DeliverViaEmail BIT DEFAULT 1,
    DeliverViaSMS BIT DEFAULT 0,
    DeliverViaPush BIT DEFAULT 1,
    DeliverViaInApp BIT DEFAULT 1, -- Always true for in-app inbox

    -- Status tracking
    IsRead BIT DEFAULT 0,
    ReadDate DATETIME2,
    IsArchived BIT DEFAULT 0,
    ArchivedDate DATETIME2,
    IsDismissed BIT DEFAULT 0, -- User clicked "Dismiss" or "Clear"
    DismissedDate DATETIME2,

    -- Interaction tracking
    ClickedAction BIT DEFAULT 0, -- Did user click the action button?
    ClickedDate DATETIME2,

    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT FK_NotifRecipient_Notification FOREIGN KEY (NotificationId)
        REFERENCES Notifications(NotificationId) ON DELETE CASCADE,
    CONSTRAINT FK_NotifRecipient_User FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT UQ_Notification_User UNIQUE (NotificationId, UserId)
);

CREATE INDEX IX_NotifRecipient_User ON NotificationRecipients(UserId, IsRead, CreatedDate DESC);
CREATE INDEX IX_NotifRecipient_Unread ON NotificationRecipients(UserId, IsRead, CreatedDate DESC)
    WHERE IsRead = 0 AND IsArchived = 0;
CREATE INDEX IX_NotifRecipient_Notification ON NotificationRecipients(NotificationId);

-- 4. Notification Delivery (Track delivery to external channels with retry)
CREATE TABLE NotificationDelivery (
    DeliveryId BIGINT PRIMARY KEY IDENTITY(1,1),
    RecipientId BIGINT NOT NULL,
    ChannelId INT NOT NULL,

    -- Delivery status
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    -- 'Pending', 'Sent', 'Delivered', 'Failed', 'Bounced', 'Spam', 'Unsubscribed'
    AttemptNumber INT DEFAULT 1,

    -- Delivery timestamps
    SentDate DATETIME2,
    DeliveredDate DATETIME2,
    FailedDate DATETIME2,
    ErrorMessage NVARCHAR(MAX),
    ErrorCode NVARCHAR(50), -- Provider-specific error code

    -- External provider tracking
    ExternalMessageId NVARCHAR(200), -- Twilio SID, SendGrid MessageID, Firebase Token
    ExternalStatus NVARCHAR(100), -- Provider's delivery status
    ExternalResponse NVARCHAR(MAX), -- Full provider response (JSON)

    -- Delivery details
    RecipientAddress NVARCHAR(500), -- Email address, phone number, device token
    MessageSize INT, -- Bytes sent
    CostAmount DECIMAL(10,4), -- Cost for SMS/external services
    CostCurrency NVARCHAR(10) DEFAULT 'KES',

    -- Retry logic
    NextRetryDate DATETIME2,
    MaxRetries INT DEFAULT 3,

    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Delivery_Recipient FOREIGN KEY (RecipientId)
        REFERENCES NotificationRecipients(RecipientId) ON DELETE CASCADE,
    CONSTRAINT FK_Delivery_Channel FOREIGN KEY (ChannelId)
        REFERENCES NotificationChannels(ChannelId),
    CONSTRAINT CK_Delivery_Status CHECK (
        Status IN ('Pending', 'Sent', 'Delivered', 'Failed', 'Bounced', 'Spam', 'Unsubscribed', 'Cancelled')
    )
);

CREATE INDEX IX_Delivery_Status ON NotificationDelivery(Status, NextRetryDate)
    WHERE Status IN ('Pending', 'Failed');
CREATE INDEX IX_Delivery_Recipient ON NotificationDelivery(RecipientId, CreatedDate DESC);
CREATE INDEX IX_Delivery_Channel ON NotificationDelivery(ChannelId, Status, CreatedDate DESC);
CREATE INDEX IX_Delivery_External ON NotificationDelivery(ExternalMessageId) WHERE ExternalMessageId IS NOT NULL;

-- 5. Notification Templates (Reusable message templates with placeholders)
CREATE TABLE NotificationTemplates (
    TemplateId INT PRIMARY KEY IDENTITY(1,1),
    TemplateCode NVARCHAR(50) UNIQUE NOT NULL,
    TemplateName NVARCHAR(200) NOT NULL,
    NotificationType NVARCHAR(50) NOT NULL,
    Description NVARCHAR(1000),

    -- Multi-channel templates
    EmailSubjectTemplate NVARCHAR(500),
    EmailBodyTemplate NVARCHAR(MAX), -- HTML template with placeholders
    SMSTemplate NVARCHAR(500), -- Plain text, max 160 chars recommended
    PushTitleTemplate NVARCHAR(100),
    PushBodyTemplate NVARCHAR(500),
    InAppTitleTemplate NVARCHAR(200),
    InAppBodyTemplate NVARCHAR(MAX),

    -- Template configuration
    Placeholders NVARCHAR(MAX), -- JSON array: ["{{UserName}}", "{{TenantName}}", "{{DueDate}}"]
    DefaultPriority NVARCHAR(20) DEFAULT 'Normal',
    DefaultIconClass NVARCHAR(50),
    DefaultColorClass NVARCHAR(20),

    -- Metadata
    IsActive BIT DEFAULT 1,
    IsSystem BIT DEFAULT 0, -- System templates cannot be deleted
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Template_Creator FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_Template_Priority CHECK (
        DefaultPriority IN ('Critical', 'High', 'Normal', 'Low')
    )
);

CREATE INDEX IX_NotifTemplate_Type ON NotificationTemplates(NotificationType, IsActive);
CREATE INDEX IX_NotifTemplate_Code ON NotificationTemplates(TemplateCode);

-- 6. User Notification Preferences (Per-user channel and frequency settings)
CREATE TABLE UserNotificationPreferences (
    PreferenceId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    NotificationType NVARCHAR(50) NOT NULL, -- Specific type or 'All' for global settings

    -- Channel preferences
    AllowEmail BIT DEFAULT 1,
    AllowSMS BIT DEFAULT 0, -- Opt-in (costs money)
    AllowPush BIT DEFAULT 1,
    AllowInApp BIT DEFAULT 1, -- Cannot be disabled

    -- Delivery frequency
    Frequency NVARCHAR(20) DEFAULT 'Immediate',
    -- 'Immediate', 'Hourly', 'Daily', 'Weekly', 'Never'
    DigestTime TIME, -- For daily/weekly: when to send digest (e.g., 08:00)

    -- Quiet hours (don't send notifications during these hours)
    QuietHoursEnabled BIT DEFAULT 0,
    QuietHoursStart TIME, -- e.g., 22:00
    QuietHoursEnd TIME, -- e.g., 06:00

    -- Priority filtering
    MinimumPriority NVARCHAR(20) DEFAULT 'Normal',
    -- Only notify for this priority and above ('Low' = all, 'Critical' = critical only)

    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT FK_NotifPref_User FOREIGN KEY (UserId)
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT UQ_UserPref_Type UNIQUE (UserId, NotificationType),
    CONSTRAINT CK_NotifPref_Frequency CHECK (
        Frequency IN ('Immediate', 'Hourly', 'Daily', 'Weekly', 'Never')
    ),
    CONSTRAINT CK_NotifPref_Priority CHECK (
        MinimumPriority IN ('Low', 'Normal', 'High', 'Critical')
    )
);

CREATE INDEX IX_NotifPref_User ON UserNotificationPreferences(UserId);

-- ============================================================================
-- ALERT SYSTEM (Integrated with Notification System)
-- ============================================================================

-- Alert Definitions (Alert rule configuration - creates notifications automatically)
CREATE TABLE AlertDefinitions (
    AlertId INT PRIMARY KEY IDENTITY(1,1),
    AlertCode NVARCHAR(50) UNIQUE NOT NULL,
    AlertName NVARCHAR(200) NOT NULL,
    AlertType NVARCHAR(50) NOT NULL,
    -- Types: 'SystemDown', 'LicenseExpiry', 'BackupFailed', 'BudgetExceeded',
    --        'TicketSLABreach', 'FormOverdue', 'HardwareFailure', 'DiskSpaceLow'
    Severity NVARCHAR(20) NOT NULL DEFAULT 'Medium', -- Maps to notification priority

    -- Trigger configuration
    TriggerCondition NVARCHAR(MAX), -- SQL expression or JSON rule definition
    CheckFrequency INT DEFAULT 60, -- Check interval in minutes (0 = event-driven)
    LastCheckedDate DATETIME2, -- Track when last evaluated
    LastTriggeredDate DATETIME2, -- For throttling check

    -- Notification configuration
    NotificationTemplateId INT NOT NULL, -- Which template to use when alert fires
    DefaultRecipients NVARCHAR(MAX) NOT NULL, -- JSON: [{"type":"Role","id":1},{"type":"Department","id":5}]
    
    -- Alert behavior
    AutoResolve BIT DEFAULT 0, -- Auto-resolve when condition no longer met
    AutoResolveAfterMinutes INT, -- Auto-resolve after X minutes even if condition still met

    -- Throttling (prevent alert spam)
    ThrottleEnabled BIT DEFAULT 1,
    ThrottleMinutes INT DEFAULT 60, -- Don't trigger same alert within X minutes

    IsActive BIT DEFAULT 1,
    Description NVARCHAR(1000),
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Alert_Template FOREIGN KEY (NotificationTemplateId)
        REFERENCES NotificationTemplates(TemplateId),
    CONSTRAINT FK_Alert_Creator FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_Alert_Severity CHECK (
        Severity IN ('Critical', 'High', 'Medium', 'Low')
    )
);

CREATE INDEX IX_Alert_Type ON AlertDefinitions(AlertType, IsActive);
CREATE INDEX IX_Alert_Active ON AlertDefinitions(IsActive, CheckFrequency);
CREATE INDEX IX_Alert_LastChecked ON AlertDefinitions(LastCheckedDate) WHERE IsActive = 1;

-- Alert History (Simplified - tracks alert triggers and links to notifications)
CREATE TABLE AlertHistory (
    AlertHistoryId BIGINT PRIMARY KEY IDENTITY(1,1),
    AlertId INT NOT NULL,
    NotificationId BIGINT NOT NULL, -- Link to created notification

    -- Context
    TenantId INT NULL,
    TriggerDate DATETIME2 DEFAULT GETUTCDATE(),
    
    -- Related entity (what triggered this alert)
    RelatedEntityType NVARCHAR(100), -- 'Ticket', 'Hardware', 'License', 'Budget', 'Form'
    RelatedEntityId INT,

    -- Alert-specific lifecycle (separate from notification read status)
    IsAcknowledged BIT DEFAULT 0,
    AcknowledgedBy INT NULL,
    AcknowledgedDate DATETIME2,
    AcknowledgementNotes NVARCHAR(MAX),

    IsResolved BIT DEFAULT 0,
    ResolvedDate DATETIME2,
    ResolvedBy INT NULL,
    ResolutionNotes NVARCHAR(MAX),
    AutoResolved BIT DEFAULT 0, -- Was this auto-resolved by system?

    CONSTRAINT FK_AlertHistory_Alert FOREIGN KEY (AlertId)
        REFERENCES AlertDefinitions(AlertId),
    CONSTRAINT FK_AlertHistory_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_AlertHistory_Notification FOREIGN KEY (NotificationId)
        REFERENCES Notifications(NotificationId),
    CONSTRAINT FK_AlertHistory_AckUser FOREIGN KEY (AcknowledgedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_AlertHistory_ResolveUser FOREIGN KEY (ResolvedBy)
        REFERENCES Users(UserId)
);

CREATE INDEX IX_AlertHistory_Alert ON AlertHistory(AlertId, TriggerDate DESC);
CREATE INDEX IX_AlertHistory_Active ON AlertHistory(IsResolved, TriggerDate DESC)
    WHERE IsResolved = 0;
CREATE INDEX IX_AlertHistory_Tenant ON AlertHistory(TenantId, TriggerDate DESC)
    WHERE TenantId IS NOT NULL;
CREATE INDEX IX_AlertHistory_Notification ON AlertHistory(NotificationId);

-- ============================================================================
-- SECTION 10: REPORTING & ANALYTICS (NEW)
-- ============================================================================

-- Tenant Performance Snapshot (Pre-aggregated for performance - flexible design)
CREATE TABLE TenantPerformanceSnapshot (
    SnapshotId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId INT NOT NULL,
    SnapshotDate DATE NOT NULL, -- Supports daily, weekly, monthly snapshots
    SnapshotType NVARCHAR(20) NOT NULL, -- 'Daily', 'Weekly', 'Monthly'
    
    -- Flexible metrics storage (JSON for extensibility)
    MetricsData NVARCHAR(MAX) NOT NULL, -- JSON: All metrics in flexible format
    
    -- Key metrics for quick queries (denormalized for performance)
    TotalDevices INT DEFAULT 0,
    WorkingDevices INT DEFAULT 0,
    FaultyDevices INT DEFAULT 0,
    UptimePercent DECIMAL(5,2),
    OpenTickets INT DEFAULT 0,
    ComplianceScore DECIMAL(5,2),
    TotalExpenses DECIMAL(18,2),
    
    -- Metadata
    GeneratedDate DATETIME2 DEFAULT GETUTCDATE(),
    GeneratedBy NVARCHAR(50), -- 'HangfireJob', 'Manual', 'Scheduled'
    DataVersion INT DEFAULT 1, -- For schema evolution
    
    CONSTRAINT FK_PerfSnapshot_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT UQ_PerfSnapshot UNIQUE (TenantId, SnapshotDate, SnapshotType),
    CONSTRAINT CK_SnapshotType CHECK (SnapshotType IN ('Daily', 'Weekly', 'Monthly', 'Quarterly'))
);

CREATE INDEX IX_PerfSnapshot_Tenant_Date ON TenantPerformanceSnapshot(TenantId, SnapshotDate DESC);
CREATE INDEX IX_PerfSnapshot_Type_Date ON TenantPerformanceSnapshot(SnapshotType, SnapshotDate DESC);
CREATE INDEX IX_PerfSnapshot_Generated ON TenantPerformanceSnapshot(GeneratedDate DESC);

-- Monthly Regional Snapshot (Aggregated from factories only)
CREATE TABLE RegionalMonthlySnapshot (
    SnapshotId BIGINT PRIMARY KEY IDENTITY(1,1),
    RegionId INT NOT NULL,
    YearMonth DATE NOT NULL,
    TotalFactories INT DEFAULT 0,
    TotalDevices INT DEFAULT 0,
    WorkingDevices INT DEFAULT 0,
    FaultyDevices INT DEFAULT 0,
    TotalTickets INT DEFAULT 0,
    OpenTickets INT DEFAULT 0,
    ResolvedTickets INT DEFAULT 0,
    AvgResolutionDays DECIMAL(5,2),
    TotalExpenses DECIMAL(18,2),
    AvgComplianceScore DECIMAL(5,2),
    LastUpdated DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_RegSnapshot_Region FOREIGN KEY (RegionId)
        REFERENCES Regions(RegionId),
    CONSTRAINT UQ_RegionMonth UNIQUE (RegionId, YearMonth)
);

-- ============================================================================
-- USER-DEFINED CUSTOM REPORTS (Self-Service Reporting)
-- ============================================================================

-- Report Definitions (User-created custom reports)
CREATE TABLE ReportDefinitions (
    ReportId INT PRIMARY KEY IDENTITY(1,1),
    ReportName NVARCHAR(200) NOT NULL,
    ReportCode NVARCHAR(50) UNIQUE NOT NULL,
    Description NVARCHAR(1000),
    
    -- What is this report based on?
    TemplateId INT NULL, -- If form-based report
    ReportType NVARCHAR(30) NOT NULL, -- 'Tabular', 'Chart', 'Pivot', 'Dashboard', 'CrossTab'
    Category NVARCHAR(100), -- 'Operations', 'Finance', 'Compliance', 'Hardware', 'Software'
    
    -- Report Configuration (JSON)
    ChartType NVARCHAR(30), -- 'Bar', 'Line', 'Pie', 'Area', 'Column' (if ReportType = Chart)
    ChartConfiguration NVARCHAR(MAX), -- JSON: Chart-specific settings
    LayoutConfiguration NVARCHAR(MAX), -- JSON: Page layout, orientation, paper size
    
    -- Access Control
    IsPublic BIT DEFAULT 0, -- Can other users see this report?
    IsSystem BIT DEFAULT 0, -- System-provided report (cannot be deleted)
    OwnerUserId INT NOT NULL, -- Who created this report
    
    -- Metadata
    Version INT DEFAULT 1,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    LastRunDate DATETIME2,
    RunCount INT DEFAULT 0, -- Track popularity
    
    CONSTRAINT FK_ReportDef_Template FOREIGN KEY (TemplateId)
        REFERENCES FormTemplates(TemplateId),
    CONSTRAINT FK_ReportDef_Owner FOREIGN KEY (OwnerUserId)
        REFERENCES Users(UserId),
    CONSTRAINT CK_Report_Type CHECK (
        ReportType IN ('Tabular', 'Chart', 'Pivot', 'Dashboard', 'CrossTab', 'Matrix')
    ),
    CONSTRAINT CK_Report_ChartType CHECK (
        ChartType IS NULL OR 
        ChartType IN ('Bar', 'Line', 'Pie', 'Doughnut', 'Area', 'Column', 'Scatter', 'Bubble', 'Radar')
    )
);

CREATE INDEX IX_ReportDef_Template ON ReportDefinitions(TemplateId, IsActive);
CREATE INDEX IX_ReportDef_Owner ON ReportDefinitions(OwnerUserId, IsActive);
CREATE INDEX IX_ReportDef_Category ON ReportDefinitions(Category, IsActive);
CREATE INDEX IX_ReportDef_Public ON ReportDefinitions(IsPublic, IsActive) WHERE IsPublic = 1;
CREATE INDEX IX_ReportDef_Popular ON ReportDefinitions(RunCount DESC, LastRunDate DESC);

-- Report Fields (Which form fields/columns to include in the report)
CREATE TABLE ReportFields (
    FieldId INT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    
    -- Field Source
    SourceType NVARCHAR(30) NOT NULL, -- 'FormItem', 'Metric', 'Computed', 'SystemField'
    ItemId INT NULL, -- FK to FormTemplateItems (if SourceType = FormItem)
    MetricId INT NULL, -- FK to MetricDefinitions (if SourceType = Metric)
    SystemFieldName NVARCHAR(100), -- 'TenantName', 'RegionName', 'SubmissionDate' (if SourceType = SystemField)
    
    -- Display Configuration
    DisplayName NVARCHAR(200) NOT NULL, -- Custom column header
    DisplayOrder INT DEFAULT 0,
    IsVisible BIT DEFAULT 1,
    ColumnWidth INT, -- For UI rendering
    
    -- Data Transformation
    AggregationType NVARCHAR(20), -- 'Sum', 'Avg', 'Count', 'Min', 'Max', 'CountDistinct', 'None'
    FormatString NVARCHAR(50), -- '#,##0.00', 'yyyy-MM-dd', '0.0%'
    ComputationFormula NVARCHAR(MAX), -- For computed fields: JSON formula
    
    -- Conditional Formatting
    ConditionalFormatting NVARCHAR(MAX), -- JSON: {"rules": [{"condition": ">90", "color": "green"}]}
    
    CONSTRAINT FK_ReportField_Report FOREIGN KEY (ReportId)
        REFERENCES ReportDefinitions(ReportId) ON DELETE CASCADE,
    CONSTRAINT FK_ReportField_Item FOREIGN KEY (ItemId)
        REFERENCES FormTemplateItems(ItemId),
    CONSTRAINT FK_ReportField_Metric FOREIGN KEY (MetricId)
        REFERENCES MetricDefinitions(MetricId),
    CONSTRAINT CK_ReportField_Source CHECK (
        SourceType IN ('FormItem', 'Metric', 'Computed', 'SystemField')
    ),
    CONSTRAINT CK_ReportField_Aggregation CHECK (
        AggregationType IS NULL OR
        AggregationType IN ('Sum', 'Avg', 'Count', 'Min', 'Max', 'CountDistinct', 'First', 'Last', 'None')
    )
);

CREATE INDEX IX_ReportField_Report ON ReportFields(ReportId, DisplayOrder);
CREATE INDEX IX_ReportField_Item ON ReportFields(ItemId) WHERE ItemId IS NOT NULL;
CREATE INDEX IX_ReportField_Metric ON ReportFields(MetricId) WHERE MetricId IS NOT NULL;

-- Report Filters (WHERE clause conditions)
CREATE TABLE ReportFilters (
    FilterId INT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    
    -- Filter Source
    FilterType NVARCHAR(30) NOT NULL, -- 'TenantId', 'RegionId', 'DateRange', 'Status', 'FieldValue', 'MetricValue'
    ItemId INT NULL, -- Filter on form field value
    MetricId INT NULL, -- Filter on metric value
    SystemFieldName NVARCHAR(100), -- 'SubmittedDate', 'TenantType', 'Status'
    
    -- Filter Logic
    Operator NVARCHAR(20) NOT NULL, -- 'Equals', 'NotEquals', 'GreaterThan', 'LessThan', 'Between', 'In', 'Contains', 'IsNull', 'IsNotNull'
    FilterValue NVARCHAR(MAX), -- JSON: Single value or array {"value": "Factory"} or {"values": [1,2,3]}
    
    -- Filter Behavior
    IsRequired BIT DEFAULT 0, -- Must be applied (cannot be removed by user)
    AllowUserOverride BIT DEFAULT 1, -- Can user change filter value?
    IsParameterized BIT DEFAULT 0, -- Prompt user for value when running report
    ParameterLabel NVARCHAR(200), -- "Select Region:" (if IsParameterized = 1)
    DefaultValue NVARCHAR(500), -- Default parameter value
    
    -- UI
    DisplayOrder INT DEFAULT 0,
    
    CONSTRAINT FK_ReportFilter_Report FOREIGN KEY (ReportId)
        REFERENCES ReportDefinitions(ReportId) ON DELETE CASCADE,
    CONSTRAINT FK_ReportFilter_Item FOREIGN KEY (ItemId)
        REFERENCES FormTemplateItems(ItemId),
    CONSTRAINT FK_ReportFilter_Metric FOREIGN KEY (MetricId)
        REFERENCES MetricDefinitions(MetricId),
    CONSTRAINT CK_ReportFilter_Type CHECK (
        FilterType IN ('TenantId', 'RegionId', 'DateRange', 'Status', 'FieldValue', 'MetricValue', 'TenantType', 'Custom')
    ),
    CONSTRAINT CK_ReportFilter_Operator CHECK (
        Operator IN ('Equals', 'NotEquals', 'GreaterThan', 'LessThan', 'GreaterOrEqual', 'LessOrEqual', 
                     'Between', 'In', 'NotIn', 'Contains', 'StartsWith', 'EndsWith', 'IsNull', 'IsNotNull')
    )
);

CREATE INDEX IX_ReportFilter_Report ON ReportFilters(ReportId, DisplayOrder);
CREATE INDEX IX_ReportFilter_Item ON ReportFilters(ItemId) WHERE ItemId IS NOT NULL;
CREATE INDEX IX_ReportFilter_Parametrized ON ReportFilters(ReportId, IsParameterized) WHERE IsParameterized = 1;

-- Report Groupings (GROUP BY logic)
CREATE TABLE ReportGroupings (
    GroupingId INT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    
    -- What to group by
    GroupByType NVARCHAR(30) NOT NULL, -- 'Tenant', 'Region', 'Month', 'Year', 'Quarter', 'FieldValue', 'MetricValue'
    ItemId INT NULL, -- Group by form field
    MetricId INT NULL, -- Group by metric
    SystemFieldName NVARCHAR(100), -- 'TenantName', 'RegionName', 'SubmissionMonth'
    
    -- Grouping Configuration
    GroupOrder INT DEFAULT 0, -- For nested grouping (Group by Region, then Tenant)
    SortDirection NVARCHAR(10) DEFAULT 'ASC', -- 'ASC', 'DESC'
    ShowSubtotals BIT DEFAULT 1, -- Show subtotal rows
    ShowGrandTotal BIT DEFAULT 1, -- Show grand total row
    
    CONSTRAINT FK_ReportGrouping_Report FOREIGN KEY (ReportId)
        REFERENCES ReportDefinitions(ReportId) ON DELETE CASCADE,
    CONSTRAINT FK_ReportGrouping_Item FOREIGN KEY (ItemId)
        REFERENCES FormTemplateItems(ItemId),
    CONSTRAINT FK_ReportGrouping_Metric FOREIGN KEY (MetricId)
        REFERENCES MetricDefinitions(MetricId),
    CONSTRAINT CK_ReportGrouping_Type CHECK (
        GroupByType IN ('Tenant', 'Region', 'Month', 'Year', 'Quarter', 'Week', 'Day', 
                        'TenantType', 'Category', 'FieldValue', 'MetricValue')
    ),
    CONSTRAINT CK_ReportGrouping_Sort CHECK (SortDirection IN ('ASC', 'DESC'))
);

CREATE INDEX IX_ReportGrouping_Report ON ReportGroupings(ReportId, GroupOrder);

-- Report Sorting (ORDER BY logic)
CREATE TABLE ReportSorting (
    SortId INT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    
    -- What to sort by
    ItemId INT NULL, -- Sort by form field
    MetricId INT NULL, -- Sort by metric
    SystemFieldName NVARCHAR(100), -- 'TenantName', 'SubmissionDate'
    
    -- Sort Configuration
    SortOrder INT DEFAULT 0, -- Primary sort, secondary sort, etc.
    SortDirection NVARCHAR(10) DEFAULT 'ASC', -- 'ASC', 'DESC'
    
    CONSTRAINT FK_ReportSort_Report FOREIGN KEY (ReportId)
        REFERENCES ReportDefinitions(ReportId) ON DELETE CASCADE,
    CONSTRAINT FK_ReportSort_Item FOREIGN KEY (ItemId)
        REFERENCES FormTemplateItems(ItemId),
    CONSTRAINT FK_ReportSort_Metric FOREIGN KEY (MetricId)
        REFERENCES MetricDefinitions(MetricId),
    CONSTRAINT CK_ReportSort_Direction CHECK (SortDirection IN ('ASC', 'DESC'))
);

CREATE INDEX IX_ReportSort_Report ON ReportSorting(ReportId, SortOrder);

-- Report Schedules (Automated report generation and distribution via notification system)
CREATE TABLE ReportSchedules (
    ScheduleId INT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    
    -- Schedule Configuration
    ScheduleName NVARCHAR(200) NOT NULL,
    ScheduleType NVARCHAR(20) NOT NULL, -- 'Daily', 'Weekly', 'Monthly', 'Quarterly', 'OnDemand'
    DayOfWeek TINYINT, -- 0-6 (Sunday-Saturday) for weekly
    DayOfMonth TINYINT, -- 1-31 for monthly
    ExecutionTime TIME, -- Time of day to run
    Timezone NVARCHAR(50) DEFAULT 'East Africa Time',
    
    -- Output Configuration
    OutputFormat NVARCHAR(20) NOT NULL, -- 'PDF', 'Excel', 'CSV', 'JSON'
    IncludeCharts BIT DEFAULT 1,
    PageOrientation NVARCHAR(20) DEFAULT 'Portrait', -- 'Portrait', 'Landscape'
    
    -- Notification Integration (uses unified notification system)
    NotificationTemplateId INT NOT NULL, -- Template for "Report Ready" notification
    Recipients NVARCHAR(MAX) NOT NULL, -- JSON: [{"type":"User","id":123},{"type":"Role","id":2}]
    
    -- Schedule Status
    IsActive BIT DEFAULT 1,
    LastRunDate DATETIME2,
    NextRunDate DATETIME2,
    LastRunStatus NVARCHAR(20), -- 'Success', 'Failed', 'Pending'
    LastRunError NVARCHAR(MAX),
    
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_ReportSchedule_Report FOREIGN KEY (ReportId)
        REFERENCES ReportDefinitions(ReportId) ON DELETE CASCADE,
    CONSTRAINT FK_ReportSchedule_Template FOREIGN KEY (NotificationTemplateId)
        REFERENCES NotificationTemplates(TemplateId),
    CONSTRAINT FK_ReportSchedule_Creator FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_ReportSchedule_Type CHECK (
        ScheduleType IN ('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Yearly', 'OnDemand')
    ),
    CONSTRAINT CK_ReportSchedule_Format CHECK (
        OutputFormat IN ('PDF', 'Excel', 'CSV', 'JSON', 'HTML')
    ),
    CONSTRAINT CK_ReportSchedule_Orientation CHECK (
        PageOrientation IN ('Portrait', 'Landscape')
    )
);

CREATE INDEX IX_ReportSchedule_Report ON ReportSchedules(ReportId, IsActive);
CREATE INDEX IX_ReportSchedule_NextRun ON ReportSchedules(NextRunDate) WHERE IsActive = 1;
CREATE INDEX IX_ReportSchedule_Type ON ReportSchedules(ScheduleType, IsActive);

-- Report Cache (Pre-generated report results for performance)
CREATE TABLE ReportCache (
    CacheId BIGINT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    
    -- Cache Key (to identify specific parameter combination)
    ParameterHash NVARCHAR(64) NOT NULL, -- SHA256 of parameter values
    Parameters NVARCHAR(MAX), -- JSON: Actual parameter values used
    
    -- Cached Data
    ResultData NVARCHAR(MAX) NOT NULL, -- JSON: Complete result set
    RowCount INT NOT NULL,
    ColumnCount INT,
    
    -- Cache Metadata
    GeneratedDate DATETIME2 DEFAULT GETUTCDATE(),
    GeneratedBy INT,
    ExpiryDate DATETIME2 NOT NULL, -- Re-generate after this
    HitCount INT DEFAULT 0, -- How many times this cache was used
    LastAccessDate DATETIME2,
    
    -- Performance Metrics
    GenerationTimeMs INT, -- How long did it take to generate
    DataSizeKB AS (DATALENGTH(ResultData) / 1024), -- Computed column
    
    CONSTRAINT FK_ReportCache_Report FOREIGN KEY (ReportId)
        REFERENCES ReportDefinitions(ReportId) ON DELETE CASCADE,
    CONSTRAINT FK_ReportCache_User FOREIGN KEY (GeneratedBy)
        REFERENCES Users(UserId),
    CONSTRAINT UQ_ReportCache_Hash UNIQUE (ReportId, ParameterHash)
);

CREATE INDEX IX_ReportCache_Expiry ON ReportCache(ExpiryDate) WHERE ExpiryDate < GETUTCDATE();
CREATE INDEX IX_ReportCache_Report ON ReportCache(ReportId, GeneratedDate DESC);
CREATE INDEX IX_ReportCache_Popular ON ReportCache(HitCount DESC, LastAccessDate DESC);

-- Report Access Control (Who can view/edit specific reports)
CREATE TABLE ReportAccessControl (
    AccessId INT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    
    -- Who gets access
    AccessType NVARCHAR(20) NOT NULL, -- 'User', 'Role', 'Department', 'Everyone'
    UserId INT NULL,
    RoleId INT NULL,
    DepartmentId INT NULL,
    
    -- Permission Level
    PermissionLevel NVARCHAR(20) NOT NULL, -- 'View', 'Run', 'Edit', 'Delete', 'Share'
    
    -- Granted By
    GrantedBy INT NOT NULL,
    GrantedDate DATETIME2 DEFAULT GETUTCDATE(),
    ExpiryDate DATETIME2, -- Optional: Time-limited access
    
    IsActive BIT DEFAULT 1,
    
    CONSTRAINT FK_ReportAccess_Report FOREIGN KEY (ReportId)
        REFERENCES ReportDefinitions(ReportId) ON DELETE CASCADE,
    CONSTRAINT FK_ReportAccess_User FOREIGN KEY (UserId)
        REFERENCES Users(UserId),
    CONSTRAINT FK_ReportAccess_Role FOREIGN KEY (RoleId)
        REFERENCES Roles(RoleId),
    CONSTRAINT FK_ReportAccess_Department FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId),
    CONSTRAINT FK_ReportAccess_Grantor FOREIGN KEY (GrantedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_ReportAccess_Type CHECK (
        AccessType IN ('User', 'Role', 'Department', 'Everyone')
    ),
    CONSTRAINT CK_ReportAccess_Permission CHECK (
        PermissionLevel IN ('View', 'Run', 'Edit', 'Delete', 'Share', 'Admin')
    ),
    -- Business rule: Must specify User, Role, or Department based on AccessType
    CONSTRAINT CK_ReportAccess_Target CHECK (
        (AccessType = 'User' AND UserId IS NOT NULL AND RoleId IS NULL AND DepartmentId IS NULL) OR
        (AccessType = 'Role' AND RoleId IS NOT NULL AND UserId IS NULL AND DepartmentId IS NULL) OR
        (AccessType = 'Department' AND DepartmentId IS NOT NULL AND UserId IS NULL AND RoleId IS NULL) OR
        (AccessType = 'Everyone' AND UserId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL)
    )
);

CREATE INDEX IX_ReportAccess_Report ON ReportAccessControl(ReportId, IsActive);
CREATE INDEX IX_ReportAccess_User ON ReportAccessControl(UserId, IsActive) WHERE UserId IS NOT NULL;
CREATE INDEX IX_ReportAccess_Role ON ReportAccessControl(RoleId, IsActive) WHERE RoleId IS NOT NULL;
CREATE INDEX IX_ReportAccess_Expiry ON ReportAccessControl(ExpiryDate) WHERE ExpiryDate IS NOT NULL;

-- Report Execution Log (Audit trail of report runs)
CREATE TABLE ReportExecutionLog (
    ExecutionId BIGINT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    
    -- Execution Context
    ExecutedBy INT NOT NULL,
    ExecutionDate DATETIME2 DEFAULT GETUTCDATE(),
    ExecutionType NVARCHAR(20) NOT NULL, -- 'Manual', 'Scheduled', 'Cached'
    ScheduleId INT NULL, -- If triggered by schedule
    
    -- Parameters Used
    Parameters NVARCHAR(MAX), -- JSON: Parameter values
    Filters NVARCHAR(MAX), -- JSON: Applied filters
    
    -- Execution Results
    Status NVARCHAR(20) NOT NULL, -- 'Success', 'Failed', 'Timeout', 'Cancelled'
    RowCount INT,
    ErrorMessage NVARCHAR(MAX),
    
    -- Performance Metrics
    ExecutionTimeMs INT,
    QueryExecutionTimeMs INT,
    RenderingTimeMs INT,
    
    -- Output
    OutputFormat NVARCHAR(20), -- 'PDF', 'Excel', 'Screen'
    OutputSizeKB INT,
    OutputPath NVARCHAR(1000), -- If saved to disk/cloud
    
    CONSTRAINT FK_ReportExec_Report FOREIGN KEY (ReportId)
        REFERENCES ReportDefinitions(ReportId),
    CONSTRAINT FK_ReportExec_User FOREIGN KEY (ExecutedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_ReportExec_Schedule FOREIGN KEY (ScheduleId)
        REFERENCES ReportSchedules(ScheduleId),
    CONSTRAINT CK_ReportExec_Type CHECK (
        ExecutionType IN ('Manual', 'Scheduled', 'Cached', 'API')
    ),
    CONSTRAINT CK_ReportExec_Status CHECK (
        Status IN ('Success', 'Failed', 'Timeout', 'Cancelled', 'Pending')
    )
);

CREATE INDEX IX_ReportExec_Report ON ReportExecutionLog(ReportId, ExecutionDate DESC);
CREATE INDEX IX_ReportExec_User ON ReportExecutionLog(ExecutedBy, ExecutionDate DESC);
CREATE INDEX IX_ReportExec_Date ON ReportExecutionLog(ExecutionDate DESC);
CREATE INDEX IX_ReportExec_Performance ON ReportExecutionLog(ExecutionTimeMs DESC);
CREATE INDEX IX_ReportExec_Failed ON ReportExecutionLog(Status, ExecutionDate DESC) WHERE Status = 'Failed';




-- ============================================================================
-- SECTION 11: MEDIA MANAGEMENT (CENTRALIZED FILE STORAGE)
-- ============================================================================
-- Replaces: ResponseAttachments, TicketAttachments, TenantExpenses.AttachmentPath
-- Benefits: Unified file storage, deduplication, virus scanning, cloud storage support

-- Media Files (Master file storage for all uploads across the system)
CREATE TABLE MediaFiles (
    FileId BIGINT PRIMARY KEY IDENTITY(1,1),
    
    -- File Identity
    FileName NVARCHAR(255) NOT NULL,           -- Original filename from user
    StoredFileName NVARCHAR(255) NOT NULL,     -- Unique stored name (GUID-based to prevent conflicts)
    FileExtension NVARCHAR(20),                -- .pdf, .jpg, .docx, .xlsx
    MimeType NVARCHAR(100),                    -- application/pdf, image/jpeg, etc.
    
    -- Storage Location
    StorageProvider NVARCHAR(20) NOT NULL DEFAULT 'Local', -- Where file is stored
    StoragePath NVARCHAR(1000) NOT NULL,       -- Physical path or cloud URL
    StorageContainer NVARCHAR(100),            -- Azure blob container / AWS S3 bucket name
    
    -- File Metadata
    FileSize BIGINT NOT NULL,                  -- Size in bytes
    FileSizeFormatted AS (
        CASE 
            WHEN FileSize < 1024 THEN CAST(FileSize AS VARCHAR(20)) + ' B'
            WHEN FileSize < 1048576 THEN CAST(FileSize/1024 AS VARCHAR(20)) + ' KB'
            WHEN FileSize < 1073741824 THEN CAST(FileSize/1048576 AS VARCHAR(20)) + ' MB'
            ELSE CAST(FileSize/1073741824 AS VARCHAR(20)) + ' GB'
        END
    ),
    FileHash NVARCHAR(64),                     -- SHA256 hash for deduplication and integrity verification
    
    -- Image-specific Metadata (if applicable)
    IsImage BIT DEFAULT 0,
    ImageWidth INT,                            -- Pixel width
    ImageHeight INT,                           -- Pixel height
    ThumbnailPath NVARCHAR(1000),              -- Path to auto-generated thumbnail
    
    -- Document-specific Metadata
    PageCount INT,                             -- For PDFs
    DocumentTitle NVARCHAR(500),               -- Extracted document title
    
    -- Security & Access Control
    AccessLevel NVARCHAR(20) DEFAULT 'Private', -- Who can access this file
    IsEncrypted BIT DEFAULT 0,                 -- Is file encrypted at rest?
    EncryptionKey NVARCHAR(500),               -- Encrypted key reference (not actual key!)
    
    -- Virus Scanning
    IsVirusSafe BIT DEFAULT 0,                 -- Has passed antivirus scan?
    VirusScanDate DATETIME2,                   -- When last scanned
    VirusScanResult NVARCHAR(100),             -- Scan result details
    
    -- Lifecycle Management
    UploadedBy INT NOT NULL,
    UploadedDate DATETIME2 DEFAULT GETUTCDATE(),
    LastAccessedDate DATETIME2,                -- Track when file was last downloaded/viewed
    AccessCount INT DEFAULT 0,                 -- How many times accessed
    ExpiryDate DATETIME2,                      -- Auto-delete after this date (for temporary files)
    
    -- Soft Delete
    IsDeleted BIT DEFAULT 0,
    DeletedDate DATETIME2,
    DeletedBy INT,
    DeleteReason NVARCHAR(500),
    
    -- Search & Organization
    Tags NVARCHAR(MAX),                        -- JSON array: ["invoice", "2025", "hardware"]
    SearchableText NVARCHAR(MAX),              -- OCR/extracted text for full-text search
    
    -- Audit
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_MediaFile_Uploader FOREIGN KEY (UploadedBy)
        REFERENCES Users(UserId),
    CONSTRAINT FK_MediaFile_Deleter FOREIGN KEY (DeletedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_MediaFile_Provider CHECK (
        StorageProvider IN ('Local', 'Azure', 'AWS', 'OneDrive', 'SharePoint', 'GoogleDrive')
    ),
    CONSTRAINT CK_MediaFile_Access CHECK (
        AccessLevel IN ('Public', 'Private', 'Internal', 'Confidential', 'Restricted')
    )
);

CREATE INDEX IX_MediaFiles_Hash ON MediaFiles(FileHash) WHERE FileHash IS NOT NULL AND IsDeleted = 0;
CREATE INDEX IX_MediaFiles_Uploader ON MediaFiles(UploadedBy, UploadedDate DESC);
CREATE INDEX IX_MediaFiles_Type ON MediaFiles(MimeType, IsDeleted) WHERE IsDeleted = 0;
CREATE INDEX IX_MediaFiles_Storage ON MediaFiles(StorageProvider, IsDeleted);
CREATE INDEX IX_MediaFiles_Expiry ON MediaFiles(ExpiryDate) WHERE ExpiryDate IS NOT NULL AND IsDeleted = 0;
CREATE INDEX IX_MediaFiles_Images ON MediaFiles(IsImage) WHERE IsImage = 1 AND IsDeleted = 0;
CREATE INDEX IX_MediaFiles_VirusScan ON MediaFiles(IsVirusSafe, VirusScanDate) WHERE IsDeleted = 0;
CREATE FULLTEXT INDEX ON MediaFiles(FileName, Tags, SearchableText);

-- Entity Media Files (Links files to any entity/record in the system - polymorphic association)
CREATE TABLE EntityMediaFiles (
    EntityMediaId BIGINT PRIMARY KEY IDENTITY(1,1),
    
    -- Which file?
    FileId BIGINT NOT NULL,
    
    -- Which entity/record? (Polymorphic relationship)
    EntityType NVARCHAR(50) NOT NULL,          -- Table name: 'Expense', 'Ticket', 'FormResponse', 'Hardware', etc.
    EntityId BIGINT NOT NULL,                  -- Primary key of the record in that table
    
    -- Relationship Metadata
    AttachmentType NVARCHAR(50),               -- 'Receipt', 'Invoice', 'Photo', 'Document', 'Certificate', 'Screenshot'
    DisplayOrder INT DEFAULT 0,                -- For multiple files, display in this order
    IsPrimary BIT DEFAULT 0,                   -- Is this the main/featured file?
    IsRequired BIT DEFAULT 0,                  -- Is this a required attachment?
    Caption NVARCHAR(500),                     -- User-provided description/caption
    
    -- Context Information
    FieldName NVARCHAR(100),                   -- For form responses: which field uploaded this?
    ResponseId BIGINT NULL,                    -- For form responses: link to specific response
    
    -- Audit
    AttachedBy INT NOT NULL,
    AttachedDate DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT DEFAULT 1,
    ModifiedDate DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_EntityMedia_File FOREIGN KEY (FileId)
        REFERENCES MediaFiles(FileId),
    CONSTRAINT FK_EntityMedia_User FOREIGN KEY (AttachedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_EntityMedia_Type CHECK (
        EntityType IN (
            'Expense', 'Budget', 'Ticket', 'FormResponse', 'FormSubmission',
            'Hardware', 'Software', 'User', 'Tenant', 'Training', 
            'Project', 'Maintenance', 'Audit', 'Report', 'Other'
        )
    )
);

CREATE INDEX IX_EntityMedia_Entity ON EntityMediaFiles(EntityType, EntityId, IsActive);
CREATE INDEX IX_EntityMedia_File ON EntityMediaFiles(FileId, IsActive);
CREATE INDEX IX_EntityMedia_Type ON EntityMediaFiles(AttachmentType, EntityType);
CREATE INDEX IX_EntityMedia_Response ON EntityMediaFiles(ResponseId) WHERE ResponseId IS NOT NULL;
CREATE INDEX IX_EntityMedia_Primary ON EntityMediaFiles(EntityType, EntityId, IsPrimary) WHERE IsPrimary = 1 AND IsActive = 1;
CREATE UNIQUE INDEX UQ_EntityMedia_OnePrimary ON EntityMediaFiles(EntityType, EntityId) 
    WHERE IsPrimary = 1 AND IsActive = 1; -- Only one primary file per entity

-- File Access Log (Track who accessed which files for security audit)
CREATE TABLE FileAccessLog (
    AccessLogId BIGINT PRIMARY KEY IDENTITY(1,1),
    FileId BIGINT NOT NULL,
    AccessedBy INT NOT NULL,
    AccessDate DATETIME2 DEFAULT GETUTCDATE(),
    AccessType NVARCHAR(20) NOT NULL,          -- 'View', 'Download', 'Delete', 'Share'
    IPAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),                   -- Browser/device info
    AccessResult NVARCHAR(20) DEFAULT 'Success', -- 'Success', 'Denied', 'NotFound'
    
    CONSTRAINT FK_FileAccess_File FOREIGN KEY (FileId)
        REFERENCES MediaFiles(FileId),
    CONSTRAINT FK_FileAccess_User FOREIGN KEY (AccessedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_FileAccess_Type CHECK (
        AccessType IN ('View', 'Download', 'Delete', 'Update', 'Share', 'Scan')
    ),
    CONSTRAINT CK_FileAccess_Result CHECK (
        AccessResult IN ('Success', 'Denied', 'NotFound', 'Error')
    )
);

CREATE INDEX IX_FileAccess_File ON FileAccessLog(FileId, AccessDate DESC);
CREATE INDEX IX_FileAccess_User ON FileAccessLog(AccessedBy, AccessDate DESC);
CREATE INDEX IX_FileAccess_Date ON FileAccessLog(AccessDate DESC);
CREATE INDEX IX_FileAccess_Type ON FileAccessLog(AccessType, AccessDate DESC);

-- ============================================================================
-- SECTION 12: AUDIT & LOGGING
-- ============================================================================

-- Audit Logs (with Temporal Table support)
CREATE TABLE AuditLogs (
    AuditId BIGINT PRIMARY KEY IDENTITY(1,1),
    TableName NVARCHAR(100) NOT NULL,
    RecordId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- INSERT, UPDATE, DELETE
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX),
    ChangedBy INT,
    ChangedDate DATETIME2 DEFAULT GETDATE(),
    IPAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    CONSTRAINT FK_Audit_User FOREIGN KEY (ChangedBy)
        REFERENCES Users(UserId)
);

CREATE INDEX IX_AuditLogs_Table ON AuditLogs(TableName, RecordId, ChangedDate DESC);
CREATE INDEX IX_AuditLogs_User ON AuditLogs(ChangedBy, ChangedDate DESC);
CREATE INDEX IX_AuditLogs_Date ON AuditLogs(ChangedDate DESC);

-- User Activity Log
CREATE TABLE UserActivityLog (
    ActivityId BIGINT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    ActivityType NVARCHAR(50), -- Login, Logout, View, Create, Update, Delete
    EntityType NVARCHAR(100),
    EntityId INT,
    Description NVARCHAR(500),
    IPAddress NVARCHAR(50),
    DeviceInfo NVARCHAR(500),
    ActivityDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Activity_User FOREIGN KEY (UserId)
        REFERENCES Users(UserId)
);

CREATE INDEX IX_Activity_User ON UserActivityLog(UserId, ActivityDate DESC);
CREATE INDEX IX_Activity_Date ON UserActivityLog(ActivityDate DESC);
-- ============================================================================

-- ============================================================================
-- END OF SCHEMA CREATION
-- ============================================================================
GO

PRINT 'Database schema created successfully!';
PRINT 'Total Tables Created: 72';
PRINT '';
PRINT 'Breakdown by Section:';
PRINT '  - Section 1 (Organizational): 5 tables';
PRINT '  - Section 2 (Identity & Access): 6 tables';
PRINT '  - Section 3 (Metrics & KPIs): 3 tables';
PRINT '  - Section 4 (Forms & Submissions): 18 tables';
PRINT '  - Section 5 (Software): 5 tables';
PRINT '  - Section 6 (Hardware): 4 tables';
PRINT '  - Section 7 (Tickets): 3 tables';
PRINT '  - Section 8 (Financial): 3 tables';
PRINT '  - Section 9 (Notifications & Alerts): 8 tables';
PRINT '  - Section 10 (Reporting & Analytics): 12 tables';
PRINT '  - Section 11 (Media Management): 3 tables';
PRINT '  - Section 12 (Audit & Logging): 2 tables';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Set up user accounts and permissions';
PRINT '2. Configure backup jobs';
PRINT '3. Seed initial data (Regions, Roles, etc.)';
PRINT '4. Deploy to development environment';
PRINT '5. Begin application layer development';

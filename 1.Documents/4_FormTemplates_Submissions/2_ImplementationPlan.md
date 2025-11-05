# Checklist Templates & Submissions - Implementation Plan

**Section:** Section 5 - Checklist Templates & Submissions
**Priority:** HIGHEST (Phase 2 - Weeks 5-9)
**Duration:** 5 weeks
**Team Size:** 2 Senior Developers + 1 Junior Developer + 1 QA
**Dependencies:** Section 1 (Organizational Structure) and Section 2 (Identity & Access) must be completed first

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture Overview](#architecture-overview)
3. [Implementation Phases](#implementation-phases)
4. [Week-by-Week Breakdown](#week-by-week-breakdown)
5. [Database Schema](#database-schema)
6. [Project Structure](#project-structure)
7. [Key Components](#key-components)
8. [Testing Strategy](#testing-strategy)
9. [Deployment Plan](#deployment-plan)
10. [Success Criteria](#success-criteria)

---

## Overview

### What We're Building

The **Checklist Templates & Submissions** system is the core of the KTDA ICT Reporting System. It enables:

1. **Template Builder** - Admins create dynamic forms with sections and questions
2. **Form Rendering** - System generates HTML forms from templates
3. **Data Capture** - Factory staff fill forms with auto-save and pre-fill
4. **Approval Workflow** - Regional managers review and approve submissions
5. **Reporting** - Automated consolidation and analytics

### Why This is Phase 2 (Highest Priority)

- ✅ **Data originates here** - All operational metrics come from checklist submissions
- ✅ **Replaces 90% of manual work** - Eliminates Excel/Word email chains
- ✅ **Most complex feature** - Needs early validation and iteration
- ✅ **Enables other modules** - Metrics, reports depend on this

### Key Statistics

- **9 database tables** - ChecklistCategories, ChecklistTemplates, ChecklistTemplateSections, ChecklistTemplateItems, ChecklistTemplateSubmissions, ChecklistTemplateResponses, TenantGroups, TenantGroupMembers, ChecklistTemplateAssignments
- **7 field types** - Text, Number, Date, Boolean, Dropdown, TextArea, FileUpload
- **4 workflow states** - Draft, Submitted, Approved, Rejected
- **4 assignment types** - All tenants, TenantType, TenantGroup, SpecificTenant
- **74% auto-fill** - Pre-filled from hardware, software, ticket data
- **3-5 templates initially** - Factory Monthly, Factory Daily, Regional Weekly, Incident Report

---

## Architecture Overview

### High-Level Components

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                       │
│  (ASP.NET Core MVC + Razor Pages + jQuery + Bootstrap)     │
├─────────────────────────────────────────────────────────────┤
│ TemplateBuilderController  │  ChecklistController          │
│ - Create template           │  - Fill form                  │
│ - Add sections/items        │  - Save draft                 │
│ - Preview                   │  - Submit                     │
│ - Publish                   │  - View submission            │
│                             │                               │
│ ApprovalController          │  ReportController             │
│ - Pending approvals         │  - Monthly summary            │
│ - Review submission         │  - Regional dashboard         │
│ - Approve/Reject            │  - Export reports             │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   APPLICATION LAYER                         │
│              (Services, DTOs, Validators)                   │
├─────────────────────────────────────────────────────────────┤
│ TemplateBuilderService      │  ChecklistService            │
│ - CreateTemplate()          │  - CreateDraftSubmission()   │
│ - AddSection()              │  - SaveResponses()           │
│ - AddItem()                 │  - SubmitForApproval()       │
│ - ValidateTemplate()        │  - GetSubmission()           │
│                             │                               │
│ FieldRendererService        │  PreFillService              │
│ - RenderField()             │  - PreFillResponses()        │
│ - RenderSection()           │  - GetPreFillValue()         │
│ - RenderForm()              │  - MapToInventory()          │
│                             │                               │
│ ApprovalService             │  ReportingService            │
│ - GetPendingApprovals()     │  - GenerateMonthlyReport()   │
│ - Approve()                 │  - GetRegionalSummary()      │
│ - Reject()                  │  - ExportToExcel()           │
│ - NotifyStakeholders()      │  - GenerateCharts()          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                     DOMAIN LAYER                            │
│               (Entities, Interfaces, Enums)                 │
├─────────────────────────────────────────────────────────────┤
│ Entities:                   │  Enums:                      │
│ - ChecklistTemplate         │  - SubmissionStatus          │
│ - ChecklistSection          │  - FieldDataType             │
│ - ChecklistItem             │  - TemplateType              │
│ - ChecklistSubmission       │                              │
│ - ChecklistResponse         │  Interfaces:                 │
│                             │  - IChecklistRepository      │
│                             │  - ITemplateRepository       │
│                             │  - ISubmissionRepository     │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                 INFRASTRUCTURE LAYER                        │
│       (EF Core, Repositories, External Services)            │
├─────────────────────────────────────────────────────────────┤
│ Repositories:               │  External Services:          │
│ - ChecklistRepository       │  - EmailService (Hangfire)   │
│ - TemplateRepository        │  - NotificationHub (SignalR) │
│ - SubmissionRepository      │  - FileStorageService        │
│ - ResponseRepository        │                              │
│                             │                              │
│ Database Context:           │  Background Jobs:            │
│ - KTDADbContext            │  - AutoSaveJob               │
│ - DbSets for all entities   │  - ReminderJob               │
│                             │  - PreAggregationJob         │
└─────────────────────────────────────────────────────────────┘
```

---

## Implementation Phases

### Phase 2.1: Template Builder (Weeks 5-6) - 2 weeks

**Goal:** Enable admins to create dynamic checklist templates

**Deliverables:**
- ✅ Template CRUD (Create, Read, Update, Delete)
- ✅ Section management with drag-drop reordering
- ✅ Field/item management with 7 field types
- ✅ Validation rules configuration
- ✅ Conditional logic (show/hide fields)
- ✅ Template preview functionality
- ✅ Template versioning

**Detailed Document:** [FormBuilder_Implementation.md](FormBuilder_Implementation.md)

---

### Phase 2.2: Form Rendering & Data Capture (Weeks 7-8) - 2 weeks

**Goal:** Factory staff can fill checklists with pre-filled data

**Deliverables:**
- ✅ Dynamic form generation from templates
- ✅ Client-side validation (jQuery Validation)
- ✅ Server-side validation
- ✅ Auto-save drafts (every 30 seconds)
- ✅ Pre-fill service (74% automation)
- ✅ Progress indicator
- ✅ File upload handling
- ✅ Form submission

**Detailed Document:** [FormRendering_Implementation.md](FormRendering_Implementation.md)

---

### Phase 2.3: Approval Workflow (Week 9) - 1 week

**Goal:** Regional managers review and approve submissions

**Deliverables:**
- ✅ Pending approvals dashboard
- ✅ Submission review interface
- ✅ Compare with previous submissions
- ✅ Approve/Reject actions with comments
- ✅ Email notifications (Hangfire)
- ✅ Real-time notifications (SignalR)
- ✅ Submission history view
- ✅ Bulk approval (optional)

**Detailed Document:** [ApprovalWorkflow_Implementation.md](ApprovalWorkflow_Implementation.md)

---

### Phase 2.4: Reports & Dashboards (Week 10) - 1 week (parallel with other phases)

**Goal:** Automated report generation and analytics

**Deliverables:**
- ✅ Monthly summary reports (per factory)
- ✅ Regional consolidated reports
- ✅ Head Office dashboard
- ✅ Export to Excel/PDF
- ✅ Trend analysis charts (Chart.js)
- ✅ Compliance tracking
- ✅ Submission status tracking

**Detailed Document:** [Reports_Dashboard_Implementation.md](Reports_Dashboard_Implementation.md)

---

## Week-by-Week Breakdown

### Week 5: Template Builder Foundation

**Days 1-2: Database & Backend**
- Create migration for all 9 checklist tables (ChecklistCategories, ChecklistTemplates, ChecklistTemplateSections, ChecklistTemplateItems, ChecklistTemplateSubmissions, ChecklistTemplateResponses, TenantGroups, TenantGroupMembers, ChecklistTemplateAssignments)
- Implement repositories (TemplateRepository, SectionRepository, ItemRepository, AssignmentRepository)
- Create domain entities with EF Core configuration
- Write unit tests for repositories
- Seed initial categories, templates, and tenant groups

**Days 3-4: Template CRUD UI**
- Create TemplateBuilderController
- Build template list page (View, Edit, Delete, Clone)
- Build create template modal
- Implement template activation/deactivation
- Add template versioning logic

**Day 5: Section Management**
- Create section CRUD endpoints
- Build section UI with SortableJS drag-drop
- Implement section reordering
- Add icon picker for sections
- Test section management

---

### Week 6: Field Builder & Configuration

**Days 1-2: Field Type Implementation**
- Create field configuration modals for each type:
  - Text (maxLength, regex)
  - Number (min, max, decimals)
  - Date (min, max, default)
  - Boolean (display type)
  - Dropdown (reference data / custom)
  - TextArea (rows, maxLength)
  - FileUpload (types, size)
- Implement field toolbox (drag from left panel)
- Add field to section logic

**Days 3-4: Advanced Features**
- Validation rules builder UI
- Conditional logic builder (show/hide based on other fields)
- Default value configuration
- Pre-fill source mapping (link to hardware/software tables)
- Help text and placeholder configuration

**Day 5: Preview & Publish**
- Build preview modal (shows form as users will see it)
- Implement publish/unpublish functionality
- Template validation before publish
- Integration testing

---

### Week 7: Form Rendering Engine

**Days 1-2: Rendering Service**
- Create FieldRendererService (7 field types)
- Implement RenderSection() method
- Implement RenderForm() method
- Handle validation rules in HTML
- Handle conditional logic in JavaScript
- Write unit tests for rendering

**Days 3-4: User Form UI**
- Create ChecklistController
- Build FillForm.cshtml view
- Implement accordion sections (Bootstrap)
- Add progress indicator (% completion)
- Client-side validation (jQuery Validation)
- Add help tooltips and placeholders

**Day 5: Submission Logic**
- Create ChecklistSubmission on form open
- Save responses as user types
- Validate before submission
- Server-side validation
- Handle file uploads
- Integration testing

---

### Week 8: Pre-fill & Auto-save

**Days 1-2: Pre-fill Service**
- Implement ChecklistPreFillService
- Add 16 pre-fill methods (hardware, software, tickets, etc.)
- Pattern matching for question inference
- Test pre-fill accuracy

**Days 3-4: Auto-save & Draft Management**
- Implement auto-save JavaScript (30-second interval)
- Create SaveDraft endpoint
- Handle concurrent edits (optimistic locking)
- Draft recovery (resume editing)
- Draft expiration (30 days)

**Day 5: Form Submission & Validation**
- Full form validation (client + server)
- SubmitForApproval() service method
- Status change: Draft → Submitted
- Email notification to regional manager
- User feedback (success/error messages)
- Integration testing

---

### Week 9: Approval Workflow

**Days 1-2: Approval Dashboard**
- Create ApprovalController
- Build PendingApprovals.cshtml view
- Filter by region, status, date
- Sorting and pagination (DataTables.js)
- Badge notifications (count pending)

**Days 3-4: Review & Actions**
- Build ReviewSubmission.cshtml view
- Display all responses grouped by section
- Compare with previous month (side-by-side)
- Approve/Reject modal with comments
- Implement ApprovalService (approve/reject logic)
- Status change: Submitted → Approved/Rejected

**Day 5: Notifications & History**
- Hangfire background jobs for emails
- SignalR real-time notifications
- Submission history view (all statuses)
- Resubmission flow (Rejected → Draft → Submitted)
- Integration testing

---

### Week 10: Reports & Dashboards (Parallel Development)

**Days 1-2: Monthly Reports**
- Create ReportController
- Build MonthlyReport.cshtml view
- Query responses with aggregation
- Generate factory-level summary
- Export to Excel (EPPlus/ClosedXML)

**Days 3-4: Regional Dashboard**
- Build RegionalDashboard.cshtml
- Aggregate data across factories
- Create charts (Chart.js):
  - Network uptime trend
  - Ticket resolution time
  - Hardware status breakdown
  - Budget utilization
- Real-time dashboard updates

**Day 5: Head Office Dashboard**
- Build HeadOfficeDashboard.cshtml
- Aggregate data across all regions
- KPI cards (total computers, uptime, tickets)
- Top performing factories
- Factories needing attention
- Export consolidated report

---

## Database Schema

### Tables Created in Phase 2

```sql
-- 1. ChecklistCategories
CREATE TABLE ChecklistCategories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    CategoryCode NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    DisplayOrder INT DEFAULT 0,
    IconClass NVARCHAR(50),
    Color NVARCHAR(20),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE()
);

-- 2. ChecklistTemplates
CREATE TABLE ChecklistTemplates (
    TemplateId INT PRIMARY KEY IDENTITY(1,1),
    CategoryId INT NOT NULL,
    TemplateName NVARCHAR(200) NOT NULL,
    TemplateCode NVARCHAR(50) UNIQUE NOT NULL,
    Description NVARCHAR(1000),
    TemplateType NVARCHAR(50) NOT NULL,
    Version INT DEFAULT 1,
    IsActive BIT DEFAULT 1,
    RequiresApproval BIT DEFAULT 1,
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Template_Category FOREIGN KEY (CategoryId)
        REFERENCES ChecklistCategories(CategoryId)
);

-- 3. ChecklistTemplateSections
CREATE TABLE ChecklistTemplateSections (
    SectionId INT PRIMARY KEY IDENTITY(1,1),
    TemplateId INT NOT NULL,
    SectionName NVARCHAR(100) NOT NULL,
    SectionDescription NVARCHAR(500),
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsCollapsible BIT DEFAULT 1,
    IsCollapsedByDefault BIT DEFAULT 0,
    IconClass NVARCHAR(50),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Section_Template FOREIGN KEY (TemplateId)
        REFERENCES ChecklistTemplates(TemplateId) ON DELETE CASCADE
);

-- 3. ChecklistTemplateItems
CREATE TABLE ChecklistTemplateItems (
    ItemId INT PRIMARY KEY IDENTITY(1,1),
    TemplateId INT NOT NULL,
    SectionId INT NOT NULL,
    ItemCode NVARCHAR(50) NOT NULL,
    ItemName NVARCHAR(200) NOT NULL,
    ItemDescription NVARCHAR(1000),
    DisplayOrder INT DEFAULT 0,
    DataType NVARCHAR(50),
    IsRequired BIT DEFAULT 0,
    DefaultValue NVARCHAR(500),
    ValidationRules NVARCHAR(MAX), -- JSON
    ConditionalLogic NVARCHAR(MAX), -- JSON
    PreFillSource NVARCHAR(100), -- Maps to PreFillService method
    Version INT DEFAULT 1,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Item_Template FOREIGN KEY (TemplateId)
        REFERENCES ChecklistTemplates(TemplateId),
    CONSTRAINT FK_Item_Section FOREIGN KEY (SectionId)
        REFERENCES ChecklistTemplateSections(SectionId) ON DELETE NO ACTION
);

-- 4. ChecklistTemplateSubmissions
CREATE TABLE ChecklistTemplateSubmissions (
    SubmissionId INT PRIMARY KEY IDENTITY(1,1),
    TemplateId INT NOT NULL,
    TenantId INT NOT NULL,
    ReportingYear INT NOT NULL,
    ReportingMonth TINYINT NOT NULL,
    ReportingPeriod DATE NOT NULL,
    SnapshotDate DATE NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    SubmittedBy INT,
    SubmittedDate DATETIME2,
    ReviewedBy INT,
    ReviewedDate DATETIME2,
    ApprovalComments NVARCHAR(MAX),
    RejectionReason NVARCHAR(MAX),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Submission_Template FOREIGN KEY (TemplateId)
        REFERENCES ChecklistTemplates(TemplateId),
    CONSTRAINT FK_Submission_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT UQ_TenantTemplate_Period UNIQUE (TenantId, TemplateId, ReportingPeriod)
);

-- 5. ChecklistTemplateResponses
CREATE TABLE ChecklistTemplateResponses (
    ResponseId BIGINT PRIMARY KEY IDENTITY(1,1),
    SubmissionId INT NOT NULL,
    ItemId INT NOT NULL,
    TextValue NVARCHAR(MAX),
    NumericValue DECIMAL(18,4),
    DateValue DATE,
    BooleanValue BIT,
    FileUrl NVARCHAR(500),
    IsPreFilled BIT DEFAULT 0, -- Track if auto-filled
    Remarks NVARCHAR(1000),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Response_Submission FOREIGN KEY (SubmissionId)
        REFERENCES ChecklistTemplateSubmissions(SubmissionId) ON DELETE CASCADE,
    CONSTRAINT FK_Response_Item FOREIGN KEY (ItemId)
        REFERENCES ChecklistTemplateItems(ItemId),
    CONSTRAINT UQ_SubmissionItem UNIQUE (SubmissionId, ItemId)
);

-- 6. TenantGroups (Flexible Template Assignment)
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

-- 7. TenantGroupMembers (Many-to-Many: TenantGroups ↔ Tenants)
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

-- 8. ChecklistTemplateAssignments (4 Assignment Types)
CREATE TABLE ChecklistTemplateAssignments (
    AssignmentId INT PRIMARY KEY IDENTITY(1,1),
    TemplateId INT NOT NULL,
    AssignmentType NVARCHAR(20) NOT NULL, -- 'All', 'TenantType', 'TenantGroup', 'SpecificTenant'
    TenantType NVARCHAR(20) NULL,
    TenantGroupId INT NULL,
    TenantId INT NULL,
    AssignedBy INT NOT NULL,
    AssignedDate DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT DEFAULT 1,
    Notes NVARCHAR(500),
    CONSTRAINT FK_TemplateAssignment_Template FOREIGN KEY (TemplateId)
        REFERENCES ChecklistTemplates(TemplateId) ON DELETE CASCADE,
    CONSTRAINT FK_TemplateAssignment_TenantGroup FOREIGN KEY (TenantGroupId)
        REFERENCES TenantGroups(TenantGroupId),
    CONSTRAINT FK_TemplateAssignment_Tenant FOREIGN KEY (TenantId)
        REFERENCES Tenants(TenantId),
    CONSTRAINT FK_TemplateAssignment_AssignedBy FOREIGN KEY (AssignedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_TemplateAssignment_Type CHECK (
        AssignmentType IN ('All', 'TenantType', 'TenantGroup', 'SpecificTenant')
    ),
    CONSTRAINT CK_TemplateAssignment_Target CHECK (
        (AssignmentType = 'All' AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL) OR
        (AssignmentType = 'TenantType' AND TenantType IS NOT NULL AND TenantGroupId IS NULL AND TenantId IS NULL) OR
        (AssignmentType = 'TenantGroup' AND TenantType IS NULL AND TenantGroupId IS NOT NULL AND TenantId IS NULL) OR
        (AssignmentType = 'SpecificTenant' AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NOT NULL)
    )
);
```

---

## Project Structure

### ASP.NET Core Solution Organization

```
KTDAFieldReports/
├── src/
│   ├── KTDAFieldReports.Web/                # Presentation
│   │   ├── Controllers/
│   │   │   ├── TemplateBuilderController.cs
│   │   │   ├── ChecklistController.cs
│   │   │   ├── ApprovalController.cs
│   │   │   └── ReportController.cs
│   │   ├── Views/
│   │   │   ├── TemplateBuilder/
│   │   │   │   ├── Index.cshtml
│   │   │   │   ├── Builder.cshtml
│   │   │   │   └── Preview.cshtml
│   │   │   ├── Checklist/
│   │   │   │   ├── FillForm.cshtml
│   │   │   │   ├── ViewSubmission.cshtml
│   │   │   │   └── MySubmissions.cshtml
│   │   │   ├── Approval/
│   │   │   │   ├── PendingApprovals.cshtml
│   │   │   │   ├── ReviewSubmission.cshtml
│   │   │   │   └── History.cshtml
│   │   │   └── Report/
│   │   │       ├── MonthlyReport.cshtml
│   │   │       ├── RegionalDashboard.cshtml
│   │   │       └── HeadOfficeDashboard.cshtml
│   │   ├── wwwroot/
│   │   │   ├── js/
│   │   │   │   ├── template-builder.js
│   │   │   │   ├── form-renderer.js
│   │   │   │   ├── auto-save.js
│   │   │   │   └── approval.js
│   │   │   ├── css/
│   │   │   │   └── checklist.css
│   │   │   └── lib/
│   │   │       └── sortablejs/
│   │   │           └── sortable.min.js
│   │   └── Program.cs
│   │
│   ├── KTDAFieldReports.Application/        # Business Logic
│   │   ├── Services/
│   │   │   ├── TemplateBuilderService.cs
│   │   │   ├── ChecklistService.cs
│   │   │   ├── FieldRendererService.cs
│   │   │   ├── PreFillService.cs
│   │   │   ├── ApprovalService.cs
│   │   │   └── ReportingService.cs
│   │   ├── DTOs/
│   │   │   ├── TemplateDto.cs
│   │   │   ├── SectionDto.cs
│   │   │   ├── ItemDto.cs
│   │   │   ├── SubmissionDto.cs
│   │   │   └── ResponseDto.cs
│   │   └── Validators/
│   │       ├── TemplateValidator.cs
│   │       ├── SubmissionValidator.cs
│   │       └── ResponseValidator.cs
│   │
│   ├── KTDAFieldReports.Core/               # Domain
│   │   ├── Entities/
│   │   │   ├── ChecklistTemplate.cs
│   │   │   ├── ChecklistSection.cs
│   │   │   ├── ChecklistItem.cs
│   │   │   ├── ChecklistSubmission.cs
│   │   │   └── ChecklistResponse.cs
│   │   ├── Enums/
│   │   │   ├── SubmissionStatus.cs
│   │   │   ├── FieldDataType.cs
│   │   │   └── TemplateType.cs
│   │   └── Interfaces/
│   │       ├── IChecklistRepository.cs
│   │       ├── ITemplateRepository.cs
│   │       └── ISubmissionRepository.cs
│   │
│   └── KTDAFieldReports.Infrastructure/     # Data Access
│       ├── Data/
│       │   ├── KTDADbContext.cs
│       │   └── Configurations/
│       │       ├── ChecklistTemplateConfiguration.cs
│       │       ├── ChecklistSectionConfiguration.cs
│       │       ├── ChecklistItemConfiguration.cs
│       │       ├── ChecklistSubmissionConfiguration.cs
│       │       └── ChecklistResponseConfiguration.cs
│       ├── Repositories/
│       │   ├── ChecklistRepository.cs
│       │   ├── TemplateRepository.cs
│       │   └── SubmissionRepository.cs
│       └── Migrations/
│           └── 20251030_CreateChecklistTables.cs
```

---

## Key Components

### 1. Template Builder Components

| Component | Purpose | Lines of Code (Estimate) |
|-----------|---------|--------------------------|
| TemplateBuilderController | Handle template CRUD | ~300 |
| TemplateBuilderService | Business logic | ~500 |
| Builder.cshtml | Three-panel UI | ~400 |
| template-builder.js | Drag-drop, AJAX | ~600 |
| SortableJS library | Drag-drop (external) | ~500 (minified) |

**Total: ~2,300 lines** (excluding libraries)

---

### 2. Form Rendering Components

| Component | Purpose | Lines of Code (Estimate) |
|-----------|---------|--------------------------|
| ChecklistController | Handle form fill/submit | ~400 |
| ChecklistService | Form lifecycle | ~600 |
| FieldRendererService | Generate HTML | ~800 |
| PreFillService | Auto-fill logic | ~700 |
| FillForm.cshtml | User form UI | ~300 |
| form-renderer.js | Client-side logic | ~400 |
| auto-save.js | Draft auto-save | ~200 |

**Total: ~3,400 lines**

---

### 3. Approval Workflow Components

| Component | Purpose | Lines of Code (Estimate) |
|-----------|---------|--------------------------|
| ApprovalController | Review/approve | ~300 |
| ApprovalService | Workflow logic | ~400 |
| PendingApprovals.cshtml | Dashboard | ~250 |
| ReviewSubmission.cshtml | Review UI | ~350 |
| approval.js | Approve/reject actions | ~200 |
| EmailService (Hangfire) | Notifications | ~300 |
| NotificationHub (SignalR) | Real-time | ~150 |

**Total: ~1,950 lines**

---

### 4. Reports & Dashboard Components

| Component | Purpose | Lines of Code (Estimate) |
|-----------|---------|--------------------------|
| ReportController | Generate reports | ~400 |
| ReportingService | Data aggregation | ~700 |
| MonthlyReport.cshtml | Factory report | ~300 |
| RegionalDashboard.cshtml | Regional view | ~400 |
| HeadOfficeDashboard.cshtml | HO view | ~450 |
| Chart generation (Chart.js) | Visualizations | ~300 |
| Excel export (EPPlus) | Export logic | ~250 |

**Total: ~2,800 lines**

---

**Grand Total: ~10,450 lines of code** (Phase 2 - Checklist System)

---

## Testing Strategy

### Unit Tests

**Coverage Target: 80%**

```csharp
// Example: FieldRendererService unit tests
[TestClass]
public class FieldRendererServiceTests
{
    [TestMethod]
    public void RenderNumberField_WithValidation_GeneratesCorrectHtml()
    {
        // Arrange
        var item = new ChecklistItem
        {
            ItemId = 1,
            ItemName = "Total computers",
            DataType = "Number",
            ValidationRules = "{\"required\": true, \"min\": 1, \"max\": 500}"
        };
        var renderer = new FieldRendererService();

        // Act
        var html = renderer.RenderField(item);

        // Assert
        Assert.IsTrue(html.Contains("type='number'"));
        Assert.IsTrue(html.Contains("min='1'"));
        Assert.IsTrue(html.Contains("max='500'"));
        Assert.IsTrue(html.Contains("required"));
    }

    [TestMethod]
    public void RenderDropdownField_WithReferenceData_LoadsOptions()
    {
        // Arrange
        var item = new ChecklistItem
        {
            ItemId = 2,
            ItemName = "WAN Type",
            DataType = "Dropdown",
            ValidationRules = "{\"referenceTypeId\": 1}"
        };
        var renderer = new FieldRendererService(_referenceRepoMock.Object);

        // Act
        var html = renderer.RenderField(item);

        // Assert
        Assert.IsTrue(html.Contains("<select"));
        Assert.IsTrue(html.Contains("Fiber Optic"));
        Assert.IsTrue(html.Contains("Microwave"));
    }
}
```

**Test Coverage:**
- TemplateBuilderService: 15 test methods
- ChecklistService: 20 test methods
- FieldRendererService: 12 test methods
- PreFillService: 18 test methods
- ApprovalService: 10 test methods
- ReportingService: 15 test methods

**Total: ~90 unit tests**

---

### Integration Tests

**Scenarios:**
1. Create template → Add sections → Add items → Preview → Publish
2. User opens form → Pre-fill executes → User fills remaining → Submit
3. Manager receives notification → Reviews → Approves → User notified
4. Query approved submissions → Generate report → Export to Excel

**Tools:**
- xUnit for test framework
- Moq for mocking
- EF Core InMemory database for integration tests
- Selenium for UI tests (optional)

---

### User Acceptance Testing (UAT)

**Week 10: UAT with 5 factories**

**Test Scenarios:**
1. Admin creates "Factory Monthly Report" template with 33 questions
2. 5 factories fill forms (1 from each region)
3. Regional managers approve submissions
4. Generate consolidated report
5. Validate accuracy (compare with manual Excel report)

**Success Criteria:**
- All 5 submissions complete without errors
- Pre-fill accuracy ≥ 90%
- Approval workflow completes in < 5 minutes
- Report matches manual Excel (±2% variance)

---

## Deployment Plan

### Week 10: Deployment to Staging

**Steps:**
1. Run database migrations on staging database
2. Deploy application to staging IIS
3. Seed initial templates (Factory Monthly, Factory Daily)
4. Configure Hangfire background jobs
5. Configure SignalR hub
6. Test end-to-end workflow

### Week 11: Production Deployment

**Steps:**
1. Backup production database
2. Run database migrations
3. Deploy application to production IIS
4. Smoke test (create template, fill form, approve)
5. Monitor logs for errors
6. Rollback plan ready (database restore + previous version)

### Rollback Plan

```sql
-- If deployment fails, restore database
RESTORE DATABASE KTDA_ICT_Reporting
FROM DISK = 'C:\Backups\KTDA_ICT_Reporting_PreChecklistDeploy.bak'
WITH REPLACE;
```

Then redeploy previous application version from IIS backup.

---

## Success Criteria

### Functional Requirements ✅

- [ ] Admin can create templates with 10+ sections and 50+ questions
- [ ] Admin can configure 7 field types with validation
- [ ] Admin can add conditional logic (show/hide)
- [ ] Admin can preview template before publishing
- [ ] Factory staff can fill forms with ≥70% pre-filled
- [ ] Auto-save works every 30 seconds
- [ ] User can submit draft for approval
- [ ] Regional manager receives email notification
- [ ] Manager can approve/reject with comments
- [ ] User receives approval notification
- [ ] Rejected submissions can be edited and resubmitted
- [ ] Approved submissions are read-only (immutable)
- [ ] Reports show accurate aggregated data
- [ ] Export to Excel works

### Non-Functional Requirements ✅

- [ ] Form loads in < 2 seconds (with 50 questions)
- [ ] Auto-save completes in < 500ms
- [ ] Submission approval completes in < 3 seconds
- [ ] Report generation completes in < 10 seconds (100 submissions)
- [ ] System supports 100 concurrent users
- [ ] 99.5% uptime during business hours
- [ ] Database backup runs nightly
- [ ] All user actions logged (audit trail)

### Business Requirements ✅

- [ ] Reduces manual data entry time by ≥60%
- [ ] Eliminates Excel/email workflows
- [ ] Provides real-time visibility for managers
- [ ] Enables month-over-month trend analysis
- [ ] Supports compliance auditing
- [ ] Training completed for 20 admins and 100 factory staff

---

## Risk Mitigation

### Risk 1: Complexity Underestimated

**Mitigation:**
- Start with simplest template (5 questions, 1 section)
- Add complexity incrementally
- Weekly demos to stakeholders
- Buffer time in Week 10 for iterations

### Risk 2: Pre-fill Accuracy Issues

**Mitigation:**
- Test pre-fill with real data from 3 factories
- Allow users to override all pre-filled values
- Track pre-fill accuracy metrics
- Improve algorithms based on feedback

### Risk 3: User Adoption Resistance

**Mitigation:**
- Involve factory ICT staff in UAT
- Provide comprehensive training (videos + manuals)
- Offer help desk support during first month
- Show time savings (before/after comparison)

### Risk 4: Performance Issues

**Mitigation:**
- Load test with 1,000 concurrent users
- Optimize database queries (indexes)
- Implement caching (Redis if needed)
- Profile slow operations

---

## Dependencies

### External Libraries

| Library | Version | Purpose | License |
|---------|---------|---------|---------|
| SortableJS | 1.15.0 | Drag-drop | MIT |
| jQuery Validation | 1.19.5 | Client validation | MIT |
| Chart.js | 4.0 | Charts | MIT |
| DataTables.js | 2.0 | Sortable tables | MIT |
| EPPlus | 6.0 | Excel export | Polyform Noncommercial |
| Hangfire | 1.8 | Background jobs | LGPL |
| SignalR | 8.0 | Real-time | MIT |

**Total Library Cost: $0** (all free/open-source for internal use)

---

## Team Responsibilities

### Senior Developer 1 (Backend Lead)

**Weeks 5-6:**
- Database schema and migrations
- Repository pattern implementation
- TemplateBuilderService, ChecklistService
- Unit tests for services

**Weeks 7-8:**
- FieldRendererService, PreFillService
- Integration with hardware/software repositories
- Server-side validation
- Performance optimization

**Week 9:**
- ApprovalService implementation
- Hangfire job configuration
- Email notifications
- Code review

### Senior Developer 2 (Frontend Lead)

**Weeks 5-6:**
- Template builder UI (three-panel layout)
- SortableJS integration
- AJAX operations for template CRUD
- Field configuration modals

**Weeks 7-8:**
- Form rendering UI (FillForm.cshtml)
- Auto-save JavaScript
- Client-side validation
- Progress indicator

**Week 9:**
- Approval UI (dashboard, review)
- SignalR real-time notifications
- UI polish and responsiveness
- Browser testing

### Junior Developer

**Weeks 5-9:**
- Assist with UI components
- Write DTOs and view models
- Create seed data scripts
- Write basic unit tests
- Bug fixing
- Documentation

### QA Engineer

**Weeks 5-9:**
- Write test plans
- Execute integration tests
- Manual testing (all user scenarios)
- Performance testing
- UAT coordination
- Bug tracking and verification

---

## Next Steps

1. **Review this plan** with development team and stakeholders
2. **Approve resource allocation** (2 seniors + 1 junior + 1 QA)
3. **Set up development environment** (Week 1 of project)
4. **Kick off Phase 2** (Week 5 - start template builder)
5. **Read detailed implementation documents**:
   - [FormBuilder_Implementation.md](FormBuilder_Implementation.md)
   - [FormRendering_Implementation.md](FormRendering_Implementation.md)
   - [ApprovalWorkflow_Implementation.md](ApprovalWorkflow_Implementation.md)
   - [Reports_Dashboard_Implementation.md](Reports_Dashboard_Implementation.md)

---

**Document Version:** 1.0
**Last Updated:** October 30, 2025
**Maintained By:** KTDA ICT Development Team
**Questions?** Contact Project Manager or Technical Lead

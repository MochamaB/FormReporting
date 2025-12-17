# Assignment & Workflow UI Implementation Guide

## Overview

This document outlines the UI structure for managing Form Template Assignments and Workflows. These components are embedded as **partial views** within the Template Details page.

---

## Architecture

### Where They Live

These are **partial views (components)** that render inside the **Template Details page** as tab content, not separate full pages.

```
┌─────────────────────────────────────────────────────────────────────┐
│ FormTemplates/Details/{templateId}                    (FULL PAGE)   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Template Header: "Monthly Production Report"                       │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  [Overview] [Structure] [Assignments] [Workflow] [Metrics] [Subs]   │
│  ═══════════════════════════════════════════════════════════════    │
│                                                                     │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │                                                               │  │
│  │   TAB CONTENT (Partial Views)                                 │  │
│  │                                                               │  │
│  │   • Overview Tab    → _OverviewPanel.cshtml (includes stats)  │  │
│  │   • Structure Tab   → _StructurePanel.cshtml                  │  │
│  │   • Assignments Tab → _FormAssignmentPanel.cshtml  ◄──────    │  │
│  │   • Workflow Tab    → _WorkflowBuilderPanel.cshtml ◄──────    │  │
│  │   • Metrics Tab     → _MetricMappingPanel.cshtml              │  │
│  │   • Submissions Tab → _SubmissionsPanel.cshtml                │  │
│  │                                                               │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Component Types

| Component | Type | Usage |
|-----------|------|-------|
| `_FormAssignmentPanel.cshtml` | **Partial View** | Embedded in Template Details tab, list + actions |
| `_WorkflowBuilderPanel.cshtml` | **Partial View** | Embedded in Template Details tab, visual designer |
| `_CreateAssignmentForm.cshtml` | **Standalone Partial** | Can be used in views OR modals |
| `_EditAssignmentForm.cshtml` | **Standalone Partial** | Can be used in views OR modals |
| `_CreateWorkflowForm.cshtml` | **Standalone Partial** | Can be used in views OR modals |
| `_WorkflowStepForm.cshtml` | **Standalone Partial** | Can be used in views OR modals |
| `WorkflowProgressTimeline` | **ViewComponent** | Reusable across Dashboard, Submission Details, etc. |

### Design Principles

1. **Standalone Form Partials** - Create/Edit forms are standalone partials that can be:
   - Embedded directly in a view (full page)
   - Loaded into a modal via AJAX
   - Used in different contexts

2. **No Stats in Panels** - All statistics go in the Overview tab, panels focus on CRUD

3. **Vertical Wizards** - All multi-step forms use the existing `_VerticalWizard.cshtml` component

4. **Reusable Components** - WorkflowProgressTimeline is a ViewComponent for reuse

---

## File Structure

```
Views/
└── Forms/
    └── FormTemplates/
        ├── Details.cshtml                          # Main page with tabs
        │
        ├── Panels/                                 # Tab content partials
        │   ├── _OverviewPanel.cshtml               # Stats + summary (ALL stats here)
        │   ├── _StructurePanel.cshtml              # Sections/fields preview
        │   ├── _FormAssignmentPanel.cshtml         # Assignment list + actions
        │   ├── _WorkflowBuilderPanel.cshtml        # Workflow designer
        │   ├── _MetricMappingPanel.cshtml          # KPI mappings
        │   └── _SubmissionsPanel.cshtml            # Submission list
        │
        └── Forms/                                  # Standalone form partials
            ├── _CreateAssignmentForm.cshtml        # Assignment wizard (uses _VerticalWizard)
            ├── _EditAssignmentForm.cshtml          # Edit assignment form
            ├── _CreateWorkflowForm.cshtml          # Create workflow form
            └── _WorkflowStepForm.cshtml            # Add/Edit step form

ViewComponents/
├── WorkflowProgressTimelineViewComponent.cs        # Timeline component logic

Views/
└── Shared/
    └── Components/
        └── WorkflowProgressTimeline/
            └── Default.cshtml                      # Timeline component view
```

---

## FormAssignmentPanel

### Purpose
Manage template assignments with all 8 assignment types, reporting periods, due dates, recurrence, and bulk operations.

### Visual Structure (No Stats - Stats in Overview Tab)

```
┌─────────────────────────────────────────────────────────────────────┐
│ _FormAssignmentPanel.cshtml                                         │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ HEADER                                                       │   │
│  │ ┌─────────────────────────────────────────────────────────┐ │   │
│  │ │ Assignments (12)              [+ Create Assignment]     │ │   │
│  │ └─────────────────────────────────────────────────────────┘ │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ FILTERS & BULK ACTIONS                                       │   │
│  │ ┌─────────────────────────────────────────────────────────┐ │   │
│  │ │ [Type ▼] [Status ▼] [Period ▼]    [Bulk Actions ▼]      │ │   │
│  │ └─────────────────────────────────────────────────────────┘ │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ ASSIGNMENTS TABLE                                            │   │
│  │ ┌─────┬────────────┬──────────┬────────┬────────┬─────────┐ │   │
│  │ │ □   │ Target     │ Type     │ Period │ Due    │ Actions │ │   │
│  │ ├─────┼────────────┼──────────┼────────┼────────┼─────────┤ │   │
│  │ │ □   │ All Fact.  │ All      │ Dec 24 │ Dec 15 │ ⋮       │ │   │
│  │ │ □   │ Finance    │ Role     │ Dec 24 │ Dec 15 │ ⋮       │ │   │
│  │ │ □   │ John Doe   │ User     │ Dec 24 │ Dec 20 │ ⋮       │ │   │
│  │ └─────┴────────────┴──────────┴────────┴────────┴─────────┘ │   │
│  │                                                              │   │
│  │ Pagination: [< 1 2 3 ... 10 >]                               │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Components

| Component | Description |
|-----------|-------------|
| **Header** | Title with count badge + Create button |
| **Filter Bar** | Type, Status, Period filters + Bulk actions dropdown |
| **Data Table** | Paginated list with checkboxes for bulk selection |
| **Row Actions** | Edit, Extend, Cancel, Remind per row |

> **Note:** Statistics (Total, Active, Overdue, Completion %) are displayed in the **Overview Tab**, not in this panel.

### Assignment Types (8 Total)

| Category | Type | Target Field | Description |
|----------|------|--------------|-------------|
| **Tenant-based** | All | - | All tenants in system |
| | TenantType | TenantType | All tenants of a type (Factory, Region, Zone) |
| | TenantGroup | TenantGroupId | All tenants in a group |
| | SpecificTenant | TenantId | Single specific tenant |
| **User-based** | Role | RoleId | All users with a role |
| | Department | DepartmentId | All users in a department |
| | UserGroup | UserGroupId | All users in a user group |
| | SpecificUser | UserId | Single specific user |

### Create Assignment Form (Vertical Wizard - Standalone Partial)

**File:** `Views/Forms/FormTemplates/Forms/_CreateAssignmentForm.cshtml`

This is a **standalone partial** that uses the `_VerticalWizard.cshtml` component. It can be:
- Embedded in a full page view
- Loaded into a modal via AJAX
- Used in different contexts

```
┌─────────────────────────────────────────────────────────────────────┐
│ _CreateAssignmentForm.cshtml (uses _VerticalWizard)                 │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌──────────────────────┐  ┌────────────────────────────────────┐  │
│  │ VERTICAL STEPPER     │  │ STEP CONTENT                       │  │
│  │                      │  │                                    │  │
│  │  ● 1. Period         │  │  ┌────────────────────────────────┐│  │
│  │  │   Select period   │  │  │ Step 1: Reporting Period       ││  │
│  │  │                   │  │  │ ─────────────────────────────  ││  │
│  │  ○ 2. Target         │  │  │                                ││  │
│  │  │   Who to assign   │  │  │ Year: [2024 ▼]                 ││  │
│  │  │                   │  │  │ Month: [December ▼]            ││  │
│  │  ○ 3. Schedule       │  │  │                                ││  │
│  │  │   Due & recurrence│  │  │ □ This is a recurring assign.  ││  │
│  │  │                   │  │  │                                ││  │
│  │  ○ 4. Review         │  │  └────────────────────────────────┘│  │
│  │      Confirm         │  │                                    │  │
│  │                      │  │  ─────────────────────────────────  │  │
│  └──────────────────────┘  │  [Back]              [Next Step →] │  │
│                            └────────────────────────────────────┘  │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

#### Wizard Configuration

```csharp
var assignmentWizard = new WizardConfig
{
    Layout = WizardLayout.Vertical,
    FormId = "create-assignment-form",
    FormAction = "/api/assignments",
    Steps = new List<WizardStep>
    {
        new WizardStep
        {
            StepId = "period",
            Title = "Reporting Period",
            Description = "Select the reporting period",
            Icon = "ri-calendar-line",
            ContentPartialPath = "~/Views/Forms/FormTemplates/Forms/Steps/_AssignmentPeriodStep.cshtml",
            State = WizardStepState.Active
        },
        new WizardStep
        {
            StepId = "target",
            Title = "Target Selection",
            Description = "Who to assign",
            Icon = "ri-user-add-line",
            ContentPartialPath = "~/Views/Forms/FormTemplates/Forms/Steps/_AssignmentTargetStep.cshtml"
        },
        new WizardStep
        {
            StepId = "schedule",
            Title = "Schedule",
            Description = "Due date & recurrence",
            Icon = "ri-time-line",
            ContentPartialPath = "~/Views/Forms/FormTemplates/Forms/Steps/_AssignmentScheduleStep.cshtml"
        },
        new WizardStep
        {
            StepId = "review",
            Title = "Review & Confirm",
            Description = "Confirm assignment",
            Icon = "ri-check-double-line",
            ContentPartialPath = "~/Views/Forms/FormTemplates/Forms/Steps/_AssignmentReviewStep.cshtml",
            ShowNext = false,
            CustomButtonHtml = "<button type='submit' class='btn btn-success ms-auto'>Create Assignment</button>"
        }
    }
}.BuildWizard();
```

#### Step Content Partials

| Step | Partial | Content |
|------|---------|---------|
| 1. Period | `_AssignmentPeriodStep.cshtml` | Year/Month dropdowns, Recurring checkbox |
| 2. Target | `_AssignmentTargetStep.cshtml` | 8 type buttons, conditional target picker, preview count |
| 3. Schedule | `_AssignmentScheduleStep.cshtml` | Due date, reminder days, recurrence pattern |
| 4. Review | `_AssignmentReviewStep.cshtml` | Summary display, notes field, submit button |

---

## WorkflowBuilderPanel

### Purpose
Visual workflow step designer for configuring approval/action workflows on form templates.

### Visual Structure

```
┌─────────────────────────────────────────────────────────────────────┐
│ _WorkflowBuilderPanel.cshtml                                        │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ HEADER                                                       │   │
│  │ ┌─────────────────────────────────────────────────────────┐ │   │
│  │ │ Workflow: Standard Approval    [Change] [+ Create New]  │ │   │
│  │ │ 4 steps • Last modified: Dec 10, 2024                   │ │   │
│  │ └─────────────────────────────────────────────────────────┘ │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ WORKFLOW TIMELINE (Visual)                                   │   │
│  │                                                              │   │
│  │    ●═══════════●═══════════●═══════════●                    │   │
│  │    │           │           │           │                    │   │
│  │  ┌─┴─┐       ┌─┴─┐       ┌─┴─┐       ┌─┴─┐                  │   │
│  │  │ 1 │       │ 2 │       │ 3 │       │ 4 │                  │   │
│  │  │Fill│      │Sign│      │Appr│      │Veri│                 │   │
│  │  └───┘       └───┘       └───┘       └───┘                  │   │
│  │  Submitter   Submitter   Manager     Finance                │   │
│  │                                                              │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ STEPS LIST (Detailed)                              [+ Add]  │   │
│  │ ┌─────────────────────────────────────────────────────────┐ │   │
│  │ │ ≡ Step 1: Fill Form                              [Edit] │ │   │
│  │ │   Action: Fill • Assignee: Submitter • Target: All      │ │   │
│  │ │   Due: 3 days • Mandatory: Yes                          │ │   │
│  │ ├─────────────────────────────────────────────────────────┤ │   │
│  │ │ ≡ Step 2: Sign Submission                        [Edit] │ │   │
│  │ │   Action: Sign • Assignee: Submitter • Target: All      │ │   │
│  │ │   Due: 1 day • Requires Signature: Yes                  │ │   │
│  │ ├─────────────────────────────────────────────────────────┤ │   │
│  │ │ ≡ Step 3: Manager Approval                       [Edit] │ │   │
│  │ │   Action: Approve • Assignee: Role (Manager)            │ │   │
│  │ │   Due: 2 days • Can Delegate: Yes                       │ │   │
│  │ ├─────────────────────────────────────────────────────────┤ │   │
│  │ │ ≡ Step 4: Finance Verification                   [Edit] │ │   │
│  │ │   Action: Verify • Assignee: Department (Finance)       │ │   │
│  │ │   Due: 2 days • Auto-approve if Amount < 10000          │ │   │
│  │ └─────────────────────────────────────────────────────────┘ │   │
│  │                                                              │   │
│  │ ≡ = Drag handle for reordering                              │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Empty State (No Workflow)

```
┌─────────────────────────────────────────────────────────────────────┐
│ NO WORKFLOW STATE                                                   │
│ ┌─────────────────────────────────────────────────────────────────┐│
│ │                                                                 ││
│ │     📋 No workflow configured                                   ││
│ │                                                                 ││
│ │     This template has no approval workflow.                     ││
│ │     Submissions will be saved directly without approval.        ││
│ │                                                                 ││
│ │     [Select Existing Workflow]  [Create New Workflow]           ││
│ │                                                                 ││
│ └─────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────┘
```

### Components

| Component | Description |
|-----------|-------------|
| **Header** | Current workflow name + Change/Create buttons |
| **Visual Timeline** | Horizontal step visualization with icons |
| **Steps List** | Detailed cards with drag-to-reorder (≡ handle) |
| **Empty State** | Shown when no workflow is assigned |
| **Add Step Button** | Opens modal to add new step |

### Workflow Actions (from WorkflowAction table)

| Action | Icon | Description | Requires Signature | Requires Comment |
|--------|------|-------------|-------------------|------------------|
| Fill | `bi-pencil-square` | Complete form fields | No | No |
| Sign | `bi-pen` | Digitally sign | Yes | No |
| Approve | `bi-check-circle` | Approve submission | No | Optional |
| Reject | `bi-x-circle` | Reject submission | No | Yes |
| Review | `bi-eye` | Review without action | No | Optional |
| Verify | `bi-shield-check` | Verify data accuracy | No | No |

### Assignee Types

| Type | Description | Target Field |
|------|-------------|--------------|
| Submitter | Person who created submission | - |
| PreviousActor | Person who completed previous step | - |
| Role | Any user with specific role | ApproverRoleId |
| User | Specific user | ApproverUserId |
| Department | Any user in department | AssigneeDepartmentId |
| FieldValue | User ID from a form field | AssigneeFieldId |

### Workflow Step Form (Vertical Wizard - Standalone Partial)

**File:** `Views/Forms/FormTemplates/Forms/_WorkflowStepForm.cshtml`

This is a **standalone partial** that uses the `_VerticalWizard.cshtml` component for Add/Edit step operations.

```
┌─────────────────────────────────────────────────────────────────────┐
│ _WorkflowStepForm.cshtml (uses _VerticalWizard)                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌──────────────────────┐  ┌────────────────────────────────────┐  │
│  │ VERTICAL STEPPER     │  │ STEP CONTENT                       │  │
│  │                      │  │                                    │  │
│  │  ● 1. Action         │  │  ┌────────────────────────────────┐│  │
│  │  │   What to do      │  │  │ Step 1: Select Action          ││  │
│  │  │                   │  │  │ ─────────────────────────────  ││  │
│  │  ○ 2. Assignee       │  │  │                                ││  │
│  │  │   Who does it     │  │  │ Step Name: [______________]    ││  │
│  │  │                   │  │  │                                ││  │
│  │  ○ 3. Target         │  │  │ Action:                        ││  │
│  │  │   What to act on  │  │  │ ○ Fill  ○ Sign  ● Approve      ││  │
│  │  │                   │  │  │ ○ Reject  ○ Review  ○ Verify   ││  │
│  │  ○ 4. Settings       │  │  │                                ││  │
│  │      Rules & timing  │  │  └────────────────────────────────┘│  │
│  │                      │  │                                    │  │
│  └──────────────────────┘  │  ─────────────────────────────────  │  │
│                            │  [Back]              [Next Step →] │  │
│                            └────────────────────────────────────┘  │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

#### Wizard Configuration

```csharp
var stepWizard = new WizardConfig
{
    Layout = WizardLayout.Vertical,
    FormId = "workflow-step-form",
    FormAction = "/api/workflows/{workflowId}/steps",
    Steps = new List<WizardStep>
    {
        new WizardStep
        {
            StepId = "action",
            Title = "Select Action",
            Description = "What to do",
            Icon = "ri-flashlight-line",
            ContentPartialPath = "~/Views/Forms/FormTemplates/Forms/Steps/_StepActionStep.cshtml",
            State = WizardStepState.Active
        },
        new WizardStep
        {
            StepId = "assignee",
            Title = "Assignee",
            Description = "Who does it",
            Icon = "ri-user-star-line",
            ContentPartialPath = "~/Views/Forms/FormTemplates/Forms/Steps/_StepAssigneeStep.cshtml"
        },
        new WizardStep
        {
            StepId = "target",
            Title = "Target",
            Description = "What to act on",
            Icon = "ri-focus-3-line",
            ContentPartialPath = "~/Views/Forms/FormTemplates/Forms/Steps/_StepTargetStep.cshtml"
        },
        new WizardStep
        {
            StepId = "settings",
            Title = "Settings",
            Description = "Rules & timing",
            Icon = "ri-settings-3-line",
            ContentPartialPath = "~/Views/Forms/FormTemplates/Forms/Steps/_StepSettingsStep.cshtml",
            ShowNext = false,
            CustomButtonHtml = "<button type='submit' class='btn btn-success ms-auto'>Save Step</button>"
        }
    }
}.BuildWizard();
```

#### Step Content Partials

| Step | Partial | Content |
|------|---------|---------|
| 1. Action | `_StepActionStep.cshtml` | Step name, Action type radio buttons (Fill/Sign/Approve/etc.) |
| 2. Assignee | `_StepAssigneeStep.cshtml` | Assignee type (Submitter/Role/User/etc.), conditional picker |
| 3. Target | `_StepTargetStep.cshtml` | Target type (Submission/Section/Field), conditional picker |
| 4. Settings | `_StepSettingsStep.cshtml` | Due days, mandatory, delegation, parallel, auto-approve, escalation |

### Create Workflow Form (Vertical Wizard - Standalone Partial)

**File:** `Views/Forms/FormTemplates/Forms/_CreateWorkflowForm.cshtml`

For creating a new workflow definition (simpler, 2-step wizard):

```csharp
var workflowWizard = new WizardConfig
{
    Layout = WizardLayout.Vertical,
    FormId = "create-workflow-form",
    FormAction = "/api/workflows",
    Steps = new List<WizardStep>
    {
        new WizardStep
        {
            StepId = "basic",
            Title = "Basic Info",
            Description = "Name & description",
            Icon = "ri-file-list-3-line",
            ContentPartialPath = "~/Views/Forms/FormTemplates/Forms/Steps/_WorkflowBasicStep.cshtml",
            State = WizardStepState.Active
        },
        new WizardStep
        {
            StepId = "confirm",
            Title = "Confirm",
            Description = "Review & create",
            Icon = "ri-check-double-line",
            ContentPartialPath = "~/Views/Forms/FormTemplates/Forms/Steps/_WorkflowConfirmStep.cshtml",
            ShowNext = false,
            CustomButtonHtml = "<button type='submit' class='btn btn-success ms-auto'>Create Workflow</button>"
        }
    }
}.BuildWizard();
```

---

## Data Flow

### FormAssignmentPanel

```
┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│ Panel Load      │────▶│ GET /api/assignments │────▶│ Render Table    │
│ (AJAX)          │     │ ?templateId=X        │     │                 │
└─────────────────┘     └──────────────────────┘     └─────────────────┘

┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│ Create Button   │────▶│ Show Modal           │────▶│ POST /api/      │
│ Click           │     │ (4-step wizard)      │     │ assignments     │
└─────────────────┘     └──────────────────────┘     └─────────────────┘

┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│ Bulk Select     │────▶│ Bulk Actions Menu    │────▶│ POST /api/      │
│ + Action        │     │ (Remind/Extend/Cancel)│    │ assignments/bulk│
└─────────────────┘     └──────────────────────┘     └─────────────────┘
```

### WorkflowBuilderPanel

```
┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│ Panel Load      │────▶│ GET /api/workflows/  │────▶│ Render Timeline │
│ (AJAX)          │     │ {workflowId}         │     │ + Steps List    │
└─────────────────┘     └──────────────────────┘     └─────────────────┘

┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│ Add Step        │────▶│ Show Modal           │────▶│ POST /api/      │
│ Click           │     │ (Step properties)    │     │ workflows/{id}/ │
└─────────────────┘     └──────────────────────┘     │ steps           │
                                                     └─────────────────┘

┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│ Drag & Drop     │────▶│ Capture new order    │────▶│ POST /api/      │
│ Reorder         │     │                      │     │ workflows/{id}/ │
└─────────────────┘     └──────────────────────┘     │ steps/reorder   │
                                                     └─────────────────┘
```

---

## ViewModels

### FormAssignmentPanelViewModel

```csharp
public class FormAssignmentPanelViewModel
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    
    // Filter options (for dropdowns)
    public List<SelectListItem> AssignmentTypes { get; set; } = new();
    public List<SelectListItem> StatusOptions { get; set; } = new();
    public List<SelectListItem> PeriodOptions { get; set; } = new();
    
    // Data loaded via AJAX, not in initial model
    // Statistics are in OverviewPanel, not here
}
```

### WorkflowBuilderPanelViewModel

```csharp
public class WorkflowBuilderPanelViewModel
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    
    // Current workflow (null if none)
    public int? WorkflowId { get; set; }
    public string? WorkflowName { get; set; }
    public string? WorkflowDescription { get; set; }
    public DateTime? LastModified { get; set; }
    
    // Steps (for initial render)
    public List<WorkflowStepViewModel> Steps { get; set; } = new();
    
    // Available workflows for selection
    public List<SelectListItem> AvailableWorkflows { get; set; } = new();
    
    // Available actions for step creation
    public List<WorkflowActionDto> AvailableActions { get; set; } = new();
}

public class WorkflowStepViewModel
{
    public int StepId { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    
    // Action
    public int ActionId { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string ActionIcon { get; set; } = string.Empty;
    public string ActionCssClass { get; set; } = string.Empty;
    
    // Assignee
    public string AssigneeType { get; set; } = string.Empty;
    public string AssigneeDisplay { get; set; } = string.Empty;
    
    // Target
    public string TargetType { get; set; } = "Submission";
    public string? TargetDisplay { get; set; }
    
    // Settings
    public int? DueDays { get; set; }
    public bool IsMandatory { get; set; }
    public bool AllowDelegate { get; set; }
    public bool IsParallel { get; set; }
    public bool RequiresSignature { get; set; }
    public bool RequiresComment { get; set; }
    
    // Conditions
    public string? AutoApproveCondition { get; set; }
    public string? EscalationRole { get; set; }
}
```

---

## API Endpoints Used

### Assignments API (`/api/assignments`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Get assignments with filtering |
| GET | `/{id}` | Get assignment details |
| POST | `/` | Create assignment |
| PUT | `/{id}` | Update assignment |
| POST | `/{id}/cancel` | Cancel assignment |
| POST | `/{id}/extend` | Extend deadline |
| POST | `/preview-targets` | Preview targets before creating |
| GET | `/target-count` | Get target count |
| GET | `/statistics` | Get dashboard statistics |
| POST | `/bulk/remind` | Send bulk reminders |
| POST | `/bulk/extend` | Bulk extend deadlines |
| POST | `/bulk/cancel` | Bulk cancel |
| GET | `/template/{templateId}` | Get template assignments |

### Workflows API (`/api/workflows`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Get all workflows |
| GET | `/{id}` | Get workflow with steps |
| POST | `/` | Create workflow |
| PUT | `/{id}` | Update workflow |
| DELETE | `/{id}` | Delete workflow |
| POST | `/{workflowId}/steps` | Add step |
| PUT | `/steps/{stepId}` | Update step |
| DELETE | `/steps/{stepId}` | Delete step |
| POST | `/{workflowId}/steps/reorder` | Reorder steps |
| GET | `/actions` | Get workflow actions |
| GET | `/lookup` | Get for dropdown |

---

## Reusable Components (Config → Extension → ViewModel → Partial Pattern)

Following the existing component pattern in this system (like `AssignmentManager`, `Wizard`), these components use:
- **Config** (POCO) - What controllers create
- **Extension** (fluent API) - Transform config to view model
- **ViewModel** - Render-ready data for partial
- **Partial View** - The actual UI

### WorkflowProgressTimeline Component

A **reusable component** that shows the progress of a submission through its workflow steps. Can be used in:
- Submission Details page
- Dashboard widgets
- Any view that needs to show workflow progress

**Files:**
- `Models/ViewModels/Components/WorkflowProgressTimelineComponents.cs` - Config + ViewModel
- `Extensions/WorkflowProgressTimelineExtensions.cs` - Fluent API + transform
- `Views/Shared/Components/WorkflowProgressTimeline/_WorkflowProgressTimeline.cshtml` - Partial view

#### Usage

```csharp
// In Controller - prepare the config
var timelineConfig = new WorkflowProgressTimelineConfig
{
    Steps = progressSteps  // List<WorkflowProgressStepItem>
}
.WithTitle("Workflow Progress")
.AsCompact()  // Optional: compact mode for dashboard
.BuildTimeline();

ViewData["WorkflowTimeline"] = timelineConfig;

// In View - render the partial
<partial name="~/Views/Shared/Components/WorkflowProgressTimeline/_WorkflowProgressTimeline.cshtml" />
```

#### Visual Structure

```
┌─────────────────────────────────────────────────────────────────────┐
│ Workflow Progress                                                   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│    ✓═══════════✓═══════════●═══════════○                           │
│    │           │           │           │                           │
│  ┌─┴─┐       ┌─┴─┐       ┌─┴─┐       ┌─┴─┐                         │
│  │ ✓ │       │ ✓ │       │ ● │       │ ○ │                         │
│  │Fill│      │Sign│      │Appr│      │Veri│                        │
│  └───┘       └───┘       └───┘       └───┘                         │
│  Completed   Completed   In Progress  Pending                      │
│  Dec 10      Dec 11      Waiting...   -                            │
│  John Doe    John Doe    Jane Smith   -                            │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

#### Config Class

```csharp
// Models/ViewModels/Components/WorkflowProgressTimelineComponents.cs

/// <summary>
/// Configuration for WorkflowProgressTimeline component (POCO - what controllers create)
/// </summary>
public class WorkflowProgressTimelineConfig
{
    public string TimelineId { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = "Workflow Progress";
    public List<WorkflowProgressStepItem> Steps { get; set; } = new();
    public bool IsCompact { get; set; } = false;
    public bool ShowActorInfo { get; set; } = true;
    public bool ShowDates { get; set; } = true;
    public string? CssClasses { get; set; }
}

/// <summary>
/// Individual step in the workflow progress
/// </summary>
public class WorkflowProgressStepItem
{
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string ActionIcon { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Rejected, Skipped
    public string? ActorName { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Comments { get; set; }
}
```

#### ViewModel Class

```csharp
/// <summary>
/// ViewModel for WorkflowProgressTimeline (render-ready)
/// </summary>
public class WorkflowProgressTimelineViewModel
{
    public string TimelineId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<WorkflowProgressStepViewModel> Steps { get; set; } = new();
    public bool IsCompact { get; set; }
    public bool ShowActorInfo { get; set; }
    public bool ShowDates { get; set; }
    public string CssClasses { get; set; } = string.Empty;

    // Computed
    public bool HasSteps => Steps.Any();
    public int TotalSteps => Steps.Count;
    public int CompletedSteps => Steps.Count(s => s.IsCompleted);
    public int ProgressPercentage => TotalSteps > 0 ? (CompletedSteps * 100) / TotalSteps : 0;
    public string CurrentStepName => Steps.FirstOrDefault(s => s.IsActive)?.StepName ?? "";
}

/// <summary>
/// Individual step view model (render-ready)
/// </summary>
public class WorkflowProgressStepViewModel
{
    public int StepOrder { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string ActionIcon { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ActorName { get; set; }
    public string? CompletedDateFormatted { get; set; }
    public string? DueDateFormatted { get; set; }
    public string? Comments { get; set; }

    // Computed for rendering
    public bool IsCompleted => Status == "Completed";
    public bool IsActive => Status == "InProgress";
    public bool IsPending => Status == "Pending";
    public bool IsRejected => Status == "Rejected";
    
    public string StateClass => Status switch
    {
        "Completed" => "done",
        "InProgress" => "active",
        "Rejected" => "rejected",
        "Skipped" => "skipped",
        _ => ""
    };
    
    public string StatusBadgeClass => Status switch
    {
        "Completed" => "bg-success",
        "InProgress" => "bg-primary",
        "Rejected" => "bg-danger",
        "Skipped" => "bg-secondary",
        _ => "bg-light text-muted"
    };
    
    public string StatusIcon => Status switch
    {
        "Completed" => "ri-check-line",
        "InProgress" => "ri-loader-4-line",
        "Rejected" => "ri-close-line",
        "Skipped" => "ri-skip-forward-line",
        _ => ""
    };
}
```

#### Extension Class

```csharp
// Extensions/WorkflowProgressTimelineExtensions.cs

public static class WorkflowProgressTimelineExtensions
{
    /// <summary>
    /// Build WorkflowProgressTimelineViewModel from Config
    /// </summary>
    public static WorkflowProgressTimelineViewModel BuildTimeline(this WorkflowProgressTimelineConfig config)
    {
        return new WorkflowProgressTimelineViewModel
        {
            TimelineId = config.TimelineId,
            Title = config.Title,
            Steps = config.Steps.OrderBy(s => s.StepOrder).Select(TransformStep).ToList(),
            IsCompact = config.IsCompact,
            ShowActorInfo = config.ShowActorInfo,
            ShowDates = config.ShowDates,
            CssClasses = config.CssClasses ?? ""
        };
    }

    /// <summary>
    /// Fluent API: Set title
    /// </summary>
    public static WorkflowProgressTimelineConfig WithTitle(this WorkflowProgressTimelineConfig config, string title)
    {
        config.Title = title;
        return config;
    }

    /// <summary>
    /// Fluent API: Enable compact mode
    /// </summary>
    public static WorkflowProgressTimelineConfig AsCompact(this WorkflowProgressTimelineConfig config)
    {
        config.IsCompact = true;
        return config;
    }

    /// <summary>
    /// Fluent API: Hide actor info
    /// </summary>
    public static WorkflowProgressTimelineConfig HideActorInfo(this WorkflowProgressTimelineConfig config)
    {
        config.ShowActorInfo = false;
        return config;
    }

    /// <summary>
    /// Fluent API: Hide dates
    /// </summary>
    public static WorkflowProgressTimelineConfig HideDates(this WorkflowProgressTimelineConfig config)
    {
        config.ShowDates = false;
        return config;
    }

    private static WorkflowProgressStepViewModel TransformStep(WorkflowProgressStepItem item)
    {
        return new WorkflowProgressStepViewModel
        {
            StepOrder = item.StepOrder,
            StepName = item.StepName,
            ActionName = item.ActionName,
            ActionIcon = item.ActionIcon,
            Status = item.Status,
            ActorName = item.ActorName,
            CompletedDateFormatted = item.CompletedDate?.ToString("MMM dd"),
            DueDateFormatted = item.DueDate?.ToString("MMM dd"),
            Comments = item.Comments
        };
    }
}
```

---

### PendingActionsWidget Component

A **reusable component** that shows user's pending workflow actions across all submissions.

**Files:**
- `Models/ViewModels/Components/PendingActionsWidgetComponents.cs` - Config + ViewModel
- `Extensions/PendingActionsWidgetExtensions.cs` - Fluent API + transform
- `Views/Shared/Components/PendingActionsWidget/_PendingActionsWidget.cshtml` - Partial view

#### Usage

```csharp
// In Controller - prepare the config
var pendingConfig = new PendingActionsWidgetConfig
{
    Actions = pendingActions  // List<PendingActionItem>
}
.WithTitle("My Pending Actions")
.WithMaxItems(5)
.WithViewAllUrl("/workflow/pending")
.BuildWidget();

ViewData["PendingActionsWidget"] = pendingConfig;

// In View - render the partial
<partial name="~/Views/Shared/Components/PendingActionsWidget/_PendingActionsWidget.cshtml" />
```

#### Visual Structure

```
┌─────────────────────────────────────────────────────────────────────┐
│ My Pending Actions                                           (5)   │
├─────────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────────────┐│
│ │ 🔵 Approve: Monthly Report - Factory A          Due: Today     ││
│ │    Step 3 of 4 • Submitted by John Doe                         ││
│ ├─────────────────────────────────────────────────────────────────┤│
│ │ 🟡 Review: Quarterly Audit - Region B           Due: Tomorrow  ││
│ │    Step 2 of 3 • Submitted by Jane Smith                       ││
│ ├─────────────────────────────────────────────────────────────────┤│
│ │ 🔴 Sign: Annual Budget - HQ                     Overdue: 2 days││
│ │    Step 1 of 2 • Submitted by Bob Wilson                       ││
│ └─────────────────────────────────────────────────────────────────┘│
│                                                                     │
│ [View All Pending Actions]                                          │
└─────────────────────────────────────────────────────────────────────┘
```

#### Config Class

```csharp
// Models/ViewModels/Components/PendingActionsWidgetComponents.cs

/// <summary>
/// Configuration for PendingActionsWidget component (POCO - what controllers create)
/// </summary>
public class PendingActionsWidgetConfig
{
    public string WidgetId { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = "My Pending Actions";
    public List<PendingActionItem> Actions { get; set; } = new();
    public int MaxItems { get; set; } = 5;
    public string? ViewAllUrl { get; set; }
    public bool ShowSubmitter { get; set; } = true;
    public string? CssClasses { get; set; }
}

/// <summary>
/// Individual pending action item
/// </summary>
public class PendingActionItem
{
    public int ProgressId { get; set; }
    public int SubmissionId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string ActionIcon { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public int TotalSteps { get; set; }
    public string SubmitterName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? ActionUrl { get; set; }
}
```

#### ViewModel Class

```csharp
/// <summary>
/// ViewModel for PendingActionsWidget (render-ready)
/// </summary>
public class PendingActionsWidgetViewModel
{
    public string WidgetId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<PendingActionItemViewModel> Actions { get; set; } = new();
    public int TotalCount { get; set; }
    public bool ShowViewAll { get; set; }
    public string? ViewAllUrl { get; set; }
    public bool ShowSubmitter { get; set; }
    public string CssClasses { get; set; } = string.Empty;

    // Computed
    public bool HasActions => Actions.Any();
    public int OverdueCount => Actions.Count(a => a.IsOverdue);
    public int DueTodayCount => Actions.Count(a => a.IsDueToday);
}

/// <summary>
/// Individual action view model (render-ready)
/// </summary>
public class PendingActionItemViewModel
{
    public int ProgressId { get; set; }
    public int SubmissionId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string ActionIcon { get; set; } = string.Empty;
    public string StepInfo { get; set; } = string.Empty; // "Step 3 of 4"
    public string SubmitterName { get; set; } = string.Empty;
    public string DueDateDisplay { get; set; } = string.Empty; // "Today", "Tomorrow", "Overdue: 2 days"
    public string? ActionUrl { get; set; }

    // Computed for rendering
    public bool IsOverdue { get; set; }
    public bool IsDueToday { get; set; }
    public bool IsDueSoon { get; set; } // Within 3 days
    
    public string UrgencyClass => IsOverdue ? "danger" : IsDueToday ? "warning" : IsDueSoon ? "info" : "secondary";
    public string UrgencyIcon => IsOverdue ? "ri-error-warning-line" : IsDueToday ? "ri-time-line" : "ri-calendar-line";
}
```

#### Extension Class

```csharp
// Extensions/PendingActionsWidgetExtensions.cs

public static class PendingActionsWidgetExtensions
{
    /// <summary>
    /// Build PendingActionsWidgetViewModel from Config
    /// </summary>
    public static PendingActionsWidgetViewModel BuildWidget(this PendingActionsWidgetConfig config)
    {
        var displayActions = config.Actions
            .OrderBy(a => a.DueDate ?? DateTime.MaxValue)
            .Take(config.MaxItems)
            .Select(TransformAction)
            .ToList();

        return new PendingActionsWidgetViewModel
        {
            WidgetId = config.WidgetId,
            Title = config.Title,
            Actions = displayActions,
            TotalCount = config.Actions.Count,
            ShowViewAll = config.Actions.Count > config.MaxItems,
            ViewAllUrl = config.ViewAllUrl,
            ShowSubmitter = config.ShowSubmitter,
            CssClasses = config.CssClasses ?? ""
        };
    }

    /// <summary>
    /// Fluent API: Set title
    /// </summary>
    public static PendingActionsWidgetConfig WithTitle(this PendingActionsWidgetConfig config, string title)
    {
        config.Title = title;
        return config;
    }

    /// <summary>
    /// Fluent API: Set max items
    /// </summary>
    public static PendingActionsWidgetConfig WithMaxItems(this PendingActionsWidgetConfig config, int maxItems)
    {
        config.MaxItems = maxItems;
        return config;
    }

    /// <summary>
    /// Fluent API: Set view all URL
    /// </summary>
    public static PendingActionsWidgetConfig WithViewAllUrl(this PendingActionsWidgetConfig config, string url)
    {
        config.ViewAllUrl = url;
        return config;
    }

    /// <summary>
    /// Fluent API: Hide submitter info
    /// </summary>
    public static PendingActionsWidgetConfig HideSubmitter(this PendingActionsWidgetConfig config)
    {
        config.ShowSubmitter = false;
        return config;
    }

    private static PendingActionItemViewModel TransformAction(PendingActionItem item)
    {
        var now = DateTime.UtcNow.Date;
        var dueDate = item.DueDate?.Date;
        var isOverdue = dueDate.HasValue && dueDate < now;
        var isDueToday = dueDate.HasValue && dueDate == now;
        var isDueSoon = dueDate.HasValue && dueDate <= now.AddDays(3) && !isOverdue && !isDueToday;

        string dueDateDisplay;
        if (!dueDate.HasValue)
            dueDateDisplay = "No due date";
        else if (isOverdue)
            dueDateDisplay = $"Overdue: {(now - dueDate.Value).Days} days";
        else if (isDueToday)
            dueDateDisplay = "Due: Today";
        else if (dueDate == now.AddDays(1))
            dueDateDisplay = "Due: Tomorrow";
        else
            dueDateDisplay = $"Due: {item.DueDate:MMM dd}";

        return new PendingActionItemViewModel
        {
            ProgressId = item.ProgressId,
            SubmissionId = item.SubmissionId,
            TemplateName = item.TemplateName,
            TenantName = item.TenantName,
            ActionName = item.ActionName,
            ActionIcon = item.ActionIcon,
            StepInfo = $"Step {item.StepOrder} of {item.TotalSteps}",
            SubmitterName = item.SubmitterName,
            DueDateDisplay = dueDateDisplay,
            ActionUrl = item.ActionUrl,
            IsOverdue = isOverdue,
            IsDueToday = isDueToday,
            IsDueSoon = isDueSoon
        };
    }
}
```

---

## Implementation Order

1. **Template Details page structure** - Set up tabs (Overview, Structure, Assignments, Workflow, Metrics, Submissions)
2. **FormAssignmentPanel** - List + filters + bulk actions
3. **_CreateAssignmentForm** - Vertical wizard partial (4 steps)
4. **WorkflowBuilderPanel** - Timeline + step list + drag-reorder
5. **_WorkflowStepForm** - Vertical wizard partial (4 steps)
6. **WorkflowProgressTimeline Component** - Config + Extension + ViewModel + Partial
7. **PendingActionsWidget Component** - Config + Extension + ViewModel + Partial

---

## Updated File Structure Summary

```
Models/
└── ViewModels/
    └── Components/
        ├── WorkflowProgressTimelineComponents.cs       # Config + ViewModel + Item classes
        └── PendingActionsWidgetComponents.cs           # Config + ViewModel + Item classes

Extensions/
├── WorkflowProgressTimelineExtensions.cs               # Fluent API + BuildTimeline()
└── PendingActionsWidgetExtensions.cs                   # Fluent API + BuildWidget()

Views/
├── Forms/
│   └── FormTemplates/
│       ├── Details.cshtml                              # Main page with tabs
│       │
│       ├── Panels/                                     # Tab content (embedded in Details)
│       │   ├── _OverviewPanel.cshtml                   # Stats + summary (ALL stats here)
│       │   ├── _StructurePanel.cshtml                  # Sections/fields preview
│       │   ├── _FormAssignmentPanel.cshtml             # Assignment list + filters + bulk
│       │   ├── _WorkflowBuilderPanel.cshtml            # Timeline + step list + drag
│       │   ├── _MetricMappingPanel.cshtml              # KPI mappings
│       │   └── _SubmissionsPanel.cshtml                # Submission list
│       │
│       └── Forms/                                      # Standalone partials (view OR modal)
│           ├── _CreateAssignmentForm.cshtml            # Uses _VerticalWizard (4 steps)
│           ├── _EditAssignmentForm.cshtml              # Simple edit form
│           ├── _CreateWorkflowForm.cshtml              # Uses _VerticalWizard (2 steps)
│           ├── _WorkflowStepForm.cshtml                # Uses _VerticalWizard (4 steps)
│           │
│           └── Steps/                                  # Wizard step content partials
│               ├── _AssignmentPeriodStep.cshtml        # Year/Month, recurring
│               ├── _AssignmentTargetStep.cshtml        # 8 types, target picker, preview
│               ├── _AssignmentScheduleStep.cshtml      # Due date, reminder, recurrence
│               ├── _AssignmentReviewStep.cshtml        # Summary, notes, submit
│               ├── _StepActionStep.cshtml              # Step name, action type
│               ├── _StepAssigneeStep.cshtml            # Assignee type, picker
│               ├── _StepTargetStep.cshtml              # Target type, picker
│               ├── _StepSettingsStep.cshtml            # Due, mandatory, delegation, etc.
│               ├── _WorkflowBasicStep.cshtml           # Workflow name, description
│               └── _WorkflowConfirmStep.cshtml         # Review, submit
│
└── Shared/
    └── Components/
        ├── WorkflowProgressTimeline/
        │   └── _WorkflowProgressTimeline.cshtml        # Timeline partial view
        └── PendingActionsWidget/
            └── _PendingActionsWidget.cshtml            # Widget partial view
```

---

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Standalone form partials** | Can be used in views OR modals, maximum reusability |
| **No stats in panels** | All statistics in Overview tab, panels focus on CRUD |
| **Vertical wizards** | Use existing `_VerticalWizard.cshtml` component |
| **Config → Extension → ViewModel → Partial** | Consistent with existing components (AssignmentManager, Wizard), fluent API, no DI in views |
| **Step content as separate partials** | Each wizard step is its own partial for maintainability |
| **Controller prepares data** | Controllers build config, extensions transform to ViewModel, partials render |

---

## Related Documents

- `Assignment_Workflows_Plan.md` - Overall implementation plan
- `3_FormAssignment_Components.md` - Original assignment component docs
- `ASSIGNMENT_MANAGER_IMPLEMENTATION_GUIDE.md` - Existing assignment manager guide
- `WIZARD_IMPLEMENTATION_GUIDE.md` - Vertical/Horizontal wizard component docs

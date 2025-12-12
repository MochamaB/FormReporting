# Template Assignment & Workflow System - Implementation Plan

## Overview

Two systems work together:
- **FormTemplateAssignment** → WHO can access/create submissions (Access Control)
- **WorkflowDefinition + WorkflowStep** → WHO does WHAT action and WHEN (Process Control)

---

## Current State

### Implemented Models
| Model | Status |
|-------|--------|
| `FormTemplateAssignment` | ✅ Complete (8 assignment types) |
| `WorkflowDefinition` | ✅ Basic |
| `WorkflowStep` | ⚠️ Partial (missing enhanced fields) |
| `SubmissionWorkflowProgress` | ⚠️ Partial (missing signature/target fields) |

### Missing Models
| Model | Purpose |
|-------|---------|
| `WorkflowAction` | Define action types (Fill, Sign, Approve, Reject, Review, Verify) |

---

## Phase 1: Model Layer

### 1.1 Create WorkflowAction Model
- ActionId, ActionCode, ActionName
- RequiresSignature, RequiresComment, AllowDelegate

### 1.2 Enhance WorkflowStep
Add fields:
- TargetType ("Submission", "Section", "Field")
- TargetId (SectionId or ItemId, NULL = whole submission)
- ActionId (FK to WorkflowAction)
- AssigneeType ("Role", "User", "Submitter", "PreviousActor", "FieldValue", "Department")
- AssigneeDepartmentId, AssigneeFieldId
- DependsOnStepIds (JSON array)

### 1.3 Enhance SubmissionWorkflowProgress
Add fields:
- TargetType, TargetId (denormalized)
- ActionId (FK)
- SignatureType, SignatureData, SignatureIP, SignatureTimestamp
- AssignedDate, DelegationReason

### 1.4 Enhance FormTemplateAssignment
Add fields:
- ReportingYear, ReportingMonth, DueDate
- Status (Pending, InProgress, Completed, Overdue)
- IsRecurring, RecurrencePattern, RecurrenceEndDate
- CancelledBy, CancelledDate, CancellationReason

### 1.5 Create Migration & Seed Data
- Generate EF migration
- Seed WorkflowAction with: Fill, Sign, Approve, Reject, Review, Verify

---

## Phase 2: Service Layer

### 2.1 FormAssignmentService
- GetAssignmentsAsync (with filters)
- CreateAssignmentAsync (expand groups to records)
- CancelAssignmentAsync, ExtendDeadlineAsync
- GetTargetCountAsync (preview recipients)
- GetAssignmentStatisticsAsync, GetComplianceMetricsAsync
- CheckUserAccessAsync

### 2.2 WorkflowService
- CRUD for workflows and steps
- ValidateWorkflowAsync (check dependencies)
- CloneWorkflowAsync

### 2.3 WorkflowEngineService
- InitializeSubmissionWorkflowAsync
- GetPendingStepsForUserAsync
- CompleteStepAsync, RejectStepAsync, DelegateStepAsync
- CanUserActOnSectionAsync, CanUserActOnFieldAsync

---

## Phase 3: API Controllers

### 3.1 AssignmentsApiController `/api/assignments`
- GET / - List with filters
- GET /statistics - Dashboard stats
- POST / - Create assignment(s)
- POST /preview-targets - Preview recipients
- PUT /{id}/extend - Extend deadline
- DELETE /{id} - Cancel
- POST /bulk/remind, /bulk/extend, /bulk/cancel

### 3.2 WorkflowApiController `/api/workflows`
- CRUD for workflows
- Step management endpoints
- GET /actions - Available actions

### 3.3 WorkflowEngineApiController `/api/workflow-engine`
- GET /submissions/{id}/progress
- POST /submissions/{id}/steps/{stepId}/complete
- GET /my-pending-actions

---

## Phase 4: UI Controllers

### 4.1 AssignmentsController `/Assignments`
- Index (dashboard)
- Create (wizard)
- Details/{id}
- ComplianceReport

### 4.2 WorkflowsController `/Workflows`
- Index, Create, Edit/{id}, Details/{id}

---

## Phase 5: Assignment Views

### 5.1 Assignment Dashboard
- 4 StatCards (Total, Active, Overdue, Completion Rate)
- Filters + DataTable
- Compliance pie chart
- Bulk actions toolbar

### 5.2 Assignment Creation Wizard (4 Steps)
1. Select Template & Period
2. Target Selection (8 types)
3. Due Date & Recurrence
4. Review & Confirm

### 5.3 Assignment Details & Compliance Report

---

## Phase 6: Workflow Views

### 6.1 Workflow List
- DataTable with workflows

### 6.2 Workflow Builder
- Visual step builder
- Step properties panel
- Dependency visualization

---

## Phase 7: JavaScript

### 7.1 assignment-wizard.js
- Wizard navigation, target selection, recurrence config

### 7.2 workflow-builder.js
- Step CRUD, drag-drop, dependency management

### 7.3 assignment-dashboard.js
- Bulk actions, filters, charts

---

## Phase 8: Form Submission Integration

- Check workflow progress in form renderer
- Show/hide sections based on user permissions
- Initialize workflow on submission creation
- Signature capture component (Checkbox, Digital, PIN)

---

## Phase 9: Notifications

- Assignment created/reminder/overdue
- Workflow step active/completed/rejected/escalated

---

## Phase 10: Menu & Navigation

- Add "Form Assignments" menu item
- Add "Assign" button to published templates
- Add "My Pending Actions" dashboard widget

---

## Implementation Order

| Order | Phase | Days |
|-------|-------|------|
| 1 | Model Layer | 1-2 |
| 2 | Service Layer | 3-4 |
| 3 | API Controllers | 2 |
| 4 | UI Controllers | 1 |
| 5 | Assignment Views | 3-4 |
| 6 | Assignment JS | 2 |
| 7 | Menu & Navigation | 0.5 |
| 8 | Workflow Views | 2-3 |
| 9 | Workflow Builder JS | 2 |
| 10 | Form Integration | 2-3 |
| 11 | Notifications | 1-2 |
| 12 | Testing | 2-3 |

**Total: ~20-28 days**

---

## Key Concepts

### Assignment Types (8)
| Type | Target |
|------|--------|
| All | Everyone |
| TenantType | All tenants of type |
| TenantGroup | Custom tenant group |
| SpecificTenant | Single tenant |
| Role | All users with role |
| Department | All users in department |
| UserGroup | Custom user group |
| SpecificUser | Single user |

### Workflow TargetTypes
| Type | Description |
|------|-------------|
| Submission | Whole form |
| Section | Specific section |
| Field | Specific field |

### Workflow AssigneeTypes
| Type | Resolution |
|------|------------|
| Role | Any user with role |
| User | Fixed user |
| Department | Any user in department |
| Submitter | Form creator |
| PreviousActor | Previous step actor |
| FieldValue | User ID from form field |

### Workflow Actions (Seed Data)
| Code | RequiresSignature | RequiresComment |
|------|-------------------|-----------------|
| Fill | No | No |
| Sign | Yes | No |
| Approve | No | No |
| Reject | No | Yes |
| Review | No | No |
| Verify | No | No |

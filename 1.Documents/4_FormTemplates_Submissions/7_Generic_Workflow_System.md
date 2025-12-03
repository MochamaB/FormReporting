# Generic Workflow System

## Overview

A flexible workflow engine that handles actions at any level: **Template**, **Section**, or **Field**.

Two systems work together:
- **FormTemplateAssignment** → WHO can access/create submissions
- **WorkflowStep** → WHO does WHAT action and WHEN

---

## System Comparison

| System | Purpose | Wizard Step |
|--------|---------|-------------|
| `FormTemplateAssignment` | Access control - who can create submissions | Step 3: Form Assignments |
| `WorkflowDefinition` + `WorkflowStep` | Process control - action sequence | Step 4: Workflow Configuration |

---

## Core Models

### WorkflowAction (Seed Data)

Defines available actions in the system.

| ActionCode | ActionName | RequiresSignature | RequiresComment |
|------------|------------|-------------------|-----------------|
| `Fill` | Fill Data | No | No |
| `Sign` | Sign/Acknowledge | Yes | No |
| `Approve` | Approve | No | No |
| `Reject` | Reject | No | Yes |
| `Review` | Review | No | No |
| `Verify` | Verify | No | No |

### WorkflowStep (Enhanced)

Generic step that can target any entity type.

**Key Fields:**
- `TargetType`: "Submission", "Section", "Field"
- `TargetId`: Specific entity ID (NULL = all of type)
- `ActionId`: FK to WorkflowAction
- `AssigneeType`: "Role", "User", "Submitter", "PreviousActor", "FieldValue"
- `DependsOnStepIds`: JSON array of step IDs that must complete first

### SubmissionWorkflowProgress (Enhanced)

Tracks completion of each step for a submission.

**Key Fields:**
- `TargetType`, `TargetId`: Copied from WorkflowStep
- `ActionId`: The action being tracked
- `Status`: "Pending", "InProgress", "Completed", "Rejected", "Skipped"
- `SignatureData`, `SignatureIP`, `SignatureTimestamp`: For signature actions
- `DelegatedToUserId`, `DelegatedByUserId`: For delegation

---

## TargetType Values

| TargetType | Description | Example |
|------------|-------------|---------|
| `Submission` | Whole form submission | Final approval |
| `Section` | Specific section | Fill/Sign a section |
| `Field` | Specific field | Verify salary field |

---

## AssigneeType Values

| AssigneeType | Description | Resolution |
|--------------|-------------|------------|
| `Role` | Assign to role | Any user with that role |
| `User` | Specific user | Fixed user ID |
| `Department` | Department members | Any user in department |
| `Submitter` | Form creator | User who created submission |
| `PreviousActor` | Previous step actor | User who completed previous step |
| `FieldValue` | Dynamic from field | User ID from a form field |

---

## Example: User Onboarding Form

### Assignment (Who can create)

```
FormTemplateAssignment:
├── AssignmentType: "Role"
├── RoleId: 5 (HR Admin)
```

### Workflow (Process after creation)

| Step | TargetType | TargetId | Action | AssigneeType | DependsOn |
|------|------------|----------|--------|--------------|-----------|
| 1 | Section | 10 (Personal Info) | Fill | Submitter | - |
| 2 | Section | 10 | Sign | PreviousActor | [1] |
| 3 | Section | 20 (IT Setup) | Fill | Role: IT Admin | [2] |
| 4 | Section | 20 | Sign | PreviousActor | [3] |
| 5 | Section | 30 (Acknowledgment) | Fill | FieldValue: EmployeeField | [2] |
| 6 | Section | 30 | Sign | PreviousActor | [5] |
| 7 | Submission | NULL | Approve | Role: HR Manager | [4, 6] |

---

## Workflow Engine Flow

```
1. User creates submission
   ↓
2. Engine initializes SubmissionWorkflowProgress for all steps
   - Steps with no dependencies → Status = "InProgress"
   - Steps with dependencies → Status = "Pending"
   ↓
3. User completes a step (Fill/Sign/Approve)
   ↓
4. Engine updates progress record
   - Status = "Completed"
   - Signature data captured if required
   ↓
5. Engine activates dependent steps
   - Check DependsOnStepIds
   - If all dependencies complete → Status = "InProgress"
   - Send notifications to assignees
   ↓
6. Repeat until all steps complete
   ↓
7. Submission status = "Completed"
```

---

## Signature Types

| Type | Description | Storage |
|------|-------------|---------|
| `Checkbox` | "I confirm this is accurate" | Confirmation text |
| `Digital` | Drawn signature | Base64 image |
| `PIN` | Password/PIN verification | Hash |

All signatures capture: `SignatureIP`, `SignatureTimestamp`

---

## Parallel vs Sequential Steps

**Sequential:** Steps with `DependsOnStepIds` wait for dependencies.

**Parallel:** Steps with same/no dependencies can run simultaneously.

```
Example: Sections 2 and 3 both depend only on Section 1
├── Section 1 completes
├── Section 2 and Section 3 both become "InProgress" (parallel)
├── Section 4 depends on [2, 3] → waits for both
```

---

## Conditional Steps

Use `ConditionLogic` JSON for conditional execution:

```json
{
  "field": "amount",
  "operator": ">",
  "value": 10000
}
```

If condition is false, step is auto-skipped.

---

## Model Changes Summary

### New Model: `WorkflowAction`

```csharp
public class WorkflowAction
{
    public int ActionId { get; set; }
    public string ActionCode { get; set; }      // "Fill", "Sign", "Approve"
    public string ActionName { get; set; }
    public bool RequiresSignature { get; set; }
    public bool RequiresComment { get; set; }
    public bool AllowDelegate { get; set; }
}
```

### Modified: `WorkflowStep`

Add fields:
- `TargetType` (string)
- `TargetId` (int?)
- `ActionId` (int, FK)
- `AssigneeType` (string)
- `AssigneeDepartmentId` (int?)
- `AssigneeFieldId` (int?)
- `DependsOnStepIds` (string, JSON)

### Modified: `SubmissionWorkflowProgress`

Add fields:
- `TargetType`, `TargetId` (denormalized)
- `ActionId` (int, FK)
- `SignatureType`, `SignatureData`, `SignatureIP`, `SignatureTimestamp`
- `AssignedDate`
- `DelegationReason`

---

## Integration Points

| Component | Integration |
|-----------|-------------|
| **Form Renderer** | Check which sections user can fill based on workflow progress |
| **Notifications** | Send alerts when step becomes "InProgress" |
| **Dashboard** | Show pending actions for current user |
| **Audit Log** | Record all step completions with signatures |

---

## Simple Form (No Workflow)

Forms without `WorkflowId` use simple flow:
1. User fills entire form
2. Submits
3. Single approval (if `RequiresApproval = true`)

The generic workflow is optional for complex multi-user forms.

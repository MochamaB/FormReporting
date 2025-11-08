# Section 4: Form Templates & Submissions - Workflows & Actions

**Database Tables:** 18 tables  
**Purpose:** Dynamic form builder system for creating, assigning, submitting, and approving operational checklists with automatic KPI tracking.

---

## System Overview

### What This Module Does

The Form Templates & Submissions module is KTDA's **digital checklist system** with 5 major subsystems:

1. **Template Builder** (Admin) - Design reusable form templates
2. **Assignment Engine** - Distribute forms to factories/users  
3. **Submission Interface** (User) - Complete assigned forms
4. **Approval Workflow** - Multi-level review process
5. **Metric Integration** - Automatic KPI population from responses

### Real-World Example

**Without this system:**
- ICT Manager emails Excel template to 67 factories
- Each factory fills manually, uploads to shared drive
- Manager downloads 67 files, consolidates data manually
- Takes 3-5 days of manual work

**With this system:**
- Template designed once (30 minutes)
- Auto-assign to all factories monthly (2 minutes)
- Factories submit online (15 mins each)
- Regional managers review automatically (5 mins each)
- System populates 20+ KPIs automatically (instant)

**Result:** From 3-5 days → 2 hours

---

## User Roles & Permissions

### 1. Template Designer (HeadOffice ICT Manager)
**Permissions:**
- `Templates.Design` - Create/edit form templates
- `Templates.Publish` - Publish templates for use
- `Templates.Archive` - Archive outdated templates

**Workflows:**
- Create form templates
- Configure field validations
- Set up conditional logic
- Map fields to KPI metrics
- Define approval workflows

### 2. Assignment Manager (HeadOffice / Regional Manager)
**Permissions:**
- `Forms.Assign` - Assign forms to tenants/users
- `Forms.Schedule` - Schedule recurring assignments

**Workflows:**
- Assign forms to factories/users
- Set submission deadlines
- Monitor submission compliance

### 3. Form Submitter (Factory ICT Officer / Clerk)
**Permissions:**
- `Forms.Submit` - Complete assigned forms
- `Forms.SaveDraft` - Save work in progress
- `Forms.ViewMyForms` - See assigned forms

**Workflows:**
- View assigned forms
- Fill out forms with validations
- Submit for approval

### 4. Form Approver (Regional Manager / HeadOffice Supervisor)
**Permissions:**
- `Forms.Approve` - Approve submissions
- `Forms.Reject` - Reject submissions
- `Forms.ViewPending` - See pending approvals

**Workflows:**
- Review submitted forms
- Approve/reject with comments
- Request revisions

---

## Core Workflows

### Workflow 1: Template Creation & Publishing

**Tables:** `FormTemplates`, `FormTemplateSections`, `FormTemplateItems`, `FormItemValidations`, `FormItemMetricMappings`

**Steps:**
1. **Create Template** - Basic info (name, code, category, frequency)
2. **Add Sections** - Organize questions into logical groups
3. **Add Fields** - Add questions/fields to each section
4. **Configure Validations** - Required, min/max, regex patterns
5. **Set Conditional Logic** - Show/hide fields based on answers
6. **Map to KPI Metrics** - Link fields to automatic metric population
7. **Assign Approval Workflow** - Multi-level review process
8. **Publish** - Make available for assignment

**Publish Rules:**
- Must have at least 1 section with 1 field
- If `RequiresApproval = true`, must have `WorkflowId`
- Status changes: `Draft` → `Published`
- Published templates are read-only (must version to edit)

---

### Workflow 2: Form Assignment

**Tables:** `FormTemplateAssignments`, `Notifications`

**Assignment Types:**
1. **Tenant-Based** - Assign to specific factories
2. **User-Based** - Assign to specific users
3. **Role-Based** - Assign to all users with a role
4. **Department-Based** - Assign to department users
5. **Region-Based** - Assign to all factories in region
6. **Group-Based** - Assign to tenant groups
7. **All Tenants** - Assign to all factories

**Recurrence Options:**
- One-time
- Monthly (e.g., 1st of every month)
- Quarterly
- Annual

**Actions:**
1. Select published template
2. Choose assignment type
3. Select target tenants/users/roles
4. Set deadline (days from assignment)
5. Configure recurrence (optional)
6. System creates `FormTemplateAssignments` records
7. System sends notifications to assigned users

---

### Workflow 3: Form Submission

**Tables:** `FormTemplateSubmissions`, `FormTemplateResponses`

**User Journey:**
1. **View Assigned Forms** - Dashboard shows pending forms
2. **Start Form** - System creates submission record (`Status = Draft`)
3. **Fill Section 1** - Answer questions with real-time validation
4. **Auto-save** - Responses saved every 30 seconds
5. **Conditional Logic** - Fields show/hide based on answers
6. **Continue to Next Section** - Multi-page form
7. **Review Answers** - Summary page before submit
8. **Submit** - Change status to `Submitted`
9. **Trigger Approval** - Create workflow progress records

**Key Features:**
- Auto-save drafts
- Real-time validation
- Conditional field visibility
- File uploads (photos/documents)
- Progress indicator
- Deadline warnings

---

### Workflow 4: Multi-Level Approval

**Tables:** `WorkflowDefinitions`, `WorkflowSteps`, `SubmissionWorkflowProgress`

**Approval Flow:**
1. **Form Submitted** - System loads `WorkflowDefinition` for template
2. **Create Progress Records** - One record per workflow step
3. **Notify Step 1 Approvers** - Email/SMS/Push notification
4. **Step 1 Review:**
   - **Approve** → Move to Step 2
   - **Reject** → Return to submitter with comments
   - **Request Changes** → Submitter must revise
   - **Delegate** → Reassign to another user
5. **Step 2 Review** - Repeat for each step
6. **Final Approval** - Status = `Approved`, populate metrics
7. **Rejection** - Status = `Rejected`, notify submitter

**Workflow Step Types:**
- **Sequential** - Must approve in order
- **Parallel** - Multiple approvers at same level
- **Conditional** - Required based on form answers
- **Auto-approve** - System approves if conditions met

---

### Workflow 5: Automatic KPI Metric Population

**Tables:** `FormItemMetricMappings`, `TenantMetrics`, `MetricPopulationLog`

**Trigger:** Form status changes to `Approved`

**Process:**
1. System finds all `FormItemMetricMappings` for template
2. For each mapping:
   - **Direct Mapping**: Copy field value → metric value
   - **Calculated Mapping**: Execute formula → metric value
   - **Binary Compliance**: Check expected value → compliance score
3. Insert into `TenantMetrics` table
4. Create `MetricPopulationLog` entry (audit trail)
5. Update dashboard data

**Mapping Examples:**

**Direct:**
```
Field: "Number of Desktops" (value: 25)
→ Metric: "Total Desktop Count" = 25
```

**Calculated:**
```
Formula: (operational / total) * 100
Fields: operational=23, total=25
→ Metric: "Device Uptime %" = 92%
```

**Binary Compliance:**
```
Field: "Is LAN working?" (value: "Yes")
Expected: "Yes"
→ Metric: "LAN Compliance" = 100%
```

---

## Detailed Actions by User Role

### Template Designer Actions

#### 1. Create New Template
**Trigger:** Click "Create Template"  
**Input:**
- Template Name
- Template Code (unique)
- Category (from `FormCategories`)
- Template Type (Monthly, Quarterly, etc.)
- Description
- Requires Approval (Yes/No)
- Workflow (if approval required)

**Output:** New `FormTemplates` record with `PublishStatus = 'Draft'`

---

#### 2. Add Section
**Trigger:** Click "Add Section" in template builder  
**Input:**
- Section Name
- Description
- Display Order (auto)
- Is Collapsible (Yes/No)
- Icon Class

**Output:** New `FormTemplateSections` record

---

#### 3. Add Field
**Trigger:** Click "Add Field" in section  
**Input:**
- Field Name
- Field Code (unique within template)
- Data Type (Text, Number, Boolean, Date, Dropdown, TextArea, FileUpload)
- Is Required
- Default Value
- Placeholder Text
- Help Text

**Output:** New `FormTemplateItems` record

**For Dropdown:** Also create `FormItemOptions` records

---

#### 4. Add Validation Rule
**Trigger:** Click "Add Validation" on field  
**Input:**
- Validation Type (Required, Range, MinLength, MaxLength, Pattern, Custom)
- Parameters (min/max values, regex pattern)
- Error Message
- Severity (Error, Warning)

**Output:** New `FormItemValidations` record

---

#### 5. Add Conditional Logic
**Trigger:** Click "Add Condition" on field  
**Input:**
- IF (source field)
- Operator (equals, not equals, greater than, etc.)
- Value
- THEN (show/hide this field)

**Output:** JSON stored in `FormTemplateItems.ConditionalLogic`

Example:
```json
{
  "action": "show",
  "rules": [
    {"itemId": 45, "operator": "equals", "value": "Yes"}
  ]
}
```

---

#### 6. Map Field to Metric
**Trigger:** Click "Map to Metric" on field  
**Input:**
- Metric (from `MetricDefinitions`)
- Mapping Type (Direct, Calculated, BinaryCompliance)
- Transformation Logic (formula for calculated)
- Expected Value (for binary compliance)

**Output:** New `FormItemMetricMappings` record

---

#### 7. Publish Template
**Trigger:** Click "Publish Template"  
**Validation:**
- At least 1 section with 1 field
- If `RequiresApproval`, must have `WorkflowId`

**Output:** 
- Update `FormTemplates.PublishStatus = 'Published'`
- Set `PublishedDate` and `PublishedBy`

---

### Assignment Manager Actions

#### 8. Assign Form to Factories
**Trigger:** Click "Assign Form"  
**Input:**
- Template (published only)
- Assignment Type (Tenant, User, Role, etc.)
- Target Selection (which factories/users/roles)
- Due Date (days from assignment)
- Recurrence (One-time, Monthly, Quarterly)

**Output:**
- Create `FormTemplateAssignments` records (one per target)
- Create `Notifications` for assigned users
- Send email/SMS/push notifications

---

#### 9. View Assignment Compliance
**Trigger:** Navigate to "Assignment Reports"  
**Query:** Show submission status for all assignments
- Total Assigned
- Submitted
- Pending
- Overdue
- Completion Rate %

---

### Form Submitter Actions

#### 10. View Assigned Forms
**Trigger:** Navigate to "My Assigned Forms"  
**Display:**
- **Pending** - Not started
- **In Progress** - Draft saved
- **Submitted** - Awaiting approval
- **Approved** - Completed
- **Overdue** - Past deadline (red alert)

**Sort:** Earliest deadline first

---

#### 11. Start New Submission
**Trigger:** Click "Start Form"  
**Process:**
1. Check if draft exists for this period
2. If no, create new `FormTemplateSubmissions` record (`Status = 'Draft'`)
3. Load template structure (sections + fields)
4. Render first section

---

#### 12. Save Field Answer
**Trigger:** User types in field (auto-save) OR clicks "Save Draft"  
**Process:**
1. Run client-side validation
2. If valid, upsert `FormTemplateResponses` record
3. Update `FormTemplateSubmissions.ModifiedDate`

**Storage (EAV Pattern):**
- Text fields → `TextValue`
- Numbers → `NumericValue`
- Dates → `DateValue`
- Yes/No → `BooleanValue`

---

#### 13. Submit Form
**Trigger:** Click "Submit Form"  
**Validation:**
- All required fields must have answers
- All validations must pass
- All sections must be complete

**Process:**
1. Update `FormTemplateSubmissions.Status = 'Submitted'`
2. Set `SubmittedDate`
3. Create `SubmissionWorkflowProgress` records for each workflow step
4. Notify first-step approvers
5. Send confirmation to submitter

---

### Form Approver Actions

#### 14. View Pending Approvals
**Trigger:** Navigate to "Pending Approvals"  
**Query:** Show submissions where:
- Current workflow step assigned to current user's role OR user
- Step status = `Pending`
- Submission status = `Submitted` or `InApproval`

**Display:**
- Factory Name
- Form Name
- Submitted By
- Submitted Date
- Days Pending
- Priority (based on deadline)

---

#### 15. Review Submission
**Trigger:** Click on submission from pending list  
**Display:**
- All form responses (read-only)
- Submission metadata
- Previous approval comments (if multi-step)
- Approve/Reject/Request Changes buttons

---

#### 16. Approve Submission
**Trigger:** Click "Approve"  
**Process:**
1. Update `SubmissionWorkflowProgress.Status = 'Approved'` for current step
2. Set `ReviewedBy` and `ReviewedDate`
3. Check if more steps exist:
   - **Yes** → Update submission `Status = 'InApproval'`, notify next approver
   - **No** → Update submission `Status = 'Approved'`, trigger metric population
4. Notify submitter of approval

---

#### 17. Reject Submission
**Trigger:** Click "Reject" with comments  
**Process:**
1. Update `SubmissionWorkflowProgress.Status = 'Rejected'` for current step
2. Update `FormTemplateSubmissions.Status = 'Rejected'`
3. Set rejection comments
4. Notify submitter with rejection reason
5. Submitter can revise and resubmit

---

#### 18. Delegate Approval
**Trigger:** Click "Delegate" and select user  
**Process:**
1. Update `SubmissionWorkflowProgress.DelegatedTo` = selected user
2. Update `DelegatedBy` = current user
3. Notify delegated user
4. Remove notification from current user

---

## Business Rules

### Template Publishing Rules
1. Cannot publish template with 0 fields
2. Cannot publish template without workflow if `RequiresApproval = true`
3. Once published, template is read-only
4. To edit published template, must create new version

### Assignment Rules
1. Cannot assign unpublished templates
2. Cannot assign to inactive tenants/users
3. Due date must be in the future
4. Recurring assignments create new assignments automatically

### Submission Rules
1. One submission per template per tenant per period
2. User submissions apply to user-based assignments only
3. Tenant submissions require user to belong to that tenant
4. Draft submissions have no deadline enforcement
5. Submitted forms cannot be edited (must reject to revise)

### Approval Rules
1. Approvers cannot approve their own submissions
2. Sequential workflows must complete in order
3. Parallel workflows require all approvers to agree
4. Rejected submissions return to submitter
5. Approved submissions trigger metric population

### Metric Population Rules
1. Only triggered when submission status = `Approved`
2. Direct mappings copy value as-is
3. Calculated mappings execute formula
4. Binary compliance checks expected value match
5. Failed metric population logged but doesn't block approval
6. Metrics written to `TenantMetrics` with `SourceType = 'UserInput'`

---

## Integration Points

### 1. Metrics Module (Section 3)
- `FormItemMetricMappings` → `MetricDefinitions`
- Approved submissions populate `TenantMetrics`
- `MetricPopulationLog` tracks auto-population

### 2. Notification System (Section 9)
- Form assignments create notifications
- Approval requests create notifications
- Submission status changes create notifications
- Deadline reminders (Hangfire job)

### 3. Identity & Access (Section 2)
- Role-based permissions control access
- User roles determine form visibility
- Department-based assignments
- Approval workflow uses roles

### 4. Organizational Structure (Section 1)
- Forms assigned to Tenants
- Submissions linked to Tenants
- Regional managers approve regional submissions
- Tenant groups for bulk assignments

### 5. Media Management (Section 11)
- File upload fields → `MediaFiles`
- Attachments linked via `EntityMediaFiles`
- Entity Type = `FormResponse`

---

## Next Steps

After reviewing this workflows document, proceed to:

1. **Data Flow Diagram** (`1_Section4_DataFlow.md`)
   - Visual data flow between tables
   - State machines for submission lifecycle
   - Approval workflow state diagrams

2. **Implementation Roadmap** (`2_Section4_Implementation_Roadmap.md`)
   - Phase 1: Template Builder UI
   - Phase 2: Assignment Engine
   - Phase 3: Submission Interface
   - Phase 4: Approval Workflow
   - Phase 5: Metric Integration

3. **UI Mockups** (`7_UI_Mockups_Wireframes.md`)
   - Template builder screenshots
   - Form submission interface
   - Approval dashboard
   - Assignment management

---

**Status:** ✅ Workflows Documented  
**Next Document:** Data Flow Diagram  
**Implementation Priority:** CRITICAL - Foundation for entire checklist system

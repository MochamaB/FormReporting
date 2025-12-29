# Notification Types, Templates & Triggers - MVP

## Notification Matrix

| # | Event Trigger | Notification Type | Template Code | When | Recipients | Channels | Priority |
|---|---------------|-------------------|---------------|------|------------|----------|----------|
| 1 | Form submitted | FormSubmitted | FORM_SUBMITTED | On form submission | Workflow first assignee | Email, InApp | Normal |
| 2 | Workflow step assigned | WorkflowAssigned | WORKFLOW_ASSIGNED | Step assigned to user | Assignee | Email, InApp | High |
| 3 | Workflow step completed | StepCompleted | STEP_COMPLETED | Step approved/verified | Submitter, Next assignee | Email, InApp | Normal |
| 4 | Workflow approved (final) | FormApproved | FORM_APPROVED | Final step approved | Submitter | Email, InApp | Normal |
| 5 | Workflow rejected | FormRejected | FORM_REJECTED | Any step rejected | Submitter | Email, InApp | High |
| 6 | Assignment created | AssignmentCreated | ASSIGNMENT_CREATED | FormAssignment created | Assignees (users/dept) | Email, InApp | Normal |
| 7 | Deadline approaching | DeadlineReminder | DEADLINE_REMINDER | 24 hours before due | Assignee | Email, InApp | High |
| 8 | Submission overdue | OverdueAlert | OVERDUE_ALERT | Past due date | Assignee, Supervisor | Email, InApp | Urgent |
| 9 | Pending approval >48h | PendingApprovalAlert | PENDING_APPROVAL_ALERT | Workflow step pending 48h | Assignee, Supervisor | Email, InApp | Urgent |

## Template Specifications

### 1. FORM_SUBMITTED

**Category**: Forms
**Priority**: Normal
**Channels**: Email, InApp

**Placeholders**:
- `{{RecipientName}}` - Name of the user receiving notification
- `{{SubmitterName}}` - Name of person who submitted form
- `{{FormName}}` - Form template name
- `{{SubmissionDate}}` - When form was submitted
- `{{ActionUrl}}` - Link to review submission

**Email Subject**:
```
New Form Submission: {{FormName}}
```

**Email Body** (HTML):
```html
<p>Hello {{RecipientName}},</p>

<p><strong>{{SubmitterName}}</strong> has submitted a new <strong>{{FormName}}</strong> form on {{SubmissionDate}} that requires your review.</p>

<p>
  <a href="{{ActionUrl}}" style="background-color: #0d6efd; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">
    Review Submission
  </a>
</p>

<p>Thank you,<br>
FormReporting System</p>
```

**InApp Message**:
```
{{SubmitterName}} submitted {{FormName}} - Click to review
```

---

### 2. WORKFLOW_ASSIGNED

**Category**: Workflows
**Priority**: High
**Channels**: Email, InApp

**Placeholders**:
- `{{RecipientName}}`
- `{{StepName}}` - Workflow step name
- `{{FormName}}`
- `{{SubmitterName}}`
- `{{DueDate}}` - When step must be completed
- `{{ActionUrl}}`

**Email Subject**:
```
Action Required: {{StepName}} for {{FormName}}
```

**Email Body**:
```html
<p>Hello {{RecipientName}},</p>

<p>A workflow step has been assigned to you:</p>

<ul>
  <li><strong>Form:</strong> {{FormName}}</li>
  <li><strong>Step:</strong> {{StepName}}</li>
  <li><strong>Submitted by:</strong> {{SubmitterName}}</li>
  <li><strong>Due Date:</strong> {{DueDate}}</li>
</ul>

<p>Please complete this step before the deadline.</p>

<p>
  <a href="{{ActionUrl}}" style="background-color: #198754; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">
    Take Action
  </a>
</p>

<p>Thank you,<br>
FormReporting System</p>
```

**InApp Message**:
```
{{StepName}} assigned for {{FormName}} - Due {{DueDate}}
```

---

### 3. STEP_COMPLETED

**Category**: Workflows
**Priority**: Normal
**Channels**: Email, InApp

**Placeholders**:
- `{{RecipientName}}`
- `{{ReviewerName}}` - Person who completed step
- `{{StepName}}`
- `{{FormName}}`
- `{{ActionName}}` - Action taken (Approved, Verified, etc.)
- `{{ActionUrl}}`

**Email Subject**:
```
{{StepName}} Completed for {{FormName}}
```

**Email Body**:
```html
<p>Hello {{RecipientName}},</p>

<p><strong>{{ReviewerName}}</strong> has completed the <strong>{{StepName}}</strong> step for your <strong>{{FormName}}</strong> submission.</p>

<p><strong>Action Taken:</strong> {{ActionName}}</p>

<p>
  <a href="{{ActionUrl}}" style="background-color: #0d6efd; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">
    View Status
  </a>
</p>

<p>Thank you,<br>
FormReporting System</p>
```

**InApp Message**:
```
{{ReviewerName}} completed {{StepName}} for {{FormName}}
```

---

### 4. FORM_APPROVED

**Category**: Workflows
**Priority**: Normal
**Channels**: Email, InApp

**Placeholders**:
- `{{RecipientName}}`
- `{{FormName}}`
- `{{ApproverName}}` - Final approver
- `{{ApprovalDate}}`
- `{{ActionUrl}}`

**Email Subject**:
```
✓ Your {{FormName}} has been Approved
```

**Email Body**:
```html
<p>Hello {{RecipientName}},</p>

<p>Great news! Your <strong>{{FormName}}</strong> submission has been fully approved.</p>

<p><strong>Approved by:</strong> {{ApproverName}}<br>
<strong>Approval Date:</strong> {{ApprovalDate}}</p>

<p>
  <a href="{{ActionUrl}}" style="background-color: #198754; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">
    View Submission
  </a>
</p>

<p>Thank you,<br>
FormReporting System</p>
```

**InApp Message**:
```
✓ Your {{FormName}} has been approved by {{ApproverName}}
```

---

### 5. FORM_REJECTED

**Category**: Workflows
**Priority**: High
**Channels**: Email, InApp

**Placeholders**:
- `{{RecipientName}}`
- `{{FormName}}`
- `{{RejectorName}}` - Person who rejected
- `{{StepName}}` - Step where rejected
- `{{RejectionReason}}` - Reason/comments
- `{{ActionUrl}}`

**Email Subject**:
```
⚠ Action Required: {{FormName}} Rejected
```

**Email Body**:
```html
<p>Hello {{RecipientName}},</p>

<p>Your <strong>{{FormName}}</strong> submission was rejected at the <strong>{{StepName}}</strong> step.</p>

<p><strong>Rejected by:</strong> {{RejectorName}}</p>

<p><strong>Reason:</strong><br>
{{RejectionReason}}</p>

<p>Please review the feedback and resubmit if necessary.</p>

<p>
  <a href="{{ActionUrl}}" style="background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">
    View Details
  </a>
</p>

<p>Thank you,<br>
FormReporting System</p>
```

**InApp Message**:
```
⚠ Your {{FormName}} was rejected - Review feedback
```

---

### 6. ASSIGNMENT_CREATED

**Category**: Assignments
**Priority**: Normal
**Channels**: Email, InApp

**Placeholders**:
- `{{RecipientName}}`
- `{{FormName}}`
- `{{DepartmentName}}` - If assigned to department
- `{{DueDate}}`
- `{{AssignedBy}}` - Person who created assignment
- `{{ActionUrl}}`

**Email Subject**:
```
New Form Assignment: {{FormName}}
```

**Email Body**:
```html
<p>Hello {{RecipientName}},</p>

<p>A new form has been assigned to you:</p>

<ul>
  <li><strong>Form:</strong> {{FormName}}</li>
  <li><strong>Assigned by:</strong> {{AssignedBy}}</li>
  <li><strong>Due Date:</strong> {{DueDate}}</li>
</ul>

<p>Please complete this form before the deadline.</p>

<p>
  <a href="{{ActionUrl}}" style="background-color: #0d6efd; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">
    Complete Form
  </a>
</p>

<p>Thank you,<br>
FormReporting System</p>
```

**InApp Message**:
```
New assignment: {{FormName}} - Due {{DueDate}}
```

---

### 7. DEADLINE_REMINDER

**Category**: Alerts
**Priority**: High
**Channels**: Email, InApp

**Placeholders**:
- `{{RecipientName}}`
- `{{FormName}}`
- `{{DueDate}}`
- `{{HoursRemaining}}` - Hours until due
- `{{ActionUrl}}`

**Email Subject**:
```
⏰ Reminder: {{FormName}} due in {{HoursRemaining}} hours
```

**Email Body**:
```html
<p>Hello {{RecipientName}},</p>

<p><strong>Reminder:</strong> Your <strong>{{FormName}}</strong> submission is due soon.</p>

<p><strong>Due Date:</strong> {{DueDate}} ({{HoursRemaining}} hours remaining)</p>

<p>Please complete this as soon as possible to avoid it becoming overdue.</p>

<p>
  <a href="{{ActionUrl}}" style="background-color: #ffc107; color: #000; padding: 10px 20px; text-decoration: none; border-radius: 5px;">
    Complete Now
  </a>
</p>

<p>Thank you,<br>
FormReporting System</p>
```

**InApp Message**:
```
⏰ {{FormName}} due in {{HoursRemaining}} hours
```

---

### 8. OVERDUE_ALERT

**Category**: Alerts
**Priority**: Urgent
**Channels**: Email, InApp

**Placeholders**:
- `{{RecipientName}}`
- `{{FormName}}`
- `{{AssigneeName}}` - Person who should have completed it
- `{{DueDate}}`
- `{{DaysOverdue}}`
- `{{ActionUrl}}`

**Email Subject**:
```
🔴 URGENT: {{FormName}} is {{DaysOverdue}} days overdue
```

**Email Body**:
```html
<p>Hello {{RecipientName}},</p>

<p><strong>URGENT:</strong> A <strong>{{FormName}}</strong> submission is overdue.</p>

<ul>
  <li><strong>Assignee:</strong> {{AssigneeName}}</li>
  <li><strong>Due Date:</strong> {{DueDate}}</li>
  <li><strong>Days Overdue:</strong> {{DaysOverdue}}</li>
</ul>

<p>Immediate action required.</p>

<p>
  <a href="{{ActionUrl}}" style="background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">
    Take Action
  </a>
</p>

<p>Thank you,<br>
FormReporting System</p>
```

**InApp Message**:
```
🔴 URGENT: {{FormName}} is {{DaysOverdue}} days overdue
```

---

### 9. PENDING_APPROVAL_ALERT

**Category**: Alerts
**Priority**: Urgent
**Channels**: Email, InApp

**Placeholders**:
- `{{RecipientName}}`
- `{{FormName}}`
- `{{StepName}}`
- `{{AssigneeName}}`
- `{{HoursPending}}`
- `{{ActionUrl}}`

**Email Subject**:
```
⚠ Approval Pending for {{HoursPending}} hours: {{FormName}}
```

**Email Body**:
```html
<p>Hello {{RecipientName}},</p>

<p>A workflow approval has been pending for an extended period:</p>

<ul>
  <li><strong>Form:</strong> {{FormName}}</li>
  <li><strong>Step:</strong> {{StepName}}</li>
  <li><strong>Assigned to:</strong> {{AssigneeName}}</li>
  <li><strong>Time Pending:</strong> {{HoursPending}} hours</li>
</ul>

<p>Please follow up to ensure timely completion.</p>

<p>
  <a href="{{ActionUrl}}" style="background-color: #ffc107; color: #000; padding: 10px 20px; text-decoration: none; border-radius: 5px;">
    View Details
  </a>
</p>

<p>Thank you,<br>
FormReporting System</p>
```

**InApp Message**:
```
⚠ {{FormName}} approval pending for {{HoursPending}} hours
```

---

## Placeholder Dictionary

| Placeholder | Source | Data Type | Example | Notes |
|-------------|--------|-----------|---------|-------|
| `{{RecipientName}}` | User.FullName | String | "John Doe" | Person receiving notification |
| `{{SubmitterName}}` | User.FullName | String | "Jane Smith" | Person who submitted |
| `{{FormName}}` | FormTemplate.TemplateName | String | "Expense Report" | Form template name |
| `{{StepName}}` | WorkflowStep.StepName | String | "Manager Approval" | Workflow step |
| `{{ActionName}}` | WorkflowAction.ActionName | String | "Approved" | Action taken |
| `{{ReviewerName}}` | User.FullName | String | "Bob Manager" | Person who acted |
| `{{ApproverName}}` | User.FullName | String | "Alice Director" | Final approver |
| `{{RejectorName}}` | User.FullName | String | "Bob Manager" | Person who rejected |
| `{{AssigneeName}}` | User.FullName | String | "John Doe" | Assigned user |
| `{{AssignedBy}}` | User.FullName | String | "Admin User" | Who created assignment |
| `{{DepartmentName}}` | Department.DepartmentName | String | "Finance" | Department name |
| `{{DueDate}}` | DateTime | DateTime | "Dec 30, 2025 5:00 PM" | Due date formatted |
| `{{SubmissionDate}}` | DateTime | DateTime | "Dec 28, 2025 2:30 PM" | Submission date |
| `{{ApprovalDate}}` | DateTime | DateTime | "Dec 29, 2025 10:00 AM" | Approval date |
| `{{RejectionReason}}` | String | String | "Missing receipts" | Rejection comments |
| `{{HoursRemaining}}` | Calculated | Integer | "24" | Hours until due |
| `{{DaysOverdue}}` | Calculated | Integer | "3" | Days past due |
| `{{HoursPending}}` | Calculated | Integer | "52" | Hours step pending |
| `{{ActionUrl}}` | Generated | String | "/Submissions/123" | Link to entity |

## Template Usage Notes

1. **HTML Email Templates**: Use inline CSS for email client compatibility
2. **InApp Templates**: Keep concise (under 100 chars preferred)
3. **Placeholders**: Always check for null values before replacing
4. **ActionUrl**: Generate based on entity type and user permissions
5. **DateTime Formatting**: Use local timezone of recipient
6. **System Templates**: TemplateCode values are referenced in code, cannot change

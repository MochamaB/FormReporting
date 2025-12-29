# Notification Registry - Complete System Overview

This document lists ALL notifications across the entire application, organized by module.
Use this as a reference when implementing notification triggers.

---

## Module 1: IDENTITY & AUTHENTICATION

| # | Event Trigger | Template Code | Recipients | Channels | Priority | Service Method |
|---|---------------|---------------|------------|----------|----------|----------------|
| 1.1 | User account created | USER_ACCOUNT_CREATED | New user | Email, InApp | Normal | `TriggerUserAccountCreatedAsync()` |
| 1.2 | Welcome email (first login) | USER_WELCOME | New user | Email | Normal | `TriggerUserWelcomeAsync()` |
| 1.3 | Password reset requested | PASSWORD_RESET_REQUEST | User | Email | High | `TriggerPasswordResetRequestAsync()` |
| 1.4 | Password changed successfully | PASSWORD_CHANGED | User | Email, InApp | Normal | `TriggerPasswordChangedAsync()` |
| 1.5 | Email verification | EMAIL_VERIFICATION | User | Email | High | `TriggerEmailVerificationAsync()` |
| 1.6 | Account locked (failed logins) | ACCOUNT_LOCKED | User | Email, InApp | Urgent | `TriggerAccountLockedAsync()` |
| 1.7 | Suspicious login detected | SUSPICIOUS_LOGIN | User | Email, InApp | Urgent | `TriggerSuspiciousLoginAsync()` |
| 1.8 | User role changed | USER_ROLE_CHANGED | User, Admin | Email, InApp | Normal | `TriggerUserRoleChangedAsync()` |
| 1.9 | Account deactivated | ACCOUNT_DEACTIVATED | User | Email | High | `TriggerAccountDeactivatedAsync()` |

**Notes:**
- All identity notifications should be sent immediately (no scheduling)
- Email verification and password reset should have expiry links
- Suspicious login should include IP address and location if available

---

## Module 2: FORM ASSIGNMENTS

| # | Event Trigger | Template Code | Recipients | Channels | Priority | Service Method |
|---|---------------|---------------|------------|----------|----------|----------------|
| 2.1 | Assignment created | ASSIGNMENT_CREATED | Assignee(s) | Email, InApp | Normal | `TriggerFormAssignmentCreatedAsync()` |
| 2.2 | Assignment updated | ASSIGNMENT_UPDATED | Assignee(s) | InApp | Low | `TriggerFormAssignmentUpdatedAsync()` |
| 2.3 | Assignment deadline reminder | DEADLINE_REMINDER | Assignee | Email, InApp | High | `TriggerAssignmentDeadlineReminderAsync()` |
| 2.4 | Assignment overdue | OVERDUE_ALERT | Assignee, Supervisor | Email, InApp | Urgent | `TriggerAssignmentOverdueAsync()` |
| 2.5 | Assignment completed | ASSIGNMENT_COMPLETED | Assigner | InApp | Normal | `TriggerAssignmentCompletedAsync()` |
| 2.6 | Bulk assignment created | BULK_ASSIGNMENT_CREATED | Multiple users | Email, InApp | Normal | `TriggerBulkAssignmentCreatedAsync()` |

**Notes:**
- Department assignments send to all users in department
- Reminders are triggered by scheduled jobs (Hangfire)
- Overdue alerts should escalate to supervisor after 2 days

---

## Module 3: FORM SUBMISSIONS

| # | Event Trigger | Template Code | Recipients | Channels | Priority | Service Method |
|---|---------------|---------------|------------|----------|----------|----------------|
| 3.1 | Form submitted | FORM_SUBMITTED | Workflow first assignee | Email, InApp | Normal | `TriggerFormSubmittedAsync()` |
| 3.2 | Form saved as draft | FORM_DRAFT_SAVED | Submitter | InApp | Low | `TriggerFormDraftSavedAsync()` |
| 3.3 | Form approved (final) | FORM_APPROVED | Submitter | Email, InApp | Normal | `TriggerFormApprovedAsync()` |
| 3.4 | Form rejected | FORM_REJECTED | Submitter | Email, InApp | High | `TriggerFormRejectedAsync()` |
| 3.5 | Form returned for revision | FORM_RETURNED | Submitter | Email, InApp | Normal | `TriggerFormReturnedAsync()` |
| 3.6 | Revision submitted | REVISION_SUBMITTED | Previous reviewer | Email, InApp | Normal | `TriggerRevisionSubmittedAsync()` |

**Notes:**
- Draft saved should only send InApp (avoid email spam)
- Rejection should include reviewer comments
- Approved forms should have download link to PDF

---

## Module 4: WORKFLOWS

| # | Event Trigger | Template Code | Recipients | Channels | Priority | Service Method |
|---|---------------|---------------|------------|----------|----------|----------------|
| 4.1 | Workflow step assigned | WORKFLOW_ASSIGNED | Step assignee | Email, InApp | High | `TriggerWorkflowStepAssignedAsync()` |
| 4.2 | Workflow step completed | STEP_COMPLETED | Submitter, Next assignee | Email, InApp | Normal | `TriggerWorkflowStepCompletedAsync()` |
| 4.3 | Workflow reassigned | WORKFLOW_REASSIGNED | New assignee, Previous assignee | Email, InApp | High | `TriggerWorkflowReassignedAsync()` |
| 4.4 | Pending approval alert | PENDING_APPROVAL_ALERT | Step assignee, Supervisor | Email, InApp | Urgent | `TriggerPendingApprovalAlertAsync()` |
| 4.5 | Workflow delegated | WORKFLOW_DELEGATED | Delegate | Email, InApp | Normal | `TriggerWorkflowDelegatedAsync()` |
| 4.6 | Workflow cancelled | WORKFLOW_CANCELLED | All participants | Email, InApp | Normal | `TriggerWorkflowCancelledAsync()` |

**Notes:**
- Pending approval alerts trigger after 48 hours of inactivity
- Reassignment should explain reason if provided
- Cancellation should include reason

---

## Module 5: FORM TEMPLATES (Admin)

| # | Event Trigger | Template Code | Recipients | Channels | Priority | Service Method |
|---|---------------|---------------|------------|----------|----------|----------------|
| 5.1 | Template published | TEMPLATE_PUBLISHED | Template subscribers | Email, InApp | Normal | `TriggerTemplatePublishedAsync()` |
| 5.2 | Template updated | TEMPLATE_UPDATED | Active assignment users | InApp | Low | `TriggerTemplateUpdatedAsync()` |
| 5.3 | Template deprecated | TEMPLATE_DEPRECATED | Users with active assignments | Email, InApp | Normal | `TriggerTemplateDeprecatedAsync()` |
| 5.4 | Template deleted | TEMPLATE_DELETED | Admins | InApp | Normal | `TriggerTemplateDeletedAsync()` |

**Notes:**
- Template changes affecting active assignments should notify users
- Deprecation should provide migration path to new template

---

## Module 6: COMMENTS & COLLABORATION

| # | Event Trigger | Template Code | Recipients | Channels | Priority | Service Method |
|---|---------------|---------------|------------|----------|----------|----------------|
| 6.1 | Comment added | COMMENT_ADDED | Submitter, Mentioned users | InApp | Low | `TriggerCommentAddedAsync()` |
| 6.2 | User mentioned in comment | USER_MENTIONED | Mentioned user | Email, InApp | Normal | `TriggerUserMentionedAsync()` |
| 6.3 | Comment reply | COMMENT_REPLY | Original commenter | InApp | Low | `TriggerCommentReplyAsync()` |

**Notes:**
- Support @username mentions
- Comment notifications should batch if multiple comments in short time
- Include context (form name, step) in notification

---

## Module 7: SYSTEM & ADMIN

| # | Event Trigger | Template Code | Recipients | Channels | Priority | Service Method |
|---|---------------|---------------|------------|----------|----------|----------------|
| 7.1 | System maintenance scheduled | SYSTEM_MAINTENANCE | All active users | Email, InApp | High | `TriggerSystemMaintenanceAsync()` |
| 7.2 | System back online | SYSTEM_ONLINE | All active users | InApp | Normal | `TriggerSystemOnlineAsync()` |
| 7.3 | Data export ready | DATA_EXPORT_READY | Requester | Email, InApp | Normal | `TriggerDataExportReadyAsync()` |
| 7.4 | Report generated | REPORT_GENERATED | Requester | Email, InApp | Normal | `TriggerReportGeneratedAsync()` |
| 7.5 | Bulk operation completed | BULK_OPERATION_COMPLETE | Initiator | Email, InApp | Normal | `TriggerBulkOperationCompleteAsync()` |
| 7.6 | Error occurred (admin) | SYSTEM_ERROR | Admins | InApp | Urgent | `TriggerSystemErrorAsync()` |

**Notes:**
- Maintenance notifications should be sent 24 hours in advance
- Export/Report notifications should include download link
- System errors should aggregate (don't spam admins)

---

## Module 8: NOTIFICATIONS SETTINGS (User Preferences)

| # | Event Trigger | Template Code | Recipients | Channels | Priority | Service Method |
|---|---------------|---------------|------------|----------|----------|----------------|
| 8.1 | Digest email (daily summary) | NOTIFICATION_DIGEST_DAILY | Users with digest enabled | Email | Normal | `TriggerDailyDigestAsync()` |
| 8.2 | Digest email (weekly summary) | NOTIFICATION_DIGEST_WEEKLY | Users with digest enabled | Email | Normal | `TriggerWeeklyDigestAsync()` |
| 8.3 | Too many unread notifications | UNREAD_NOTIFICATIONS_ALERT | User | InApp | Low | `TriggerUnreadAlertAsync()` |

**Notes:**
- Digest emails sent via scheduled jobs
- Respect user notification preferences (email on/off per category)
- Allow users to configure quiet hours

---

## Implementation Priority (MVP)

### Phase 1 (Immediate - MVP Core)
- ✅ 2.1 Assignment created
- ✅ 3.1 Form submitted
- ✅ 3.3 Form approved
- ✅ 3.4 Form rejected
- ✅ 4.1 Workflow assigned
- ✅ 4.2 Workflow step completed
- ✅ 2.3 Deadline reminder
- ✅ 2.4 Assignment overdue
- ✅ 4.4 Pending approval alert

### Phase 2 (Identity & Security)
- 1.1 User account created
- 1.3 Password reset request
- 1.4 Password changed
- 1.5 Email verification
- 1.6 Account locked

### Phase 3 (Enhanced Collaboration)
- 3.5 Form returned for revision
- 4.3 Workflow reassigned
- 6.1 Comment added
- 6.2 User mentioned

### Phase 4 (Admin & System)
- 5.1 Template published
- 7.1 System maintenance
- 7.3 Data export ready
- 7.4 Report generated

### Phase 5 (Optimization)
- 8.1 Daily digest
- 8.2 Weekly digest
- 3.2 Form draft saved
- Comment batching

---

## Template Code Naming Convention

Format: `{MODULE}_{ACTION}_{ENTITY}`

Examples:
- `USER_ACCOUNT_CREATED` - Identity module, account creation
- `FORM_SUBMITTED` - Forms module, submission
- `WORKFLOW_ASSIGNED` - Workflow module, assignment

**Rules:**
- All uppercase
- Underscores only
- Be specific (FORM_APPROVED vs APPROVED)
- Consistent verbs (CREATED, UPDATED, DELETED, ASSIGNED, COMPLETED)

---

## Service Method Naming Convention

Format: `Trigger{Entity}{Action}Async`

Examples:
- `TriggerUserAccountCreatedAsync(int userId, int createdByUserId)`
- `TriggerFormSubmittedAsync(int responseId)`
- `TriggerWorkflowStepAssignedAsync(int stepInstanceId, int assignedToUserId)`

**Parameters:**
- Always include entity ID (assignmentId, userId, responseId)
- Include actor IDs when relevant (createdByUserId, approvedByUserId)
- Optional parameters for additional context (comments, reason, etc.)

---

## Notification Priority Levels

| Priority | Usage | Email | InApp | Response Time |
|----------|-------|-------|-------|---------------|
| **Urgent** | Security issues, critical deadlines | ✅ | ✅ | Immediate |
| **High** | Action required, approaching deadlines | ✅ | ✅ | Within 1 hour |
| **Normal** | Standard notifications | ✅ | ✅ | Within 24 hours |
| **Low** | Informational, can wait | ❌ | ✅ | Batched/digest |

---

## Channel Selection Guidelines

### When to use EMAIL:
- User hasn't logged in recently
- High/Urgent priority
- Requires action outside system (password reset, verification)
- End-of-day summaries/digests

### When to use IN-APP:
- User is active in system
- Quick informational updates
- Draft saves, minor status changes
- Complements email notification

### When to use SMS (Future):
- Critical security alerts
- Time-sensitive approvals
- System downtime

---

## Next Steps

1. ✅ Seed Phase 1 templates (DONE)
2. ⏳ Implement NotificationTriggerService with Phase 1 methods
3. ⏳ Integrate triggers into existing services (FormAssignmentService, WorkflowService)
4. ⏳ Create notification UI pages
5. ⏳ Implement Phase 2 (Identity) templates and triggers
6. ⏳ Add user notification preferences
7. ⏳ Implement digest emails (Hangfire)
8. ⏳ Add SMS provider (Phase 3+)

---

## Quick Reference: Where to Add Triggers

| Module | Service File | Controller | Trigger Methods Needed |
|--------|-------------|------------|------------------------|
| Identity | `IdentityService.cs` | `AccountController.cs` | 9 triggers (1.1-1.9) |
| Assignments | `FormAssignmentService.cs` | `FormAssignmentsController.cs` | 6 triggers (2.1-2.6) |
| Submissions | `FormResponseService.cs` | `FormResponsesController.cs` | 6 triggers (3.1-3.6) |
| Workflows | `WorkflowService.cs` | `WorkflowController.cs` | 6 triggers (4.1-4.6) |
| Templates | `FormTemplateService.cs` | `FormTemplatesController.cs` | 4 triggers (5.1-5.4) |
| Comments | `CommentService.cs` | `CommentsController.cs` | 3 triggers (6.1-6.3) |
| System | Various admin services | `AdminController.cs` | 6 triggers (7.1-7.6) |
| Digest | `NotificationDigestService.cs` | Background Job | 3 triggers (8.1-8.3) |

**Total Triggers to Implement: 43**
**Phase 1 MVP: 9 triggers**

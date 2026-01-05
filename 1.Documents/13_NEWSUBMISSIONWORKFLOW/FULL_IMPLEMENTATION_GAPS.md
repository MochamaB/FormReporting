# FormTemplate System - Full Implementation Gaps

## Current State Summary

| Component | Backend | Frontend | Integration |
|-----------|---------|----------|-------------|
| Assignments | ✅ Complete | ✅ Complete | ❌ Not enforced |
| Workflows | ✅ Complete | ✅ Complete | ❌ Not triggered |
| Submission Rules | ✅ Complete | ⚠️ Partial | ❌ Not enforced |
| Metrics | ✅ Complete | ⚠️ Partial | ❌ Not populated |
| Collaborative Rendering | ❌ Missing | ❌ Missing | ❌ Missing |

---

## P0: Critical - Data Flow Broken

### 1. Post-Submission Processing (FormResponseService.SubmitAsync)

**Current:** Sets status and saves. Nothing else.

**Missing:**
- Call SubmissionRuleService.ValidateSubmissionTimingAsync before allowing submit
- Call WorkflowEngineService.InitializeSubmissionWorkflowAsync after submit
- Call MetricPopulationService.PopulateMetricsFromSubmissionAsync after submit
- Call NotificationTriggerService.TriggerFormSubmittedAsync after submit

### 2. Assignment-Based Template Filtering

**Current:** GetAvailableTemplatesAsync returns ALL published templates.

**Missing:**
- Filter templates by user's active assignments
- Check assignment effective dates
- Check assignment status (Active only)
- Respect AllowAnonymous flag

### 3. Assignment-Based Submission Access

**Current:** CanUserAccessTemplateAsync returns true if template exists.

**Missing:**
- Call FormAssignmentService.CheckUserAccessAsync
- Verify user matches assignment criteria (tenant, role, department, etc.)

---

## P1: High - Collaborative Mode Not Functional

### 4. Form Rendering for Collaborative Mode

**Current:** All sections render equally for all users.

**Missing in FormViewModel:**
- SubmissionMode property
- CurrentWorkflowStepId property
- EditableSectionIds list
- Per-section IsEditableByCurrentUser flag
- Per-section AssignedToUserName for display

**Missing in BuildFormViewModelAsync:**
- Load workflow progress for submission
- Get current user's pending workflow step
- Filter editable sections based on step's TargetSectionId
- Mark non-editable sections as read-only
- Include step assignee info for locked sections

**Missing in SubmissionsController.Fill:**
- Check if user has pending workflow step for this submission
- Redirect if no pending action
- Pass workflow context to view

### 5. Section-Level Completion (Collaborative)

**Current:** Only full form submit exists.

**Missing:**
- "Complete Section" action for FILL workflow steps
- Call WorkflowEngineService.CompleteStepAsync when section done
- Auto-advance to next step after section completion
- Auto-submit form when all FILL steps complete

### 6. Collaborative Mode Entry Point

**Current:** CreateSubmission creates draft, user fills all sections.

**Missing:**
- For Collaborative: Create submission and initialize workflow immediately
- First FILL step assignee gets notified
- Other users cannot access until their step is active

---

## P2: Medium - Features Not Enforced

### 7. Submission Rules Enforcement

**Current:** Rules exist but not checked anywhere.

**Missing:**
- Check ValidateSubmissionTimingAsync before allowing form access
- Display due date/time on form
- Show warning if approaching due date
- Block submission if past due + grace period (when AllowLateSubmission=false)
- Mark submission as "Late" if submitted after due date

### 8. Submission Rule Reminders

**Current:** GetRulesNeedingRemindersAsync exists but never called.

**Missing:**
- Background job to check rules needing reminders daily
- Parse ReminderDaysBefore (e.g., "7,3,1")
- Send reminder notifications via NotificationTriggerService
- Track sent reminders to avoid duplicates

### 9. Workflow Notifications

**Current:** WorkflowEngineService actions complete but no notifications sent.

**Missing in CompleteStepAsync:**
- Call NotificationTriggerService.TriggerWorkflowStepCompletedAsync
- Notify next step assignee

**Missing in InitializeSubmissionWorkflowAsync:**
- Call NotificationTriggerService.TriggerWorkflowStepAssignedAsync for first step

### 10. Formula Evaluator for Metrics

**Current:** Basic regex replacement for formulas.

**Missing:**
- Replace with NCalc or DynamicExpresso library
- Support complex formulas with parentheses
- Handle division by zero gracefully
- Support conditional formulas

---

## P3: Low - Polish & Edge Cases

### 11. Collaborative Mode UI Indicators

**Missing:**
- Lock icon on sections assigned to other users
- "Assigned to [Name]" label on locked sections
- Progress bar showing completed vs pending sections
- Real-time update when other user completes (SignalR)

### 12. Workflow Step Delegation

**Current:** DelegateStepAsync exists but no UI.

**Missing:**
- UI to delegate step to another user
- Validation that delegate has appropriate access
- Notification to new assignee

### 13. Submission Comments/Notes

**Current:** CommentsCount hardcoded to 0.

**Missing:**
- Comments entity and table
- Add comment during workflow step completion
- Display comment history on submission view

### 14. Submission Flagging

**Current:** IsFlagged hardcoded to false.

**Missing:**
- Flag submission for review
- Filter submissions by flagged status
- Notification when submission flagged

### 15. Bulk Submission Actions

**Current:** BulkAction endpoint has TODO placeholders.

**Missing:**
- Implement actual delete logic
- Implement bulk approve logic
- Add bulk reject action

---

## Implementation Order

### Phase 1: Fix Data Flow (P0)
1. Update FormResponseService.SubmitAsync to call workflow + metrics + notifications
2. Update GetAvailableTemplatesAsync to filter by assignments
3. Update CanUserAccessTemplateAsync to check assignments

### Phase 2: Enable Collaborative Mode (P1)
4. Add workflow context properties to FormViewModel
5. Update BuildFormViewModelAsync for collaborative filtering
6. Update SubmissionsController.Fill for workflow validation
7. Add section-level completion endpoint
8. Update form wizard UI for read-only sections

### Phase 3: Enforce Rules (P2)
9. Add submission timing validation to form access
10. Create background job for reminders
11. Add notifications to workflow engine
12. Upgrade formula evaluator

### Phase 4: Polish (P3)
13. Add collaborative UI indicators
14. Build delegation UI
15. Implement comments feature
16. Implement flagging feature
17. Complete bulk actions

---

## Key Files to Modify

| Phase | File | Changes |
|-------|------|---------|
| 1 | Services/Forms/FormResponseService.cs | Add post-submit calls |
| 1 | Services/Forms/FormSubmissionService.cs | Add assignment filtering |
| 2 | Models/ViewModels/Components/FormViewModel.cs | Add workflow properties |
| 2 | Services/Forms/FormSubmissionService.cs | Add BuildFormViewModelForWorkflowAsync |
| 2 | Controllers/Submissions/SubmissionsController.cs | Add workflow checks |
| 2 | Views/Shared/Components/Form/_FormWizard.cshtml | Add section filtering |
| 3 | Controllers/Submissions/SubmissionsController.cs | Add timing validation |
| 3 | Services/Forms/WorkflowEngineService.cs | Add notification calls |

---

## Success Criteria

**Phase 1 Complete When:**
- Users only see templates they're assigned to
- Metrics populate after submission
- Workflows initialize after submission

**Phase 2 Complete When:**
- Collaborative mode users only edit their assigned sections
- Section completion advances workflow
- Form auto-submits when all FILL steps done

**Phase 3 Complete When:**
- Late submissions blocked/flagged per rules
- Reminder emails sent before due dates
- Workflow participants notified of their pending actions

**Phase 4 Complete When:**
- Locked sections show assignee info
- Users can delegate their workflow steps
- Comments attached to submissions

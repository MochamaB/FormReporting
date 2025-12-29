# **DUAL-MODE FORM SYSTEM - COMPLETE LOGIC**

## **1. CORE CONCEPT: FormTemplate.SubmissionMode**

### **The Distinguishing Field**

When creating a FormTemplate, the admin/creator selects:

**FormTemplate.SubmissionMode = "Traditional" OR "Collaborative"**

This single field fundamentally changes:
- Who can access the form
- How the submission is initiated
- Whether "Fill" is a workflow action
- How sections/fields are rendered
- When the submission is considered "complete"
- How assignments and workflows interact

---

## **2. TRADITIONAL MODE - COMPLETE FLOW**

### **2A. Template Creation Phase**

**What Happens:**
- Admin creates FormTemplate
- Sets SubmissionMode = "Traditional"
- Builds form structure (sections, fields)
- Creates FormTemplateAssignments (who can submit)
- Creates Workflow with ONLY post-submission actions

**Workflow Structure:**
```
Step 1: Approve (Manager) - NOT Fill
Step 2: Sign (Director)
Step 3: Verify (Auditor)

Notice: NO "Fill" action in workflow
```

**Assignment Logic:**
- Assignment determines: "Who can create and fill a new submission"
- Example: "All ICT Managers" or "Finance Department"
- These users will see "New Submission" button
- They fill the ENTIRE form themselves

### **2B. User Perspective - Starting Submission**

**User Journey:**
1. User logs in
2. System checks: "Do I have a FormTemplateAssignment for this template?"
3. If YES → "New Submission" button appears
4. User clicks "New Submission"

**What Happens Behind the Scenes:**
- New FormTemplateSubmission record created
- SubmittedBy = current user
- Status = "Draft"
- CreatedDate = now
- NO WorkflowProgress records created yet (workflow hasn't started)

### **2C. Form Filling Phase**

**What User Sees:**
- ALL sections visible
- ALL fields editable
- No sections locked
- No "assigned to you" messaging
- Standard form filling experience

**User Actions:**
- Fill fields across all sections
- Save draft (multiple times, auto-save)
- Validate fields
- When ready → Click "Submit"

**Key Point:** This is NOT a workflow step. This is PRE-workflow.

### **2D. Submission Trigger**

**When User Clicks "Submit":**

**System Actions:**
1. Validate all required fields filled
2. Change Status: "Draft" → "Submitted"
3. Set SubmittedDate = now
4. **NOW workflow starts** → Create SubmissionWorkflowProgress records
5. For each WorkflowStep, create progress record:
   - Step 1 → Status = "InProgress", AssignedTo = resolved assignee
   - Step 2+ → Status = "Pending"
6. Send notification to Step 1 assignee (Manager)

**Key Point:** Workflow BEGINS after submission, not during form filling.

### **2E. Approval Phase**

**Manager's Experience:**
1. Manager logs in
2. Sees notification: "New approval pending"
3. Clicks to view submission
4. Sees ENTIRE form (read-only)
5. All sections visible, no sections locked
6. Reviews data
7. Takes action: Approve/Reject/Delegate

**System Actions After Approval:**
1. Update SubmissionWorkflowProgress for Step 1:
   - Status = "Completed"
   - ReviewedBy = Manager
   - ReviewedDate = now
   - Comments = "Looks good"
2. Activate Step 2:
   - Step 2 Status = "InProgress"
   - Resolve assignee (Director)
   - Send notification
3. If all steps complete → Submission.Status = "Approved"

### **2F. Key Characteristics of Traditional Mode**

**Assignments:**
- Control who can START a submission
- User assigned = User can fill entire form
- No section-level assignments

**Workflow:**
- ONLY post-submission actions
- Actions: Approve, Reject, Sign, Review, Verify
- NO Fill action
- Sequential or parallel steps

**Submission:**
- One user creates and fills
- SubmittedBy = that one user
- Status: Draft → Submitted → InApproval → Approved/Rejected

**Form Rendering:**
- All sections visible to submitter
- All fields editable during Draft
- All read-only during approval

---

## **3. COLLABORATIVE MODE - COMPLETE FLOW**

### **3A. Template Creation Phase**

**What Happens:**
- Admin creates FormTemplate
- Sets SubmissionMode = "Collaborative"
- Builds form structure (sections, fields)
- Creates Workflow with Fill actions FIRST
- Assignments are OPTIONAL or not used

**Workflow Structure:**
```
Step 1: Fill Section 2 (Budget) → Finance User
Step 2: Fill Section 1 (Technical) → Tech User
Step 3: Fill Section 3 (HR) → HR User
Step 4: Review → Compliance Officer
Step 5: Approve → Manager
Step 6: Sign → Director

Notice: Fill actions are FIRST in workflow
```

**Assignment Logic:**
- Assignments are OPTIONAL
- If used, they grant "View" permission only
- Workflow controls who fills what

### **3B. Submission Initiation - System/Admin Creates**

**Key Difference:** User does NOT click "New Submission"

**Instead, System/Admin Initiates:**

**Option A: Admin Manually Initiates**
- Admin goes to "Manage Submissions"
- Clicks "Create Submission for Period"
- Selects: Template, Tenant, Reporting Period
- System creates blank submission
- Status = "InProgress" (not Draft)

**Option B: System Auto-Initiates**
- Scheduled job runs monthly
- For each active tenant
- Creates submission for "Monthly ICT Report"
- Status = "InProgress"

**What Gets Created:**
1. FormTemplateSubmission:
   - SubmittedBy = System/Admin (not the actual filler)
   - Status = "InProgress"
   - SubmittedDate = NULL (not submitted yet)
2. SubmissionWorkflowProgress records for ALL steps:
   - Step 1 (Fill Budget) → Status = "InProgress", AssignedTo = Finance User
   - Step 2 (Fill Technical) → Status = "Pending"
   - Step 3+ → Status = "Pending"
3. Send notification to Step 1 assignee

**Key Point:** Workflow starts IMMEDIATELY when submission is created.

### **3C. Collaborative Filling Phase - Step by Step**

#### **Step 1: Finance User Fills Budget Section**

**Finance User's Experience:**
1. Receives notification: "You are assigned to fill Budget section"
2. Logs in, sees task in dashboard
3. Clicks to open submission
4. Sees ONLY Section 2 (Budget) editable
5. Other sections are HIDDEN or SHOWN but LOCKED with message:
   - "Section 1: Assigned to Tech User (pending)"
   - "Section 3: Assigned to HR User (pending)"

**Form Rendering Logic:**
- Filter sections based on WorkflowStep.TargetType and TargetId
- If TargetType = "Section" AND TargetId = 2 → Only show Section 2
- All other sections: Hidden or read-only with assignment info
- User fills only their assigned section

**Finance User Actions:**
1. Fills Budget fields
2. Validates section
3. Clicks "Complete Section" (NOT "Submit")

**System Actions:**
1. Save FormTemplateResponse records for Section 2 fields
2. Update SubmissionWorkflowProgress for Step 1:
   - Status = "Completed"
   - ReviewedBy = Finance User
   - ReviewedDate = now
3. Activate Step 2:
   - Step 2 Status = "InProgress"
   - Resolve assignee (Tech User)
   - Send notification
4. Submission.Status remains "InProgress" (not all sections filled)

#### **Step 2: Tech User Fills Technical Section**

**Tech User's Experience:**
1. Receives notification: "You are assigned to fill Technical section"
2. Opens submission
3. Sees:
   - Section 1 (Technical) → EDITABLE (assigned to them)
   - Section 2 (Budget) → READ-ONLY (already filled by Finance)
   - Section 3 (HR) → LOCKED (not yet assigned)

**Tech User Actions:**
1. Fills Technical fields
2. Can VIEW Budget section (read-only, for context)
3. Clicks "Complete Section"

**System Actions:**
- Same as Step 1
- Activate Step 3 (HR User)
- Still Status = "InProgress"

#### **Step 3: HR User Fills HR Section**

**Same pattern as above**

**After HR Completes:**
- All "Fill" steps now complete
- System checks: "Are all Fill steps done?"
- If YES:
  - Change Submission.Status: "InProgress" → "Submitted"
  - Set SubmittedDate = now
  - Move to next step (Review/Approve)

**Key Point:** "Submitted" happens AFTER all Fill steps, not after first fill.

### **3D. Approval Phase (Same as Traditional)**

**Compliance Officer Reviews:**
- Sees ENTIRE form (all sections, read-only)
- Reviews all filled data
- Cannot edit
- Marks as "Reviewed" with comments
- Step completes, moves to Approve

**Manager Approves:**
- Same experience as Traditional mode
- Sees entire submission
- Approves/Rejects
- If approved → Director signs

**Director Signs:**
- Final step
- Adds signature
- Completion

### **3E. Key Characteristics of Collaborative Mode**

**Assignments:**
- OPTIONAL or grant "View" permission only
- NOT used to control who fills what
- Workflow controls filling

**Workflow:**
- INCLUDES Fill actions as initial steps
- Fill actions specify section/field targets
- Approval actions come AFTER all fills complete

**Submission:**
- Created by System/Admin, NOT user
- SubmittedBy = System/Admin/Initiator
- Status: InProgress → Submitted (when fills done) → InApproval → Approved

**Form Rendering:**
- Section visibility based on current workflow step
- Only assigned section editable
- Other sections hidden or read-only
- Progress indicator shows "Section 1/3 complete"

---

## **4. HOW THE MODELS ADAPT TO EACH MODE**

### **4A. FormTemplateAssignment Behavior**

**Traditional Mode:**
```
Assignment Purpose: "Who can create and fill submissions"
Assignment Check: On "New Submission" button click
Assignment Scope: Entire form
Result: User can fill ALL sections
```

**Collaborative Mode:**
```
Assignment Purpose: "Who can view submissions" (optional)
Assignment Check: NOT used for filling
Assignment Scope: View-only permission
Result: User sees submission but cannot fill (workflow assigns)
OR: Assignments not used at all
```

### **4B. WorkflowDefinition & WorkflowStep Behavior**

**Traditional Mode:**
```
Step 1: Approve (Manager)
Step 2: Sign (Director)
Step 3: Verify (Auditor)

NO Fill actions
All steps are post-submission
AssigneeType: Role, User, Department (static)
TargetType: "Submission" (entire form)
```

**Collaborative Mode:**
```
Step 1: Fill, TargetType=Section, TargetId=2 (Finance User)
Step 2: Fill, TargetType=Section, TargetId=1 (Tech User)
Step 3: Fill, TargetType=Section, TargetId=3 (HR User)
Step 4: Review (Compliance)
Step 5: Approve (Manager)
Step 6: Sign (Director)

Fill actions FIRST
Approval actions AFTER
Each Fill step targets specific section/field
```

### **4C. FormTemplateSubmission Behavior**

**Traditional Mode:**
```
Created When: User clicks "New Submission"
Created By: User (submitter)
SubmittedBy: That user
Status Flow: Draft → Submitted → InApproval → Approved
SubmittedDate: When user clicks "Submit"
WorkflowProgress Created: AFTER submission
```

**Collaborative Mode:**
```
Created When: System/Admin initiates
Created By: System/Admin
SubmittedBy: System/Admin/Initiator (NOT the fillers)
Status Flow: InProgress → Submitted → InApproval → Approved
SubmittedDate: When ALL Fill steps complete
WorkflowProgress Created: IMMEDIATELY when submission created
```

### **4D. SubmissionWorkflowProgress Behavior**

**Traditional Mode:**
```
Created: After user clicks "Submit"
First Step: Always "InProgress" (Approve/Review)
Status Values: Pending, InProgress, Completed, Rejected
ReviewedBy: Approvers only (not submitter)
```

**Collaborative Mode:**
```
Created: When submission is created
First Step: "Fill" action, InProgress
Status Values: Pending, InProgress, Completed (for Fill steps)
ReviewedBy: Fillers AND approvers (each person who acts)
```

---

## **5. FORM RENDERING LOGIC DIFFERENCES**

### **5A. Traditional Mode Rendering**

**Submitter View (During Draft):**
```
Show: All sections
Edit: All fields
Lock: Nothing
Progress: "X% complete" (based on filled fields)
Action Button: "Save Draft" | "Submit"
```

**Approver View (After Submit):**
```
Show: All sections
Edit: Nothing (read-only)
Lock: Everything
Highlight: None (entire form in scope)
Action Button: "Approve" | "Reject" | "Delegate"
```

### **5B. Collaborative Mode Rendering**

**Filler View (During Assigned Step):**
```
Show: Assigned section + completed sections (read-only)
Edit: ONLY assigned section/fields
Lock: Pending sections (not yet assigned)
Highlight: Assigned section (bold, colored border)
Progress: "Section 2/5 complete"
Action Button: "Complete Section"
```

**Example Visual:**
```
✅ Section 1: Technical Details (Completed by John)
   [Read-only view of filled fields]

⏩ Section 2: Budget Information (ASSIGNED TO YOU)
   [Editable fields]

⏸️ Section 3: HR Section (Assigned to Jane, pending)
   [Locked, greyed out]

⏸️ Section 4: Approvals (Pending)
   [Locked]
```

**Reviewer/Approver View (After Fills Complete):**
```
Show: All sections
Edit: Nothing (read-only)
Lock: Everything (same as Traditional)
Highlight: None
Action Button: "Approve" | "Reject"
```

---

## **6. ASSIGNMENT-WORKFLOW INTERACTION**

### **6A. Traditional Mode Interaction**

**Sequence:**
1. Check Assignment → "Can user create submission?"
2. If YES → User creates submission, fills form
3. User submits → Workflow STARTS
4. Workflow resolves approvers (from WorkflowStep.AssigneeType)

**Assignment and Workflow are SEPARATE:**
- Assignment: Pre-workflow (who fills)
- Workflow: Post-workflow (who approves)
- No overlap

### **6B. Collaborative Mode Interaction**

**Sequence:**
1. System creates submission
2. Workflow STARTS immediately
3. Workflow Step 1: Resolve assignee → Finance User
4. System checks: "Does Finance User have permission?" (optional Assignment check)
5. Finance User fills section
6. Repeat for each Fill step
7. After fills → Approval steps begin

**Assignment and Workflow OVERLAP:**
- Assignment: Optional, grants view permission
- Workflow: Controls EVERYTHING (who fills what, who approves)
- Workflow is primary authority

**Optional Assignment Check:**
- If FormTemplateAssignment exists for Finance User → Allowed
- If NOT exists → Workflow grants temporary permission
- Assignment is safety check, not primary control

---

## **7. SUBMISSION COMPLETION LOGIC**

### **7A. Traditional Mode Completion**

**What Triggers "Submitted" Status:**
- User clicks "Submit" button
- System validates all required fields
- Status changes: Draft → Submitted

**What Triggers "Approved" Status:**
- All mandatory workflow steps complete
- Last step completes (e.g., Director signs)
- Status changes: InApproval → Approved

### **7B. Collaborative Mode Completion**

**What Triggers "Submitted" Status:**
- System checks: "Are all Fill steps complete?"
- If YES (Section 1, 2, 3 all filled) → Status = "Submitted"
- SubmittedDate = when last Fill step completes

**What Triggers "Approved" Status:**
- Same as Traditional
- All approval steps complete
- Status = Approved

**Key Difference:**
- Traditional: "Submitted" = user action (button click)
- Collaborative: "Submitted" = automatic (when fills complete)

---

## **8. USER DASHBOARD VIEWS**

### **8A. Traditional Mode Dashboard**

**Submitter's Dashboard:**
```
My Submissions:
- Draft (3) - Forms I started but haven't submitted
- Submitted (5) - Waiting for approval
- Approved (12) - Completed

Available Forms:
- Monthly ICT Report [New Submission]
- Expense Request [New Submission]
```

**Approver's Dashboard:**
```
Pending Approvals:
- Monthly ICT Report (Kangaita) - Due Tomorrow
- Expense Request (John Doe) - Due in 3 days

[View & Approve buttons]
```

### **8B. Collaborative Mode Dashboard**

**Filler's Dashboard:**
```
My Tasks:
- Fill Budget Section (Monthly Report) - Due Tomorrow
- Fill Technical Section (Quarterly Review) - Due in 5 days

Completed Sections:
- Budget Section (Monthly Report - Nov) ✓
- HR Section (Annual Report) ✓
```

**Approver's Dashboard:**
```
Pending Approvals:
- Monthly ICT Report (All sections complete) - Due Tomorrow
- Quarterly Review (In Progress - 2/4 sections complete)

[View & Approve for complete ones only]
```

**Key Difference:**
- Traditional: Task = entire submission
- Collaborative: Task = specific section

---

## **9. VALIDATION & BUSINESS RULES**

### **9A. Traditional Mode Rules**

**On Submit:**
- All required fields must be filled
- Cannot submit partial form
- All sections must validate

**On Workflow Creation:**
- Cannot have "Fill" action in workflow
- All steps must be Approve/Sign/Review/Verify
- Steps can be sequential or parallel

### **9B. Collaborative Mode Rules**

**On Section Complete:**
- Only assigned section required fields must be filled
- Other sections can be empty
- Validation is section-specific

**On Workflow Creation:**
- Must have at least one "Fill" step
- Fill steps must cover all sections OR specify which sections
- Fill steps must come before Approve steps
- Each Fill step must have AssigneeType specified

**System Check:**
- If Fill steps don't cover all sections → Warning:
  - "Some sections have no assigned filler. They will be skipped."

---

## **10. IMPLEMENTATION DECISION POINTS**

### **10A. When Creating FormTemplate**

**Admin Workflow:**
1. Create template, build sections/fields
2. **DECISION POINT**: "How will this form be filled?"
   - Option A: "Single user fills entire form" → Traditional
   - Option B: "Multiple users collaborate on sections" → Collaborative
3. Save SubmissionMode selection
4. Create Workflow:
   - If Traditional → Only approval actions
   - If Collaborative → Fill actions first, then approvals
5. Create Assignments:
   - If Traditional → Assign who can submit
   - If Collaborative → Optional (workflow handles it)

### **10B. When User Accesses Form**

**System Checks:**
```
1. Load FormTemplate
2. Check SubmissionMode

IF Traditional:
   - Check Assignment: "Is user assigned?"
   - Show "New Submission" button
   - Render all sections editable

IF Collaborative:
   - Check WorkflowProgress: "Is user assigned to current step?"
   - Show "Complete Section" button
   - Render only assigned section editable
```

---

## **11. SUMMARY: KEY DIFFERENCES TABLE**

| Aspect | Traditional Mode | Collaborative Mode |
|--------|-----------------|-------------------|
| **Who creates submission** | User clicks "New Submission" | System/Admin initiates |
| **SubmittedBy** | Actual user | System/Admin/Initiator |
| **Assignment purpose** | Who can submit | View permission (optional) |
| **Workflow starts** | After user submits | Immediately on creation |
| **Fill action** | NOT in workflow | FIRST steps in workflow |
| **Section visibility** | All sections visible | Only assigned section |
| **Progress tracking** | % of fields filled | # of sections completed |
| **Submit button** | User clicks "Submit" | Auto when fills complete |
| **Use case** | Individual reports, expenses | Multi-department reports |

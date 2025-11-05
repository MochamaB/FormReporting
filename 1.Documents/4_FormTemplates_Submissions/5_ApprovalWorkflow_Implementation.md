# KTDA ICT REPORTING SYSTEM
## Approval Workflow Implementation Guide

**Document Version:** 1.0
**Last Updated:** 2025-10-30
**Phase:** 2.3 - Approval Workflow
**Duration:** Week 9 (5 days)
**Implementation Order:** Fifth document - Read after Form Rendering Implementation

---

## TABLE OF CONTENTS

1. [Overview](#1-overview)
2. [UI Layout & Mockups](#2-ui-layout--mockups)
3. [User Journeys](#3-user-journeys)
4. [Component Breakdown](#4-component-breakdown)
5. [Data Flow Diagrams](#5-data-flow-diagrams)
6. [State Machine](#6-state-machine)
7. [Step-by-Step Implementation Guide](#7-step-by-step-implementation-guide)
8. [Notification Strategy](#8-notification-strategy)
9. [Approval Rules & Permissions](#9-approval-rules--permissions)
10. [Testing Checklist](#10-testing-checklist)

---

## 1. OVERVIEW

### 1.1 Purpose

The Approval Workflow allows regional managers and head office staff to review, approve, or reject checklist submissions from factory ICT staff. This ensures data quality and accountability in the reporting process.

### 1.2 Key Features

- **Pending Approvals Dashboard** - View all submissions awaiting approval
- **Submission Review Interface** - Compare current submission with previous month
- **Approve/Reject Actions** - Quick approval or rejection with comments
- **Real-time Notifications** - Email + in-app notifications via SignalR
- **Submission History** - Audit trail of all approval actions
- **Bulk Approval** - Approve multiple submissions at once (if enabled)
- **Comments Thread** - Discussion between submitter and approver

### 1.3 User Roles Involved

| Role | Permissions |
|------|-------------|
| **Factory ICT Staff** | Submit checklists, view own submissions, resubmit after rejection |
| **Regional Manager** | Approve/reject submissions from factories in their region |
| **Head Office ICT Manager** | Approve/reject all submissions, override regional decisions |
| **System Administrator** | View all approvals, configure workflow rules |

### 1.4 Workflow States

```
Draft → Submitted → Under Review → Approved
                                 ↓
                              Rejected → Draft (with comments)
```

### 1.5 Implementation Scope

**Week 9 Deliverables:**
- Pending Approvals Dashboard (Day 1-2)
- Submission Review Interface (Day 3-4)
- Approve/Reject Actions with Comments (Day 3-4)
- Email & SignalR Notifications (Day 5)
- Submission History & Audit Trail (Day 5)
- Unit & Integration Tests (ongoing)

**Out of Scope:**
- Multi-level approval chains (future enhancement)
- Approval delegation (future enhancement)
- SLA tracking (future enhancement)

---

## 2. UI LAYOUT & MOCKUPS

### 2.1 Pending Approvals Dashboard

**Route:** `/Approvals/PendingApprovals`
**Access:** Regional Managers, Head Office ICT Manager

```
┌────────────────────────────────────────────────────────────────────────┐
│ KTDA ICT Reporting System          👤 John Kamau (Regional Manager)   │
├────────────────────────────────────────────────────────────────────────┤
│ 🏠 Dashboard  📋 Reports  ✅ Approvals  ⚙️ Settings  🔔 (3)           │
└────────────────────────────────────────────────────────────────────────┘

╔════════════════════════════════════════════════════════════════════════╗
║ PENDING APPROVALS                                          🔄 Refresh  ║
╠════════════════════════════════════════════════════════════════════════╣
║                                                                        ║
║ Filter by:  [All Factories ▼]  [All Templates ▼]  [Last 30 days ▼]  ║
║                                                                        ║
║ ┌────────────┬──────────────┬────────────┬──────────┬───────────────┐ ║
║ │ Submission │ Factory      │ Template   │ Submitted│ Actions       │ ║
║ ├────────────┼──────────────┼────────────┼──────────┼───────────────┤ ║
║ │ #00145     │ Kangaita     │ Monthly    │ 2 days   │ [📄 Review]   │ ║
║ │ 📊 85% Pre │ Tea Factory  │ Checklist  │ ago      │ [✅ Approve]  │ ║
║ │            │              │            │          │ [❌ Reject]   │ ║
║ ├────────────┼──────────────┼────────────┼──────────┼───────────────┤ ║
║ │ #00144     │ Ragati       │ Monthly    │ 3 days   │ [📄 Review]   │ ║
║ │ 📊 92% Pre │ Tea Factory  │ Checklist  │ ago      │ [✅ Approve]  │ ║
║ │ ⚠️ 3 Notes │              │            │          │ [❌ Reject]   │ ║
║ ├────────────┼──────────────┼────────────┼──────────┼───────────────┤ ║
║ │ #00143     │ Tetu        │ Monthly    │ 4 days   │ [📄 Review]   │ ║
║ │ 📊 78% Pre │ Tea Factory  │ Checklist  │ ago      │ [✅ Approve]  │ ║
║ │            │              │            │          │ [❌ Reject]   │ ║
║ └────────────┴──────────────┴────────────┴──────────┴───────────────┘ ║
║                                                                        ║
║ Showing 3 of 3 pending approvals                     [1] [2] [3] →   ║
║                                                                        ║
║ 📊 Summary: 3 Pending | 12 Approved this month | 1 Rejected          ║
╚════════════════════════════════════════════════════════════════════════╝

Legend:
- 📊 85% Pre = 85% of fields were pre-filled (less manual entry)
- ⚠️ 3 Notes = Submitter added 3 explanatory notes
- Days ago = Time since submission (SLA tracking)
```

**Key UI Elements:**
- **Filter Controls** - Factory, Template, Date Range dropdowns
- **Submission Card** - Shows submission ID, factory, template, submission date
- **Quick Actions** - Review, Approve, Reject buttons
- **Visual Indicators** - Pre-fill percentage, notes count, age
- **Summary Stats** - Pending, Approved, Rejected counts

---

### 2.2 Submission Review Interface

**Route:** `/Approvals/ReviewSubmission/{submissionId}`
**Access:** Approvers only

```
┌────────────────────────────────────────────────────────────────────────┐
│ ← Back to Approvals                                   🔔 Notifications │
└────────────────────────────────────────────────────────────────────────┘

╔════════════════════════════════════════════════════════════════════════╗
║ REVIEW SUBMISSION #00145                                              ║
╠════════════════════════════════════════════════════════════════════════╣
║                                                                        ║
║ Factory: Kangaita Tea Factory                                         ║
║ Template: Monthly ICT Status Checklist                                ║
║ Submitted by: Peter Mwangi (ICT Officer)                             ║
║ Submitted on: 28 Oct 2025, 3:45 PM                                   ║
║ Reporting Period: September 2025                                      ║
║ Status: Submitted (Awaiting Approval)                                 ║
║                                                                        ║
╚════════════════════════════════════════════════════════════════════════╝

┌────────────────────────────────────────────────────────────────────────┐
│ View Mode:  ⚫ Current Submission  ⚪ Side-by-Side Comparison         │
└────────────────────────────────────────────────────────────────────────┘

╔════════════════════════════════════════════════════════════════════════╗
║ 📦 SECTION 1: HARDWARE INVENTORY                              [▼ Expand]║
╠════════════════════════════════════════════════════════════════════════╣
║                                                                        ║
║ 1. Total number of computers *                                        ║
║    ┌──────────────────────────────────────────────────────────────┐   ║
║    │ 45                                         ✓ Pre-filled      │   ║
║    └──────────────────────────────────────────────────────────────┘   ║
║    Count all desktop and laptop computers                             ║
║                                                                        ║
║ 2. Number of operational computers *                                  ║
║    ┌──────────────────────────────────────────────────────────────┐   ║
║    │ 42                                         ✓ Pre-filled      │   ║
║    └──────────────────────────────────────────────────────────────┘   ║
║    Computers in working condition                                     ║
║                                                                        ║
║ 3. Number of faulty computers *                                       ║
║    ┌──────────────────────────────────────────────────────────────┐   ║
║    │ 3                                          ✓ Pre-filled      │   ║
║    └──────────────────────────────────────────────────────────────┘   ║
║    Computers requiring repair                                         ║
║    💬 Note from submitter: "2 have motherboard issues, 1 has dead HD" ║
║                                                                        ║
╚════════════════════════════════════════════════════════════════════════╝

╔════════════════════════════════════════════════════════════════════════╗
║ 📦 SECTION 2: SOFTWARE LICENSES                               [▲ Collapse]║
╠════════════════════════════════════════════════════════════════════════╣
║ ... (more sections) ...                                               ║
╚════════════════════════════════════════════════════════════════════════╝

┌────────────────────────────────────────────────────────────────────────┐
│ APPROVAL ACTIONS                                                       │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│ [✅ Approve Submission]              [❌ Reject Submission]           │
│                                                                        │
│ Add Comments (optional for approval, required for rejection):         │
│ ┌────────────────────────────────────────────────────────────────┐     │
│ │                                                                │     │
│ │                                                                │     │
│ │                                                                │     │
│ └────────────────────────────────────────────────────────────────┘     │
│                                                                        │
│ [📧 Send copy to Head Office]  [🔔 Notify submitter immediately]     │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

**Key UI Elements:**
- **Submission Header** - Factory, template, submitter, date, status
- **View Mode Toggle** - Switch between current and comparison view
- **Sections** - Accordion display of all form sections
- **Field Display** - Shows value, pre-fill indicator, submitter notes
- **Approval Actions Panel** - Approve/Reject buttons with comments
- **Notification Options** - Email copy, immediate notification

---

### 2.3 Side-by-Side Comparison View

**Route:** `/Approvals/ReviewSubmission/{submissionId}?view=comparison`
**Access:** Approvers only

```
╔════════════════════════════════════════════════════════════════════════╗
║ COMPARISON VIEW: September 2025 vs August 2025                       ║
╠════════════════════════════════════════════════════════════════════════╣
║                                                                        ║
║ 📦 SECTION 1: HARDWARE INVENTORY                                      ║
║                                                                        ║
║ ┌──────────────────────────────────┬──────────────────────────────────┐║
║ │ CURRENT (September 2025)         │ PREVIOUS (August 2025)          │║
║ ├──────────────────────────────────┼──────────────────────────────────┤║
║ │ Total computers: 45              │ Total computers: 45             │║
║ │ ✓ No change                      │                                 │║
║ ├──────────────────────────────────┼──────────────────────────────────┤║
║ │ Operational: 42                  │ Operational: 44                 │║
║ │ ⚠️ Decreased by 2                │                                 │║
║ ├──────────────────────────────────┼──────────────────────────────────┤║
║ │ Faulty: 3                        │ Faulty: 1                       │║
║ │ ⚠️ Increased by 2                │                                 │║
║ │ 💬 Note: "2 have motherboard     │                                 │║
║ │    issues, 1 has dead HD"        │                                 │║
║ └──────────────────────────────────┴──────────────────────────────────┘║
║                                                                        ║
║ 📊 CHANGE INDICATORS:                                                  ║
║ ✓ No change (green) | ⚠️ Significant change (yellow) | ❌ Critical (red)║
║                                                                        ║
╚════════════════════════════════════════════════════════════════════════╝

╔════════════════════════════════════════════════════════════════════════╗
║ 📦 SECTION 2: SOFTWARE LICENSES                                       ║
║                                                                        ║
║ ┌──────────────────────────────────┬──────────────────────────────────┐║
║ │ CURRENT (September 2025)         │ PREVIOUS (August 2025)          │║
║ ├──────────────────────────────────┼──────────────────────────────────┤║
║ │ Active Windows licenses: 45      │ Active Windows licenses: 45     │║
║ │ ✓ No change                      │                                 │║
║ ├──────────────────────────────────┼──────────────────────────────────┤║
║ │ Antivirus coverage: 100%         │ Antivirus coverage: 100%        │║
║ │ ✓ No change                      │                                 │║
║ └──────────────────────────────────┴──────────────────────────────────┘║
║                                                                        ║
╚════════════════════════════════════════════════════════════════════════╝
```

**Change Detection Logic:**
- **No Change** (✓ Green) - Value identical to previous month
- **Minor Change** (📊 Blue) - Value changed by <10%
- **Significant Change** (⚠️ Yellow) - Value changed by 10-25%
- **Critical Change** (❌ Red) - Value changed by >25% or critical field
- **New Data** (🆕 Purple) - Field didn't exist in previous submission

**Benefits:**
- Quickly spot anomalies (e.g., sudden increase in faulty equipment)
- Validate consistency (e.g., total should equal operational + faulty)
- Identify trends (e.g., gradual decline in operational equipment)

---

### 2.4 Approve Modal

**Trigger:** Click "Approve Submission" button
**Modal Size:** Medium (600px width)

```
┌────────────────────────────────────────────────────────────────────────┐
│ ✅ APPROVE SUBMISSION #00145                                     [✕]  │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│ You are about to approve this submission:                             │
│                                                                        │
│ Factory: Kangaita Tea Factory                                         │
│ Template: Monthly ICT Status Checklist                                │
│ Submitted by: Peter Mwangi                                            │
│ Reporting Period: September 2025                                      │
│                                                                        │
│ Add approval comments (optional):                                     │
│ ┌────────────────────────────────────────────────────────────────┐     │
│ │ Good work. All data looks accurate.                            │     │
│ │                                                                │     │
│ └────────────────────────────────────────────────────────────────┘     │
│                                                                        │
│ Notifications:                                                         │
│ ☑ Email submitter (Peter Mwangi)                                     │
│ ☑ Notify Head Office ICT Manager                                     │
│ ☐ Copy Regional Director                                             │
│                                                                        │
│ ⚠️ This action cannot be undone (unless overridden by Head Office)   │
│                                                                        │
│ [Cancel]                                   [✅ Confirm Approval]      │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

**Approval Confirmation Flow:**
1. User clicks "Approve Submission" button
2. Modal opens with summary and comment box
3. User enters optional comments
4. User selects notification recipients
5. User clicks "Confirm Approval"
6. System shows loading spinner
7. System updates submission status to "Approved"
8. System sends notifications via email + SignalR
9. System logs approval action with timestamp
10. Modal closes, user redirected to Pending Approvals dashboard
11. Success toast message: "Submission #00145 approved successfully"

---

### 2.5 Reject Modal

**Trigger:** Click "Reject Submission" button
**Modal Size:** Medium (600px width)

```
┌────────────────────────────────────────────────────────────────────────┐
│ ❌ REJECT SUBMISSION #00145                                      [✕]  │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│ You are about to reject this submission:                              │
│                                                                        │
│ Factory: Kangaita Tea Factory                                         │
│ Template: Monthly ICT Status Checklist                                │
│ Submitted by: Peter Mwangi                                            │
│ Reporting Period: September 2025                                      │
│                                                                        │
│ Reason for rejection (required): *                                    │
│ ┌────────────────────────────────────────────────────────────────┐     │
│ │ Please correct the following issues:                           │     │
│ │ 1. Hardware section: Total computers (45) doesn't match        │     │
│ │    operational (42) + faulty (3). Please verify counts.        │     │
│ │ 2. Network section: Please provide explanation for 12-hour     │     │
│ │    downtime on Sept 15.                                        │     │
│ └────────────────────────────────────────────────────────────────┘     │
│ Character count: 245/1000                                              │
│                                                                        │
│ Notifications:                                                         │
│ ☑ Email submitter (Peter Mwangi) - Required                          │
│ ☑ Notify Regional Director                                           │
│ ☐ Escalate to Head Office                                            │
│                                                                        │
│ ⚠️ The submitter will be able to correct and resubmit                │
│                                                                        │
│ [Cancel]                                   [❌ Confirm Rejection]     │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

**Rejection Validation:**
- Comments field is REQUIRED (minimum 20 characters)
- Must provide clear, actionable feedback
- Cannot reject without notifying submitter
- System automatically changes submission status to "Rejected"
- Submitter can view rejection comments and resubmit

**Rejection Flow:**
1. User clicks "Reject Submission" button
2. Modal opens with required comments field
3. User enters detailed rejection reasons (min 20 chars)
4. User selects notification recipients (submitter required)
5. User clicks "Confirm Rejection"
6. System validates comments (not empty, min length)
7. System updates submission status to "Rejected"
8. System sends notifications with rejection comments
9. System logs rejection action with timestamp
10. Modal closes, user redirected to dashboard
11. Warning toast: "Submission #00145 rejected. Submitter notified."

---

### 2.6 Submission History View

**Route:** `/Approvals/SubmissionHistory/{submissionId}`
**Access:** All authenticated users (visibility varies by role)

```
╔════════════════════════════════════════════════════════════════════════╗
║ SUBMISSION HISTORY: #00145                                            ║
╠════════════════════════════════════════════════════════════════════════╣
║                                                                        ║
║ Factory: Kangaita Tea Factory                                         ║
║ Template: Monthly ICT Status Checklist                                ║
║ Reporting Period: September 2025                                      ║
║ Current Status: Approved                                               ║
║                                                                        ║
╚════════════════════════════════════════════════════════════════════════╝

┌────────────────────────────────────────────────────────────────────────┐
│ AUDIT TRAIL                                                            │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│ ✅ APPROVED                                     28 Oct 2025, 9:15 AM  │
│    By: John Kamau (Regional Manager - Nairobi Region)                │
│    Comments: "Good work. All data looks accurate."                    │
│    Duration: 30 minutes (from submission to approval)                 │
│                                                                        │
│ ────────────────────────────────────────────────────────────────────  │
│                                                                        │
│ 📤 SUBMITTED                                    28 Oct 2025, 8:45 AM  │
│    By: Peter Mwangi (ICT Officer)                                    │
│    Comments: None                                                      │
│                                                                        │
│ ────────────────────────────────────────────────────────────────────  │
│                                                                        │
│ 💾 AUTO-SAVED                                   28 Oct 2025, 8:43 AM  │
│    Progress: 100% complete (33/33 questions answered)                 │
│                                                                        │
│ ────────────────────────────────────────────────────────────────────  │
│                                                                        │
│ 💾 AUTO-SAVED                                   28 Oct 2025, 8:41 AM  │
│    Progress: 91% complete (30/33 questions answered)                  │
│                                                                        │
│ ────────────────────────────────────────────────────────────────────  │
│                                                                        │
│ 📝 DRAFT CREATED                                28 Oct 2025, 8:10 AM  │
│    By: Peter Mwangi (ICT Officer)                                    │
│    Pre-fill: 74% of fields auto-populated                             │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘

[⬇️ Export History as PDF]  [📧 Email History]  [🔙 Back to Submission]
```

**History Entry Types:**
- **Draft Created** - Initial form opened
- **Auto-saved** - Periodic saves with progress percentage
- **Submitted** - Submitter sends for approval
- **Under Review** - Approver opened submission (optional)
- **Approved** - Approved with optional comments
- **Rejected** - Rejected with required comments
- **Resubmitted** - Submitter corrected and resubmitted after rejection

---

### 2.7 Real-time Notification Badge

**Location:** Top navigation bar (visible on all pages)
**Trigger:** SignalR push notification when approval action occurs

```
┌────────────────────────────────────────────────────────────────────────┐
│ KTDA ICT Reporting System          👤 Peter Mwangi (ICT Officer)     │
├────────────────────────────────────────────────────────────────────────┤
│ 🏠 Dashboard  📋 Reports  📝 My Submissions  ⚙️ Settings  🔔 (1) ←   │
└────────────────────────────────────────────────────────────────────────┘
                                                              │
                                                              └─ Red badge
```

**Notification Dropdown:**
```
┌────────────────────────────────────────────────────────────────┐
│ NOTIFICATIONS                                       [Mark all read]│
├────────────────────────────────────────────────────────────────┤
│                                                                │
│ ✅ Submission #00145 Approved                  🕐 2 min ago   │
│    Your Monthly Checklist (Sep 2025) was approved by          │
│    John Kamau (Regional Manager).                             │
│    Comments: "Good work. All data looks accurate."            │
│    [View Submission]                                           │
│                                                                │
├────────────────────────────────────────────────────────────────┤
│ 💾 Submission #00144 Auto-saved               🕐 15 min ago   │
│    Draft saved successfully (Progress: 85%)                   │
│                                                                │
├────────────────────────────────────────────────────────────────┤
│ 📝 New Template Published                     🕐 2 hours ago  │
│    "Quarterly Network Assessment" template is now available.  │
│    [Start Checklist]                                           │
│                                                                │
└────────────────────────────────────────────────────────────────┘
[View All Notifications]
```

**SignalR Integration:**
- Real-time push when approval/rejection occurs
- Badge count updates without page refresh
- Toast notification appears in bottom-right corner
- Notification dropdown shows recent 5 notifications
- "View All Notifications" page shows full history

---

## 3. USER JOURNEYS

### 3.1 Journey 1: Happy Path - Submission Approved

**Actors:** Peter Mwangi (Factory ICT Officer), John Kamau (Regional Manager)

**Scenario:** Peter submits monthly checklist, John reviews and approves it.

**Steps:**

1. **Peter submits checklist** (28 Oct 2025, 8:45 AM)
   - Peter completes and submits "Monthly ICT Status Checklist" for September 2025
   - Submission ID #00145 created with status "Submitted"
   - System logs submission in audit trail
   - System sends email notification to John Kamau (Regional Manager)
   - Peter sees success message: "Submission sent for approval"

2. **John receives notification** (28 Oct 2025, 8:45 AM)
   - John receives email: "New submission awaiting approval from Kangaita Factory"
   - Email contains submission summary and direct link to review page
   - John's notification badge shows (1) unread notification
   - SignalR pushes real-time notification to John's browser

3. **John opens Pending Approvals dashboard** (28 Oct 2025, 9:00 AM)
   - John navigates to `/Approvals/PendingApprovals`
   - Dashboard shows 3 pending submissions
   - Submission #00145 appears at top (most recent)
   - Card shows: Factory (Kangaita), Template (Monthly Checklist), Submitted (15 min ago)
   - Pre-fill indicator: 85% of fields were auto-populated

4. **John clicks "Review" button**
   - Browser navigates to `/Approvals/ReviewSubmission/145`
   - System loads submission data with all sections collapsed
   - Header shows submission metadata (factory, template, submitter, date, period, status)
   - System updates submission status to "Under Review" (optional tracking)

5. **John expands Hardware Inventory section**
   - Section accordion opens smoothly
   - Shows 3 questions: Total computers (45), Operational (42), Faulty (3)
   - All 3 fields show "✓ Pre-filled" indicator
   - Faulty computers question has note: "2 have motherboard issues, 1 has dead HD"
   - John verifies math: 42 + 3 = 45 ✓

6. **John switches to Side-by-Side Comparison view**
   - Clicks "Side-by-Side Comparison" radio button
   - Page reloads with comparison layout
   - Hardware section shows:
     - Total computers: 45 (September) vs 45 (August) → ✓ No change
     - Operational: 42 (September) vs 44 (August) → ⚠️ Decreased by 2
     - Faulty: 3 (September) vs 1 (August) → ⚠️ Increased by 2
   - John notes the decrease in operational computers is explained by the increase in faulty ones

7. **John reviews Software Licenses section**
   - Expands Software Licenses section
   - Sees: Windows licenses (45), Antivirus coverage (100%)
   - Comparison shows no changes from previous month
   - All values consistent and reasonable

8. **John reviews remaining sections**
   - Expands Network Infrastructure section
   - Expands Support Tickets section
   - Expands Financial Summary section
   - All data appears accurate and consistent

9. **John scrolls to Approval Actions panel**
   - Panel fixed at bottom of page
   - Two buttons: "Approve Submission" (green) and "Reject Submission" (red)
   - Comments textarea empty (optional for approval)
   - Notification checkboxes: Email submitter (checked), Notify Head Office (checked)

10. **John clicks "Approve Submission" button** (28 Oct 2025, 9:15 AM)
    - Approve Modal opens
    - Modal shows submission summary
    - John enters comment: "Good work. All data looks accurate."
    - Notification checkboxes pre-selected: Email submitter (checked), Notify Head Office (checked)
    - John clicks "Confirm Approval"

11. **System processes approval**
    - Modal shows loading spinner
    - System updates submission status: "Submitted" → "Approved"
    - System updates ApprovedById, ApprovedDate, ApproverComments fields
    - System logs approval action in audit trail
    - System sends email to Peter: "Your submission #00145 has been approved"
    - System sends email to Head Office ICT Manager
    - System pushes SignalR notification to Peter's browser

12. **John sees success confirmation**
    - Modal closes automatically
    - Browser redirects to `/Approvals/PendingApprovals`
    - Toast notification appears: "Submission #00145 approved successfully"
    - Dashboard now shows 2 pending approvals (down from 3)
    - Summary stats update: "2 Pending | 13 Approved this month"

13. **Peter receives approval notification** (28 Oct 2025, 9:15 AM)
    - Peter's notification badge updates: 🔔 (1)
    - Toast notification appears in bottom-right: "Submission #00145 Approved"
    - Peter clicks notification, navigates to submission history page
    - History shows full audit trail from Draft Created → Approved
    - Peter sees John's comment: "Good work. All data looks accurate."

**Result:** Submission successfully approved in 30 minutes. Peter notified immediately. Audit trail complete.

---

### 3.2 Journey 2: Rejection Path - Submission Rejected & Resubmitted

**Actors:** Mary Wanjiku (Factory ICT Officer), John Kamau (Regional Manager)

**Scenario:** Mary submits checklist with data inconsistencies. John rejects it with comments. Mary corrects and resubmits.

**Steps:**

1. **Mary submits checklist** (29 Oct 2025, 10:00 AM)
   - Mary completes "Monthly ICT Status Checklist" for September 2025
   - Submission ID #00146 created with status "Submitted"
   - System sends notification to John Kamau (Regional Manager)

2. **John reviews submission** (29 Oct 2025, 10:30 AM)
   - John opens `/Approvals/ReviewSubmission/146`
   - Reviews Hardware Inventory section
   - **Spots inconsistency:** Total computers (50) ≠ Operational (42) + Faulty (5) = 47
   - Missing 3 computers in the calculation

3. **John reviews Network Infrastructure section**
   - Sees "Network uptime: 88%" with note "Some downtime on Sept 15"
   - Clicks on "Downtime duration" question
   - **Spots missing information:** Question left blank (no duration specified)
   - Cannot assess if downtime was acceptable without this data

4. **John decides to reject submission**
   - Scrolls to Approval Actions panel
   - Clicks "Reject Submission" button (red)
   - Reject Modal opens

5. **John enters detailed rejection comments** (29 Oct 2025, 10:35 AM)
   - John types in required comments field:
     ```
     Please correct the following issues:
     1. Hardware section: Total computers (50) doesn't match
        operational (42) + faulty (5) = 47. Missing 3 computers.
        Please verify counts and explain discrepancy.
     2. Network section: Please provide downtime duration for
        Sept 15 incident and explain cause.
     ```
   - Character count: 298/1000
   - Checkboxes: Email submitter (required, checked), Notify Regional Director (checked)
   - John clicks "Confirm Rejection"

6. **System processes rejection**
   - System validates comments (not empty, min 20 chars) ✓
   - System updates submission status: "Submitted" → "Rejected"
   - System updates RejectedById, RejectedDate, RejectionComments fields
   - System logs rejection action in audit trail
   - System sends email to Mary with rejection comments
   - System sends copy to Regional Director
   - System pushes SignalR notification to Mary's browser

7. **John sees rejection confirmation**
   - Modal closes
   - Browser redirects to `/Approvals/PendingApprovals`
   - Warning toast: "Submission #00146 rejected. Submitter notified."
   - Dashboard still shows 2 pending (other submissions)

8. **Mary receives rejection notification** (29 Oct 2025, 10:35 AM)
   - Mary's notification badge: 🔔 (1)
   - Toast notification: "Submission #00146 Rejected"
   - Mary clicks notification, navigates to submission details
   - Status shows "Rejected" in red badge
   - Rejection comments displayed prominently:
     ```
     ❌ Rejected by John Kamau (Regional Manager) on 29 Oct 2025, 10:35 AM

     Reason for rejection:
     Please correct the following issues:
     1. Hardware section: Total computers (50) doesn't match operational
        (42) + faulty (5) = 47. Missing 3 computers. Please verify counts
        and explain discrepancy.
     2. Network section: Please provide downtime duration for Sept 15
        incident and explain cause.
     ```
   - Button appears: [📝 Correct & Resubmit]

9. **Mary clicks "Correct & Resubmit" button**
   - Browser navigates to `/Checklists/EditSubmission/146`
   - Form loads with all previously entered data
   - Banner at top: "⚠️ This submission was rejected. Please review and correct the issues below."
   - Rejected sections highlighted with yellow background
   - Rejection comments shown inline next to affected questions

10. **Mary corrects Hardware Inventory section** (29 Oct 2025, 11:00 AM)
    - Opens Hardware Inventory section
    - Reviews counts: Total (50), Operational (42), Faulty (5)
    - Realizes she forgot to include "Under Maintenance" category
    - Updates counts:
      - Total computers: 50 (no change)
      - Operational computers: 42 (no change)
      - Faulty computers: 5 (no change)
      - **Adds note:** "3 computers currently under maintenance (awaiting spare parts). Total: 42 + 5 + 3 = 50 ✓"
    - Yellow highlight disappears after correction

11. **Mary corrects Network Infrastructure section**
    - Opens Network Infrastructure section
    - Finds "Downtime duration" question (previously blank)
    - Enters: "12 hours"
    - Adds note: "Power outage on Sept 15, 8 AM - 8 PM. Affected entire factory."
    - Yellow highlight disappears

12. **Mary resubmits checklist**
    - Clicks "Resubmit for Approval" button
    - Confirmation modal: "Are you sure you want to resubmit? This will send the corrected submission back to your regional manager."
    - Mary clicks "Confirm Resubmission"
    - System updates submission status: "Rejected" → "Submitted"
    - System updates ResubmittedDate field
    - System logs resubmission in audit trail
    - System sends notification to John Kamau

13. **John receives resubmission notification** (29 Oct 2025, 11:05 AM)
    - John's notification badge: 🔔 (1)
    - Email: "Submission #00146 has been corrected and resubmitted by Mary Wanjiku"
    - Toast notification: "Resubmission received from Ragati Factory"

14. **John reviews resubmission** (29 Oct 2025, 11:20 AM)
    - John opens `/Approvals/ReviewSubmission/146?highlight=changes`
    - System highlights corrected fields with blue border
    - Hardware section shows new note: "3 computers under maintenance. Total: 42 + 5 + 3 = 50 ✓"
    - Network section now shows "12 hours" with explanation note
    - John verifies corrections address all rejection comments

15. **John approves resubmission**
    - Clicks "Approve Submission"
    - Enters comment: "Thank you for the clarifications. Approved."
    - Clicks "Confirm Approval"
    - System updates status: "Submitted" → "Approved"
    - System sends approval notification to Mary

16. **Mary receives approval notification** (29 Oct 2025, 11:21 AM)
    - Mary's notification badge: 🔔 (1)
    - Toast: "Submission #00146 Approved"
    - Mary views submission history showing complete audit trail:
      ```
      ✅ Approved (29 Oct 2025, 11:20 AM) by John Kamau
      📤 Resubmitted (29 Oct 2025, 11:05 AM) by Mary Wanjiku
      📝 Draft Edited (29 Oct 2025, 11:00 AM) by Mary Wanjiku
      ❌ Rejected (29 Oct 2025, 10:35 AM) by John Kamau
      📤 Submitted (29 Oct 2025, 10:00 AM) by Mary Wanjiku
      📝 Draft Created (29 Oct 2025, 9:30 AM) by Mary Wanjiku
      ```

**Result:** Submission rejected due to data inconsistencies. Submitter corrected issues and resubmitted. Approver verified corrections and approved. Total time: 1 hour 21 minutes (including correction time).

**Learning Points:**
- Clear rejection comments help submitters correct issues quickly
- Inline highlighting of rejected sections speeds up correction process
- Audit trail provides complete visibility into approval lifecycle
- System supports iterative correction workflow

---

### 3.3 Journey 3: Bulk Approval (Optional Feature)

**Actors:** John Kamau (Regional Manager)

**Scenario:** John has 5 routine monthly checklists to approve, all from reliable factories with consistent data. He uses bulk approval to save time.

**Steps:**

1. **John opens Pending Approvals dashboard** (30 Oct 2025, 2:00 PM)
   - Dashboard shows 5 pending submissions
   - All from different factories in Nairobi region
   - All using "Monthly ICT Status Checklist" template
   - All submitted within last 2 days

2. **John enables "Bulk Selection" mode**
   - Clicks checkbox icon in top-right corner of dashboard
   - Checkboxes appear next to each submission card
   - Bulk actions toolbar appears at bottom of screen

3. **John quickly reviews each submission**
   - Opens submission #00147 in new tab, scans data → Looks good
   - Opens submission #00148 in new tab, scans data → Looks good
   - Opens submission #00149 in new tab, scans data → Looks good
   - Opens submission #00150 in new tab → **Spots issue** → Skips this one
   - Opens submission #00151 in new tab, scans data → Looks good

4. **John selects submissions for bulk approval**
   - Returns to Pending Approvals dashboard
   - Checks boxes for: #00147, #00148, #00149, #00151 (4 submissions)
   - Does NOT check #00150 (will review separately)
   - Bulk toolbar shows: "4 selected"

5. **John clicks "Bulk Approve" button**
   - Bulk Approve Modal opens
   - Modal shows list of 4 submissions:
     ```
     ✅ #00147 - Kangaita Factory
     ✅ #00148 - Ragati Factory
     ✅ #00149 - Tetu Factory
     ✅ #00151 - Kiganjo Factory
     ```
   - Add comments (optional, applies to all):
     ```
     [Bulk approval for routine monthly reports. All data verified.]
     ```
   - Notification options:
     - Email all submitters (checked)
     - Notify Head Office (checked)

6. **John confirms bulk approval** (30 Oct 2025, 2:15 PM)
   - Clicks "Approve 4 Submissions"
   - System shows progress bar: "Approving 1 of 4..."
   - System processes each approval sequentially:
     - Updates status for #00147 → Approved
     - Updates status for #00148 → Approved
     - Updates status for #00149 → Approved
     - Updates status for #00151 → Approved
   - System sends 4 individual email notifications
   - System logs 4 separate audit trail entries

7. **John sees bulk approval confirmation**
   - Modal shows success summary:
     ```
     ✅ Successfully approved 4 submissions

     #00147 - Approved
     #00148 - Approved
     #00149 - Approved
     #00151 - Approved

     All submitters have been notified.
     ```
   - Modal closes after 3 seconds
   - Dashboard refreshes automatically
   - Now shows 1 pending approval (#00150)
   - Toast notification: "4 submissions approved successfully"

8. **John reviews remaining submission**
   - Clicks "Review" on submission #00150
   - Identifies data issue in Software Licenses section
   - Rejects with detailed comments (as in Journey 2)

**Result:** Bulk approval processed 4 submissions in 15 minutes (vs 1-2 hours individually). John saved time while maintaining oversight on suspicious submission #00150.

**Business Rules for Bulk Approval:**
- Maximum 10 submissions per bulk action (prevent accidental mass approvals)
- All submissions must use same template (consistency check)
- Cannot bulk approve if any submission has validation warnings
- Bulk rejection NOT allowed (requires individual comments)
- Bulk approval comment optional but recommended
- Each submission gets individual audit trail entry (not grouped)

---

## 4. COMPONENT BREAKDOWN

### 4.1 Pending Approvals Dashboard Component

**Component Name:** `PendingApprovalsDashboard`
**Route:** `/Approvals/PendingApprovals`
**Controller:** `ApprovalsController.PendingApprovals()`
**View:** `Views/Approvals/PendingApprovals.cshtml`

**Sub-components:**
- **FilterPanel** - Factory, template, date range dropdowns
- **SubmissionCard** - Individual submission display
- **ActionButtons** - Review, Approve, Reject quick actions
- **SummaryStats** - Pending, approved, rejected counts
- **BulkActionsToolbar** - Appears when bulk mode enabled

**Data Requirements:**
- List of submissions with status "Submitted"
- Filtered by approver's region (regional managers) or all regions (head office)
- Include submission metadata: ID, factory name, template name, submitter name, submitted date
- Include computed fields: pre-fill percentage, notes count, age in days

**User Interactions:**
- Click "Review" → Navigate to review page
- Click "Approve" → Open approve modal (quick approval without review)
- Click "Reject" → Open reject modal (requires review first)
- Filter by factory → Reload dashboard with filtered data
- Filter by template → Reload dashboard with filtered data
- Filter by date range → Reload dashboard with filtered data
- Enable bulk mode → Show checkboxes on all cards
- Select submissions → Enable bulk actions toolbar

**Responsiveness:**
- Desktop (>1200px): 3-column grid layout
- Tablet (768-1200px): 2-column grid layout
- Mobile (<768px): Single column stack

---

### 4.2 Submission Review Interface Component

**Component Name:** `SubmissionReviewInterface`
**Route:** `/Approvals/ReviewSubmission/{submissionId}`
**Controller:** `ApprovalsController.ReviewSubmission(int submissionId)`
**View:** `Views/Approvals/ReviewSubmission.cshtml`

**Sub-components:**
- **SubmissionHeader** - Metadata display (factory, template, submitter, date, status)
- **ViewModeToggle** - Switch between current and comparison view
- **SectionAccordion** - Collapsible section display
- **FieldDisplay** - Individual field rendering with value, pre-fill indicator, notes
- **ApprovalActionsPanel** - Approve/reject buttons, comments textarea, notification options
- **ComparisonView** - Side-by-side current vs previous submission

**Data Requirements:**
- Current submission data (all sections, items, responses)
- Previous submission data (for comparison view)
- Submitter information (name, role, contact)
- Template metadata (name, description, version)
- Audit trail history

**User Interactions:**
- Expand/collapse sections → Toggle accordion
- Switch view mode → Reload with comparison layout
- Enter comments → Update textarea
- Select notification options → Toggle checkboxes
- Click "Approve" → Open approve modal
- Click "Reject" → Open reject modal
- Click submitter notes → Expand note popover

**Responsiveness:**
- Desktop: Side-by-side comparison (50/50 split)
- Tablet: Side-by-side comparison (50/50 split, smaller font)
- Mobile: Stacked comparison (current above, previous below)

---

### 4.3 Approve Modal Component

**Component Name:** `ApproveModal`
**Trigger:** Click "Approve Submission" button in review interface
**Implementation:** Bootstrap modal with jQuery handler

**Sub-components:**
- **SubmissionSummary** - Read-only display of key submission metadata
- **CommentsTextarea** - Optional approval comments
- **NotificationCheckboxes** - Select who to notify
- **ConfirmButton** - Triggers approval action
- **CancelButton** - Closes modal without action

**Data Requirements:**
- Submission ID
- Submission metadata (factory, template, submitter, period)
- Default notification recipients (submitter, head office)

**User Interactions:**
- Enter comments (optional) → Update textarea
- Toggle notification checkboxes → Enable/disable recipients
- Click "Confirm Approval" → Submit approval via AJAX
- Click "Cancel" → Close modal without action
- Press Escape key → Close modal without action

**Validation:**
- No validation required (comments optional)
- At least one notification recipient must be selected (submitter recommended)

**AJAX Submission:**
- **Endpoint:** `POST /Approvals/Approve`
- **Payload:**
  ```json
  {
    "submissionId": 145,
    "approverComments": "Good work. All data looks accurate.",
    "notifySubmitter": true,
    "notifyHeadOffice": true,
    "notifyRegionalDirector": false
  }
  ```
- **Success Response:**
  ```json
  {
    "success": true,
    "message": "Submission #00145 approved successfully",
    "redirectUrl": "/Approvals/PendingApprovals"
  }
  ```
- **Error Response:**
  ```json
  {
    "success": false,
    "message": "Approval failed. Submission already approved by another manager.",
    "errorCode": "ALREADY_APPROVED"
  }
  ```

**Post-approval Actions:**
- Close modal
- Redirect to Pending Approvals dashboard
- Show success toast notification
- Send email notifications asynchronously (Hangfire background job)
- Send SignalR real-time notification to submitter

---

### 4.4 Reject Modal Component

**Component Name:** `RejectModal`
**Trigger:** Click "Reject Submission" button in review interface
**Implementation:** Bootstrap modal with jQuery validation

**Sub-components:**
- **SubmissionSummary** - Read-only display of key submission metadata
- **RejectionTextarea** - REQUIRED rejection comments (min 20 chars)
- **CharacterCounter** - Live count of characters entered (e.g., "245/1000")
- **NotificationCheckboxes** - Select who to notify (submitter required)
- **ConfirmButton** - Triggers rejection action
- **CancelButton** - Closes modal without action

**Data Requirements:**
- Submission ID
- Submission metadata (factory, template, submitter, period)
- Mandatory notification recipients (submitter always notified)

**User Interactions:**
- Enter rejection reasons (required) → Update textarea, update character counter
- Toggle notification checkboxes → Enable/disable optional recipients (submitter cannot be unchecked)
- Click "Confirm Rejection" → Validate comments, submit rejection via AJAX
- Click "Cancel" → Close modal without action

**Validation:**
- **Comments required** - Must not be empty
- **Minimum length** - 20 characters minimum (provide actionable feedback)
- **Maximum length** - 1000 characters maximum (prevent essay-length comments)
- **Submitter notification** - Cannot be disabled (submitter must always be notified)

**Client-side Validation:**
```javascript
// Pseudo-code validation logic
function validateRejectionComments() {
  var comments = $('#rejectionComments').val().trim();

  if (comments.length === 0) {
    showError('Rejection comments are required');
    return false;
  }

  if (comments.length < 20) {
    showError('Please provide at least 20 characters of feedback');
    return false;
  }

  if (comments.length > 1000) {
    showError('Comments cannot exceed 1000 characters');
    return false;
  }

  return true;
}
```

**AJAX Submission:**
- **Endpoint:** `POST /Approvals/Reject`
- **Payload:**
  ```json
  {
    "submissionId": 146,
    "rejectionComments": "Please correct the following issues...",
    "notifySubmitter": true,
    "notifyRegionalDirector": true,
    "escalateToHeadOffice": false
  }
  ```
- **Success Response:**
  ```json
  {
    "success": true,
    "message": "Submission #00146 rejected. Submitter notified.",
    "redirectUrl": "/Approvals/PendingApprovals"
  }
  ```
- **Error Response:**
  ```json
  {
    "success": false,
    "message": "Rejection failed. Comments are required.",
    "errorCode": "VALIDATION_FAILED"
  }
  ```

**Post-rejection Actions:**
- Close modal
- Redirect to Pending Approvals dashboard
- Show warning toast notification
- Send email notifications with rejection comments (Hangfire)
- Send SignalR notification to submitter
- Update submission status to "Rejected"
- Allow submitter to edit and resubmit

---

### 4.5 Submission History Component

**Component Name:** `SubmissionHistoryTimeline`
**Route:** `/Approvals/SubmissionHistory/{submissionId}`
**Controller:** `ApprovalsController.SubmissionHistory(int submissionId)`
**View:** `Views/Approvals/SubmissionHistory.cshtml`

**Sub-components:**
- **SubmissionHeader** - Metadata display (factory, template, period, current status)
- **TimelineEntry** - Individual audit trail event
- **ExportButtons** - Export history as PDF or email

**Data Requirements:**
- All audit trail entries for submission (sorted by date descending)
- Entry types: Draft Created, Auto-saved, Submitted, Under Review, Approved, Rejected, Resubmitted
- Entry metadata: Actor (user who performed action), timestamp, comments, duration
- Computed fields: Time between events (e.g., "30 minutes from submission to approval")

**Timeline Entry Structure:**
```
[Icon] ACTION_TYPE                      Date & Time
       By: Actor Name (Role)
       Comments: Optional comments text
       Duration: Computed time duration (optional)
```

**User Interactions:**
- Scroll through timeline → View all historical events
- Click "Export as PDF" → Generate PDF document of audit trail
- Click "Email History" → Send audit trail to specified recipients
- Click "Back to Submission" → Navigate to submission review page

**Access Control:**
- **Submitter:** Can view own submission history
- **Approver:** Can view history of submissions they have authority over
- **Head Office:** Can view all submission histories
- **System Admin:** Can view all submission histories

---

### 4.6 Real-time Notification Component

**Component Name:** `NotificationHub` (SignalR Hub)
**Client-side Component:** `NotificationBadge`, `NotificationDropdown`
**Location:** Top navigation bar (all pages)

**SignalR Hub Methods:**
- **SendApprovalNotification** - Push notification when submission approved
- **SendRejectionNotification** - Push notification when submission rejected
- **SendResubmissionNotification** - Push notification when submission resubmitted
- **SendCommentNotification** - Push notification when comment added (future)

**Client-side JavaScript:**
```javascript
// Pseudo-code for SignalR connection
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

// Listen for approval notifications
connection.on("ReceiveApprovalNotification", function (notification) {
    updateNotificationBadge();
    showToastNotification(notification);
    updateNotificationDropdown(notification);
});

// Listen for rejection notifications
connection.on("ReceiveRejectionNotification", function (notification) {
    updateNotificationBadge();
    showToastNotification(notification, "warning");
    updateNotificationDropdown(notification);
});

connection.start();
```

**Notification Badge:**
- Red circular badge with count (e.g., 🔔 (3))
- Updates in real-time without page refresh
- Click to open notification dropdown
- Badge disappears when all notifications read

**Notification Dropdown:**
- Shows most recent 5 notifications
- Each notification: icon, title, description, timestamp, action button
- "Mark all read" button at top
- "View All Notifications" link at bottom
- Auto-refreshes when new notification received

**Toast Notification:**
- Appears in bottom-right corner
- Types: success (green), warning (yellow), info (blue), error (red)
- Auto-dismisses after 5 seconds (or click X to dismiss)
- Click notification to navigate to relevant page
- Multiple toasts stack vertically

**Email Notification:**
- Sent asynchronously via Hangfire background job
- Uses HTML email template with KTDA branding
- Contains submission summary, action taken, comments, direct link to submission
- Sent to recipients selected in approval/rejection modal

---

### 4.7 Bulk Actions Toolbar Component

**Component Name:** `BulkActionsToolbar`
**Location:** Fixed at bottom of Pending Approvals dashboard
**Visibility:** Only appears when bulk selection mode enabled

**UI Mockup:**
```
┌────────────────────────────────────────────────────────────────────────┐
│ 4 submissions selected     [✅ Approve Selected]     [Cancel Selection]│
└────────────────────────────────────────────────────────────────────────┘
```

**Sub-components:**
- **SelectionCounter** - Shows count of selected submissions
- **ApproveButton** - Opens bulk approve modal
- **CancelButton** - Deselects all and hides toolbar

**Business Rules:**
- Maximum 10 submissions per bulk action
- All selected submissions must use same template
- Cannot select submissions with validation warnings
- Bulk rejection NOT supported (requires individual comments)

**User Interactions:**
- Click "Approve Selected" → Open bulk approve modal
- Click "Cancel Selection" → Clear all selections, hide toolbar
- Select/deselect submissions → Update counter dynamically

---

## 5. DATA FLOW DIAGRAMS

### 5.1 Approval Flow

```
┌──────────────────────────────────────────────────────────────────────┐
│                         APPROVAL FLOW                                │
└──────────────────────────────────────────────────────────────────────┘

[Submitter]                [System]                    [Approver]
    │                          │                            │
    │ 1. Submit Checklist      │                            │
    ├─────────────────────────>│                            │
    │                          │ Update status: Submitted   │
    │                          │ Log audit trail            │
    │                          │ 2. Send email notification │
    │                          ├───────────────────────────>│
    │                          │ 3. Push SignalR notification
    │                          ├───────────────────────────>│
    │                          │                            │
    │                          │  4. Open review page       │
    │                          │<───────────────────────────┤
    │                          │ Load submission data       │
    │                          ├───────────────────────────>│
    │                          │                            │
    │                          │  5. Click "Approve"        │
    │                          │<───────────────────────────┤
    │                          │ Open approve modal         │
    │                          ├───────────────────────────>│
    │                          │                            │
    │                          │  6. Confirm approval       │
    │                          │<───────────────────────────┤
    │                          │ Update status: Approved    │
    │                          │ Log audit trail            │
    │                          │ Queue email job (Hangfire) │
    │ 7. Receive email         │                            │
    │<─────────────────────────┤                            │
    │ 8. Receive SignalR push  │                            │
    │<─────────────────────────┤                            │
    │                          │ 9. Success response        │
    │                          ├───────────────────────────>│
    │                          │ Redirect to dashboard      │
    │                          │                            │
```

**Key Steps:**
1. **Submission** - Submitter completes and submits checklist
2. **Email Notification** - System sends email to approver
3. **Real-time Notification** - System pushes SignalR notification to approver's browser
4. **Review** - Approver opens submission review page
5. **Approve** - Approver clicks "Approve" button, modal opens
6. **Confirm** - Approver confirms approval with optional comments
7. **Email to Submitter** - System sends approval email to submitter
8. **Real-time to Submitter** - System pushes SignalR notification to submitter's browser
9. **Success** - Approver sees success message and dashboard updates

---

### 5.2 Rejection & Resubmission Flow

```
┌──────────────────────────────────────────────────────────────────────┐
│                  REJECTION & RESUBMISSION FLOW                       │
└──────────────────────────────────────────────────────────────────────┘

[Submitter]                [System]                    [Approver]
    │                          │                            │
    │ 1. Submit Checklist      │                            │
    ├─────────────────────────>│                            │
    │                          │  2. Review submission      │
    │                          │<───────────────────────────┤
    │                          │  3. Spot issues            │
    │                          │                            │
    │                          │  4. Click "Reject"         │
    │                          │<───────────────────────────┤
    │                          │ Open reject modal          │
    │                          ├───────────────────────────>│
    │                          │  5. Enter comments (20+)   │
    │                          │<───────────────────────────┤
    │                          │ Validate comments          │
    │                          │ Update status: Rejected    │
    │                          │ Log audit trail            │
    │ 6. Receive rejection     │                            │
    │<─────────────────────────┤                            │
    │ Read rejection comments  │                            │
    │                          │                            │
    │ 7. Click "Correct &      │                            │
    │    Resubmit"             │                            │
    ├─────────────────────────>│                            │
    │                          │ Load draft with data       │
    │                          ├───────────────────────────>│
    │ 8. Correct issues        │                            │
    ├─────────────────────────>│                            │
    │                          │ Auto-save changes          │
    │                          │                            │
    │ 9. Click "Resubmit"      │                            │
    ├─────────────────────────>│                            │
    │                          │ Update status: Submitted   │
    │                          │ Log resubmission           │
    │                          │ 10. Send notification      │
    │                          ├───────────────────────────>│
    │                          │  11. Review again          │
    │                          │<───────────────────────────┤
    │                          │ Highlight corrected fields │
    │                          ├───────────────────────────>│
    │                          │  12. Approve               │
    │                          │<───────────────────────────┤
    │ 13. Receive approval     │                            │
    │<─────────────────────────┤                            │
    │                          │                            │
```

**Key Steps:**
1. **Submission** - Submitter submits checklist
2-3. **Review & Issues** - Approver reviews and spots data issues
4-5. **Rejection** - Approver rejects with detailed comments (min 20 chars)
6. **Notification** - Submitter receives rejection with comments
7. **Edit** - Submitter clicks "Correct & Resubmit", draft loads with data
8. **Corrections** - Submitter fixes identified issues
9. **Resubmission** - Submitter resubmits corrected checklist
10. **Notification** - Approver receives resubmission notification
11. **Review** - Approver reviews again (corrected fields highlighted)
12. **Approval** - Approver verifies corrections and approves
13. **Final Notification** - Submitter receives approval

---

### 5.3 Notification Flow (Email + SignalR)

```
┌──────────────────────────────────────────────────────────────────────┐
│                      NOTIFICATION FLOW                               │
└──────────────────────────────────────────────────────────────────────┘

[Approval Action]           [Backend]                  [Recipients]
    │                          │                            │
    │ Approval/Rejection       │                            │
    │ confirmed                │                            │
    ├─────────────────────────>│                            │
    │                          │ 1. Update database         │
    │                          │    (status, comments)      │
    │                          │                            │
    │                          │ 2. Log audit trail         │
    │                          │                            │
    │                          │ 3. Queue email job         │
    │                          │    (Hangfire)              │
    │                          │    ├─ To: Submitter       │
    │                          │    ├─ CC: Head Office     │
    │                          │    └─ Subject: Approval   │
    │                          │                            │
    │                          │ 4. Push SignalR notification
    │                          ├───────────────────────────>│
    │                          │    Real-time browser push  │
    │                          │                            │
    │                          │ 5. Return success response │
    │<─────────────────────────┤                            │
    │                          │                            │
    │                          │ [Background Job Execution] │
    │                          │ 6. Hangfire worker picks   │
    │                          │    up email job            │
    │                          │                            │
    │                          │ 7. Generate email HTML     │
    │                          │    (template + data)       │
    │                          │                            │
    │                          │ 8. Send via SMTP           │
    │                          ├───────────────────────────>│
    │                          │    Email delivered         │
    │                          │                            │
    │                          │ 9. Update notification     │
    │                          │    read status when opened │
    │                          │                            │
```

**Email Template Structure:**
```
Subject: [KTDA] Submission #00145 Approved

Dear Peter Mwangi,

Your submission has been APPROVED by John Kamau (Regional Manager).

Submission Details:
- Submission ID: #00145
- Factory: Kangaita Tea Factory
- Template: Monthly ICT Status Checklist
- Reporting Period: September 2025
- Approved on: 28 Oct 2025, 9:15 AM

Approver Comments:
"Good work. All data looks accurate."

You can view the full submission history here:
[View Submission]

---
KTDA ICT Reporting System
This is an automated email. Please do not reply.
```

**SignalR Payload:**
```json
{
  "notificationType": "approval",
  "submissionId": 145,
  "title": "Submission #00145 Approved",
  "message": "Your Monthly Checklist (Sep 2025) was approved by John Kamau.",
  "approverName": "John Kamau",
  "approverComments": "Good work. All data looks accurate.",
  "timestamp": "2025-10-28T09:15:00Z",
  "actionUrl": "/Checklists/ViewSubmission/145",
  "icon": "success"
}
```

---

### 5.4 Bulk Approval Flow

```
┌──────────────────────────────────────────────────────────────────────┐
│                      BULK APPROVAL FLOW                              │
└──────────────────────────────────────────────────────────────────────┘

[Approver]                 [System]                    [Submitters]
    │                          │                            │
    │ 1. Select 4 submissions  │                            │
    ├─────────────────────────>│                            │
    │                          │ Validate selection:        │
    │                          │ - Max 10 submissions       │
    │                          │ - Same template            │
    │                          │ - No validation warnings   │
    │                          │                            │
    │ 2. Click "Bulk Approve"  │                            │
    ├─────────────────────────>│                            │
    │                          │ Open bulk approve modal    │
    │                          ├───────────────────────────>│
    │                          │                            │
    │ 3. Enter comments        │                            │
    │    (optional, applies    │                            │
    │    to all)               │                            │
    ├─────────────────────────>│                            │
    │                          │                            │
    │ 4. Confirm bulk approval │                            │
    ├─────────────────────────>│                            │
    │                          │ For each submission:       │
    │                          │   - Update status          │
    │                          │   - Log audit trail        │
    │                          │   - Queue email job        │
    │                          │   - Push SignalR           │
    │                          │                            │
    │                          │ 5. Send 4 notifications    │
    │                          ├───────────────────────────>│
    │                          │    (individual emails)     │
    │                          │                            │
    │                          │ 6. Return summary          │
    │<─────────────────────────┤                            │
    │ Show success toast:      │                            │
    │ "4 submissions approved" │                            │
    │                          │                            │
```

**Bulk Approval Validation:**
```javascript
// Pseudo-code for bulk approval validation
function validateBulkSelection(selectedSubmissions) {
  // Rule 1: Maximum 10 submissions
  if (selectedSubmissions.length > 10) {
    return { valid: false, error: "Cannot approve more than 10 submissions at once" };
  }

  // Rule 2: All submissions must use same template
  var templates = selectedSubmissions.map(s => s.templateId);
  if (new Set(templates).size > 1) {
    return { valid: false, error: "All submissions must use the same template" };
  }

  // Rule 3: No validation warnings
  var hasWarnings = selectedSubmissions.some(s => s.hasValidationWarnings);
  if (hasWarnings) {
    return { valid: false, error: "Cannot bulk approve submissions with validation warnings" };
  }

  return { valid: true };
}
```

---

## 6. STATE MACHINE

### 6.1 Submission Status State Machine

```
┌─────────────────────────────────────────────────────────────────────┐
│                    SUBMISSION STATUS STATES                         │
└─────────────────────────────────────────────────────────────────────┘

                    ┌──────────┐
                    │  Draft   │ (Initial state)
                    └────┬─────┘
                         │
                         │ Submitter clicks "Submit"
                         ▼
                    ┌──────────┐
                    │Submitted │ (Awaiting approval)
                    └────┬─────┘
                         │
                         │ Approver opens submission
                         ▼
                ┌────────────────┐
                │ Under Review   │ (Optional tracking state)
                └────────┬───────┘
                         │
            ┌────────────┴────────────┐
            │                         │
            │ Approve                 │ Reject
            ▼                         ▼
      ┌──────────┐              ┌──────────┐
      │ Approved │              │ Rejected │
      └──────────┘              └────┬─────┘
                                     │
                                     │ Submitter corrects & resubmits
                                     ▼
                                ┌──────────┐
                                │Submitted │ (Back to awaiting approval)
                                └────┬─────┘
                                     │
                                     │ Approver reviews again
                                     ▼
                                ┌──────────┐
                                │ Approved │ (Final state)
                                └──────────┘
```

**Status Definitions:**

| Status | Description | Allowed Actions | Next States |
|--------|-------------|-----------------|-------------|
| **Draft** | Form in progress, not submitted | Edit, Auto-save, Submit | Submitted |
| **Submitted** | Awaiting approval from regional manager | View (read-only), Approve, Reject | Under Review, Approved, Rejected |
| **Under Review** | Approver opened submission (optional) | Approve, Reject | Approved, Rejected |
| **Approved** | Approved by authorized approver | View (read-only) | None (final state) |
| **Rejected** | Rejected by approver with comments | Edit, Resubmit | Submitted |

**Transition Rules:**

1. **Draft → Submitted**
   - Trigger: Submitter clicks "Submit for Approval"
   - Validation: All required fields completed, no validation errors
   - Side effects: Send notification to approver, log audit trail

2. **Submitted → Under Review**
   - Trigger: Approver opens submission review page
   - Validation: Approver has authority to review this submission
   - Side effects: Update last viewed timestamp (optional)

3. **Under Review → Approved**
   - Trigger: Approver confirms approval
   - Validation: Approver has authority, submission not already approved
   - Side effects: Send notification to submitter, log audit trail, update ApprovedDate

4. **Under Review → Rejected**
   - Trigger: Approver confirms rejection with comments
   - Validation: Rejection comments provided (min 20 chars)
   - Side effects: Send notification to submitter with comments, log audit trail

5. **Rejected → Submitted**
   - Trigger: Submitter corrects issues and clicks "Resubmit"
   - Validation: At least one field modified since rejection
   - Side effects: Send notification to approver, log resubmission, reset RejectionComments

6. **Submitted → Approved** (after resubmission)
   - Same as transition #3

**Forbidden Transitions:**
- **Draft → Approved** - Cannot approve without submission
- **Approved → Rejected** - Cannot reject approved submission (requires Head Office override)
- **Approved → Draft** - Cannot revert approved submission to draft
- **Submitted → Draft** - Cannot revert submitted to draft (requires rejection first)

---

### 6.2 Approval Authority Matrix

**Who can approve what?**

| Approver Role | Can Approve | Notes |
|---------------|-------------|-------|
| **Factory ICT Officer** | Nothing | Can only submit |
| **Regional Manager** | Factories in their region | Cannot approve own submissions |
| **Head Office ICT Manager** | All factories | Can override regional decisions |
| **System Administrator** | Nothing (by default) | Can be granted approval rights |

**Database Implementation:**
```
ChecklistSubmissions table:
- SubmittedByUserId (FK to Users)
- TenantId (FK to Tenants - identifies factory)

Users table:
- UserId
- RoleId (FK to Roles)
- RegionId (FK to Regions - for regional managers)

Approval Logic:
- Regional Manager can approve if: Submission.Tenant.RegionId == User.RegionId
- Head Office can approve if: User.RoleId == 'HeadOfficeICTManager'
- Cannot approve own submissions: Submission.SubmittedByUserId != User.UserId
```

---

## 7. STEP-BY-STEP IMPLEMENTATION GUIDE

### Day 1: Pending Approvals Dashboard (Backend)

**Duration:** 8 hours
**Developer:** Senior Developer A

**Tasks:**

1. **Create ApprovalsController** (1 hour)
   - Create `Controllers/ApprovalsController.cs`
   - Add authorization attributes (restrict to approvers)
   - Add constructor with dependency injection (IChecklistService, INotificationService)

2. **Implement PendingApprovals action** (2 hours)
   - Query submissions with status "Submitted"
   - Filter by approver's authority (region or all)
   - Calculate computed fields:
     - Pre-fill percentage (count pre-filled responses / total responses)
     - Notes count (count non-empty ResponseNotes)
     - Age in days (DateTime.Now - SubmittedDate)
   - Include related data: Factory, Template, Submitter
   - Return ViewModel with submissions list

3. **Create PendingApprovalsViewModel** (1 hour)
   - Create `ViewModels/Approvals/PendingApprovalsViewModel.cs`
   - Properties:
     - List<SubmissionSummaryDto> Submissions
     - int PendingCount
     - int ApprovedThisMonthCount
     - int RejectedThisMonthCount
     - List<SelectListItem> FactoryFilter
     - List<SelectListItem> TemplateFilter

4. **Implement filter actions** (2 hours)
   - Add `PendingApprovals(int? factoryId, int? templateId, DateRange? dateRange)` overload
   - Apply filters to query
   - Return filtered results
   - Maintain filter state in ViewBag

5. **Add unit tests** (2 hours)
   - Test: PendingApprovals returns only submitted submissions
   - Test: Regional manager sees only their region's submissions
   - Test: Head office sees all submissions
   - Test: Filters work correctly
   - Test: Computed fields calculate correctly

**Deliverable:** Backend API for Pending Approvals dashboard with filtering

---

### Day 2: Pending Approvals Dashboard (Frontend)

**Duration:** 8 hours
**Developer:** Senior Developer B

**Tasks:**

1. **Create PendingApprovals view** (2 hours)
   - Create `Views/Approvals/PendingApprovals.cshtml`
   - Add page header with title and refresh button
   - Add filter panel with 3 dropdowns (factory, template, date range)
   - Add summary stats display

2. **Create submission card partial** (2 hours)
   - Create `Views/Shared/_SubmissionCard.cshtml`
   - Display submission ID, factory name, template name, submitted date
   - Display pre-fill percentage badge
   - Display notes count badge (if > 0)
   - Add 3 action buttons: Review, Approve (quick), Reject (quick)
   - Style with Bootstrap cards

3. **Implement filter JavaScript** (1 hour)
   - Add change handlers for filter dropdowns
   - Submit form on filter change (or use AJAX)
   - Show loading spinner during filter operation
   - Update URL parameters to maintain filter state

4. **Implement quick approve/reject** (2 hours)
   - Add click handlers for "Approve" and "Reject" buttons on cards
   - Open modals without navigating to review page
   - Load submission summary via AJAX
   - Wire up to approval/rejection logic (implemented Day 3)

5. **Responsive styling** (1 hour)
   - Desktop: 3-column grid
   - Tablet: 2-column grid
   - Mobile: Single column stack
   - Test on multiple screen sizes

**Deliverable:** Fully functional Pending Approvals dashboard UI

---

### Day 3: Review Interface & Approve/Reject Actions

**Duration:** 8 hours
**Developer:** Senior Developer A

**Tasks:**

1. **Create ReviewSubmission action** (2 hours)
   - Create `ApprovalsController.ReviewSubmission(int submissionId)` action
   - Query submission with all sections, items, responses
   - Check approver authority
   - Optionally update status to "Under Review"
   - Return ReviewSubmissionViewModel

2. **Create ReviewSubmissionViewModel** (1 hour)
   - Properties:
     - SubmissionDto Submission
     - List<SectionDto> Sections (with Items and Responses)
     - PreviousSubmissionDto PreviousSubmission (for comparison)
     - bool CanApprove (authority check)
     - bool CanReject (authority check)

3. **Implement Approve action** (2 hours)
   - Create `[HttpPost] Approve(ApproveRequestDto request)` action
   - Validate approver authority
   - Validate submission is in "Submitted" status
   - Update status to "Approved"
   - Update ApprovedById, ApprovedDate, ApproverComments
   - Log audit trail
   - Queue email notification (Hangfire)
   - Send SignalR notification
   - Return success response

4. **Implement Reject action** (2 hours)
   - Create `[HttpPost] Reject(RejectRequestDto request)` action
   - Validate rejection comments (required, min 20 chars)
   - Validate approver authority
   - Update status to "Rejected"
   - Update RejectedById, RejectedDate, RejectionComments
   - Log audit trail
   - Queue email notification with comments
   - Send SignalR notification
   - Return success response

5. **Add unit tests** (1 hour)
   - Test: Approve updates status and sends notifications
   - Test: Reject validates comments
   - Test: Cannot approve/reject without authority
   - Test: Cannot approve already approved submission

**Deliverable:** Backend API for review, approve, and reject actions

---

### Day 4: Review Interface (Frontend) & Comparison View

**Duration:** 8 hours
**Developer:** Senior Developer B

**Tasks:**

1. **Create ReviewSubmission view** (2 hours)
   - Create `Views/Approvals/ReviewSubmission.cshtml`
   - Display submission header with metadata
   - Add view mode toggle (current vs comparison)
   - Render sections using accordion (Bootstrap collapse)
   - Render fields using FieldRenderer (reuse from Form Rendering)

2. **Implement comparison view** (3 hours)
   - Add `ReviewSubmission(int submissionId, string view)` action overload
   - Query previous submission for same template and factory
   - Calculate change indicators:
     - No change (value identical)
     - Minor change (<10%)
     - Significant change (10-25%)
     - Critical change (>25%)
   - Create side-by-side layout (2-column grid)
   - Add color coding: green (no change), yellow (significant), red (critical)

3. **Create approval actions panel** (2 hours)
   - Fixed panel at bottom of page
   - Approve button (green) → Open approve modal
   - Reject button (red) → Open reject modal
   - Comments textarea (optional for approve, required for reject)
   - Notification checkboxes (who to notify)

4. **Implement approve/reject modals** (1 hour)
   - Create approve modal with Bootstrap
   - Create reject modal with Bootstrap
   - Wire up to backend actions (AJAX POST)
   - Show loading spinner during submission
   - Handle success/error responses
   - Redirect to dashboard on success

**Deliverable:** Fully functional review interface with comparison view

---

### Day 5: Notifications, History & Testing

**Duration:** 8 hours
**Developers:** Senior Developer A + B

**Tasks:**

**Senior Developer A (4 hours):**

1. **Implement SignalR NotificationHub** (2 hours)
   - Create `Hubs/NotificationHub.cs`
   - Add methods: SendApprovalNotification, SendRejectionNotification
   - Add connection management (track connected users)
   - Configure SignalR in `Program.cs`

2. **Implement email notifications** (2 hours)
   - Create approval email template (HTML)
   - Create rejection email template (HTML)
   - Implement email sending service (SMTP)
   - Create Hangfire background jobs for email sending
   - Test email delivery

**Senior Developer B (4 hours):**

3. **Implement notification badge** (1 hour)
   - Add notification badge to top nav (all pages)
   - Implement SignalR client-side connection
   - Update badge count when notification received
   - Show toast notification on new notification

4. **Implement notification dropdown** (1 hour)
   - Create notification dropdown component
   - Load recent 5 notifications via AJAX
   - Display notification icon, title, description, timestamp
   - Add "Mark all read" functionality
   - Link to "View All Notifications" page

5. **Create submission history page** (2 hours)
   - Create `SubmissionHistory(int submissionId)` action
   - Query all audit trail entries for submission
   - Create timeline view (vertical timeline with icons)
   - Add export buttons (PDF, email)
   - Test with various submission states

**Both Developers (4 hours):**

6. **Integration testing** (2 hours)
   - Test full approval flow end-to-end
   - Test full rejection & resubmission flow
   - Test notifications (email + SignalR)
   - Test comparison view accuracy
   - Test bulk approval (if implemented)

7. **Bug fixes & polish** (2 hours)
   - Fix any issues found during testing
   - Improve UI/UX based on testing feedback
   - Add loading states and error handling
   - Optimize database queries

**Deliverable:** Complete approval workflow with notifications and history

---

## 8. NOTIFICATION STRATEGY

### 8.1 Email Notifications

**When to send emails:**
- Submission approved (to submitter)
- Submission rejected (to submitter, with comments)
- Submission resubmitted (to approver)
- New submission awaiting approval (to approver)

**Email Configuration:**
- **SMTP Server:** Configure in `appsettings.json`
- **From Address:** noreply@ktda.co.ke
- **Template Engine:** Razor (for HTML emails)
- **Delivery:** Asynchronous via Hangfire background jobs
- **Retry Logic:** 3 retries with exponential backoff

**Email Template Structure:**
```html
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        .header { background-color: #2c5f2d; color: white; padding: 20px; }
        .content { padding: 20px; }
        .footer { background-color: #f5f5f5; padding: 10px; text-align: center; }
        .button { background-color: #2c5f2d; color: white; padding: 10px 20px; text-decoration: none; }
    </style>
</head>
<body>
    <div class="header">
        <h2>KTDA ICT Reporting System</h2>
    </div>
    <div class="content">
        <h3>@Model.Title</h3>
        <p>Dear @Model.RecipientName,</p>
        <p>@Model.MessageBody</p>

        <h4>Submission Details:</h4>
        <ul>
            <li><strong>Submission ID:</strong> @Model.SubmissionId</li>
            <li><strong>Factory:</strong> @Model.FactoryName</li>
            <li><strong>Template:</strong> @Model.TemplateName</li>
            <li><strong>Reporting Period:</strong> @Model.ReportingPeriod</li>
        </ul>

        @if (!string.IsNullOrEmpty(Model.Comments))
        {
            <h4>Comments:</h4>
            <p>@Model.Comments</p>
        }

        <p>
            <a href="@Model.ActionUrl" class="button">View Submission</a>
        </p>
    </div>
    <div class="footer">
        <p>This is an automated email. Please do not reply.</p>
        <p>&copy; 2025 Kenya Tea Development Agency (KTDA)</p>
    </div>
</body>
</html>
```

**Hangfire Job Implementation:**
```csharp
// Pseudo-code for background email job
public class EmailNotificationJob
{
    public async Task SendApprovalEmail(int submissionId, int recipientUserId)
    {
        var submission = await _db.GetSubmission(submissionId);
        var recipient = await _db.GetUser(recipientUserId);

        var model = new EmailViewModel
        {
            Title = $"Submission #{submission.Id} Approved",
            RecipientName = recipient.FullName,
            MessageBody = $"Your submission has been approved by {submission.ApprovedBy.FullName}.",
            SubmissionId = submission.DisplayId,
            FactoryName = submission.Factory.Name,
            TemplateName = submission.Template.Name,
            ReportingPeriod = submission.ReportingPeriod,
            Comments = submission.ApproverComments,
            ActionUrl = $"https://reporting.ktda.co.ke/Checklists/ViewSubmission/{submissionId}"
        };

        var html = await _razorEngine.RenderTemplate("ApprovalEmail", model);

        await _emailService.SendAsync(
            to: recipient.Email,
            subject: $"[KTDA] Submission #{submission.DisplayId} Approved",
            htmlBody: html
        );
    }
}
```

---

### 8.2 SignalR Real-time Notifications

**SignalR Hub Implementation:**
```csharp
// Pseudo-code for SignalR hub
public class NotificationHub : Hub
{
    // Send notification to specific user
    public async Task SendToUser(int userId, NotificationDto notification)
    {
        await Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
    }

    // Send notification to all users in a role
    public async Task SendToRole(string roleName, NotificationDto notification)
    {
        await Clients.Group(roleName).SendAsync("ReceiveNotification", notification);
    }

    // User joins their user-specific group on connect
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);

        var roles = Context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        foreach (var role in roles)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, role);
        }

        await base.OnConnectedAsync();
    }
}
```

**Client-side JavaScript:**
```javascript
// Pseudo-code for SignalR client
$(document).ready(function() {
    // Establish connection
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    // Listen for notifications
    connection.on("ReceiveNotification", function(notification) {
        console.log("Received notification:", notification);

        // Update badge count
        updateNotificationBadge();

        // Show toast notification
        showToast(notification);

        // Update notification dropdown
        addToNotificationList(notification);
    });

    // Start connection
    connection.start()
        .then(() => console.log("SignalR connected"))
        .catch(err => console.error("SignalR connection failed:", err));

    // Reconnect on disconnect
    connection.onclose(() => {
        console.log("SignalR disconnected, attempting to reconnect...");
        setTimeout(() => connection.start(), 5000);
    });
});

function showToast(notification) {
    var toastHtml = `
        <div class="toast" role="alert">
            <div class="toast-header">
                <i class="${notification.icon}"></i>
                <strong class="me-auto">${notification.title}</strong>
                <small>${notification.timeAgo}</small>
                <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">
                ${notification.message}
                <br>
                <a href="${notification.actionUrl}">View Details</a>
            </div>
        </div>
    `;

    $('#toastContainer').append(toastHtml);
    $('.toast').toast('show');

    // Auto-dismiss after 5 seconds
    setTimeout(() => $('.toast').toast('hide'), 5000);
}
```

**Notification Types:**
- **approval** - Submission approved (icon: ✅, color: green)
- **rejection** - Submission rejected (icon: ❌, color: red)
- **resubmission** - Submission resubmitted after correction (icon: 📤, color: blue)
- **comment** - New comment added (icon: 💬, color: yellow)
- **reminder** - Approval pending for X days (icon: ⏰, color: orange)

---

### 8.3 In-app Notifications Page

**Route:** `/Notifications/Index`
**Purpose:** View all historical notifications (email + in-app)

**UI Mockup:**
```
╔════════════════════════════════════════════════════════════════════════╗
║ ALL NOTIFICATIONS                                    [Mark all as read]║
╠════════════════════════════════════════════════════════════════════════╣
║                                                                        ║
║ Filter: [All Types ▼]  [Last 30 days ▼]         🔍 [Search...]       ║
║                                                                        ║
║ ┌──────────────────────────────────────────────────────────────────┐   ║
║ │ ✅ Submission #00145 Approved             🕐 2 hours ago  [Unread]│   ║
║ │    Your Monthly Checklist (Sep 2025) was approved by John Kamau  │   ║
║ │    Comments: "Good work. All data looks accurate."               │   ║
║ │    [View Submission] [Mark as Read]                              │   ║
║ └──────────────────────────────────────────────────────────────────┘   ║
║                                                                        ║
║ ┌──────────────────────────────────────────────────────────────────┐   ║
║ │ 📤 New submission awaiting approval       🕐 5 hours ago   [Read] │   ║
║ │    Mary Wanjiku submitted Monthly Checklist (Sep 2025)           │   ║
║ │    [Review Submission]                                            │   ║
║ └──────────────────────────────────────────────────────────────────┘   ║
║                                                                        ║
║ ┌──────────────────────────────────────────────────────────────────┐   ║
║ │ ❌ Submission #00143 Rejected             🕐 1 day ago     [Read] │   ║
║ │    Your submission was rejected by John Kamau                     │   ║
║ │    Reason: "Please correct hardware count discrepancy"            │   ║
║ │    [View Details] [Correct & Resubmit]                           │   ║
║ └──────────────────────────────────────────────────────────────────┘   ║
║                                                                        ║
║ Showing 3 of 47 notifications                     [1] [2] [3] ... →  ║
╚════════════════════════════════════════════════════════════════════════╝
```

**Features:**
- Filter by type (approval, rejection, resubmission, etc.)
- Filter by date range
- Search by submission ID or factory name
- Mark as read/unread
- Mark all as read
- Pagination (20 per page)
- Auto-refresh every 60 seconds (check for new notifications)

---

## 9. APPROVAL RULES & PERMISSIONS

### 9.1 Role-Based Approval Authority

**Authority Matrix:**

| Role | Submissions They Can Approve | Restrictions |
|------|------------------------------|--------------|
| **Factory ICT Officer** | None | Can only submit |
| **Regional Manager** | All factories in their region | Cannot approve own factory, cannot approve own submissions |
| **Head Office ICT Manager** | All factories (all regions) | Can override regional decisions, cannot approve own submissions |
| **System Administrator** | None (by default) | Can be granted approval rights via role assignment |

**Database Implementation:**
```sql
-- Check if user can approve submission
CREATE FUNCTION dbo.CanApproveSubmission(@UserId INT, @SubmissionId INT)
RETURNS BIT
AS
BEGIN
    DECLARE @CanApprove BIT = 0;
    DECLARE @UserRole NVARCHAR(50);
    DECLARE @UserRegionId INT;
    DECLARE @SubmissionTenantId INT;
    DECLARE @SubmissionRegionId INT;
    DECLARE @SubmittedByUserId INT;

    -- Get user info
    SELECT @UserRole = r.RoleName, @UserRegionId = u.RegionId
    FROM Users u
    JOIN Roles r ON u.RoleId = r.RoleId
    WHERE u.UserId = @UserId;

    -- Get submission info
    SELECT @SubmissionTenantId = s.TenantId, @SubmittedByUserId = s.SubmittedByUserId
    FROM ChecklistSubmissions s
    WHERE s.SubmissionId = @SubmissionId;

    SELECT @SubmissionRegionId = t.RegionId
    FROM Tenants t
    WHERE t.TenantId = @SubmissionTenantId;

    -- Rule 1: Cannot approve own submissions
    IF @SubmittedByUserId = @UserId
        RETURN 0;

    -- Rule 2: Head Office can approve all
    IF @UserRole = 'HeadOfficeICTManager'
        SET @CanApprove = 1;

    -- Rule 3: Regional Manager can approve if same region
    IF @UserRole = 'RegionalManager' AND @UserRegionId = @SubmissionRegionId
        SET @CanApprove = 1;

    RETURN @CanApprove;
END;
GO
```

**Backend Authorization Check:**
```csharp
// Pseudo-code for authorization check
public async Task<Result> ApproveSubmission(int submissionId, int approverId)
{
    var submission = await _db.Submissions.FindAsync(submissionId);
    var approver = await _db.Users.FindAsync(approverId);

    // Rule 1: Cannot approve own submissions
    if (submission.SubmittedByUserId == approverId)
    {
        return Result.Fail("You cannot approve your own submissions");
    }

    // Rule 2: Check role-based authority
    if (approver.RoleName == "HeadOfficeICTManager")
    {
        // Head Office can approve all
        return Result.Ok();
    }
    else if (approver.RoleName == "RegionalManager")
    {
        // Regional Manager can only approve their region
        if (approver.RegionId != submission.Tenant.RegionId)
        {
            return Result.Fail("You can only approve submissions from factories in your region");
        }
        return Result.Ok();
    }
    else
    {
        return Result.Fail("You do not have permission to approve submissions");
    }
}
```

---

### 9.2 Approval Workflow Rules

**Business Rules:**

1. **Single Approver Required**
   - Each submission requires exactly 1 approval
   - No multi-level approval chains (future enhancement)
   - First approval finalizes the submission

2. **Rejection Can Be Corrected**
   - Rejected submissions can be edited and resubmitted
   - Resubmission goes back to same approver
   - No limit on resubmission attempts

3. **Approval is Final**
   - Once approved, submission cannot be edited by submitter
   - Only Head Office can override/reverse approval (future enhancement)
   - Approved submissions used for reporting

4. **SLA Tracking (Future Enhancement)**
   - Target: 2 business days from submission to approval
   - Reminder emails sent if pending > 2 days
   - Escalation to Head Office if pending > 5 days

5. **Bulk Approval Restrictions**
   - Maximum 10 submissions per bulk action
   - All must use same template
   - No validation warnings allowed
   - Cannot bulk reject (requires individual comments)

6. **Conflict Resolution**
   - Cannot approve submission that's already approved by another manager
   - Optimistic locking: Check ApprovedDate before approving
   - Show error if concurrent approval detected

---

## 10. TESTING CHECKLIST

### 10.1 Unit Tests

**Controller Tests:**
- [ ] `PendingApprovals_ReturnsOnlySubmittedSubmissions`
- [ ] `PendingApprovals_RegionalManagerSeesOnlyTheirRegion`
- [ ] `PendingApprovals_HeadOfficeSeesAllSubmissions`
- [ ] `PendingApprovals_FiltersWorkCorrectly`
- [ ] `ReviewSubmission_LoadsSubmissionData`
- [ ] `ReviewSubmission_LoadsPreviousSubmissionForComparison`
- [ ] `ReviewSubmission_ReturnsNotFoundForInvalidId`
- [ ] `Approve_UpdatesStatusToApproved`
- [ ] `Approve_SendsNotifications`
- [ ] `Approve_LogsAuditTrail`
- [ ] `Approve_FailsIfAlreadyApproved`
- [ ] `Approve_FailsIfNoAuthority`
- [ ] `Approve_FailsIfApprovingOwnSubmission`
- [ ] `Reject_RequiresComments`
- [ ] `Reject_ValidatesMinimumCommentLength`
- [ ] `Reject_UpdatesStatusToRejected`
- [ ] `Reject_SendsNotificationsWithComments`

**Service Tests:**
- [ ] `NotificationService_SendsEmailViaHangfire`
- [ ] `NotificationService_SendsSignalRNotification`
- [ ] `ApprovalService_ChecksAuthority`
- [ ] `ApprovalService_PreventsDoubleApproval`
- [ ] `ComparisonService_CalculatesChangeIndicators`
- [ ] `ComparisonService_HandlesNullPreviousSubmission`

---

### 10.2 Integration Tests

**Approval Flow:**
- [ ] Submit checklist → Approver receives notification → Approve → Submitter receives notification
- [ ] Submit checklist → Approver receives notification → Reject → Submitter receives notification with comments
- [ ] Reject → Submitter corrects → Resubmit → Approver receives notification → Approve

**Database Tests:**
- [ ] Submission status updates correctly
- [ ] ApprovedById and ApprovedDate set correctly
- [ ] RejectionComments stored correctly
- [ ] Audit trail entries created correctly
- [ ] Optimistic locking prevents concurrent approvals

**Notification Tests:**
- [ ] Email sent when submission approved
- [ ] Email sent when submission rejected
- [ ] Email contains correct submission details and comments
- [ ] SignalR notification received by submitter in real-time
- [ ] Notification badge updates without page refresh

---

### 10.3 UI/UX Tests

**Pending Approvals Dashboard:**
- [ ] Dashboard displays all pending submissions
- [ ] Filters work correctly (factory, template, date range)
- [ ] Summary stats update when filters applied
- [ ] Quick approve/reject buttons open modals
- [ ] Review button navigates to review page
- [ ] Responsive layout works on mobile/tablet/desktop

**Review Interface:**
- [ ] Submission metadata displays correctly
- [ ] All sections expand/collapse correctly
- [ ] Fields display pre-fill indicator
- [ ] Submitter notes display correctly
- [ ] View mode toggle switches to comparison view
- [ ] Comparison view shows change indicators
- [ ] Approval actions panel fixed at bottom
- [ ] Approve/reject buttons open modals

**Modals:**
- [ ] Approve modal shows submission summary
- [ ] Approve modal allows optional comments
- [ ] Approve modal selects notification recipients
- [ ] Reject modal requires comments (min 20 chars)
- [ ] Reject modal validates comment length
- [ ] Reject modal prevents submission without comments
- [ ] Character counter updates in real-time
- [ ] Loading spinner shows during AJAX submission

**Notifications:**
- [ ] Notification badge appears when new notification received
- [ ] Badge count updates correctly
- [ ] Toast notification appears in bottom-right
- [ ] Toast auto-dismisses after 5 seconds
- [ ] Notification dropdown shows recent 5 notifications
- [ ] Click notification navigates to correct page
- [ ] "Mark all read" clears badge

---

### 10.4 Performance Tests

**Load Testing:**
- [ ] Dashboard loads < 1 second with 100 pending submissions
- [ ] Review page loads < 2 seconds with 10 sections, 50 questions
- [ ] Comparison view loads < 3 seconds (2 submissions with 50 questions each)
- [ ] Approve action completes < 500ms (excluding email)
- [ ] Reject action completes < 500ms (excluding email)
- [ ] SignalR notification delivered < 200ms

**Stress Testing:**
- [ ] 10 concurrent approvals on different submissions (no conflicts)
- [ ] 2 concurrent approvals on same submission (conflict detected)
- [ ] 100 users connected to SignalR hub simultaneously
- [ ] Bulk approve 10 submissions < 5 seconds

---

### 10.5 Security Tests

**Authorization Tests:**
- [ ] Factory ICT Officer cannot access approval pages
- [ ] Regional Manager cannot approve submissions outside their region
- [ ] Regional Manager cannot approve own submissions
- [ ] Head Office can approve all submissions
- [ ] Cannot approve via direct API call without authorization
- [ ] Cannot approve already approved submission

**Input Validation Tests:**
- [ ] SQL injection attempt in rejection comments blocked
- [ ] XSS attempt in rejection comments sanitized
- [ ] Invalid submission ID returns 404
- [ ] Malformed AJAX request returns 400
- [ ] CSRF token validated on POST requests

---

### 10.6 Accessibility Tests

**WCAG 2.1 Compliance:**
- [ ] All interactive elements keyboard accessible (Tab, Enter, Escape)
- [ ] Modal can be closed with Escape key
- [ ] Form fields have proper labels (for screen readers)
- [ ] Color contrast meets WCAG AA standards (4.5:1 minimum)
- [ ] Focus indicators visible on all interactive elements
- [ ] ARIA attributes used correctly (aria-label, aria-describedby)
- [ ] Screen reader announces status changes (approved, rejected)

---

## DOCUMENT END

**Next Steps:**
1. Review this document with development team
2. Clarify any ambiguities or questions
3. Begin Day 1 implementation (Pending Approvals Dashboard backend)
4. Follow step-by-step guide through Day 5
5. Conduct thorough testing using checklist in Section 10
6. Deploy to staging environment for UAT
7. Gather feedback and make refinements
8. Deploy to production

**Related Documents:**
- [0_ChecklistSystem_Overview.md](0_ChecklistSystem_Overview.md) - System overview for non-technical stakeholders
- [1_Automation_Points.md](1_Automation_Points.md) - Pre-fill opportunities and automation strategy
- [2_ImplementationPlan.md](2_ImplementationPlan.md) - Master implementation plan (Weeks 5-9)
- [3_FormBuilder_Implementation.md](3_FormBuilder_Implementation.md) - Template builder implementation
- [4_FormRendering_Implementation.md](4_FormRendering_Implementation.md) - Form rendering implementation
- [6_Reports_Dashboard_Implementation.md](6_Reports_Dashboard_Implementation.md) - Reports & dashboards (next document)

**Questions or Clarifications:**
Contact: ICT Development Team Lead
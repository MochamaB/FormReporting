# Checklist System - Complete Overview & Process Flow

**Version:** 1.0
**Date:** October 29, 2025
**Purpose:** Explain how the dynamic checklist system works for both technical and non-technical audiences
**Priority:** HIGHEST - This is where all operational data originates

---

## Table of Contents

1. [Business Overview (Non-Technical)](#1-business-overview-non-technical)
2. [The Problem We're Solving](#2-the-problem-were-solving)
3. [Key Concepts](#3-key-concepts)
4. [User Roles & Permissions](#4-user-roles--permissions)
5. [Complete Process Flow](#5-complete-process-flow)
6. [Example Walkthrough](#6-example-walkthrough-factory-monthly-report)
7. [Technical Implementation](#7-technical-implementation-for-developers)
8. [Database Tables Explained](#8-database-tables-explained)
9. [Data Flow Architecture](#9-data-flow-architecture)
10. [Approval Workflow](#10-approval-workflow)
11. [Reporting & Analytics](#11-reporting--analytics)
12. [FAQs](#12-faqs)

---

## 1. Business Overview (Non-Technical)

### What is the Checklist System?

The Checklist System is the **heart of the KTDA ICT Reporting System**. It replaces manual Excel sheets and Word documents with an online form system that:

- ✅ Allows admins to **create custom forms** (like "Daily Checklist", "Factory Monthly Report")
- ✅ Factory ICT staff **fill out these forms** online (instead of Excel)
- ✅ Regional managers **review and approve** submissions
- ✅ System **automatically generates reports** matching your current Excel/Word formats
- ✅ Historical data is **searchable and comparable** over time

### Why is This Important?

Currently at KTDA:
- Factory ICT staff fill Excel sheets daily/monthly by hand
- Files are emailed to regional managers
- Regional managers consolidate data manually
- Reports are reformatted for Head Office
- Data is trapped in separate files (hard to analyze trends)
- Version control issues (which Excel file is latest?)

**The Checklist System solves all of this** by centralizing data collection, automating workflows, and enabling real-time reporting.

---

## 2. The Problem We're Solving

### Current Manual Process:

```
Day 1:
Factory ICT Staff → Fill Excel "Daily Checklist.xlsx" → Email to Regional Manager

Day 2-30:
Factory ICT Staff → Fill Excel "Daily Checklist.xlsx" → Email to Regional Manager

End of Month:
Factory ICT Staff → Fill Word "Monthly Report.docx" → Email to Regional Manager

Regional Manager → Opens 30+ Excel files → Manually consolidates data →
Creates summary Excel → Emails to Head Office

Head Office → Receives 7 regional summaries → Manually consolidates →
Creates PowerPoint presentation
```

**Problems:**
- ⚠️ Time-consuming manual data entry and consolidation
- ⚠️ Emails get lost or delayed
- ⚠️ Hard to track who submitted what and when
- ⚠️ No validation (errors slip through)
- ⚠️ Can't compare trends across factories easily
- ⚠️ No audit trail of changes
- ⚠️ Approvals happen via email (no formal workflow)

### New Automated Process:

```
Daily:
Factory ICT Staff → Open web app → Fill "Daily Checklist" form → Click Submit →
System notifies Regional Manager → Manager reviews online → Clicks Approve

End of Month:
Factory ICT Staff → Open web app → Fill "Monthly Report" form → Click Submit →
System notifies Regional Manager → Manager reviews online → Clicks Approve

Anytime:
Regional Manager → Open dashboard → See all submissions in real-time →
Export consolidated report with one click

Head Office → Open dashboard → See all 7 regions data live →
Generate PowerPoint-ready charts automatically
```

**Benefits:**
- ✅ No emails needed (all data in central system)
- ✅ Real-time visibility for managers
- ✅ Automatic validation (system checks required fields)
- ✅ Instant trend analysis (compare this month vs last month)
- ✅ Complete audit trail (who submitted, when, what changed)
- ✅ Formal approval workflow with notifications
- ✅ Reports generated automatically

---

## 3. Key Concepts

### Concept 1: Checklist Template

**What it is:** A reusable form design (like a blank Excel template)

**Example:** "Factory Monthly Report" template

**Contains:**
- Template name and description
- Frequency (Daily, Weekly, Monthly, Quarterly)
- List of questions/fields to fill out
- Validation rules (which fields are required)
- Who can fill it (Factories, Subsidiaries, Head Office)

**Created by:** System Administrator or Head Office ICT Manager

**Analogy:** Think of this as the **empty form** that gets filled out repeatedly.

---

### Concept 2: Checklist Item (Field/Question)

**What it is:** A single question or field in a checklist template

**Examples:**
- "Total number of computers in factory" (Number field)
- "Date of last backup" (Date field)
- "Were there any network issues today?" (Yes/No field)
- "Describe any incidents" (Text area)
- "Operating system" (Dropdown: Windows 10, Windows 11, Linux)

**Properties:**
- Field type (Text, Number, Date, Dropdown, Yes/No, File Upload)
- Is it required or optional?
- Validation rules (e.g., "Number must be between 1 and 1000")
- Display order (which question comes first)
- Section grouping (e.g., "Hardware Section", "Software Section")

**Analogy:** Think of this as **one cell or question** in your Excel sheet.

---

### Concept 3: Checklist Submission

**What it is:** A completed checklist form for a specific period

**Example:** Kambaa Factory's "Daily Checklist" submission for October 29, 2025

**Contains:**
- Which template was used (e.g., "Daily Checklist")
- Which tenant submitted it (e.g., Kambaa Factory)
- Reporting period (e.g., October 29, 2025)
- Who filled it out (e.g., Peter Mwangi - ICT Support)
- When it was submitted
- Current status (Draft, Submitted, Approved, Rejected)
- All the answers to the questions

**Analogy:** Think of this as **one completed Excel file** that was emailed to the manager.

---

### Concept 4: Checklist Response (Answer)

**What it is:** The actual answer to one specific question in a submission

**Examples:**
- Question: "Total number of computers" → Answer: 45
- Question: "Date of last backup" → Answer: October 28, 2025
- Question: "Were there network issues?" → Answer: No
- Question: "Describe incidents" → Answer: "Power outage from 2pm-3pm"

**Storage:** Uses flexible "EAV" pattern (Entity-Attribute-Value) to handle different answer types

**Analogy:** Think of this as **one cell's value** in a completed Excel sheet.

---

## 4. User Roles & Permissions

### Role 1: System Administrator (Head Office)

**Can do:**
- ✅ Create new checklist templates
- ✅ Edit existing templates
- ✅ Add/remove questions from templates
- ✅ Publish templates (make them available to factories)
- ✅ Archive old template versions
- ✅ View all submissions from all tenants
- ✅ Generate system-wide reports

**Cannot do:**
- ❌ Cannot fill out checklists on behalf of factories (data integrity)

**Example Users:** ICT Manager at Head Office, System Admin

---

### Role 2: Regional ICT Manager

**Can do:**
- ✅ View all submissions from factories in their region
- ✅ Approve or reject submissions
- ✅ Add comments/feedback to submissions
- ✅ Generate regional consolidated reports
- ✅ View submission history and trends

**Cannot do:**
- ❌ Cannot edit templates (only admins can)
- ❌ Cannot fill out checklists (only factory staff can)
- ❌ Cannot view other regions' data (unless given permission)

**Example Users:** Region 1 Manager John Kamau, Region 2 Manager Mary Wanjiku

---

### Role 3: Factory ICT Support Staff

**Can do:**
- ✅ View available checklist templates for their factory
- ✅ Fill out checklist forms
- ✅ Save drafts (partial completion)
- ✅ Submit completed checklists for approval
- ✅ View their own submission history
- ✅ Edit rejected submissions and resubmit

**Cannot do:**
- ❌ Cannot create templates
- ❌ Cannot approve their own submissions
- ❌ Cannot view other factories' submissions
- ❌ Cannot edit approved submissions (immutable)

**Example Users:** Peter Mwangi (Kambaa Factory ICT), Jane Njeri (Tbesonik Factory ICT)

---

### Role 4: Subsidiary ICT Staff

**Can do:**
- Same as Factory ICT Support Staff
- Submissions go directly to Head Office (no regional manager)

**Example Users:** KTDA Training Lodge ICT, Research Department ICT

---

## 5. Complete Process Flow

### Phase 1: Template Creation (One-time setup per form type)

```
┌─────────────────────────────────────────────────────────┐
│ STEP 1: Admin Creates Template                         │
│                                                         │
│ Admin (Head Office) logs in                            │
│   ↓                                                     │
│ Goes to "Checklist Templates" section                  │
│   ↓                                                     │
│ Clicks "Create New Template"                           │
│   ↓                                                     │
│ Fills in:                                              │
│   - Template Name: "Factory Monthly Report"           │
│   - Description: "Monthly operational report..."       │
│   - Frequency: Monthly                                 │
│   - Requires Approval: Yes                             │
│   - Applicable To: Factories only                      │
│   ↓                                                     │
│ Clicks "Save Template"                                 │
└─────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────┐
│ STEP 2: Admin Adds Questions (Checklist Items)        │
│                                                         │
│ Admin clicks "Add Question"                            │
│   ↓                                                     │
│ Question 1:                                            │
│   - Question Text: "Total number of computers"        │
│   - Field Type: Number                                │
│   - Required: Yes                                      │
│   - Min Value: 1                                       │
│   - Max Value: 500                                     │
│   - Section: Hardware                                  │
│   ↓                                                     │
│ Question 2:                                            │
│   - Question Text: "Date of last backup"              │
│   - Field Type: Date                                  │
│   - Required: Yes                                      │
│   - Must be within last 7 days                        │
│   - Section: Data Management                           │
│   ↓                                                     │
│ Question 3:                                            │
│   - Question Text: "Network uptime percentage"        │
│   - Field Type: Number (decimal)                      │
│   - Required: Yes                                      │
│   - Min: 0, Max: 100                                   │
│   - Section: Infrastructure                            │
│   ↓                                                     │
│ ... (adds 30 more questions)                           │
│   ↓                                                     │
│ Admin reviews question order                           │
│ Admin drags questions to reorder sections              │
│   ↓                                                     │
│ Admin clicks "Preview Template"                        │
│ (sees how form will look to users)                     │
│   ↓                                                     │
│ Admin clicks "Publish Template"                        │
└─────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────┐
│ RESULT: Template is now available to all factories    │
└─────────────────────────────────────────────────────────┘
```

---

### Phase 2: Form Filling (Monthly by factory staff)

```
┌─────────────────────────────────────────────────────────┐
│ STEP 3: Factory Staff Opens Checklist                 │
│                                                         │
│ Peter Mwangi (Kambaa Factory) logs in                 │
│   ↓                                                     │
│ Dashboard shows:                                        │
│   "📋 You have 1 pending checklist to complete:       │
│    Factory Monthly Report - October 2025"             │
│   ↓                                                     │
│ Peter clicks "Fill Checklist"                         │
│   ↓                                                     │
│ System displays form with all questions grouped:       │
│                                                         │
│   [Hardware Section]                                   │
│   Q1: Total number of computers: [___45___]           │
│   Q2: Number of laptops: [___12___]                   │
│   Q3: Number of desktops: [___33___]                  │
│                                                         │
│   [Data Management Section]                            │
│   Q4: Date of last backup: [📅 Oct 28, 2025]         │
│   Q5: Backup successful?: [✓ Yes] [ ] No             │
│                                                         │
│   [Infrastructure Section]                             │
│   Q6: Network uptime %: [___98.5___]                  │
│   Q7: Internet provider: [Dropdown: Safaricom ▼]     │
│                                                         │
│   ... (28 more questions)                              │
│   ↓                                                     │
│ Peter fills out first 10 questions                     │
│   ↓                                                     │
│ System auto-saves as DRAFT every 30 seconds           │
│ (Peter can close browser and come back later)         │
└─────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────┐
│ STEP 4: Completion & Validation                       │
│                                                         │
│ Next day, Peter logs back in                           │
│   ↓                                                     │
│ Dashboard shows: "You have 1 draft checklist"         │
│   ↓                                                     │
│ Peter clicks "Continue Editing"                        │
│   ↓                                                     │
│ System loads saved draft (10 questions already filled) │
│   ↓                                                     │
│ Peter completes remaining 23 questions                 │
│   ↓                                                     │
│ Progress bar shows: "100% Complete ✓"                 │
│   ↓                                                     │
│ Peter clicks "Submit for Approval"                     │
│   ↓                                                     │
│ System validates:                                       │
│   ✓ All required fields filled                        │
│   ✓ Numbers within valid ranges                       │
│   ✓ Dates in correct format                           │
│   ✓ Conditional logic satisfied                       │
│   ↓                                                     │
│ Validation passes ✓                                    │
│   ↓                                                     │
│ System changes status: DRAFT → SUBMITTED              │
│   ↓                                                     │
│ System records:                                         │
│   - Submitted by: Peter Mwangi                        │
│   - Submission date: Oct 29, 2025 2:45 PM            │
│   - Tenant: Kambaa Factory                            │
│   - Reporting period: October 2025                     │
└─────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────┐
│ STEP 5: Automatic Notifications                       │
│                                                         │
│ System sends:                                          │
│   📧 Email to John Kamau (Regional Manager):          │
│      "New submission awaiting approval:                │
│       Kambaa Factory - Monthly Report - Oct 2025"     │
│   ↓                                                     │
│   🔔 Real-time notification in John's dashboard:      │
│      "1 new pending approval" (badge appears)         │
│   ↓                                                     │
│ Peter sees confirmation:                               │
│   "✓ Submission successful. Awaiting manager approval"│
└─────────────────────────────────────────────────────────┘
```

---

### Phase 3: Approval Workflow (By regional manager)

```
┌─────────────────────────────────────────────────────────┐
│ STEP 6: Manager Reviews Submission                    │
│                                                         │
│ John Kamau (Region 1 Manager) logs in                 │
│   ↓                                                     │
│ Dashboard shows:                                        │
│   "🔔 5 submissions pending your approval"            │
│   ↓                                                     │
│ John clicks "Pending Approvals"                        │
│   ↓                                                     │
│ System displays table:                                 │
│                                                         │
│ ┌─────────┬──────────┬─────────┬──────────┬────────┐ │
│ │ Factory │ Template │ Period  │ Submitted│ Action │ │
│ ├─────────┼──────────┼─────────┼──────────┼────────┤ │
│ │ Kambaa  │ Monthly  │ Oct 2025│ Oct 29   │ Review │ │
│ │ Tbesonik│ Monthly  │ Oct 2025│ Oct 28   │ Review │ │
│ │ Kangaita│ Monthly  │ Oct 2025│ Oct 27   │ Review │ │
│ └─────────┴──────────┴─────────┴──────────┴────────┘ │
│   ↓                                                     │
│ John clicks "Review" for Kambaa submission             │
└─────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────┐
│ STEP 7: Review Interface                              │
│                                                         │
│ System shows:                                          │
│                                                         │
│ ┌───────────────────────────────────────────────────┐ │
│ │ Kambaa Factory - Monthly Report - October 2025   │ │
│ │ Submitted by: Peter Mwangi                        │ │
│ │ Submitted: Oct 29, 2025 2:45 PM                  │ │
│ │ Status: Awaiting Approval                         │ │
│ ├───────────────────────────────────────────────────┤ │
│ │ [Hardware Section]                                │ │
│ │ Total computers: 45                               │ │
│ │ Laptops: 12                                       │ │
│ │ Desktops: 33                                      │ │
│ │                                                   │ │
│ │ [Data Management Section]                         │ │
│ │ Last backup: Oct 28, 2025                        │ │
│ │ Backup successful: Yes                            │ │
│ │                                                   │ │
│ │ [Infrastructure Section]                          │ │
│ │ Network uptime: 98.5%                            │ │
│ │ Internet provider: Safaricom                      │ │
│ │                                                   │ │
│ │ ... (28 more responses)                           │ │
│ └───────────────────────────────────────────────────┘ │
│   ↓                                                     │
│ John reviews all answers                               │
│   ↓                                                     │
│ John can:                                              │
│   [Compare with Previous Month]  (shows side-by-side)  │
│   [View Submission History]      (all past submissions)│
│   [Export to PDF]                (for offline review)  │
└─────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────┐
│ STEP 8A: Approval Path                                │
│                                                         │
│ John finds data looks correct                          │
│   ↓                                                     │
│ John clicks "Approve" button                           │
│   ↓                                                     │
│ (Optional) John adds comment:                          │
│   "Good report. Network uptime improved from last      │
│    month (95% → 98.5%). Keep it up!"                  │
│   ↓                                                     │
│ John clicks "Confirm Approval"                         │
│   ↓                                                     │
│ System updates:                                         │
│   - Status: SUBMITTED → APPROVED                      │
│   - Approved by: John Kamau                           │
│   - Approval date: Oct 29, 2025 4:15 PM              │
│   - Comments: "Good report..."                        │
│   ↓                                                     │
│ System sends:                                          │
│   📧 Email to Peter Mwangi:                           │
│      "✓ Your October monthly report was approved"     │
│   🔔 Real-time notification to Peter's dashboard      │
│   ↓                                                     │
│ Submission is now LOCKED (immutable, cannot be edited) │
│   ↓                                                     │
│ Data is available for reporting and analytics          │
└─────────────────────────────────────────────────────────┘
          OR
┌─────────────────────────────────────────────────────────┐
│ STEP 8B: Rejection Path (if data has issues)          │
│                                                         │
│ John notices network uptime shows 150% (impossible!)   │
│   ↓                                                     │
│ John clicks "Reject" button                            │
│   ↓                                                     │
│ System requires John to enter rejection reason:        │
│   "Network uptime shows 150% which is impossible.      │
│    Please verify and correct this value."             │
│   ↓                                                     │
│ John clicks "Confirm Rejection"                        │
│   ↓                                                     │
│ System updates:                                         │
│   - Status: SUBMITTED → REJECTED                      │
│   - Rejected by: John Kamau                           │
│   - Rejection date: Oct 29, 2025 4:15 PM             │
│   - Rejection reason: "Network uptime shows 150%..."  │
│   ↓                                                     │
│ System sends:                                          │
│   📧 Email to Peter Mwangi:                           │
│      "❌ Your October monthly report was rejected"    │
│      "Reason: Network uptime shows 150%..."           │
│   🔔 Real-time notification to Peter's dashboard      │
│   ↓                                                     │
│ Submission returns to Peter as EDITABLE                │
└─────────────────────────────────────────────────────────┘
          ↓
┌─────────────────────────────────────────────────────────┐
│ STEP 9: Resubmission (if rejected)                    │
│                                                         │
│ Peter logs in and sees notification:                   │
│   "Your October monthly report needs corrections"      │
│   ↓                                                     │
│ Peter clicks "View Feedback"                           │
│   ↓                                                     │
│ System shows rejection reason highlighted:             │
│   "⚠️ Network uptime shows 150%..."                   │
│   ↓                                                     │
│ Peter clicks "Edit Submission"                         │
│   ↓                                                     │
│ Form reopens with all previous answers pre-filled      │
│   ↓                                                     │
│ Peter corrects: Network uptime: [___98.5___]          │
│   ↓                                                     │
│ Peter clicks "Resubmit for Approval"                   │
│   ↓                                                     │
│ System:                                                 │
│   - Status: REJECTED → SUBMITTED (again)              │
│   - Adds to John's pending approvals queue            │
│   - Notifies John of resubmission                     │
│   ↓                                                     │
│ John reviews again → Approves                          │
└─────────────────────────────────────────────────────────┘
```

---

## 6. Example Walkthrough: Factory Monthly Report

Let's trace one complete example from start to finish.

### Scenario:
**Admin needs to create "Factory Monthly Report" template that factories will fill out every month**

---

### Step 1: Admin Creates Template

**User:** Sarah Wambui (System Administrator at Head Office)
**Date:** October 1, 2025

Sarah logs into the KTDA system and navigates to:
```
Dashboard → Checklist Management → Templates → Create New Template
```

She fills in the template details:

| Field | Value |
|-------|-------|
| Template Name | Factory Monthly Report |
| Description | Comprehensive monthly operational report covering hardware, software, network, and support activities |
| Frequency | Monthly |
| Requires Approval | Yes |
| Applicable To | Factories only (not subsidiaries) |
| Active | Yes |

She clicks **Save Template**.

---

### Step 2: Admin Adds Questions

Now Sarah adds 33 questions across 5 sections:

#### Section 1: Hardware (7 questions)

| # | Question Text | Field Type | Required | Validation |
|---|---------------|------------|----------|------------|
| 1 | Total number of computers | Number | Yes | Min: 1, Max: 500 |
| 2 | Number of laptops | Number | Yes | Min: 0, Max: 500 |
| 3 | Number of desktops | Number | Yes | Min: 0, Max: 500 |
| 4 | Number of printers | Number | Yes | Min: 0, Max: 100 |
| 5 | Number of network switches | Number | Yes | Min: 0, Max: 50 |
| 6 | Number of servers | Number | Yes | Min: 0, Max: 20 |
| 7 | Any hardware failures this month? | Boolean (Yes/No) | Yes | - |

#### Section 2: Software (8 questions)

| # | Question Text | Field Type | Required | Validation |
|---|---------------|------------|----------|------------|
| 8 | Windows OS installations | Number | Yes | Min: 0 |
| 9 | Office 365 licenses active | Number | Yes | Min: 0 |
| 10 | Antivirus software up to date? | Boolean | Yes | - |
| 11 | Last antivirus update date | Date | Yes | Must be within last 30 days |
| 12 | ERP system version | Dropdown | Yes | Options: v2.1, v2.2, v3.0 |
| 13 | ERP system uptime % | Number (decimal) | Yes | Min: 0, Max: 100 |
| 14 | Any software licensing issues? | Boolean | Yes | - |
| 15 | If yes, describe issues | Text Area | No | Only show if Q14 = Yes |

#### Section 3: Network & Infrastructure (9 questions)

| # | Question Text | Field Type | Required | Validation |
|---|---------------|------------|----------|------------|
| 16 | Internet provider | Dropdown | Yes | Safaricom, Airtel, Telkom, Other |
| 17 | Network uptime % | Number (decimal) | Yes | Min: 0, Max: 100 |
| 18 | Average download speed (Mbps) | Number | Yes | Min: 1 |
| 19 | Average upload speed (Mbps) | Number | Yes | Min: 1 |
| 20 | Any network outages this month? | Boolean | Yes | - |
| 21 | If yes, total outage hours | Number | No | Only show if Q20 = Yes |
| 22 | Backup performed this month? | Boolean | Yes | - |
| 23 | Date of last backup | Date | Yes | Must be within last 31 days |
| 24 | Backup successful? | Boolean | Yes | - |

#### Section 4: Support & Tickets (6 questions)

| # | Question Text | Field Type | Required | Validation |
|---|---------------|------------|----------|------------|
| 25 | Total support tickets received | Number | Yes | Min: 0 |
| 26 | Tickets resolved | Number | Yes | Min: 0 |
| 27 | Tickets pending | Number | Yes | Min: 0 |
| 28 | Average resolution time (hours) | Number (decimal) | Yes | Min: 0 |
| 29 | Most common issue type | Dropdown | Yes | Hardware, Software, Network, User Training, Other |
| 30 | Any critical unresolved issues? | Boolean | Yes | - |

#### Section 5: General Comments (3 questions)

| # | Question Text | Field Type | Required | Validation |
|---|---------------|------------|----------|------------|
| 31 | Key achievements this month | Text Area | No | Max 1000 characters |
| 32 | Challenges faced | Text Area | No | Max 1000 characters |
| 33 | Support needed from Head Office | Text Area | No | Max 1000 characters |

Sarah reviews the template, uses drag-and-drop to reorder questions, then clicks **Publish Template**.

The template is now live and available to all 100+ factories.

---

### Step 3: Factory Staff Fills Form (November 1, 2025)

**User:** Peter Mwangi (ICT Support at Kambaa Factory)
**Task:** Fill out October 2025 monthly report

Peter logs in and sees:

```
Dashboard
┌─────────────────────────────────────────────────┐
│ 📋 Pending Checklists                          │
├─────────────────────────────────────────────────┤
│ Factory Monthly Report - October 2025          │
│ Due: November 5, 2025                          │
│ Status: Not Started                            │
│ [Fill Checklist]                               │
└─────────────────────────────────────────────────┘
```

Peter clicks **Fill Checklist**. The system generates a form with all 33 questions.

Peter fills it out:

**Section 1: Hardware**
- Total computers: `45`
- Laptops: `12`
- Desktops: `33`
- Printers: `5`
- Network switches: `3`
- Servers: `2`
- Hardware failures?: `No`

**Section 2: Software**
- Windows installations: `45`
- Office 365 licenses: `45`
- Antivirus up to date?: `Yes`
- Last antivirus update: `October 28, 2025`
- ERP version: `v3.0`
- ERP uptime: `99.2%`
- Licensing issues?: `No`

**Section 3: Network**
- Internet provider: `Safaricom`
- Network uptime: `98.5%`
- Download speed: `50 Mbps`
- Upload speed: `20 Mbps`
- Network outages?: `Yes`
- Total outage hours: `2` (conditional field appeared)
- Backup performed?: `Yes`
- Last backup date: `October 31, 2025`
- Backup successful?: `Yes`

**Section 4: Support**
- Total tickets: `23`
- Resolved: `21`
- Pending: `2`
- Avg resolution time: `4.5 hours`
- Most common issue: `User Training`
- Critical unresolved?: `No`

**Section 5: Comments**
- Key achievements: `"Completed migration to Windows 11 for all admin PCs. Conducted user training sessions."`
- Challenges: `"Brief power outage on Oct 15 caused 2-hour network downtime."`
- Support needed: `"Need additional UPS units for server room."`

Peter clicks **Submit for Approval**.

System validates all fields → ✓ Validation passes → Status changes to **SUBMITTED**

---

### Step 4: Regional Manager Approves (November 2, 2025)

**User:** John Kamau (Region 1 ICT Manager)

John logs in and sees notification:

```
🔔 5 new submissions pending approval
```

John navigates to **Pending Approvals** and sees:

| Factory | Template | Period | Submitted | Status |
|---------|----------|--------|-----------|--------|
| Kambaa | Monthly Report | Oct 2025 | Nov 1 | Pending |
| Tbesonik | Monthly Report | Oct 2025 | Nov 1 | Pending |
| Kangaita | Monthly Report | Oct 2025 | Oct 31 | Pending |
| ... | ... | ... | ... | ... |

John clicks **Review** for Kambaa's submission.

He reviews all 33 answers, compares with September data:

```
Comparison View:
┌──────────────────┬───────────┬───────────┬────────┐
│ Metric           │ September │ October   │ Change │
├──────────────────┼───────────┼───────────┼────────┤
│ Total Computers  │ 45        │ 45        │ 0      │
│ Network Uptime   │ 95.0%     │ 98.5%     │ ↑ 3.5% │
│ Total Tickets    │ 28        │ 23        │ ↓ 5    │
│ Resolved Tickets │ 25        │ 21        │ ↓ 4    │
│ Avg Resolution   │ 6.2 hrs   │ 4.5 hrs   │ ↓ 1.7  │
└──────────────────┴───────────┴───────────┴────────┘
```

John is satisfied with the data. He adds comment:

```
"Excellent work improving network uptime from 95% to 98.5%.
Resolution time also improved. UPS request noted - will forward
to procurement."
```

John clicks **Approve**.

System:
- Changes status to **APPROVED**
- Locks submission (immutable)
- Sends email/notification to Peter
- Makes data available for regional/HO reports

---

### Step 5: Head Office Views Consolidated Report

**User:** Managing Director or Head of ICT

Logs into dashboard and navigates to:
```
Reports → Monthly Consolidated Report → October 2025
```

System automatically generates report showing:

**Region 1 Summary (15 factories):**
- Average network uptime: `97.8%`
- Total support tickets: `345`
- Average resolution time: `5.2 hours`
- Total computers managed: `675`

**Region 2 Summary (18 factories):**
- Average network uptime: `96.5%`
- Total support tickets: `412`
- Average resolution time: `6.1 hours`
- Total computers managed: `810`

**All 7 Regions (100+ factories):**
- Charts showing trends
- Top performing factories
- Factories needing attention
- Export to Excel/PDF/PowerPoint

**All of this happens automatically - no manual consolidation needed!**

---

## 7. Technical Implementation (For Developers)

### Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│  (Razor Pages + jQuery + Bootstrap + Chart.js)          │
├─────────────────────────────────────────────────────────┤
│ ChecklistController.cs - handles form rendering        │
│ TemplateController.cs - template CRUD operations       │
│ ApprovalController.cs - approval workflow              │
│                                                         │
│ Views/Checklists/FillForm.cshtml - dynamic form        │
│ Views/Checklists/Review.cshtml - approval UI           │
│ Views/Templates/Builder.cshtml - template designer     │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│                   Application Layer                     │
│         (Business Logic + DTOs + Validators)            │
├─────────────────────────────────────────────────────────┤
│ ChecklistService.cs - form rendering logic             │
│ TemplateService.cs - template management               │
│ ApprovalService.cs - workflow orchestration            │
│ ValidationService.cs - server-side validation          │
│                                                         │
│ DTOs:                                                   │
│ - ChecklistSubmissionDto                               │
│ - ChecklistResponseDto                                 │
│ - TemplateDto                                          │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│                      Domain Layer                       │
│            (Core Entities + Interfaces)                 │
├─────────────────────────────────────────────────────────┤
│ ChecklistTemplate.cs - entity                          │
│ ChecklistItem.cs - entity                              │
│ ChecklistSubmission.cs - entity                        │
│ ChecklistResponse.cs - entity (EAV pattern)            │
│                                                         │
│ IChecklistRepository.cs - data contract                │
│ ITemplateRepository.cs - data contract                 │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                   │
│         (EF Core + Database + External Services)        │
├─────────────────────────────────────────────────────────┤
│ ChecklistRepository.cs - data access                   │
│ TemplateRepository.cs - data access                    │
│                                                         │
│ KTDADbContext.cs with tables:                          │
│ - ChecklistTemplates                                   │
│ - ChecklistTemplateItems                                       │
│ - ChecklistTemplateSubmissions                                 │
│ - ChecklistTemplateResponses (EAV)                             │
│                                                         │
│ NotificationService.cs - emails + SignalR              │
│ HangfireJobs.cs - background notifications             │
└─────────────────────────────────────────────────────────┘
```

---

### Key Technical Patterns

#### 1. EAV Pattern (Entity-Attribute-Value)

**Problem:** Checklist forms are dynamic - we can't predict all possible questions in advance.

**Solution:** Store responses flexibly using EAV pattern.

**Traditional Approach (Rigid):**
```sql
-- Would need a separate table for each checklist type!
CREATE TABLE FactoryMonthlyReportResponses (
    SubmissionId INT,
    TotalComputers INT,
    NumberOfLaptops INT,
    NumberOfDesktops INT,
    ... (33 columns - what if we add a question later?)
);
```

**EAV Approach (Flexible):**
```sql
CREATE TABLE ChecklistTemplateResponses (
    ResponseId INT PRIMARY KEY,
    SubmissionId INT,         -- Which submission
    ChecklistItemId INT,      -- Which question
    TextValue NVARCHAR(MAX),  -- For text answers
    NumericValue DECIMAL,     -- For number answers
    DateValue DATETIME2,      -- For date answers
    BooleanValue BIT          -- For yes/no answers
);

-- Example data:
-- Submission 1, Question "Total computers", Answer = 45
INSERT INTO ChecklistTemplateResponses VALUES
(1, 1, 1, NULL, 45, NULL, NULL);

-- Submission 1, Question "Last backup date", Answer = Oct 28
INSERT INTO ChecklistTemplateResponses VALUES
(2, 1, 23, NULL, NULL, '2025-10-28', NULL);

-- Submission 1, Question "Backup successful?", Answer = Yes
INSERT INTO ChecklistTemplateResponses VALUES
(3, 1, 24, NULL, NULL, NULL, 1);
```

**Benefits:**
- ✅ Add new questions without schema changes
- ✅ Each form can have different questions
- ✅ Historical data preserved even if template changes

---

#### 2. Dynamic Form Rendering

**How it works:**

1. **Template stored as structured data:**
```json
{
  "templateId": 1,
  "templateName": "Factory Monthly Report",
  "sections": [
    {
      "sectionName": "Hardware",
      "items": [
        {
          "itemId": 1,
          "questionText": "Total number of computers",
          "fieldType": "Number",
          "required": true,
          "validation": {
            "min": 1,
            "max": 500
          }
        },
        {
          "itemId": 2,
          "questionText": "Number of laptops",
          "fieldType": "Number",
          "required": true
        }
      ]
    }
  ]
}
```

2. **Backend generates HTML:**
```csharp
public class ChecklistService
{
    public string RenderForm(int templateId, int? submissionId = null)
    {
        var template = _templateRepo.GetById(templateId);
        var existingResponses = submissionId.HasValue
            ? _responseRepo.GetBySubmission(submissionId.Value)
            : null;

        var html = new StringBuilder();

        foreach (var section in template.Sections)
        {
            html.Append($"<div class='section'><h3>{section.Name}</h3>");

            foreach (var item in section.Items)
            {
                var existingValue = existingResponses?
                    .FirstOrDefault(r => r.ChecklistItemId == item.ItemId);

                html.Append(GenerateField(item, existingValue));
            }

            html.Append("</div>");
        }

        return html.ToString();
    }

    private string GenerateField(ChecklistItem item, Response existing)
    {
        return item.FieldType switch
        {
            "Number" => $@"
                <div class='form-group'>
                    <label>{item.QuestionText}</label>
                    <input type='number'
                           name='item_{item.ItemId}'
                           value='{existing?.NumericValue}'
                           {(item.Required ? "required" : "")}
                           min='{item.ValidationRules?.Min}'
                           max='{item.ValidationRules?.Max}' />
                </div>",

            "Text" => $@"
                <div class='form-group'>
                    <label>{item.QuestionText}</label>
                    <input type='text'
                           name='item_{item.ItemId}'
                           value='{existing?.TextValue}'
                           {(item.Required ? "required" : "")} />
                </div>",

            // ... other field types
        };
    }
}
```

3. **Frontend renders form:**
```html
<!-- FillForm.cshtml -->
<form id="checklistForm" data-auto-save="true">
    @Html.Raw(Model.GeneratedFormHtml)

    <button type="button" id="saveDraft">Save Draft</button>
    <button type="submit">Submit for Approval</button>
</form>

<script>
// Auto-save every 30 seconds
setInterval(function() {
    saveDraft();
}, 30000);

function saveDraft() {
    var formData = $('#checklistForm').serialize();
    $.ajax({
        url: '/Checklists/SaveDraft',
        method: 'POST',
        data: formData,
        success: function() {
            showNotification('Draft saved ✓');
        }
    });
}

// Submit handler
$('#checklistForm').submit(function(e) {
    e.preventDefault();

    // Client-side validation
    if (!validateForm()) {
        return false;
    }

    // Submit to server
    $.ajax({
        url: '/Checklists/Submit',
        method: 'POST',
        data: $(this).serialize(),
        success: function(result) {
            if (result.success) {
                window.location.href = '/Checklists/Success';
            } else {
                showErrors(result.errors);
            }
        }
    });
});
</script>
```

---

#### 3. Approval Workflow State Machine

```csharp
public enum SubmissionStatus
{
    Draft = 0,      // Being filled out
    Submitted = 1,  // Awaiting approval
    Approved = 2,   // Manager approved (immutable)
    Rejected = 3    // Sent back for corrections
}

public class ApprovalService
{
    public Result<ChecklistSubmission> SubmitForApproval(int submissionId)
    {
        var submission = _submissionRepo.GetById(submissionId);

        // Validate current state
        if (submission.Status != SubmissionStatus.Draft)
            return Result<ChecklistSubmission>.Failure(
                "Only draft submissions can be submitted");

        // Validate all required fields filled
        var validation = _validationService.ValidateSubmission(submissionId);
        if (!validation.IsValid)
            return Result<ChecklistSubmission>.Failure(
                validation.Errors);

        // Change state
        submission.Status = SubmissionStatus.Submitted;
        submission.SubmittedDate = DateTime.Now;

        _submissionRepo.Update(submission);
        _unitOfWork.SaveChanges();

        // Notify regional manager (background job)
        _hangfire.Enqueue(() =>
            SendApprovalNotification(submission.Id));

        return Result<ChecklistSubmission>.Success(submission);
    }

    public Result<ChecklistSubmission> Approve(
        int submissionId,
        int approverId,
        string comments)
    {
        var submission = _submissionRepo.GetById(submissionId);

        // Validate current state
        if (submission.Status != SubmissionStatus.Submitted)
            return Result<ChecklistSubmission>.Failure(
                "Only submitted items can be approved");

        // Validate approver has permission
        if (!_authService.CanApprove(approverId, submission.TenantId))
            return Result<ChecklistSubmission>.Failure(
                "You don't have permission to approve this submission");

        // Change state
        submission.Status = SubmissionStatus.Approved;
        submission.ApprovedByUserId = approverId;
        submission.ApprovedDate = DateTime.Now;
        submission.ApprovalComments = comments;

        _submissionRepo.Update(submission);
        _unitOfWork.SaveChanges();

        // Notify submitter
        _hangfire.Enqueue(() =>
            SendApprovalConfirmation(submission.Id));

        return Result<ChecklistSubmission>.Success(submission);
    }

    public Result<ChecklistSubmission> Reject(
        int submissionId,
        int rejectorId,
        string reason)
    {
        var submission = _submissionRepo.GetById(submissionId);

        if (submission.Status != SubmissionStatus.Submitted)
            return Result<ChecklistSubmission>.Failure(
                "Only submitted items can be rejected");

        // Change state
        submission.Status = SubmissionStatus.Rejected;
        submission.RejectedByUserId = rejectorId;
        submission.RejectedDate = DateTime.Now;
        submission.RejectionReason = reason;

        _submissionRepo.Update(submission);
        _unitOfWork.SaveChanges();

        // Notify submitter
        _hangfire.Enqueue(() =>
            SendRejectionNotification(submission.Id));

        return Result<ChecklistSubmission>.Success(submission);
    }
}
```

---

## 8. Database Tables Explained

### Table 1: ChecklistTemplates

**Purpose:** Stores the blueprint/design of each checklist form type.

**Non-Technical:** Think of this as the "master template" like a blank Excel form that gets reused.

**Technical:** Entity definition for form templates with metadata.

**Key Fields:**
- `TemplateId` - Unique identifier
- `TemplateName` - e.g., "Factory Monthly Report"
- `Description` - What this checklist is for
- `Frequency` - Daily, Weekly, Monthly, Quarterly, Annual, Ad-Hoc
- `ApplicableTenantTypes` - Who can use this (Factories, Subsidiaries, HeadOffice)
- `RequiresApproval` - Does submission need manager approval?
- `IsActive` - Is template currently in use?
- `Version` - Template version (for change tracking)

**Example Data:**
```sql
TemplateId: 1
TemplateName: "Factory Monthly Report"
Description: "Comprehensive monthly operational report"
Frequency: "Monthly"
ApplicableTenantTypes: "Factory"
RequiresApproval: TRUE
IsActive: TRUE
Version: 1
CreatedDate: 2025-10-01
CreatedByUserId: 2
```

---

### Table 2: ChecklistTemplateSections

**Purpose:** Organizes checklist items into logical groupings within a template.

**Non-Technical:** Think of this as the main headers or tabs in your Excel sheet that group related questions together.

**Technical:** Provides structured section management with UI configuration for accordion-style forms.

**Key Fields:**
- `SectionId` - Unique identifier
- `TemplateId` - Which template this section belongs to
- `SectionName` - Section name (e.g., "Hardware Status", "Software Licenses")
- `SectionDescription` - Help text for the entire section
- `DisplayOrder` - Sequence of sections in form
- `IsCollapsible` - Can users collapse this section?
- `IsCollapsedByDefault` - Starts collapsed when form loads?
- `IconClass` - Font Awesome icon class (e.g., "fa-desktop", "fa-cube")

**Example Data:**
```sql
SectionId: 1
TemplateId: 1
SectionName: "Hardware Status"
SectionDescription: "Report on computer and device status"
DisplayOrder: 1
IsCollapsible: TRUE
IsCollapsedByDefault: FALSE
IconClass: "fa-desktop"

SectionId: 2
TemplateId: 1
SectionName: "Software Licenses"
SectionDescription: "Report on software installations and licensing"
DisplayOrder: 2
IsCollapsible: TRUE
IsCollapsedByDefault: FALSE
IconClass: "fa-cube"

SectionId: 3
TemplateId: 1
SectionName: "Network Infrastructure"
SectionDescription: "Network connectivity and performance metrics"
DisplayOrder: 3
IsCollapsible: TRUE
IsCollapsedByDefault: FALSE
IconClass: "fa-network-wired"
```

---

### Table 3: ChecklistTemplateItems

**Purpose:** Stores individual questions/fields within a section.

**Non-Technical:** Each row is one question in the form (like one cell/row in your Excel sheet).

**Technical:** Defines form fields with type, validation, and conditional logic.

**Key Fields:**
- `ItemId` - Unique identifier
- `TemplateId` - Which template this belongs to
- `SectionId` - Which section this item belongs to (foreign key to ChecklistTemplateSections)
- `QuestionText` - The actual question displayed to user
- `FieldType` - Text, Number, Date, Boolean, Dropdown, TextArea, FileUpload
- `IsRequired` - Must be filled to submit?
- `DisplayOrder` - Sequence within the section
- `ValidationRules` - JSON with min/max/regex rules
- `ConditionalLogic` - JSON defining when to show/hide this field
- `DefaultValue` - Pre-filled value (optional)

**Example Data:**
```sql
ItemId: 1
TemplateId: 1
SectionId: 1  -- "Hardware Status" section
QuestionText: "Total number of computers"
FieldType: "Number"
IsRequired: TRUE
DisplayOrder: 1
ValidationRules: '{"min": 1, "max": 500}'
ConditionalLogic: NULL
DefaultValue: NULL

ItemId: 15
TemplateId: 1
SectionId: 2  -- "Software Licenses" section
QuestionText: "If yes, describe licensing issues"
FieldType: "TextArea"
IsRequired: FALSE
DisplayOrder: 5
ValidationRules: '{"maxLength": 1000}'
ConditionalLogic: '{"showIf": {"itemId": 14, "value": true}}'
DefaultValue: NULL
```

---

### Table 4: ChecklistTemplateSubmissions

**Purpose:** Records each completed checklist form.

**Non-Technical:** This is like one filled Excel file that was submitted.

**Technical:** Entity representing a form submission with workflow state.

**Key Fields:**
- `SubmissionId` - Unique identifier
- `TemplateId` - Which template was used
- `TenantId` - Which factory/subsidiary submitted
- `SubmittedByUserId` - Who filled it out
- `ReportingPeriodStart` - Date range start (e.g., Oct 1, 2025)
- `ReportingPeriodEnd` - Date range end (e.g., Oct 31, 2025)
- `Status` - Draft, Submitted, Approved, Rejected
- `SubmittedDate` - When submitted for approval
- `ApprovedByUserId` - Who approved (if approved)
- `ApprovedDate` - When approved
- `ApprovalComments` - Manager's feedback
- `RejectedByUserId` - Who rejected (if rejected)
- `RejectedDate` - When rejected
- `RejectionReason` - Why rejected

**Example Data:**
```sql
SubmissionId: 1
TemplateId: 1 (Factory Monthly Report)
TenantId: 12 (Kambaa Factory)
SubmittedByUserId: 15 (Peter Mwangi)
ReportingPeriodStart: 2025-10-01
ReportingPeriodEnd: 2025-10-31
Status: Approved
SubmittedDate: 2025-11-01 14:45:00
ApprovedByUserId: 5 (John Kamau)
ApprovedDate: 2025-11-02 09:15:00
ApprovalComments: "Excellent work improving network uptime"
RejectedByUserId: NULL
RejectedDate: NULL
RejectionReason: NULL
CreatedDate: 2025-11-01 10:30:00 (when draft started)
ModifiedDate: 2025-11-02 09:15:00
```

---

### Table 5: ChecklistTemplateResponses (EAV Pattern)

**Purpose:** Stores the actual answers to checklist questions.

**Non-Technical:** Each row is one answer to one question (like one cell's value in Excel).

**Technical:** Flexible key-value storage using EAV pattern.

**Key Fields:**
- `ResponseId` - Unique identifier
- `SubmissionId` - Which submission this belongs to
- `ChecklistItemId` - Which question was answered
- `TextValue` - For text/textarea answers
- `NumericValue` - For number answers
- `DateValue` - For date answers
- `BooleanValue` - For yes/no answers
- `FileUrl` - For file upload answers

**Why 5 value columns?** Because we don't know in advance what type of answer each question will have. Only one column is filled per row (others are NULL).

**Example Data:**
```sql
-- Answer to "Total number of computers" = 45
ResponseId: 1
SubmissionId: 1
ChecklistItemId: 1
TextValue: NULL
NumericValue: 45
DateValue: NULL
BooleanValue: NULL
FileUrl: NULL

-- Answer to "Last backup date" = Oct 28, 2025
ResponseId: 2
SubmissionId: 1
ChecklistItemId: 23
TextValue: NULL
NumericValue: NULL
DateValue: 2025-10-28
BooleanValue: NULL
FileUrl: NULL

-- Answer to "Backup successful?" = Yes
ResponseId: 3
SubmissionId: 1
ChecklistItemId: 24
TextValue: NULL
NumericValue: NULL
DateValue: NULL
BooleanValue: TRUE
FileUrl: NULL

-- Answer to "Challenges faced" = "Power outage..."
ResponseId: 4
SubmissionId: 1
ChecklistItemId: 32
TextValue: "Brief power outage on Oct 15 caused 2-hour downtime"
NumericValue: NULL
DateValue: NULL
BooleanValue: NULL
FileUrl: NULL
```

**How to Query Responses:**
```sql
-- Get all responses for submission 1
SELECT
    i.QuestionText,
    r.TextValue,
    r.NumericValue,
    r.DateValue,
    r.BooleanValue,
    r.FileUrl
FROM ChecklistTemplateResponses r
JOIN ChecklistTemplateItems i ON r.ChecklistItemId = i.ItemId
WHERE r.SubmissionId = 1
ORDER BY i.DisplayOrder;

-- Result:
-- QuestionText                    | NumericValue | DateValue  | BooleanValue
-- Total number of computers       | 45           | NULL       | NULL
-- Number of laptops               | 12           | NULL       | NULL
-- Last backup date                | NULL         | 2025-10-28 | NULL
-- Backup successful?              | NULL         | NULL       | TRUE
```

---

### How Tables Work Together

```
┌─────────────────────────────────────────────────────────┐
│ 1. TEMPLATE DEFINITION (One-time setup)                │
└─────────────────────────────────────────────────────────┘
                        ↓
        ChecklistTemplates (1 row per form type)
                ↓
        ChecklistTemplateItems (33 rows for "Factory Monthly Report")

┌─────────────────────────────────────────────────────────┐
│ 2. FORM FILLING (Happens monthly per factory)         │
└─────────────────────────────────────────────────────────┘
                        ↓
        ChecklistTemplateSubmissions (1 row per form filled)
                ↓
        ChecklistTemplateResponses (33 rows = 33 answers)

┌─────────────────────────────────────────────────────────┐
│ 3. APPROVAL WORKFLOW                                    │
└─────────────────────────────────────────────────────────┘
                        ↓
        ChecklistTemplateSubmissions.Status changes:
        Draft → Submitted → Approved/Rejected

┌─────────────────────────────────────────────────────────┐
│ 4. REPORTING                                            │
└─────────────────────────────────────────────────────────┘
                        ↓
        Query approved submissions
                ↓
        Join with responses to get answers
                ↓
        Aggregate across factories/regions
                ↓
        Generate charts and Excel reports
```

**Relationships:**
```
ChecklistTemplates (1) ──→ (Many) ChecklistTemplateSections
                                    ↓
                                    └──→ (Many) ChecklistTemplateItems
        ↓
        └──→ (Many) ChecklistTemplateSubmissions
                    ↓
                    └──→ (Many) ChecklistTemplateResponses
```

**Example Query to Get Full Submission:**
```sql
-- Get Kambaa Factory's October 2025 Monthly Report
SELECT
    t.TemplateName,
    sub.Status,
    sub.SubmittedDate,
    sub.ApprovedDate,
    s.SectionName,
    s.DisplayOrder AS SectionOrder,
    i.QuestionText,
    i.DisplayOrder AS ItemOrder,
    COALESCE(
        r.TextValue,
        CAST(r.NumericValue AS NVARCHAR),
        CAST(r.DateValue AS NVARCHAR),
        CASE r.BooleanValue
            WHEN 1 THEN 'Yes'
            WHEN 0 THEN 'No'
            ELSE NULL
        END,
        r.FileUrl
    ) AS Answer
FROM ChecklistTemplateSubmissions sub
JOIN ChecklistTemplates t ON sub.TemplateId = t.TemplateId
JOIN ChecklistTemplateResponses r ON r.SubmissionId = sub.SubmissionId
JOIN ChecklistTemplateItems i ON r.ChecklistItemId = i.ItemId
JOIN ChecklistTemplateSections s ON i.SectionId = s.SectionId
WHERE sub.TenantId = 12  -- Kambaa Factory
  AND sub.ReportingPeriodStart = '2025-10-01'
  AND t.TemplateName = 'Factory Monthly Report'
ORDER BY s.DisplayOrder, i.DisplayOrder;
```

---

## 9. Data Flow Architecture

### Diagram: Template Creation → Form Filling → Approval → Reporting

```
┌─────────────────────────────────────────────────────────────────────┐
│                         TEMPLATE CREATION                           │
│                      (One-time per form type)                       │
└─────────────────────────────────────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Admin creates template in UI        │
            │ (TemplateController.Create)         │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ INSERT INTO ChecklistTemplates      │
            │ TemplateId = 1                      │
            └────────���────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Admin adds 33 questions             │
            │ (TemplateController.AddItem)        │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ INSERT INTO ChecklistTemplateItems (33x)    │
            │ ItemId 1-33, TemplateId = 1         │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Admin publishes template            │
            │ IsActive = TRUE                     │
            └─────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                           FORM FILLING                              │
│                   (Monthly per factory - 100+ times)                │
└─────────────────────────────────────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Factory user logs in                │
            │ Dashboard loads pending checklists  │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ SELECT * FROM ChecklistTemplates    │
            │ WHERE IsActive = 1                  │
            │   AND ApplicableTo = 'Factory'      │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ User clicks "Fill Checklist"        │
            │ (ChecklistController.FillForm)      │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ System creates draft submission     │
            │ INSERT INTO ChecklistTemplateSubmissions    │
            │ Status = Draft                      │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ System generates form HTML          │
            │ SELECT * FROM ChecklistTemplateItems        │
            │ WHERE TemplateId = 1                │
            │ ORDER BY DisplayOrder               │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ User fills out questions            │
            │ JavaScript auto-saves every 30s     │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ AJAX POST to /Checklists/SaveDraft  │
            │ INSERT/UPDATE ChecklistTemplateResponses    │
            │ (33 rows)                           │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ User clicks "Submit for Approval"   │
            │ (ChecklistController.Submit)        │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Server-side validation:             │
            │ - All required fields filled?       │
            │ - Values within valid ranges?       │
            │ - Conditional logic satisfied?      │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ UPDATE ChecklistTemplateSubmissions         │
            │ SET Status = Submitted,             │
            │     SubmittedDate = NOW()           │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Background job (Hangfire):          │
            │ Send email to regional manager      │
            │ Send SignalR notification           │
            └─────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                         APPROVAL WORKFLOW                           │
│                    (Per submission by manager)                      │
└─────────────────────────────────────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Regional manager logs in            │
            │ Dashboard shows notification badge  │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ SELECT COUNT(*) FROM Submissions    │
            │ WHERE Status = Submitted            │
            │   AND RegionId = <manager region>   │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Manager clicks "Pending Approvals"  │
            │ (ApprovalController.Index)          │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ SELECT s.*, t.TenantName            │
            │ FROM ChecklistTemplateSubmissions s         │
            │ JOIN Tenants t ON s.TenantId=t.Id   │
            │ WHERE s.Status = Submitted          │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Manager clicks "Review"             │
            │ (ApprovalController.Review)         │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ SELECT * FROM ChecklistTemplateResponses    │
            │ WHERE SubmissionId = <id>           │
            │ JOIN ChecklistTemplateItems...              │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Display all 33 answers              │
            │ Show comparison with previous month │
            │ (SELECT previous period responses)  │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Manager clicks "Approve" OR "Reject"│
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ IF APPROVE:                         │
            │ UPDATE ChecklistTemplateSubmissions         │
            │ SET Status = Approved,              │
            │     ApprovedByUserId = <manager>,   │
            │     ApprovedDate = NOW(),           │
            │     ApprovalComments = <text>       │
            │                                     │
            │ IF REJECT:                          │
            │ UPDATE ChecklistTemplateSubmissions         │
            │ SET Status = Rejected,              │
            │     RejectedByUserId = <manager>,   │
            │     RejectedDate = NOW(),           │
            │     RejectionReason = <text>        │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Background job (Hangfire):          │
            │ Send notification to factory user   │
            │ "Your submission was approved/      │
            │  rejected"                          │
            └─────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                       REPORTING & ANALYTICS                         │
│                   (On-demand by HO/managers)                        │
└─────────────────────────────────────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Manager/HO clicks "Reports"         │
            │ (ReportsController.Index)           │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ SELECT ALL approved submissions     │
            │ FROM ChecklistTemplateSubmissions           │
            │ WHERE Status = Approved             │
            │   AND ReportingPeriod = Oct 2025    │
            │   AND RegionId = <filter>           │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ For each submission:                │
            │ JOIN ChecklistTemplateResponses             │
            │ JOIN ChecklistTemplateItems                 │
            │ Extract numeric values              │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Aggregate across all factories:     │
            │ - AVG(network uptime)               │
            │ - SUM(total tickets)                │
            │ - AVG(resolution time)              │
            │ - SUM(total computers)              │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Generate visualizations:            │
            │ - Chart.js for trend charts         │
            │ - DataTables for tabular data       │
            │ - Export to Excel/PDF               │
            └─────────────────────────────────────┘
                                  ↓
            ┌─────────────────────────────────────┐
            │ Display consolidated report         │
            │ "Region 1: 15 factories, 98% uptime"│
            └─────────────────────────────────────┘
```

---

## 10. Approval Workflow

### State Diagram

```
┌─────────┐
│  DRAFT  │ ← Initial state when user starts filling form
└─────────┘
     │
     │ User clicks "Submit for Approval"
     │ (Triggers: validation, email notification)
     ↓
┌────────────┐
│ SUBMITTED  │ ← Awaiting manager review
└────────────┘
     │
     ├─────────────────────┬─────────────────────┐
     │ Manager Approves    │ Manager Rejects     │
     ↓                     ↓
┌──────────┐         ┌──────────┐
│ APPROVED │         │ REJECTED │
└──────────┘         └──────────┘
     │                     │
     │ (Immutable)         │ User can edit
     │                     │
     │                     ↓
     │               ┌─────────┐
     │               │  DRAFT  │ ← Edit and resubmit
     │               └─────────┘
     │                     │
     │                     ↓
     │               ┌────────────┐
     └───────────────│ SUBMITTED  │
                     └────────────┘
                           │
                           ↓
                     (Approval cycle repeats)
```

### State Transitions

| Current Status | Action | New Status | Allowed By | Side Effects |
|----------------|--------|------------|------------|--------------|
| (None) | Create submission | Draft | Factory ICT Staff | Creates new record |
| Draft | Auto-save | Draft | Factory ICT Staff | Updates responses |
| Draft | Submit for approval | Submitted | Factory ICT Staff | Email to manager, lock responses |
| Submitted | Approve | Approved | Regional Manager | Email to staff, make immutable |
| Submitted | Reject | Rejected | Regional Manager | Email to staff, unlock for editing |
| Rejected | Edit | Draft | Factory ICT Staff | Responses editable again |
| Draft | Resubmit | Submitted | Factory ICT Staff | Email to manager |
| Approved | (Any) | (None) | (No one) | **Immutable - no changes allowed** |

### Business Rules

1. **Only factory staff can create submissions** for their own tenant
2. **Only regional managers can approve submissions** for factories in their region
3. **Head Office can approve submissions** for subsidiaries (not in regions)
4. **Approved submissions are immutable** (cannot be edited or deleted)
5. **Rejected submissions become editable** again as Drafts
6. **Notifications are async** (sent via Hangfire background jobs)
7. **One approval per submission** (no multi-level approval in v1.0)

---

## 11. Reporting & Analytics

### How Reports Are Generated

Once submissions are approved, they become available for reporting:

#### Example 1: Regional Summary Report

**Query:**
```sql
-- Get October 2025 summary for Region 1
SELECT
    t.TenantName AS Factory,
    -- Total computers (ItemId = 1)
    MAX(CASE WHEN r.ChecklistItemId = 1
        THEN r.NumericValue END) AS TotalComputers,
    -- Network uptime (ItemId = 17)
    MAX(CASE WHEN r.ChecklistItemId = 17
        THEN r.NumericValue END) AS NetworkUptime,
    -- Total tickets (ItemId = 25)
    MAX(CASE WHEN r.ChecklistItemId = 25
        THEN r.NumericValue END) AS TotalTickets,
    sub.ApprovedDate
FROM ChecklistTemplateSubmissions sub
JOIN Tenants t ON sub.TenantId = t.TenantId
JOIN ChecklistTemplateResponses r ON r.SubmissionId = sub.SubmissionId
WHERE sub.Status = 'Approved'
  AND sub.TemplateId = 1  -- Factory Monthly Report
  AND sub.ReportingPeriodStart = '2025-10-01'
  AND t.RegionId = 1
GROUP BY t.TenantName, sub.ApprovedDate;
```

**Result:**
| Factory | TotalComputers | NetworkUptime | TotalTickets | ApprovedDate |
|---------|----------------|---------------|--------------|--------------|
| Kambaa | 45 | 98.5 | 23 | 2025-11-02 |
| Tbesonik | 52 | 97.2 | 31 | 2025-11-01 |
| Kangaita | 38 | 99.1 | 18 | 2025-11-02 |

**Aggregated:**
```sql
-- Region 1 averages
SELECT
    COUNT(*) AS TotalFactories,
    SUM(r1.NumericValue) AS TotalComputers,
    AVG(r2.NumericValue) AS AvgNetworkUptime,
    SUM(r3.NumericValue) AS TotalTickets
FROM ChecklistTemplateSubmissions sub
JOIN ChecklistTemplateResponses r1 ON r1.SubmissionId = sub.SubmissionId
    AND r1.ChecklistItemId = 1
JOIN ChecklistTemplateResponses r2 ON r2.SubmissionId = sub.SubmissionId
    AND r2.ChecklistItemId = 17
JOIN ChecklistTemplateResponses r3 ON r3.SubmissionId = sub.SubmissionId
    AND r3.ChecklistItemId = 25
WHERE sub.Status = 'Approved'
  AND sub.TemplateId = 1
  AND sub.ReportingPeriodStart = '2025-10-01'
  AND sub.TenantId IN (
      SELECT TenantId FROM Tenants WHERE RegionId = 1
  );
```

**Result:**
- Total Factories: 15
- Total Computers: 675
- Avg Network Uptime: 98.2%
- Total Tickets: 345

---

#### Example 2: Trend Analysis (Month over Month)

```sql
-- Compare Oct 2025 vs Sep 2025 network uptime for Kambaa Factory
WITH CurrentMonth AS (
    SELECT r.NumericValue AS Uptime
    FROM ChecklistTemplateSubmissions sub
    JOIN ChecklistTemplateResponses r ON r.SubmissionId = sub.SubmissionId
    WHERE sub.TenantId = 12  -- Kambaa
      AND sub.TemplateId = 1
      AND r.ChecklistItemId = 17  -- Network uptime
      AND sub.ReportingPeriodStart = '2025-10-01'
      AND sub.Status = 'Approved'
),
PreviousMonth AS (
    SELECT r.NumericValue AS Uptime
    FROM ChecklistTemplateSubmissions sub
    JOIN ChecklistTemplateResponses r ON r.SubmissionId = sub.SubmissionId
    WHERE sub.TenantId = 12
      AND sub.TemplateId = 1
      AND r.ChecklistItemId = 17
      AND sub.ReportingPeriodStart = '2025-09-01'
      AND sub.Status = 'Approved'
)
SELECT
    c.Uptime AS CurrentUptime,
    p.Uptime AS PreviousUptime,
    (c.Uptime - p.Uptime) AS Change,
    CASE
        WHEN c.Uptime > p.Uptime THEN '↑ Improved'
        WHEN c.Uptime < p.Uptime THEN '↓ Declined'
        ELSE '→ No change'
    END AS Trend
FROM CurrentMonth c, PreviousMonth p;
```

**Result:**
| CurrentUptime | PreviousUptime | Change | Trend |
|---------------|----------------|--------|-------|
| 98.5% | 95.0% | +3.5% | ↑ Improved |

---

#### Example 3: Export to Excel

The system generates an Excel file matching your current manual format:

```
┌──────────────────────────────────────────────────────────┐
│ KTDA - Region 1 Monthly Report - October 2025           │
├──────────────────────────────────────────────────────────┤
│                                                          │
│ Factory         | Computers | Uptime | Tickets | Status │
│ ──────────────────────────────────────────────────────   │
│ Kambaa          | 45        | 98.5%  | 23      | ✓      │
│ Tbesonik        | 52        | 97.2%  | 31      | ✓      │
│ Kangaita        | 38        | 99.1%  | 18      | ✓      │
│ ...             | ...       | ...    | ...     | ...    │
│                                                          │
│ REGION TOTAL    | 675       | 98.2%  | 345     |        │
│                                                          │
│ ┌────────────────────────────────────────────────┐      │
│ │ [Chart: Network Uptime Trend]                  │      │
│ │ Sep: 95% ━━━━━━━━━━━━━━━━━━━━━━━                      │
│ │ Oct: 98% ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━               │
│ └────────────────────────────────────────────────┘      │
└──────────────────────────────────────────────────────────┘
```

**Implementation:** Uses EPPlus or ClosedXML library to generate Excel files programmatically.

---

## 12. FAQs

### For Non-Technical Users:

**Q1: What happens if I lose internet connection while filling a form?**
**A:** Your progress is auto-saved every 30 seconds as a draft. When you reconnect, you can continue where you left off.

---

**Q2: Can I edit a submission after I submit it?**
**A:** No, once submitted, only your regional manager can approve or reject it. If rejected, you can then edit and resubmit.

---

**Q3: How do I know if my submission was approved?**
**A:** You'll receive:
- Email notification
- Notification badge in your dashboard when you log in
- Status changes to "Approved" in your submission history

---

**Q4: What if I made a mistake after my submission was approved?**
**A:** Approved submissions cannot be edited (data integrity). Contact your regional manager or Head Office to discuss options.

---

**Q5: Can I fill out checklists offline?**
**A:** No, the system requires internet connection. However, the system is lightweight and works on slow connections.

---

**Q6: What if I don't finish filling a checklist by the deadline?**
**A:** You'll see reminders in your dashboard. Late submissions are tracked, but it's better to submit on time for accurate reporting.

---

**Q7: Can I compare this month's data with previous months?**
**A:** Yes, when filling forms, you can view previous submissions. Managers can also compare data during review.

---

**Q8: Who can see my submissions?**
**A:** Only:
- You (the submitter)
- Your regional ICT manager
- Head Office staff (for consolidated reports)

Other factories cannot see your data.

---

### For Developers:

**Q1: Why use EAV pattern instead of fixed columns?**
**A:** Flexibility. Admin can add new questions without database migration. Forms can evolve without code changes.

---

**Q2: How do you handle performance with EAV queries?**
**A:**
- Index on `SubmissionId` and `ChecklistItemId`
- Pre-aggregate common metrics in fact tables
- Use query optimization (avoid N+1 queries)
- Cache template definitions

---

**Q3: What if two users edit the same submission simultaneously?**
**A:**
- Drafts: Last write wins (show warning if modified date changed)
- Submitted/Approved: Immutable, no conflicts possible

---

**Q4: How do you enforce validation rules stored as JSON?**
**A:**
- Client-side: jQuery Validation reads JSON and applies rules
- Server-side: ValidationService parses JSON and validates using FluentValidation

---

**Q5: How do you handle template versioning?**
**A:** Each submission stores `TemplateId` + `TemplateVersion`. Historical data references old version, new submissions use latest version.

---

**Q6: How do you implement conditional logic (show/hide fields)?**
**A:** JavaScript reads `ConditionalLogic` JSON from `ChecklistTemplateItems`:
```javascript
{
  "showIf": {
    "itemId": 14,
    "operator": "equals",
    "value": true
  }
}
```
When ItemId 14 changes, evaluate condition and show/hide dependent field.

---

**Q7: How do you prevent SQL injection in EAV queries?**
**A:** Always use parameterized queries via Entity Framework Core. Never concatenate user input into SQL strings.

---

**Q8: How do you backup/restore checklist data?**
**A:** Standard SQL Server backups include all four tables. For disaster recovery, restore database to any point in time.

---

**Q9: Can we export checklist data to external systems?**
**A:** Yes, create API endpoints that return JSON:
```
GET /api/submissions?status=approved&period=2025-10
```
Returns all approved submissions for October 2025.

---

**Q10: How do you test the dynamic form rendering?**
**A:**
- Unit tests: Test `ChecklistService.RenderForm()` with mock data
- Integration tests: Create template, submit form, verify database
- UI tests: Selenium/Playwright to fill forms and submit

---

## Summary

The **Checklist System** is the core of the KTDA ICT Reporting System because:

1. ✅ **Replaces manual Excel/Word workflows** with structured database-driven forms
2. ✅ **Dynamic and flexible** - admin can create new forms without code changes
3. ✅ **Enforces validation** - prevents bad data from entering the system
4. ✅ **Automates approval workflow** - managers review and approve online with audit trail
5. ✅ **Enables real-time reporting** - no more manual consolidation of Excel files
6. ✅ **Preserves historical data** - all submissions are searchable and comparable over time

**Key Innovation:** The EAV pattern allows the system to handle any type of checklist (daily, monthly, ad-hoc) without predefined schema, giving KTDA maximum flexibility for future reporting needs.

**Implementation Priority:** This system should be implemented in **Phase 2 (Weeks 5-9)** immediately after the foundation (auth + multi-tenancy), as all operational data originates from checklist submissions.

---

**Document Version:** 1.0
**Last Updated:** October 29, 2025
**Maintained By:** KTDA ICT Development Team
**Questions?** Contact system administrator or refer to [ImplementationPlan.md](../ImplementationPlan.md)

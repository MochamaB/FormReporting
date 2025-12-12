# Form Submission Views Specification

## Overview

The Submissions module requires **6 views** that work together to provide a complete form submission experience. These views leverage the existing form rendering components (`_Form.cshtml`, `_FormWizard.cshtml`, `_FormSection.cshtml`, `_FormField.cshtml`) already built for the Form Builder preview.

---

## 1. Index View

**File:** `Views/Submissions/Index.cshtml`

### Purpose
Dashboard showing user's submissions with filtering, statistics, and quick actions.

### UI Structure
```
┌─────────────────────────────────────────────────────────────────────┐
│ Page Header                                                          │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ Title: "My Submissions"              [+ New Submission] button  │ │
│ │ Breadcrumb: Dashboard > Submissions                              │ │
│ └─────────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ Stat Cards Row (4 cards)                                            │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐                │
│ │ Total    │ │ Drafts   │ │ Submitted│ │ Approved │                │
│ │ 24       │ │ 5        │ │ 12       │ │ 7        │                │
│ │ 📊       │ │ 📝       │ │ ✅       │ │ ✓✓       │                │
│ └──────────┘ └──────────┘ └──────────┘ └──────────┘                │
├─────────────────────────────────────────────────────────────────────┤
│ Filter Bar                                                          │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ [Search...        ] [Status ▼] [Date Range ▼]    [Clear Filters]│ │
│ └─────────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ Submissions Table                                                   │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ Form Name    │ Status  │ Period    │ Last Saved │ Actions       │ │
│ ├─────────────────────────────────────────────────────────────────┤ │
│ │ Monthly Rep  │ 🟡 Draft│ Dec 2025  │ 2 hrs ago  │ [Resume][Del] │ │
│ │ Safety Audit │ 🟢 Subm │ Nov 2025  │ Dec 1      │ [View]        │ │
│ │ Inventory    │ 🔵 Appr │ Oct 2025  │ Nov 15     │ [View][Print] │ │
│ └─────────────────────────────────────────────────────────────────┘ │
│ Pagination: [< Prev] [1] [2] [3] [Next >]                          │
└─────────────────────────────────────────────────────────────────────┘
```

### Data Required
| Property | Type | Source |
|----------|------|--------|
| Model | `List<FormTemplateSubmission>` | Controller |
| `ViewBag.TotalSubmissions` | int | Controller |
| `ViewBag.DraftCount` | int | Controller |
| `ViewBag.SubmittedCount` | int | Controller |
| `ViewBag.InApprovalCount` | int | Controller |
| `ViewBag.ApprovedCount` | int | Controller |
| `ViewBag.AvailableTemplates` | `List<FormTemplate>` | Controller |
| `ViewBag.CurrentStatus` | string | Controller |
| `ViewBag.CurrentSearch` | string | Controller |

### Key Features
- Status badges with colors:
  - Draft = Yellow (`bg-warning`)
  - Submitted = Green (`bg-success`)
  - InApproval = Blue (`bg-info`)
  - Approved = Teal (`bg-primary`)
  - Rejected = Red (`bg-danger`)
- Action buttons based on status:
  - Draft: Resume, Delete
  - Submitted/InApproval: View
  - Approved: View, Print
- Delete confirmation modal for drafts
- Empty state when no submissions
- "New Submission" dropdown with available templates

### Table Columns
| Column | Description |
|--------|-------------|
| Form Name | Template name with link |
| Tenant | Tenant name (if applicable) |
| Status | Colored badge |
| Reporting Period | Month/Year format |
| Last Saved | Relative time (e.g., "2 hours ago") |
| Submitted Date | Date if submitted |
| Actions | Context-appropriate buttons |

---

## 2. Available Forms View

**File:** `Views/Submissions/AvailableForms.cshtml`

### Purpose
Catalog of form templates the user can fill out, organized by category.

### UI Structure
```
┌─────────────────────────────────────────────────────────────────────┐
│ Page Header                                                          │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ Title: "Available Forms"                                         │ │
│ │ Subtitle: "Select a form to start a new submission"              │ │
│ └─────────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ Filter Bar                                                          │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ [Search forms...      ] [Category ▼]                             │ │
│ └─────────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ Form Cards Grid (3 columns)                                         │
│ ┌───────────────────┐ ┌───────────────────┐ ┌───────────────────┐  │
│ │ 📋 Monthly Report │ │ 🔒 Safety Audit   │ │ 📦 Inventory      │  │
│ │                   │ │                   │ │                   │  │
│ │ Category: Reports │ │ Category: Audit   │ │ Category: Ops     │  │
│ │ Version: 2.0      │ │ Version: 1.5      │ │ Version: 1.0      │  │
│ │                   │ │                   │ │                   │  │
│ │ Description text  │ │ Description text  │ │ Description text  │  │
│ │ goes here...      │ │ goes here...      │ │ goes here...      │  │
│ │                   │ │                   │ │                   │  │
│ │ [Start Form →]    │ │ [Start Form →]    │ │ [Start Form →]    │  │
│ └───────────────────┘ └───────────────────┘ └───────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### Data Required
| Property | Type | Source |
|----------|------|--------|
| Model | `List<FormTemplate>` | Controller |
| `ViewBag.Categories` | `List<string>` | Controller |
| `ViewBag.CurrentCategory` | string | Controller |
| `ViewBag.CurrentSearch` | string | Controller |

### Key Features
- Card-based layout (Bootstrap grid, 3 columns on desktop)
- Template icon based on category or type
- Category badge
- Version indicator
- "Requires Approval" badge if `RequiresApproval = true`
- Description truncated with ellipsis
- Click card or button to navigate to Start action
- Search filters by name, description, code
- Category dropdown filter
- Empty state when no templates available

### Card Structure
```html
<div class="card template-card">
    <div class="card-body">
        <div class="d-flex align-items-center mb-3">
            <div class="avatar-sm me-3">
                <span class="avatar-title bg-primary-subtle rounded">
                    <i class="ri-file-list-3-line text-primary"></i>
                </span>
            </div>
            <div>
                <h5 class="card-title mb-1">Template Name</h5>
                <span class="badge bg-secondary">Category</span>
                <span class="badge bg-info">v2.0</span>
            </div>
        </div>
        <p class="card-text text-muted">Description...</p>
        <a href="/Submissions/Start/{id}" class="btn btn-primary">
            Start Form <i class="ri-arrow-right-line"></i>
        </a>
    </div>
</div>
```

---

## 3. Submit Form View

**File:** `Views/Submissions/SubmitForm.cshtml`

### Purpose
Main form entry view - renders the dynamic form for user input.

### UI Structure
```
┌─────────────────────────────────────────────────────────────────────┐
│ Form Header                                                          │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ Title: "Monthly Factory Report"                                  │ │
│ │ Description: "Submit your monthly production metrics"            │ │
│ │ Auto-save: ✓ Saved at 8:30 PM                    [Cancel]       │ │
│ └─────────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ Progress Bar (if wizard mode)                                       │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ ●───────●───────○───────○                                       │ │
│ │ Basic    Production  Quality   Submit                           │ │
│ │ Info     Data        Metrics   Review                           │ │
│ └─────────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ Form Content (uses _Form.cshtml or _FormWizard.cshtml)              │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ Section: Basic Information                                       │ │
│ │ ┌─────────────────────────────────────────────────────────────┐ │ │
│ │ │ Factory Name *          │ Reporting Period *                │ │ │
│ │ │ [___________________]   │ [December 2025 ▼]                 │ │ │
│ │ └─────────────────────────────────────────────────────────────┘ │ │
│ │ ┌─────────────────────────────────────────────────────────────┐ │ │
│ │ │ Manager Name *                                               │ │ │
│ │ │ [___________________]                                        │ │ │
│ │ └─────────────────────────────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ Form Actions                                                        │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │              [Save Draft]  [← Previous]  [Next →]               │ │
│ │              (or [Submit Form] on last step)                    │ │
│ └─────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘
```

### Data Required
| Property | Type | Source |
|----------|------|--------|
| Model | `FormViewModel` | Controller |
| `ViewBag.SubmissionId` | int | Controller |
| `ViewBag.TemplateId` | int | Controller |
| `ViewBag.TenantId` | int? | Controller |
| `ViewBag.ReportingPeriod` | string | Controller |
| `ViewBag.IsNewSubmission` | bool | Controller |
| `ViewBag.LastSavedDate` | string | Controller (for resume) |

### Key Features

#### Auto-Save
- Timer runs every 30 seconds
- Visual indicator: "Saving...", "Saved ✓", or error state
- Collects all form field values
- POST to `/api/submissions/auto-save`
- Skip if no changes since last save

#### Wizard Mode (multi-section forms)
- Horizontal stepper showing all sections
- Only current section visible
- Previous/Next navigation buttons
- Validate current section before allowing Next
- Submit button appears on last step
- Progress saved on step change

#### Single Page Mode (single section forms)
- All sections visible
- Collapsible sections (optional)
- Submit button at bottom

#### Field Validation
- Real-time validation on blur
- Error messages displayed inline below field
- Invalid fields highlighted with red border
- Section validation before wizard navigation

#### Conditional Logic
- Fields show/hide based on other field values
- Parsed from `data-conditional` attribute
- Re-evaluated on any field change
- Supports: equals, notEquals, contains, greaterThan, lessThan, isEmpty, isNotEmpty

#### Unsaved Changes Warning
- Track form dirty state
- Warn on page navigation/close if unsaved changes
- Disable warning after successful save

### Reuses Components
| Component | Path |
|-----------|------|
| `_Form.cshtml` | `Views/Shared/Components/Form/` |
| `_FormWizard.cshtml` | `Views/Shared/Components/Form/` |
| `_FormSection.cshtml` | `Views/Shared/Components/Form/` |
| `_FormField.cshtml` | `Views/Shared/Components/Form/` |
| `_*Field.cshtml` | `Views/Shared/Components/Form/Fields/` |

### Hidden Fields Required
```html
<input type="hidden" name="submissionId" value="@Model.SubmissionId" />
<input type="hidden" name="templateId" value="@Model.TemplateId" />
<input type="hidden" name="tenantId" value="@Model.TenantId" />
<input type="hidden" name="reportingPeriod" value="@Model.ReportingPeriod" />
<input type="hidden" id="currentSection" name="currentSection" value="@Model.CurrentSectionIndex" />
```

---

## 4. View Submission View

**File:** `Views/Submissions/ViewSubmission.cshtml`

### Purpose
Read-only view of a submitted form with all responses displayed.

### UI Structure
```
┌─────────────────────────────────────────────────────────────────────┐
│ Header                                                               │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ Title: "Monthly Factory Report"                                  │ │
│ │ Status: ✅ Submitted on Dec 8, 2025 at 3:45 PM                  │ │
│ │ Submitted by: John Doe                                           │ │
│ │                                          [Print] [Back to List] │ │
│ └─────────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ Submission Metadata Card                                            │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ Reporting Period: December 2025    │ Tenant: Factory A          │ │
│ │ Submission ID: #12345              │ Template Version: 2.0      │ │
│ └─────────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ Form Content (Read-Only)                                            │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ Section: Basic Information                                       │ │
│ │ ┌─────────────────────────────────────────────────────────────┐ │ │
│ │ │ Factory Name              │ Reporting Period                │ │ │
│ │ │ Nairobi Factory           │ December 2025                   │ │ │
│ │ └─────────────────────────────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘
```

### Data Required
| Property | Type | Source |
|----------|------|--------|
| Model | `FormViewModel` | Controller (with `IsReadOnly = true`) |
| `ViewBag.Submission` | `FormTemplateSubmission` | Controller |
| `ViewBag.SubmissionId` | int | Controller |
| `ViewBag.IsReadOnly` | bool | Controller |

### Key Features
- All fields displayed as static text (not inputs)
- Or inputs with `disabled` attribute
- Status badge with submission timestamp
- Metadata card showing:
  - Submission ID
  - Reporting Period
  - Tenant Name
  - Template Version
  - Submitted By
  - Submitted Date
- Print button → navigates to Print view
- Back to List button
- File attachments displayed as download links
- Future: Workflow status and approval history

### Read-Only Field Display Options

**Option 1: Static Text**
```html
<div class="mb-3">
    <label class="form-label text-muted">Field Name</label>
    <p class="form-control-plaintext">Field Value</p>
</div>
```

**Option 2: Disabled Input**
```html
<div class="mb-3">
    <label class="form-label">Field Name</label>
    <input type="text" class="form-control" value="Field Value" disabled />
</div>
```

---

## 5. Confirmation View

**File:** `Views/Submissions/Confirmation.cshtml`

### Purpose
Success page shown after form submission.

### UI Structure
```
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│                         ┌─────────────────┐                        │
│                         │       ✓         │                        │
│                         │   (checkmark)   │                        │
│                         └─────────────────┘                        │
│                                                                     │
│                    Form Submitted Successfully!                     │
│                                                                     │
│              Your submission has been received and is               │
│              being processed.                                       │
│                                                                     │
│              ┌─────────────────────────────────────┐               │
│              │ Submission Details                  │               │
│              ├─────────────────────────────────────┤               │
│              │ Form: Monthly Factory Report        │               │
│              │ Submission ID: #12345               │               │
│              │ Submitted: Dec 8, 2025 at 3:45 PM   │               │
│              │ Status: Awaiting Approval           │               │
│              └─────────────────────────────────────┘               │
│                                                                     │
│              [View Submission]  [Submit Another]  [Back to List]   │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Data Required
| Property | Type | Source |
|----------|------|--------|
| Model | `FormTemplateSubmission` | Controller |

### Key Features
- Large success checkmark icon (animated optional)
- Success message
- Submission details card:
  - Form name
  - Submission ID/Reference
  - Submitted date/time
  - Status (Submitted or Awaiting Approval)
- Action buttons:
  - View Submission → `/Submissions/View/{id}`
  - Submit Another → `/Submissions/AvailableForms`
  - Back to List → `/Submissions`
- Optional: "Email confirmation sent" indicator
- Optional: Next steps guidance based on workflow

### Status Messages
| Status | Message |
|--------|---------|
| Submitted | "Your submission has been received." |
| InApproval | "Your submission is awaiting approval." |

---

## 6. Print Submission View

**File:** `Views/Submissions/PrintSubmission.cshtml`

### Purpose
Print-optimized view for generating PDF or physical printout.

### UI Structure
```
┌─────────────────────────────────────────────────────────────────────┐
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ [Company Logo]                                                   │ │
│ │                                                                  │ │
│ │ MONTHLY FACTORY REPORT                                          │ │
│ │ Submission #12345                                                │ │
│ └─────────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ Submission Information                                              │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ Submitted By: John Doe          Date: December 8, 2025          │ │
│ │ Tenant: Nairobi Factory         Period: December 2025           │ │
│ │ Status: Submitted               Version: 2.0                    │ │
│ └─────────────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ Section 1: Basic Information                                        │
│ ─────────────────────────────────────────────────────────────────── │
│ Factory Name: Nairobi Factory                                       │
│ Manager Name: John Doe                                              │
│ Reporting Period: December 2025                                     │
│                                                                     │
│ Section 2: Production Data                                          │
│ ─────────────────────────────────────────────────────────────────── │
│ Total Units Produced: 15,000                                        │
│ Defect Rate: 2.5%                                                   │
│ ...                                                                 │
├─────────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ Signature: ________________    Date: ________________           │ │
│ └─────────────────────────────────────────────────────────────────┘ │
│                                                                     │
│ Page 1 of 2                                    Generated: 8/12/2025 │
└─────────────────────────────────────────────────────────────────────┘
```

### Data Required
| Property | Type | Source |
|----------|------|--------|
| Model | `FormViewModel` | Controller (with `IsReadOnly = true`) |
| `ViewBag.Submission` | `FormTemplateSubmission` | Controller |
| `ViewBag.IsPrintView` | bool | Controller |

### Key Features
- Clean, minimal layout optimized for printing
- Company branding/logo at top
- No navigation elements (hidden via CSS)
- No buttons or interactive elements
- Page breaks between sections (CSS `page-break-before`)
- Footer with page numbers
- Signature line (optional, configurable)
- Print media CSS styles
- Auto-print dialog on load (optional via JavaScript)

### Print CSS Requirements
```css
@media print {
    /* Hide non-print elements */
    .no-print, .navbar, .sidebar, .footer, .btn {
        display: none !important;
    }
    
    /* Page setup */
    @page {
        size: A4;
        margin: 2cm;
    }
    
    /* Section breaks */
    .print-section {
        page-break-inside: avoid;
    }
    
    .print-page-break {
        page-break-before: always;
    }
    
    /* Typography */
    body {
        font-size: 12pt;
        line-height: 1.4;
    }
    
    h1 { font-size: 18pt; }
    h2 { font-size: 14pt; }
}
```

### Layout Options
| Option | Description |
|--------|-------------|
| Separate Layout | Use `_PrintLayout.cshtml` with minimal chrome |
| Same Layout | Use `_Layout.cshtml` with print CSS hiding elements |

---

## Shared Components & Partials

### Existing Components (Reuse)

| Component | Path | Purpose |
|-----------|------|---------|
| `_Form.cshtml` | `Views/Shared/Components/Form/` | Main form container |
| `_FormWizard.cshtml` | `Views/Shared/Components/Form/` | Wizard mode rendering |
| `_FormSection.cshtml` | `Views/Shared/Components/Form/` | Section rendering |
| `_FormField.cshtml` | `Views/Shared/Components/Form/` | Field type router |
| `_TextField.cshtml` | `Views/Shared/Components/Form/Fields/` | Text input |
| `_TextAreaField.cshtml` | `Views/Shared/Components/Form/Fields/` | Textarea |
| `_NumberField.cshtml` | `Views/Shared/Components/Form/Fields/` | Number input |
| `_DecimalField.cshtml` | `Views/Shared/Components/Form/Fields/` | Decimal input |
| `_DateField.cshtml` | `Views/Shared/Components/Form/Fields/` | Date picker |
| `_TimeField.cshtml` | `Views/Shared/Components/Form/Fields/` | Time picker |
| `_DateTimeField.cshtml` | `Views/Shared/Components/Form/Fields/` | DateTime picker |
| `_DropdownField.cshtml` | `Views/Shared/Components/Form/Fields/` | Select dropdown |
| `_RadioField.cshtml` | `Views/Shared/Components/Form/Fields/` | Radio buttons |
| `_CheckboxField.cshtml` | `Views/Shared/Components/Form/Fields/` | Checkboxes |
| `_MultiSelectField.cshtml` | `Views/Shared/Components/Form/Fields/` | Multi-select |
| `_FileUploadField.cshtml` | `Views/Shared/Components/Form/Fields/` | File upload |
| `_ImageField.cshtml` | `Views/Shared/Components/Form/Fields/` | Image upload |
| `_SignatureField.cshtml` | `Views/Shared/Components/Form/Fields/` | Signature pad |
| `_RatingField.cshtml` | `Views/Shared/Components/Form/Fields/` | Star rating |
| `_SliderField.cshtml` | `Views/Shared/Components/Form/Fields/` | Range slider |
| `_EmailField.cshtml` | `Views/Shared/Components/Form/Fields/` | Email input |
| `_PhoneField.cshtml` | `Views/Shared/Components/Form/Fields/` | Phone input |
| `_UrlField.cshtml` | `Views/Shared/Components/Form/Fields/` | URL input |
| `_CurrencyField.cshtml` | `Views/Shared/Components/Form/Fields/` | Currency input |
| `_PercentageField.cshtml` | `Views/Shared/Components/Form/Fields/` | Percentage input |

### New Partials to Create

| Partial | Path | Purpose |
|---------|------|---------|
| `_SubmissionRow.cshtml` | `Views/Submissions/Partials/` | Table row for submission |
| `_TemplateCard.cshtml` | `Views/Submissions/Partials/` | Card for available template |
| `_StatusBadge.cshtml` | `Views/Submissions/Partials/` | Colored status badge |
| `_AutoSaveIndicator.cshtml` | `Views/Submissions/Partials/` | Auto-save status UI |
| `_SubmissionMetadata.cshtml` | `Views/Submissions/Partials/` | Metadata card |

---

## Status Badge Colors

| Status | Bootstrap Class | Color |
|--------|-----------------|-------|
| Draft | `bg-warning` | Yellow |
| Submitted | `bg-success` | Green |
| InApproval | `bg-info` | Blue |
| Approved | `bg-primary` | Teal/Primary |
| Rejected | `bg-danger` | Red |
| Cancelled | `bg-secondary` | Gray |

### Badge HTML
```html
@{
    var badgeClass = Model.Status switch
    {
        "Draft" => "bg-warning",
        "Submitted" => "bg-success",
        "InApproval" => "bg-info",
        "Approved" => "bg-primary",
        "Rejected" => "bg-danger",
        _ => "bg-secondary"
    };
}
<span class="badge @badgeClass">@Model.Status</span>
```

---

## Implementation Order

| Priority | View | Reason |
|----------|------|--------|
| 1 | `Index.cshtml` | Most frequently accessed |
| 2 | `SubmitForm.cshtml` | Core functionality |
| 3 | `Confirmation.cshtml` | Simple, needed for submit flow |
| 4 | `ViewSubmission.cshtml` | View submitted forms |
| 5 | `AvailableForms.cshtml` | Template catalog |
| 6 | `PrintSubmission.cshtml` | Lowest priority |

---

## JavaScript Dependencies (Phase 6)

| File | Purpose |
|------|---------|
| `form-submission.js` | Auto-save, wizard navigation, form submission |
| `conditional-logic.js` | Field show/hide logic |
| `form-validation.js` | Client-side validation |

### Existing JS to Reuse
| File | Purpose |
|------|---------|
| `form-wizard.js` | Wizard stepper functionality |

---

## CSS Requirements

### New Styles Needed
```css
/* Auto-save indicator */
.auto-save-indicator {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
}

.auto-save-indicator.saving {
    color: var(--bs-warning);
}

.auto-save-indicator.saved {
    color: var(--bs-success);
}

.auto-save-indicator.error {
    color: var(--bs-danger);
}

/* Template card hover */
.template-card {
    transition: transform 0.2s, box-shadow 0.2s;
    cursor: pointer;
}

.template-card:hover {
    transform: translateY(-4px);
    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

/* Submission status row colors */
.submission-row.status-draft {
    border-left: 3px solid var(--bs-warning);
}

.submission-row.status-submitted {
    border-left: 3px solid var(--bs-success);
}
```

---

## Accessibility Considerations

- All form fields have associated labels
- Error messages linked to fields via `aria-describedby`
- Focus management in wizard mode
- Keyboard navigation support
- Screen reader announcements for auto-save status
- Color not sole indicator of status (icons + text)

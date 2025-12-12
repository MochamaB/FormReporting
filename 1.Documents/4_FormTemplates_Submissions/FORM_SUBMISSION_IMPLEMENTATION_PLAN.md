# Form Submission Implementation Plan

## Overview

This document outlines the phased implementation plan for the Form Submission feature, which allows users to fill and submit dynamic forms.

---

## Existing Components Analysis

### Already Built ✅

| Component | Location | Purpose |
|-----------|----------|---------|
| `_FormWizard.cshtml` | `Views/Shared/Components/Form/` | Wizard wrapper with horizontal stepper, navigation buttons |
| `_FormSection.cshtml` | `Views/Shared/Components/Form/` | Section container (collapsible/non-collapsible) |
| `_FormField.cshtml` | `Views/Shared/Components/Form/` | Field type router/dispatcher |
| `_FormFieldMatrix.cshtml` | `Views/Shared/Components/Form/Layouts/` | Matrix/grid field layout |
| **21 Field Partials** | `Views/Shared/Components/Form/Fields/` | All field types implemented |
| `FormViewModel.cs` | `Models/ViewModels/Components/` | Complete ViewModel structure |

### Field Types Available (21 types)
- **Input**: Text, TextArea, Number, Decimal, Email, Phone, Url
- **Date/Time**: Date, Time, DateTime
- **Selection**: Dropdown, Radio, Checkbox, MultiSelect
- **Media**: FileUpload, Image, Signature
- **Specialized**: Currency, Percentage, Rating, Slider

---

## Design Decisions

### 1. Tenant Selection
- **Rule**: Only show tenant selector if user has access to more than one tenant
- **Implementation**: Check `UserTenantAccess` count in controller
- **UI**: If single tenant, auto-select and hide dropdown

### 2. Reporting Period Selection
Based on `TemplateType`:

| Template Type | Period Logic | UI Control |
|---------------|--------------|------------|
| **Daily** | User selects specific date | Date picker |
| **Weekly** | Calculate from last submission + 1 week, snap to Monday | Week picker (shows week range) |
| **Monthly** | Calculate from last submission + 1 month, snap to 1st | Month picker (e.g., "December 2025") |
| **Quarterly** | Calculate from last submission + 1 quarter, snap to Q start | Quarter picker (e.g., "Q4 2025") |
| **Annual** | Calculate from last submission + 1 year, snap to Jan 1 | Year picker (e.g., "2025") |
| **On Demand** | User selects From and To dates | Date range picker |

**Period Calculation Logic**:
```
1. Find last submitted (non-draft) submission for this template + tenant
2. If no previous submission:
   - Monthly: Current month 1st to last day
   - Quarterly: Current quarter start to end
   - Annual: Current year Jan 1 to Dec 31
3. If previous submission exists:
   - Calculate next period from last submission's period end date
   - Snap to appropriate boundary (1st of month, Monday, etc.)
```

### 3. Draft Resume Behavior
- **Rule**: Always resume existing draft if one exists
- **Logic**:
  1. Check for draft with same: TemplateId + TenantId + ReportingPeriod
  2. If found: Load draft and redirect to SubmitForm
  3. If not found: Create new submission with status "Draft"

### 4. Progress Stepper Style
- **Style**: Horizontal steps (as in existing `_FormWizard.cshtml`)
- **Max Visible Steps**: 6 steps before horizontal scrolling
- **Responsive**: On mobile, show current step with prev/next indicators
- **Features**:
  - Step numbers
  - Step titles
  - Completed checkmark
  - Active highlight
  - Clickable navigation (if `AllowStepSkipping = true`)

---

## Phase 1: Welcome/Start Form Page ✅ COMPLETED

### Purpose
Introduce user to the form, collect tenant and period, check for existing draft.

### Files Created
- `Views/Submissions/Start.cshtml` ✅
- `Models/ViewModels/Forms/StartSubmissionViewModel.cs` ✅

### Controller Actions Added
- `Start(int templateId)` - GET: Show welcome page ✅
- `CreateSubmission(...)` - POST: Create or resume submission ✅
- `Fill(int submissionId)` - GET: Render form for filling ✅
- `Resume(int submissionId)` - GET: Redirects to Fill ✅

### API Endpoints Added
- `GET /api/submissions/check-draft` - AJAX draft detection ✅

### Service Methods Added
- `GetTenantsAsync(List<int> tenantIds)` ✅

### UI Elements
```
┌─────────────────────────────────────────────────────────────────────┐
│ [Breadcrumb: Submissions > Available Forms > Start Form]            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  FORM HEADER CARD                                            │   │
│  │  ┌─────────┐                                                 │   │
│  │  │  Icon   │  Monthly Factory Report                         │   │
│  │  │         │  Category: Reports | Type: Monthly | v1.0       │   │
│  │  └─────────┘                                                 │   │
│  │                                                              │   │
│  │  Description text here...                                    │   │
│  │                                                              │   │
│  │  ───────────────────────────────────────────────────────    │   │
│  │  📊 3 Sections  |  📝 15 Fields  |  ⏱️ ~10 mins              │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  SUBMISSION DETAILS CARD                                     │   │
│  │                                                              │   │
│  │  Select Facility * (only if multiple tenants)               │   │
│  │  [Kericho Factory ▼]                                         │   │
│  │                                                              │   │
│  │  Reporting Period *                                          │   │
│  │  [December 2025 ▼]  (or date range for On Demand)           │   │
│  │                                                              │   │
│  │  ⚠️ Draft exists - you will continue from where you left    │   │
│  │                                                              │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  [← Back to Forms]                      [Begin Form →]       │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Draft Detection
- On tenant/period change, AJAX check for existing draft
- Show alert if draft exists: "A draft exists for this period. You will continue from where you left off."

---

## Phase 2: Wizard Shell & Navigation

### Purpose
Create the main SubmitForm view that wraps the wizard component.

### Files to Create/Modify
- `Views/Submissions/SubmitForm.cshtml` - Main view
- Modify `_FormWizard.cshtml` - Add scrollable steps support

### Wizard Enhancements

#### Scrollable Steps (Max 6 visible)
```css
.nav-wizard {
    display: flex;
    overflow-x: auto;
    scroll-snap-type: x mandatory;
    -webkit-overflow-scrolling: touch;
}

.nav-wizard .nav-item {
    flex: 0 0 auto;
    min-width: 120px;
    max-width: 150px;
    scroll-snap-align: start;
}

/* Show scroll indicators when needed */
.nav-wizard-container {
    position: relative;
}

.nav-wizard-container.has-scroll-left::before,
.nav-wizard-container.has-scroll-right::after {
    /* Fade gradient indicators */
}
```

#### Auto-Save Status in Header
```html
<div class="d-flex justify-content-between align-items-center mb-3">
    <h4 class="mb-0">@Model.Title</h4>
    <div class="d-flex align-items-center gap-3">
        <span id="auto-save-status" class="text-muted small">
            <i class="ri-check-line text-success"></i> Saved at 8:30 PM
        </span>
        <button type="button" class="btn btn-soft-danger btn-sm" onclick="cancelForm()">
            <i class="ri-close-line"></i> Cancel
        </button>
    </div>
</div>
```

### JavaScript Functionality
```javascript
// form-submission.js

class FormWizard {
    constructor(formId) {
        this.formId = formId;
        this.currentStep = 0;
        this.totalSteps = 0;
        this.init();
    }
    
    init() {
        this.bindNavigation();
        this.updateProgress();
        this.checkScrollIndicators();
    }
    
    goToStep(stepIndex) {
        // Hide current, show target
        // Update progress bar
        // Update step states
        // Scroll step into view if needed
    }
    
    nextStep() {
        if (this.validateCurrentStep()) {
            this.goToStep(this.currentStep + 1);
        }
    }
    
    previousStep() {
        this.goToStep(this.currentStep - 1);
    }
    
    validateCurrentStep() {
        // Client-side validation for current section
        return true; // For Phase 2, always return true
    }
    
    checkScrollIndicators() {
        // Show/hide scroll fade indicators
    }
}
```

---

## Phase 3: Section Loading

### Purpose
Load actual sections from the template and display section headers with placeholder content.

### Service Method
```csharp
// FormSubmissionService.cs
public async Task<FormViewModel> BuildSubmissionFormAsync(int submissionId)
{
    // Load submission with template, sections, fields
    // Build FormViewModel with all sections
    // Populate CurrentValue from existing responses
    // Return complete FormViewModel
}
```

### Section Display
Each section shows:
- Section title and description
- Field count badge
- Placeholder: "This section contains X fields"

### Test Navigation
- Verify Previous/Next buttons work
- Verify step clicking works (if enabled)
- Verify progress bar updates
- Verify step states (completed, active, pending)

---

## Phase 4: Field Rendering

### Purpose
Render actual form fields within each section.

### Implementation
- Use existing `_FormSection.cshtml` which already calls `_FormField.cshtml`
- Use existing field partials in `Views/Shared/Components/Form/Fields/`
- Apply field configurations (readonly, disabled, prefix/suffix)
- Populate `CurrentValue` from draft responses

### Field Name Convention
```html
<!-- Input name format for form POST -->
<input name="responses[@Model.FieldId]" value="@Model.CurrentValue" />
```

### Response Loading
```csharp
// Load existing responses for draft
var responses = await _context.FormSubmissionResponses
    .Where(r => r.SubmissionId == submissionId)
    .ToDictionaryAsync(r => r.ItemId, r => r.ResponseValue);

// Map to field CurrentValue
foreach (var field in section.Fields)
{
    if (responses.TryGetValue(field.FieldId, out var value))
    {
        field.CurrentValue = value;
    }
}
```

---

## Phase 5: Auto-Save, Validation & Submit

### Auto-Save Implementation
```javascript
class FormAutoSave {
    constructor(formId, saveUrl, intervalMs = 30000) {
        this.formId = formId;
        this.saveUrl = saveUrl;
        this.intervalMs = intervalMs;
        this.isDirty = false;
        this.lastSavedData = null;
        this.init();
    }
    
    init() {
        this.trackChanges();
        this.startAutoSave();
    }
    
    trackChanges() {
        // Mark form as dirty on any input change
        document.querySelectorAll(`#${this.formId} input, select, textarea`)
            .forEach(el => el.addEventListener('change', () => this.isDirty = true));
    }
    
    startAutoSave() {
        setInterval(() => {
            if (this.isDirty) {
                this.save();
            }
        }, this.intervalMs);
    }
    
    async save() {
        const formData = this.collectFormData();
        if (JSON.stringify(formData) === JSON.stringify(this.lastSavedData)) {
            return; // No changes
        }
        
        this.updateStatus('saving');
        
        try {
            const response = await fetch(this.saveUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData)
            });
            
            if (response.ok) {
                this.lastSavedData = formData;
                this.isDirty = false;
                this.updateStatus('saved');
            } else {
                this.updateStatus('error');
            }
        } catch (error) {
            this.updateStatus('error');
        }
    }
    
    updateStatus(status) {
        const statusEl = document.getElementById('auto-save-status');
        switch (status) {
            case 'saving':
                statusEl.innerHTML = '<i class="ri-loader-4-line spin"></i> Saving...';
                break;
            case 'saved':
                statusEl.innerHTML = `<i class="ri-check-line text-success"></i> Saved at ${new Date().toLocaleTimeString()}`;
                break;
            case 'error':
                statusEl.innerHTML = '<i class="ri-error-warning-line text-danger"></i> Save failed';
                break;
        }
    }
}
```

### Validation Implementation
```javascript
class FormValidator {
    validateField(fieldEl, rules) {
        const value = fieldEl.value;
        const errors = [];
        
        if (rules.required && !value) {
            errors.push(rules.requiredMessage || 'This field is required');
        }
        
        if (rules.minLength && value.length < rules.minLength) {
            errors.push(`Minimum ${rules.minLength} characters required`);
        }
        
        if (rules.maxLength && value.length > rules.maxLength) {
            errors.push(`Maximum ${rules.maxLength} characters allowed`);
        }
        
        if (rules.min && parseFloat(value) < rules.min) {
            errors.push(`Minimum value is ${rules.min}`);
        }
        
        if (rules.max && parseFloat(value) > rules.max) {
            errors.push(`Maximum value is ${rules.max}`);
        }
        
        if (rules.pattern && !new RegExp(rules.pattern).test(value)) {
            errors.push(rules.patternMessage || 'Invalid format');
        }
        
        return errors;
    }
    
    showFieldError(fieldEl, errors) {
        fieldEl.classList.add('is-invalid');
        const feedback = fieldEl.parentElement.querySelector('.invalid-feedback');
        if (feedback) {
            feedback.textContent = errors[0];
        }
    }
    
    clearFieldError(fieldEl) {
        fieldEl.classList.remove('is-invalid');
    }
}
```

### Submit Flow
```javascript
async function submitForm(formId) {
    const form = document.getElementById(formId);
    const validator = new FormValidator();
    
    // Validate all fields
    const allValid = validator.validateAllFields(form);
    
    if (!allValid) {
        // Show summary of errors
        showValidationSummary(validator.errors);
        return;
    }
    
    // Confirm submission
    const confirmed = await showConfirmDialog(
        'Submit Form',
        'Are you sure you want to submit this form? You cannot edit it after submission.'
    );
    
    if (!confirmed) return;
    
    // Submit
    form.submit();
}
```

---

## File Structure Summary

```
Views/Submissions/
├── Index.cshtml                    ✅ Exists
├── AvailableForms.cshtml           ✅ Exists
├── Start.cshtml                    📝 Phase 1
├── SubmitForm.cshtml               📝 Phase 2
├── ViewSubmission.cshtml           📝 Future
└── Partials/
    ├── _SubmissionsContent.cshtml  ✅ Exists
    ├── _AvailableFormsContent.cshtml ✅ Exists
    └── _AvailableFormCard.cshtml   ✅ Exists

Views/Shared/Components/Form/
├── _Form.cshtml                    ✅ Exists
├── _FormWizard.cshtml              ✅ Exists (enhance in Phase 2)
├── _FormSection.cshtml             ✅ Exists
├── _FormField.cshtml               ✅ Exists
├── Layouts/
│   └── _FormFieldMatrix.cshtml     ✅ Exists
└── Fields/
    └── [21 field partials]         ✅ All exist

wwwroot/js/
├── form-wizard.js                  ✅ Exists (for form builder)
└── form-submission.js              📝 Phase 2-5

Controllers/Submissions/
└── SubmissionsController.cs        ✅ Exists (extend)

Services/Forms/
└── FormSubmissionService.cs        ✅ Exists (extend)
```

---

## Implementation Timeline

| Phase | Description | Estimated Time |
|-------|-------------|----------------|
| **Phase 1** | Welcome/Start Page | 1 day |
| **Phase 2** | Wizard Shell & Navigation | 1-2 days |
| **Phase 3** | Section Loading | 0.5 day |
| **Phase 4** | Field Rendering | 1-2 days |
| **Phase 5** | Auto-Save, Validation, Submit | 2-3 days |

**Total Estimated Time**: 6-9 days

---

## Ready to Begin?

Start with **Phase 1: Welcome/Start Form Page**?

This involves:
1. Create `Start.cshtml` view
2. Add `Start` and `CreateSubmission` actions to controller
3. Implement period calculation logic
4. Add draft detection AJAX endpoint

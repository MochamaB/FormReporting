# Form Render & Submission - Implementation Plan

**Module:** Form Submissions (Section 4)  
**Priority:** HIGH - Core functionality  
**Estimated Duration:** 8-10 days  
**Last Updated:** December 8, 2025

---

## Overview

The Form Render module enables users to fill out published form templates and submit responses. It transforms database-stored form definitions into interactive HTML forms with validation, auto-save, and wizard navigation.

---

## Prerequisites

### Already Implemented ✅
- Form Builder (templates, sections, fields created)
- Field type components (21 types in `Views/Shared/Components/Form/Fields/`)
- Form container components (`_Form.cshtml`, `_FormWizard.cshtml`, `_FormSection.cshtml`, `_FormField.cshtml`)
- ViewModels (`FormViewModel`, `FormSectionViewModel`, `FormFieldViewModel`)
- Database models (`FormTemplateSubmission`, `FormTemplateResponse`)

### Needs Minor Update ⚠️
- `FormTemplateSubmission` - Add 2 columns for draft functionality

---

## Phase 1: Model Updates

### 1.1 Update FormTemplateSubmission Model
**File:** `Models/Entities/Forms/FormTemplateSubmission.cs`

**Add columns:**
- `LastSavedDate` (DateTime?, nullable) - Timestamp of last auto-save
- `CurrentSection` (int, default 0) - Current wizard step for resume

### 1.2 Update FormTemplateSubmission Configuration
**File:** `Data/Configurations/Forms/FormTemplateSubmissionConfiguration.cs`

**Add:**
- Index on (SubmittedBy, Status) for "My Submissions" queries
- Index on (TemplateId, Status) for template submission lists

### 1.3 Create Migration
**Migration name:** `AddDraftFieldsToFormTemplateSubmission`

---

## Phase 2: Service Layer

### 2.1 Create IFormSubmissionService Interface
**File:** `Services/Forms/IFormSubmissionService.cs`

**Methods:**
- `BuildFormViewModelAsync(templateId, submissionId?)` - Transform DB to ViewModel
- `CreateSubmissionAsync(templateId, userId, tenantId)` - Create new draft
- `GetSubmissionAsync(submissionId)` - Load existing submission
- `GetUserSubmissionsAsync(userId)` - List user's submissions
- `GetTemplateSubmissionsAsync(templateId)` - List all submissions for template
- `CanUserAccessTemplateAsync(userId, templateId)` - Check access (basic, expand later for assignments)

### 2.2 Create FormSubmissionService Implementation
**File:** `Services/Forms/FormSubmissionService.cs`

**Responsibilities:**
- Load FormTemplate with Sections, Items, Options, Validations, Configurations
- Load existing FormTemplateResponses if resuming draft
- Map database entities to ViewModels
- Parse DataType string to FormFieldType enum
- Extract response values based on data type (TextValue, NumericValue, DateValue, BooleanValue)
- Build conditional logic JSON for client-side evaluation

### 2.3 Create IFormResponseService Interface
**File:** `Services/Forms/IFormResponseService.cs`

**Methods:**
- `SaveResponsesAsync(submissionId, responses)` - Save/update responses
- `SaveDraftAsync(submissionId, responses, currentSection)` - Auto-save draft
- `SubmitAsync(submissionId)` - Finalize submission
- `ValidateResponsesAsync(submissionId)` - Server-side validation

### 2.4 Create FormResponseService Implementation
**File:** `Services/Forms/FormResponseService.cs`

**Responsibilities:**
- Upsert FormTemplateResponse records
- Determine correct value column based on DataType
- Update submission status (Draft → Submitted → InApproval)
- Validate required fields and validation rules
- Update LastSavedDate and CurrentSection for drafts

### 2.5 Register Services in DI
**File:** `Program.cs`

---

## Phase 3: API Controller

### 3.1 Create FormSubmissionsApiController
**File:** `Controllers/API/FormSubmissionsApiController.cs`

**Endpoints:**

| Method | Route | Purpose |
|--------|-------|---------|
| POST | `/api/submissions/auto-save` | Save draft (called every 30 seconds) |
| POST | `/api/submissions/{id}/upload` | Upload file attachment |
| DELETE | `/api/submissions/{id}/files/{fileId}` | Delete file attachment |
| GET | `/api/submissions/{id}/responses` | Get current responses (for resume) |
| POST | `/api/submissions/{id}/validate` | Validate before final submit |

**Auto-save request model:**
- SubmissionId (int, 0 for new)
- TemplateId (int)
- Responses (Dictionary of ItemId → Value)
- CurrentSection (int)

**Auto-save response model:**
- Success (bool)
- SubmissionId (int)
- SavedAt (DateTime)
- Errors (List, if any)

---

## Phase 4: MVC Controller

### 4.1 Create SubmissionsController
**File:** `Controllers/Submissions/SubmissionsController.cs`

**Actions:**

| Action | Route | Purpose |
|--------|-------|---------|
| Index | GET `/Submissions` | List user's submissions |
| Start | GET `/Submissions/Start/{templateId}` | Start new submission |
| Resume | GET `/Submissions/Resume/{submissionId}` | Resume draft |
| Submit | POST `/Submissions/Submit` | Final submission |
| View | GET `/Submissions/View/{submissionId}` | Read-only view |
| Confirmation | GET `/Submissions/Confirmation/{submissionId}` | Success page |

### 4.2 Controller Logic Flow

**Start Action:**
1. Verify template exists and is published
2. Check user has access (basic check, expand for assignments later)
3. Create new FormTemplateSubmission (Status = "Draft")
4. Build FormViewModel from template
5. Return SubmitForm view

**Resume Action:**
1. Load existing submission
2. Verify user owns submission
3. Verify status is "Draft"
4. Build FormViewModel with existing responses
5. Set CurrentSectionIndex from saved value
6. Return SubmitForm view

**Submit Action:**
1. Validate all required fields
2. Run server-side validation rules
3. Update status to "Submitted" (or "InApproval" if RequiresApproval)
4. Set SubmittedDate
5. Redirect to Confirmation

**View Action:**
1. Load submission with responses
2. Build FormViewModel in read-only mode
3. Return ViewSubmission view

---

## Phase 5: Views

### 5.1 Create Submissions Index View
**File:** `Views/Submissions/Index.cshtml`

**Content:**
- DataTable listing user's submissions
- Columns: Form Name, Status, Submitted Date, Actions
- Actions: Resume (if Draft), View, Delete (if Draft)
- Filter by status
- Link to available forms

### 5.2 Create SubmitForm View
**File:** `Views/Submissions/SubmitForm.cshtml`

**Content:**
- Uses existing `_Form.cshtml` or `_FormWizard.cshtml` partial
- Header with template name, description
- Resume indicator (if resuming draft)
- Auto-save status indicator
- Form rendered via partial components

### 5.3 Create ViewSubmission View
**File:** `Views/Submissions/View.cshtml`

**Content:**
- Read-only form display
- All fields disabled/readonly
- Submission metadata (submitted by, date, status)
- Approval history (if applicable)
- Print button

### 5.4 Create Confirmation View
**File:** `Views/Submissions/Confirmation.cshtml`

**Content:**
- Success message
- Submission reference number
- Summary of what was submitted
- Next steps (approval pending, etc.)
- Links: View Submission, Submit Another, Back to Dashboard

### 5.5 Create Available Forms View
**File:** `Views/Submissions/AvailableForms.cshtml`

**Content:**
- List of published templates user can access
- Card layout with template name, description, category
- "Start" button for each
- Filter by category

---

## Phase 6: JavaScript

### 6.1 Create form-submission.js
**File:** `wwwroot/js/pages/form-submission.js`

**Features:**

**Auto-Save:**
- Timer runs every 30 seconds
- Collects all form field values
- Compares with last saved data (skip if unchanged)
- POST to `/api/submissions/auto-save`
- Update UI indicator (Saving... → Saved ✓)
- Handle errors with retry

**Wizard Navigation:**
- Track current step
- Validate current section before next
- Show/hide step content
- Update progress bar
- Update stepper UI (active, completed states)
- Save draft on step change

**Form Validation:**
- Real-time validation on blur
- Section validation before navigation
- Full form validation before submit
- Display validation errors inline

**Conditional Logic:**
- Parse conditional logic JSON from data attributes
- Evaluate rules on field change
- Show/hide/enable/disable fields based on rules
- Re-evaluate all conditions on page load

**Unsaved Changes Warning:**
- Track if form has unsaved changes
- Warn on page navigation/close
- Clear warning after successful save

### 6.2 Create form-validation.js
**File:** `wwwroot/js/pages/form-validation.js`

**Features:**
- Required field validation
- Min/Max length validation
- Min/Max value validation
- Range validation
- Regex pattern validation
- Email format validation
- Custom expression evaluation
- Error message display

### 6.3 Create conditional-logic.js
**File:** `wwwroot/js/pages/conditional-logic.js`

**Features:**
- Parse JSON conditional logic
- Evaluate operators: equals, notEquals, contains, greaterThan, lessThan, isEmpty, isNotEmpty
- Support AND/OR logic combinations
- Actions: show, hide, enable, disable
- Cascade evaluation (field A affects B, B affects C)

---

## Phase 7: File Upload Handling

### 7.1 Create File Upload Infrastructure
**Folder:** `wwwroot/uploads/submissions/{submissionId}/`

### 7.2 Create FormSubmissionAttachment Model (if not exists)
**File:** `Models/Entities/Forms/FormSubmissionAttachment.cs`

**Columns:**
- AttachmentId (PK)
- SubmissionId (FK)
- ItemId (FK)
- FileName (original name)
- UniqueFileName (stored name)
- FilePath
- FileSize
- ContentType
- UploadedDate
- UploadedBy

### 7.3 File Upload API Endpoints
- Validate file type against field configuration
- Validate file size against limits
- Generate unique filename
- Save to disk
- Create database record
- Return attachment info

### 7.4 File Delete API Endpoint
- Verify ownership
- Delete physical file
- Delete database record

---

## Phase 8: Menu & Navigation

### 8.1 Add Menu Items
**Update:** `Data/Seeders/MenuItemSeeder.cs`

**New menu items under "Forms" section:**
- My Submissions
- Available Forms

### 8.2 Update Breadcrumbs
**Update:** Breadcrumb configuration for Submissions controller

---

## Phase 9: Testing & Validation

### 9.1 Manual Testing Checklist

**Start New Submission:**
- [ ] Can start submission for published template
- [ ] Cannot start for unpublished template
- [ ] Form renders correctly (single page and wizard)
- [ ] All field types render properly
- [ ] Required fields marked with asterisk
- [ ] Help text displays correctly

**Auto-Save:**
- [ ] Draft saves automatically every 30 seconds
- [ ] Save indicator shows status
- [ ] Can close and resume draft
- [ ] Resumes at correct wizard step
- [ ] All responses restored correctly

**Validation:**
- [ ] Required fields validated
- [ ] Min/Max length enforced
- [ ] Min/Max value enforced
- [ ] Regex patterns work
- [ ] Error messages display correctly
- [ ] Cannot submit invalid form

**Conditional Logic:**
- [ ] Fields show/hide based on conditions
- [ ] Fields enable/disable based on conditions
- [ ] Cascading conditions work
- [ ] Conditions re-evaluate on value change

**File Upload:**
- [ ] Can upload allowed file types
- [ ] Rejects disallowed file types
- [ ] Enforces file size limits
- [ ] Can delete uploaded files
- [ ] Files persist after save

**Submission:**
- [ ] Can submit completed form
- [ ] Status changes to Submitted
- [ ] Confirmation page displays
- [ ] Cannot edit after submission
- [ ] Can view submitted form (read-only)

**My Submissions:**
- [ ] Lists all user's submissions
- [ ] Shows correct status
- [ ] Can filter by status
- [ ] Can resume drafts
- [ ] Can view completed submissions

---

## Implementation Checklist

### Phase 1: Model Updates
- [ ] Add LastSavedDate to FormTemplateSubmission
- [ ] Add CurrentSection to FormTemplateSubmission
- [ ] Update configuration with indexes
- [ ] Create and run migration

### Phase 2: Service Layer
- [ ] Create IFormSubmissionService interface
- [ ] Create FormSubmissionService implementation
- [ ] Create IFormResponseService interface
- [ ] Create FormResponseService implementation
- [ ] Register services in DI

### Phase 3: API Controller
- [ ] Create FormSubmissionsApiController
- [ ] Implement auto-save endpoint
- [ ] Implement file upload endpoint
- [ ] Implement file delete endpoint
- [ ] Implement validate endpoint

### Phase 4: MVC Controller
- [ ] Create SubmissionsController
- [ ] Implement Index action
- [ ] Implement Start action
- [ ] Implement Resume action
- [ ] Implement Submit action
- [ ] Implement View action
- [ ] Implement Confirmation action

### Phase 5: Views
- [ ] Create Index.cshtml
- [ ] Create SubmitForm.cshtml
- [ ] Create View.cshtml
- [ ] Create Confirmation.cshtml
- [ ] Create AvailableForms.cshtml

### Phase 6: JavaScript
- [ ] Create form-submission.js
- [ ] Implement auto-save functionality
- [ ] Implement wizard navigation
- [ ] Create form-validation.js
- [ ] Create conditional-logic.js

### Phase 7: File Upload
- [ ] Create upload folder structure
- [ ] Create FormSubmissionAttachment model (if needed)
- [ ] Implement file upload logic
- [ ] Implement file delete logic

### Phase 8: Menu & Navigation
- [ ] Add menu items
- [ ] Update breadcrumbs

### Phase 9: Testing
- [ ] Complete manual testing checklist
- [ ] Fix any issues found

---

## Dependencies

| Dependency | Status | Notes |
|------------|--------|-------|
| Form Builder | ✅ Complete | Templates can be created |
| Field Components | ✅ Complete | All 21 types exist |
| Form ViewModels | ✅ Complete | FormViewModel, etc. exist |
| FormTemplateSubmission | ✅ Exists | Needs 2 new columns |
| FormTemplateResponse | ✅ Exists | Ready to use |

---

## Future Enhancements (Not in This Phase)

- **Assignment Integration** - Check assignments before allowing submission
- **Workflow Integration** - Trigger workflows after submission
- **Metric Population** - Populate metrics from responses
- **Pre-fill Service** - Auto-populate from inventory data
- **Offline Support** - Save locally when offline
- **Bulk Operations** - Submit multiple forms at once
- **Import/Export** - Import responses from Excel

---

## Files to Create/Modify Summary

### New Files
| File | Type |
|------|------|
| `Services/Forms/IFormSubmissionService.cs` | Interface |
| `Services/Forms/FormSubmissionService.cs` | Service |
| `Services/Forms/IFormResponseService.cs` | Interface |
| `Services/Forms/FormResponseService.cs` | Service |
| `Controllers/API/FormSubmissionsApiController.cs` | API Controller |
| `Controllers/Submissions/SubmissionsController.cs` | MVC Controller |
| `Views/Submissions/Index.cshtml` | View |
| `Views/Submissions/SubmitForm.cshtml` | View |
| `Views/Submissions/View.cshtml` | View |
| `Views/Submissions/Confirmation.cshtml` | View |
| `Views/Submissions/AvailableForms.cshtml` | View |
| `wwwroot/js/pages/form-submission.js` | JavaScript |
| `wwwroot/js/pages/form-validation.js` | JavaScript |
| `wwwroot/js/pages/conditional-logic.js` | JavaScript |

### Modified Files
| File | Changes |
|------|---------|
| `Models/Entities/Forms/FormTemplateSubmission.cs` | Add 2 columns |
| `Data/Configurations/Forms/FormTemplateSubmissionConfiguration.cs` | Add indexes |
| `Program.cs` | Register new services |
| `Data/Seeders/MenuItemSeeder.cs` | Add menu items |

---

**End of Implementation Plan**

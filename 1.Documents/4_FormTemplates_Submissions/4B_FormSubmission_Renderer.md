# Form Submission: Dynamic Renderer & Data Capture

**Purpose:** Dynamically render forms from templates and capture user responses  
**Users:** All users filling out assigned forms  
**Architecture:** Reusable Field Components + Wizard + Auto-save + Validation

---

## Component Summary

| Component | Type | Purpose |
|-----------|------|---------||
| 1. Dynamic Form Renderer | Template Engine | Generate HTML from template definition |
| 2. Field Type Components | Reusable Views | Render each data type (Text, Number, Date, etc.) |
| 3. Form Submission Wizard | Multi-step Wizard | Navigate through sections |
| 4. Auto-save Service | Background Service | Save drafts every 30 seconds |
| 5. File Upload Handler | Upload Component | Handle file attachments |
| 6. Pre-fill Service | Data Loader | Auto-populate fields from inventory |
| 7. Client-Side Validation | JavaScript | Real-time validation feedback |

---

## 1. Dynamic Form Renderer

**Component Type:** Template engine that generates form HTML from database

**Reusable Components:**
- Form Wizard (if multiple sections)
- Field Type Components (one per data type)
- Validation Messages
- Progress Tracker

### Rendering Strategy

**Server-Side (C# Razor):**
```
1. Load template definition from database
2. Load existing responses (if resuming draft)
3. Group fields by section
4. Pass to view with ViewModel
5. View iterates and renders each field using appropriate component
```

**ViewModel Structure:**
```csharp
public class FormSubmissionViewModel
{
    public int SubmissionId { get; set; }
    public int AssignmentId { get; set; }
    public FormTemplate Template { get; set; }
    public List<FormSection> Sections { get; set; }
    public Dictionary<int, string> ExistingResponses { get; set; }  // ItemId → Value
    public DateTime? LastSavedDate { get; set; }
    public int CurrentSectionIndex { get; set; }
    public bool IsResuming { get; set; }
}

public class FormSection
{
    public int SectionId { get; set; }
    public string SectionName { get; set; }
    public string SectionDescription { get; set; }
    public int DisplayOrder { get; set; }
    public List<FormField> Fields { get; set; }
}

public class FormField
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; }
    public string ItemName { get; set; }  // The question
    public string DataType { get; set; }  // Text, Number, Date, Boolean, Dropdown, FileUpload
    public bool IsRequired { get; set; }
    public string PlaceholderText { get; set; }
    public string HelpText { get; set; }
    public string ConditionalLogic { get; set; }  // JSON
    public List<ValidationRule> Validations { get; set; }
    public PreFillConfig PreFill { get; set; }
    
    // Type-specific properties (populated based on DataType)
    public NumberConfig NumberConfig { get; set; }
    public DateConfig DateConfig { get; set; }
    public DropdownConfig DropdownConfig { get; set; }
    public FileUploadConfig FileUploadConfig { get; set; }
}
```

### Main View Structure

**File:** `Views/Submissions/SubmitForm.cshtml`

```razor
@model FormSubmissionViewModel

<div class="submission-container">
    <!-- Header -->
    <div class="submission-header">
        <h2>@Model.Template.TemplateName</h2>
        <span class="badge">@Model.Template.TemplateType</span>
        <p>@Model.Template.Description</p>
        
        @if (Model.IsResuming)
        {
            <div class="alert alert-info">
                ℹ️ Resuming from saved draft. Last saved: @Model.LastSavedDate.Value.ToString("MMM d, yyyy h:mm tt")
            </div>
        }
    </div>

    <!-- Progress Tracker (if multiple sections) -->
    @if (Model.Sections.Count > 1)
    {
        <partial name="_FormProgressTracker" model="Model" />
    }

    <!-- Form Content -->
    <form id="submissionForm" asp-action="SaveSubmission" method="post" enctype="multipart/form-data">
        <input type="hidden" name="SubmissionId" value="@Model.SubmissionId" />
        <input type="hidden" name="AssignmentId" value="@Model.AssignmentId" />
        <input type="hidden" name="CurrentSection" id="currentSection" value="@Model.CurrentSectionIndex" />

        @if (Model.Sections.Count > 1)
        {
            <!-- Multi-section: Render as Wizard -->
            <div class="form-wizard">
                @for (int i = 0; i < Model.Sections.Count; i++)
                {
                    var section = Model.Sections[i];
                    var isActive = i == Model.CurrentSectionIndex;
                    
                    <div class="wizard-step" data-step="@i" style="display: @(isActive ? "block" : "none")">
                        <h3>@section.SectionName</h3>
                        @if (!string.IsNullOrEmpty(section.SectionDescription))
                        {
                            <p class="text-muted">@section.SectionDescription</p>
                        }
                        
                        <div class="section-fields">
                            @foreach (var field in section.Fields.OrderBy(f => f.DisplayOrder))
                            {
                                @await Html.PartialAsync("_FormField", field, new ViewDataDictionary(ViewData)
                                {
                                    { "ExistingValue", Model.ExistingResponses.GetValueOrDefault(field.ItemId) }
                                })
                            }
                        </div>
                    </div>
                }
            </div>

            <!-- Wizard Navigation -->
            <div class="wizard-navigation">
                <button type="button" class="btn btn-secondary" id="prevBtn" onclick="navigateSection(-1)">
                    ← Previous
                </button>
                <button type="button" class="btn btn-primary" id="nextBtn" onclick="navigateSection(1)">
                    Next →
                </button>
                <button type="submit" class="btn btn-success" id="submitBtn" style="display: none;">
                    Submit Form
                </button>
            </div>
        }
        else
        {
            <!-- Single section: Render all fields -->
            var section = Model.Sections.First();
            <div class="section-fields">
                @foreach (var field in section.Fields.OrderBy(f => f.DisplayOrder))
                {
                    @await Html.PartialAsync("_FormField", field, new ViewDataDictionary(ViewData)
                    {
                        { "ExistingValue", Model.ExistingResponses.GetValueOrDefault(field.ItemId) }
                    })
                }
            </div>
            
            <div class="form-actions">
                <button type="submit" class="btn btn-success">Submit Form</button>
            </div>
        }
    </form>

    <!-- Auto-save Indicator -->
    <div class="autosave-indicator" id="autosaveIndicator">
        💾 All changes saved
    </div>
</div>

@section Scripts {
    <script src="~/js/form-submission.js"></script>
}
```

---

## 2. Field Type Components

**Reusable field components will be created separately** following the Reusable Components Architecture pattern.

### Required Components

The following field type components are needed and will be created in a separate implementation process:

**Component List:**
- `_FormField.cshtml` - Router that selects appropriate component based on DataType
- `_TextField.cshtml` - Text input rendering
- `_TextAreaField.cshtml` - Multi-line text rendering
- `_NumberField.cshtml` - Number input with unit display
- `_DateField.cshtml` - Date picker rendering
- `_BooleanField.cshtml` - Checkbox, toggle, or radio button rendering
- `_DropdownField.cshtml` - Select dropdown rendering
- `_FileUploadField.cshtml` - File attachment rendering

**Component Responsibilities:**
- Render appropriate HTML input based on field configuration
- Apply validation attributes
- Display help text and tooltips
- Show pre-filled values with indicators
- Handle conditional logic visibility
- Display validation errors

**Usage in Form Renderer:**
```razor
@foreach (var field in section.Fields.OrderBy(f => f.DisplayOrder))
{
    @await Html.PartialAsync("_FormField", field, new ViewDataDictionary(ViewData)
    {
        { "ExistingValue", Model.ExistingResponses.GetValueOrDefault(field.ItemId) }
    })
}
```

The `_FormField` component acts as a router, determining which specific field component to render based on the field's `DataType` property.

---

## 3. Form Submission Wizard

**Component Type:** Multi-step wizard for forms with multiple sections

**Reusable Component:** Horizontal Wizard (from Reusable Components Architecture)

### Wizard Navigation Logic

**JavaScript:** `wwwroot/js/form-submission.js`

```javascript
let currentStep = 0;
const totalSteps = document.querySelectorAll('.wizard-step').length;

function navigateSection(direction) {
    const currentSection = document.querySelector(`.wizard-step[data-step="${currentStep}"]`);
    
    // Validate current section before proceeding forward
    if (direction > 0) {
        if (!validateSection(currentStep)) {
            showValidationError("Please complete all required fields before proceeding.");
            return;
        }
    }
    
    // Hide current section
    currentSection.style.display = 'none';
    
    // Calculate new step
    currentStep = currentStep + direction;
    
    // Show new section
    const newSection = document.querySelector(`.wizard-step[data-step="${currentStep}"]`);
    newSection.style.display = 'block';
    
    // Update hidden field
    document.getElementById('currentSection').value = currentStep;
    
    // Update navigation buttons
    updateNavigationButtons();
    
    // Scroll to top
    window.scrollTo(0, 0);
    
    // Trigger auto-save
    autoSave();
}

function updateNavigationButtons() {
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const submitBtn = document.getElementById('submitBtn');
    
    // Previous button
    if (currentStep === 0) {
        prevBtn.style.display = 'none';
    } else {
        prevBtn.style.display = 'inline-block';
    }
    
    // Next vs Submit button
    if (currentStep === totalSteps - 1) {
        nextBtn.style.display = 'none';
        submitBtn.style.display = 'inline-block';
    } else {
        nextBtn.style.display = 'inline-block';
        submitBtn.style.display = 'none';
    }
}

function validateSection(sectionIndex) {
    const section = document.querySelector(`.wizard-step[data-step="${sectionIndex}"]`);
    const requiredFields = section.querySelectorAll('[required]');
    let isValid = true;
    
    requiredFields.forEach(field => {
        if (!field.value || field.value.trim() === '') {
            field.classList.add('is-invalid');
            isValid = false;
        } else {
            field.classList.remove('is-invalid');
        }
    });
    
    return isValid;
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    updateNavigationButtons();
});
```

### Progress Tracker

**Partial View:** `Views/Shared/_FormProgressTracker.cshtml`

Shows step progress at top of form:

```
[1. General Info ✓] → [2. Hardware ●] → [3. Network] → [4. Summary]
     Completed           Current          Pending       Pending
```

**Logic:**
- Mark sections as completed when all required fields filled
- Highlight current section
- Allow clicking on completed sections to go back
- Disable clicking on future sections

---

## 4. Auto-save Service

**Purpose:** Automatically save draft responses every 30 seconds to prevent data loss

### Auto-save Implementation

**JavaScript:** `wwwroot/js/form-submission.js`

```javascript
let autoSaveTimer = null;
let lastSavedData = null;
const AUTO_SAVE_INTERVAL = 30000; // 30 seconds

// Start auto-save timer when form loads
document.addEventListener('DOMContentLoaded', function() {
    startAutoSave();
    
    // Also save when user navigates away
    window.addEventListener('beforeunload', function(e) {
        if (hasUnsavedChanges()) {
            autoSave(true); // Synchronous save
            e.preventDefault();
            e.returnValue = 'You have unsaved changes. Are you sure you want to leave?';
        }
    });
});

function startAutoSave() {
    autoSaveTimer = setInterval(function() {
        if (hasUnsavedChanges()) {
            autoSave();
        }
    }, AUTO_SAVE_INTERVAL);
}

function autoSave(isSync = false) {
    const formData = collectFormData();
    
    // Compare with last saved data
    if (JSON.stringify(formData) === lastSavedData) {
        return; // No changes, skip save
    }
    
    showAutoSaveIndicator('saving');
    
    const request = {
        submissionId: document.querySelector('[name="SubmissionId"]').value,
        assignmentId: document.querySelector('[name="AssignmentId"]').value,
        responses: formData,
        currentSection: currentStep,
        isDraft: true
    };
    
    if (isSync) {
        // Synchronous save for page unload
        navigator.sendBeacon('/api/submissions/auto-save', JSON.stringify(request));
    } else {
        // Asynchronous save
        fetch('/api/submissions/auto-save', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(request)
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                lastSavedData = JSON.stringify(formData);
                showAutoSaveIndicator('saved');
                
                // Update submission ID if first save
                if (!request.submissionId) {
                    document.querySelector('[name="SubmissionId"]').value = data.submissionId;
                }
            } else {
                showAutoSaveIndicator('error');
            }
        })
        .catch(error => {
            console.error('Auto-save failed:', error);
            showAutoSaveIndicator('error');
        });
    }
}

function collectFormData() {
    const form = document.getElementById('submissionForm');
    const formData = new FormData(form);
    const data = {};
    
    for (let [key, value] of formData.entries()) {
        if (key.startsWith('responses[')) {
            const itemId = key.match(/\d+/)[0];
            data[itemId] = value;
        }
    }
    
    return data;
}

function hasUnsavedChanges() {
    const currentData = JSON.stringify(collectFormData());
    return currentData !== lastSavedData;
}

function showAutoSaveIndicator(status) {
    const indicator = document.getElementById('autosaveIndicator');
    
    switch(status) {
        case 'saving':
            indicator.innerHTML = '💾 Saving...';
            indicator.className = 'autosave-indicator saving';
            break;
        case 'saved':
            indicator.innerHTML = '✓ All changes saved';
            indicator.className = 'autosave-indicator saved';
            setTimeout(() => {
                indicator.style.opacity = '0.6';
            }, 2000);
            break;
        case 'error':
            indicator.innerHTML = '⚠️ Save failed. Retrying...';
            indicator.className = 'autosave-indicator error';
            // Retry after 5 seconds
            setTimeout(() => autoSave(), 5000);
            break;
    }
}
```

### Auto-save API Endpoint

**Controller:** `Controllers/SubmissionsController.cs`

```csharp
[HttpPost("api/submissions/auto-save")]
public async Task<IActionResult> AutoSave([FromBody] AutoSaveRequest request)
{
    try
    {
        FormTemplateSubmission submission;
        
        if (request.SubmissionId > 0)
        {
            // Update existing submission
            submission = await _context.FormTemplateSubmissions
                .FirstOrDefaultAsync(s => s.SubmissionId == request.SubmissionId);
            
            if (submission == null)
                return NotFound();
        }
        else
        {
            // Create new submission
            submission = new FormTemplateSubmission
            {
                AssignmentId = request.AssignmentId,
                Status = "Draft",
                SubmittedBy = User.GetUserId(),
                CreatedDate = DateTime.UtcNow
            };
            _context.FormTemplateSubmissions.Add(submission);
            await _context.SaveChangesAsync(); // Get SubmissionId
        }
        
        // Update or insert responses
        foreach (var response in request.Responses)
        {
            var existing = await _context.FormTemplateResponses
                .FirstOrDefaultAsync(r => 
                    r.SubmissionId == submission.SubmissionId && 
                    r.ItemId == response.Key);
            
            if (existing != null)
            {
                existing.ResponseValue = response.Value;
                existing.ModifiedDate = DateTime.UtcNow;
            }
            else
            {
                _context.FormTemplateResponses.Add(new FormTemplateResponse
                {
                    SubmissionId = submission.SubmissionId,
                    ItemId = response.Key,
                    ResponseValue = response.Value,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }
        
        submission.LastSavedDate = DateTime.UtcNow;
        submission.CurrentSection = request.CurrentSection;
        
        await _context.SaveChangesAsync();
        
        return Ok(new { 
            success = true, 
            submissionId = submission.SubmissionId,
            savedAt = submission.LastSavedDate 
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Auto-save failed for assignment {AssignmentId}", request.AssignmentId);
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}
```

### Auto-save Database Schema

**Tables:**
- `FormTemplateSubmissions.LastSavedDate` - Track when draft last saved
- `FormTemplateSubmissions.CurrentSection` - Resume from correct section
- `FormTemplateSubmissions.Status` - "Draft" until submitted

---

## 5. File Upload Handler

**Purpose:** Handle file attachments for FileUpload field types

### File Upload Component Usage

The `_FileUploadField` component (created separately) handles UI rendering. This section covers backend logic.

### File Upload API

**Controller:** `Controllers/SubmissionsController.cs`

```csharp
[HttpPost("api/submissions/upload-file")]
[RequestSizeLimit(26214400)] // 25 MB limit
public async Task<IActionResult> UploadFile(
    [FromForm] int submissionId,
    [FromForm] int itemId,
    [FromForm] IFormFile file)
{
    try
    {
        // Validate file
        var field = await GetFieldConfig(itemId);
        var config = field.FileUploadConfig;
        
        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!config.AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { 
                error = $"File type {extension} not allowed. Allowed: {string.Join(", ", config.AllowedExtensions)}" 
            });
        }
        
        // Check file size
        var maxSizeBytes = config.MaxFileSizeMB * 1024 * 1024;
        if (file.Length > maxSizeBytes)
        {
            return BadRequest(new { 
                error = $"File size exceeds maximum of {config.MaxFileSizeMB} MB" 
            });
        }
        
        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "submissions", submissionId.ToString());
        Directory.CreateDirectory(uploadPath);
        
        var filePath = Path.Combine(uploadPath, uniqueFileName);
        
        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        // Save file reference to database
        var attachment = new FormSubmissionAttachment
        {
            SubmissionId = submissionId,
            ItemId = itemId,
            FileName = file.FileName,
            UniqueFileName = uniqueFileName,
            FilePath = filePath,
            FileSize = file.Length,
            ContentType = file.ContentType,
            UploadedDate = DateTime.UtcNow,
            UploadedBy = User.GetUserId()
        };
        
        _context.FormSubmissionAttachments.Add(attachment);
        await _context.SaveChangesAsync();
        
        return Ok(new { 
            success = true, 
            attachmentId = attachment.AttachmentId,
            fileName = file.FileName,
            fileSize = file.Length,
            uploadedDate = attachment.UploadedDate
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "File upload failed");
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}

[HttpDelete("api/submissions/delete-file/{attachmentId}")]
public async Task<IActionResult> DeleteFile(int attachmentId)
{
    var attachment = await _context.FormSubmissionAttachments
        .FirstOrDefaultAsync(a => a.AttachmentId == attachmentId);
    
    if (attachment == null)
        return NotFound();
    
    // Check ownership
    var submission = await _context.FormTemplateSubmissions
        .FirstOrDefaultAsync(s => s.SubmissionId == attachment.SubmissionId);
    
    if (submission.SubmittedBy != User.GetUserId())
        return Forbid();
    
    // Delete physical file
    if (System.IO.File.Exists(attachment.FilePath))
    {
        System.IO.File.Delete(attachment.FilePath);
    }
    
    // Delete database record
    _context.FormSubmissionAttachments.Remove(attachment);
    await _context.SaveChangesAsync();
    
    return Ok(new { success = true });
}
```

### File Storage Structure

```
wwwroot/uploads/submissions/
├─ 123/                           (SubmissionId)
│  ├─ abc123_report.pdf
│  ├─ def456_screenshot.png
│  └─ ghi789_invoice.xlsx
├─ 124/
│  └─ xyz999_document.docx
└─ ...
```

### Multiple File Handling

If `AllowMultiple = true` in field config:
- User can upload multiple files
- Each file creates separate attachment record
- All linked to same ItemId
- Display as list with delete buttons

---

## 6. Pre-fill Service

**Purpose:** Auto-populate fields from existing data sources (Hardware, Software, Tenant)

### Pre-fill Logic

**Service:** `Services/PreFillService.cs`

```csharp
public class PreFillService
{
    private readonly ApplicationDbContext _context;
    
    public async Task<Dictionary<int, string>> GetPreFilledValues(
        int templateId, 
        int tenantId)
    {
        var preFilledData = new Dictionary<int, string>();
        
        // Get all fields with pre-fill configuration
        var fieldsWithPreFill = await _context.FormTemplateItems
            .Where(i => i.TemplateId == templateId && i.PreFillSource != null)
            .ToListAsync();
        
        foreach (var field in fieldsWithPreFill)
        {
            var value = await GetPreFillValue(field.PreFillSource, field.PreFillMapping, tenantId);
            if (!string.IsNullOrEmpty(value))
            {
                preFilledData[field.ItemId] = value;
            }
        }
        
        return preFilledData;
    }
    
    private async Task<string> GetPreFillValue(string source, string mapping, int tenantId)
    {
        return source switch
        {
            "HardwareAsset" => await GetHardwareAssetValue(mapping, tenantId),
            "SoftwareAsset" => await GetSoftwareAssetValue(mapping, tenantId),
            "TenantProperty" => await GetTenantPropertyValue(mapping, tenantId),
            _ => null
        };
    }
    
    private async Task<string> GetHardwareAssetValue(string mapping, int tenantId)
    {
        // Get list of hardware assets for dropdown
        var assets = await _context.HardwareAssets
            .Where(h => h.TenantId == tenantId && h.IsActive)
            .Select(h => new {
                h.AssetId,
                h.AssetTag,
                h.AssetName,
                h.SerialNumber,
                h.Model,
                h.Manufacturer
            })
            .ToListAsync();
        
        // Return as JSON for dropdown population
        return JsonConvert.SerializeObject(assets.Select(a => mapping switch
        {
            "AssetTag" => a.AssetTag,
            "AssetName" => a.AssetName,
            "SerialNumber" => a.SerialNumber,
            "Model" => a.Model,
            "Manufacturer" => a.Manufacturer,
            _ => a.AssetTag
        }));
    }
    
    private async Task<string> GetSoftwareAssetValue(string mapping, int tenantId)
    {
        var assets = await _context.SoftwareAssets
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .Select(s => new {
                s.SoftwareId,
                s.SoftwareName,
                s.LicenseKey,
                s.Version,
                s.Vendor
            })
            .ToListAsync();
        
        return JsonConvert.SerializeObject(assets.Select(a => mapping switch
        {
            "SoftwareName" => a.SoftwareName,
            "LicenseKey" => a.LicenseKey,
            "Version" => a.Version,
            "Vendor" => a.Vendor,
            _ => a.SoftwareName
        }));
    }
    
    private async Task<string> GetTenantPropertyValue(string mapping, int tenantId)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Region)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        
        if (tenant == null) return null;
        
        return mapping switch
        {
            "TenantName" => tenant.TenantName,
            "TenantCode" => tenant.TenantCode,
            "Address" => tenant.Address,
            "Phone" => tenant.Phone,
            "Email" => tenant.Email,
            "RegionName" => tenant.Region?.RegionName,
            _ => null
        };
    }
}
```

### Pre-fill in Controller

```csharp
[HttpGet("submissions/start/{assignmentId}")]
public async Task<IActionResult> StartSubmission(int assignmentId)
{
    var assignment = await _context.FormTemplateAssignments
        .Include(a => a.Template)
        .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
    
    if (assignment == null)
        return NotFound();
    
    // Get pre-filled values
    var tenantId = User.GetTenantId();
    var preFillService = new PreFillService(_context);
    var preFilledValues = await preFillService.GetPreFilledValues(
        assignment.TemplateId, 
        tenantId
    );
    
    var viewModel = new FormSubmissionViewModel
    {
        AssignmentId = assignmentId,
        Template = assignment.Template,
        Sections = LoadSections(assignment.TemplateId),
        ExistingResponses = preFilledValues, // Pre-filled as existing responses
        IsResuming = false
    };
    
    return View("SubmitForm", viewModel);
}
```

---

## 7. Client-Side Validation

**Purpose:** Real-time validation feedback before server submission

### Validation Engine

**JavaScript:** `wwwroot/js/form-validation.js`

```javascript
function validateField(fieldElement) {
    const validations = JSON.parse(fieldElement.dataset.validations || '[]');
    const value = fieldElement.value;
    const errorContainer = document.getElementById(`error_${fieldElement.id.split('_')[1]}`);
    
    for (let validation of validations) {
        const result = executeValidation(validation, value);
        
        if (!result.isValid) {
            // Show error
            fieldElement.classList.add('is-invalid');
            errorContainer.textContent = validation.errorMessage;
            errorContainer.style.display = 'block';
            
            if (validation.severity === 'Error') {
                return false; // Stop on first error
            } else if (validation.severity === 'Warning') {
                // Show warning but continue
                errorContainer.classList.add('warning');
            }
        }
    }
    
    // All validations passed
    fieldElement.classList.remove('is-invalid');
    errorContainer.style.display = 'none';
    return true;
}

function executeValidation(validation, value) {
    switch(validation.validationType) {
        case 'Required':
            return {
                isValid: value && value.trim() !== '',
                message: validation.errorMessage
            };
        
        case 'MinLength':
            return {
                isValid: value.length >= validation.minLength,
                message: validation.errorMessage
            };
        
        case 'MaxLength':
            return {
                isValid: value.length <= validation.maxLength,
                message: validation.errorMessage
            };
        
        case 'MinValue':
            return {
                isValid: parseFloat(value) >= validation.minValue,
                message: validation.errorMessage
            };
        
        case 'MaxValue':
            return {
                isValid: parseFloat(value) <= validation.maxValue,
                message: validation.errorMessage
            };
        
        case 'Range':
            const numValue = parseFloat(value);
            return {
                isValid: numValue >= validation.minValue && numValue <= validation.maxValue,
                message: validation.errorMessage
            };
        
        case 'RegexPattern':
            const regex = new RegExp(validation.regexPattern);
            return {
                isValid: regex.test(value),
                message: validation.errorMessage
            };
        
        default:
            return { isValid: true };
    }
}
```

### Conditional Logic Evaluation

**JavaScript:** `wwwroot/js/conditional-logic.js`

```javascript
// Evaluate all conditional logic on page load
document.addEventListener('DOMContentLoaded', function() {
    evaluateAllConditionalLogic();
    
    // Re-evaluate when any field changes
    document.querySelectorAll('input, select, textarea').forEach(field => {
        field.addEventListener('change', function() {
            evaluateAllConditionalLogic();
        });
    });
});

function evaluateAllConditionalLogic() {
    document.querySelectorAll('[data-conditional]').forEach(container => {
        const logic = JSON.parse(container.dataset.conditional || 'null');
        if (logic) {
            const shouldApply = evaluateLogic(logic);
            applyAction(container, logic.action, shouldApply);
        }
    });
}

function evaluateLogic(logic) {
    if (!logic || !logic.rules || logic.rules.length === 0) {
        return true;
    }
    
    const results = logic.rules.map(rule => {
        const sourceField = document.getElementById(`field_${rule.sourceItemId}`);
        if (!sourceField) return false;
        
        const sourceValue = sourceField.value;
        return checkCondition(sourceValue, rule.operator, rule.value);
    });
    
    // Apply AND/OR logic
    if (logic.logicOperator === 'AND') {
        return results.every(r => r === true);
    } else {
        return results.some(r => r === true);
    }
}

function checkCondition(sourceValue, operator, expectedValue) {
    switch(operator) {
        case 'equals':
            return sourceValue == expectedValue;
        case 'not_equals':
            return sourceValue != expectedValue;
        case 'contains':
            return sourceValue && sourceValue.includes(expectedValue);
        case 'not_contains':
            return !sourceValue || !sourceValue.includes(expectedValue);
        case 'greater_than':
            return parseFloat(sourceValue) > parseFloat(expectedValue);
        case 'less_than':
            return parseFloat(sourceValue) < parseFloat(expectedValue);
        case 'is_empty':
            return !sourceValue || sourceValue === '';
        case 'not_empty':
            return sourceValue && sourceValue !== '';
        default:
            return true;
    }
}

function applyAction(container, action, shouldApply) {
    if (action === 'show') {
        container.style.display = shouldApply ? 'block' : 'none';
    } else if (action === 'hide') {
        container.style.display = shouldApply ? 'none' : 'block';
    } else if (action === 'enable') {
        const input = container.querySelector('input, select, textarea');
        if (input) input.disabled = !shouldApply;
    } else if (action === 'disable') {
        const input = container.querySelector('input, select, textarea');
        if (input) input.disabled = shouldApply;
    }
}
```

---

## API Endpoints Summary

```
# Form Loading
GET    /submissions/start/{assignmentId}         - Start new submission
GET    /submissions/resume/{submissionId}        - Resume draft submission

# Auto-save
POST   /api/submissions/auto-save                - Save draft responses

# File Uploads
POST   /api/submissions/upload-file              - Upload file attachment
DELETE /api/submissions/delete-file/{id}         - Delete attachment

# Pre-fill
GET    /api/submissions/prefill/{templateId}     - Get pre-filled values

# Final Submission
POST   /submissions/submit                        - Submit for approval/completion
```

---

**Next:** Submission Review & Approval Workflows (File 4C)
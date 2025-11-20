# Resume & Edit Functionality - Implementation Summary

**Date:** November 20, 2025  
**Feature:** Separate Resume (Draft continuation) and Edit (Version creation) workflows

---

## 🎯 **Two Distinct Workflows**

### **1. Resume (Draft Continuation)**
- **Purpose:** Continue incomplete template creation
- **Status:** Draft only
- **URL:** `/FormTemplates/Create?id=123`
- **Versioning:** NO - same draft version
- **Button:** "Continue Editing"

### **2. Edit (Version Creation)**
- **Purpose:** Modify published template
- **Status:** Published only
- **URL:** `/FormTemplates/Edit/123`
- **Versioning:** YES - creates v2, v3, etc.
- **Button:** "Edit"

---

## ✅ **What Was Implemented**

### **1. Service Layer (Complex Logic)**

**File:** `Services/Forms/FormTemplateService.cs`

#### **Methods for Resume:**

```csharp
/// Load template with all related entities for editing
Task<FormTemplate?> LoadTemplateForEditingAsync(int templateId)
```
- Loads template with Sections, Items, Assignments, Workflow, Category
- Single responsibility: Data loading
- Used by both resume and edit operations

```csharp
/// Analyze template progress and determine current step
FormBuilderResumeInfo AnalyzeTemplateProgress(FormTemplate template)
```
- **Complex logic isolated here**
- Checks completion of all 7 steps
- Determines first incomplete step
- Calculates completion percentage
- Handles optional vs required steps
- Returns structured resume information

#### **Methods for Edit/Versioning:**

```csharp
/// Create new version from published template
Task<FormTemplate> CreateNewVersionAsync(int publishedTemplateId, int userId)
```
- **140+ lines of complex cloning logic**
- Copies template metadata (name, code, category, etc.)
- Increments version number (v1 → v2)
- Clones all sections with proper ordering
- Clones all items with field configurations
- Clones all assignments (who can fill it)
- Sets new version status to "Draft"
- Returns fully loaded new version

```csharp
/// Check if template can be versioned
bool CanCreateVersion(FormTemplate template)
```
- Validates template is "Published"
- Used before version creation

---

### **2. View Model**

**File:** `Models/ViewModels/Forms/FormBuilderResumeInfo.cs`

```csharp
public class FormBuilderResumeInfo
{
    public int TemplateId { get; set; }
    public FormBuilderStep CurrentStep { get; set; }
    public Dictionary<FormBuilderStep, bool> CompletedSteps { get; set; }
    public Dictionary<FormBuilderStep, StepStatus> StepStatuses { get; set; }
    public int CompletionPercentage { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public bool CanEdit { get; } // Only drafts
    public string CurrentStepName { get; } // User-friendly name
}
```

---

### **3. Controller Layer (Clean & Simple)**

**File:** `Controllers/Forms/FormTemplatesController.cs`

#### **Updated Actions:**

```csharp
/// Create NEW template OR RESUME draft
public async Task<IActionResult> Create(int? id = null)
{
    if (id.HasValue)
        return await ResumeDraft(id.Value); // Resume draft logic
    
    // Otherwise new template logic
    return View("Create.cshtml");
}
```

```csharp
/// Edit PUBLISHED template (create new version)
[HttpGet("Edit/{id}")]
public async Task<IActionResult> Edit(int id)
{
    // 1. Load published template
    var template = await _templateService.LoadTemplateForEditingAsync(id);
    
    // 2. Validate it's published
    if (!_templateService.CanCreateVersion(template))
        return error;
    
    // 3. Create new version via service
    var newVersion = await _templateService.CreateNewVersionAsync(id, userId);
    
    // 4. Redirect to Create with new version ID
    return RedirectToAction("Create", new { id = newVersion.TemplateId });
}
```

#### **Private Helpers:**

```csharp
/// Resume draft - analyze progress and route to step
private async Task<IActionResult> ResumeDraft(int id)
{
    var template = await _templateService.LoadTemplateForEditingAsync(id);
    var resumeInfo = _templateService.AnalyzeTemplateProgress(template);
    
    // Route to appropriate step based on progress
    return resumeInfo.CurrentStep switch { ... };
}
```

```csharp
/// Resume Step 1 specifically
private async Task<IActionResult> ResumeTemplateSetup(FormTemplate template)
{
    ViewBag.Categories = await _categoryService.GetCategorySelectListAsync();
    return View("Create.cshtml", template);
}
```

**Controller responsibilities:**
- ✅ Coordinate service calls
- ✅ Handle routing and redirects
- ✅ Validate permissions (Draft vs Published)
- ✅ Show user messages (TempData)
- ❌ NO complex business logic

---

### **4. Enhanced SaveDraft**

**Updated:** `SaveDraft` method now returns progress info

```json
{
    "success": true,
    "templateId": 123,
    "message": "Draft updated",
    "currentStep": 2,
    "completionPercentage": 28
}
```

Client-side can now show progress indicator after each autosave.

---

## 🎯 **How It Works**

### **Step Detection Logic** (in FormTemplateService)

```
Step 1 (Template Setup):
✅ Complete if: Name, Code, Category, Type all filled
❌ Incomplete if: Any required field missing
→ User resumes at Step 1

Step 2 (Form Builder):
✅ Complete if: Has sections AND items AND each section has items
❌ Incomplete if: No sections or no items
→ User resumes at Step 2

Step 3 (Metric Mapping): OPTIONAL
✅ Complete if: Any item has MetricId
⏭️ Skippable: Not required
→ Shows as Pending if skipped

Step 4 (Approval Workflow): CONDITIONAL
✅ Complete if: RequiresApproval=false OR WorkflowId exists
❌ Incomplete if: RequiresApproval=true AND no WorkflowId
→ User resumes at Step 4

Step 5 (Form Assignments): REQUIRED
✅ Complete if: Has at least one assignment
❌ Incomplete if: No assignments
→ User resumes at Step 5

Step 6 (Report Configuration): OPTIONAL
✅ Always complete for now (not yet implemented)
⏭️ Skippable

Step 7 (Review & Publish):
✅ Complete if: PublishStatus = "Published"
❌ Incomplete if: PublishStatus = "Draft"
→ Final step for validation and publishing
```

---

## 🎨 **User Flows**

### **Flow 1: Create New Template**
```
1. User clicks "Create Template"
   ↓
2. Create() with no id → New template form
   ↓
3. User fills Step 1 (Name, Code, Category)
   ↓
4. Autosave creates draft every 30 seconds
   ↓
5. User leaves (browser closes)
   ↓
6. Draft saved with partial completion
```

### **Flow 2: Resume Draft**
```
1. User sees draft in Index with "Continue Editing" button
   ↓
2. Clicks "Continue Editing"
   ↓
3. Create?id=123 (detects id parameter)
   ↓
4. ResumeDraft(123) called
   ↓
5. Service analyzes:
   • Step 1: ✅ Complete
   • Step 2: ❌ Incomplete
   ↓
6. User lands on Step 2 (Form Builder)
   ↓
7. Progress tracker shows correct state
```

### **Flow 3: Edit Published Template**
```
1. Published Template v1 shown in Index
   ↓
2. User clicks "Edit"
   ↓
3. Edit(id) → Loads v1
   ↓
4. Service creates v2 (clones everything)
   ↓
5. v2 created as Draft
   ↓
6. Redirect to Create?id=124 (v2's ID)
   ↓
7. User goes through wizard on v2
   ↓
8. User modifies sections/items
   ↓
9. User publishes v2
   ↓
10. v1 can be archived, v2 is now live
```

---

## 🏗️ **Architecture Benefits**

### **Clean Separation of Concerns:**

```
FormTemplatesController (Thin Layer)
├─ Handles HTTP requests
├─ Validates permissions
├─ Routes to views
└─ Calls service methods

FormTemplateService (Business Logic)
├─ Analyzes template progress
├─ Determines current step
├─ Calculates completion %
├─ Loads data with relationships
└─ Returns structured info

FormBuilderResumeInfo (Data Transfer)
└─ Carries progress information between layers
```

---

## 📋 **Next Steps for Index View**

To complete the feature, update the Index view with different buttons for different statuses:

### **For Drafts (Resume):**
```cshtml
@if (template.PublishStatus == "Draft")
{
    @* Progress bar *@
    <div class="progress" style="height: 6px;">
        <div class="progress-bar bg-primary" 
             style="width: @ViewBag.Progress[template.TemplateId]%">
        </div>
    </div>
    <small class="text-muted">
        @ViewBag.Progress[template.TemplateId]% Complete
    </small>
    
    @* Resume button - links to Create?id *@
    <a href="@Url.Action("Create", new { id = template.TemplateId })" 
       class="btn btn-primary btn-sm">
        <i class="ri-play-line"></i> Continue Editing
    </a>
    
    @* Current step badge *@
    <span class="badge bg-info-subtle text-info">
        <i class="ri-arrow-right-line"></i> 
        Step @ViewBag.CurrentSteps[template.TemplateId]
    </span>
}
```

### **For Published (Edit/Version):**
```cshtml
@if (template.PublishStatus == "Published")
{
    @* Version badge *@
    <span class="badge bg-success-subtle text-success">
        v@template.Version
    </span>
    
    @* Edit button - creates new version *@
    <a href="@Url.Action("Edit", new { id = template.TemplateId })" 
       class="btn btn-soft-warning btn-sm">
        <i class="ri-edit-line"></i> Edit (Create v@(template.Version + 1))
    </a>
    
    @* View/Preview button *@
    <a href="@Url.Action("Preview", new { id = template.TemplateId })" 
       class="btn btn-soft-info btn-sm">
        <i class="ri-eye-line"></i> Preview
    </a>
}
```

---

## ✅ **Summary**

| **Component** | **Status** | **Responsibility** |
|---------------|------------|-------------------|
| `FormTemplateService.LoadTemplateForEditingAsync` | ✅ Implemented | Load template with relations |
| `FormTemplateService.AnalyzeTemplateProgress` | ✅ Implemented | Progress analysis (130 lines) |
| `FormTemplateService.CreateNewVersionAsync` | ✅ Implemented | Clone template (140 lines) |
| `FormTemplateService.CanCreateVersion` | ✅ Implemented | Validation helper |
| `FormBuilderResumeInfo` | ✅ Implemented | Resume data structure |
| `FormTemplatesController.Create` | ✅ Refactored | New + Resume logic |
| `FormTemplatesController.Edit` | ✅ Refactored | Version creation |
| `FormTemplatesController.ResumeDraft` | ✅ Implemented | Private resume helper |
| `FormTemplatesController.SaveDraft` | ✅ Updated | Returns progress info |
| Index View Updates | ⏳ **PENDING** | Different buttons per status |

---

## 🎯 **What's Different Now**

### **Before Refactoring:**
```
❌ Edit() handled drafts only
❌ No way to version published templates
❌ Resume and Edit were the same thing
```

### **After Refactoring:**
```
✅ Create(id) handles draft resume
✅ Edit(id) creates new version from published
✅ Clear separation of concerns
✅ 270+ lines of complex logic in service
✅ Controller stays clean (40-50 lines per action)
```

---

## 🚀 **Key Achievements**

**Architecture:**
- ✅ Complex logic isolated in service (270+ lines)
- ✅ Controller is clean and readable (40-50 lines per action)
- ✅ Service methods are testable and reusable
- ✅ No database changes needed
- ✅ Progress calculated from existing data
- ✅ Version creation handles all relationships

**Functionality:**
- ✅ **Resume:** Continue draft from any step
- ✅ **Edit:** Create versioned copy of published template
- ✅ **Clone:** Full deep copy (sections, items, assignments)
- ✅ **Progress:** Intelligent step detection
- ✅ **Validation:** Status-based permissions

**User Experience:**
- ✅ Users can pause and resume anytime
- ✅ Published templates safely versioned
- ✅ Clear button labels ("Continue Editing" vs "Edit")
- ✅ Version numbers tracked (v1, v2, v3)
- ✅ Progress indicators show completion

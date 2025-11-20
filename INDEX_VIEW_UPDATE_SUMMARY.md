# Index View Updates - Resume vs Edit Implementation

**Date:** November 20, 2025  
**File Updated:** `Views/Forms/FormTemplates/Index.cshtml`

---

## ✅ **Changes Made**

### **1. Updated Row Actions Function** ⭐

**Location:** Lines 279-437

Completely refactored the `BuildRowActions()` function to show different actions based on template status:

---

#### **For DRAFT Templates:**

```cshtml
✅ Continue Editing (Primary Action)
   - Links to: /FormTemplates/Create?id=123
   - Icon: ri-play-line
   - Color: Primary (Blue)
   - Purpose: Resume draft from where user left off

✅ View Preview
   - Links to: /FormTemplates/Preview/123
   - Color: Secondary

✅ Delete Draft
   - Color: Danger (Red)
   - Requires confirmation
   - Purpose: Delete incomplete template

✅ Clone
   - Available for all statuses
```

---

#### **For PUBLISHED Templates:**

```cshtml
✅ Edit (Create v2, v3, etc.)
   - Links to: /FormTemplates/Edit/123
   - Icon: ri-git-branch-line
   - Color: Warning (Orange)
   - Text: "Edit (Create v{X+1})"
   - Purpose: Create new version

✅ View Template
   - Links to: /FormTemplates/Preview/123
   - Color: Primary

✅ View Assignments
   - Links to: /FormTemplates/Assignments/123
   - Color: Info

✅ View Submissions
   - Links to: /FormSubmissions?templateId=123
   - Color: Success

✅ Archive
   - Color: Danger
   - Requires confirmation

✅ Clone
   - Available for all statuses
```

---

#### **For ARCHIVED/DEPRECATED Templates:**

```cshtml
✅ View Template
   - Color: Secondary

✅ Create New Version
   - Links to: /FormTemplates/Edit/123
   - Purpose: Version from archived template

✅ Restore (if Archived)
   - Requires confirmation
   - Purpose: Restore to published

✅ Clone
   - Available for all statuses
```

---

### **2. Added Visual Progress Indicators** ⭐

**Location:** Lines 540-563 (Template Name column)

Added inline progress indicators for draft templates:

```cshtml
@if (item.PublishStatus == "Draft")
{
    <div class="mt-2">
        <div class="d-flex align-items-center gap-2 mb-1">
            <span class="badge bg-info-subtle text-info">
                <i class="ri-draft-line"></i> In Progress
            </span>
            @* TODO: Add completion % from controller *@
        </div>
    </div>
}
```

Added "Live Template" indicator for published:

```cshtml
@if (item.PublishStatus == "Published")
{
    <small class="text-muted">
        <i class="ri-checkbox-circle-line text-success"></i> 
        Live Template
    </small>
}
```

---

## 🎯 **Before vs After**

### **BEFORE:**
```
Draft Template Row:
└─ Actions: "Edit" (went to Edit/123 - wrong!)

Published Template Row:
└─ Actions: "Create New Version" (generic)
```

### **AFTER:**
```
Draft Template Row:
├─ Visual: "In Progress" badge
└─ Actions: "Continue Editing" (goes to Create?id=123) ✅

Published Template Row:
├─ Visual: "Live Template" indicator
└─ Actions: "Edit (Create v2)" (goes to Edit/123) ✅
```

---

## 📊 **Action Buttons Summary**

| **Status** | **Primary Action** | **URL** | **Purpose** |
|------------|-------------------|---------|-------------|
| **Draft** | Continue Editing | `Create?id=X` | Resume from last step |
| **Published** | Edit (Create vX) | `Edit/X` | Create new version |
| **Archived** | Create New Version | `Edit/X` | Version from archived |

---

## 🎨 **User Experience Improvements**

### **1. Clear Button Labels**
- ✅ "Continue Editing" clearly indicates resuming work
- ✅ "Edit (Create v2)" shows version will be created
- ✅ Icons match the action intent

### **2. Visual Status Indicators**
- ✅ Drafts show "In Progress" badge
- ✅ Published show "Live Template" indicator
- ✅ Color-coded for quick scanning

### **3. Context-Appropriate Actions**
- ✅ Drafts: Focus on completing creation
- ✅ Published: Focus on viewing and versioning
- ✅ Archived: Focus on viewing and restoring

---

## ⏳ **Pending Enhancements**

### **Add Completion Percentage to Index** (Optional)

To show progress percentage in Index view, update the controller:

```csharp
// In FormTemplatesController.Index()
public async Task<IActionResult> Index(...)
{
    // ... existing code ...
    
    // For each draft template, calculate progress
    var progressInfo = new Dictionary<int, int>();
    foreach (var template in templates.Where(t => t.PublishStatus == "Draft"))
    {
        var fullTemplate = await _templateService.LoadTemplateForEditingAsync(template.TemplateId);
        var resumeInfo = _templateService.AnalyzeTemplateProgress(fullTemplate);
        progressInfo[template.TemplateId] = resumeInfo.CompletionPercentage;
    }
    ViewBag.ProgressInfo = progressInfo;
    
    return View(templates);
}
```

Then in Index.cshtml:

```cshtml
@if (item.PublishStatus == "Draft" && ViewBag.ProgressInfo?.ContainsKey(item.TemplateId) == true)
{
    var progress = ViewBag.ProgressInfo[item.TemplateId];
    <div class="progress mt-2" style="height: 4px;">
        <div class="progress-bar bg-primary" style="width: @progress%"></div>
    </div>
    <small class="text-muted">@progress% Complete</small>
}
```

**Note:** This is optional and adds database queries. Consider implementing only if needed for UX.

---

## ✅ **Testing Checklist**

- [ ] Draft template shows "Continue Editing" button
- [ ] Clicking "Continue Editing" goes to `Create?id=X`
- [ ] Published template shows "Edit (Create vX)" button
- [ ] Clicking "Edit" goes to `Edit/X` and creates new version
- [ ] Drafts show "In Progress" badge
- [ ] Published show "Live Template" indicator
- [ ] All dropdown actions work correctly
- [ ] Confirmation dialogs appear for Delete/Archive

---

## 🎉 **Result**

The Index view now correctly implements:
- ✅ **Resume functionality** for drafts (via Create?id)
- ✅ **Edit/Version functionality** for published templates (via Edit/id)
- ✅ **Clear visual indicators** for status
- ✅ **Context-appropriate actions** per status
- ✅ **User-friendly button labels**

Users can now easily distinguish between:
- **Continuing incomplete work** (Resume)
- **Creating new versions** (Edit)

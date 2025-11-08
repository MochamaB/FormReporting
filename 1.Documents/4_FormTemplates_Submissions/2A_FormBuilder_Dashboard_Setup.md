# Form Builder: Dashboard & Template Setup

**Purpose:** Landing page and initial template configuration  
**Users:** Head Office ICT Managers (System Administrators)  
**Components:** Template Dashboard, Template Form

---

## 1. Template Management Dashboard

**Component Type:** Page with StatCards + DataTable + Modals

**Reusable Components:**
- **StatCards** - Display template statistics
- **DataTable** - List all templates with filters
- **Modal** - Archive confirmation, version history viewer

### Data Sources
```
Tables:
- FormTemplates (all templates)
- FormTemplateSubmissions (for statistics)

Filters:
- RoleLevel-based visibility
  • HeadOffice: See all templates
  • Regional: See templates assigned to their region
  • Factory: See templates assigned to them
```

### Statistics Cards (4 Cards)

**Card 1: Total Templates**
```
Calculation: COUNT(FormTemplates WHERE IsActive=1)
Color: Primary
Icon: ri-file-list-line
Link: "View all templates"
```

**Card 2: Published Templates**
```
Calculation: COUNT(PublishStatus='Published')
Color: Success
Icon: ri-file-check-line
Link: "View published"
Trend: Growth from last month
```

**Card 3: Submissions This Month**
```
Calculation: COUNT(FormTemplateSubmissions WHERE ReportingPeriod=CurrentMonth)
Color: Info
Icon: ri-file-text-line
Trend: Comparison to last month
```

**Card 4: Completion Rate**
```
Calculation: (COUNT(Status='Approved') / COUNT(All Submissions)) * 100
Color: Warning
Icon: ri-percent-line
Trend: Up/Down indicator
```

### DataTable Configuration

**Columns:**
1. **Template Name** (sortable, searchable)
   - Link to preview
   - Bold if published

2. **Template Code** (sortable, searchable)
   - Unique identifier
   - Monospace font

3. **Category** (filterable)
   - Badge: Compliance, Operational, Safety, IT, HR

4. **Type** (filterable)
   - Monthly, Quarterly, Annual, OnDemand

5. **Status** (filterable)
   - Draft (gray badge) - Can edit all
   - Published (green badge) - Read-only, can assign
   - Archived (orange badge) - Cannot assign
   - Deprecated (red badge) - Replaced by new version

6. **Version** (sortable)
   - Format: v1.0, v1.1, v2.0

7. **Last Modified** (sortable, default sort DESC)
   - Format: "Nov 6, 2025 8:00 PM"
   - Show "Modified by: John Doe"

8. **Actions** (conditional buttons)
   - **View Preview** (all statuses) → Open Component 8
   - **Edit** (draft only) → Navigate to Component 2
   - **Clone** (all statuses) → Create copy as new draft
   - **Archive** (published only) → Confirmation modal
   - **Create New Version** (published/deprecated) → Clone with incremented version
   - **View Assignments** (published only) → Navigate to Assignment Dashboard

### Filters

**Status Filter (Dropdown):**
```
Options: All, Draft, Published, Archived, Deprecated
Default: All
```

**Type Filter (Dropdown):**
```
Options: All, Monthly, Quarterly, Annual, OnDemand
Default: All
```

**Category Filter (Dropdown):**
```
Options: All, Compliance, Operational, Safety, IT, HR
Default: All
```

**Date Range Filter:**
```
Created Date: [From] to [To]
Modified Date: [From] to [To]
```

**Creator Filter (User Selector):**
```
Multi-select dropdown
Shows: Users with role = System Administrator
```

**Search Box:**
```
Full-text search on:
- Template Name
- Template Code
- Description
```

### Key Business Logic

**Status Determination:**
```csharp
string GetTemplateStatus(FormTemplate template)
{
    if (template.PublishStatus == "Draft") return "Draft";
    if (template.PublishStatus == "Published" && template.IsActive) return "Published";
    if (template.PublishStatus == "Archived") return "Archived";
    if (template.PublishStatus == "Deprecated") return "Deprecated";
}
```

**Action Visibility:**
```csharp
bool CanEdit(FormTemplate template, User user)
{
    return template.PublishStatus == "Draft" 
        && user.HasPermission("FormTemplates.Edit");
}

bool CanArchive(FormTemplate template, User user)
{
    return template.PublishStatus == "Published" 
        && user.HasPermission("FormTemplates.Archive");
}

bool CanCreateVersion(FormTemplate template, User user)
{
    return (template.PublishStatus == "Published" || template.PublishStatus == "Deprecated")
        && user.HasPermission("FormTemplates.Create");
}
```

**Version Increment Logic:**
```csharp
string GetNextVersion(string currentVersion)
{
    // Parse current version (e.g., "1.5")
    var parts = currentVersion.Split('.');
    int major = int.Parse(parts[0]);
    int minor = int.Parse(parts[1]);
    
    // Increment minor version
    minor++;
    
    // If minor reaches 10, increment major and reset minor
    if (minor >= 10)
    {
        major++;
        minor = 0;
    }
    
    return $"{major}.{minor}";
}

// Examples:
// v1.0 → v1.1
// v1.9 → v2.0
// v2.5 → v2.6
```

**Archive Confirmation:**
```
Modal Title: "Archive Template"
Message: "Are you sure you want to archive '{TemplateName}'?"
Warning: "Archived templates cannot be assigned to new users. Existing assignments will remain active."
Actions: [Confirm Archive] [Cancel]

On Confirm:
- UPDATE PublishStatus = 'Archived'
- UPDATE ArchivedDate = NOW()
- UPDATE ArchivedBy = CurrentUserId
- Show success message
- Refresh DataTable
```

### API Endpoints

```
GET    /api/templates                     - List all templates (with filters)
GET    /api/templates/{id}                - Get template details
POST   /api/templates/create              - Navigate to Component 2
PUT    /api/templates/{id}/archive        - Archive template
POST   /api/templates/{id}/clone          - Clone template (create new version)
DELETE /api/templates/{id}                - Soft delete (IsActive=0)
```

---

## 2. Template Create/Edit Form

**Component Type:** Standard Form with Tabs (3 tabs)

**Reusable Components:**
- **Standard Form** - CRUD form layout
- **Tabs** - Organize into logical groups
- **Dropdowns** - With validation and conditional display
- **Rich Text Editor** - For instructions field

### Form Modes

**Create Mode:**
- All fields editable
- Save as Draft by default
- Auto-generate options enabled

**Edit Mode (Draft):**
- All fields editable
- Can change structure

**Edit Mode (Published):**
- Only Name, Description, Instructions, Support Contact editable
- Show warning: "Published templates are read-only. Create new version for structural changes."

---

### Tab 1: Basic Information

**Template Name***
```
Type: Text input
Max Length: 100 characters
Validation: Required, unique within category
Help Text: "Descriptive name shown to users"
Example: "Factory Monthly ICT Report"
```

**Template Code***
```
Type: Text input + Auto-generate button
Max Length: 50 characters
Validation: Required, globally unique, alphanumeric + underscore only
Pattern: ^[A-Z0-9_]+$
Help Text: "Unique identifier for this template"
Example: "TPL_IT_001"

Auto-Generate Logic:
Format: TPL_{Category}_{Sequence}
Sequence = MAX(sequence in category) + 1
Examples:
- Category=IT, Sequence=1 → TPL_IT_001
- Category=COMPLIANCE, Sequence=23 → TPL_COMPLIANCE_023
```

**Description**
```
Type: Textarea
Max Length: 500 characters
Validation: Optional
Rows: 4
Help Text: "Brief description of template purpose"
Example: "Monthly checklist for factory ICT equipment and network status"
```

**Category***
```
Type: Dropdown
Options: Compliance, Operational, Safety, IT, HR
Validation: Required
Help Text: "Classification for organizing templates"
```

**Version**
```
Type: Text (read-only)
Value: Auto-assigned
Format: v1.0, v1.1, v2.0
Help Text: "Version is auto-managed by the system"
Display: Only in Edit mode
```

---

### Tab 2: Settings

**Template Type***
```
Type: Dropdown
Options:
- Monthly (recurring every month)
- Quarterly (recurring every 3 months)
- Annual (recurring every year)
- OnDemand (manually assigned)
Validation: Required
Help Text: "Determines assignment frequency"
```

**Estimated Completion Time**
```
Type: Number input
Unit: Minutes
Min: 1, Max: 480 (8 hours)
Validation: Optional
Help Text: "How long it takes to complete this form"
Default: 30
```

**Requires Approval***
```
Type: Checkbox
Default: true
Help Text: "If checked, submissions go through approval workflow"
On Change: Show/hide Workflow Definition dropdown
```

**Workflow Definition**
```
Type: Dropdown
Visibility: Shown only if "Requires Approval" is checked
Data Source: SELECT * FROM WorkflowDefinitions WHERE IsActive=1
Display: WorkflowName (e.g., "2-Step Approval", "Regional Manager Approval")
Validation: Required if Requires Approval = true
Help Text: "Select approval workflow for submissions"
```

**KPI Category**
```
Type: Dropdown (nullable)
Data Source: SELECT * FROM KPICategories WHERE IsActive=1
Display: CategoryName (e.g., "IT Operations", "Compliance Metrics")
Validation: Optional
Help Text: "Link template to KPI category for metric filtering"
```

**Allow Multiple Submissions Per Period**
```
Type: Checkbox
Default: false
Help Text: "If checked, users can submit multiple times for same period"
Use Case: OnDemand templates that can be submitted multiple times
```

---

### Tab 3: Access Control

**Access Level***
```
Type: Dropdown
Options:
- HeadOffice (Only HeadOffice users can be assigned)
- Regional (Regional + Factory users can be assigned)
- Factory (Only Factory users can be assigned)
- All (Any user can be assigned)
Validation: Required
Help Text: "Determines who can be assigned this template"
```

**Visible to Roles**
```
Type: Multi-select dropdown (optional)
Data Source: SELECT * FROM Roles WHERE RoleLevel matches Access Level
Display: RoleName
Help Text: "Optionally restrict visibility to specific roles"
Example: If Access Level = Regional, show only Regional-level roles
```

**Instructions for Submitters**
```
Type: Rich text editor
Max Length: 2000 characters
Validation: Optional
Features: Bold, Italic, Bullets, Numbering, Links
Help Text: "Guidance shown to users when filling the form"
Example: "Complete all sections. Use actual data from your factory. Submit by end of month."
```

**Support Contact**
```
Type: Text input
Format: Email
Validation: Optional, must be valid email if provided
Pattern: ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$
Help Text: "Email for users to contact if they have questions"
Example: "ict.support@ktda.co.ke"
```

---

### Validation Rules

**Cross-Tab Validation:**
```
1. Template Name + Category combination must be unique
2. Template Code must be globally unique
3. If Requires Approval = true, Workflow Definition required
4. Access Level must match user's permission level
```

**Field-Level Validation:**
```
- Template Name: Required, max 100 chars
- Template Code: Required, max 50 chars, pattern ^[A-Z0-9_]+$
- Description: Max 500 chars
- Category: Required
- Template Type: Required
- Estimated Time: 1-480 minutes
- Support Contact: Valid email format if provided
```

---

### Save Behavior

**New Template (Create Mode):**
```
POST /api/templates/create
Body: { all form fields }

Database Operations:
1. INSERT FormTemplates
   - PublishStatus = 'Draft'
   - Version = '1.0'
   - CreatedBy = CurrentUserId
   - CreatedDate = NOW()
   - IsActive = 1

2. Return TemplateId

3. Redirect: Navigate to Section Builder (Component 3)
   URL: /templates/{templateId}/sections
```

**Edit Existing Template:**
```
PUT /api/templates/{id}
Body: { modified fields }

IF PublishStatus = 'Draft':
  - UPDATE all fields
  - UPDATE ModifiedBy = CurrentUserId
  - UPDATE ModifiedDate = NOW()
  - Redirect: Navigate to Section Builder

ELSE IF PublishStatus = 'Published':
  - UPDATE only: Name, Description, Instructions, Support Contact
  - Show warning: "Limited fields updated. Create new version for structural changes."
  - Redirect: Back to Dashboard
```

**Cancel Action:**
```
Show confirmation if form is dirty:
"You have unsaved changes. Are you sure you want to leave?"
Actions: [Leave] [Stay]

If confirmed: Navigate back to Dashboard
```

---

### Conditional Field Logic

**Workflow Definition Dropdown:**
```javascript
// JavaScript logic
const requiresApprovalCheckbox = document.getElementById('requiresApproval');
const workflowDropdown = document.getElementById('workflowDefinition');

requiresApprovalCheckbox.addEventListener('change', function() {
    if (this.checked) {
        workflowDropdown.disabled = false;
        workflowDropdown.required = true;
        workflowDropdown.parentElement.style.display = 'block';
    } else {
        workflowDropdown.disabled = true;
        workflowDropdown.required = false;
        workflowDropdown.parentElement.style.display = 'none';
        workflowDropdown.value = '';
    }
});
```

**Visible to Roles Multi-Select:**
```javascript
// Filter roles based on Access Level
const accessLevelDropdown = document.getElementById('accessLevel');
const visibleToRolesSelect = document.getElementById('visibleToRoles');

accessLevelDropdown.addEventListener('change', function() {
    const selectedLevel = this.value;
    
    // Fetch roles matching the selected access level
    fetch(`/api/roles?level=${selectedLevel}`)
        .then(response => response.json())
        .then(roles => {
            // Clear existing options
            visibleToRoles.innerHTML = '';
            
            // Populate with filtered roles
            roles.forEach(role => {
                const option = new Option(role.roleName, role.roleId);
                visibleToRoles.add(option);
            });
        });
});
```

---

### Navigation Flow

```
Dashboard → [Create New] → Template Form (Create Mode)
  ↓
Fill Basic Info Tab → Fill Settings Tab → Fill Access Control Tab
  ↓
[Save] → Template saved as Draft
  ↓
Redirect to Section Builder (Component 3)
```

**Back Button:**
```
[← Back to Dashboard]
Shows confirmation if form has unsaved changes
```

**Next Button:**
```
[Save & Continue to Section Builder →]
Validates all tabs before saving
```

---

### Error Handling

**Duplicate Template Code:**
```
Error Message: "Template code 'TPL_IT_001' already exists. Please use a unique code or click Auto-Generate."
Field Highlight: Template Code field highlighted in red
```

**Missing Required Workflow:**
```
Error Message: "Workflow Definition is required when 'Requires Approval' is checked."
Tab Indicator: Settings tab shows error badge
Field Highlight: Workflow Definition dropdown highlighted
```

**Invalid Email Format:**
```
Error Message: "Please enter a valid email address."
Field: Support Contact
Pattern Check: ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$
```

---

### Success Messages

**Template Created:**
```
Toast Notification:
"Template '{TemplateName}' created successfully!"
Type: Success (green)
Duration: 3 seconds
Auto-dismiss: Yes
```

**Template Updated:**
```
Toast Notification:
"Template '{TemplateName}' updated successfully!"
Type: Success (green)
Duration: 3 seconds
```

---

**Next Steps:** After saving, user proceeds to Section Builder (Component 3) to add sections and fields

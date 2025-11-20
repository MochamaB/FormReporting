# AssignmentManager Implementation Guide - KTDA Form Reporting System

## 📋 Overview

The **AssignmentManager** is a highly flexible, reusable component for managing **any assignment relationship** in the system. It provides a consistent UI for assigning Users, Roles, Departments, UserGroups, and Tenants across different contexts.

**Pattern:** ViewModels → Extensions → Partial Views (with Modal)

**Universal Use Cases:**
- 👤 Users → Roles (Role membership)
- 🛡️ Roles → Users (User assignment to roles)
- 📋 Form Templates → Users/Roles/Departments (Access control)
- ✅ Approval Workflows → Users/Roles (Multi-level approvers)
- 👥 User Groups → Users (Group membership)
- 🏢 Tenant Groups → Tenants (Group membership)

---

## 🎯 Three-Layer Architecture

### **Layer 1: ViewModels** (WHAT to assign)
- `AssignmentManagerConfig` - Configuration object (what you create)
- `AssignmentType` - Type of entity to assign
- `AssignmentItem` - Individual assignment record

### **Layer 2: Extensions** (HOW to transform)
- `AssignmentManagerExtensions.BuildAssignmentManager()` - Main transformation
- Fluent helpers: `ForApprovalWorkflow()`, `ForFormAssignments()`, etc.

### **Layer 3: Partial Views** (HOW to render)
- `_AssignmentManager.cshtml` - Main component
- `_AddAssignmentModal.cshtml` - Add new assignments

---

## 📁 File Structure

```
Models/ViewModels/Components/
└── AssignmentManager.cs                   # Config and ViewModel classes

Extensions/
└── AssignmentManagerExtensions.cs         # Transformation logic

Views/Shared/Components/AssignmentManager/
├── _AssignmentManager.cshtml              # Main component (display table)
├── _AddAssignmentModal.cshtml             # Base modal shell
└── ModalContent/                          # Pluggable modal content per context
    ├── _AddWorkflowStepContent.cshtml     # For approval workflows
    ├── _AddUserRoleContent.cshtml         # For user-role assignments
    ├── _AddFormAssignmentContent.cshtml   # For form access control
    └── _AddTenantGroupContent.cshtml      # For tenant group membership

Controllers/
└── {YourController}.cs                    # Configure assignments
```

## 🔑 **Key Innovation: Pluggable Modal Content**

Different contexts require different fields! The AssignmentManager uses **pluggable modal content** similar to wizard step partials:

**Approval Workflow** needs: Step name, due days, escalation rules, parallel execution, conditions
**User-Role Assignment** needs: Simple user search only
**Form Assignments** needs: Type selection (User/Role/Dept) + search

Each context gets its own modal content partial with specific fields for its database model.

---

## 🔧 Complete Implementation Examples

### **EXAMPLE 1: Assign Users to Role (Role Edit Page)**

#### **Controller**
```csharp
// Controllers/Identity/RolesController.cs

[HttpGet("Edit/{id}")]
public async Task<IActionResult> Edit(int id)
{
    var role = await _context.Roles
        .Include(r => r.UserRoles)
        .ThenInclude(ur => ur.User)
        .FirstOrDefaultAsync(r => r.RoleId == id);
    
    if (role == null) return NotFound();
    
    var model = new RoleEditViewModel
    {
        RoleId = role.RoleId,
        RoleName = role.RoleName,
        // ... other properties
    };
    
    // ═══════════════════════════════════════════════════════════
    // ASSIGNMENT MANAGER CONFIGURATION
    // ═══════════════════════════════════════════════════════════
    
    var assignmentConfig = new AssignmentManagerConfig 
    { 
        ManagerId = "role-users",
        SearchEndpoint = Url.Action("SearchUsers", "Roles"),  // AJAX search endpoint
        ModalContentPartial = "~/Views/Shared/Components/AssignmentManager/ModalContent/_AddUserRoleContent.cshtml"  // ← Custom modal content!
    }
    .ForUserGroupMembers()  // Pre-configured for simple user list
    .AsCollapsible(initiallyCollapsed: false);
    
    // Load existing user assignments
    foreach (var userRole in role.UserRoles.Where(ur => ur.IsActive))
    {
        assignmentConfig.WithAssignment(
            assignmentType: "User",
            targetId: userRole.UserId,
            targetName: userRole.User.FullName,
            targetDetails: userRole.User.Email
        );
    }
    
    var assignmentManager = assignmentConfig.BuildAssignmentManager();
    ViewData["AssignmentManager"] = assignmentManager;
    
    return View(model);
}

[HttpGet("SearchUsers")]
public async Task<IActionResult> SearchUsers(string query)
{
    var users = await _context.Users
        .Where(u => u.IsActive && 
               (u.FullName.Contains(query) || u.Email.Contains(query)))
        .Select(u => new
        {
            id = u.UserId,
            type = "User",
            name = u.FullName,
            details = u.Email,
            badge = u.Department.DepartmentName
        })
        .Take(10)
        .ToListAsync();
    
    return Json(users);
}
```

#### **View**
```cshtml
@* Views/Identity/Roles/Edit.cshtml *@

@model RoleEditViewModel

<div class="row">
    <div class="col-12">
        <div class="card">
            <div class="card-header">
                <h5 class="card-title mb-0">Users in this Role</h5>
            </div>
            <div class="card-body">
                @* Render AssignmentManager Component *@
                <partial name="~/Views/Shared/Components/AssignmentManager/_AssignmentManager.cshtml" />
            </div>
        </div>
    </div>
</div>
```

---

### **EXAMPLE 2: Form Template Access Control**

#### **Controller**
```csharp
// Controllers/Forms/FormTemplatesController.cs

[HttpGet("Edit/{id}")]
public async Task<IActionResult> Edit(int id)
{
    var template = await _context.FormTemplates
        .Include(t => t.FormAssignments)
        .FirstOrDefaultAsync(t => t.TemplateId == id);
    
    if (template == null) return NotFound();
    
    var model = new FormTemplateEditViewModel { /* ... */ };
    
    // ═══════════════════════════════════════════════════════════
    // ASSIGNMENT MANAGER - Form Access Control
    // ═══════════════════════════════════════════════════════════
    
    var assignmentConfig = new AssignmentManagerConfig 
    { 
        ManagerId = "form-assignments",
        SearchEndpoint = Url.Action("SearchAssignable", "FormTemplates"),
        HelpText = "Define who can access and fill this form template"
    }
    .ForFormAssignments();  // Supports User, Role, Department, UserGroup, Tenant
    
    // Load existing assignments
    foreach (var assignment in template.FormAssignments)
    {
        assignmentConfig.WithAssignment(
            assignmentType: assignment.AssignmentType,  // "User", "Role", "Department", etc.
            targetId: assignment.TargetId,
            targetName: assignment.TargetName,
            targetDetails: assignment.TargetDetails
        );
    }
    
    ViewData["AssignmentManager"] = assignmentConfig.BuildAssignmentManager();
    return View(model);
}

[HttpGet("SearchAssignable")]
public async Task<IActionResult> SearchAssignable(string type, string query)
{
    // Search based on type
    if (type == "User")
    {
        var users = await _context.Users
            .Where(u => u.FullName.Contains(query))
            .Select(u => new { id = u.UserId, type = "User", name = u.FullName, details = u.Email })
            .Take(10)
            .ToListAsync();
        return Json(users);
    }
    else if (type == "Role")
    {
        var roles = await _context.Roles
            .Where(r => r.RoleName.Contains(query))
            .Select(r => new { id = r.RoleId, type = "Role", name = r.RoleName, details = r.Description })
            .Take(10)
            .ToListAsync();
        return Json(roles);
    }
    else if (type == "Department")
    {
        var departments = await _context.Departments
            .Where(d => d.DepartmentName.Contains(query))
            .Select(d => new { id = d.DepartmentId, type = "Department", name = d.DepartmentName, details = d.TenantName })
            .Take(10)
            .ToListAsync();
        return Json(departments);
    }
    // ... handle UserGroup, Tenant
    
    return Json(new List<object>());
}
```

---

### **EXAMPLE 3: Multi-Level Approval Workflow**

#### **Controller**
```csharp
// Controllers/Workflows/ApprovalWorkflowController.cs

[HttpGet("Configure/{formId}")]
public async Task<IActionResult> Configure(int formId)
{
    var workflow = await _context.ApprovalWorkflows
        .Include(w => w.ApprovalLevels)
        .FirstOrDefaultAsync(w => w.FormTemplateId == formId);
    
    if (workflow == null)
    {
        workflow = new ApprovalWorkflow { FormTemplateId = formId };
    }
    
    var model = new ApprovalWorkflowViewModel { /* ... */ };
    
    // ═══════════════════════════════════════════════════════════
    // ASSIGNMENT MANAGER - Approval Levels
    // ═══════════════════════════════════════════════════════════
    
    var assignmentConfig = new AssignmentManagerConfig 
    { 
        ManagerId = "approval-workflow",
        SearchEndpoint = Url.Action("SearchApprovers", "ApprovalWorkflow"),
        HelpText = "Define approval levels for this form. Each level must approve before moving to the next."
    }
    .ForApprovalWorkflow();  // Enables levels, restricts to User/Role
    
    // Load existing approval levels
    foreach (var level in workflow.ApprovalLevels.OrderBy(l => l.Level))
    {
        assignmentConfig.WithAssignment(
            assignmentType: level.ApproverType,  // "User" or "Role"
            targetId: level.ApproverId,
            targetName: level.ApproverName,
            targetDetails: level.ApproverDetails,
            level: level.Level,                  // ← Level number
            isMandatory: level.IsMandatory
        );
    }
    
    ViewData["AssignmentManager"] = assignmentConfig.BuildAssignmentManager();
    return View(model);
}
```

#### **View**
```cshtml
@* Views/Workflows/Configure.cshtml *@

<div class="card">
    <div class="card-header">
        <h5>Approval Workflow Configuration</h5>
    </div>
    <div class="card-body">
        <partial name="~/Views/Shared/Components/AssignmentManager/_AssignmentManager.cshtml" />
    </div>
</div>
```

**Visual Result:**
```
┌────────────────────────────────────────────────────┐
│ Approval Levels                    [3]  [+ Add]    │
├────────────────────────────────────────────────────┤
│ Level │ Type    │ Name            │ Details        │
├───────┼─────────┼─────────────────┼────────────────┤
│ L1 *  │ 👤 User │ Supervisor      │ super@ktda.com │
│ L2 *  │ 👤 User │ Manager         │ mgr@ktda.com   │
│ L3    │ 🛡️ Role │ HOD             │ -              │
└────────────────────────────────────────────────────┘
```

---

### **EXAMPLE 4: Tenant Group Membership**

#### **Controller**
```csharp
[HttpGet("Edit/{id}")]
public async Task<IActionResult> Edit(int id)
{
    var group = await _context.TenantGroups
        .Include(g => g.TenantGroupMembers)
        .ThenInclude(m => m.Tenant)
        .FirstOrDefaultAsync(g => g.GroupId == id);
    
    var model = new TenantGroupEditViewModel { /* ... */ };
    
    var assignmentConfig = new AssignmentManagerConfig 
    { 
        ManagerId = "tenant-group-members",
        ContextLabel = "Group Members (Factories/Branches)",
        HelpText = "Factories and branches assigned to this tenant group",
        SearchEndpoint = Url.Action("SearchTenants", "TenantGroups")
    }
    .WithAssignmentType("Tenant", "Factory/Branch", "ri-community-line", isDefault: true);
    
    foreach (var member in group.TenantGroupMembers)
    {
        assignmentConfig.WithAssignment(
            "Tenant",
            member.TenantId,
            member.Tenant.TenantName,
            member.Tenant.Location
        );
    }
    
    ViewData["AssignmentManager"] = assignmentConfig.BuildAssignmentManager();
    return View(model);
}
```

---

## 🎨 **Modal Content Pattern (Context-Specific Forms)**

### **Why Pluggable Modal Content?**

Different assignment contexts have **completely different database models** with unique fields:

| Context | Model | Required Fields |
|---------|-------|----------------|
| **Approval Workflow** | `WorkflowStep` | StepOrder, StepName, ApproverUserId/RoleId, IsMandatory, IsParallel, DueDays, EscalationRoleId, AutoApproveCondition |
| **User-Role** | `UserRole` | UserId, RoleId, AssignedDate, AssignedBy |
| **Form Assignment** | `FormAssignment` | AssignmentType, TargetId, FormTemplateId |
| **Tenant Group** | `TenantGroupMember` | TenantId, GroupId |

**Solution:** Each context uses a **custom modal content partial** with fields matching its database model.

### **Modal Architecture:**

```
_AddAssignmentModal.cshtml (Base Shell)
├── Modal Header (Standard) ✓
├── Modal Body 
│   └── <partial name="@Model.ModalContentPartial" /> ← Pluggable!
└── Modal Footer (Standard) ✓
```

### **Available Modal Content Partials:**

| Partial File | Complexity | Use Case | Database Model | Key Fields |
|--------------|------------|----------|----------------|------------|
| **_AddWorkflowStepContent.cshtml** | Complex | Approval workflows | `WorkflowStep` | StepOrder, StepName, ApproverUserId/RoleId, DueDays, Escalation, IsParallel, AutoApproveCondition |
| **_AddUserRoleContent.cshtml** | Simple | User-role assignments | `UserRole` | UserId, AssignedDate |
| **_AddFormAssignmentContent.cshtml** | Medium | Form access control | `FormAssignment` | AssignmentType, TargetId, AccessLevel, ValidFrom/To |
| **_AddTenantGroupContent.cshtml** | Simple | Tenant group membership | `TenantGroupMember` | TenantId, AssignedDate, IsActive |

### **Detailed Descriptions:**

#### **1. _AddWorkflowStepContent.cshtml** (Complex)
**Use for:** Approval workflow configuration
**Fields:** Step order, step name, approver type, approver search, due days, escalation role, is mandatory, is parallel, auto-approve condition
**Example:** Multi-level approval process for expense forms

#### **2. _AddUserRoleContent.cshtml** (Simple)
**Use for:** User-role assignments, user group membership
**Fields:** User search, assigned date
**Example:** Assigning users to "Manager" role, adding members to user groups

#### **3. _AddFormAssignmentContent.cshtml** (Medium)
**Use for:** Form template access control
**Fields:** Assignment type (User/Role/Dept/UserGroup/Tenant), target search, access level, validity dates
**Example:** Defining who can fill the "Daily Production Report" form

#### **4. _AddTenantGroupContent.cshtml** (Simple)
**Use for:** Tenant group membership
**Fields:** Tenant search (factory/branch), assigned date, is active
**Example:** Adding factories to "Western Region Group"

---

## 🎨 **Pre-Built Contexts (Fluent Helpers)**

### **1. ForApprovalWorkflow()**
```csharp
.ForApprovalWorkflow()
```

**Pre-configured:**
- ContextLabel: "Approval Levels"
- ShowLevels: `true` (displays level column)
- AllowMultiplePerLevel: `false` (one approver per level)
- SupportedTypes: User, Role only
- **ModalContentPartial:** `_AddWorkflowStepContent.cshtml` ✨
- **Best for:** Sequential approval processes

### **2. ForFormAssignments()**
```csharp
.ForFormAssignments()
```

**Pre-configured:**
- ContextLabel: "Form Assignments"
- ShowLevels: `false`
- AllowMultiplePerLevel: `true`
- SupportedTypes: User, Role, Department, UserGroup, Tenant
- **ModalContentPartial:** `_AddFormAssignmentContent.cshtml` ✨
- **Best for:** Form template access control

### **3. ForUserGroupMembers()**
```csharp
.ForUserGroupMembers()
```

**Pre-configured:**
- ContextLabel: "Group Members"
- ShowLevels: `false`
- SupportedTypes: User only
- **ModalContentPartial:** `_AddUserRoleContent.cshtml` ✨
- **Best for:** User group membership, role membership

### **4. Custom Configuration**
```csharp
new AssignmentManagerConfig
{
    ManagerId = "custom-id",
    ContextLabel = "Custom Label",
    ShowLevels = true/false,
    SearchEndpoint = "/api/search",
    ModalContentPartial = "~/Views/.../YourCustomContent.cshtml"  // ← Specify your modal content
}
.WithAssignmentType("User", "Specific User", "ri-user-line", isDefault: true)
.WithAssignmentType("Role", "By Role", "ri-shield-user-line")
```

**Note:** Fluent helpers automatically set the appropriate `ModalContentPartial` for their context. You can override it if needed.

---

## ⚙️ **Configuration Options**

```csharp
var config = new AssignmentManagerConfig
{
    // Required
    ManagerId = "unique-id",                    // Unique identifier for this instance
    
    // Display
    ContextLabel = "Assignments",               // Header title
    HelpText = "Optional help text",            // Shown below title
    
    // Modal Content (NEW!)
    ModalContentPartial = "~/Views/.../ModalContent/_YourContent.cshtml",  // Custom modal form for context
    
    // Features
    ShowLevels = false,                         // Show level/step column
    AllowMultiplePerLevel = true,               // Multiple per level
    ShowAddButton = true,                       // Show add button
    ShowRemoveButton = true,                    // Show remove buttons
    
    // Collapsible
    IsCollapsible = false,                      // Make section collapsible
    InitiallyCollapsed = true,                  // Start collapsed
    
    // AJAX
    SearchEndpoint = "/api/search",             // Search endpoint for modal
    
    // Styling
    CssClasses = "custom-class"                 // Additional CSS classes
};
```

---

## 🔄 **User Interaction Flow**

### **Add Assignment Flow:**
```
1. User clicks [+ Add] button
   ↓
2. Modal opens
   ↓
3. User selects assignment type (User/Role/Department/etc.)
   [Radio button group]
   ↓
4. If ShowLevels: User selects level
   [Dropdown: Level 1, 2, 3...]
   ↓
5. User types in search box
   [Type-ahead search with AJAX]
   ↓
6. Results appear, user selects one
   [List of matching entities]
   ↓
7. User toggles "Mandatory" checkbox
   ↓
8. User clicks "Add Assignment"
   ↓
9. New row appears in assignments table
   Badge count updates
```

### **Remove Assignment Flow:**
```
1. User clicks remove button (🗑️)
   ↓
2. Confirmation dialog appears
   ↓
3. User confirms
   ↓
4. Row fades out and is removed
   Badge count updates
```

---

## 🎨 **Visual Components**

### **Assignment Type Badges**
```
👤 User       → Blue badge     (bg-primary)
🛡️ Role       → Green badge    (bg-success)
🏢 Department → Cyan badge     (bg-info)
👥 UserGroup  → Orange badge   (bg-warning)
🏭 Tenant     → Gray badge     (bg-secondary)
```

### **Level Badges (When ShowLevels = true)**
```
L1, L2, L3... → Gray badges (bg-secondary)
* = Mandatory (red asterisk)
```

### **Empty State**
```
┌────────────────────────────────┐
│          👤                    │
│    No assignments yet          │
│  Click "Add" to assign users   │
└────────────────────────────────┘
```

---

## 🔌 **AJAX Search Endpoint**

### **Backend Endpoint Structure:**
```csharp
[HttpGet]
public async Task<IActionResult> SearchAssignable(string type, string query)
{
    // Return JSON array of matching entities
    // Format: { id, type, name, details, badge? }
    
    if (type == "User")
    {
        var users = await _context.Users
            .Where(u => u.FullName.Contains(query) || u.Email.Contains(query))
            .Select(u => new
            {
                id = u.UserId,
                type = "User",
                name = u.FullName,
                details = u.Email,
                badge = u.Department.DepartmentName  // Optional
            })
            .Take(10)
            .ToListAsync();
        
        return Json(users);
    }
    
    // Handle other types...
    return Json(new List<object>());
}
```

### **Expected JSON Response:**
```json
[
  {
    "id": 1,
    "type": "User",
    "name": "John Doe",
    "details": "john.doe@ktda.com",
    "badge": "ICT Department"
  },
  {
    "id": 5,
    "type": "Role",
    "name": "Data Entry Clerk",
    "details": "Can enter form data"
  }
]
```

---

## 📊 **When to Use AssignmentManager**

### ✅ **Use For:**
- User-to-role assignments
- Role-to-user assignments
- Form template access control
- Approval workflow configuration
- User group memberships
- Tenant group memberships
- Report recipient lists
- Any many-to-many relationship

### ❌ **Don't Use For:**
- Simple one-to-one relationships → Use SimpleForms dropdown
- Fixed/static relationships → Use configuration
- Complex hierarchical structures → Build custom component

---

## 🚀 **Quick Start Template**

```csharp
// Controller
var config = new AssignmentManagerConfig { ManagerId = "my-assignments" }
    .ForFormAssignments()  // or .ForApprovalWorkflow() or custom
    .WithAssignment("User", 1, "John Doe", "john@example.com")
    .WithAssignment("Role", 5, "Manager", null)
    .BuildAssignmentManager();

ViewData["AssignmentManager"] = config;

// View
<partial name="~/Views/Shared/Components/AssignmentManager/_AssignmentManager.cshtml" />

// AJAX Endpoint
[HttpGet]
public async Task<IActionResult> Search(string type, string query)
{
    // Return JSON: [{ id, type, name, details }]
}
```

---

## 📝 **Creating Custom Modal Content**

To create your own modal content partial for a specific context:

### **Step 1: Create Partial File**
```
Views/Shared/Components/AssignmentManager/ModalContent/
└── _AddYourContextContent.cshtml
```

### **Step 2: Define Form Fields**
```cshtml
@model FormReporting.Models.ViewModels.Components.AssignmentManagerViewModel

@* Fields matching your database model *@
<div class="mb-3">
    <label>Your Field</label>
    <input type="text" class="form-control" id="yourField_@Model.ManagerId" />
</div>

@* Include search, dropdowns, dates, etc. as needed *@
```

### **Step 3: Add JavaScript**
```javascript
<script>
    $(document).ready(function() {
        var managerId = '@Model.ManagerId';
        
        // Your custom JavaScript logic
        // - AJAX search
        // - Validation
        // - Business logic
    });
</script>
```

### **Step 4: Configure in Controller**
```csharp
var config = new AssignmentManagerConfig 
{ 
    ModalContentPartial = "~/Views/Shared/Components/AssignmentManager/ModalContent/_AddYourContextContent.cshtml"
}
```

---

## ✅ **Implementation Checklist**

- [ ] Identify database model for your assignment context
- [ ] Choose appropriate modal content partial (or create custom)
- [ ] Create AssignmentManagerConfig in controller
- [ ] Set `ModalContentPartial` property
- [ ] Use fluent helper or custom configuration
- [ ] Load existing assignments with `.WithAssignment()`
- [ ] Call `.BuildAssignmentManager()`
- [ ] Store in ViewData["AssignmentManager"]
- [ ] Render partial in view
- [ ] Create AJAX search endpoint
- [ ] Handle POST to save assignments to database
- [ ] Test add/remove functionality
- [ ] Verify type badges display correctly
- [ ] Test modal content with actual data

---

**The AssignmentManager with pluggable modal content provides a flexible, powerful UI for managing all assignment relationships across the KTDA system, adapting to each context's unique database model.**

# TODO List - FormReporting Project

## Assignment Wizard - Target Selection Step

### 1. Create UserGroups on the Fly
**Priority:** Medium  
**Status:** Pending

**Current Issue:**  
When selecting "User Group" as assignment type, users can only choose from existing groups. There's no way to create a new group inline.

**Implementation Plan:**
1. **Add API endpoint** in `AssignmentsApiController.cs`:
   ```csharp
   [HttpPost("user-groups")]
   public async Task<IActionResult> CreateUserGroup([FromBody] CreateUserGroupRequest request)
   ```

2. **Create request model**:
   ```csharp
   public class CreateUserGroupRequest
   {
       public string GroupName { get; set; }
       public string GroupCode { get; set; }
       public string? GroupType { get; set; }
       public string? Description { get; set; }
       public List<int>? InitialMemberIds { get; set; }
   }
   ```

3. **Add modal to Target Step UI** (`_AssignmentTargetStep.cshtml`):
   - "Create New Group" button next to dropdown
   - Modal with fields: GroupName, GroupCode, GroupType, Description
   - Optional: Multi-select for initial members
   - On save: POST to API, refresh dropdown, auto-select new group

4. **Files to modify:**
   - `Controllers/API/AssignmentsApiController.cs` - Add endpoint
   - `Views/Forms/FormTemplates/Assignments/Steps/_AssignmentTargetStep.cshtml` - Add modal and button
   - `Services/Forms/FormAssignmentService.cs` - Add CreateUserGroupAsync method (or use dedicated UserGroupService)

---

### 2. Display Users by Department (Hierarchical Selection)
**Priority:** Medium  
**Status:** Pending

**Current Issue:**  
The "Specific User" dropdown returns all users (up to 500) in a flat list grouped by tenant. This is not user-friendly for large organizations.

**Proposed Solution:**  
Implement a cascading/hierarchical selection:
1. First select a Tenant (if scope allows multiple)
2. Then select a Department
3. Then select a User from that department

**Implementation Plan:**

1. **Option A: Cascading Dropdowns**
   - Add tenant dropdown (if GLOBAL/REGIONAL scope)
   - Add department dropdown (filtered by selected tenant)
   - Add user dropdown (filtered by selected department)
   - Each dropdown triggers AJAX to load next level

2. **Option B: Searchable Select with AJAX**
   - Use a searchable select component (e.g., Select2, Choices.js)
   - Load users on-demand as user types (minimum 3 characters)
   - API endpoint: `GET /api/assignments/search-users?query=john&tenantId=5&departmentId=10`

3. **Option C: Tree View Selection**
   - Display hierarchical tree: Tenant → Department → Users
   - Allow expanding/collapsing nodes
   - Single-select a user from the tree

**Recommended:** Option B (Searchable Select) for best UX with large datasets.

**Files to modify:**
- `Controllers/API/AssignmentsApiController.cs` - Add search endpoint
- `Services/Forms/FormAssignmentService.cs` - Add SearchUsersAsync method
- `Views/Forms/FormTemplates/Assignments/Steps/_AssignmentTargetStep.cshtml` - Replace dropdown with searchable select

---

## Other Pending Items

### 3. Assignment Wizard - Schedule Step
**Priority:** High  
**Status:** Next to implement

See implementation details in `AssignmentWorkflowUI.md`

---

### 4. Assignment Wizard - Review Step
**Priority:** High  
**Status:** Pending

---

### 5. Form Submission Workflow
**Priority:** High  
**Status:** Pending

---

### Form Assignment Edit, Delete, extend period, revole access
**Priority:** High  
**Status:** Pending


*Last Updated: December 16, 2025*

# SECTION 1: ORGANIZATIONAL STRUCTURE - Workflows & Actions

**Module:** Multi-Tenancy & Organizational Hierarchy  
**Tables:** 5 (Regions, Tenants, TenantGroups, TenantGroupMembers, Departments)

---

## 1. REGIONS

### **CRUD Operations:**
- **CREATE** Region
- **READ** Region (Single)
- **READ** All Regions (List)
- **UPDATE** Region Details
- **DELETE** Region (Soft delete - sets IsActive = 0)

### **Business Rules:**
- RegionNumber must be unique
- RegionCode must be unique
- Cannot delete Region if Tenants exist (FK constraint)

### **Workflows:**
None (Simple CRUD only)

---

## 2. TENANTS

### **CRUD Operations:**
- **CREATE** Tenant
- **READ** Tenant (Single)
- **READ** All Tenants (List with filters)
- **READ** Tenants by Region
- **READ** Tenants by Type (Factory/HeadOffice/Subsidiary)
- **UPDATE** Tenant Details
- **DELETE** Tenant (Soft delete - sets IsActive = 0)

### **Business Rules:**
- TenantCode must be unique
- Only one TenantType = 'HeadOffice' allowed
- TenantType must be 'Factory', 'HeadOffice', or 'Subsidiary'
- If TenantType = 'Factory', RegionId is required
- If TenantType = 'HeadOffice', RegionId must be NULL
- ParentTenantId used for subsidiaries (optional)

### **Workflows:**

#### **WF-1.1: Tenant Activation**
```
Trigger: User creates new Tenant
Steps:
  1. Validate TenantCode uniqueness
  2. If TenantType = 'Factory' → Require RegionId
  3. If TenantType = 'HeadOffice' → Check only one HO exists
  4. Save Tenant with IsActive = 1
  5. Auto-create default Department "General" for this Tenant
  6. Trigger notification to Regional Manager (if Factory)
```

#### **WF-1.2: Tenant Deactivation**
```
Trigger: User deactivates Tenant
Steps:
  1. Check if Tenant has active users
     - If yes → Prompt: "X active users. Deactivate all users first?"
  2. Check if Tenant has pending form submissions
     - If yes → Prompt: "X pending submissions. Complete or cancel first?"
  3. Set IsActive = 0
  4. Cascade deactivate all associated users
  5. Log action in AuditLogs
  6. Notify Head Office ICT Manager
```

---

## 3. TENANT GROUPS

### **CRUD Operations:**
- **CREATE** Tenant Group
- **READ** Tenant Group (Single)
- **READ** All Tenant Groups (List)
- **UPDATE** Tenant Group Details
- **DELETE** Tenant Group (Cascade deletes TenantGroupMembers)

### **Business Rules:**
- GroupCode must be unique
- GroupType must be 'Region', 'Project', 'Performance', or 'Custom'
- IsActive defaults to 1

### **Workflows:**

#### **WF-1.3: Create Tenant Group**
```
Trigger: User creates new Tenant Group
Steps:
  1. Validate GroupCode uniqueness
  2. Set CreatedBy = Current UserId
  3. Set CreatedDate = NOW()
  4. Save TenantGroup
  5. Navigate to "Add Tenants" screen
```

#### **WF-1.4: Attach Tenants to Group**
```
Trigger: User selects "Add Tenants to Group" action
Steps:
  1. Display modal with list of all active Tenants
  2. Show checkboxes (exclude already added Tenants)
  3. User selects multiple Tenants
  4. For each selected Tenant:
     a. INSERT INTO TenantGroupMembers (GroupId, TenantId, AddedDate, AddedBy)
  5. Refresh Tenant Group members list
  6. Show success message: "X tenants added to group"
```

#### **WF-1.5: Detach Tenant from Group**
```
Trigger: User clicks "Remove" next to Tenant in group
Steps:
  1. Confirm: "Remove {TenantName} from {GroupName}?"
  2. DELETE FROM TenantGroupMembers WHERE GroupId = X AND TenantId = Y
  3. Refresh list
  4. Show success message: "{TenantName} removed from group"
```

#### **WF-1.6: Bulk Assign by Region**
```
Trigger: User selects "Auto-add all Factories in Region X"
Steps:
  1. User selects Region from dropdown
  2. System queries: SELECT TenantId FROM Tenants WHERE RegionId = X AND TenantType = 'Factory'
  3. For each TenantId:
     a. Check if already in group (prevent duplicates)
     b. If not → INSERT INTO TenantGroupMembers
  4. Show summary: "X factories from {RegionName} added to group"
```

---

## 4. TENANT GROUP MEMBERS

### **CRUD Operations:**
- **CREATE** Group Member (via WF-1.4)
- **READ** Group Members (List for specific group)
- **DELETE** Group Member (via WF-1.5)

### **Business Rules:**
- Unique constraint: (GroupId, TenantId) - same Tenant cannot be added twice
- ON DELETE CASCADE when TenantGroup is deleted
- AddedBy tracks who added the Tenant to group

### **Workflows:**
See WF-1.4, WF-1.5, WF-1.6 above (managed through TenantGroups UI)

---

## 5. DEPARTMENTS

### **CRUD Operations:**
- **CREATE** Department
- **READ** Department (Single)
- **READ** Departments by Tenant
- **READ** All Active Departments (List)
- **UPDATE** Department Details
- **DELETE** Department (Check for associated users first)

### **Business Rules:**
- DepartmentCode must be unique within Tenant (compound unique: TenantId + DepartmentCode)
- ParentDepartmentId allows hierarchical structure (e.g., ICT > Networking)
- Cannot delete Department if Users exist (FK from Users.DepartmentId)

### **Workflows:**

#### **WF-1.7: Create Department**
```
Trigger: Tenant Admin creates new Department
Steps:
  1. Validate DepartmentCode uniqueness within Tenant
  2. If ParentDepartmentId specified:
     a. Validate parent exists and belongs to same Tenant
  3. Save Department with IsActive = 1
  4. Show success: "Department {Name} created"
```

#### **WF-1.8: Department Hierarchy View**
```
Trigger: User views Departments for a Tenant
Display:
  - Tree view showing parent-child relationships
  - ICT Department
    ├─ Networking
    ├─ Support
    └─ Development
  - Finance Department
    ├─ Accounts Payable
    └─ Payroll
```

#### **WF-1.9: Delete Department with Users Check**
```
Trigger: User attempts to delete Department
Steps:
  1. Query: SELECT COUNT(*) FROM Users WHERE DepartmentId = X
  2. If count > 0:
     a. Show error: "Cannot delete. X users assigned to this department."
     b. Suggest: "Reassign users first or deactivate department instead."
  3. If count = 0:
     a. Confirm: "Delete {DepartmentName}?"
     b. DELETE Department
  4. Check for child departments:
     a. If children exist → Set ParentDepartmentId = NULL or cascade delete
```

---

## CROSS-TABLE WORKFLOWS

### **WF-1.10: Tenant Creation Wizard (Full Setup)**
```
Trigger: Admin selects "Add New Factory"
Steps:
  1. Step 1: Basic Details
     - TenantName, TenantCode, TenantType
     - Select Region (if Factory)
     - Contact details
  
  2. Step 2: Create Default Departments
     - Auto-create: General, ICT, Finance, Operations
     - Option to add more departments
  
  3. Step 3: Assign to Groups (Optional)
     - Show available TenantGroups
     - Multi-select to add to groups
  
  4. Step 4: Summary & Confirm
     - Review all details
     - Click "Create Tenant"
  
  5. Post-Creation:
     - Create Tenant record
     - Create Department records
     - Create TenantGroupMembers records
     - Trigger notification to Regional Manager
     - Redirect to "Add Users" screen
```

### **WF-1.11: Organizational Structure Report**
```
Trigger: User runs "Org Structure" report
Query Logic:
  - Group Tenants by Region
  - Show Tenant count per Region
  - Show Department count per Tenant
  - Show User count per Department
  - Display TenantGroup memberships

Output Example:
  Region 1: Mt. Kenya
    ├─ Factory A (5 departments, 25 users)
    │  └─ Groups: High Performers, Q4 Project
    ├─ Factory B (4 departments, 18 users)
    └─ Factory C (6 departments, 32 users)
  
  Region 2: Rift Valley
    ├─ Factory D (5 departments, 28 users)
    └─ ...
```

---

## SUMMARY

### **Total Operations:**
- **CRUD Actions:** 20 basic operations across 5 tables
- **Workflows:** 11 defined workflows
- **Business Rules:** 15+ validation rules

### **Key Integration Points:**
1. **Users Table** → Links to Tenants, Departments
2. **UserTenantAccess Table** → Multi-tenant user access
3. **FormTemplateAssignments** → Form access by Tenant/TenantGroup
4. **Notifications** → Regional/Tenant-scoped notifications
5. **Reporting** → Tenant/Region aggregations

### **Permissions Required:**
- **System Admin:** Full CRUD on all tables
- **Regional Manager:** CRUD on Tenants in their Region only
- **Tenant Admin:** CRUD on Departments in their Tenant only
- **General User:** Read-only access

---

**End of Section 1 Workflows**

# FormItemOptionTemplates Migration Instructions

## ✅ FILES CREATED

### **1. Model Files**
- ✅ `Models/Entities/Forms/FormItemOptionTemplate.cs`
- ✅ `Models/Entities/Forms/FormItemOptionTemplateItem.cs`
- ✅ `Models/Entities/Forms/FormItemOptionTemplateCategory.cs`
- ✅ `Models/Entities/Forms/FormItemOption.cs` (Updated with ScoreValue & ScoreWeight)

### **2. Configuration Files**
- ✅ `Data/Configurations/Forms/FormItemOptionTemplateConfiguration.cs`
  - Contains configurations for all three template entities

### **3. Seeder File**
- ✅ `Data/Seeders/FormItemOptionTemplateSeeder.cs`
  - Seeds 12 standard option templates with 50+ options

### **4. Database Context**
- ✅ `Data/ApplicationDbContext.cs` (Updated)
  - Added 3 new DbSets
  - Applied configurations in OnModelCreating

---

## 📋 MIGRATION COMMANDS

### **Step 1: Build the Project**

First, make sure the project builds successfully:

```powershell
dotnet build
```

**Expected:** Build succeeds with no errors.

---

### **Step 2: Create the Migration**

Open **Package Manager Console** in Visual Studio:
- **Tools** → **NuGet Package Manager** → **Package Manager Console**

Run:

```powershell
Add-Migration AddFormItemOptionTemplatesAndScoring -Context ApplicationDbContext
```

**OR** using .NET CLI in PowerShell:

```powershell
dotnet ef migrations add AddFormItemOptionTemplatesAndScoring --context ApplicationDbContext
```

**Expected Output:**
```
Build started...
Build succeeded.
To undo this action, use Remove-Migration.
```

---

### **Step 3: Review the Migration**

Check the generated migration file in:
```
Data/Migrations/[Timestamp]_AddFormItemOptionTemplatesAndScoring.cs
```

**The migration should include:**

1. ✅ Add `ScoreValue` column to `FormItemOptions` table
2. ✅ Add `ScoreWeight` column to `FormItemOptions` table
3. ✅ Create `FormItemOptionTemplates` table
4. ✅ Create `FormItemOptionTemplateItems` table
5. ✅ Create `FormItemOptionTemplateCategories` table
6. ✅ Create indexes and foreign keys

---

### **Step 4: Apply the Migration**

```powershell
Update-Database -Context ApplicationDbContext
```

**OR** using .NET CLI:

```powershell
dotnet ef database update --context ApplicationDbContext
```

**Expected Output:**
```
Build started...
Build succeeded.
Applying migration '20241201XXXXXX_AddFormItemOptionTemplatesAndScoring'.
Done.
```

---

### **Step 5: Verify Database Changes**

Open **SQL Server Management Studio** or **Azure Data Studio** and check:

#### **New Tables Created:**
```sql
-- Check new tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN (
    'FormItemOptionTemplates',
    'FormItemOptionTemplateItems',
    'FormItemOptionTemplateCategories'
)
```

#### **Updated Table:**
```sql
-- Check ScoreValue column was added to FormItemOptions
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'FormItemOptions' 
  AND COLUMN_NAME IN ('ScoreValue', 'ScoreWeight')
```

---

### **Step 6: Run the Seeder (Optional)**

The seeder will populate 12 standard templates with 50+ options.

**Option A: Add to Program.cs startup (Recommended)**

Add this code in `Program.cs` before `app.Run()`:

```csharp
// Seed option templates on startup (only runs once)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Run option template seeder
        var templateSeeder = new FormItemOptionTemplateSeeder(context);
        await templateSeeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding option templates");
    }
}
```

Then run the application once to seed.

**Option B: Manual Seeding via Controller/API**

Create a temporary endpoint to trigger seeding:

```csharp
[HttpPost("api/admin/seed-option-templates")]
[Authorize(Roles = "Administrator")]
public async Task<IActionResult> SeedOptionTemplates()
{
    var seeder = new FormItemOptionTemplateSeeder(_context);
    await seeder.SeedAsync();
    return Ok("Templates seeded successfully");
}
```

**Option C: Database Script**

Manually run SQL inserts (see seeder file for data).

---

### **Step 7: Test the Migration**

#### **Test 1: Query Templates**

```sql
-- Check seeded templates
SELECT 
    TemplateId,
    TemplateName,
    TemplateCode,
    Category,
    HasScoring,
    (SELECT COUNT(*) FROM FormItemOptionTemplateItems WHERE TemplateId = t.TemplateId) AS OptionCount
FROM FormItemOptionTemplates t
ORDER BY DisplayOrder
```

**Expected:** 12 rows with template data.

#### **Test 2: Query Template Items**

```sql
-- Check template options
SELECT 
    ti.TemplateId,
    t.TemplateName,
    ti.OptionLabel,
    ti.ScoreValue,
    ti.ColorHint
FROM FormItemOptionTemplateItems ti
INNER JOIN FormItemOptionTemplates t ON ti.TemplateId = t.TemplateId
WHERE t.TemplateCode = 'SATISFACTION_5PT'
ORDER BY ti.DisplayOrder
```

**Expected:** 5 rows (Very Satisfied to Very Dissatisfied).

#### **Test 3: EF Core Query**

```csharp
// Test in a controller or service
var templates = await _context.FormItemOptionTemplates
    .Include(t => t.Items)
    .Where(t => t.IsActive)
    .OrderBy(t => t.DisplayOrder)
    .ToListAsync();

Console.WriteLine($"Found {templates.Count} templates");
foreach (var template in templates)
{
    Console.WriteLine($"  - {template.TemplateName}: {template.Items.Count} options");
}
```

---

## 🎯 SEEDED TEMPLATES

After seeding, you'll have these templates:

### **Rating Category (3 templates)**
1. Satisfaction Scale (5-point)
2. Satisfaction Scale (3-point)
3. Quality Scale

### **Agreement Category (2 templates)**
4. Agree - Disagree (5-point)
5. Agree - Disagree (7-point)

### **Binary Category (2 templates)**
6. Yes - No
7. True - False

### **Frequency Category (1 template)**
8. Frequency Scale (Always - Never)

### **Likelihood Category (1 template)**
9. Likelihood Scale

### **Custom/KTDA Category (3 templates)**
10. Operational Status
11. Condition Assessment
12. Compliance Status

**Total:** 12 templates with ~55 total options

---

## 🚨 TROUBLESHOOTING

### **Error: "Build failed"**

**Solution:** Check that all using statements are correct in the model files.

```csharp
using FormReporting.Models.Entities.Organizational;
using FormReporting.Models.Entities.Identity;
```

### **Error: "Type already has a key defined"**

**Solution:** Check that the `[Key]` attribute is only on one property per model.

### **Error: "Foreign key constraint failed"**

**Solution:** Make sure `Tenants` and `Users` tables exist before running migration.

### **Error: "Duplicate key value in unique index"**

**Solution:** If re-seeding, clear existing data first:

```sql
DELETE FROM FormItemOptionTemplateItems;
DELETE FROM FormItemOptionTemplates;
DBCC CHECKIDENT ('FormItemOptionTemplates', RESEED, 0);
DBCC CHECKIDENT ('FormItemOptionTemplateItems', RESEED, 0);
```

---

## 📊 DATABASE SCHEMA

### **FormItemOptionTemplates** (12 rows expected)
```
TemplateId (PK)
TemplateName
TemplateCode (UNIQUE)
Category
SubCategory
Description
UsageCount
DisplayOrder
ApplicableFieldTypes (JSON)
RecommendedFor
HasScoring
ScoringType
IsSystemTemplate
TenantId (FK, nullable)
IsActive
CreatedDate
CreatedBy (FK)
ModifiedDate
ModifiedBy (FK)
```

### **FormItemOptionTemplateItems** (~55 rows expected)
```
TemplateItemId (PK)
TemplateId (FK)
OptionValue
OptionLabel
DisplayOrder
ScoreValue
ScoreWeight
IconClass
ColorHint
IsDefault
```

### **FormItemOptions** (Existing table - updated)
```
... existing columns ...
ScoreValue (NEW)
ScoreWeight (NEW)
```

---

## ✅ VERIFICATION CHECKLIST

- [ ] Build succeeds with no errors
- [ ] Migration file created successfully
- [ ] Database updated successfully
- [ ] `FormItemOptionTemplates` table exists with 12 rows
- [ ] `FormItemOptionTemplateItems` table exists with ~55 rows
- [ ] `FormItemOptions` has `ScoreValue` and `ScoreWeight` columns
- [ ] EF Core can query templates successfully
- [ ] Seeder runs without errors (if used)

---

## 🎉 SUCCESS!

Once all steps are complete, you're ready to:

1. **Next:** Build the API controller (`FormItemOptionTemplatesApiController`)
2. **Next:** Implement Form Builder UI integration
3. **Next:** Create template picker dropdown

---

**Created:** December 1, 2025  
**Migration Name:** `AddFormItemOptionTemplatesAndScoring`  
**Tables Added:** 3  
**Columns Updated:** FormItemOptions (+2)  
**Seed Data:** 12 templates, ~55 options

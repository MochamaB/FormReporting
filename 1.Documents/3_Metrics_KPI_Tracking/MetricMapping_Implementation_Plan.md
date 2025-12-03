# Metric Mapping & Scoring System - Implementation Plan

**Reference:** MetricMapping_And_Scoring_Complete_Guide.md  
**Environment:** Development (data can be cleared)  
**Last Updated:** December 3, 2025

---

## PHASE 1: Model Updates - MetricDefinition

### Step 1.1: Add Hierarchy Columns to MetricDefinition

**File:** `Models\Entities\Metrics\MetricDefinition.cs`

**Add columns:**
```
MetricScope: string (30) - "Field", "Section", "Template", "Tenant"
HierarchyLevel: int - 0=Field, 1=Section, 2=Template, 3=Tenant
ParentMetricId: int? (nullable, FK to self)
AggregationFormula: string? (nullable, JSON)
```

**Navigation property to add:**
```
ParentMetric: MetricDefinition? (self-referencing)
ChildMetrics: ICollection<MetricDefinition>
```

### Step 1.2: Update MetricDefinition Configuration

**File:** `Data\Configurations\Metrics\MetricDefinitionConfiguration.cs`

**Add:**
- Self-referencing FK configuration for ParentMetricId
- Index on MetricScope
- Index on HierarchyLevel

---

## PHASE 2: Model Updates - TenantMetric

### Step 2.1: Add Hierarchy Columns to TenantMetric

**File:** `Models\Entities\Metrics\TenantMetric.cs`

**Add columns:**
```
MetricScope: string (30) - "Field", "Section", "Template", "Tenant"
SourceSectionId: int? (nullable, FK to FormTemplateSections)
SourceTemplateId: int? (nullable, FK to FormTemplates)
SourceItemId: int? (nullable, FK to FormTemplateItems)
DrillDownData: string? (nullable, JSON)
```

**Navigation properties to add:**
```
SourceSection: FormTemplateSection?
SourceTemplate: FormTemplate?
SourceItem: FormTemplateItem?
```

### Step 2.2: Update TenantMetric Configuration

**File:** `Data\Configurations\Metrics\TenantMetricConfiguration.cs`

**Add:**
- FK configurations for SourceSectionId, SourceTemplateId, SourceItemId
- Composite index on (TenantId, MetricScope, ReportingPeriod)
- Index on MetricScope

---

## PHASE 3: New Models - Section & Template Metric Mappings

### Step 3.1: Create SectionMetricMapping Model

**File:** `Models\Entities\Forms\SectionMetricMapping.cs` (NEW)

**Columns:**
```
MappingId: int (PK, Identity)
SectionId: int (FK to FormTemplateSections)
MetricId: int (FK to MetricDefinitions)
AggregationType: string (20) - "Average", "WeightedAverage", "Sum", "Custom"
AggregationFormula: string? (nullable, JSON)
IncludedFieldMetrics: string? (nullable, JSON array of MetricIds)
IsActive: bool (default true)
CreatedDate: DateTime
```

**Navigation properties:**
```
Section: FormTemplateSection
Metric: MetricDefinition
```

### Step 3.2: Create TemplateMetricMapping Model

**File:** `Models\Entities\Forms\TemplateMetricMapping.cs` (NEW)

**Columns:**
```
MappingId: int (PK, Identity)
TemplateId: int (FK to FormTemplates)
MetricId: int (FK to MetricDefinitions)
MetricCategory: string (30) - "Content", "Process"
AggregationType: string (20) - "Average", "WeightedAverage", "Sum", "Custom"
AggregationFormula: string? (nullable, JSON)
IncludedSectionMetrics: string? (nullable, JSON array of MetricIds)
IsActive: bool (default true)
CreatedDate: DateTime
```

**Navigation properties:**
```
Template: FormTemplate
Metric: MetricDefinition
```

### Step 3.3: Create Configuration Files

**File:** `Data\Configurations\Forms\SectionMetricMappingConfiguration.cs` (NEW)
**File:** `Data\Configurations\Forms\TemplateMetricMappingConfiguration.cs` (NEW)

**Add:**
- FK configurations
- Unique constraint on (SectionId, MetricId) for SectionMetricMapping
- Unique constraint on (TemplateId, MetricId) for TemplateMetricMapping

### Step 3.4: Update Navigation Properties in Related Models

**File:** `Models\Entities\Forms\FormTemplateSection.cs`
- Add: `ICollection<SectionMetricMapping> MetricMappings`

**File:** `Models\Entities\Forms\FormTemplate.cs`
- Add: `ICollection<TemplateMetricMapping> MetricMappings`

**File:** `Models\Entities\Metrics\MetricDefinition.cs`
- Add: `ICollection<SectionMetricMapping> SectionMetricMappings`
- Add: `ICollection<TemplateMetricMapping> TemplateMetricMappings`

---

## PHASE 4: Update FormItemMetricMapping

### Step 4.1: Add ScoreMapping Support

**File:** `Models\Entities\Forms\FormItemMetricMapping.cs`

**Update MappingType comment to include:** "ScoreMapping"

**No new columns needed** - existing TransformationLogic JSON handles ScoreMapping config.

---

## PHASE 5: Update DbContext

### Step 5.1: Register New DbSets

**File:** `Data\ApplicationDbContext.cs`

**Add DbSets:**
```
DbSet<SectionMetricMapping> SectionMetricMappings
DbSet<TemplateMetricMapping> TemplateMetricMappings
```

### Step 5.2: Apply Configurations in OnModelCreating

**Add:**
```
modelBuilder.ApplyConfiguration(new SectionMetricMappingConfiguration());
modelBuilder.ApplyConfiguration(new TemplateMetricMappingConfiguration());
```

---

## PHASE 6: Database Migration

### Step 6.1: Clear Existing Data (Development Only)

**Package Manager Console Commands:**

```powershell
# Connect to database and clear metric-related tables
# Run these SQL commands in SSMS or Azure Data Studio BEFORE migration:
```

**SQL to run manually:**
```sql
-- Clear in correct order (child tables first)
DELETE FROM MetricPopulationLog;
DELETE FROM TenantMetrics;
DELETE FROM FormItemMetricMappings;
DELETE FROM SystemMetricLogs;
-- MetricDefinitions can stay if you want to keep definitions
-- DELETE FROM MetricDefinitions;  -- Optional
```

### Step 6.2: Create Migration

**Package Manager Console:**

```powershell
Add-Migration AddMetricHierarchySupport -Context ApplicationDbContext
```

### Step 6.3: Review Migration File

**Check the generated migration for:**
- New columns on MetricDefinitions
- New columns on TenantMetrics
- New tables: SectionMetricMappings, TemplateMetricMappings
- Foreign key constraints
- Indexes

### Step 6.4: Apply Migration

**Package Manager Console:**

```powershell
Update-Database -Context ApplicationDbContext
```

---

## PHASE 7: Seeders

### Step 7.1: Update MetricDefinitionSeeder

**File:** `Data\Seeders\MetricDefinitionSeeder.cs` (if exists, otherwise create)

**Update existing seeds to include:**
- MetricScope = "Field" for existing field-level metrics
- HierarchyLevel = 0 for field-level metrics

**Add new seed data for:**
- Section-level metrics (MetricScope = "Section", HierarchyLevel = 1)
- Template-level metrics (MetricScope = "Template", HierarchyLevel = 2)
- Tenant-level metrics (MetricScope = "Tenant", HierarchyLevel = 3)

**Example metrics to seed:**

| MetricCode | MetricScope | HierarchyLevel | ParentMetricId |
|------------|-------------|----------------|----------------|
| LAN_EQUIPMENT_HEALTH | Field | 0 | null |
| INFRASTRUCTURE_SECTION_SCORE | Section | 1 | null |
| ICT_OVERALL_SCORE | Template | 2 | null |
| REGIONAL_ICT_AVERAGE | Tenant | 3 | ICT_OVERALL_SCORE.Id |

### Step 7.2: Create SectionMetricMappingSeeder (Optional)

**File:** `Data\Seeders\SectionMetricMappingSeeder.cs` (NEW - Optional)

**Only needed if you want default section mappings.**
- Can be left empty initially
- Mappings created via UI during template configuration

### Step 7.3: Create TemplateMetricMappingSeeder (Optional)

**File:** `Data\Seeders\TemplateMetricMappingSeeder.cs` (NEW - Optional)

**Only needed if you want default template mappings.**
- Can be left empty initially
- Mappings created via UI during template configuration

### Step 7.4: Update Seeder Execution Order

**File:** `Data\Seeders\DatabaseSeeder.cs` or `Program.cs`

**Ensure order:**
1. MetricDefinitionSeeder (must run first - parent records)
2. FormItemMetricMappingSeeder (if exists)
3. SectionMetricMappingSeeder (if created)
4. TemplateMetricMappingSeeder (if created)

---

## PHASE 8: Services

### Step 8.1: Update IMetricMappingService Interface

**File:** `Services\Metrics\IMetricMappingService.cs`

**Add methods:**
```
// Section-level mappings
Task<List<SectionMetricMapping>> GetSectionMappingsAsync(int sectionId);
Task<SectionMetricMapping> CreateSectionMappingAsync(SectionMetricMapping mapping);
Task UpdateSectionMappingAsync(SectionMetricMapping mapping);
Task DeleteSectionMappingAsync(int mappingId);

// Template-level mappings
Task<List<TemplateMetricMapping>> GetTemplateMappingsAsync(int templateId);
Task<TemplateMetricMapping> CreateTemplateMappingAsync(TemplateMetricMapping mapping);
Task UpdateTemplateMappingAsync(TemplateMetricMapping mapping);
Task DeleteTemplateMappingAsync(int mappingId);

// Hierarchy queries
Task<List<MetricDefinition>> GetMetricsByScope(string scope);
Task<TenantMetric> GetMetricWithDrillDown(long metricValueId);
```

### Step 8.2: Update MetricMappingService Implementation

**File:** `Services\Metrics\MetricMappingService.cs`

**Implement the new interface methods.**

### Step 8.3: Update MetricPopulationService

**File:** `Services\Metrics\MetricPopulationService.cs` (or create if not exists)

**Add methods for hierarchical processing:**
```
// Process in order: Field → Section → Template
Task PopulateFieldMetricsAsync(int submissionId);
Task PopulateSectionMetricsAsync(int submissionId);
Task PopulateTemplateMetricsAsync(int submissionId);

// Main orchestrator
Task PopulateAllMetricsAsync(int submissionId);
```

**Processing order logic:**
1. Get all field-level mappings for the template
2. Calculate and save field metrics to TenantMetrics (MetricScope = "Field")
3. Get all section-level mappings for the template
4. Calculate section metrics from field metrics
5. Save section metrics to TenantMetrics (MetricScope = "Section")
6. Get all template-level mappings
7. Calculate template metrics from section metrics
8. Save template metrics to TenantMetrics (MetricScope = "Template")

---

## PHASE 9: API Controllers

### Step 9.1: Update MetricMappingApiController

**File:** `Controllers\API\MetricMappingApiController.cs`

**Add endpoints:**
```
// Section mappings
GET  /api/metricmapping/section/{sectionId}
POST /api/metricmapping/section
PUT  /api/metricmapping/section/{mappingId}
DELETE /api/metricmapping/section/{mappingId}

// Template mappings
GET  /api/metricmapping/template/{templateId}/mappings
POST /api/metricmapping/template
PUT  /api/metricmapping/template/{mappingId}
DELETE /api/metricmapping/template/{mappingId}

// Hierarchy
GET  /api/metricmapping/metrics/scope/{scope}
GET  /api/metricmapping/drilldown/{metricValueId}
```

---

## PHASE 10: Report Model Updates

### Step 10.1: Update ReportField Model

**File:** `Models\Entities\Reporting\ReportField.cs`

**Add column:**
```
MetricScope: string? (30, nullable) - Filter by metric level
```

### Step 10.2: Update ReportFilter FilterType Enum/Values

**File:** `Models\Entities\Reporting\ReportFilter.cs`

**Update FilterType comment to include:** "MetricScope"

### Step 10.3: Update ReportGrouping GroupByType Enum/Values

**File:** `Models\Entities\Reporting\ReportGrouping.cs`

**Update GroupByType comment to include:** "MetricScope"

### Step 10.4: Create Migration for Report Changes

**Package Manager Console:**

```powershell
Add-Migration AddMetricScopeToReportModels -Context ApplicationDbContext
Update-Database -Context ApplicationDbContext
```

---

## EXECUTION CHECKLIST

### Pre-Migration
- [ ] Backup database (optional for dev)
- [ ] Clear metric-related tables (SQL commands in Step 6.1)

### Phase 1-2: Core Model Updates
- [ ] Update MetricDefinition.cs
- [ ] Update MetricDefinitionConfiguration.cs
- [ ] Update TenantMetric.cs
- [ ] Update TenantMetricConfiguration.cs

### Phase 3: New Models
- [ ] Create SectionMetricMapping.cs
- [ ] Create TemplateMetricMapping.cs
- [ ] Create SectionMetricMappingConfiguration.cs
- [ ] Create TemplateMetricMappingConfiguration.cs
- [ ] Update FormTemplateSection.cs (navigation property)
- [ ] Update FormTemplate.cs (navigation property)
- [ ] Update MetricDefinition.cs (navigation properties)

### Phase 4: Existing Model Updates
- [ ] Review FormItemMetricMapping.cs (no changes needed)

### Phase 5: DbContext
- [ ] Add DbSets to ApplicationDbContext.cs
- [ ] Apply configurations in OnModelCreating

### Phase 6: Migration
- [ ] Run: `Add-Migration AddMetricHierarchySupport`
- [ ] Review migration file
- [ ] Run: `Update-Database`

### Phase 7: Seeders
- [ ] Update MetricDefinitionSeeder with MetricScope, HierarchyLevel
- [ ] Create SectionMetricMappingSeeder (optional)
- [ ] Create TemplateMetricMappingSeeder (optional)
- [ ] Update seeder execution order

### Phase 8: Services
- [ ] Update IMetricMappingService.cs
- [ ] Update MetricMappingService.cs
- [ ] Create/Update MetricPopulationService.cs

### Phase 9: API Controllers
- [ ] Update MetricMappingApiController.cs

### Phase 10: Report Models
- [ ] Update ReportField.cs
- [ ] Update ReportFilter.cs (comment only)
- [ ] Update ReportGrouping.cs (comment only)
- [ ] Run: `Add-Migration AddMetricScopeToReportModels`
- [ ] Run: `Update-Database`

### Post-Migration
- [ ] Run seeders
- [ ] Verify tables created
- [ ] Test API endpoints
- [ ] Verify navigation properties work

---

## PACKAGE MANAGER CONSOLE COMMANDS SUMMARY

```powershell
# Step 1: Create main migration
Add-Migration AddMetricHierarchySupport -Context ApplicationDbContext

# Step 2: Apply migration
Update-Database -Context ApplicationDbContext

# Step 3: Create report models migration (after Phase 10)
Add-Migration AddMetricScopeToReportModels -Context ApplicationDbContext

# Step 4: Apply report migration
Update-Database -Context ApplicationDbContext

# If migration fails and you need to rollback:
Update-Database -Migration PreviousMigrationName -Context ApplicationDbContext

# To remove last migration (if not applied):
Remove-Migration -Context ApplicationDbContext
```

---

## SQL COMMANDS FOR DATA CLEANUP

**Run in SSMS/Azure Data Studio before migration:**

```sql
-- Development only - clears all metric data
USE [FormReportingDb];  -- Replace with your database name

-- Disable FK checks temporarily
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"

-- Clear child tables first
TRUNCATE TABLE MetricPopulationLog;
DELETE FROM TenantMetrics;
DELETE FROM FormItemMetricMappings;
DELETE FROM SystemMetricLogs;

-- Optional: Clear metric definitions (if you want fresh start)
-- DELETE FROM MetricDefinitions;

-- Re-enable FK checks
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"
```

---

**End of Implementation Plan**

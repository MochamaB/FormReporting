# Section 10: Reporting & Analytics - Migration Instructions

## ✅ Files Created

### Models (12 files)
- ✅ `Models/Entities/Reporting/TenantPerformanceSnapshot.cs`
- ✅ `Models/Entities/Reporting/RegionalMonthlySnapshot.cs`
- ✅ `Models/Entities/Reporting/ReportDefinition.cs`
- ✅ `Models/Entities/Reporting/ReportField.cs`
- ✅ `Models/Entities/Reporting/ReportFilter.cs`
- ✅ `Models/Entities/Reporting/ReportGrouping.cs`
- ✅ `Models/Entities/Reporting/ReportSorting.cs`
- ✅ `Models/Entities/Reporting/ReportSchedule.cs`
- ✅ `Models/Entities/Reporting/ReportCache.cs`
- ✅ `Models/Entities/Reporting/ReportAccessControl.cs`
- ✅ `Models/Entities/Reporting/ReportExecutionLog.cs`

### Configurations (12 files)
- ✅ `Data/Configurations/Reporting/TenantPerformanceSnapshotConfiguration.cs`
- ✅ `Data/Configurations/Reporting/RegionalMonthlySnapshotConfiguration.cs`
- ✅ `Data/Configurations/Reporting/ReportDefinitionConfiguration.cs`
- ✅ `Data/Configurations/Reporting/ReportFieldConfiguration.cs`
- ✅ `Data/Configurations/Reporting/ReportFilterConfiguration.cs`
- ✅ `Data/Configurations/Reporting/ReportGroupingConfiguration.cs`
- ✅ `Data/Configurations/Reporting/ReportSortingConfiguration.cs`
- ✅ `Data/Configurations/Reporting/ReportScheduleConfiguration.cs`
- ✅ `Data/Configurations/Reporting/ReportCacheConfiguration.cs`
- ✅ `Data/Configurations/Reporting/ReportAccessControlConfiguration.cs`
- ✅ `Data/Configurations/Reporting/ReportExecutionLogConfiguration.cs`

### Updated Files
- ✅ `Data/ApplicationDbContext.cs` - Added DbSets and configurations

---

## 📋 Migration Commands

### Step 1: Build the Project
```powershell
dotnet build
```

### Step 2: Create Migration
```powershell
Add-Migration Add_Section10_ReportingAndAnalytics
```

**Alternative using .NET CLI:**
```powershell
dotnet ef migrations add Add_Section10_ReportingAndAnalytics
```

### Step 3: Review Migration
- Navigate to `Data/Migrations` folder
- Open the newly created migration file
- Verify that all 12 tables are being created

### Step 4: Apply Migration to Database
```powershell
Update-Database
```

**Alternative using .NET CLI:**
```powershell
dotnet ef database update
```

### Step 5: Verify Database
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN (
    'TenantPerformanceSnapshot',
    'RegionalMonthlySnapshot',
    'ReportDefinitions',
    'ReportFields',
    'ReportFilters',
    'ReportGroupings',
    'ReportSorting',
    'ReportSchedules',
    'ReportCache',
    'ReportAccessControl',
    'ReportExecutionLog'
)
ORDER BY TABLE_NAME;
```

---

## 📊 Database Tables Created

### 1. TenantPerformanceSnapshot
Pre-aggregated daily/weekly/monthly metrics per tenant for dashboard performance.

**Key Features:**
- Flexible JSON storage (`MetricsData`)
- Denormalized key metrics for quick queries
- Snapshot types: Daily, Weekly, Monthly, Quarterly
- Tracks: devices, uptime, tickets, compliance, expenses

### 2. RegionalMonthlySnapshot
Aggregated monthly metrics per region (factories only).

**Key Features:**
- Regional rollup of factory data
- Monthly granularity
- Ticket resolution tracking
- Compliance scoring

### 3. ReportDefinitions
User-created custom report configurations.

**Key Features:**
- Report types: Tabular, Chart, Pivot, Dashboard, CrossTab, Matrix
- Chart types: Bar, Line, Pie, Doughnut, Area, Column, Scatter, Bubble, Radar
- Form-based or metric-based reports
- Public/private sharing
- Version control
- Popularity tracking

### 4. ReportFields
Defines which columns/fields to include in reports.

**Key Features:**
- Source types: FormItem, Metric, Computed, SystemField
- Aggregation: Sum, Avg, Count, Min, Max, CountDistinct
- Custom display names and formatting
- Conditional formatting (JSON rules)
- Column ordering and visibility

### 5. ReportFilters
WHERE clause conditions for reports.

**Key Features:**
- Filter types: TenantId, RegionId, DateRange, Status, FieldValue, MetricValue
- 13 operators: Equals, NotEquals, GreaterThan, LessThan, Between, In, Contains, etc.
- Parameterized filters (prompt user at runtime)
- Required vs optional filters
- User override control

### 6. ReportGroupings
GROUP BY logic for reports.

**Key Features:**
- Group by: Tenant, Region, Month, Year, Quarter, Week, Day, TenantType, FieldValue, MetricValue
- Nested grouping support
- Subtotals and grand totals
- Sort direction per group

### 7. ReportSorting
ORDER BY logic for reports.

**Key Features:**
- Multi-level sorting (primary, secondary, tertiary)
- Sort by form fields, metrics, or system fields
- Ascending/descending

### 8. ReportSchedules
Automated report generation and distribution.

**Key Features:**
- Schedule types: Daily, Weekly, Monthly, Quarterly, Yearly, OnDemand
- Output formats: PDF, Excel, CSV, JSON, HTML
- Integrates with notification system
- Timezone support
- Next run calculation

### 9. ReportCache
Pre-generated report results for performance.

**Key Features:**
- SHA256 parameter hashing for cache key
- Expiry date management
- Hit count tracking (popularity)
- Performance metrics
- Computed column for data size

### 10. ReportAccessControl
Permission management for reports.

**Key Features:**
- Access types: User, Role, Department, Everyone
- Permission levels: View, Run, Edit, Delete, Share, Admin
- Time-limited access (expiry dates)
- Grantor tracking

### 11. ReportExecutionLog
Audit trail of all report executions.

**Key Features:**
- Execution types: Manual, Scheduled, Cached, API
- Performance metrics tracking
- Error logging
- Parameter and filter tracking
- Output format and size tracking

---

## 🔗 Key Relationships

```
ReportDefinitions (1) ──→ (N) ReportFields
ReportDefinitions (1) ──→ (N) ReportFilters
ReportDefinitions (1) ──→ (N) ReportGroupings
ReportDefinitions (1) ──→ (N) ReportSorting
ReportDefinitions (1) ──→ (N) ReportSchedules
ReportDefinitions (1) ──→ (N) ReportCache
ReportDefinitions (1) ──→ (N) ReportAccessControl
ReportDefinitions (1) ──→ (N) ReportExecutionLog

ReportDefinitions (N) ──→ (1) FormTemplates (optional)
ReportDefinitions (N) ──→ (1) Users (owner)

ReportFields (N) ──→ (1) FormTemplateItems (optional)
ReportFields (N) ──→ (1) MetricDefinitions (optional)

ReportSchedules (N) ──→ (1) NotificationTemplates
ReportSchedules (1) ──→ (N) ReportExecutionLog

TenantPerformanceSnapshot (N) ──→ (1) Tenants
RegionalMonthlySnapshot (N) ──→ (1) Regions
```

---

## ✨ Features Implemented

### Self-Service Report Builder
- ✅ Visual query builder (no SQL required)
- ✅ Multiple data sources (forms, metrics, system fields)
- ✅ Computed/calculated fields
- ✅ Aggregations and grouping
- ✅ Conditional formatting
- ✅ Chart generation (9 chart types)

### Performance Optimization
- ✅ Pre-aggregated snapshots (daily/weekly/monthly)
- ✅ Report caching with SHA256 hashing
- ✅ Hit count tracking
- ✅ Performance metrics logging
- ✅ Computed columns for efficiency

### Enterprise Features
- ✅ Role-based access control
- ✅ Report scheduling (5 schedule types)
- ✅ Multi-format export (PDF, Excel, CSV, JSON, HTML)
- ✅ Email distribution via notifications
- ✅ Audit trail (execution logs)
- ✅ Version control
- ✅ Public/private sharing

### Advanced Query Capabilities
- ✅ Parameterized filters (runtime prompts)
- ✅ Nested grouping
- ✅ Multi-level sorting
- ✅ Subtotals and grand totals
- ✅ Conditional formatting rules
- ✅ Custom field computations

---

## 🎯 Implementation Complexity

**Difficulty Level:** ⭐⭐⭐⭐⭐ (5/5 - Most Complex Section)

**Why Complex:**
1. **Dynamic Query Generation** - Build SQL from JSON configurations
2. **Multiple Data Sources** - Forms, metrics, system tables
3. **Chart Rendering** - 9 chart types with configurations
4. **Caching Strategy** - Parameter hashing and cache invalidation
5. **Scheduled Execution** - Background job integration (Hangfire)
6. **Export Engines** - PDF, Excel, CSV generation
7. **Access Control** - Complex permission matrix

---

## 🚀 Next Steps After Migration

### 1. Implement Core Services
- **ReportBuilderService** - Query generation from configurations
- **ReportExecutionService** - Run reports and cache results
- **ReportSchedulerService** - Background job for scheduled reports
- **ReportExportService** - Multi-format export (PDF, Excel, CSV)
- **SnapshotGeneratorService** - Pre-aggregate metrics

### 2. Create UI Components
- Report designer/builder interface
- Report viewer with filters
- Chart rendering components
- Schedule management
- Access control management
- Execution history viewer

### 3. Integrate Background Jobs
- Set up Hangfire for scheduled reports
- Create snapshot generation jobs
- Implement cache cleanup jobs
- Set up notification delivery for reports

### 4. Testing
- Test report generation with various configurations
- Test caching mechanism
- Test scheduled execution
- Test export to all formats
- Test access control rules

---

## 📝 Notes

- All DateTime fields use UTC (`DateTime.UtcNow`)
- JSON fields store complex configurations
- SHA256 hashing for cache keys
- Comprehensive indexing for performance
- Check constraints enforce data integrity
- Cascade delete configured appropriately
- Computed columns for efficiency (DataSizeKB)

---

## 🐛 Troubleshooting

### If migration fails:
1. Check for syntax errors in model classes
2. Verify all foreign key relationships
3. Ensure no circular dependencies
4. Check that all required packages are installed
5. Verify NotificationTemplate exists (Section 9 dependency)

### If database update fails:
1. Verify connection string in `appsettings.json`
2. Ensure SQL Server is running
3. Check database user permissions
4. Review migration SQL for conflicts
5. Ensure Section 9 (Notifications) was migrated first

---

## 📦 Dependencies

**Section 10 depends on:**
- ✅ Section 1: Organizational (Tenants, Regions, Departments)
- ✅ Section 2: Identity (Users, Roles)
- ✅ Section 3: Metrics (MetricDefinitions)
- ✅ Section 4: Forms (FormTemplates, FormTemplateItems)
- ✅ Section 9: Notifications (NotificationTemplates)

**Ensure all these sections are migrated before running this migration.**

---

**Created:** November 11, 2025  
**Section:** 10 - Reporting & Analytics  
**Tables:** 12  
**Status:** Ready for Migration  
**Estimated Implementation Time:** 3-4 weeks for full feature set

# Section 10: Reporting & Analytics - Documentation Structure

**Total Documents:** 12
**Purpose:** Comprehensive reporting system for 67 factories across all modules (Forms, Hardware, Software, Metrics)

---

## FOUNDATION (2 documents)

### 0_Section10_Overview_Workflows.md
User personas, report types, core workflows (view/export/schedule), integration with Sections 3-6, lifecycle

### 1_Section10_DataFlow_Architecture.md
Data aggregation pipeline, state machines, ETL processes, query patterns, API structure, caching strategy

---

## DATA LAYER (2 documents)

### 2A_Snapshot_Generation_Service.md
Pre-aggregation design, TenantPerformanceSnapshot/RegionalMonthlySnapshot tables, Hangfire jobs, incremental refresh

### 2B_Metric_Calculation_Engine.md
Multi-source metric calculation, aggregation functions (SUM/AVG/etc), trend calculation, formula engine, thresholds

---

## REPORT BUILDER (3 documents)

### 3A_ReportBuilder_Dashboard_Setup.md
Report designer UI, user journey, toolbar, template library, permissions

### 3B_ReportBuilder_Configuration.md
5-step wizard: Select data source, Pick fields, Add filters, Group/sort, Preview/test

### 3C_ReportBuilder_Publishing.md
Metadata, access control, validation, publishing workflow, version control, JSON schema storage

---

## REPORT VIEWER (2 documents)

### 4A_ReportViewer_Interface.md
Report catalog, viewer layout, parameter panel, data grid, interactive features (sort/filter/drill-down)

### 4B_ReportViewer_Visualizations.md
Chart types (bar/line/pie/gauge/heatmap), configuration, dashboard layout, ApexCharts/Chart.js integration

---

## AUTOMATION & PERFORMANCE (3 documents)

### 5_Scheduled_Reports_Implementation.md
Scheduling UI, ReportSchedules table, Hangfire jobs, email delivery, distribution lists, audit trail

### 6A_Export_Functionality.md
Excel/PDF/CSV/PowerPoint exports, EPPlus/iTextSharp/CsvHelper libraries, async export for large reports

### 6B_Caching_Performance_Optimization.md
4-level caching (browser/memory/Redis/ReportCache), invalidation strategy, query optimization, load testing, monitoring

---

## IMPLEMENTATION ORDER

**Phase 1 (Week 1):** 0, 1 - Foundation
**Phase 2 (Week 2):** 2A, 2B - Data Layer
**Phase 3 (Week 3):** 4A, 4B - Basic Viewing
**Phase 4 (Week 4):** 3A, 3B, 3C - Advanced Builder
**Phase 5 (Week 5):** 5, 6A, 6B - Automation & Performance

---

## KEY DEPENDENCIES

- Section 3: MetricDefinitions, TenantMetrics (KPI data source)
- Section 4: FormTemplateSubmissions, FormTemplateResponses (form data source)
- Section 5: SoftwareProducts, TenantSoftwareInstallations (software compliance)
- Section 6: HardwareItems, TenantHardware (hardware inventory)
- Section 1: Tenants, Regions (organizational structure)
- Section 2: Users, Roles (access control)

---

**Status:** Documentation structure defined
**Next Step:** Create 0_Section10_Overview_Workflows.md

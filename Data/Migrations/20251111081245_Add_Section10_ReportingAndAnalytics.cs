using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Section10_ReportingAndAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegionalMonthlySnapshot",
                columns: table => new
                {
                    SnapshotId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    YearMonth = table.Column<DateTime>(type: "date", nullable: false),
                    TotalFactories = table.Column<int>(type: "int", nullable: false),
                    TotalDevices = table.Column<int>(type: "int", nullable: false),
                    WorkingDevices = table.Column<int>(type: "int", nullable: false),
                    FaultyDevices = table.Column<int>(type: "int", nullable: false),
                    TotalTickets = table.Column<int>(type: "int", nullable: false),
                    OpenTickets = table.Column<int>(type: "int", nullable: false),
                    ResolvedTickets = table.Column<int>(type: "int", nullable: false),
                    AvgResolutionDays = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    TotalExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AvgComplianceScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionalMonthlySnapshot", x => x.SnapshotId);
                    table.ForeignKey(
                        name: "FK_RegionalMonthlySnapshot_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportDefinitions",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReportCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TemplateId = table.Column<int>(type: "int", nullable: true),
                    ReportType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChartType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ChartConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LayoutConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    OwnerUserId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastRunDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RunCount = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDefinitions", x => x.ReportId);
                    table.CheckConstraint("CK_Report_ChartType", "[ChartType] IS NULL OR [ChartType] IN ('Bar', 'Line', 'Pie', 'Doughnut', 'Area', 'Column', 'Scatter', 'Bubble', 'Radar')");
                    table.CheckConstraint("CK_Report_Type", "[ReportType] IN ('Tabular', 'Chart', 'Pivot', 'Dashboard', 'CrossTab', 'Matrix')");
                    table.ForeignKey(
                        name: "FK_ReportDefinitions_FormTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FormTemplates",
                        principalColumn: "TemplateId");
                    table.ForeignKey(
                        name: "FK_ReportDefinitions_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TenantPerformanceSnapshot",
                columns: table => new
                {
                    SnapshotId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "date", nullable: false),
                    SnapshotType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MetricsData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalDevices = table.Column<int>(type: "int", nullable: false),
                    WorkingDevices = table.Column<int>(type: "int", nullable: false),
                    FaultyDevices = table.Column<int>(type: "int", nullable: false),
                    UptimePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    OpenTickets = table.Column<int>(type: "int", nullable: false),
                    ComplianceScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    TotalExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeneratedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DataVersion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantPerformanceSnapshot", x => x.SnapshotId);
                    table.CheckConstraint("CK_SnapshotType", "[SnapshotType] IN ('Daily', 'Weekly', 'Monthly', 'Quarterly')");
                    table.ForeignKey(
                        name: "FK_TenantPerformanceSnapshot_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportAccessControl",
                columns: table => new
                {
                    AccessId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    AccessType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    PermissionLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GrantedBy = table.Column<int>(type: "int", nullable: false),
                    GrantedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportAccessControl", x => x.AccessId);
                    table.CheckConstraint("CK_ReportAccess_Permission", "[PermissionLevel] IN ('View', 'Run', 'Edit', 'Delete', 'Share', 'Admin')");
                    table.CheckConstraint("CK_ReportAccess_Target", "([AccessType] = 'User' AND [UserId] IS NOT NULL AND [RoleId] IS NULL AND [DepartmentId] IS NULL) OR\n                  ([AccessType] = 'Role' AND [RoleId] IS NOT NULL AND [UserId] IS NULL AND [DepartmentId] IS NULL) OR\n                  ([AccessType] = 'Department' AND [DepartmentId] IS NOT NULL AND [UserId] IS NULL AND [RoleId] IS NULL) OR\n                  ([AccessType] = 'Everyone' AND [UserId] IS NULL AND [RoleId] IS NULL AND [DepartmentId] IS NULL)");
                    table.CheckConstraint("CK_ReportAccess_Type", "[AccessType] IN ('User', 'Role', 'Department', 'Everyone')");
                    table.ForeignKey(
                        name: "FK_ReportAccessControl_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_ReportAccessControl_ReportDefinitions_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportDefinitions",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportAccessControl_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId");
                    table.ForeignKey(
                        name: "FK_ReportAccessControl_Users_GrantedBy",
                        column: x => x.GrantedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_ReportAccessControl_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ReportCache",
                columns: table => new
                {
                    CacheId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ParameterHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    ColumnCount = table.Column<int>(type: "int", nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeneratedBy = table.Column<int>(type: "int", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HitCount = table.Column<int>(type: "int", nullable: false),
                    LastAccessDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GenerationTimeMs = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportCache", x => x.CacheId);
                    table.ForeignKey(
                        name: "FK_ReportCache_ReportDefinitions_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportDefinitions",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportCache_Users_GeneratedBy",
                        column: x => x.GeneratedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ReportFields",
                columns: table => new
                {
                    FieldId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    MetricId = table.Column<int>(type: "int", nullable: true),
                    SystemFieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    ColumnWidth = table.Column<int>(type: "int", nullable: true),
                    AggregationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FormatString = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ComputationFormula = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConditionalFormatting = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportFields", x => x.FieldId);
                    table.CheckConstraint("CK_ReportField_Aggregation", "[AggregationType] IS NULL OR [AggregationType] IN ('Sum', 'Avg', 'Count', 'Min', 'Max', 'CountDistinct', 'First', 'Last', 'None')");
                    table.CheckConstraint("CK_ReportField_Source", "[SourceType] IN ('FormItem', 'Metric', 'Computed', 'SystemField')");
                    table.ForeignKey(
                        name: "FK_ReportFields_FormTemplateItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "FormTemplateItems",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ReportFields_MetricDefinitions_MetricId",
                        column: x => x.MetricId,
                        principalTable: "MetricDefinitions",
                        principalColumn: "MetricId");
                    table.ForeignKey(
                        name: "FK_ReportFields_ReportDefinitions_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportDefinitions",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportFilters",
                columns: table => new
                {
                    FilterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    FilterType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    MetricId = table.Column<int>(type: "int", nullable: true),
                    SystemFieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FilterValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    AllowUserOverride = table.Column<bool>(type: "bit", nullable: false),
                    IsParameterized = table.Column<bool>(type: "bit", nullable: false),
                    ParameterLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportFilters", x => x.FilterId);
                    table.CheckConstraint("CK_ReportFilter_Operator", "[Operator] IN ('Equals', 'NotEquals', 'GreaterThan', 'LessThan', 'GreaterOrEqual', 'LessOrEqual', 'Between', 'In', 'NotIn', 'Contains', 'StartsWith', 'EndsWith', 'IsNull', 'IsNotNull')");
                    table.CheckConstraint("CK_ReportFilter_Type", "[FilterType] IN ('TenantId', 'RegionId', 'DateRange', 'Status', 'FieldValue', 'MetricValue', 'TenantType', 'Custom')");
                    table.ForeignKey(
                        name: "FK_ReportFilters_FormTemplateItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "FormTemplateItems",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ReportFilters_MetricDefinitions_MetricId",
                        column: x => x.MetricId,
                        principalTable: "MetricDefinitions",
                        principalColumn: "MetricId");
                    table.ForeignKey(
                        name: "FK_ReportFilters_ReportDefinitions_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportDefinitions",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportGroupings",
                columns: table => new
                {
                    GroupingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    GroupByType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    MetricId = table.Column<int>(type: "int", nullable: true),
                    SystemFieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GroupOrder = table.Column<int>(type: "int", nullable: false),
                    SortDirection = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ShowSubtotals = table.Column<bool>(type: "bit", nullable: false),
                    ShowGrandTotal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportGroupings", x => x.GroupingId);
                    table.CheckConstraint("CK_ReportGrouping_Sort", "[SortDirection] IN ('ASC', 'DESC')");
                    table.CheckConstraint("CK_ReportGrouping_Type", "[GroupByType] IN ('Tenant', 'Region', 'Month', 'Year', 'Quarter', 'Week', 'Day', 'TenantType', 'Category', 'FieldValue', 'MetricValue')");
                    table.ForeignKey(
                        name: "FK_ReportGroupings_FormTemplateItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "FormTemplateItems",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ReportGroupings_MetricDefinitions_MetricId",
                        column: x => x.MetricId,
                        principalTable: "MetricDefinitions",
                        principalColumn: "MetricId");
                    table.ForeignKey(
                        name: "FK_ReportGroupings_ReportDefinitions_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportDefinitions",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportSchedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ScheduleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ScheduleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DayOfWeek = table.Column<byte>(type: "tinyint", nullable: true),
                    DayOfMonth = table.Column<byte>(type: "tinyint", nullable: true),
                    ExecutionTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Timezone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OutputFormat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IncludeCharts = table.Column<bool>(type: "bit", nullable: false),
                    PageOrientation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NotificationTemplateId = table.Column<int>(type: "int", nullable: false),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastRunDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRunDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRunStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LastRunError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSchedules", x => x.ScheduleId);
                    table.CheckConstraint("CK_ReportSchedule_Format", "[OutputFormat] IN ('PDF', 'Excel', 'CSV', 'JSON', 'HTML')");
                    table.CheckConstraint("CK_ReportSchedule_Orientation", "[PageOrientation] IN ('Portrait', 'Landscape')");
                    table.CheckConstraint("CK_ReportSchedule_Type", "[ScheduleType] IN ('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Yearly', 'OnDemand')");
                    table.ForeignKey(
                        name: "FK_ReportSchedules_NotificationTemplates_NotificationTemplateId",
                        column: x => x.NotificationTemplateId,
                        principalTable: "NotificationTemplates",
                        principalColumn: "TemplateId");
                    table.ForeignKey(
                        name: "FK_ReportSchedules_ReportDefinitions_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportDefinitions",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportSchedules_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ReportSorting",
                columns: table => new
                {
                    SortId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    MetricId = table.Column<int>(type: "int", nullable: true),
                    SystemFieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    SortDirection = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSorting", x => x.SortId);
                    table.CheckConstraint("CK_ReportSort_Direction", "[SortDirection] IN ('ASC', 'DESC')");
                    table.ForeignKey(
                        name: "FK_ReportSorting_FormTemplateItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "FormTemplateItems",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_ReportSorting_MetricDefinitions_MetricId",
                        column: x => x.MetricId,
                        principalTable: "MetricDefinitions",
                        principalColumn: "MetricId");
                    table.ForeignKey(
                        name: "FK_ReportSorting_ReportDefinitions_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportDefinitions",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportExecutionLog",
                columns: table => new
                {
                    ExecutionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ExecutedBy = table.Column<int>(type: "int", nullable: false),
                    ExecutionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Filters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RowCount = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutionTimeMs = table.Column<int>(type: "int", nullable: true),
                    QueryExecutionTimeMs = table.Column<int>(type: "int", nullable: true),
                    RenderingTimeMs = table.Column<int>(type: "int", nullable: true),
                    OutputFormat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OutputSizeKB = table.Column<int>(type: "int", nullable: true),
                    OutputPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportExecutionLog", x => x.ExecutionId);
                    table.CheckConstraint("CK_ReportExec_Status", "[Status] IN ('Success', 'Failed', 'Timeout', 'Cancelled', 'Pending')");
                    table.CheckConstraint("CK_ReportExec_Type", "[ExecutionType] IN ('Manual', 'Scheduled', 'Cached', 'API')");
                    table.ForeignKey(
                        name: "FK_ReportExecutionLog_ReportDefinitions_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportDefinitions",
                        principalColumn: "ReportId");
                    table.ForeignKey(
                        name: "FK_ReportExecutionLog_ReportSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "ReportSchedules",
                        principalColumn: "ScheduleId");
                    table.ForeignKey(
                        name: "FK_ReportExecutionLog_Users_ExecutedBy",
                        column: x => x.ExecutedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_RegionMonth",
                table: "RegionalMonthlySnapshot",
                columns: new[] { "RegionId", "YearMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportAccess_Expiry",
                table: "ReportAccessControl",
                column: "ExpiryDate",
                filter: "[ExpiryDate] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReportAccess_Report",
                table: "ReportAccessControl",
                columns: new[] { "ReportId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportAccess_Role",
                table: "ReportAccessControl",
                columns: new[] { "RoleId", "IsActive" },
                filter: "[RoleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReportAccess_User",
                table: "ReportAccessControl",
                columns: new[] { "UserId", "IsActive" },
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReportAccessControl_DepartmentId",
                table: "ReportAccessControl",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportAccessControl_GrantedBy",
                table: "ReportAccessControl",
                column: "GrantedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportCache_Expiry",
                table: "ReportCache",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReportCache_GeneratedBy",
                table: "ReportCache",
                column: "GeneratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportCache_Popular",
                table: "ReportCache",
                columns: new[] { "HitCount", "LastAccessDate" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ReportCache_Report",
                table: "ReportCache",
                columns: new[] { "ReportId", "GeneratedDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UQ_ReportCache_Hash",
                table: "ReportCache",
                columns: new[] { "ReportId", "ParameterHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportDef_Category",
                table: "ReportDefinitions",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportDef_Owner",
                table: "ReportDefinitions",
                columns: new[] { "OwnerUserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportDef_Popular",
                table: "ReportDefinitions",
                columns: new[] { "RunCount", "LastRunDate" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ReportDef_Public",
                table: "ReportDefinitions",
                columns: new[] { "IsPublic", "IsActive" },
                filter: "[IsPublic] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReportDef_Template",
                table: "ReportDefinitions",
                columns: new[] { "TemplateId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "UQ_Report_Code",
                table: "ReportDefinitions",
                column: "ReportCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportExec_Date",
                table: "ReportExecutionLog",
                column: "ExecutionDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ReportExec_Failed",
                table: "ReportExecutionLog",
                columns: new[] { "Status", "ExecutionDate" },
                descending: new[] { false, true },
                filter: "[Status] = 'Failed'");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExec_Performance",
                table: "ReportExecutionLog",
                column: "ExecutionTimeMs",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ReportExec_Report",
                table: "ReportExecutionLog",
                columns: new[] { "ReportId", "ExecutionDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ReportExec_User",
                table: "ReportExecutionLog",
                columns: new[] { "ExecutedBy", "ExecutionDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutionLog_ScheduleId",
                table: "ReportExecutionLog",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportField_Item",
                table: "ReportFields",
                column: "ItemId",
                filter: "[ItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReportField_Metric",
                table: "ReportFields",
                column: "MetricId",
                filter: "[MetricId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReportField_Report",
                table: "ReportFields",
                columns: new[] { "ReportId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportFilter_Item",
                table: "ReportFilters",
                column: "ItemId",
                filter: "[ItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFilter_Parametrized",
                table: "ReportFilters",
                columns: new[] { "ReportId", "IsParameterized" },
                filter: "[IsParameterized] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFilter_Report",
                table: "ReportFilters",
                columns: new[] { "ReportId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportFilters_MetricId",
                table: "ReportFilters",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportGrouping_Report",
                table: "ReportGroupings",
                columns: new[] { "ReportId", "GroupOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportGroupings_ItemId",
                table: "ReportGroupings",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportGroupings_MetricId",
                table: "ReportGroupings",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedule_NextRun",
                table: "ReportSchedules",
                column: "NextRunDate",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedule_Report",
                table: "ReportSchedules",
                columns: new[] { "ReportId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedule_Type",
                table: "ReportSchedules",
                columns: new[] { "ScheduleType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedules_CreatedBy",
                table: "ReportSchedules",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedules_NotificationTemplateId",
                table: "ReportSchedules",
                column: "NotificationTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSort_Report",
                table: "ReportSorting",
                columns: new[] { "ReportId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportSorting_ItemId",
                table: "ReportSorting",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSorting_MetricId",
                table: "ReportSorting",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfSnapshot_Generated",
                table: "TenantPerformanceSnapshot",
                column: "GeneratedDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_PerfSnapshot_Tenant_Date",
                table: "TenantPerformanceSnapshot",
                columns: new[] { "TenantId", "SnapshotDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PerfSnapshot_Type_Date",
                table: "TenantPerformanceSnapshot",
                columns: new[] { "SnapshotType", "SnapshotDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "UQ_PerfSnapshot",
                table: "TenantPerformanceSnapshot",
                columns: new[] { "TenantId", "SnapshotDate", "SnapshotType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegionalMonthlySnapshot");

            migrationBuilder.DropTable(
                name: "ReportAccessControl");

            migrationBuilder.DropTable(
                name: "ReportCache");

            migrationBuilder.DropTable(
                name: "ReportExecutionLog");

            migrationBuilder.DropTable(
                name: "ReportFields");

            migrationBuilder.DropTable(
                name: "ReportFilters");

            migrationBuilder.DropTable(
                name: "ReportGroupings");

            migrationBuilder.DropTable(
                name: "ReportSorting");

            migrationBuilder.DropTable(
                name: "TenantPerformanceSnapshot");

            migrationBuilder.DropTable(
                name: "ReportSchedules");

            migrationBuilder.DropTable(
                name: "ReportDefinitions");
        }
    }
}

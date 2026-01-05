using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class MetricHierarchyImplementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegionalMonthlySnapshot");

            migrationBuilder.DropTable(
                name: "TenantPerformanceSnapshot");

            migrationBuilder.DropIndex(
                name: "IX_TenantMetrics_Tenant",
                table: "TenantMetrics");

            migrationBuilder.DropIndex(
                name: "UQ_ItemMetricMap",
                table: "FormItemMetricMappings");

            migrationBuilder.AlterColumn<int>(
                name: "MetricId",
                table: "TenantMetrics",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "MetricScope",
                table: "TenantMetrics",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceMappingId",
                table: "TenantMetrics",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DrillDownEnabled",
                table: "ReportFields",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MetricScope",
                table: "ReportFields",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MetricId",
                table: "MetricPopulationLog",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "HierarchyLevel",
                table: "MetricDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetricScope",
                table: "MetricDefinitions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentMetricId",
                table: "MetricDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MetricId",
                table: "FormItemMetricMappings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AggregationType",
                table: "FormItemMetricMappings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MappingName",
                table: "FormItemMetricMappings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "FormSectionMetricMappings",
                columns: table => new
                {
                    MappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    MetricId = table.Column<int>(type: "int", nullable: true),
                    MappingName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MappingType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AggregationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSectionMetricMappings", x => x.MappingId);
                    table.ForeignKey(
                        name: "FK_FormSectionMetricMappings_FormTemplateSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "FormTemplateSections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormSectionMetricMappings_MetricDefinitions_MetricId",
                        column: x => x.MetricId,
                        principalTable: "MetricDefinitions",
                        principalColumn: "MetricId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FormTemplateMetricMappings",
                columns: table => new
                {
                    MappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    MetricId = table.Column<int>(type: "int", nullable: true),
                    MappingName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MappingType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AggregationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormTemplateMetricMappings", x => x.MappingId);
                    table.ForeignKey(
                        name: "FK_FormTemplateMetricMappings_FormTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FormTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormTemplateMetricMappings_MetricDefinitions_MetricId",
                        column: x => x.MetricId,
                        principalTable: "MetricDefinitions",
                        principalColumn: "MetricId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FormSectionMetricSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionMappingId = table.Column<int>(type: "int", nullable: false),
                    ItemMappingId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSectionMetricSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormSectionMetricSources_FormItemMetricMappings_ItemMappingId",
                        column: x => x.ItemMappingId,
                        principalTable: "FormItemMetricMappings",
                        principalColumn: "MappingId");
                    table.ForeignKey(
                        name: "FK_FormSectionMetricSources_FormSectionMetricMappings_SectionMappingId",
                        column: x => x.SectionMappingId,
                        principalTable: "FormSectionMetricMappings",
                        principalColumn: "MappingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormTemplateMetricSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateMappingId = table.Column<int>(type: "int", nullable: false),
                    SectionMappingId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormTemplateMetricSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormTemplateMetricSources_FormSectionMetricMappings_SectionMappingId",
                        column: x => x.SectionMappingId,
                        principalTable: "FormSectionMetricMappings",
                        principalColumn: "MappingId");
                    table.ForeignKey(
                        name: "FK_FormTemplateMetricSources_FormTemplateMetricMappings_TemplateMappingId",
                        column: x => x.TemplateMappingId,
                        principalTable: "FormTemplateMetricMappings",
                        principalColumn: "MappingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantMetrics_Tenant",
                table: "TenantMetrics",
                columns: new[] { "TenantId", "MetricId", "ReportingPeriod" },
                unique: true,
                descending: new[] { false, false, true },
                filter: "[MetricId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MetricDefinitions_ParentMetricId",
                table: "MetricDefinitions",
                column: "ParentMetricId");

            migrationBuilder.CreateIndex(
                name: "UQ_ItemMetricMap",
                table: "FormItemMetricMappings",
                columns: new[] { "ItemId", "MetricId" },
                unique: true,
                filter: "[MetricId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FormSectionMetricMappings_MetricId",
                table: "FormSectionMetricMappings",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSectionMetricMappings_SectionId",
                table: "FormSectionMetricMappings",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSectionMetricMappings_SectionId_MappingName",
                table: "FormSectionMetricMappings",
                columns: new[] { "SectionId", "MappingName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormSectionMetricSources_ItemMappingId",
                table: "FormSectionMetricSources",
                column: "ItemMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSectionMetricSources_SectionMappingId",
                table: "FormSectionMetricSources",
                column: "SectionMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSectionMetricSources_SectionMappingId_ItemMappingId",
                table: "FormSectionMetricSources",
                columns: new[] { "SectionMappingId", "ItemMappingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateMetricMappings_MetricId",
                table: "FormTemplateMetricMappings",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateMetricMappings_TemplateId",
                table: "FormTemplateMetricMappings",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateMetricMappings_TemplateId_MappingName",
                table: "FormTemplateMetricMappings",
                columns: new[] { "TemplateId", "MappingName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateMetricSources_SectionMappingId",
                table: "FormTemplateMetricSources",
                column: "SectionMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateMetricSources_TemplateMappingId",
                table: "FormTemplateMetricSources",
                column: "TemplateMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateMetricSources_TemplateMappingId_SectionMappingId",
                table: "FormTemplateMetricSources",
                columns: new[] { "TemplateMappingId", "SectionMappingId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MetricDefinitions_MetricDefinitions_ParentMetricId",
                table: "MetricDefinitions",
                column: "ParentMetricId",
                principalTable: "MetricDefinitions",
                principalColumn: "MetricId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetricDefinitions_MetricDefinitions_ParentMetricId",
                table: "MetricDefinitions");

            migrationBuilder.DropTable(
                name: "FormSectionMetricSources");

            migrationBuilder.DropTable(
                name: "FormTemplateMetricSources");

            migrationBuilder.DropTable(
                name: "FormSectionMetricMappings");

            migrationBuilder.DropTable(
                name: "FormTemplateMetricMappings");

            migrationBuilder.DropIndex(
                name: "IX_TenantMetrics_Tenant",
                table: "TenantMetrics");

            migrationBuilder.DropIndex(
                name: "IX_MetricDefinitions_ParentMetricId",
                table: "MetricDefinitions");

            migrationBuilder.DropIndex(
                name: "UQ_ItemMetricMap",
                table: "FormItemMetricMappings");

            migrationBuilder.DropColumn(
                name: "MetricScope",
                table: "TenantMetrics");

            migrationBuilder.DropColumn(
                name: "SourceMappingId",
                table: "TenantMetrics");

            migrationBuilder.DropColumn(
                name: "DrillDownEnabled",
                table: "ReportFields");

            migrationBuilder.DropColumn(
                name: "MetricScope",
                table: "ReportFields");

            migrationBuilder.DropColumn(
                name: "HierarchyLevel",
                table: "MetricDefinitions");

            migrationBuilder.DropColumn(
                name: "MetricScope",
                table: "MetricDefinitions");

            migrationBuilder.DropColumn(
                name: "ParentMetricId",
                table: "MetricDefinitions");

            migrationBuilder.DropColumn(
                name: "AggregationType",
                table: "FormItemMetricMappings");

            migrationBuilder.DropColumn(
                name: "MappingName",
                table: "FormItemMetricMappings");

            migrationBuilder.AlterColumn<int>(
                name: "MetricId",
                table: "TenantMetrics",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MetricId",
                table: "MetricPopulationLog",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MetricId",
                table: "FormItemMetricMappings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "RegionalMonthlySnapshot",
                columns: table => new
                {
                    SnapshotId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    AvgComplianceScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    AvgResolutionDays = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    FaultyDevices = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpenTickets = table.Column<int>(type: "int", nullable: false),
                    ResolvedTickets = table.Column<int>(type: "int", nullable: false),
                    TotalDevices = table.Column<int>(type: "int", nullable: false),
                    TotalExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalFactories = table.Column<int>(type: "int", nullable: false),
                    TotalTickets = table.Column<int>(type: "int", nullable: false),
                    WorkingDevices = table.Column<int>(type: "int", nullable: false),
                    YearMonth = table.Column<DateTime>(type: "date", nullable: false)
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
                name: "TenantPerformanceSnapshot",
                columns: table => new
                {
                    SnapshotId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ComplianceScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DataVersion = table.Column<int>(type: "int", nullable: false),
                    FaultyDevices = table.Column<int>(type: "int", nullable: false),
                    GeneratedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetricsData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenTickets = table.Column<int>(type: "int", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "date", nullable: false),
                    SnapshotType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalDevices = table.Column<int>(type: "int", nullable: false),
                    TotalExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UptimePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    WorkingDevices = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_TenantMetrics_Tenant",
                table: "TenantMetrics",
                columns: new[] { "TenantId", "MetricId", "ReportingPeriod" },
                unique: true,
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "UQ_ItemMetricMap",
                table: "FormItemMetricMappings",
                columns: new[] { "ItemId", "MetricId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RegionMonth",
                table: "RegionalMonthlySnapshot",
                columns: new[] { "RegionId", "YearMonth" },
                unique: true);

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
    }
}

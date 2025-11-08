using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSection3And4MetricsAndForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FieldLibrary",
                columns: table => new
                {
                    LibraryFieldId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FieldCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FieldType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DefaultConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldLibrary", x => x.LibraryFieldId);
                    table.ForeignKey(
                        name: "FK_FieldLibrary_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "MetricDefinitions",
                columns: table => new
                {
                    MetricId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetricCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SourceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AggregationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsKPI = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ThresholdGreen = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ThresholdYellow = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ThresholdRed = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ExpectedValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ComplianceRule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricDefinitions", x => x.MetricId);
                    table.CheckConstraint("CK_Metric_AggregationType", "AggregationType IS NULL OR AggregationType IN ('SUM', 'AVG', 'MAX', 'MIN', 'LAST_VALUE', 'COUNT', 'NONE')");
                    table.CheckConstraint("CK_Metric_DataType", "DataType IN ('Integer', 'Decimal', 'Percentage', 'Boolean', 'Text', 'Duration', 'Date', 'DateTime')");
                    table.CheckConstraint("CK_Metric_SourceType", "SourceType IN ('UserInput', 'SystemCalculated', 'ExternalSystem', 'ComplianceTracking', 'AutomatedCheck')");
                    table.CheckConstraint("CK_Metric_Unit", "Unit IS NULL OR Unit IN ('Count', 'Percentage', 'Version', 'Status', 'Days', 'Hours', 'Minutes', 'Seconds', 'GB', 'MB', 'KB', 'TB', 'Bytes', 'None')");
                });

            migrationBuilder.CreateTable(
                name: "WorkflowDefinitions",
                columns: table => new
                {
                    WorkflowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDefinitions", x => x.WorkflowId);
                    table.ForeignKey(
                        name: "FK_WorkflowDefinitions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemMetricLogs",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    MetricId = table.Column<int>(type: "int", nullable: false),
                    CheckDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NumericValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TextValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExecutionDuration = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemMetricLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_SystemMetricLogs_MetricDefinitions_MetricId",
                        column: x => x.MetricId,
                        principalTable: "MetricDefinitions",
                        principalColumn: "MetricId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemMetricLogs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantMetrics",
                columns: table => new
                {
                    MetricValueId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    MetricId = table.Column<int>(type: "int", nullable: false),
                    ReportingPeriod = table.Column<DateTime>(type: "date", nullable: false),
                    NumericValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TextValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SourceReferenceId = table.Column<int>(type: "int", nullable: true),
                    CapturedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CapturedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantMetrics", x => x.MetricValueId);
                    table.CheckConstraint("CK_TenantMetric_SourceType", "SourceType IS NULL OR SourceType IN ('UserInput', 'SystemCalculated', 'HangfireJob', 'ExternalAPI', 'Manual', 'Import')");
                    table.ForeignKey(
                        name: "FK_TenantMetrics_MetricDefinitions_MetricId",
                        column: x => x.MetricId,
                        principalTable: "MetricDefinitions",
                        principalColumn: "MetricId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantMetrics_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TemplateCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TemplateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    WorkflowId = table.Column<int>(type: "int", nullable: true),
                    PublishStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedBy = table.Column<int>(type: "int", nullable: true),
                    ArchivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<int>(type: "int", nullable: true),
                    ArchivedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormTemplates", x => x.TemplateId);
                    table.CheckConstraint("CK_Template_Approval", "(RequiresApproval = 0 AND WorkflowId IS NULL) OR (RequiresApproval = 1 AND WorkflowId IS NOT NULL) OR (PublishStatus = 'Draft')");
                    table.CheckConstraint("CK_Template_PublishStatus", "PublishStatus IN ('Draft', 'Published', 'Archived', 'Deprecated')");
                    table.ForeignKey(
                        name: "FK_FormTemplates_FormCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "FormCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplates_Users_ArchivedBy",
                        column: x => x.ArchivedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplates_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplates_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplates_Users_PublishedBy",
                        column: x => x.PublishedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplates_WorkflowDefinitions_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "WorkflowId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowSteps",
                columns: table => new
                {
                    StepId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApproverRoleId = table.Column<int>(type: "int", nullable: true),
                    ApproverUserId = table.Column<int>(type: "int", nullable: true),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ConditionLogic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsParallel = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DueDays = table.Column<int>(type: "int", nullable: true),
                    EscalationRoleId = table.Column<int>(type: "int", nullable: true),
                    AutoApproveCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSteps", x => x.StepId);
                    table.CheckConstraint("CK_WorkflowStep_Approver", "(ApproverRoleId IS NOT NULL AND ApproverUserId IS NULL) OR (ApproverRoleId IS NULL AND ApproverUserId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_Roles_ApproverRoleId",
                        column: x => x.ApproverRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_Roles_EscalationRoleId",
                        column: x => x.EscalationRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_Users_ApproverUserId",
                        column: x => x.ApproverUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_WorkflowDefinitions_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "WorkflowId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormTemplateAssignments",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    AssignmentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TenantGroupId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    UserGroupId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    AssignedBy = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormTemplateAssignments", x => x.AssignmentId);
                    table.CheckConstraint("CK_TemplateAssignment_Target", "(AssignmentType = 'All' AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR\n                  (AssignmentType = 'TenantType' AND TenantType IS NOT NULL AND TenantGroupId IS NULL AND TenantId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR\n                  (AssignmentType = 'TenantGroup' AND TenantGroupId IS NOT NULL AND TenantType IS NULL AND TenantId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR\n                  (AssignmentType = 'SpecificTenant' AND TenantId IS NOT NULL AND TenantType IS NULL AND TenantGroupId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR\n                  (AssignmentType = 'Role' AND RoleId IS NOT NULL AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR\n                  (AssignmentType = 'Department' AND DepartmentId IS NOT NULL AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL AND RoleId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR\n                  (AssignmentType = 'UserGroup' AND UserGroupId IS NOT NULL AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL AND UserId IS NULL) OR\n                  (AssignmentType = 'SpecificUser' AND UserId IS NOT NULL AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL)");
                    table.CheckConstraint("CK_TemplateAssignment_Type", "AssignmentType IN ('All', 'TenantType', 'TenantGroup', 'SpecificTenant', 'Role', 'Department', 'UserGroup', 'SpecificUser')");
                    table.ForeignKey(
                        name: "FK_FormTemplateAssignments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateAssignments_FormTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FormTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormTemplateAssignments_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateAssignments_TenantGroups_TenantGroupId",
                        column: x => x.TenantGroupId,
                        principalTable: "TenantGroups",
                        principalColumn: "TenantGroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateAssignments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateAssignments_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "UserGroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateAssignments_Users_AssignedBy",
                        column: x => x.AssignedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormTemplateSections",
                columns: table => new
                {
                    SectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    SectionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SectionDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsCollapsible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsCollapsedByDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormTemplateSections", x => x.SectionId);
                    table.ForeignKey(
                        name: "FK_FormTemplateSections_FormTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FormTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormTemplateSubmissions",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    ReportingYear = table.Column<int>(type: "int", nullable: false),
                    ReportingMonth = table.Column<byte>(type: "tinyint", nullable: false),
                    ReportingPeriod = table.Column<DateTime>(type: "date", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    SubmittedBy = table.Column<int>(type: "int", nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    ReviewedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormTemplateSubmissions", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_FormTemplateSubmissions_FormTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FormTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateSubmissions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateSubmissions_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateSubmissions_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateSubmissions_Users_SubmittedBy",
                        column: x => x.SubmittedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormTemplateItems",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PlaceholderText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HelpText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PrefixText = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SuffixText = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConditionalLogic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LayoutType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Single"),
                    MatrixGroupId = table.Column<int>(type: "int", nullable: true),
                    MatrixRowLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LibraryFieldId = table.Column<int>(type: "int", nullable: true),
                    IsLibraryOverride = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormTemplateItems", x => x.ItemId);
                    table.CheckConstraint("CK_Item_LayoutType", "LayoutType IN ('Single', 'Matrix', 'Grid', 'Inline')");
                    table.ForeignKey(
                        name: "FK_FormTemplateItems_FieldLibrary_LibraryFieldId",
                        column: x => x.LibraryFieldId,
                        principalTable: "FieldLibrary",
                        principalColumn: "LibraryFieldId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateItems_FormTemplateSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "FormTemplateSections",
                        principalColumn: "SectionId");
                    table.ForeignKey(
                        name: "FK_FormTemplateItems_FormTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FormTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormAnalytics",
                columns: table => new
                {
                    AnalyticId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SubmissionId = table.Column<int>(type: "int", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormAnalytics", x => x.AnalyticId);
                    table.CheckConstraint("CK_Analytics_EventType", "EventType IN ('FormOpened', 'SectionStarted', 'SectionCompleted', 'FieldFilled', 'FormAbandoned', 'FormSubmitted', 'FormSaved')");
                    table.ForeignKey(
                        name: "FK_FormAnalytics_FormTemplateSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "FormTemplateSubmissions",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormAnalytics_FormTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FormTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormAnalytics_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionWorkflowProgress",
                columns: table => new
                {
                    ProgressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    StepId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    ReviewedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DelegatedTo = table.Column<int>(type: "int", nullable: true),
                    DelegatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DelegatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionWorkflowProgress", x => x.ProgressId);
                    table.ForeignKey(
                        name: "FK_SubmissionWorkflowProgress_FormTemplateSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "FormTemplateSubmissions",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubmissionWorkflowProgress_Users_DelegatedBy",
                        column: x => x.DelegatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubmissionWorkflowProgress_Users_DelegatedTo",
                        column: x => x.DelegatedTo,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubmissionWorkflowProgress_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubmissionWorkflowProgress_WorkflowSteps_StepId",
                        column: x => x.StepId,
                        principalTable: "WorkflowSteps",
                        principalColumn: "StepId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormItemCalculations",
                columns: table => new
                {
                    CalculationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetItemId = table.Column<int>(type: "int", nullable: false),
                    CalculationFormula = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormItemCalculations", x => x.CalculationId);
                    table.ForeignKey(
                        name: "FK_FormItemCalculations_FormTemplateItems_TargetItemId",
                        column: x => x.TargetItemId,
                        principalTable: "FormTemplateItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormItemConfiguration",
                columns: table => new
                {
                    ConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ConfigKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormItemConfiguration", x => x.ConfigId);
                    table.ForeignKey(
                        name: "FK_FormItemConfiguration_FormTemplateItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "FormTemplateItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormItemMetricMappings",
                columns: table => new
                {
                    MappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    MetricId = table.Column<int>(type: "int", nullable: false),
                    MappingType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TransformationLogic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormItemMetricMappings", x => x.MappingId);
                    table.ForeignKey(
                        name: "FK_FormItemMetricMappings_FormTemplateItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "FormTemplateItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormItemMetricMappings_MetricDefinitions_MetricId",
                        column: x => x.MetricId,
                        principalTable: "MetricDefinitions",
                        principalColumn: "MetricId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormItemOptions",
                columns: table => new
                {
                    OptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    OptionValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OptionLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ParentOptionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormItemOptions", x => x.OptionId);
                    table.ForeignKey(
                        name: "FK_FormItemOptions_FormItemOptions_ParentOptionId",
                        column: x => x.ParentOptionId,
                        principalTable: "FormItemOptions",
                        principalColumn: "OptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormItemOptions_FormTemplateItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "FormTemplateItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormItemValidations",
                columns: table => new
                {
                    ItemValidationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ValidationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MinLength = table.Column<int>(type: "int", nullable: true),
                    MaxLength = table.Column<int>(type: "int", nullable: true),
                    RegexPattern = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomExpression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidationOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Error"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormItemValidations", x => x.ItemValidationId);
                    table.CheckConstraint("CK_ValidationSeverity", "Severity IN ('Error', 'Warning', 'Info')");
                    table.CheckConstraint("CK_ValidationType", "ValidationType IN ('Required', 'Email', 'Phone', 'URL', 'Range', 'MinLength', 'MaxLength', 'Pattern', 'Custom', 'CrossField', 'Date', 'Number', 'Integer', 'Decimal')");
                    table.ForeignKey(
                        name: "FK_FormItemValidations_FormTemplateItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "FormTemplateItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormTemplateResponses",
                columns: table => new
                {
                    ResponseId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    TextValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumericValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    DateValue = table.Column<DateTime>(type: "date", nullable: true),
                    BooleanValue = table.Column<bool>(type: "bit", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormTemplateResponses", x => x.ResponseId);
                    table.ForeignKey(
                        name: "FK_FormTemplateResponses_FormTemplateItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "FormTemplateItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateResponses_FormTemplateSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "FormTemplateSubmissions",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectionRouting",
                columns: table => new
                {
                    RoutingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceSectionId = table.Column<int>(type: "int", nullable: false),
                    SourceItemId = table.Column<int>(type: "int", nullable: false),
                    TargetSectionId = table.Column<int>(type: "int", nullable: true),
                    ConditionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConditionValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionRouting", x => x.RoutingId);
                    table.CheckConstraint("CK_Routing_Condition", "ConditionType IN ('equals', 'not_equals', 'contains', 'greater_than', 'less_than', 'is_empty')");
                    table.ForeignKey(
                        name: "FK_SectionRouting_FormTemplateItems_SourceItemId",
                        column: x => x.SourceItemId,
                        principalTable: "FormTemplateItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SectionRouting_FormTemplateSections_SourceSectionId",
                        column: x => x.SourceSectionId,
                        principalTable: "FormTemplateSections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SectionRouting_FormTemplateSections_TargetSectionId",
                        column: x => x.TargetSectionId,
                        principalTable: "FormTemplateSections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MetricPopulationLog",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    MetricId = table.Column<int>(type: "int", nullable: false),
                    MappingId = table.Column<int>(type: "int", nullable: false),
                    SourceItemId = table.Column<int>(type: "int", nullable: false),
                    SourceValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalculatedValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CalculationFormula = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PopulatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    PopulatedBy = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProcessingTimeMs = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricPopulationLog", x => x.LogId);
                    table.CheckConstraint("CK_MetricLog_Status", "Status IN ('Success', 'Failed', 'Skipped', 'Pending')");
                    table.ForeignKey(
                        name: "FK_MetricPopulationLog_FormItemMetricMappings_MappingId",
                        column: x => x.MappingId,
                        principalTable: "FormItemMetricMappings",
                        principalColumn: "MappingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetricPopulationLog_FormTemplateItems_SourceItemId",
                        column: x => x.SourceItemId,
                        principalTable: "FormTemplateItems",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetricPopulationLog_FormTemplateSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "FormTemplateSubmissions",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetricPopulationLog_MetricDefinitions_MetricId",
                        column: x => x.MetricId,
                        principalTable: "MetricDefinitions",
                        principalColumn: "MetricId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetricPopulationLog_Users_PopulatedBy",
                        column: x => x.PopulatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldLibrary_Category",
                table: "FieldLibrary",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_FieldLibrary_Code",
                table: "FieldLibrary",
                column: "FieldCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldLibrary_CreatedBy",
                table: "FieldLibrary",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FieldLibrary_Type",
                table: "FieldLibrary",
                columns: new[] { "FieldType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_EventType",
                table: "FormAnalytics",
                columns: new[] { "EventType", "EventDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_Session",
                table: "FormAnalytics",
                columns: new[] { "SessionId", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_Submission",
                table: "FormAnalytics",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_Template_Date",
                table: "FormAnalytics",
                columns: new[] { "TemplateId", "EventDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_User_Date",
                table: "FormAnalytics",
                columns: new[] { "UserId", "EventDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Active",
                table: "FormCategories",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_FormCategories_CategoryCode",
                table: "FormCategories",
                column: "CategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormCategories_CategoryName",
                table: "FormCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCalculations_Target",
                table: "FormItemCalculations",
                columns: new[] { "TargetItemId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemConfig_Item",
                table: "FormItemConfiguration",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemConfig_Key",
                table: "FormItemConfiguration",
                column: "ConfigKey");

            migrationBuilder.CreateIndex(
                name: "UQ_ItemConfig",
                table: "FormItemConfiguration",
                columns: new[] { "ItemId", "ConfigKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemMetricMap_Item",
                table: "FormItemMetricMappings",
                columns: new[] { "ItemId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemMetricMap_Metric",
                table: "FormItemMetricMappings",
                columns: new[] { "MetricId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemMetricMap_Type",
                table: "FormItemMetricMappings",
                columns: new[] { "MappingType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "UQ_ItemMetricMap",
                table: "FormItemMetricMappings",
                columns: new[] { "ItemId", "MetricId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemOptions_Item",
                table: "FormItemOptions",
                columns: new[] { "ItemId", "DisplayOrder", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemOptions_Parent",
                table: "FormItemOptions",
                column: "ParentOptionId");

            migrationBuilder.CreateIndex(
                name: "UQ_ItemOption",
                table: "FormItemOptions",
                columns: new[] { "ItemId", "OptionValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemValidations_Item",
                table: "FormItemValidations",
                columns: new[] { "ItemId", "ValidationOrder", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemValidations_Type",
                table: "FormItemValidations",
                columns: new[] { "ValidationType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateAssignments_AssignedBy",
                table: "FormTemplateAssignments",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateAssignments_DepartmentId",
                table: "FormTemplateAssignments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateAssignments_UserGroupId",
                table: "FormTemplateAssignments",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateAssignments_UserId",
                table: "FormTemplateAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignment_Role",
                table: "FormTemplateAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignments_Template",
                table: "FormTemplateAssignments",
                columns: new[] { "TemplateId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignments_Tenant",
                table: "FormTemplateAssignments",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignments_TenantGroup",
                table: "FormTemplateAssignments",
                columns: new[] { "TenantGroupId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignments_TenantType",
                table: "FormTemplateAssignments",
                columns: new[] { "TenantType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignments_Type",
                table: "FormTemplateAssignments",
                columns: new[] { "AssignmentType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateItems_Layout",
                table: "FormTemplateItems",
                columns: new[] { "LayoutType", "MatrixGroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateItems_Library",
                table: "FormTemplateItems",
                column: "LibraryFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateItems_Section",
                table: "FormTemplateItems",
                columns: new[] { "SectionId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateItems_Template",
                table: "FormTemplateItems",
                columns: new[] { "TemplateId", "DisplayOrder", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "UQ_TemplateItem",
                table: "FormTemplateItems",
                columns: new[] { "TemplateId", "ItemCode", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateResponses_ItemId",
                table: "FormTemplateResponses",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateResponses_Submission",
                table: "FormTemplateResponses",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "UQ_SubmissionItem",
                table: "FormTemplateResponses",
                columns: new[] { "SubmissionId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplates_ArchivedBy",
                table: "FormTemplates",
                column: "ArchivedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplates_CreatedBy",
                table: "FormTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplates_ModifiedBy",
                table: "FormTemplates",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplates_PublishedBy",
                table: "FormTemplates",
                column: "PublishedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplates_TemplateCode",
                table: "FormTemplates",
                column: "TemplateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Template_Workflow",
                table: "FormTemplates",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Category",
                table: "FormTemplates",
                columns: new[] { "CategoryId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Templates_PublishStatus",
                table: "FormTemplates",
                columns: new[] { "PublishStatus", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSections_Template",
                table: "FormTemplateSections",
                columns: new[] { "TemplateId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "UQ_TemplateSection_Name",
                table: "FormTemplateSections",
                columns: new[] { "TemplateId", "SectionName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateSubmissions_ModifiedBy",
                table: "FormTemplateSubmissions",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateSubmissions_ReviewedBy",
                table: "FormTemplateSubmissions",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateSubmissions_TemplateId",
                table: "FormTemplateSubmissions",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_Location_Unique",
                table: "FormTemplateSubmissions",
                columns: new[] { "TenantId", "TemplateId", "ReportingPeriod" },
                unique: true,
                filter: "TenantId IS NOT NULL AND Status <> 'Draft'");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_User_Period",
                table: "FormTemplateSubmissions",
                columns: new[] { "SubmittedBy", "ReportingPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_Submission_User_Unique",
                table: "FormTemplateSubmissions",
                columns: new[] { "SubmittedBy", "TemplateId", "ReportingPeriod" },
                unique: true,
                filter: "TenantId IS NULL AND Status <> 'Draft'");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSubmissions_Status",
                table: "FormTemplateSubmissions",
                columns: new[] { "Status", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSubmissions_Tenant_Recent",
                table: "FormTemplateSubmissions",
                columns: new[] { "TenantId", "ReportingPeriod" },
                descending: new[] { false, true },
                filter: "Status IN ('Submitted', 'Approved')");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSubmissions_TimeSeries",
                table: "FormTemplateSubmissions",
                columns: new[] { "ReportingPeriod", "TenantId", "TemplateId" },
                descending: new[] { true, false, false })
                .Annotation("SqlServer:Include", new[] { "Status", "SubmittedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricDefinitions_MetricCode",
                table: "MetricDefinitions",
                column: "MetricCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_Category",
                table: "MetricDefinitions",
                columns: new[] { "Category", "IsKPI", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_SourceType",
                table: "MetricDefinitions",
                columns: new[] { "SourceType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricLog_Date",
                table: "MetricPopulationLog",
                column: "PopulatedDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_MetricLog_Metric",
                table: "MetricPopulationLog",
                columns: new[] { "MetricId", "PopulatedDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_MetricLog_Status",
                table: "MetricPopulationLog",
                columns: new[] { "Status", "PopulatedDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_MetricLog_Submission",
                table: "MetricPopulationLog",
                columns: new[] { "SubmissionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricPopulationLog_MappingId",
                table: "MetricPopulationLog",
                column: "MappingId");

            migrationBuilder.CreateIndex(
                name: "IX_MetricPopulationLog_PopulatedBy",
                table: "MetricPopulationLog",
                column: "PopulatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MetricPopulationLog_SourceItemId",
                table: "MetricPopulationLog",
                column: "SourceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionRouting_Item",
                table: "SectionRouting",
                columns: new[] { "SourceItemId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SectionRouting_Source",
                table: "SectionRouting",
                columns: new[] { "SourceSectionId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SectionRouting_Target",
                table: "SectionRouting",
                column: "TargetSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Progress_Delegated",
                table: "SubmissionWorkflowProgress",
                column: "DelegatedTo",
                filter: "DelegatedTo IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Progress_DueDate",
                table: "SubmissionWorkflowProgress",
                column: "DueDate",
                filter: "Status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_Progress_Status",
                table: "SubmissionWorkflowProgress",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Progress_Submission_Order",
                table: "SubmissionWorkflowProgress",
                columns: new[] { "SubmissionId", "StepOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionWorkflowProgress_DelegatedBy",
                table: "SubmissionWorkflowProgress",
                column: "DelegatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionWorkflowProgress_ReviewedBy",
                table: "SubmissionWorkflowProgress",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionWorkflowProgress_StepId",
                table: "SubmissionWorkflowProgress",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "UQ_Progress_Submission_Step",
                table: "SubmissionWorkflowProgress",
                columns: new[] { "SubmissionId", "StepId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetricLogs_Metric_Date",
                table: "SystemMetricLogs",
                columns: new[] { "MetricId", "CheckDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetricLogs_Status",
                table: "SystemMetricLogs",
                columns: new[] { "Status", "CheckDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetricLogs_Tenant_Date",
                table: "SystemMetricLogs",
                columns: new[] { "TenantId", "CheckDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_TenantMetrics_MetricId",
                table: "TenantMetrics",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMetrics_Period",
                table: "TenantMetrics",
                column: "ReportingPeriod",
                descending: new bool[0])
                .Annotation("SqlServer:Include", new[] { "TenantId", "NumericValue" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantMetrics_Source",
                table: "TenantMetrics",
                columns: new[] { "SourceType", "SourceReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantMetrics_Tenant",
                table: "TenantMetrics",
                columns: new[] { "TenantId", "MetricId", "ReportingPeriod" },
                unique: true,
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_TenantMetrics_TimeSeries",
                table: "TenantMetrics",
                columns: new[] { "ReportingPeriod", "TenantId", "MetricId" },
                descending: new[] { true, false, false })
                .Annotation("SqlServer:Include", new[] { "NumericValue", "TextValue" });

            migrationBuilder.CreateIndex(
                name: "IX_Workflow_Active",
                table: "WorkflowDefinitions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowDefinitions_CreatedBy",
                table: "WorkflowDefinitions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStep_Escalation",
                table: "WorkflowSteps",
                column: "EscalationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStep_Workflow",
                table: "WorkflowSteps",
                columns: new[] { "WorkflowId", "StepOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_ApproverRoleId",
                table: "WorkflowSteps",
                column: "ApproverRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_ApproverUserId",
                table: "WorkflowSteps",
                column: "ApproverUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormAnalytics");

            migrationBuilder.DropTable(
                name: "FormItemCalculations");

            migrationBuilder.DropTable(
                name: "FormItemConfiguration");

            migrationBuilder.DropTable(
                name: "FormItemOptions");

            migrationBuilder.DropTable(
                name: "FormItemValidations");

            migrationBuilder.DropTable(
                name: "FormTemplateAssignments");

            migrationBuilder.DropTable(
                name: "FormTemplateResponses");

            migrationBuilder.DropTable(
                name: "MetricPopulationLog");

            migrationBuilder.DropTable(
                name: "SectionRouting");

            migrationBuilder.DropTable(
                name: "SubmissionWorkflowProgress");

            migrationBuilder.DropTable(
                name: "SystemMetricLogs");

            migrationBuilder.DropTable(
                name: "TenantMetrics");

            migrationBuilder.DropTable(
                name: "FormItemMetricMappings");

            migrationBuilder.DropTable(
                name: "FormTemplateSubmissions");

            migrationBuilder.DropTable(
                name: "WorkflowSteps");

            migrationBuilder.DropTable(
                name: "FormTemplateItems");

            migrationBuilder.DropTable(
                name: "MetricDefinitions");

            migrationBuilder.DropTable(
                name: "FieldLibrary");

            migrationBuilder.DropTable(
                name: "FormTemplateSections");

            migrationBuilder.DropTable(
                name: "FormTemplates");

            migrationBuilder.DropTable(
                name: "FormCategories");

            migrationBuilder.DropTable(
                name: "WorkflowDefinitions");
        }
    }
}

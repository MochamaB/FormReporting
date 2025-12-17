using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFormSubmissionRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignment_Frequency",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignments_Template",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignments_Tenant",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignments_TenantGroup",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignments_TenantType",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignments_Type",
                table: "FormTemplateAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateAssignment_DueDay",
                table: "FormTemplateAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateAssignment_Frequency",
                table: "FormTemplateAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateAssignment_GracePeriod",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "AllowLateSubmission",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "DueDay",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "DueTime",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "GracePeriodDays",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "ReminderDaysBefore",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "ReminderSent",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "ReminderSentDate",
                table: "FormTemplateAssignments");

            migrationBuilder.CreateTable(
                name: "FormTemplateSubmissionRules",
                columns: table => new
                {
                    SubmissionRuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    RuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DueDay = table.Column<int>(type: "int", nullable: true),
                    DueMonth = table.Column<int>(type: "int", nullable: true),
                    DueTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    SpecificDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RuleConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GracePeriodDays = table.Column<int>(type: "int", nullable: false),
                    AllowLateSubmission = table.Column<bool>(type: "bit", nullable: false),
                    ReminderDaysBefore = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormTemplateSubmissionRules", x => x.SubmissionRuleId);
                    table.ForeignKey(
                        name: "FK_FormTemplateSubmissionRules_FormTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FormTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormTemplateSubmissionRules_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormTemplateSubmissionRules_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignments_Template",
                table: "FormTemplateAssignments",
                columns: new[] { "TemplateId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignments_Tenant",
                table: "FormTemplateAssignments",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignments_TenantGroup",
                table: "FormTemplateAssignments",
                columns: new[] { "TenantGroupId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignments_TenantType",
                table: "FormTemplateAssignments",
                columns: new[] { "TenantType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignments_Type",
                table: "FormTemplateAssignments",
                columns: new[] { "AssignmentType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateSubmissionRules_CreatedBy",
                table: "FormTemplateSubmissionRules",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateSubmissionRules_ModifiedBy",
                table: "FormTemplateSubmissionRules",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateSubmissionRules_Status",
                table: "FormTemplateSubmissionRules",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateSubmissionRules_TemplateId",
                table: "FormTemplateSubmissionRules",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateSubmissionRules_TemplateId_Status",
                table: "FormTemplateSubmissionRules",
                columns: new[] { "TemplateId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormTemplateSubmissionRules");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignments_Template",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignments_Tenant",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignments_TenantGroup",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignments_TenantType",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignments_Type",
                table: "FormTemplateAssignments");

            migrationBuilder.AddColumn<bool>(
                name: "AllowLateSubmission",
                table: "FormTemplateAssignments",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "DueDay",
                table: "FormTemplateAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DueTime",
                table: "FormTemplateAssignments",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "FormTemplateAssignments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GracePeriodDays",
                table: "FormTemplateAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FormTemplateAssignments",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "ReminderDaysBefore",
                table: "FormTemplateAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                table: "FormTemplateAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentDate",
                table: "FormTemplateAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignment_Frequency",
                table: "FormTemplateAssignments",
                column: "Frequency");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateAssignment_DueDay",
                table: "FormTemplateAssignments",
                sql: "DueDay IS NULL OR (DueDay >= -1 AND DueDay <= 31)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateAssignment_Frequency",
                table: "FormTemplateAssignments",
                sql: "Frequency IS NULL OR Frequency IN ('Custom', 'Daily', 'Weekly', 'Monthly', 'Quarterly', 'Annually')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateAssignment_GracePeriod",
                table: "FormTemplateAssignments",
                sql: "GracePeriodDays >= 0");
        }
    }
}

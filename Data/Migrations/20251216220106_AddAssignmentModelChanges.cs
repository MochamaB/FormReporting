using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignment_DueDate",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignment_Recurring",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignment_ReportingPeriod",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignment_Template_Status_Due",
                table: "FormTemplateAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateAssignment_RecurrencePattern",
                table: "FormTemplateAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateAssignment_Status",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "RecurrenceDayOf",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "ReportingMonth",
                table: "FormTemplateAssignments");

            migrationBuilder.RenameColumn(
                name: "ReportingYear",
                table: "FormTemplateAssignments",
                newName: "DueDay");

            migrationBuilder.RenameColumn(
                name: "RecurrencePattern",
                table: "FormTemplateAssignments",
                newName: "Frequency");

            migrationBuilder.RenameColumn(
                name: "RecurrenceEndDate",
                table: "FormTemplateAssignments",
                newName: "EffectiveUntil");

            migrationBuilder.RenameColumn(
                name: "IsRecurring",
                table: "FormTemplateAssignments",
                newName: "AllowAnonymous");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FormTemplateAssignments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pending");

            migrationBuilder.AddColumn<bool>(
                name: "AllowLateSubmission",
                table: "FormTemplateAssignments",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DueTime",
                table: "FormTemplateAssignments",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveFrom",
                table: "FormTemplateAssignments",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "GracePeriodDays",
                table: "FormTemplateAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignment_Anonymous",
                table: "FormTemplateAssignments",
                column: "AllowAnonymous",
                filter: "AllowAnonymous = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignment_EffectivePeriod",
                table: "FormTemplateAssignments",
                columns: new[] { "EffectiveFrom", "EffectiveUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignment_Frequency",
                table: "FormTemplateAssignments",
                column: "Frequency");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignment_Template_Status_Effective",
                table: "FormTemplateAssignments",
                columns: new[] { "TemplateId", "Status", "EffectiveFrom" });

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateAssignment_Status",
                table: "FormTemplateAssignments",
                sql: "Status IN ('Active', 'Suspended', 'Revoked')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignment_Anonymous",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignment_EffectivePeriod",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignment_Frequency",
                table: "FormTemplateAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TemplateAssignment_Template_Status_Effective",
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

            migrationBuilder.DropCheckConstraint(
                name: "CK_TemplateAssignment_Status",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "AllowLateSubmission",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "DueTime",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "GracePeriodDays",
                table: "FormTemplateAssignments");

            migrationBuilder.RenameColumn(
                name: "Frequency",
                table: "FormTemplateAssignments",
                newName: "RecurrencePattern");

            migrationBuilder.RenameColumn(
                name: "EffectiveUntil",
                table: "FormTemplateAssignments",
                newName: "RecurrenceEndDate");

            migrationBuilder.RenameColumn(
                name: "DueDay",
                table: "FormTemplateAssignments",
                newName: "ReportingYear");

            migrationBuilder.RenameColumn(
                name: "AllowAnonymous",
                table: "FormTemplateAssignments",
                newName: "IsRecurring");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FormTemplateAssignments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Active");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "FormTemplateAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceDayOf",
                table: "FormTemplateAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportingMonth",
                table: "FormTemplateAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignment_DueDate",
                table: "FormTemplateAssignments",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignment_Recurring",
                table: "FormTemplateAssignments",
                column: "IsRecurring",
                filter: "IsRecurring = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignment_ReportingPeriod",
                table: "FormTemplateAssignments",
                columns: new[] { "ReportingYear", "ReportingMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAssignment_Template_Status_Due",
                table: "FormTemplateAssignments",
                columns: new[] { "TemplateId", "Status", "DueDate" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateAssignment_RecurrencePattern",
                table: "FormTemplateAssignments",
                sql: "RecurrencePattern IS NULL OR RecurrencePattern IN ('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Annually')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TemplateAssignment_Status",
                table: "FormTemplateAssignments",
                sql: "Status IN ('Pending', 'InProgress', 'Completed', 'Overdue', 'Cancelled')");
        }
    }
}

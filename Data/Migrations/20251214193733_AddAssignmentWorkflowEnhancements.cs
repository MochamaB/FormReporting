using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentWorkflowEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WorkflowStep_Approver",
                table: "WorkflowSteps");

            migrationBuilder.AddColumn<int>(
                name: "ActionId",
                table: "WorkflowSteps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssigneeDepartmentId",
                table: "WorkflowSteps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssigneeFieldId",
                table: "WorkflowSteps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssigneeType",
                table: "WorkflowSteps",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Role");

            migrationBuilder.AddColumn<string>(
                name: "DependsOnStepIds",
                table: "WorkflowSteps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetId",
                table: "WorkflowSteps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetType",
                table: "WorkflowSteps",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Submission");

            migrationBuilder.AddColumn<int>(
                name: "ActionId",
                table: "SubmissionWorkflowProgress",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "SubmissionWorkflowProgress",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedTo",
                table: "SubmissionWorkflowProgress",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DelegationReason",
                table: "SubmissionWorkflowProgress",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureData",
                table: "SubmissionWorkflowProgress",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureIP",
                table: "SubmissionWorkflowProgress",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SignatureTimestamp",
                table: "SubmissionWorkflowProgress",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureType",
                table: "SubmissionWorkflowProgress",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetId",
                table: "SubmissionWorkflowProgress",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetType",
                table: "SubmissionWorkflowProgress",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "FormTemplateAssignments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancelledBy",
                table: "FormTemplateAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledDate",
                table: "FormTemplateAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "FormTemplateAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "FormTemplateAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceDayOf",
                table: "FormTemplateAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecurrenceEndDate",
                table: "FormTemplateAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecurrencePattern",
                table: "FormTemplateAssignments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

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

            migrationBuilder.AddColumn<int>(
                name: "ReportingMonth",
                table: "FormTemplateAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportingYear",
                table: "FormTemplateAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "FormTemplateAssignments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateTable(
                name: "WorkflowActions",
                columns: table => new
                {
                    ActionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresSignature = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RequiresComment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowDelegate = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CssClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowActions", x => x.ActionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStep_Action",
                table: "WorkflowSteps",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStep_AssigneeType",
                table: "WorkflowSteps",
                column: "AssigneeType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStep_TargetType",
                table: "WorkflowSteps",
                column: "TargetType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_AssigneeDepartmentId",
                table: "WorkflowSteps",
                column: "AssigneeDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_AssigneeFieldId",
                table: "WorkflowSteps",
                column: "AssigneeFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_Progress_Action",
                table: "SubmissionWorkflowProgress",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_Progress_AssignedTo",
                table: "SubmissionWorkflowProgress",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_Progress_AssignedTo_Status",
                table: "SubmissionWorkflowProgress",
                columns: new[] { "AssignedTo", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Progress_TargetType",
                table: "SubmissionWorkflowProgress",
                column: "TargetType");

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateAssignments_CancelledBy",
                table: "FormTemplateAssignments",
                column: "CancelledBy");

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
                name: "IX_TemplateAssignment_Status",
                table: "FormTemplateAssignments",
                column: "Status");

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

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowAction_Active",
                table: "WorkflowActions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowAction_DisplayOrder",
                table: "WorkflowActions",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "UQ_WorkflowAction_Code",
                table: "WorkflowActions",
                column: "ActionCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FormTemplateAssignments_Users_CancelledBy",
                table: "FormTemplateAssignments",
                column: "CancelledBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionWorkflowProgress_Users_AssignedTo",
                table: "SubmissionWorkflowProgress",
                column: "AssignedTo",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionWorkflowProgress_WorkflowActions_ActionId",
                table: "SubmissionWorkflowProgress",
                column: "ActionId",
                principalTable: "WorkflowActions",
                principalColumn: "ActionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_Departments_AssigneeDepartmentId",
                table: "WorkflowSteps",
                column: "AssigneeDepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_FormTemplateItems_AssigneeFieldId",
                table: "WorkflowSteps",
                column: "AssigneeFieldId",
                principalTable: "FormTemplateItems",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_WorkflowActions_ActionId",
                table: "WorkflowSteps",
                column: "ActionId",
                principalTable: "WorkflowActions",
                principalColumn: "ActionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormTemplateAssignments_Users_CancelledBy",
                table: "FormTemplateAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionWorkflowProgress_Users_AssignedTo",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionWorkflowProgress_WorkflowActions_ActionId",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_Departments_AssigneeDepartmentId",
                table: "WorkflowSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_FormTemplateItems_AssigneeFieldId",
                table: "WorkflowSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_WorkflowActions_ActionId",
                table: "WorkflowSteps");

            migrationBuilder.DropTable(
                name: "WorkflowActions");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowStep_Action",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowStep_AssigneeType",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowStep_TargetType",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_AssigneeDepartmentId",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_AssigneeFieldId",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_Progress_Action",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropIndex(
                name: "IX_Progress_AssignedTo",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropIndex(
                name: "IX_Progress_AssignedTo_Status",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropIndex(
                name: "IX_Progress_TargetType",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropIndex(
                name: "IX_FormTemplateAssignments_CancelledBy",
                table: "FormTemplateAssignments");

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
                name: "IX_TemplateAssignment_Status",
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
                name: "ActionId",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "AssigneeDepartmentId",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "AssigneeFieldId",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "AssigneeType",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "DependsOnStepIds",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "TargetType",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "ActionId",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropColumn(
                name: "DelegationReason",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropColumn(
                name: "SignatureData",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropColumn(
                name: "SignatureIP",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropColumn(
                name: "SignatureTimestamp",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropColumn(
                name: "SignatureType",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropColumn(
                name: "TargetType",
                table: "SubmissionWorkflowProgress");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "CancelledDate",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "RecurrenceDayOf",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "RecurrenceEndDate",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "RecurrencePattern",
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

            migrationBuilder.DropColumn(
                name: "ReportingMonth",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "ReportingYear",
                table: "FormTemplateAssignments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FormTemplateAssignments");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WorkflowStep_Approver",
                table: "WorkflowSteps",
                sql: "(ApproverRoleId IS NOT NULL AND ApproverUserId IS NULL) OR (ApproverRoleId IS NULL AND ApproverUserId IS NOT NULL)");
        }
    }
}

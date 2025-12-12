using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftFieldsToFormTemplateSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentSection",
                table: "FormTemplateSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSavedDate",
                table: "FormTemplateSubmissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submission_User_Status",
                table: "FormTemplateSubmissions",
                columns: new[] { "SubmittedBy", "Status" })
                .Annotation("SqlServer:Include", new[] { "TemplateId", "CreatedDate", "LastSavedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submission_User_Status",
                table: "FormTemplateSubmissions");

            migrationBuilder.DropColumn(
                name: "CurrentSection",
                table: "FormTemplateSubmissions");

            migrationBuilder.DropColumn(
                name: "LastSavedDate",
                table: "FormTemplateSubmissions");
        }
    }
}

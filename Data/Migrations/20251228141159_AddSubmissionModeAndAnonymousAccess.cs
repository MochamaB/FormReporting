using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionModeAndAnonymousAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowAnonymousAccess",
                table: "FormTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SubmissionMode",
                table: "FormTemplates",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_AnonymousAccess",
                table: "FormTemplates",
                column: "AllowAnonymousAccess");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_SubmissionMode",
                table: "FormTemplates",
                column: "SubmissionMode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Templates_AnonymousAccess",
                table: "FormTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_SubmissionMode",
                table: "FormTemplates");

            migrationBuilder.DropColumn(
                name: "AllowAnonymousAccess",
                table: "FormTemplates");

            migrationBuilder.DropColumn(
                name: "SubmissionMode",
                table: "FormTemplates");
        }
    }
}

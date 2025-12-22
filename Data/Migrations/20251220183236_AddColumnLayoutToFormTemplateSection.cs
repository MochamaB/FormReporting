using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnLayoutToFormTemplateSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColumnLayout",
                table: "FormTemplateSections",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColumnLayout",
                table: "FormTemplateSections");
        }
    }
}

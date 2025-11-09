using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MenuSectionId",
                table: "Modules",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MenuSections",
                columns: table => new
                {
                    MenuSectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SectionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuSections", x => x.MenuSectionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_MenuSectionId",
                table: "Modules",
                column: "MenuSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_MenuSections_MenuSectionId",
                table: "Modules",
                column: "MenuSectionId",
                principalTable: "MenuSections",
                principalColumn: "MenuSectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_MenuSections_MenuSectionId",
                table: "Modules");

            migrationBuilder.DropTable(
                name: "MenuSections");

            migrationBuilder.DropIndex(
                name: "IX_Modules_MenuSectionId",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "MenuSectionId",
                table: "Modules");
        }
    }
}

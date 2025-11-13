using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScopeLevelAndUpdateRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Roles",
                newName: "ScopeLevelId");

            migrationBuilder.CreateTable(
                name: "ScopeLevels",
                columns: table => new
                {
                    ScopeLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScopeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScopeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScopeLevels", x => x.ScopeLevelId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ScopeLevelId",
                table: "Roles",
                column: "ScopeLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ScopeLevel_Level",
                table: "ScopeLevels",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "UQ_ScopeLevel_Code",
                table: "ScopeLevels",
                column: "ScopeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ScopeLevel_Name",
                table: "ScopeLevels",
                column: "ScopeName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_ScopeLevels_ScopeLevelId",
                table: "Roles",
                column: "ScopeLevelId",
                principalTable: "ScopeLevels",
                principalColumn: "ScopeLevelId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_ScopeLevels_ScopeLevelId",
                table: "Roles");

            migrationBuilder.DropTable(
                name: "ScopeLevels");

            migrationBuilder.DropIndex(
                name: "IX_Roles_ScopeLevelId",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "ScopeLevelId",
                table: "Roles",
                newName: "Level");
        }
    }
}

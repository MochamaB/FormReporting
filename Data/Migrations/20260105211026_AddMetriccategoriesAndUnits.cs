using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMetriccategoriesAndUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Metrics_Category",
                table: "MetricDefinitions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Metric_Unit",
                table: "MetricDefinitions");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "MetricDefinitions");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "MetricDefinitions");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "MetricDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "MetricDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MetricCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ColorHint = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "MetricUnits",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitSymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    FormatPattern = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SuggestedAggregation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UnitCategory = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricUnits", x => x.UnitId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetricDefinitions_UnitId",
                table: "MetricDefinitions",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_Category",
                table: "MetricDefinitions",
                columns: new[] { "CategoryId", "IsKPI", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricCategories_Active_Order",
                table: "MetricCategories",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricCategories_CategoryCode",
                table: "MetricCategories",
                column: "CategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetricUnits_Active_Order",
                table: "MetricUnits",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricUnits_Category",
                table: "MetricUnits",
                column: "UnitCategory");

            migrationBuilder.CreateIndex(
                name: "IX_MetricUnits_UnitCode",
                table: "MetricUnits",
                column: "UnitCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MetricDefinitions_MetricCategories_CategoryId",
                table: "MetricDefinitions",
                column: "CategoryId",
                principalTable: "MetricCategories",
                principalColumn: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MetricDefinitions_MetricUnits_UnitId",
                table: "MetricDefinitions",
                column: "UnitId",
                principalTable: "MetricUnits",
                principalColumn: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetricDefinitions_MetricCategories_CategoryId",
                table: "MetricDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_MetricDefinitions_MetricUnits_UnitId",
                table: "MetricDefinitions");

            migrationBuilder.DropTable(
                name: "MetricCategories");

            migrationBuilder.DropTable(
                name: "MetricUnits");

            migrationBuilder.DropIndex(
                name: "IX_MetricDefinitions_UnitId",
                table: "MetricDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Metrics_Category",
                table: "MetricDefinitions");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "MetricDefinitions");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "MetricDefinitions");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "MetricDefinitions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "MetricDefinitions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_Category",
                table: "MetricDefinitions",
                columns: new[] { "Category", "IsKPI", "IsActive" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Metric_Unit",
                table: "MetricDefinitions",
                sql: "Unit IS NULL OR Unit IN ('Count', 'Percentage', 'Version', 'Status', 'Days', 'Hours', 'Minutes', 'Seconds', 'GB', 'MB', 'KB', 'TB', 'Bytes', 'None')");
        }
    }
}

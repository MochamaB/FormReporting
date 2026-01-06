using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMetricSubcategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetricDefinitions_MetricCategories_CategoryId",
                table: "MetricDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_MetricDefinitions_MetricUnits_UnitId",
                table: "MetricDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Metrics_Category",
                table: "MetricDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_ItemMetricMap_Type",
                table: "FormItemMetricMappings");

            migrationBuilder.DropColumn(
                name: "AggregationType",
                table: "FormItemMetricMappings");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "MetricDefinitions",
                newName: "MetricUnitUnitId");

            migrationBuilder.AddColumn<int>(
                name: "SubCategoryId",
                table: "MetricDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "MappingType",
                table: "FormItemMetricMappings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Direct",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "ComparisonOperator",
                table: "FormItemMetricMappings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Equals");

            migrationBuilder.AddColumn<string>(
                name: "OutputType",
                table: "FormItemMetricMappings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Raw");

            migrationBuilder.CreateTable(
                name: "MetricSubCategories",
                columns: table => new
                {
                    SubCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubCategoryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubCategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AllowedDataTypes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllowedAggregationTypes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultDataType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefaultAggregationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefaultUnitId = table.Column<int>(type: "int", nullable: true),
                    AllowedScopes = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DefaultScope = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SuggestedThresholdGreen = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SuggestedThresholdYellow = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SuggestedThresholdRed = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricSubCategories", x => x.SubCategoryId);
                    table.ForeignKey(
                        name: "FK_MetricSubCategories_MetricCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "MetricCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetricSubCategories_MetricUnits_DefaultUnitId",
                        column: x => x.DefaultUnitId,
                        principalTable: "MetricUnits",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MetricSubCategoryUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricSubCategoryUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetricSubCategoryUnits_MetricSubCategories_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalTable: "MetricSubCategories",
                        principalColumn: "SubCategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetricSubCategoryUnits_MetricUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "MetricUnits",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetricDefinitions_MetricUnitUnitId",
                table: "MetricDefinitions",
                column: "MetricUnitUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_SubCategory",
                table: "MetricDefinitions",
                columns: new[] { "SubCategoryId", "IsKPI", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemMetricMap_Type",
                table: "FormItemMetricMappings",
                columns: new[] { "MappingType", "OutputType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricSubCategories_CategoryId_DisplayOrder",
                table: "MetricSubCategories",
                columns: new[] { "CategoryId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricSubCategories_DefaultUnitId",
                table: "MetricSubCategories",
                column: "DefaultUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MetricSubCategories_SubCategoryCode",
                table: "MetricSubCategories",
                column: "SubCategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetricSubCategoryUnits_SubCategoryId_DisplayOrder",
                table: "MetricSubCategoryUnits",
                columns: new[] { "SubCategoryId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricSubCategoryUnits_SubCategoryId_UnitId",
                table: "MetricSubCategoryUnits",
                columns: new[] { "SubCategoryId", "UnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetricSubCategoryUnits_UnitId",
                table: "MetricSubCategoryUnits",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_MetricDefinitions_MetricSubCategories_SubCategoryId",
                table: "MetricDefinitions",
                column: "SubCategoryId",
                principalTable: "MetricSubCategories",
                principalColumn: "SubCategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MetricDefinitions_MetricUnits_MetricUnitUnitId",
                table: "MetricDefinitions",
                column: "MetricUnitUnitId",
                principalTable: "MetricUnits",
                principalColumn: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_MetricDefinitions_MetricUnits_UnitId",
                table: "MetricDefinitions",
                column: "UnitId",
                principalTable: "MetricUnits",
                principalColumn: "UnitId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetricDefinitions_MetricSubCategories_SubCategoryId",
                table: "MetricDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_MetricDefinitions_MetricUnits_MetricUnitUnitId",
                table: "MetricDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_MetricDefinitions_MetricUnits_UnitId",
                table: "MetricDefinitions");

            migrationBuilder.DropTable(
                name: "MetricSubCategoryUnits");

            migrationBuilder.DropTable(
                name: "MetricSubCategories");

            migrationBuilder.DropIndex(
                name: "IX_MetricDefinitions_MetricUnitUnitId",
                table: "MetricDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Metrics_SubCategory",
                table: "MetricDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_ItemMetricMap_Type",
                table: "FormItemMetricMappings");

            migrationBuilder.DropColumn(
                name: "SubCategoryId",
                table: "MetricDefinitions");

            migrationBuilder.DropColumn(
                name: "ComparisonOperator",
                table: "FormItemMetricMappings");

            migrationBuilder.DropColumn(
                name: "OutputType",
                table: "FormItemMetricMappings");

            migrationBuilder.RenameColumn(
                name: "MetricUnitUnitId",
                table: "MetricDefinitions",
                newName: "CategoryId");

            migrationBuilder.AlterColumn<string>(
                name: "MappingType",
                table: "FormItemMetricMappings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Direct");

            migrationBuilder.AddColumn<string>(
                name: "AggregationType",
                table: "FormItemMetricMappings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_Category",
                table: "MetricDefinitions",
                columns: new[] { "CategoryId", "IsKPI", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemMetricMap_Type",
                table: "FormItemMetricMappings",
                columns: new[] { "MappingType", "IsActive" });

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
    }
}

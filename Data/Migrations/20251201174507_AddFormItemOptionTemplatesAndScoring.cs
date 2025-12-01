using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFormItemOptionTemplatesAndScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ScoreValue",
                table: "FormItemOptions",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ScoreWeight",
                table: "FormItemOptions",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FormItemOptionTemplateCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormItemOptionTemplateCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "FormItemOptionTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TemplateCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SubCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ApplicableFieldTypes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecommendedFor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HasScoring = table.Column<bool>(type: "bit", nullable: false),
                    ScoringType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsSystemTemplate = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormItemOptionTemplates", x => x.TemplateId);
                    table.ForeignKey(
                        name: "FK_FormItemOptionTemplates_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormItemOptionTemplates_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormItemOptionTemplates_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormItemOptionTemplateCategoryMappings",
                columns: table => new
                {
                    FormItemOptionTemplateCategoryCategoryId = table.Column<int>(type: "int", nullable: false),
                    TemplatesTemplateId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormItemOptionTemplateCategoryMappings", x => new { x.FormItemOptionTemplateCategoryCategoryId, x.TemplatesTemplateId });
                    table.ForeignKey(
                        name: "FK_FormItemOptionTemplateCategoryMappings_FormItemOptionTemplateCategories_FormItemOptionTemplateCategoryCategoryId",
                        column: x => x.FormItemOptionTemplateCategoryCategoryId,
                        principalTable: "FormItemOptionTemplateCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormItemOptionTemplateCategoryMappings_FormItemOptionTemplates_TemplatesTemplateId",
                        column: x => x.TemplatesTemplateId,
                        principalTable: "FormItemOptionTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormItemOptionTemplateItems",
                columns: table => new
                {
                    TemplateItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    OptionValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OptionLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ScoreValue = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ScoreWeight = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ColorHint = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormItemOptionTemplateItems", x => x.TemplateItemId);
                    table.ForeignKey(
                        name: "FK_FormItemOptionTemplateItems_FormItemOptionTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FormItemOptionTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormItemOptionTemplateCategories_CategoryName",
                table: "FormItemOptionTemplateCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormItemOptionTemplateCategories_DisplayOrder",
                table: "FormItemOptionTemplateCategories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_FormItemOptionTemplateCategoryMappings_TemplatesTemplateId",
                table: "FormItemOptionTemplateCategoryMappings",
                column: "TemplatesTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FormItemOptionTemplateItems_Template_Order",
                table: "FormItemOptionTemplateItems",
                columns: new[] { "TemplateId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormItemOptionTemplates_Category_IsActive",
                table: "FormItemOptionTemplates",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_FormItemOptionTemplates_CreatedBy",
                table: "FormItemOptionTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormItemOptionTemplates_DisplayOrder",
                table: "FormItemOptionTemplates",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_FormItemOptionTemplates_ModifiedBy",
                table: "FormItemOptionTemplates",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FormItemOptionTemplates_TemplateCode",
                table: "FormItemOptionTemplates",
                column: "TemplateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormItemOptionTemplates_Tenant_IsActive",
                table: "FormItemOptionTemplates",
                columns: new[] { "TenantId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormItemOptionTemplateCategoryMappings");

            migrationBuilder.DropTable(
                name: "FormItemOptionTemplateItems");

            migrationBuilder.DropTable(
                name: "FormItemOptionTemplateCategories");

            migrationBuilder.DropTable(
                name: "FormItemOptionTemplates");

            migrationBuilder.DropColumn(
                name: "ScoreValue",
                table: "FormItemOptions");

            migrationBuilder.DropColumn(
                name: "ScoreWeight",
                table: "FormItemOptions");
        }
    }
}

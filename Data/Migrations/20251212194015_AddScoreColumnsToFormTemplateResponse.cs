using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScoreColumnsToFormTemplateResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SelectedOptionId",
                table: "FormTemplateResponses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SelectedScoreValue",
                table: "FormTemplateResponses",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SelectedScoreWeight",
                table: "FormTemplateResponses",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightedScore",
                table: "FormTemplateResponses",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplateResponses_SelectedOptionId",
                table: "FormTemplateResponses",
                column: "SelectedOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormTemplateResponses_FormItemOptions_SelectedOptionId",
                table: "FormTemplateResponses",
                column: "SelectedOptionId",
                principalTable: "FormItemOptions",
                principalColumn: "OptionId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormTemplateResponses_FormItemOptions_SelectedOptionId",
                table: "FormTemplateResponses");

            migrationBuilder.DropIndex(
                name: "IX_FormTemplateResponses_SelectedOptionId",
                table: "FormTemplateResponses");

            migrationBuilder.DropColumn(
                name: "SelectedOptionId",
                table: "FormTemplateResponses");

            migrationBuilder.DropColumn(
                name: "SelectedScoreValue",
                table: "FormTemplateResponses");

            migrationBuilder.DropColumn(
                name: "SelectedScoreWeight",
                table: "FormTemplateResponses");

            migrationBuilder.DropColumn(
                name: "WeightedScore",
                table: "FormTemplateResponses");
        }
    }
}

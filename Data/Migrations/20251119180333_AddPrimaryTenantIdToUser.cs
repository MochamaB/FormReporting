using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimaryTenantIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Users",
                type: "int",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_User_PrimaryTenant",
                table: "Users",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_PrimaryTenant",
                table: "Users",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_PrimaryTenant",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_User_PrimaryTenant",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");
        }
    }
}

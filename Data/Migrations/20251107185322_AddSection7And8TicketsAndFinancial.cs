using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSection7And8TicketsAndFinancial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    IsCapital = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetCategories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_BudgetCategories_BudgetCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "BudgetCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    SLAHours = table.Column<int>(type: "int", nullable: true),
                    EscalationHours = table.Column<int>(type: "int", nullable: true),
                    GenericCategoryMapping = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCategories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_TicketCategories_TicketCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "TicketCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantBudgets",
                columns: table => new
                {
                    BudgetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    BudgetedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "date", nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantBudgets", x => x.BudgetId);
                    table.ForeignKey(
                        name: "FK_TenantBudgets_BudgetCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "BudgetCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantBudgets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantExpenses",
                columns: table => new
                {
                    ExpenseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpenseType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Purchase"),
                    IsCapital = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VendorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AttachmentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "date", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantExpenses", x => x.ExpenseId);
                    table.CheckConstraint("CK_Expense_Type", "ExpenseType IN ('Purchase', 'Subscription', 'Maintenance', 'Service', 'Internal', 'Utility', 'Other')");
                    table.ForeignKey(
                        name: "FK_TenantExpenses_BudgetCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "BudgetCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantExpenses_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantExpenses_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Open"),
                    ReportedBy = table.Column<int>(type: "int", nullable: false),
                    ReportedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AssignedTo = table.Column<int>(type: "int", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscalatedTo = table.Column<int>(type: "int", nullable: true),
                    EscalatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<int>(type: "int", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolutionTime = table.Column<int>(type: "int", nullable: true),
                    SLADueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSLABreached = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExternalSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExternalTicketId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExternalTicketUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SyncStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Synced"),
                    SyncError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RelatedHardwareId = table.Column<int>(type: "int", nullable: true),
                    RelatedSoftwareId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.TicketId);
                    table.CheckConstraint("CK_Ticket_ExternalSystem", "ExternalSystem IN ('Internal', 'Jira', 'ServiceNow', 'Zendesk', 'Freshdesk', 'BMC', 'ManageEngine', 'Other') OR ExternalSystem IS NULL");
                    table.CheckConstraint("CK_Ticket_Priority", "Priority IN ('Low', 'Medium', 'High', 'Critical')");
                    table.CheckConstraint("CK_Ticket_Status", "Status IN ('Open', 'InProgress', 'Escalated', 'Resolved', 'Closed', 'Cancelled')");
                    table.CheckConstraint("CK_Ticket_SyncStatus", "SyncStatus IN ('Synced', 'Pending', 'Failed', 'NotApplicable')");
                    table.ForeignKey(
                        name: "FK_Tickets_TenantHardware_RelatedHardwareId",
                        column: x => x.RelatedHardwareId,
                        principalTable: "TenantHardware",
                        principalColumn: "TenantHardwareId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_TenantSoftwareInstallations_RelatedSoftwareId",
                        column: x => x.RelatedSoftwareId,
                        principalTable: "TenantSoftwareInstallations",
                        principalColumn: "InstallationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_TicketCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TicketCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_AssignedTo",
                        column: x => x.AssignedTo,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_EscalatedTo",
                        column: x => x.EscalatedTo,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_ReportedBy",
                        column: x => x.ReportedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_ResolvedBy",
                        column: x => x.ResolvedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketComments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsInternal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketComments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_TicketComments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "TicketId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetCategories_CategoryCode",
                table: "BudgetCategories",
                column: "CategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetCategories_ParentCategoryId",
                table: "BudgetCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantBudgets_CategoryId",
                table: "TenantBudgets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "UQ_TenantBudget",
                table: "TenantBudgets",
                columns: new[] { "TenantId", "FiscalYear", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_Date",
                table: "TenantExpenses",
                columns: new[] { "ExpenseDate", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_Tenant",
                table: "TenantExpenses",
                columns: new[] { "TenantId", "ExpenseDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_Type",
                table: "TenantExpenses",
                columns: new[] { "ExpenseType", "IsCapital" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantExpenses_CategoryId",
                table: "TenantExpenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantExpenses_CreatedBy",
                table: "TenantExpenses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TicketCategories_Active",
                table: "TicketCategories",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketCategories_CategoryCode",
                table: "TicketCategories",
                column: "CategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketCategories_Parent",
                table: "TicketCategories",
                column: "ParentCategoryId",
                filter: "ParentCategoryId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Ticket",
                table: "TicketComments",
                columns: new[] { "TicketId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_UserId",
                table: "TicketComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Assigned",
                table: "Tickets",
                columns: new[] { "AssignedTo", "Status" },
                filter: "AssignedTo IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CategoryId",
                table: "Tickets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_EscalatedTo",
                table: "Tickets",
                column: "EscalatedTo");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_External",
                table: "Tickets",
                columns: new[] { "ExternalSystem", "ExternalTicketId" },
                filter: "IsExternal = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ExternalId",
                table: "Tickets",
                column: "ExternalTicketId",
                filter: "ExternalTicketId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Hardware",
                table: "Tickets",
                column: "RelatedHardwareId",
                filter: "RelatedHardwareId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Priority",
                table: "Tickets",
                columns: new[] { "Priority", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ReportedBy",
                table: "Tickets",
                column: "ReportedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ResolvedBy",
                table: "Tickets",
                column: "ResolvedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SLA",
                table: "Tickets",
                columns: new[] { "SLADueDate", "Status" },
                filter: "Status <> 'Resolved' AND Status <> 'Closed'");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Software",
                table: "Tickets",
                column: "RelatedSoftwareId",
                filter: "RelatedSoftwareId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Status",
                table: "Tickets",
                columns: new[] { "Status", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Sync",
                table: "Tickets",
                columns: new[] { "LastSyncDate", "SyncStatus" },
                filter: "IsExternal = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SyncFailed",
                table: "Tickets",
                columns: new[] { "SyncStatus", "LastSyncDate" },
                filter: "SyncStatus = 'Failed'");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Tenant",
                table: "Tickets",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketNumber",
                table: "Tickets",
                column: "TicketNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantBudgets");

            migrationBuilder.DropTable(
                name: "TenantExpenses");

            migrationBuilder.DropTable(
                name: "TicketComments");

            migrationBuilder.DropTable(
                name: "BudgetCategories");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "TicketCategories");
        }
    }
}

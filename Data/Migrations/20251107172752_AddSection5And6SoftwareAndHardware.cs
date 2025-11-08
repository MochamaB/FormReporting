using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSection5And6SoftwareAndHardware : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HardwareCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HardwareCategories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_HardwareCategories_HardwareCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "HardwareCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareProducts",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProductCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LicenseModel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsKTDAProduct = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RequiresLicense = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareProducts", x => x.ProductId);
                    table.CheckConstraint("CK_Product_LicenseModel", "LicenseModel IN ('PerUser', 'PerDevice', 'Enterprise', 'Subscription', 'OpenSource', 'Concurrent') OR LicenseModel IS NULL");
                });

            migrationBuilder.CreateTable(
                name: "HardwareItems",
                columns: table => new
                {
                    HardwareItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Specifications = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HardwareItems", x => x.HardwareItemId);
                    table.ForeignKey(
                        name: "FK_HardwareItems_HardwareCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "HardwareCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareLicenses",
                columns: table => new
                {
                    LicenseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    LicenseKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LicenseType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "date", nullable: true),
                    QuantityPurchased = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    QuantityUsed = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PurchaseOrderNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Vendor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "KES"),
                    SupportContact = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupportPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SupportEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareLicenses", x => x.LicenseId);
                    table.CheckConstraint("CK_License_Quantity", "QuantityUsed <= QuantityPurchased");
                    table.CheckConstraint("CK_License_Type", "LicenseType IN ('Perpetual', 'Subscription', 'Trial', 'Volume', 'Academic', 'OEM')");
                    table.ForeignKey(
                        name: "FK_SoftwareLicenses_SoftwareProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "SoftwareProducts",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareVersions",
                columns: table => new
                {
                    VersionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VersionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MajorVersion = table.Column<int>(type: "int", nullable: true),
                    MinorVersion = table.Column<int>(type: "int", nullable: true),
                    PatchVersion = table.Column<int>(type: "int", nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "date", nullable: true),
                    EndOfLifeDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsCurrentVersion = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsSupported = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SecurityLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Stable"),
                    MinimumSupportedVersion = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReleaseNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DownloadUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    ChecksumSHA256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareVersions", x => x.VersionId);
                    table.CheckConstraint("CK_Version_SecurityLevel", "SecurityLevel IN ('Critical', 'Stable', 'Vulnerable', 'Unsupported')");
                    table.ForeignKey(
                        name: "FK_SoftwareVersions_SoftwareProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "SoftwareProducts",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantHardware",
                columns: table => new
                {
                    TenantHardwareId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    HardwareItemId = table.Column<int>(type: "int", nullable: false),
                    AssetTag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "date", nullable: true),
                    WarrantyExpiryDate = table.Column<DateTime>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantHardware", x => x.TenantHardwareId);
                    table.CheckConstraint("CK_TenantHw_Status", "Status IS NULL OR Status IN ('Operational', 'Faulty', 'UnderRepair', 'Retired', 'InStorage', 'PendingDeployment', 'Disposed')");
                    table.ForeignKey(
                        name: "FK_TenantHardware_HardwareItems_HardwareItemId",
                        column: x => x.HardwareItemId,
                        principalTable: "HardwareItems",
                        principalColumn: "HardwareItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantHardware_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantSoftwareInstallations",
                columns: table => new
                {
                    InstallationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VersionId = table.Column<int>(type: "int", nullable: false),
                    LicenseId = table.Column<int>(type: "int", nullable: true),
                    InstallationDate = table.Column<DateTime>(type: "date", nullable: true),
                    LastVerifiedDate = table.Column<DateTime>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    InstallationType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    InstallationPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VerifiedBy = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSoftwareInstallations", x => x.InstallationId);
                    table.CheckConstraint("CK_Install_Status", "Status IN ('Active', 'Deprecated', 'NeedsUpgrade', 'EndOfLife', 'Uninstalled')");
                    table.CheckConstraint("CK_Install_Type", "InstallationType IN ('Server', 'Workstation', 'Cloud', 'Virtual', 'Container') OR InstallationType IS NULL");
                    table.ForeignKey(
                        name: "FK_TenantSoftwareInstallations_SoftwareLicenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "SoftwareLicenses",
                        principalColumn: "LicenseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantSoftwareInstallations_SoftwareProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "SoftwareProducts",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantSoftwareInstallations_SoftwareVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "VersionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantSoftwareInstallations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantSoftwareInstallations_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantSoftwareInstallations_Users_VerifiedBy",
                        column: x => x.VerifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HardwareMaintenanceLog",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantHardwareId = table.Column<int>(type: "int", nullable: false),
                    MaintenanceDate = table.Column<DateTime>(type: "date", nullable: false),
                    MaintenanceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformedBy = table.Column<int>(type: "int", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "date", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HardwareMaintenanceLog", x => x.LogId);
                    table.CheckConstraint("CK_Maintenance_Type", "MaintenanceType IS NULL OR MaintenanceType IN ('Preventive', 'Corrective', 'Upgrade', 'Emergency', 'Calibration', 'Inspection')");
                    table.ForeignKey(
                        name: "FK_HardwareMaintenanceLog_TenantHardware_TenantHardwareId",
                        column: x => x.TenantHardwareId,
                        principalTable: "TenantHardware",
                        principalColumn: "TenantHardwareId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareInstallationHistory",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstallationId = table.Column<int>(type: "int", nullable: false),
                    FromVersionId = table.Column<int>(type: "int", nullable: true),
                    ToVersionId = table.Column<int>(type: "int", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ChangedBy = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SuccessStatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareInstallationHistory", x => x.HistoryId);
                    table.CheckConstraint("CK_History_ChangeType", "ChangeType IN ('Install', 'Upgrade', 'Downgrade', 'Uninstall', 'Reinstall', 'Patch')");
                    table.ForeignKey(
                        name: "FK_SoftwareInstallationHistory_SoftwareVersions_FromVersionId",
                        column: x => x.FromVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "VersionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoftwareInstallationHistory_SoftwareVersions_ToVersionId",
                        column: x => x.ToVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "VersionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoftwareInstallationHistory_TenantSoftwareInstallations_InstallationId",
                        column: x => x.InstallationId,
                        principalTable: "TenantSoftwareInstallations",
                        principalColumn: "InstallationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoftwareInstallationHistory_Users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HardwareCategories_CategoryCode",
                table: "HardwareCategories",
                column: "CategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HardwareCategories_ParentCategoryId",
                table: "HardwareCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HardwareItems_CategoryId",
                table: "HardwareItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HardwareItems_ItemCode",
                table: "HardwareItems",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HardwareMaintenanceLog_TenantHardwareId",
                table: "HardwareMaintenanceLog",
                column: "TenantHardwareId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallHistory_Date",
                table: "SoftwareInstallationHistory",
                column: "ChangeDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_InstallHistory_Installation",
                table: "SoftwareInstallationHistory",
                columns: new[] { "InstallationId", "ChangeDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_InstallHistory_Type",
                table: "SoftwareInstallationHistory",
                columns: new[] { "ChangeType", "ChangeDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_InstallHistory_User",
                table: "SoftwareInstallationHistory",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareInstallationHistory_FromVersionId",
                table: "SoftwareInstallationHistory",
                column: "FromVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareInstallationHistory_ToVersionId",
                table: "SoftwareInstallationHistory",
                column: "ToVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_Available",
                table: "SoftwareLicenses",
                columns: new[] { "ProductId", "QuantityPurchased", "QuantityUsed", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_Expiry",
                table: "SoftwareLicenses",
                column: "ExpiryDate",
                filter: "IsActive = 1 AND ExpiryDate IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_Product",
                table: "SoftwareLicenses",
                columns: new[] { "ProductId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "SoftwareProducts",
                columns: new[] { "ProductCategory", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Vendor",
                table: "SoftwareProducts",
                column: "Vendor");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareProducts_ProductCode",
                table: "SoftwareProducts",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Versions_EOL",
                table: "SoftwareVersions",
                column: "EndOfLifeDate",
                filter: "EndOfLifeDate IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Versions_Product",
                table: "SoftwareVersions",
                columns: new[] { "ProductId", "MajorVersion", "MinorVersion", "PatchVersion" },
                descending: new[] { false, true, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Versions_Security",
                table: "SoftwareVersions",
                columns: new[] { "SecurityLevel", "EndOfLifeDate" },
                filter: "IsSupported = 1");

            migrationBuilder.CreateIndex(
                name: "UQ_ProductVersion",
                table: "SoftwareVersions",
                columns: new[] { "ProductId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantHardware_HardwareItemId",
                table: "TenantHardware",
                column: "HardwareItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantHw_Status",
                table: "TenantHardware",
                column: "Status",
                filter: "Status != 'Retired'");

            migrationBuilder.CreateIndex(
                name: "IX_TenantHw_Tenant",
                table: "TenantHardware",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Installations_License",
                table: "TenantSoftwareInstallations",
                column: "LicenseId",
                filter: "LicenseId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Installations_Product",
                table: "TenantSoftwareInstallations",
                columns: new[] { "ProductId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Installations_Tenant",
                table: "TenantSoftwareInstallations",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Installations_Verification",
                table: "TenantSoftwareInstallations",
                column: "LastVerifiedDate",
                filter: "Status = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_Installations_Version",
                table: "TenantSoftwareInstallations",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSoftwareInstallations_ModifiedBy",
                table: "TenantSoftwareInstallations",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSoftwareInstallations_VerifiedBy",
                table: "TenantSoftwareInstallations",
                column: "VerifiedBy");

            migrationBuilder.CreateIndex(
                name: "UQ_Install_Tenant_Product_Type",
                table: "TenantSoftwareInstallations",
                columns: new[] { "TenantId", "ProductId", "InstallationType", "MachineName" },
                unique: true,
                filter: "[InstallationType] IS NOT NULL AND [MachineName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HardwareMaintenanceLog");

            migrationBuilder.DropTable(
                name: "SoftwareInstallationHistory");

            migrationBuilder.DropTable(
                name: "TenantHardware");

            migrationBuilder.DropTable(
                name: "TenantSoftwareInstallations");

            migrationBuilder.DropTable(
                name: "HardwareItems");

            migrationBuilder.DropTable(
                name: "SoftwareLicenses");

            migrationBuilder.DropTable(
                name: "SoftwareVersions");

            migrationBuilder.DropTable(
                name: "HardwareCategories");

            migrationBuilder.DropTable(
                name: "SoftwareProducts");
        }
    }
}

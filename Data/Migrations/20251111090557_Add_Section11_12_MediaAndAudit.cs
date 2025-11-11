using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Section11_12_MediaAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecordId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedBy = table.Column<int>(type: "int", nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    FileId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StorageProvider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StorageContainer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IsImage = table.Column<bool>(type: "bit", nullable: false),
                    ImageWidth = table.Column<int>(type: "int", nullable: true),
                    ImageHeight = table.Column<int>(type: "int", nullable: true),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PageCount = table.Column<int>(type: "int", nullable: true),
                    DocumentTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccessLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    EncryptionKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsVirusSafe = table.Column<bool>(type: "bit", nullable: false),
                    VirusScanDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VirusScanResult = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UploadedBy = table.Column<int>(type: "int", nullable: false),
                    UploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAccessedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccessCount = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeleteReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SearchableText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.FileId);
                    table.CheckConstraint("CK_MediaFile_Access", "[AccessLevel] IN ('Public', 'Private', 'Internal', 'Confidential', 'Restricted')");
                    table.CheckConstraint("CK_MediaFile_Provider", "[StorageProvider] IN ('Local', 'Azure', 'AWS', 'OneDrive', 'SharePoint', 'GoogleDrive')");
                    table.ForeignKey(
                        name: "FK_MediaFiles_Users_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_MediaFiles_Users_UploadedBy",
                        column: x => x.UploadedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserActivityLog",
                columns: table => new
                {
                    ActivityId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeviceInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActivityDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityLog", x => x.ActivityId);
                    table.ForeignKey(
                        name: "FK_UserActivityLog_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntityMediaFiles",
                columns: table => new
                {
                    EntityMediaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<long>(type: "bigint", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<long>(type: "bigint", nullable: false),
                    AttachmentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResponseId = table.Column<long>(type: "bigint", nullable: true),
                    AttachedBy = table.Column<int>(type: "int", nullable: false),
                    AttachedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityMediaFiles", x => x.EntityMediaId);
                    table.CheckConstraint("CK_EntityMedia_Type", "[EntityType] IN ('Expense', 'Budget', 'Ticket', 'FormResponse', 'FormSubmission', 'Hardware', 'Software', 'User', 'Tenant', 'Training', 'Project', 'Maintenance', 'Audit', 'Report', 'Other')");
                    table.ForeignKey(
                        name: "FK_EntityMediaFiles_MediaFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "MediaFiles",
                        principalColumn: "FileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityMediaFiles_Users_AttachedBy",
                        column: x => x.AttachedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "FileAccessLog",
                columns: table => new
                {
                    AccessLogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<long>(type: "bigint", nullable: false),
                    AccessedBy = table.Column<int>(type: "int", nullable: false),
                    AccessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccessType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccessResult = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAccessLog", x => x.AccessLogId);
                    table.CheckConstraint("CK_FileAccess_Result", "[AccessResult] IN ('Success', 'Denied', 'NotFound', 'Error')");
                    table.CheckConstraint("CK_FileAccess_Type", "[AccessType] IN ('View', 'Download', 'Delete', 'Update', 'Share', 'Scan')");
                    table.ForeignKey(
                        name: "FK_FileAccessLog_MediaFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "MediaFiles",
                        principalColumn: "FileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileAccessLog_Users_AccessedBy",
                        column: x => x.AccessedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Date",
                table: "AuditLogs",
                column: "ChangedDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Table",
                table: "AuditLogs",
                columns: new[] { "TableName", "RecordId", "ChangedDate" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_User",
                table: "AuditLogs",
                columns: new[] { "ChangedBy", "ChangedDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_EntityMedia_Entity",
                table: "EntityMediaFiles",
                columns: new[] { "EntityType", "EntityId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityMedia_File",
                table: "EntityMediaFiles",
                columns: new[] { "FileId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityMedia_Primary",
                table: "EntityMediaFiles",
                columns: new[] { "EntityType", "EntityId", "IsPrimary" },
                filter: "[IsPrimary] = 1 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_EntityMedia_Response",
                table: "EntityMediaFiles",
                column: "ResponseId",
                filter: "[ResponseId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EntityMedia_Type",
                table: "EntityMediaFiles",
                columns: new[] { "AttachmentType", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityMediaFiles_AttachedBy",
                table: "EntityMediaFiles",
                column: "AttachedBy");

            migrationBuilder.CreateIndex(
                name: "UQ_EntityMedia_OnePrimary",
                table: "EntityMediaFiles",
                columns: new[] { "EntityType", "EntityId" },
                unique: true,
                filter: "[IsPrimary] = 1 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_FileAccess_Date",
                table: "FileAccessLog",
                column: "AccessDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_FileAccess_File",
                table: "FileAccessLog",
                columns: new[] { "FileId", "AccessDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_FileAccess_Type",
                table: "FileAccessLog",
                columns: new[] { "AccessType", "AccessDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_FileAccess_User",
                table: "FileAccessLog",
                columns: new[] { "AccessedBy", "AccessDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_DeletedBy",
                table: "MediaFiles",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Expiry",
                table: "MediaFiles",
                column: "ExpiryDate",
                filter: "[ExpiryDate] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Hash",
                table: "MediaFiles",
                column: "FileHash",
                filter: "[FileHash] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Images",
                table: "MediaFiles",
                column: "IsImage",
                filter: "[IsImage] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Storage",
                table: "MediaFiles",
                columns: new[] { "StorageProvider", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Type",
                table: "MediaFiles",
                columns: new[] { "MimeType", "IsDeleted" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Uploader",
                table: "MediaFiles",
                columns: new[] { "UploadedBy", "UploadedDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_VirusScan",
                table: "MediaFiles",
                columns: new[] { "IsVirusSafe", "VirusScanDate" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_Date",
                table: "UserActivityLog",
                column: "ActivityDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Activity_User",
                table: "UserActivityLog",
                columns: new[] { "UserId", "ActivityDate" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "EntityMediaFiles");

            migrationBuilder.DropTable(
                name: "FileAccessLog");

            migrationBuilder.DropTable(
                name: "UserActivityLog");

            migrationBuilder.DropTable(
                name: "MediaFiles");
        }
    }
}

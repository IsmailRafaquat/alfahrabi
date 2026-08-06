using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class Added_ShopProfitLossAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    NavigationUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ActionLabel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SourceKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SourceDataJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TriggeredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DismissedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopNotificationSettingsList",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LowStockNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    OutOfStockNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NearExpiryNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ExpiredBatchNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CustomerDueNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SupplierDueNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CashDifferenceNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    BankLowBalanceNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PendingDraftNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ProfitLossWarningNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NearExpiryDefaultDays = table.Column<int>(type: "int", nullable: false),
                    CustomerDueReminderDays = table.Column<int>(type: "int", nullable: false),
                    SupplierDueReminderDays = table.Column<int>(type: "int", nullable: false),
                    DraftPendingHours = table.Column<int>(type: "int", nullable: false),
                    BankLowBalanceThreshold = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ProfitLossWarningThreshold = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    NotificationRetentionDays = table.Column<int>(type: "int", nullable: false),
                    EmailNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastExpiryCheckDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastProfitLossCheckDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopNotificationSettingsList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopNotificationUserStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DismissedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopNotificationUserStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopNotificationUserStates_ShopNotifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "ShopNotifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId",
                table: "ShopNotifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId_ExpiresDate",
                table: "ShopNotifications",
                columns: new[] { "TenantId", "ExpiresDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId_Severity_Status",
                table: "ShopNotifications",
                columns: new[] { "TenantId", "Severity", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId_SourceKey",
                table: "ShopNotifications",
                columns: new[] { "TenantId", "SourceKey" },
                unique: true,
                filter: "[Status] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId_TriggeredDate",
                table: "ShopNotifications",
                columns: new[] { "TenantId", "TriggeredDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId_Type_Status",
                table: "ShopNotifications",
                columns: new[] { "TenantId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId_UserId_Status",
                table: "ShopNotifications",
                columns: new[] { "TenantId", "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotificationSettingsList_TenantId",
                table: "ShopNotificationSettingsList",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotificationUserStates_NotificationId",
                table: "ShopNotificationUserStates",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotificationUserStates_TenantId",
                table: "ShopNotificationUserStates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotificationUserStates_TenantId_NotificationId_UserId",
                table: "ShopNotificationUserStates",
                columns: new[] { "TenantId", "NotificationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotificationUserStates_TenantId_UserId_Status",
                table: "ShopNotificationUserStates",
                columns: new[] { "TenantId", "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopNotificationSettingsList");

            migrationBuilder.DropTable(
                name: "ShopNotificationUserStates");

            migrationBuilder.DropTable(
                name: "ShopNotifications");
        }
    }
}

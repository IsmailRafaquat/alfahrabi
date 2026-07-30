using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class Added_ShopAiGuidedCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopAiPendingActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    ModuleKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CollectedValuesJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    MissingFieldsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LookupResolutionsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmationTokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopAiPendingActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopAiPendingActions_ShopAiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ShopAiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopAiPendingActions_ShopAiMessages_SourceMessageId",
                        column: x => x.SourceMessageId,
                        principalTable: "ShopAiMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiPendingActions_ConversationId",
                table: "ShopAiPendingActions",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiPendingActions_SourceMessageId",
                table: "ShopAiPendingActions",
                column: "SourceMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiPendingActions_TenantId",
                table: "ShopAiPendingActions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiPendingActions_TenantId_ConversationId_Status",
                table: "ShopAiPendingActions",
                columns: new[] { "TenantId", "ConversationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiPendingActions_TenantId_ExpiryDate",
                table: "ShopAiPendingActions",
                columns: new[] { "TenantId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiPendingActions_TenantId_UserId_Status",
                table: "ShopAiPendingActions",
                columns: new[] { "TenantId", "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopAiPendingActions");
        }
    }
}

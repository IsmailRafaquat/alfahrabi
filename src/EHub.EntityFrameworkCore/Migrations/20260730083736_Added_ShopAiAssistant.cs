using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class Added_ShopAiAssistant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopAiConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DetectedLanguage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastMessageDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ShopAiConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopAiMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    OriginalTranscription = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    DetectedLanguage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DetectedAction = table.Column<int>(type: "int", nullable: true),
                    ActionPayloadJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ErrorCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConfirmationTokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ConfirmationExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutionResultJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopAiMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopAiMessages_ShopAiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ShopAiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopAiActionAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SanitizedPayloadJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    ConfirmationRequired = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConfirmationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutionStatus = table.Column<int>(type: "int", nullable: false),
                    ResultReferenceType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ResultReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopAiActionAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopAiActionAudits_ShopAiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ShopAiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopAiActionAudits_ShopAiMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ShopAiMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiActionAudits_ConversationId",
                table: "ShopAiActionAudits",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiActionAudits_MessageId",
                table: "ShopAiActionAudits",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiActionAudits_TenantId",
                table: "ShopAiActionAudits",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiActionAudits_TenantId_ActionName_CreationTime",
                table: "ShopAiActionAudits",
                columns: new[] { "TenantId", "ActionName", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiActionAudits_TenantId_ExecutionStatus",
                table: "ShopAiActionAudits",
                columns: new[] { "TenantId", "ExecutionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiActionAudits_TenantId_UserId_CreationTime",
                table: "ShopAiActionAudits",
                columns: new[] { "TenantId", "UserId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiConversations_TenantId",
                table: "ShopAiConversations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiConversations_TenantId_Status",
                table: "ShopAiConversations",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiConversations_TenantId_UserId_LastMessageDate",
                table: "ShopAiConversations",
                columns: new[] { "TenantId", "UserId", "LastMessageDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiMessages_ConversationId",
                table: "ShopAiMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiMessages_TenantId",
                table: "ShopAiMessages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiMessages_TenantId_ConversationId_CreationTime",
                table: "ShopAiMessages",
                columns: new[] { "TenantId", "ConversationId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiMessages_TenantId_DetectedAction",
                table: "ShopAiMessages",
                columns: new[] { "TenantId", "DetectedAction" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiMessages_TenantId_Status",
                table: "ShopAiMessages",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiMessages_TenantId_UserId_CreationTime",
                table: "ShopAiMessages",
                columns: new[] { "TenantId", "UserId", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopAiActionAudits");

            migrationBuilder.DropTable(
                name: "ShopAiMessages");

            migrationBuilder.DropTable(
                name: "ShopAiConversations");
        }
    }
}

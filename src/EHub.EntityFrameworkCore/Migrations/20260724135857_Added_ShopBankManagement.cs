using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class Added_ShopBankManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                table: "ShopSupplierPayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                table: "ShopSaleReturns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                table: "ShopExpenses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                table: "ShopCustomerPayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShopBankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_ShopBankAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopBankTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReferenceType = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BalanceAfterTransaction = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SourceItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReversalOfTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsReversal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopBankTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopBankTransactions_ShopBankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "ShopBankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopBankTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferType = table.Column<int>(type: "int", nullable: false),
                    FromBankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToBankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CashRegisterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PostedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ShopBankTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopBankTransfers_ShopBankAccounts_FromBankAccountId",
                        column: x => x.FromBankAccountId,
                        principalTable: "ShopBankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopBankTransfers_ShopBankAccounts_ToBankAccountId",
                        column: x => x.ToBankAccountId,
                        principalTable: "ShopBankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankAccounts_TenantId",
                table: "ShopBankAccounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankAccounts_TenantId_BankName",
                table: "ShopBankAccounts",
                columns: new[] { "TenantId", "BankName" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankAccounts_TenantId_Code",
                table: "ShopBankAccounts",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankAccounts_TenantId_IsActive",
                table: "ShopBankAccounts",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankAccounts_TenantId_IsDefault",
                table: "ShopBankAccounts",
                columns: new[] { "TenantId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_BankAccountId",
                table: "ShopBankTransactions",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId",
                table: "ShopBankTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_BankAccountId",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "BankAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_Direction",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "Direction" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_ReferenceNumber",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "ReferenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_ReferenceType_ReferenceId",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_ReferenceType_ReferenceId_TransactionType_IsReversal",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "ReferenceType", "ReferenceId", "TransactionType", "IsReversal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_TransactionDate",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_TransactionType",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "TransactionType" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_FromBankAccountId",
                table: "ShopBankTransfers",
                column: "FromBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId",
                table: "ShopBankTransfers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId_FromBankAccountId",
                table: "ShopBankTransfers",
                columns: new[] { "TenantId", "FromBankAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId_Status",
                table: "ShopBankTransfers",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId_ToBankAccountId",
                table: "ShopBankTransfers",
                columns: new[] { "TenantId", "ToBankAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId_TransferDate",
                table: "ShopBankTransfers",
                columns: new[] { "TenantId", "TransferDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId_TransferNumber",
                table: "ShopBankTransfers",
                columns: new[] { "TenantId", "TransferNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId_TransferType",
                table: "ShopBankTransfers",
                columns: new[] { "TenantId", "TransferType" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_ToBankAccountId",
                table: "ShopBankTransfers",
                column: "ToBankAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopBankTransactions");

            migrationBuilder.DropTable(
                name: "ShopBankTransfers");

            migrationBuilder.DropTable(
                name: "ShopBankAccounts");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "ShopSupplierPayments");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "ShopSaleReturns");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "ShopExpenses");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "ShopCustomerPayments");
        }
    }
}

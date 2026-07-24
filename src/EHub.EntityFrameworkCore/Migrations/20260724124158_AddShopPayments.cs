using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class AddShopPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopCashRegisters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_ShopCashRegisters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopCashClosings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashRegisterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OpeningCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CashSales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CustomerCashPayments = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    SupplierCashPayments = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CashExpenses = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CustomerRefunds = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ManualCashIn = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ManualCashOut = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ExpectedClosingCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ActualClosingCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DifferenceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OpenedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OpenedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ShopCashClosings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopCashClosings_ShopCashRegisters_CashRegisterId",
                        column: x => x.CashRegisterId,
                        principalTable: "ShopCashRegisters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopCashRegisterTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashRegisterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashClosingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReferenceType = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SourceItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopCashRegisterTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopCashRegisterTransactions_ShopCashClosings_CashClosingId",
                        column: x => x.CashClosingId,
                        principalTable: "ShopCashClosings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopCashRegisterTransactions_ShopCashRegisters_CashRegisterId",
                        column: x => x.CashRegisterId,
                        principalTable: "ShopCashRegisters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashClosings_CashRegisterId",
                table: "ShopCashClosings",
                column: "CashRegisterId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashClosings_TenantId",
                table: "ShopCashClosings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashClosings_TenantId_CashRegisterId_BusinessDate",
                table: "ShopCashClosings",
                columns: new[] { "TenantId", "CashRegisterId", "BusinessDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashClosings_TenantId_CashRegisterId_Status",
                table: "ShopCashClosings",
                columns: new[] { "TenantId", "CashRegisterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisters_TenantId",
                table: "ShopCashRegisters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisters_TenantId_Code",
                table: "ShopCashRegisters",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisters_TenantId_IsDefault",
                table: "ShopCashRegisters",
                columns: new[] { "TenantId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_CashClosingId",
                table: "ShopCashRegisterTransactions",
                column: "CashClosingId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_CashRegisterId",
                table: "ShopCashRegisterTransactions",
                column: "CashRegisterId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_TenantId",
                table: "ShopCashRegisterTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_TenantId_ReferenceType_ReferenceId",
                table: "ShopCashRegisterTransactions",
                columns: new[] { "TenantId", "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_TenantId_ReferenceType_ReferenceId_TransactionType_Direction",
                table: "ShopCashRegisterTransactions",
                columns: new[] { "TenantId", "ReferenceType", "ReferenceId", "TransactionType", "Direction" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_TenantId_TransactionDate",
                table: "ShopCashRegisterTransactions",
                columns: new[] { "TenantId", "TransactionDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopCashRegisterTransactions");

            migrationBuilder.DropTable(
                name: "ShopCashClosings");

            migrationBuilder.DropTable(
                name: "ShopCashRegisters");
        }
    }
}

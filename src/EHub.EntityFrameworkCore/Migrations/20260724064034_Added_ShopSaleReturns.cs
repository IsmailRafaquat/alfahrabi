using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class Added_ShopSaleReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopSaleReturns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleReturnNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    ReasonDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SettlementType = table.Column<int>(type: "int", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CustomerCreditAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ShopSaleReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopSaleReturns_ShopCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "ShopCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleReturns_ShopSales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "ShopSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopSaleReturnItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleReturnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCodeSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitShortNameSnapshot = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoldQuantitySnapshot = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PreviouslyReturnedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    ReturnQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitSalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitCostSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    LineSubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopSaleReturnItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopSaleReturnItems_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleReturnItems_ShopSaleItems_SaleItemId",
                        column: x => x.SaleItemId,
                        principalTable: "ShopSaleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleReturnItems_ShopSaleReturns_SaleReturnId",
                        column: x => x.SaleReturnId,
                        principalTable: "ShopSaleReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturnItems_ProductId",
                table: "ShopSaleReturnItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturnItems_SaleItemId",
                table: "ShopSaleReturnItems",
                column: "SaleItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturnItems_SaleReturnId_SaleItemId",
                table: "ShopSaleReturnItems",
                columns: new[] { "SaleReturnId", "SaleItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturnItems_TenantId_SaleReturnId",
                table: "ShopSaleReturnItems",
                columns: new[] { "TenantId", "SaleReturnId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_CustomerId",
                table: "ShopSaleReturns",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_SaleId",
                table: "ShopSaleReturns",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_TenantId",
                table: "ShopSaleReturns",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_TenantId_CustomerId",
                table: "ShopSaleReturns",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_TenantId_ReturnDate",
                table: "ShopSaleReturns",
                columns: new[] { "TenantId", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_TenantId_SaleId",
                table: "ShopSaleReturns",
                columns: new[] { "TenantId", "SaleId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_TenantId_SaleReturnNumber",
                table: "ShopSaleReturns",
                columns: new[] { "TenantId", "SaleReturnNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_TenantId_Status",
                table: "ShopSaleReturns",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopSaleReturnItems");

            migrationBuilder.DropTable(
                name: "ShopSaleReturns");
        }
    }
}

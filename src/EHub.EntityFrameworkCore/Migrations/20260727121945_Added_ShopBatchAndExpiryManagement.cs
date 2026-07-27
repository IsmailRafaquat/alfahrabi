using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class Added_ShopBatchAndExpiryManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShopStockCountItems_StockCountId_ProductId",
                table: "ShopStockCountItems");

            migrationBuilder.DropIndex(
                name: "IX_ShopStockAdjustmentItems_StockAdjustmentId_ProductId",
                table: "ShopStockAdjustmentItems");

            migrationBuilder.AddColumn<decimal>(
                name: "BatchBalanceQuantity",
                table: "ShopStockTransactions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductBatchId",
                table: "ShopStockTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumberSnapshot",
                table: "ShopStockCountItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDateSnapshot",
                table: "ShopStockCountItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductBatchId",
                table: "ShopStockCountItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManufacturingDate",
                table: "ShopStockAdjustmentItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductBatchId",
                table: "ShopStockAdjustmentItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BlockExpiredSale",
                table: "ShopProducts",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "ExpiryAlertDays",
                table: "ShopProducts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShopProductBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    NormalizedBatchNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ManufacturingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    IssuedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    AvailableQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    ReservedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FirstReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastMovementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GoodsReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GoodsReceiptItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    BlockReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ShopProductBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopProductBatches_ShopGoodsReceiptItems_GoodsReceiptItemId",
                        column: x => x.GoodsReceiptItemId,
                        principalTable: "ShopGoodsReceiptItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopProductBatches_ShopGoodsReceipts_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalTable: "ShopGoodsReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopProductBatches_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopProductBatches_ShopSuppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "ShopSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopSaleItemBatchAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchNumberSnapshot = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiryDateSnapshot = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCostSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopSaleItemBatchAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopSaleItemBatchAllocations_ShopProductBatches_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "ShopProductBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleItemBatchAllocations_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleItemBatchAllocations_ShopSaleItems_SaleItemId",
                        column: x => x.SaleItemId,
                        principalTable: "ShopSaleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleItemBatchAllocations_ShopSales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "ShopSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_ProductBatchId",
                table: "ShopStockTransactions",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId_ProductBatchId",
                table: "ShopStockTransactions",
                columns: new[] { "TenantId", "ProductBatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_ProductBatchId",
                table: "ShopStockCountItems",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_StockCountId_ProductId_ProductBatchId",
                table: "ShopStockCountItems",
                columns: new[] { "StockCountId", "ProductId", "ProductBatchId" },
                unique: true,
                filter: "[ProductBatchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustmentItems_ProductBatchId",
                table: "ShopStockAdjustmentItems",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustmentItems_StockAdjustmentId_ProductId_ProductBatchId",
                table: "ShopStockAdjustmentItems",
                columns: new[] { "StockAdjustmentId", "ProductId", "ProductBatchId" },
                unique: true,
                filter: "[ProductBatchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_GoodsReceiptId",
                table: "ShopProductBatches",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_GoodsReceiptItemId",
                table: "ShopProductBatches",
                column: "GoodsReceiptItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_ProductId",
                table: "ShopProductBatches",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_SupplierId",
                table: "ShopProductBatches",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId",
                table: "ShopProductBatches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_AvailableQuantity",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "AvailableQuantity" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_ExpiryDate",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_ProductId",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_ProductId_NormalizedBatchNumber",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "ProductId", "NormalizedBatchNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_Status",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_SupplierId",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_ProductBatchId",
                table: "ShopSaleItemBatchAllocations",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_ProductId",
                table: "ShopSaleItemBatchAllocations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_SaleId",
                table: "ShopSaleItemBatchAllocations",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_SaleItemId_ProductBatchId",
                table: "ShopSaleItemBatchAllocations",
                columns: new[] { "SaleItemId", "ProductBatchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_TenantId",
                table: "ShopSaleItemBatchAllocations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_TenantId_ProductBatchId",
                table: "ShopSaleItemBatchAllocations",
                columns: new[] { "TenantId", "ProductBatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_TenantId_SaleId",
                table: "ShopSaleItemBatchAllocations",
                columns: new[] { "TenantId", "SaleId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_TenantId_SaleItemId",
                table: "ShopSaleItemBatchAllocations",
                columns: new[] { "TenantId", "SaleItemId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ShopStockAdjustmentItems_ShopProductBatches_ProductBatchId",
                table: "ShopStockAdjustmentItems",
                column: "ProductBatchId",
                principalTable: "ShopProductBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopStockCountItems_ShopProductBatches_ProductBatchId",
                table: "ShopStockCountItems",
                column: "ProductBatchId",
                principalTable: "ShopProductBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopStockTransactions_ShopProductBatches_ProductBatchId",
                table: "ShopStockTransactions",
                column: "ProductBatchId",
                principalTable: "ShopProductBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopStockAdjustmentItems_ShopProductBatches_ProductBatchId",
                table: "ShopStockAdjustmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopStockCountItems_ShopProductBatches_ProductBatchId",
                table: "ShopStockCountItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopStockTransactions_ShopProductBatches_ProductBatchId",
                table: "ShopStockTransactions");

            migrationBuilder.DropTable(
                name: "ShopSaleItemBatchAllocations");

            migrationBuilder.DropTable(
                name: "ShopProductBatches");

            migrationBuilder.DropIndex(
                name: "IX_ShopStockTransactions_ProductBatchId",
                table: "ShopStockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ShopStockTransactions_TenantId_ProductBatchId",
                table: "ShopStockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ShopStockCountItems_ProductBatchId",
                table: "ShopStockCountItems");

            migrationBuilder.DropIndex(
                name: "IX_ShopStockCountItems_StockCountId_ProductId_ProductBatchId",
                table: "ShopStockCountItems");

            migrationBuilder.DropIndex(
                name: "IX_ShopStockAdjustmentItems_ProductBatchId",
                table: "ShopStockAdjustmentItems");

            migrationBuilder.DropIndex(
                name: "IX_ShopStockAdjustmentItems_StockAdjustmentId_ProductId_ProductBatchId",
                table: "ShopStockAdjustmentItems");

            migrationBuilder.DropColumn(
                name: "BatchBalanceQuantity",
                table: "ShopStockTransactions");

            migrationBuilder.DropColumn(
                name: "ProductBatchId",
                table: "ShopStockTransactions");

            migrationBuilder.DropColumn(
                name: "BatchNumberSnapshot",
                table: "ShopStockCountItems");

            migrationBuilder.DropColumn(
                name: "ExpiryDateSnapshot",
                table: "ShopStockCountItems");

            migrationBuilder.DropColumn(
                name: "ProductBatchId",
                table: "ShopStockCountItems");

            migrationBuilder.DropColumn(
                name: "ManufacturingDate",
                table: "ShopStockAdjustmentItems");

            migrationBuilder.DropColumn(
                name: "ProductBatchId",
                table: "ShopStockAdjustmentItems");

            migrationBuilder.DropColumn(
                name: "BlockExpiredSale",
                table: "ShopProducts");

            migrationBuilder.DropColumn(
                name: "ExpiryAlertDays",
                table: "ShopProducts");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_StockCountId_ProductId",
                table: "ShopStockCountItems",
                columns: new[] { "StockCountId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustmentItems_StockAdjustmentId_ProductId",
                table: "ShopStockAdjustmentItems",
                columns: new[] { "StockAdjustmentId", "ProductId" },
                unique: true);
        }
    }
}

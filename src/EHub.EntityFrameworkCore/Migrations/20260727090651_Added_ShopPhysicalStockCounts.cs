using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class Added_ShopPhysicalStockCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopStockCounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockCountNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CountDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    ProductCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GeneratedStockAdjustmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CountedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CountedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ShopStockCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopStockCounts_ShopProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ShopProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopStockCounts_ShopStockAdjustments_GeneratedStockAdjustmentId",
                        column: x => x.GeneratedStockAdjustmentId,
                        principalTable: "ShopStockAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopStockCountItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockCountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCodeSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitShortNameSnapshot = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SystemQuantitySnapshot = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PhysicalQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DifferenceQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    AdjustmentType = table.Column<int>(type: "int", nullable: true),
                    IsCounted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CountedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CountedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopStockCountItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopStockCountItems_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopStockCountItems_ShopStockCounts_StockCountId",
                        column: x => x.StockCountId,
                        principalTable: "ShopStockCounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_ProductId",
                table: "ShopStockCountItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_StockCountId_ProductId",
                table: "ShopStockCountItems",
                columns: new[] { "StockCountId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_TenantId",
                table: "ShopStockCountItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_TenantId_ProductId",
                table: "ShopStockCountItems",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_TenantId_StockCountId",
                table: "ShopStockCountItems",
                columns: new[] { "TenantId", "StockCountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_GeneratedStockAdjustmentId",
                table: "ShopStockCounts",
                column: "GeneratedStockAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_ProductCategoryId",
                table: "ShopStockCounts",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId",
                table: "ShopStockCounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId_CountDate",
                table: "ShopStockCounts",
                columns: new[] { "TenantId", "CountDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId_GeneratedStockAdjustmentId",
                table: "ShopStockCounts",
                columns: new[] { "TenantId", "GeneratedStockAdjustmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId_ProductCategoryId",
                table: "ShopStockCounts",
                columns: new[] { "TenantId", "ProductCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId_Scope",
                table: "ShopStockCounts",
                columns: new[] { "TenantId", "Scope" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId_Status",
                table: "ShopStockCounts",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId_StockCountNumber",
                table: "ShopStockCounts",
                columns: new[] { "TenantId", "StockCountNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopStockCountItems");

            migrationBuilder.DropTable(
                name: "ShopStockCounts");
        }
    }
}

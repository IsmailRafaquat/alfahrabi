using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class Added_ShopDashboardQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShopSales_TenantId_Status_SaleDate",
                table: "ShopSales",
                columns: new[] { "TenantId", "Status", "SaleDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_ExpiryDate_Status",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "ExpiryDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_TenantId_Status_ReceiptDate",
                table: "ShopGoodsReceipts",
                columns: new[] { "TenantId", "Status", "ReceiptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_TenantId_Status_ExpenseDate",
                table: "ShopExpenses",
                columns: new[] { "TenantId", "Status", "ExpenseDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShopSales_TenantId_Status_SaleDate",
                table: "ShopSales");

            migrationBuilder.DropIndex(
                name: "IX_ShopProductBatches_TenantId_ExpiryDate_Status",
                table: "ShopProductBatches");

            migrationBuilder.DropIndex(
                name: "IX_ShopGoodsReceipts_TenantId_Status_ReceiptDate",
                table: "ShopGoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_ShopExpenses_TenantId_Status_ExpenseDate",
                table: "ShopExpenses");
        }
    }
}

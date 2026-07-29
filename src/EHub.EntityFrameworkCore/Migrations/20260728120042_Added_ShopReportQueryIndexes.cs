using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class Added_ShopReportQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPayments_TenantId_SupplierId_PaymentDate",
                table: "ShopSupplierPayments",
                columns: new[] { "TenantId", "SupplierId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId_ProductId_TransactionDate",
                table: "ShopStockTransactions",
                columns: new[] { "TenantId", "ProductId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId_TransactionType_TransactionDate",
                table: "ShopStockTransactions",
                columns: new[] { "TenantId", "TransactionType", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSales_TenantId_CustomerId_SaleDate",
                table: "ShopSales",
                columns: new[] { "TenantId", "CustomerId", "SaleDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_TenantId_SupplierId_ReceiptDate",
                table: "ShopGoodsReceipts",
                columns: new[] { "TenantId", "SupplierId", "ReceiptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_TenantId_ExpenseCategoryId_ExpenseDate",
                table: "ShopExpenses",
                columns: new[] { "TenantId", "ExpenseCategoryId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPayments_TenantId_CustomerId_PaymentDate",
                table: "ShopCustomerPayments",
                columns: new[] { "TenantId", "CustomerId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_TenantId_CashRegisterId_TransactionDate",
                table: "ShopCashRegisterTransactions",
                columns: new[] { "TenantId", "CashRegisterId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_BankAccountId_TransactionDate",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "BankAccountId", "TransactionDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShopSupplierPayments_TenantId_SupplierId_PaymentDate",
                table: "ShopSupplierPayments");

            migrationBuilder.DropIndex(
                name: "IX_ShopStockTransactions_TenantId_ProductId_TransactionDate",
                table: "ShopStockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ShopStockTransactions_TenantId_TransactionType_TransactionDate",
                table: "ShopStockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ShopSales_TenantId_CustomerId_SaleDate",
                table: "ShopSales");

            migrationBuilder.DropIndex(
                name: "IX_ShopGoodsReceipts_TenantId_SupplierId_ReceiptDate",
                table: "ShopGoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_ShopExpenses_TenantId_ExpenseCategoryId_ExpenseDate",
                table: "ShopExpenses");

            migrationBuilder.DropIndex(
                name: "IX_ShopCustomerPayments_TenantId_CustomerId_PaymentDate",
                table: "ShopCustomerPayments");

            migrationBuilder.DropIndex(
                name: "IX_ShopCashRegisterTransactions_TenantId_CashRegisterId_TransactionDate",
                table: "ShopCashRegisterTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ShopBankTransactions_TenantId_BankAccountId_TransactionDate",
                table: "ShopBankTransactions");
        }
    }
}

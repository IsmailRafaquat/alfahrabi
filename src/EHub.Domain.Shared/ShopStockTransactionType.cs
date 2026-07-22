namespace EHub.ShopManagement.StockTransactions;

public enum ShopStockTransactionType
{
    OpeningStock = 0,
    Purchase = 1,
    PurchaseReturn = 2,
    Sale = 3,
    SaleReturn = 4,
    StockAdjustmentIncrease = 5,
    StockAdjustmentDecrease = 6,
    Damaged = 7,
    Expired = 8,
    FreeSample = 9
}

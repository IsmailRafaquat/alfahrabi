namespace EHub.ShopManagement.Reports;

public enum ShopSalesReportGroupBy
{
    None = 0,
    Day = 1,
    Month = 2,
    Customer = 3,
    Product = 4,
    Category = 5,
    PaymentMethod = 6
}

public enum ShopStockReportStatus
{
    All = 0,
    InStock = 1,
    LowStock = 2,
    OutOfStock = 3,
    NegativeStock = 4
}

public enum ShopStockQuantityDirection
{
    All = 0,
    StockIn = 1,
    StockOut = 2
}

public enum ShopReportExportFormat
{
    Excel = 0,
    Pdf = 1,
    Csv = 2
}

public enum ShopExpenseReportGroupBy
{
    None = 0,
    Day = 1,
    Month = 2,
    ExpenseCategory = 3,
    PaymentSource = 4,
    User = 5
}

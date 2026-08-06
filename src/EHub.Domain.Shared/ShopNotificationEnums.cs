namespace EHub.ShopManagement.Notifications;

public enum ShopNotificationType
{
    LowStock = 0,
    OutOfStock = 1,
    NearExpiry = 2,
    ExpiredBatch = 3,
    CustomerPaymentDue = 4,
    SupplierPaymentDue = 5,
    CustomerOverdueBalance = 6,
    SupplierOverdueBalance = 7,
    CashClosingDifference = 8,
    BankLowBalance = 9,
    DraftSalePending = 10,
    DraftPurchasePending = 11,
    UnpostedExpense = 12,
    StockCountPending = 13,
    StockAdjustmentPending = 14,
    ProfitLossWarning = 15,
    SystemInformation = 16
}

public enum ShopNotificationSeverity
{
    Information = 0,
    Success = 1,
    Warning = 2,
    Critical = 3
}

public enum ShopNotificationStatus
{
    Unread = 0,
    Read = 1,
    Dismissed = 2,
    Resolved = 3
}

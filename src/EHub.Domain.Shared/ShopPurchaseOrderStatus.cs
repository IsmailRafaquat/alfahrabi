namespace EHub.ShopManagement.PurchaseOrders;

public enum ShopPurchaseOrderStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    PartiallyReceived = 4,
    FullyReceived = 5,
    Cancelled = 6,
    Closed = 7
}

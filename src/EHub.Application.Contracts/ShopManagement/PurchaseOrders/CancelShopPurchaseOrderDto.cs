using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.PurchaseOrders;

public class CancelShopPurchaseOrderDto
{
    [Required, StringLength(ShopPurchaseOrderConsts.CancellationReasonMaxLength)]
    public string CancellationReason { get; set; } = string.Empty;
}

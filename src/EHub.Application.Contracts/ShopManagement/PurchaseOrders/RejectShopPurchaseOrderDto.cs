using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.PurchaseOrders;

public class RejectShopPurchaseOrderDto
{
    [Required, StringLength(ShopPurchaseOrderConsts.RejectionReasonMaxLength)]
    public string RejectionReason { get; set; } = string.Empty;
}

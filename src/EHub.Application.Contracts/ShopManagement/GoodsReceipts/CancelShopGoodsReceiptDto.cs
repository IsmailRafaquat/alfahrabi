using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.GoodsReceipts;

public class CancelShopGoodsReceiptDto
{
    [Required, StringLength(ShopGoodsReceiptConsts.CancellationReasonMaxLength)]
    public string CancellationReason { get; set; } = string.Empty;
}

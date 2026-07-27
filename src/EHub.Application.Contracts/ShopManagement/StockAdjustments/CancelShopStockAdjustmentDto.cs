using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.StockAdjustments;

public class CancelShopStockAdjustmentDto
{
    [Required, StringLength(ShopStockAdjustmentConsts.CancellationReasonMaxLength)]
    public string CancellationReason { get; set; } = string.Empty;
}

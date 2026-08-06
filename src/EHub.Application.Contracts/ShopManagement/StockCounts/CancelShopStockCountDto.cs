using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.StockCounts;

public class CancelShopStockCountDto
{
    [Required, StringLength(ShopStockCountConsts.CancellationReasonMaxLength)]
    public string CancellationReason { get; set; } = string.Empty;
}

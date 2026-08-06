using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.Sales;

public class CancelShopSaleDto
{
    [Required, StringLength(ShopSaleConsts.CancellationReasonMaxLength)]
    public string CancellationReason { get; set; } = string.Empty;
}

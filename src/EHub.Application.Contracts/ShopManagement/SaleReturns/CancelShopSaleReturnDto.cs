using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.SaleReturns;

public class CancelShopSaleReturnDto
{
    [Required]
    public string CancellationReason { get; set; } = string.Empty;
}

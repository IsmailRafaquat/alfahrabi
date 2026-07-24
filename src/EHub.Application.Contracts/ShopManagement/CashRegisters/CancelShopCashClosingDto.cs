using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.CashRegisters;

public class CancelShopCashClosingDto
{
    [Required]
    public string CancellationReason { get; set; } = string.Empty;
}

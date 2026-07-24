using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.CashRegisters;

public class CloseShopCashRegisterDto
{
    // Cannot be negative; enforced by ShopCashClosing as a localized BusinessException.
    public decimal ActualClosingCash { get; set; }

    [StringLength(ShopCashRegisterConsts.NotesMaxLength)]
    public string? Notes { get; set; }
}

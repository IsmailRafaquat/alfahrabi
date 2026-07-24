using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.CashRegisters;

public class OpenShopCashRegisterDto
{
    [Required]
    public DateTime BusinessDate { get; set; }

    // Cannot be negative; enforced by ShopCashClosing as a localized BusinessException.
    public decimal OpeningCash { get; set; }

    [StringLength(ShopCashRegisterConsts.NotesMaxLength)]
    public string? Notes { get; set; }
}

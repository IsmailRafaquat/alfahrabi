using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.CashRegisters;

public class CreateManualCashMovementDto
{
    [Required]
    public Guid CashRegisterId { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; }

    [Required]
    public ShopCashDirection Direction { get; set; }

    // Must be > 0; enforced by ShopCashRegisterManager as a localized BusinessException.
    public decimal Amount { get; set; }

    [StringLength(ShopCashRegisterConsts.ReferenceNumberMaxLength)]
    public string? ReferenceNumber { get; set; }

    [StringLength(ShopCashRegisterConsts.NotesMaxLength)]
    public string? Description { get; set; }
}

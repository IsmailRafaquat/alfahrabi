using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.BankAccounts;

public class CreateUpdateShopBankTransferDto
{
    [Required]
    public DateTime TransferDate { get; set; }

    [Required]
    public ShopBankTransferType TransferType { get; set; }

    public Guid? FromBankAccountId { get; set; }
    public Guid? ToBankAccountId { get; set; }
    public Guid? CashRegisterId { get; set; }

    // Must be > 0; enforced by ShopBankTransfer as a localized BusinessException.
    public decimal Amount { get; set; }

    [StringLength(ShopBankAccountConsts.ReferenceNumberMaxLength)]
    public string? ReferenceNumber { get; set; }

    [StringLength(ShopBankAccountConsts.NotesMaxLength)]
    public string? Notes { get; set; }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.BankAccounts;

public class CreateManualBankMovementDto
{
    [Required]
    public Guid BankAccountId { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; }

    [Required]
    public ShopBankDirection Direction { get; set; }

    // Must be > 0; enforced by ShopBankAccountManager as a localized BusinessException.
    public decimal Amount { get; set; }

    [StringLength(ShopBankAccountConsts.ReferenceNumberMaxLength)]
    public string? ReferenceNumber { get; set; }

    [StringLength(ShopBankAccountConsts.DescriptionMaxLength)]
    public string? Description { get; set; }
}

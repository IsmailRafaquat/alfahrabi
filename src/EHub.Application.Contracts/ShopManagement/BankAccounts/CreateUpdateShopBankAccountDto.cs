using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.BankAccounts;

public class CreateUpdateShopBankAccountDto
{
    [Required, StringLength(ShopBankAccountConsts.CodeMaxLength)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(ShopBankAccountConsts.AccountNameMaxLength)]
    public string AccountName { get; set; } = string.Empty;

    [Required, StringLength(ShopBankAccountConsts.BankNameMaxLength)]
    public string BankName { get; set; } = string.Empty;

    [StringLength(ShopBankAccountConsts.AccountNumberMaxLength)]
    public string? AccountNumber { get; set; }

    [StringLength(ShopBankAccountConsts.IbanMaxLength)]
    public string? IBAN { get; set; }

    [StringLength(ShopBankAccountConsts.BranchNameMaxLength)]
    public string? BranchName { get; set; }

    // Cannot be negative; enforced by ShopBankAccount as a localized BusinessException.
    public decimal OpeningBalance { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    [StringLength(ShopBankAccountConsts.NotesMaxLength)]
    public string? Notes { get; set; }
}

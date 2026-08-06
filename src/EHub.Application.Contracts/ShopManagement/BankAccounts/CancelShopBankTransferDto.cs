using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.BankAccounts;

public class CancelShopBankTransferDto
{
    [Required, StringLength(ShopBankAccountConsts.CancellationReasonMaxLength)]
    public string CancellationReason { get; set; } = string.Empty;
}

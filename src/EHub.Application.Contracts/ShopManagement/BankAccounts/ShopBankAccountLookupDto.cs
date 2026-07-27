using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.BankAccounts;

public class ShopBankAccountLookupDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

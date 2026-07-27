using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.BankAccounts;

public class ShopBankAccountDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? IBAN { get; set; }
    public string? BranchName { get; set; }

    public decimal? OpeningBalance { get; set; }
    public decimal? CurrentBalance { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    public DateTime CreationTime { get; set; }
}

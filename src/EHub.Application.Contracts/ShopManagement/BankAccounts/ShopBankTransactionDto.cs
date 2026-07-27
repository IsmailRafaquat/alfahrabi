using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.BankAccounts;

public class ShopBankTransactionDto : EntityDto<Guid>
{
    public Guid BankAccountId { get; set; }
    public string BankAccountCode { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }
    public ShopBankTransactionType TransactionType { get; set; }
    public ShopBankDirection Direction { get; set; }
    public decimal? Amount { get; set; }

    public ShopBankReferenceType ReferenceType { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal? BalanceAfterTransaction { get; set; }
    public bool IsReversal { get; set; }

    public DateTime CreationTime { get; set; }
}

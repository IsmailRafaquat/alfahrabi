using System;
using System.Collections.Generic;
using EHub.ShopManagement.BankAccounts;

namespace EHub.ShopManagement.Reports;

public class GetShopBankTransactionReportInput : ShopReportInputBase
{
    public Guid? BankAccountId { get; set; }
    public ShopBankTransactionType? TransactionType { get; set; }
    public ShopBankReferenceType? ReferenceType { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
}

public class ShopBankTransactionReportItemDto
{
    public Guid BankTransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid BankAccountId { get; set; }
    public string BankAccountName { get; set; } = string.Empty;
    public string AccountNumberMasked { get; set; } = string.Empty;
    public ShopBankTransactionType TransactionType { get; set; }
    public ShopBankReferenceType ReferenceType { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal AmountIn { get; set; }
    public decimal AmountOut { get; set; }
    public decimal BalanceAfterTransaction { get; set; }
    public string? Description { get; set; }
    public DateTime CreationTime { get; set; }
}

public class ShopBankTransactionReportTotalsDto
{
    public decimal OpeningBalance { get; set; }
    public decimal TotalAmountIn { get; set; }
    public decimal TotalAmountOut { get; set; }
    public decimal ClosingBalance { get; set; }
    public int TransactionCount { get; set; }
}

public class ShopBankTransactionReportResultDto
{
    public List<ShopBankTransactionReportItemDto> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public ShopBankTransactionReportTotalsDto Totals { get; set; } = new();
}

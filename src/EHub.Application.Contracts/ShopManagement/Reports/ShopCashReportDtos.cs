using System;
using System.Collections.Generic;
using EHub.ShopManagement.CashRegisters;

namespace EHub.ShopManagement.Reports;

public class GetShopCashReportInput : ShopReportInputBase
{
    public Guid? CashRegisterId { get; set; }
    public ShopCashTransactionType? TransactionType { get; set; }
    public ShopCashReferenceType? ReferenceType { get; set; }
}

public class ShopCashReportItemDto
{
    public Guid CashTransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid CashRegisterId { get; set; }
    public string CashRegisterName { get; set; } = string.Empty;
    public ShopCashTransactionType TransactionType { get; set; }
    public ShopCashReferenceType ReferenceType { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal CashIn { get; set; }
    public decimal CashOut { get; set; }
    public decimal BalanceAfterTransaction { get; set; }
    public string? Description { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreationTime { get; set; }
}

public class ShopCashDailySummaryDto
{
    public DateTime BusinessDate { get; set; }
    public Guid CashRegisterId { get; set; }
    public string CashRegisterName { get; set; } = string.Empty;
    public decimal OpeningCash { get; set; }
    public decimal TotalCashIn { get; set; }
    public decimal TotalCashOut { get; set; }
    public decimal ExpectedClosingCash { get; set; }
    public decimal? ActualClosingCash { get; set; }
    public decimal? Difference { get; set; }
}

public class ShopCashReportTotalsDto
{
    public decimal OpeningCash { get; set; }
    public decimal TotalCashIn { get; set; }
    public decimal TotalCashOut { get; set; }
    public decimal ExpectedClosingCash { get; set; }
    public decimal? ActualClosingCash { get; set; }
    public decimal? Difference { get; set; }
}

public class ShopCashReportResultDto
{
    public List<ShopCashReportItemDto> Items { get; set; } = new();
    public List<ShopCashDailySummaryDto> DailySummaries { get; set; } = new();
    public long TotalCount { get; set; }
    public ShopCashReportTotalsDto Totals { get; set; } = new();
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EHub.ShopManagement.Reports;

namespace EHub.ShopManagement.ProfitLoss;

public class GetShopProfitLossInput
{
    public ShopReportPeriod Period { get; set; } = ShopReportPeriod.ThisMonth;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool CompareWithPreviousPeriod { get; set; } = true;
    public bool IncludeExpenseBreakdown { get; set; } = true;
    public bool IncludeProductContribution { get; set; } = true;

    [Range(1, 50)]
    public int TopProductCount { get; set; } = 10;
}

public class ShopProfitLossRevenueDto
{
    public decimal GrossSales { get; set; }
    public decimal SalesDiscounts { get; set; }
    public decimal SalesTax { get; set; }
    public decimal SalesReturns { get; set; }
    public decimal NetSales { get; set; }
}

public class ShopProfitLossCostDto
{
    public decimal CostOfGoodsSold { get; set; }
    public decimal OpeningInventoryValue { get; set; }
    public decimal NetPurchases { get; set; }
    public decimal ClosingInventoryValue { get; set; }
}

public class ShopProfitLossExpenseDto
{
    public decimal OperatingExpenses { get; set; }
    public decimal OtherIncome { get; set; }
}

public class ShopProfitLossMarginDto
{
    public decimal? GrossProfitMarginPercentage { get; set; }
    public decimal? NetProfitMarginPercentage { get; set; }
}

public class ShopProfitLossSummaryDto
{
    public decimal GrossSales { get; set; }
    public decimal SalesDiscounts { get; set; }
    public decimal SalesTax { get; set; }
    public decimal SalesReturns { get; set; }
    public decimal NetSales { get; set; }

    /// <summary>COGS from Sale stock-out activity only, before any return reversal.</summary>
    public decimal? CostOfGoodsSoldBeforeReturns { get; set; }
    /// <summary>COGS reversed by posted Sale Return activity (sourced from the original Sale
    /// item's cost snapshot, never the product's current/average cost).</summary>
    public decimal? ReturnedCostOfGoodsSold { get; set; }
    /// <summary>Net COGS = CostOfGoodsSoldBeforeReturns - ReturnedCostOfGoodsSold. Authoritative
    /// figure GrossProfit is computed from.</summary>
    public decimal? CostOfGoodsSold { get; set; }
    public decimal? GrossProfit { get; set; }
    public decimal? GrossProfitMarginPercentage { get; set; }

    public decimal? OperatingExpenses { get; set; }
    public decimal OtherIncome { get; set; }
    public decimal? NetProfit { get; set; }
    public decimal? NetProfitMarginPercentage { get; set; }
    public ShopProfitLossResultStatus ResultStatus { get; set; }

    public decimal? OpeningInventoryValue { get; set; }
    public decimal? NetPurchases { get; set; }
    public decimal? ClosingInventoryValue { get; set; }

    public DateTime CurrentPeriodFrom { get; set; }
    public DateTime CurrentPeriodTo { get; set; }
    public string CurrencyCode { get; set; } = "PKR";
    public string CurrencySymbol { get; set; } = "₨";
}

public class ShopProfitLossTrendPointDto
{
    public string Label { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public decimal NetSales { get; set; }
    public decimal? CostOfGoodsSold { get; set; }
    public decimal? GrossProfit { get; set; }
    public decimal? OperatingExpenses { get; set; }
    public decimal? NetProfit { get; set; }
}

public class ShopProfitLossExpenseCategoryDto
{
    public Guid ExpenseCategoryId { get; set; }
    public string ExpenseCategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PercentageOfTotalExpenses { get; set; }
    public int TransactionCount { get; set; }
}

public class ShopProfitLossProductContributionDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public decimal NetSales { get; set; }
    public decimal? CostOfGoodsSold { get; set; }
    public decimal? GrossProfit { get; set; }
    public decimal? GrossMarginPercentage { get; set; }
}

public class ShopProfitLossComparisonDto
{
    public decimal CurrentNetSales { get; set; }
    public decimal PreviousNetSales { get; set; }
    public decimal? CurrentGrossProfit { get; set; }
    public decimal? PreviousGrossProfit { get; set; }
    public decimal? CurrentNetProfit { get; set; }
    public decimal? PreviousNetProfit { get; set; }
    public decimal? NetProfitChangeAmount { get; set; }
    public decimal? NetProfitChangePercentage { get; set; }
}

public class ShopProfitLossDto
{
    public ShopProfitLossSummaryDto Summary { get; set; } = new();
    public List<ShopProfitLossTrendPointDto> Trend { get; set; } = new();
    public List<ShopProfitLossExpenseCategoryDto>? ExpenseBreakdown { get; set; }
    public List<ShopProfitLossProductContributionDto>? ProductContribution { get; set; }
    public ShopProfitLossComparisonDto? Comparison { get; set; }
}

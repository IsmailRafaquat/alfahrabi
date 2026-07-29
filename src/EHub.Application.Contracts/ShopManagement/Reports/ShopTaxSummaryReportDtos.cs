using System;

namespace EHub.ShopManagement.Reports;

public class GetShopTaxSummaryReportInput
{
    public ShopReportPeriod Period { get; set; } = ShopReportPeriod.ThisMonth;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class ShopTaxSummaryLineDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class ShopTaxSummaryReportDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    public decimal SalesTaxCollected { get; set; }
    public decimal SalesReturnTaxReversed { get; set; }
    public decimal NetSalesTax { get; set; }

    public decimal PurchaseTaxPaid { get; set; }
    public decimal PurchaseReturnTaxReversed { get; set; }
    public decimal NetPurchaseTax { get; set; }

    /// <summary>Always 0 - ShopExpense has no tax field in the current schema; kept for shape completeness only.</summary>
    public decimal ExpenseTaxPaid { get; set; }

    public decimal NetTaxPosition { get; set; }
}

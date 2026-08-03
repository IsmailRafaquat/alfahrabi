using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Reports;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Content;

namespace EHub.ShopManagement.ProfitLoss;

public partial class ShopProfitLossAppService
{
    [Authorize(EHubPermissions.ShopProfitLoss.Export)]
    public async Task<IRemoteStreamContent> ExportAsync(GetShopProfitLossInput input, ShopReportExportFormat format)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        var core = await _calculator.ComputeAsync(tenantId, range.From, range.ToExclusive);
        var summary = await BuildSummaryDtoAsync(tenantId, core, range);
        var setting = await GetSettingAsync(tenantId);

        var canRevenue = await CanAsync(EHubPermissions.ShopProfitLoss.ViewRevenue);
        var canCost = await CanAsync(EHubPermissions.ShopProfitLoss.ViewCost);
        var canExpenses = await CanAsync(EHubPermissions.ShopProfitLoss.ViewExpenses);
        var canMargins = await CanAsync(EHubPermissions.ShopProfitLoss.ViewMargins);

        var rows = new List<Dictionary<string, object?>>();
        void AddRow(string label, string? amount) => rows.Add(new Dictionary<string, object?> { ["label"] = label, ["amount"] = amount });

        if (canRevenue)
        {
            AddRow("Gross Sales", summary.GrossSales.ToString("N2"));
            AddRow("Less: Sales Discounts", $"({summary.SalesDiscounts:N2})");
            AddRow("Less: Sale Returns", $"({summary.SalesReturns:N2})");
            AddRow("Net Sales", summary.NetSales.ToString("N2"));
            AddRow("Sales Tax (informational, not included in revenue)", summary.SalesTax.ToString("N2"));
        }

        if (canCost)
        {
            AddRow("Cost of Goods Sold", summary.CostOfGoodsSold?.ToString("N2"));
            AddRow("Gross Profit", summary.GrossProfit?.ToString("N2"));
        }

        if (canExpenses)
        {
            AddRow("Operating Expenses", summary.OperatingExpenses?.ToString("N2"));
            AddRow("Other Income", summary.OtherIncome.ToString("N2"));
        }

        if (canCost && canExpenses)
        {
            AddRow("Net Profit / (Loss)", summary.NetProfit?.ToString("N2"));
            AddRow("Result Status", summary.ResultStatus.ToString());
        }

        if (canMargins)
        {
            AddRow("Gross Profit Margin %", summary.GrossProfitMarginPercentage?.ToString("N2") ?? "N/A");
            AddRow("Net Profit Margin %", summary.NetProfitMarginPercentage?.ToString("N2") ?? "N/A");
        }

        if (canCost)
        {
            AddRow("Opening Inventory Value (approx.)", summary.OpeningInventoryValue?.ToString("N2"));
            AddRow("Net Purchases", summary.NetPurchases?.ToString("N2"));
            AddRow("Closing Inventory Value (approx.)", summary.ClosingInventoryValue?.ToString("N2"));
        }

        List<(string, string)> totalsLines = new();
        if (canCost && canExpenses)
        {
            totalsLines.Add(("Net Profit / (Loss)", summary.NetProfit?.ToString("N2") ?? "N/A"));
            totalsLines.Add(("Result", summary.ResultStatus.ToString()));
        }

        var request = new ShopReportExportRequest
        {
            ReportTitle = "Profit and Loss Statement",
            ShopName = setting?.ShopDisplayName,
            ShopAddress = CombineAddress(setting),
            ShopPhone = setting?.Phone,
            DateFrom = range.From,
            DateTo = range.ToExclusive.AddDays(-1),
            GeneratedDate = Clock.Now,
            Columns = new List<ShopReportExportColumn> { new("Line", "label"), new("Amount", "amount") },
            Rows = rows,
            TotalsLines = totalsLines,
        };

        return await _exportService.ExportAsync(request, format);
    }

    private static string? CombineAddress(EHub.ShopManagement.Settings.ShopSetting? setting)
    {
        if (setting == null) return null;
        var parts = new[] { setting.AddressLine1, setting.AddressLine2, setting.City, setting.StateOrProvince, setting.PostalCode, setting.Country };
        var filtered = new List<string>();
        foreach (var p in parts) if (!string.IsNullOrWhiteSpace(p)) filtered.Add(p!);
        return filtered.Count > 0 ? string.Join(", ", filtered) : null;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.ProfitLoss;

public partial class ShopProfitLossAppService
{
    public async Task<ListResultDto<ShopProfitLossProductContributionDto>> GetProductContributionAsync(GetShopProfitLossInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        var computation = await _calculator.ComputeDetailedAsync(tenantId, range.From, range.ToExclusive);
        var items = await BuildProductContributionAsync(computation, input.TopProductCount);
        return new ListResultDto<ShopProfitLossProductContributionDto>(items);
    }

    /// <summary>Groups the already-loaded SaleItems/SaleReturnItems for this period in memory by ProductId - only the product name/code lookup needs a query.</summary>
    private async Task<List<ShopProfitLossProductContributionDto>> BuildProductContributionAsync(ShopProfitLossComputation computation, int topProductCount)
    {
        var canCost = await CanAsync(EHubPermissions.ShopProfitLoss.ViewCost);

        var saleAgg = computation.SaleItems
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity),
                NetSales = g.Sum(x => x.LineSubTotal - x.DiscountAmount),
                Cost = g.Sum(x => x.Quantity * x.UnitCostSnapshot),
            })
            .ToList();

        var returnAgg = computation.SaleReturnItems
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.ReturnQuantity),
                NetSales = g.Sum(x => x.LineSubTotal - x.DiscountAmount),
                Cost = g.Sum(x => x.ReturnQuantity * x.UnitCostSnapshot),
            })
            .ToList();

        var returnByProduct = returnAgg.ToDictionary(x => x.ProductId);

        var productIds = saleAgg.Select(x => x.ProductId).Union(returnAgg.Select(x => x.ProductId)).Distinct().ToList();
        if (productIds.Count == 0) return new List<ShopProfitLossProductContributionDto>();

        var products = (await ProductRepository.GetListAsync(x => productIds.Contains(x.Id)))
            .ToDictionary(x => x.Id, x => new { x.Code, x.Name });

        var results = new List<ShopProfitLossProductContributionDto>();
        foreach (var sale in saleAgg)
        {
            returnByProduct.TryGetValue(sale.ProductId, out var ret);
            products.TryGetValue(sale.ProductId, out var product);

            var quantitySold = Math.Round(sale.Quantity - (ret?.Quantity ?? 0), 2);
            var netSales = Math.Round(sale.NetSales - (ret?.NetSales ?? 0), 2);

            var dto = new ShopProfitLossProductContributionDto
            {
                ProductId = sale.ProductId,
                ProductCode = product?.Code ?? string.Empty,
                ProductName = product?.Name ?? string.Empty,
                QuantitySold = quantitySold,
                NetSales = netSales,
            };

            if (canCost)
            {
                var cogs = Math.Round(sale.Cost - (ret?.Cost ?? 0), 2);
                dto.CostOfGoodsSold = cogs;
                var grossProfit = Math.Round(netSales - cogs, 2);
                dto.GrossProfit = grossProfit;
                dto.GrossMarginPercentage = netSales > 0 ? Math.Round(grossProfit / netSales * 100, 2) : null;
            }

            results.Add(dto);
        }

        return results
            .OrderByDescending(x => x.GrossProfit ?? x.NetSales)
            .Take(topProductCount)
            .ToList();
    }
}

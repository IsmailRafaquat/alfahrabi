using System;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.ExpenseCategories;
using EHub.ShopManagement.Expenses;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.Reports;
using EHub.ShopManagement.SaleReturns;
using EHub.ShopManagement.Sales;
using EHub.ShopManagement.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using System.Linq;

namespace EHub.ShopManagement.ProfitLoss;

/// <summary>
/// Read-only Profit and Loss statement calculated from posted Sales, Sale Returns, Sale Item cost
/// snapshots, and posted Expenses. Never creates, updates, or persists any totals - every method is
/// a projection/aggregation over <see cref="ShopProfitLossCalculator"/>. Section-level permissions
/// (Revenue/Cost/Expenses/Margins/ProductContribution) are enforced here, server-side, by nulling
/// out the corresponding DTO sections - never left to the frontend to hide.
/// </summary>
[Authorize(EHubPermissions.ShopProfitLoss.View)]
public partial class ShopProfitLossAppService : ApplicationService, IShopProfitLossAppService
{
    private readonly ShopProfitLossCalculator _calculator;
    private readonly IRepository<ShopSetting, Guid> _settingRepository;
    private readonly IShopReportDateRangeResolver _dateRangeResolver;
    private readonly IShopReportExportService _exportService;

    protected readonly IRepository<ShopSale, Guid> SaleRepository;
    protected readonly IRepository<ShopSaleItem, Guid> SaleItemRepository;
    protected readonly IRepository<ShopSaleReturn, Guid> SaleReturnRepository;
    protected readonly IRepository<ShopSaleReturnItem, Guid> SaleReturnItemRepository;
    protected readonly IRepository<ShopExpense, Guid> ExpenseRepository;
    protected readonly IRepository<ShopExpenseCategory, Guid> ExpenseCategoryRepository;
    protected readonly IRepository<ShopProduct, Guid> ProductRepository;
    protected readonly IRepository<ShopProductCategory, Guid> ProductCategoryRepository;

    public ShopProfitLossAppService(
        ShopProfitLossCalculator calculator,
        IRepository<ShopSetting, Guid> settingRepository,
        IShopReportDateRangeResolver dateRangeResolver,
        IShopReportExportService exportService,
        IRepository<ShopSale, Guid> saleRepository,
        IRepository<ShopSaleItem, Guid> saleItemRepository,
        IRepository<ShopSaleReturn, Guid> saleReturnRepository,
        IRepository<ShopSaleReturnItem, Guid> saleReturnItemRepository,
        IRepository<ShopExpense, Guid> expenseRepository,
        IRepository<ShopExpenseCategory, Guid> expenseCategoryRepository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopProductCategory, Guid> productCategoryRepository)
    {
        _calculator = calculator;
        _settingRepository = settingRepository;
        _dateRangeResolver = dateRangeResolver;
        _exportService = exportService;
        SaleRepository = saleRepository;
        SaleItemRepository = saleItemRepository;
        SaleReturnRepository = saleReturnRepository;
        SaleReturnItemRepository = saleReturnItemRepository;
        ExpenseRepository = expenseRepository;
        ExpenseCategoryRepository = expenseCategoryRepository;
        ProductRepository = productRepository;
        ProductCategoryRepository = productCategoryRepository;
    }

    public async Task<ShopProfitLossDto> GetAsync(GetShopProfitLossInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        var closingInventoryValue = await _calculator.ComputeClosingInventoryValueAsync(tenantId);

        // One load of this period's sales/items/returns/return-items/expenses, reused below by the
        // trend, expense-breakdown, and product-contribution sections instead of each re-querying
        // the same rows for the same period.
        var computation = await _calculator.ComputeDetailedAsync(tenantId, range.From, range.ToExclusive, closingInventoryValue);
        var core = computation.Core;

        var dto = new ShopProfitLossDto
        {
            Summary = await BuildSummaryDtoAsync(tenantId, core, range),
            Trend = await BuildTrendAsync(tenantId, input.Period, range, computation),
        };

        if (input.IncludeExpenseBreakdown && await CanAsync(EHubPermissions.ShopProfitLoss.ViewExpenses))
        {
            dto.ExpenseBreakdown = await BuildExpenseBreakdownAsync(computation);
        }

        if (input.IncludeProductContribution && await CanAsync(EHubPermissions.ShopProfitLoss.ViewProductContribution))
        {
            dto.ProductContribution = await BuildProductContributionAsync(computation, input.TopProductCount);
        }

        if (input.CompareWithPreviousPeriod)
        {
            // Reuse the current period's already-computed core (and the inventory valuation we just
            // did once above) instead of recomputing "current" from scratch inside the comparison.
            dto.Comparison = await BuildComparisonAsync(tenantId, input.Period, range, core, closingInventoryValue);
        }

        return dto;
    }

    public async Task<ShopProfitLossSummaryDto> GetSummaryAsync(GetShopProfitLossInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        var core = await _calculator.ComputeAsync(tenantId, range.From, range.ToExclusive);
        return await BuildSummaryDtoAsync(tenantId, core, range);
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:ReportTenantRequired");

    private async Task<bool> CanAsync(string permission) => await AuthorizationService.IsGrantedAsync(permission);

    private async Task<ShopSetting?> GetSettingAsync(Guid tenantId)
    {
        var query = (await _settingRepository.GetQueryableAsync()).AsNoTracking();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.TenantId == tenantId));
    }

    private async Task<ShopProfitLossSummaryDto> BuildSummaryDtoAsync(Guid tenantId, ShopProfitLossCoreResult core, ShopReportDateRange range)
    {
        var setting = await GetSettingAsync(tenantId);

        var dto = new ShopProfitLossSummaryDto
        {
            CurrentPeriodFrom = range.From,
            CurrentPeriodTo = range.ToExclusive.AddDays(-1),
            CurrencyCode = setting?.CurrencyCode ?? "PKR",
            CurrencySymbol = setting?.CurrencySymbol ?? "₨",
            ResultStatus = core.ResultStatus,
        };

        if (await CanAsync(EHubPermissions.ShopProfitLoss.ViewRevenue))
        {
            dto.GrossSales = core.GrossSales;
            dto.SalesDiscounts = core.SalesDiscounts;
            dto.SalesTax = core.SalesTax;
            dto.SalesReturns = core.SalesReturns;
            dto.NetSales = core.NetSales;
        }

        if (await CanAsync(EHubPermissions.ShopProfitLoss.ViewCost))
        {
            dto.CostOfGoodsSold = core.CostOfGoodsSold;
            dto.GrossProfit = core.GrossProfit;
            dto.OpeningInventoryValue = core.OpeningInventoryValue;
            dto.NetPurchases = core.NetPurchases;
            dto.ClosingInventoryValue = core.ClosingInventoryValue;
        }

        if (await CanAsync(EHubPermissions.ShopProfitLoss.ViewExpenses))
        {
            dto.OperatingExpenses = core.OperatingExpenses;
            dto.OtherIncome = core.OtherIncome;
        }

        if (dto.CostOfGoodsSold.HasValue && dto.OperatingExpenses.HasValue)
        {
            dto.NetProfit = core.NetProfit;
        }

        if (await CanAsync(EHubPermissions.ShopProfitLoss.ViewMargins))
        {
            dto.GrossProfitMarginPercentage = core.NetSales > 0 ? Math.Round(core.GrossProfit / core.NetSales * 100, 2) : (decimal?)null;
            dto.NetProfitMarginPercentage = core.NetSales > 0 ? Math.Round(core.NetProfit / core.NetSales * 100, 2) : (decimal?)null;
        }

        return dto;
    }
}

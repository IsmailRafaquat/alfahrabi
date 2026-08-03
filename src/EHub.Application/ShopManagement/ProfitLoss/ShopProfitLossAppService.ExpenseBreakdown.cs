using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.ProfitLoss;

public partial class ShopProfitLossAppService
{
    public async Task<ListResultDto<ShopProfitLossExpenseCategoryDto>> GetExpenseBreakdownAsync(GetShopProfitLossInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        var computation = await _calculator.ComputeDetailedAsync(tenantId, range.From, range.ToExclusive);
        var breakdown = await BuildExpenseBreakdownAsync(computation);
        return new ListResultDto<ShopProfitLossExpenseCategoryDto>(breakdown);
    }

    /// <summary>Groups the already-loaded Expenses for this period (see ShopProfitLossCalculator.ComputeDetailedAsync) in memory - only the category-name lookup needs a query.</summary>
    private async Task<List<ShopProfitLossExpenseCategoryDto>> BuildExpenseBreakdownAsync(ShopProfitLossComputation computation)
    {
        var grouped = computation.Expenses
            .GroupBy(x => x.ExpenseCategoryId)
            .Select(g => new { ExpenseCategoryId = g.Key, Amount = g.Sum(x => x.Amount), Count = g.Count() })
            .ToList();

        if (grouped.Count == 0) return new List<ShopProfitLossExpenseCategoryDto>();

        var categoryIds = grouped.Select(x => x.ExpenseCategoryId).Distinct().ToList();
        var categories = await ExpenseCategoryRepository.GetListAsync(x => categoryIds.Contains(x.Id));
        var categoryNames = categories.ToDictionary(x => x.Id, x => x.Name);

        var totalExpenses = grouped.Sum(x => x.Amount);

        return grouped
            .Select(g => new ShopProfitLossExpenseCategoryDto
            {
                ExpenseCategoryId = g.ExpenseCategoryId,
                ExpenseCategoryName = categoryNames.GetValueOrDefault(g.ExpenseCategoryId, "Unknown"),
                Amount = g.Amount,
                PercentageOfTotalExpenses = totalExpenses > 0 ? Math.Round(g.Amount / totalExpenses * 100, 2) : 0,
                TransactionCount = g.Count,
            })
            .OrderByDescending(x => x.Amount)
            .ToList();
    }
}

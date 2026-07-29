using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.CashRegisters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Content;

namespace EHub.ShopManagement.Reports;

public partial class ShopReportAppService
{
    // ------------------------------------------------------------------
    // 10. Expense Report
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.Expenses)]
    public async Task<ShopExpenseReportResultDto> GetExpenseReportAsync(GetShopExpenseReportInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);

        var filtered = await BuildExpenseReportQueryAsync(tenantId, range, input);
        var totals = await ComputeExpenseTotalsAsync(filtered);

        var sorting = ApplySorting(input.Sorting, "ExpenseDate desc, CreationTime desc");
        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var paged = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = await ProjectExpenseItemsAsync(paged);
        var result = new ShopExpenseReportResultDto { Items = items, TotalCount = totalCount, Totals = totals };

        if (input.GroupBy != ShopExpenseReportGroupBy.None)
        {
            result.Groups = await BuildExpenseGroupsAsync(filtered, input.GroupBy);
        }

        return result;
    }

    [Authorize(EHubPermissions.ShopReports.Expenses), Authorize(EHubPermissions.ShopReports.Export)]
    public async Task<IRemoteStreamContent> ExportExpenseReportAsync(GetShopExpenseReportInput input, ShopReportExportFormat format)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        var filtered = await BuildExpenseReportQueryAsync(tenantId, range, input);
        var totals = await ComputeExpenseTotalsAsync(filtered);

        var count = await AsyncExecuter.CountAsync(filtered);
        EnsureWithinExportLimit(count);

        var all = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, ApplySorting(input.Sorting, "ExpenseDate desc")));
        var items = await ProjectExpenseItemsAsync(all);

        var setting = await GetSettingAsync(tenantId);
        var request = new ShopReportExportRequest
        {
            ReportTitle = "Expense Report",
            ShopName = setting?.ShopDisplayName,
            ShopAddress = CombineAddress(setting),
            ShopPhone = setting?.Phone,
            DateFrom = range.From,
            DateTo = range.ToExclusive.AddDays(-1),
            GeneratedDate = Clock.Now,
            Columns = new List<ShopReportExportColumn>
            {
                new("Date", "expenseDate"), new("Number", "expenseNumber"), new("Category", "expenseCategoryName"),
                new("Description", "description"), new("Amount", "amount"), new("Total", "totalAmount"),
                new("Payment Source", "paymentSource"), new("Reference", "referenceNumber"), new("Status", "status"),
            },
            Rows = items.Select(x => new Dictionary<string, object?>
            {
                ["expenseDate"] = x.ExpenseDate, ["expenseNumber"] = x.ExpenseNumber, ["expenseCategoryName"] = x.ExpenseCategoryName,
                ["description"] = x.Description, ["amount"] = x.Amount, ["totalAmount"] = x.TotalAmount,
                ["paymentSource"] = x.PaymentSource.ToString(), ["referenceNumber"] = x.ReferenceNumber, ["status"] = x.Status.ToString(),
            }).ToList(),
            TotalsLines = new List<(string, string)>
            {
                ("Expense Count", totals.ExpenseCount.ToString()), ("Total Amount", totals.TotalExpenseAmount.ToString("N2")),
                ("Cash", totals.CashExpenses.ToString("N2")), ("Bank", totals.BankExpenses.ToString("N2")),
            },
        };

        return await _exportService.ExportAsync(request, format);
    }

    private async Task<IQueryable<Expenses.ShopExpense>> BuildExpenseReportQueryAsync(Guid tenantId, ShopReportDateRange range, GetShopExpenseReportInput input)
    {
        var query = (await _expenseRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ExpenseDate >= range.From && x.ExpenseDate < range.ToExclusive);

        query = input.ExpenseStatus.HasValue
            ? query.Where(x => x.Status == input.ExpenseStatus.Value)
            : query.Where(x => x.Status == Expenses.ShopExpenseStatus.Posted);

        if (input.ExpenseCategoryId.HasValue) query = query.Where(x => x.ExpenseCategoryId == input.ExpenseCategoryId.Value);
        if (input.PaymentSource.HasValue) query = query.Where(x => x.PaymentMethod == input.PaymentSource.Value);
        if (input.BankAccountId.HasValue) query = query.Where(x => x.BankAccountId == input.BankAccountId.Value);
        if (input.MinimumAmount.HasValue) query = query.Where(x => x.Amount >= input.MinimumAmount.Value);
        if (input.MaximumAmount.HasValue) query = query.Where(x => x.Amount <= input.MaximumAmount.Value);
        if (input.CreatedByUserId.HasValue) query = query.Where(x => x.CreatorId == input.CreatedByUserId.Value);
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var term = input.Filter;
            query = query.Where(x => x.ExpenseNumber.Contains(term)
                || (x.Description != null && x.Description.Contains(term))
                || (x.PaidTo != null && x.PaidTo.Contains(term))
                || (x.ReferenceNumber != null && x.ReferenceNumber.Contains(term)));
        }

        // CashRegisterId has no direct expense column - expenses paid from cash are matched via the cash register's transaction ledger.
        if (input.CashRegisterId.HasValue)
        {
            var cashTxQuery = (await _cashTransactionRepository.GetQueryableAsync()).AsNoTracking();
            var expenseIds = await AsyncExecuter.ToListAsync(
                cashTxQuery.Where(x => x.TenantId == tenantId && x.CashRegisterId == input.CashRegisterId.Value
                    && x.ReferenceType == ShopCashReferenceType.Expense)
                    .Select(x => x.ReferenceId));
            query = query.Where(x => expenseIds.Contains(x.Id));
        }

        return query;
    }

    private async Task<List<ShopExpenseReportItemDto>> ProjectExpenseItemsAsync(List<Expenses.ShopExpense> expenses)
    {
        if (expenses.Count == 0) return new List<ShopExpenseReportItemDto>();

        var categoryIds = expenses.Select(x => x.ExpenseCategoryId).Distinct().ToList();
        var categoryQuery = (await _expenseCategoryRepository.GetQueryableAsync()).AsNoTracking();
        var categories = await AsyncExecuter.ToListAsync(categoryQuery.Where(x => categoryIds.Contains(x.Id)).Select(x => new { x.Id, x.Name }));
        var categoryDict = categories.ToDictionary(x => x.Id, x => x.Name);

        var bankAccountIds = expenses.Where(x => x.BankAccountId.HasValue).Select(x => x.BankAccountId!.Value).Distinct().ToList();
        var bankDict = new Dictionary<Guid, string>();
        if (bankAccountIds.Count > 0)
        {
            var bankQuery = (await _bankAccountRepository.GetQueryableAsync()).AsNoTracking();
            var accounts = await AsyncExecuter.ToListAsync(bankQuery.Where(x => bankAccountIds.Contains(x.Id)).Select(x => new { x.Id, x.AccountName }));
            bankDict = accounts.ToDictionary(x => x.Id, x => x.AccountName);
        }

        var userNames = await GetUserNamesAsync(expenses.Select(x => x.CreatorId).Where(x => x.HasValue).Select(x => x!.Value));

        return expenses.Select(e => new ShopExpenseReportItemDto
        {
            ExpenseId = e.Id,
            ExpenseNumber = e.ExpenseNumber,
            ExpenseDate = e.ExpenseDate,
            ExpenseCategoryId = e.ExpenseCategoryId,
            ExpenseCategoryName = categoryDict.GetValueOrDefault(e.ExpenseCategoryId, string.Empty),
            Description = e.Description,
            Amount = e.Amount,
            TaxAmount = 0, // ShopExpense has no persisted TaxAmount field - kept for shape completeness only.
            TotalAmount = e.Amount,
            PaymentSource = e.PaymentMethod,
            BankAccountId = e.BankAccountId,
            BankAccountName = e.BankAccountId.HasValue ? bankDict.GetValueOrDefault(e.BankAccountId.Value) : null,
            ReferenceNumber = e.ReferenceNumber,
            Status = e.Status,
            CreatedByUserName = e.CreatorId.HasValue ? userNames.GetValueOrDefault(e.CreatorId.Value) : null,
            CreationTime = e.CreationTime,
        }).ToList();
    }

    private async Task<ShopExpenseReportTotalsDto> ComputeExpenseTotalsAsync(IQueryable<Expenses.ShopExpense> filtered)
    {
        var agg = await AsyncExecuter.FirstOrDefaultAsync(
            filtered.GroupBy(x => 1).Select(g => new
            {
                Count = g.Count(),
                Amount = g.Sum(x => x.Amount),
                Cash = g.Where(x => x.PaymentMethod == Expenses.ShopExpensePaymentMethod.Cash).Sum(x => x.Amount),
                Bank = g.Where(x => x.PaymentMethod == Expenses.ShopExpensePaymentMethod.BankTransfer || x.PaymentMethod == Expenses.ShopExpensePaymentMethod.Cheque).Sum(x => x.Amount),
                Other = g.Where(x => x.PaymentMethod == Expenses.ShopExpensePaymentMethod.Card || x.PaymentMethod == Expenses.ShopExpensePaymentMethod.Other).Sum(x => x.Amount),
            }));

        var count = agg?.Count ?? 0;
        var amount = agg?.Amount ?? 0;

        return new ShopExpenseReportTotalsDto
        {
            ExpenseCount = count,
            ExpenseAmount = amount,
            TaxAmount = 0,
            TotalExpenseAmount = amount,
            CashExpenses = agg?.Cash ?? 0,
            BankExpenses = agg?.Bank ?? 0,
            OtherSourceExpenses = agg?.Other ?? 0,
            AverageExpense = count > 0 ? Math.Round(amount / count, 2) : 0,
        };
    }

    private async Task<List<ShopExpenseReportGroupItemDto>> BuildExpenseGroupsAsync(IQueryable<Expenses.ShopExpense> filtered, ShopExpenseReportGroupBy groupBy)
    {
        var expenses = await AsyncExecuter.ToListAsync(filtered.Select(x => new
        {
            x.ExpenseDate, x.ExpenseCategoryId, x.PaymentMethod, x.Amount, x.CreatorId,
        }));

        var groups = new Dictionary<string, (string Label, int Count, decimal Amount)>();

        void AddToGroup(string key, string label, decimal amount)
        {
            if (groups.TryGetValue(key, out var existing))
                groups[key] = (existing.Label, existing.Count + 1, existing.Amount + amount);
            else
                groups[key] = (label, 1, amount);
        }

        Dictionary<Guid, string>? categoryDict = null;
        Dictionary<Guid, string>? userDict = null;
        if (groupBy == ShopExpenseReportGroupBy.ExpenseCategory)
        {
            var categoryQuery = (await _expenseCategoryRepository.GetQueryableAsync()).AsNoTracking();
            var ids = expenses.Select(x => x.ExpenseCategoryId).Distinct().ToList();
            categoryDict = (await AsyncExecuter.ToListAsync(categoryQuery.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name })))
                .ToDictionary(x => x.Id, x => x.Name);
        }
        if (groupBy == ShopExpenseReportGroupBy.User)
        {
            userDict = await GetUserNamesAsync(expenses.Where(x => x.CreatorId.HasValue).Select(x => x.CreatorId!.Value));
        }

        foreach (var e in expenses)
        {
            switch (groupBy)
            {
                case ShopExpenseReportGroupBy.Day:
                    var day = e.ExpenseDate.Date;
                    AddToGroup(day.ToString("yyyy-MM-dd"), day.ToString("yyyy-MM-dd"), e.Amount);
                    break;
                case ShopExpenseReportGroupBy.Month:
                    var month = new DateTime(e.ExpenseDate.Year, e.ExpenseDate.Month, 1);
                    AddToGroup(month.ToString("yyyy-MM"), month.ToString("yyyy-MM"), e.Amount);
                    break;
                case ShopExpenseReportGroupBy.ExpenseCategory:
                    var categoryName = categoryDict!.GetValueOrDefault(e.ExpenseCategoryId, "Unknown");
                    AddToGroup(e.ExpenseCategoryId.ToString(), categoryName, e.Amount);
                    break;
                case ShopExpenseReportGroupBy.PaymentSource:
                    AddToGroup(e.PaymentMethod.ToString(), e.PaymentMethod.ToString(), e.Amount);
                    break;
                case ShopExpenseReportGroupBy.User:
                    var userKey = e.CreatorId?.ToString() ?? "unknown";
                    var userName = e.CreatorId.HasValue ? userDict!.GetValueOrDefault(e.CreatorId.Value, "Unknown") : "Unknown";
                    AddToGroup(userKey, userName ?? "Unknown", e.Amount);
                    break;
            }
        }

        return groups.Select(g => new ShopExpenseReportGroupItemDto
        {
            GroupKey = g.Key,
            GroupLabel = g.Value.Label,
            ExpenseCount = g.Value.Count,
            Amount = g.Value.Amount,
            TotalAmount = g.Value.Amount,
        }).OrderByDescending(x => x.Amount).ToList();
    }
}

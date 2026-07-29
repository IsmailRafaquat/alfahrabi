using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.BankAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Content;

namespace EHub.ShopManagement.Reports;

public partial class ShopReportAppService
{
    // ------------------------------------------------------------------
    // 12. Bank Transaction Report
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.Bank)]
    public async Task<ShopBankTransactionReportResultDto> GetBankTransactionReportAsync(GetShopBankTransactionReportInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);

        var filtered = await BuildBankTransactionReportQueryAsync(tenantId, range, input);
        var totals = await ComputeBankTotalsAsync(tenantId, range, input.BankAccountId, filtered);

        var sorting = ApplySorting(input.Sorting, "TransactionDate desc, CreationTime desc");
        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var paged = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = await ProjectBankTransactionItemsAsync(paged);

        return new ShopBankTransactionReportResultDto { Items = items, TotalCount = totalCount, Totals = totals };
    }

    [Authorize(EHubPermissions.ShopReports.Bank), Authorize(EHubPermissions.ShopReports.Export)]
    public async Task<IRemoteStreamContent> ExportBankTransactionReportAsync(GetShopBankTransactionReportInput input, ShopReportExportFormat format)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);

        var filtered = await BuildBankTransactionReportQueryAsync(tenantId, range, input);
        var totals = await ComputeBankTotalsAsync(tenantId, range, input.BankAccountId, filtered);

        var count = await AsyncExecuter.CountAsync(filtered);
        EnsureWithinExportLimit(count);

        var all = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, ApplySorting(input.Sorting, "TransactionDate desc")));
        var items = await ProjectBankTransactionItemsAsync(all);

        var setting = await GetSettingAsync(tenantId);
        var request = new ShopReportExportRequest
        {
            ReportTitle = "Bank Transaction Report",
            ShopName = setting?.ShopDisplayName,
            ShopAddress = CombineAddress(setting),
            ShopPhone = setting?.Phone,
            DateFrom = range.From,
            DateTo = range.ToExclusive.AddDays(-1),
            GeneratedDate = Clock.Now,
            Columns = new List<ShopReportExportColumn>
            {
                new("Date", "transactionDate"), new("Account", "bankAccountName"), new("Account No.", "accountNumberMasked"),
                new("Type", "transactionType"), new("Reference", "referenceNumber"), new("Amount In", "amountIn"),
                new("Amount Out", "amountOut"), new("Balance", "balanceAfterTransaction"), new("Description", "description"),
            },
            Rows = items.Select(x => new Dictionary<string, object?>
            {
                ["transactionDate"] = x.TransactionDate, ["bankAccountName"] = x.BankAccountName, ["accountNumberMasked"] = x.AccountNumberMasked,
                ["transactionType"] = x.TransactionType.ToString(), ["referenceNumber"] = x.ReferenceNumber, ["amountIn"] = x.AmountIn,
                ["amountOut"] = x.AmountOut, ["balanceAfterTransaction"] = x.BalanceAfterTransaction, ["description"] = x.Description,
            }).ToList(),
            TotalsLines = new List<(string, string)>
            {
                ("Opening Balance", totals.OpeningBalance.ToString("N2")), ("Total In", totals.TotalAmountIn.ToString("N2")),
                ("Total Out", totals.TotalAmountOut.ToString("N2")), ("Closing Balance", totals.ClosingBalance.ToString("N2")),
            },
        };

        return await _exportService.ExportAsync(request, format);
    }

    private async Task<IQueryable<ShopBankTransaction>> BuildBankTransactionReportQueryAsync(Guid tenantId, ShopReportDateRange range, GetShopBankTransactionReportInput input)
    {
        var query = (await _bankTransactionRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TransactionDate >= range.From && x.TransactionDate < range.ToExclusive);

        if (input.BankAccountId.HasValue) query = query.Where(x => x.BankAccountId == input.BankAccountId.Value);
        if (input.TransactionType.HasValue) query = query.Where(x => x.TransactionType == input.TransactionType.Value);
        if (input.ReferenceType.HasValue) query = query.Where(x => x.ReferenceType == input.ReferenceType.Value);
        if (input.MinimumAmount.HasValue) query = query.Where(x => x.Amount >= input.MinimumAmount.Value);
        if (input.MaximumAmount.HasValue) query = query.Where(x => x.Amount <= input.MaximumAmount.Value);
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var term = input.Filter;
            query = query.Where(x => x.ReferenceNumber.Contains(term) || (x.Description != null && x.Description.Contains(term)));
        }

        return query;
    }

    private async Task<List<ShopBankTransactionReportItemDto>> ProjectBankTransactionItemsAsync(List<ShopBankTransaction> rows)
    {
        if (rows.Count == 0) return new List<ShopBankTransactionReportItemDto>();

        var accountIds = rows.Select(x => x.BankAccountId).Distinct().ToList();
        var accountQuery = (await _bankAccountRepository.GetQueryableAsync()).AsNoTracking();
        var accounts = await AsyncExecuter.ToListAsync(accountQuery.Where(x => accountIds.Contains(x.Id)).Select(x => new { x.Id, x.AccountName, x.AccountNumber }));
        var accountDict = accounts.ToDictionary(x => x.Id, x => x);

        return rows.Select(tx =>
        {
            accountDict.TryGetValue(tx.BankAccountId, out var account);
            return new ShopBankTransactionReportItemDto
            {
                BankTransactionId = tx.Id,
                TransactionDate = tx.TransactionDate,
                BankAccountId = tx.BankAccountId,
                BankAccountName = account?.AccountName ?? string.Empty,
                AccountNumberMasked = MaskAccountNumber(account?.AccountNumber),
                TransactionType = tx.TransactionType,
                ReferenceType = tx.ReferenceType,
                ReferenceId = tx.ReferenceId,
                ReferenceNumber = tx.ReferenceNumber,
                AmountIn = tx.Direction == ShopBankDirection.In ? tx.Amount : 0,
                AmountOut = tx.Direction == ShopBankDirection.Out ? tx.Amount : 0,
                BalanceAfterTransaction = tx.BalanceAfterTransaction,
                Description = tx.Description,
                CreationTime = tx.CreationTime,
            };
        }).ToList();
    }

    private async Task<ShopBankTransactionReportTotalsDto> ComputeBankTotalsAsync(Guid tenantId, ShopReportDateRange range, Guid? bankAccountId, IQueryable<ShopBankTransaction> filtered)
    {
        var agg = await AsyncExecuter.FirstOrDefaultAsync(
            filtered.GroupBy(x => 1).Select(g => new
            {
                Count = g.Count(),
                In = g.Where(x => x.Direction == ShopBankDirection.In).Sum(x => x.Amount),
                Out = g.Where(x => x.Direction == ShopBankDirection.Out).Sum(x => x.Amount),
            }));

        var totalIn = agg?.In ?? 0;
        var totalOut = agg?.Out ?? 0;

        var accountIds = await AsyncExecuter.ToListAsync(filtered.Select(x => x.BankAccountId).Distinct());
        if (accountIds.Count == 0 && bankAccountId.HasValue) accountIds.Add(bankAccountId.Value);

        decimal openingBalance = 0;
        var txQuery = (await _bankTransactionRepository.GetQueryableAsync()).AsNoTracking();
        var accountQuery = (await _bankAccountRepository.GetQueryableAsync()).AsNoTracking();

        foreach (var accountId in accountIds)
        {
            var lastBefore = await AsyncExecuter.FirstOrDefaultAsync(
                txQuery.Where(x => x.TenantId == tenantId && x.BankAccountId == accountId && x.TransactionDate < range.From)
                    .OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.CreationTime));

            if (lastBefore != null)
            {
                openingBalance += lastBefore.BalanceAfterTransaction;
            }
            else
            {
                var account = await AsyncExecuter.FirstOrDefaultAsync(accountQuery.Where(x => x.Id == accountId));
                openingBalance += account?.OpeningBalance ?? 0;
            }
        }

        return new ShopBankTransactionReportTotalsDto
        {
            OpeningBalance = openingBalance,
            TotalAmountIn = totalIn,
            TotalAmountOut = totalOut,
            ClosingBalance = Math.Round(openingBalance + totalIn - totalOut, 2),
            TransactionCount = agg?.Count ?? 0,
        };
    }

    private static string MaskAccountNumber(string? accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber)) return string.Empty;
        var trimmed = accountNumber.Trim();
        return trimmed.Length <= 4 ? new string('*', trimmed.Length) : "****" + trimmed[^4..];
    }
}

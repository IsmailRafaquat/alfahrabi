using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.PurchaseReturns;
using EHub.ShopManagement.Settings;
using EHub.ShopManagement.SupplierPayments;
using EHub.ShopManagement.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.SupplierLedger;

[Authorize(EHubPermissions.ShopSupplierLedger.Default)]
public class ShopSupplierLedgerAppService : ApplicationService, IShopSupplierLedgerAppService
{
    private readonly IRepository<ShopSupplier, Guid> _supplierRepository;
    private readonly IRepository<ShopGoodsReceipt, Guid> _goodsReceiptRepository;
    private readonly IRepository<ShopSupplierPayment, Guid> _paymentRepository;
    private readonly IRepository<ShopPurchaseReturn, Guid> _purchaseReturnRepository;
    private readonly IRepository<ShopSetting, Guid> _settingRepository;

    public ShopSupplierLedgerAppService(
        IRepository<ShopSupplier, Guid> supplierRepository,
        IRepository<ShopGoodsReceipt, Guid> goodsReceiptRepository,
        IRepository<ShopSupplierPayment, Guid> paymentRepository,
        IRepository<ShopPurchaseReturn, Guid> purchaseReturnRepository,
        IRepository<ShopSetting, Guid> settingRepository)
    {
        _supplierRepository = supplierRepository;
        _goodsReceiptRepository = goodsReceiptRepository;
        _paymentRepository = paymentRepository;
        _purchaseReturnRepository = purchaseReturnRepository;
        _settingRepository = settingRepository;
    }

    public async Task<ShopSupplierLedgerDto> GetLedgerAsync(GetShopSupplierLedgerInput input)
    {
        var dto = await BuildLedgerAsync(input);
        await HideLedgerAmountsIfNotAllowedAsync(dto);
        return dto;
    }

    public async Task<ShopSupplierBalanceSummaryDto> GetBalanceSummaryAsync(Guid supplierId)
    {
        var tenantId = RequireTenant();
        var supplier = await GetSupplierAsync(supplierId, tenantId);
        var rows = await LoadAllRowsAsync(tenantId, supplierId);

        var totalPurchases = Round(rows.Where(x => x.ReferenceType == ShopSupplierLedgerReferenceType.GoodsReceipt).Sum(x => x.Debit));
        var totalReturns = Round(rows.Where(x => x.ReferenceType == ShopSupplierLedgerReferenceType.PurchaseReturn).Sum(x => x.Credit));
        var totalPayments = Round(rows.Where(x => x.ReferenceType == ShopSupplierLedgerReferenceType.SupplierPayment).Sum(x => x.Credit));
        var currentBalance = Round(supplier.OpeningBalance + rows.Sum(x => x.Debit - x.Credit));
        var lastTransactionDate = rows.Count > 0 ? rows.Max(x => x.TransactionDate) : (DateTime?)null;

        var dto = new ShopSupplierBalanceSummaryDto
        {
            SupplierId = supplier.Id,
            SupplierCode = supplier.Code,
            SupplierName = supplier.Name,
            OpeningBalance = supplier.OpeningBalance,
            TotalCompletedPurchases = totalPurchases,
            TotalCompletedReturns = totalReturns,
            TotalPostedPayments = totalPayments,
            CurrentBalance = currentBalance,
            PayableAmount = Math.Max(currentBalance, 0),
            AdvanceAmount = Math.Max(-currentBalance, 0),
            LastTransactionDate = lastTransactionDate
        };

        await HideSummaryAmountsIfNotAllowedAsync(dto);
        return dto;
    }

    public async Task<ShopSupplierStatementDto> GetStatementAsync(GetShopSupplierLedgerInput input)
    {
        var tenantId = RequireTenant();
        var ledger = await BuildLedgerAsync(input);
        var supplier = await GetSupplierAsync(input.SupplierId, tenantId);

        var settingQuery = await _settingRepository.GetQueryableAsync();
        var setting = await AsyncExecuter.FirstOrDefaultAsync(settingQuery.Where(x => x.TenantId == tenantId));

        var statement = new ShopSupplierStatementDto
        {
            ShopName = setting?.ShopDisplayName ?? string.Empty,
            ShopAddress = CombineAddress(setting?.AddressLine1, setting?.AddressLine2, setting?.City, setting?.StateOrProvince, setting?.PostalCode, setting?.Country),
            ShopPhone = setting?.Phone,
            ShopEmail = setting?.Email,
            SupplierId = ledger.SupplierId,
            SupplierCode = ledger.SupplierCode,
            SupplierName = ledger.SupplierName,
            SupplierAddress = CombineAddress(supplier.AddressLine1, supplier.AddressLine2, supplier.City, supplier.StateOrProvince, supplier.PostalCode, supplier.Country),
            SupplierPhone = supplier.Phone,
            SupplierEmail = supplier.Email,
            DateFrom = ledger.DateFrom,
            DateTo = ledger.DateTo,
            GeneratedDate = Clock.Now,
            OpeningBalance = ledger.OpeningBalance,
            TotalDebit = ledger.TotalDebit,
            TotalCredit = ledger.TotalCredit,
            ClosingBalance = ledger.ClosingBalance,
            PayableAmount = ledger.PayableAmount,
            AdvanceAmount = ledger.AdvanceAmount,
            Entries = ledger.Entries
        };

        await HideStatementAmountsIfNotAllowedAsync(statement);
        return statement;
    }

    private async Task<ShopSupplierLedgerDto> BuildLedgerAsync(GetShopSupplierLedgerInput input)
    {
        var tenantId = RequireTenant();
        var supplier = await GetSupplierAsync(input.SupplierId, tenantId);
        var allRows = await LoadAllRowsAsync(tenantId, input.SupplierId);

        var openingBalance = supplier.OpeningBalance;
        var effectiveOpening = openingBalance;
        var periodRows = allRows;

        if (input.DateFrom.HasValue)
        {
            var before = allRows.Where(x => x.TransactionDate.Date < input.DateFrom.Value.Date).ToList();
            effectiveOpening = Round(openingBalance + before.Sum(x => x.Debit - x.Credit));
            periodRows = allRows.Where(x => x.TransactionDate.Date >= input.DateFrom.Value.Date).ToList();
        }

        if (input.DateTo.HasValue)
        {
            periodRows = periodRows.Where(x => x.TransactionDate.Date <= input.DateTo.Value.Date).ToList();
        }

        var entries = new List<ShopSupplierLedgerEntryDto> { BuildOpeningEntry(supplier, input.DateFrom, effectiveOpening, openingBalance) };

        var running = effectiveOpening;
        foreach (var row in periodRows)
        {
            running = Round(running + row.Debit - row.Credit);
            entries.Add(new ShopSupplierLedgerEntryDto
            {
                TransactionDate = row.TransactionDate,
                ReferenceType = row.ReferenceType,
                ReferenceId = row.ReferenceId,
                ReferenceNumber = row.ReferenceNumber,
                Description = row.Description,
                DebitAmount = row.Debit,
                CreditAmount = row.Credit,
                RunningBalance = running,
                TransactionStatus = row.Status
            });
        }

        var totalDebit = Round(periodRows.Sum(x => x.Debit));
        var totalCredit = Round(periodRows.Sum(x => x.Credit));
        var totalPurchases = Round(periodRows.Where(x => x.ReferenceType == ShopSupplierLedgerReferenceType.GoodsReceipt).Sum(x => x.Debit));
        var totalPayments = Round(periodRows.Where(x => x.ReferenceType == ShopSupplierLedgerReferenceType.SupplierPayment).Sum(x => x.Credit));
        var totalReturns = Round(periodRows.Where(x => x.ReferenceType == ShopSupplierLedgerReferenceType.PurchaseReturn).Sum(x => x.Credit));
        var closingBalance = running;

        var displayEntries = new List<ShopSupplierLedgerEntryDto> { entries[0] };
        displayEntries.AddRange(entries.Skip(1).Where(x => MatchesFilters(x, input)));

        return new ShopSupplierLedgerDto
        {
            SupplierId = supplier.Id,
            SupplierCode = supplier.Code,
            SupplierName = supplier.Name,
            DateFrom = input.DateFrom,
            DateTo = input.DateTo,
            OpeningBalance = effectiveOpening,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            ClosingBalance = closingBalance,
            PayableAmount = Math.Max(closingBalance, 0),
            AdvanceAmount = Math.Max(-closingBalance, 0),
            TotalPurchases = totalPurchases,
            TotalPayments = totalPayments,
            TotalReturns = totalReturns,
            Entries = displayEntries
        };
    }

    private static bool MatchesFilters(ShopSupplierLedgerEntryDto entry, GetShopSupplierLedgerInput input)
    {
        if (input.ReferenceType.HasValue && entry.ReferenceType != input.ReferenceType.Value) return false;

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var filter = input.Filter!;
            var matches = entry.ReferenceNumber.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                          entry.Description.Contains(filter, StringComparison.OrdinalIgnoreCase);
            if (!matches) return false;
        }

        return true;
    }

    private static ShopSupplierLedgerEntryDto BuildOpeningEntry(ShopSupplier supplier, DateTime? dateFrom, decimal effectiveOpening, decimal trueOpeningBalance)
    {
        if (dateFrom.HasValue)
        {
            return new ShopSupplierLedgerEntryDto
            {
                TransactionDate = dateFrom.Value,
                ReferenceType = ShopSupplierLedgerReferenceType.OpeningBalance,
                ReferenceId = supplier.Id,
                ReferenceNumber = "Opening",
                Description = "Balance Brought Forward",
                DebitAmount = 0,
                CreditAmount = 0,
                RunningBalance = effectiveOpening,
                TransactionStatus = "N/A"
            };
        }

        return new ShopSupplierLedgerEntryDto
        {
            TransactionDate = supplier.CreationTime,
            ReferenceType = ShopSupplierLedgerReferenceType.OpeningBalance,
            ReferenceId = supplier.Id,
            ReferenceNumber = "Opening",
            Description = "Opening Balance",
            DebitAmount = trueOpeningBalance,
            CreditAmount = 0,
            RunningBalance = effectiveOpening,
            TransactionStatus = "N/A"
        };
    }

    private async Task<List<LedgerRow>> LoadAllRowsAsync(Guid tenantId, Guid supplierId)
    {
        var goodsReceiptQuery = await _goodsReceiptRepository.GetQueryableAsync();
        var goodsReceipts = await AsyncExecuter.ToListAsync(goodsReceiptQuery.Where(x =>
            x.TenantId == tenantId && x.SupplierId == supplierId && x.Status == ShopGoodsReceiptStatus.Completed));

        var paymentQuery = await _paymentRepository.GetQueryableAsync();
        var payments = await AsyncExecuter.ToListAsync(paymentQuery.Where(x =>
            x.TenantId == tenantId && x.SupplierId == supplierId && x.Status == ShopSupplierPaymentStatus.Posted));

        var returnQuery = await _purchaseReturnRepository.GetQueryableAsync();
        var purchaseReturns = await AsyncExecuter.ToListAsync(returnQuery.Where(x =>
            x.TenantId == tenantId && x.SupplierId == supplierId && x.Status == ShopPurchaseReturnStatus.Completed));

        var rows = new List<LedgerRow>();

        foreach (var goodsReceipt in goodsReceipts)
        {
            rows.Add(new LedgerRow(
                goodsReceipt.ReceiptDate, goodsReceipt.CreationTime, ShopSupplierLedgerReferenceType.GoodsReceipt,
                goodsReceipt.Id, goodsReceipt.GoodsReceiptNumber, BuildGoodsReceiptDescription(goodsReceipt),
                goodsReceipt.GrandTotal, 0, "Completed"));
        }

        foreach (var payment in payments)
        {
            rows.Add(new LedgerRow(
                payment.PaymentDate, payment.CreationTime, ShopSupplierLedgerReferenceType.SupplierPayment,
                payment.Id, payment.PaymentNumber, BuildPaymentDescription(payment),
                0, payment.Amount, "Posted"));
        }

        foreach (var purchaseReturn in purchaseReturns)
        {
            rows.Add(new LedgerRow(
                purchaseReturn.ReturnDate, purchaseReturn.CreationTime, ShopSupplierLedgerReferenceType.PurchaseReturn,
                purchaseReturn.Id, purchaseReturn.PurchaseReturnNumber, $"Purchase Return - {purchaseReturn.PurchaseReturnNumber}",
                0, purchaseReturn.GrandTotal, "Completed"));
        }

        return rows.OrderBy(x => x.TransactionDate).ThenBy(x => x.CreationTime).ThenBy(x => x.ReferenceNumber).ToList();
    }

    private static string BuildGoodsReceiptDescription(ShopGoodsReceipt goodsReceipt) =>
        string.IsNullOrWhiteSpace(goodsReceipt.SupplierInvoiceNumber)
            ? $"Goods Receipt - {goodsReceipt.GoodsReceiptNumber}"
            : $"Goods Receipt - Supplier Invoice {goodsReceipt.SupplierInvoiceNumber}";

    private static string BuildPaymentDescription(ShopSupplierPayment payment)
    {
        var text = $"Supplier Payment - {payment.PaymentMethod}";
        if (!string.IsNullOrWhiteSpace(payment.ReferenceNumber)) text += $" (Ref: {payment.ReferenceNumber})";
        return text;
    }

    private static string? CombineAddress(string? line1, string? line2, string? city, string? state, string? postalCode, string? country)
    {
        var parts = new[] { line1, line2, city, state, postalCode, country }.Where(x => !string.IsNullOrWhiteSpace(x));
        var combined = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(combined) ? null : combined;
    }

    private async Task HideLedgerAmountsIfNotAllowedAsync(ShopSupplierLedgerDto dto)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopSupplierLedger.ViewAmounts)) return;

        dto.OpeningBalance = null;
        dto.TotalDebit = null;
        dto.TotalCredit = null;
        dto.ClosingBalance = null;
        dto.PayableAmount = null;
        dto.AdvanceAmount = null;
        dto.TotalPurchases = null;
        dto.TotalPayments = null;
        dto.TotalReturns = null;
        foreach (var entry in dto.Entries)
        {
            entry.DebitAmount = null;
            entry.CreditAmount = null;
            entry.RunningBalance = null;
        }
    }

    private async Task HideSummaryAmountsIfNotAllowedAsync(ShopSupplierBalanceSummaryDto dto)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopSupplierLedger.ViewAmounts)) return;

        dto.OpeningBalance = null;
        dto.TotalCompletedPurchases = null;
        dto.TotalCompletedReturns = null;
        dto.TotalPostedPayments = null;
        dto.CurrentBalance = null;
        dto.PayableAmount = null;
        dto.AdvanceAmount = null;
    }

    private async Task HideStatementAmountsIfNotAllowedAsync(ShopSupplierStatementDto dto)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopSupplierLedger.ViewAmounts)) return;

        dto.OpeningBalance = null;
        dto.TotalDebit = null;
        dto.TotalCredit = null;
        dto.ClosingBalance = null;
        dto.PayableAmount = null;
        dto.AdvanceAmount = null;
        foreach (var entry in dto.Entries)
        {
            entry.DebitAmount = null;
            entry.CreditAmount = null;
            entry.RunningBalance = null;
        }
    }

    private async Task<ShopSupplier> GetSupplierAsync(Guid supplierId, Guid tenantId)
    {
        var query = await _supplierRepository.GetQueryableAsync();
        var supplier = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == supplierId && x.TenantId == tenantId));
        return supplier ?? throw new BusinessException("ShopManagement:SupplierLedgerSupplierNotFound");
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private sealed record LedgerRow(
        DateTime TransactionDate,
        DateTime CreationTime,
        ShopSupplierLedgerReferenceType ReferenceType,
        Guid ReferenceId,
        string ReferenceNumber,
        string Description,
        decimal Debit,
        decimal Credit,
        string Status);
}

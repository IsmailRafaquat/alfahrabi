using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.BankAccounts;
using EHub.ShopManagement.CashRegisters;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.Sales;
using EHub.ShopManagement.StockTransactions;
using EHub.ShopManagement.Units;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace EHub.ShopManagement.SaleReturns;

public class ShopSaleReturnManager : DomainService
{
    private const string DocumentType = "SaleReturn";
    private const string NumberPrefix = "SR-";

    private readonly IRepository<ShopSaleReturn, Guid> _repository;
    private readonly IRepository<ShopSaleReturnItem, Guid> _itemRepository;
    private readonly IRepository<ShopSale, Guid> _saleRepository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly IRepository<ShopStockTransaction, Guid> _stockTransactionRepository;
    private readonly ShopDocumentNumberGenerator _numberGenerator;
    private readonly ShopCashRegisterManager _cashRegisterManager;
    private readonly ShopBankAccountManager _bankAccountManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ShopSaleReturnManager(
        IRepository<ShopSaleReturn, Guid> repository,
        IRepository<ShopSaleReturnItem, Guid> itemRepository,
        IRepository<ShopSale, Guid> saleRepository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        IRepository<ShopStockTransaction, Guid> stockTransactionRepository,
        ShopDocumentNumberGenerator numberGenerator,
        ShopCashRegisterManager cashRegisterManager,
        ShopBankAccountManager bankAccountManager,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _itemRepository = itemRepository;
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _numberGenerator = numberGenerator;
        _cashRegisterManager = cashRegisterManager;
        _bankAccountManager = bankAccountManager;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<ShopSaleReturn> CreateAsync(
        Guid saleId,
        DateTime returnDate,
        ShopSaleReturnReason reason,
        string? reasonDetails,
        ShopSaleReturnSettlementType settlementType,
        Guid? bankAccountId,
        decimal otherCharges,
        string? notes,
        IReadOnlyList<ShopSaleReturnItemInput> items)
    {
        var tenantId = RequireTenant();
        var sale = await GetCompletedSaleAsync(saleId, tenantId);
        await ValidateBankAccountAsync(bankAccountId, tenantId);

        var returnId = GuidGenerator.Create();
        var itemEntities = await BuildItemEntitiesAsync(returnId, tenantId, sale, items, excludeReturnId: null);

        var number = await _numberGenerator.GetNextNumberAsync(tenantId, DocumentType, NumberPrefix);

        return new ShopSaleReturn(returnId, tenantId, number, sale.Id, sale.CustomerId, returnDate, reason,
            reasonDetails, settlementType, bankAccountId, otherCharges, notes, itemEntities);
    }

    public async Task UpdateAsync(
        ShopSaleReturn saleReturn,
        DateTime returnDate,
        ShopSaleReturnReason reason,
        string? reasonDetails,
        ShopSaleReturnSettlementType settlementType,
        Guid? bankAccountId,
        decimal otherCharges,
        string? notes,
        IReadOnlyList<ShopSaleReturnItemInput> items)
    {
        var tenantId = RequireTenantOwnership(saleReturn);
        saleReturn.EnsureEditable();
        await ValidateBankAccountAsync(bankAccountId, tenantId);

        var sale = await GetCompletedSaleAsync(saleReturn.SaleId, tenantId);
        var itemEntities = await BuildItemEntitiesAsync(saleReturn.Id, tenantId, sale, items, excludeReturnId: saleReturn.Id);

        saleReturn.Update(returnDate, reason, reasonDetails, settlementType, bankAccountId, otherCharges, notes, itemEntities);
    }

    public async Task CompleteAsync(ShopSaleReturn saleReturn)
    {
        var tenantId = RequireTenantOwnership(saleReturn);
        saleReturn.EnsureCompletable();

        // Re-validate returnable quantities against the current completed-returns state. This protects
        // against two concurrently-created drafts each reserving more than what is actually returnable.
        var sale = await GetCompletedSaleAsync(saleReturn.SaleId, tenantId);
        var saleItemsById = sale.Items.ToDictionary(x => x.Id);
        foreach (var item in saleReturn.Items)
        {
            if (!saleItemsById.TryGetValue(item.SaleItemId, out var saleItem))
                throw new BusinessException("ShopManagement:SaleReturnSaleItemNotFound");

            var previouslyReturned = await GetReturnedQuantityAsync(item.SaleItemId, tenantId, excludeReturnId: saleReturn.Id);
            var returnable = saleItem.Quantity - previouslyReturned;
            if (item.ReturnQuantity > returnable) throw new BusinessException("ShopManagement:SaleReturnQuantityExceedsReturnable");
        }

        var itemIds = saleReturn.Items.Select(x => x.Id).ToList();
        var existingTransactionQuery = await _stockTransactionRepository.GetQueryableAsync();
        var alreadyHasTransactions = await AsyncExecuter.AnyAsync(existingTransactionQuery.Where(x =>
            x.TenantId == tenantId && x.ReferenceType == ShopStockReferenceType.SaleReturn &&
            x.SourceItemId.HasValue && itemIds.Contains(x.SourceItemId.Value)));
        if (alreadyHasTransactions) throw new BusinessException("ShopManagement:SaleReturnStockTransactionAlreadyExists");

        var productIds = saleReturn.Items.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);

        var completedByUserId = _currentUser.GetId();
        var completedDate = Clock.Now;

        foreach (var item in saleReturn.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product)) throw new BusinessException("ShopManagement:SaleReturnProductNotFound");

            product.IncreaseStock(item.ReturnQuantity);

            var transaction = new ShopStockTransaction(
                GuidGenerator.Create(), tenantId, item.ProductId, ShopStockTransactionType.SaleReturn, ShopStockReferenceType.SaleReturn,
                saleReturn.Id, saleReturn.SaleReturnNumber, item.Id, saleReturn.ReturnDate,
                item.ReturnQuantity, 0, product.CurrentStock, item.UnitCostSnapshot, item.BatchNumber, item.ExpiryDate,
                null, completedByUserId, completedDate);
            await _stockTransactionRepository.InsertAsync(transaction, autoSave: true);
        }

        foreach (var product in products.Values)
        {
            await _productRepository.UpdateAsync(product, autoSave: true);
        }

        saleReturn.MarkAsCompleted(completedByUserId, completedDate);

        if (saleReturn.SettlementType == ShopSaleReturnSettlementType.CashRefund && saleReturn.RefundAmount > 0)
        {
            await _cashRegisterManager.RecordAutomaticTransactionAsync(
                tenantId, ShopCashTransactionType.CustomerRefund, ShopCashDirection.Out, saleReturn.RefundAmount,
                ShopCashReferenceType.SaleReturn, saleReturn.Id, saleReturn.SaleReturnNumber, $"Cash refund - {saleReturn.SaleReturnNumber}", saleReturn.ReturnDate);
        }
        else if (saleReturn.SettlementType == ShopSaleReturnSettlementType.BankRefund && saleReturn.RefundAmount > 0 && saleReturn.BankAccountId.HasValue)
        {
            await _bankAccountManager.RecordTransactionAsync(
                tenantId, saleReturn.BankAccountId.Value, ShopBankTransactionType.CustomerRefund, ShopBankDirection.Out, saleReturn.RefundAmount,
                ShopBankReferenceType.SaleReturn, saleReturn.Id, saleReturn.SaleReturnNumber, $"Bank refund - {saleReturn.SaleReturnNumber}", saleReturn.ReturnDate);
        }
    }

    private async Task ValidateBankAccountAsync(Guid? bankAccountId, Guid tenantId)
    {
        if (bankAccountId.HasValue) await _bankAccountManager.GetAccountAsync(bankAccountId.Value, tenantId);
    }

    public Task CancelAsync(ShopSaleReturn saleReturn, string cancellationReason)
    {
        RequireTenantOwnership(saleReturn);
        saleReturn.MarkAsCancelled(_currentUser.GetId(), Clock.Now, cancellationReason);
        return Task.CompletedTask;
    }

    public Task ValidateDeleteAsync(ShopSaleReturn saleReturn)
    {
        RequireTenantOwnership(saleReturn);
        saleReturn.EnsureDeletable();
        return Task.CompletedTask;
    }

    public async Task<decimal> GetReturnedQuantityAsync(Guid saleItemId, Guid tenantId, Guid? excludeReturnId = null)
    {
        var itemQuery = await _itemRepository.GetQueryableAsync();
        var returnQuery = await _repository.GetQueryableAsync();

        var query = from item in itemQuery
                     join saleReturn in returnQuery on item.SaleReturnId equals saleReturn.Id
                     where item.SaleItemId == saleItemId && item.TenantId == tenantId &&
                           saleReturn.Status == ShopSaleReturnStatus.Completed &&
                           (!excludeReturnId.HasValue || saleReturn.Id != excludeReturnId.Value)
                     select item.ReturnQuantity;

        var quantities = await AsyncExecuter.ToListAsync(query);
        return quantities.Sum();
    }

    public async Task<ShopSale> GetCompletedSaleAsync(Guid saleId, Guid tenantId)
    {
        var query = await _saleRepository.WithDetailsAsync(x => x.Items);
        var sale = query.FirstOrDefault(x => x.Id == saleId && x.TenantId == tenantId);
        if (sale == null) throw new BusinessException("ShopManagement:SaleReturnSaleNotFound");
        if (sale.Status != ShopSaleStatus.Completed) throw new BusinessException("ShopManagement:SaleReturnRequiresCompletedSale");
        return sale;
    }

    private async Task<List<ShopSaleReturnItem>> BuildItemEntitiesAsync(
        Guid returnId,
        Guid tenantId,
        ShopSale sale,
        IReadOnlyList<ShopSaleReturnItemInput> items,
        Guid? excludeReturnId)
    {
        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:SaleReturnRequiresItems");
        ValidateNoDuplicateSaleItems(items);

        var saleItemsById = sale.Items.ToDictionary(x => x.Id);
        var productIds = sale.Items.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);

        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var itemEntities = new List<ShopSaleReturnItem>();
        foreach (var input in items)
        {
            if (!saleItemsById.TryGetValue(input.SaleItemId, out var saleItem))
                throw new BusinessException("ShopManagement:SaleReturnSaleItemNotFound");

            if (!products.TryGetValue(saleItem.ProductId, out var product))
                throw new BusinessException("ShopManagement:SaleReturnProductNotFound");
            if (!units.TryGetValue(product.UnitId, out var unit))
                throw new BusinessException("ShopManagement:SaleReturnProductNotFound");

            var previouslyReturned = await GetReturnedQuantityAsync(input.SaleItemId, tenantId, excludeReturnId);

            itemEntities.Add(new ShopSaleReturnItem(
                GuidGenerator.Create(), tenantId, returnId, saleItem, previouslyReturned,
                input.ReturnQuantity, unit.AllowDecimal, input.Reason, input.Notes));
        }

        return itemEntities;
    }

    private static void ValidateNoDuplicateSaleItems(IReadOnlyList<ShopSaleReturnItemInput> items)
    {
        var ids = items.Select(x => x.SaleItemId).ToList();
        if (ids.Distinct().Count() != ids.Count) throw new BusinessException("ShopManagement:SaleReturnDuplicateSaleItem");
    }

    private Guid RequireTenantOwnership(ShopSaleReturn saleReturn)
    {
        var tenantId = RequireTenant();
        if (saleReturn.TenantId != tenantId) throw new BusinessException("ShopManagement:SaleReturnNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}

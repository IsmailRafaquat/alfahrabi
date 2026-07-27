using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.StockTransactions;
using EHub.ShopManagement.Units;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace EHub.ShopManagement.StockAdjustments;

public class ShopStockAdjustmentManager : DomainService
{
    private const string DocumentType = "StockAdjustment";
    private const string NumberPrefix = "SA-";

    private readonly IRepository<ShopStockAdjustment, Guid> _repository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly IRepository<ShopStockTransaction, Guid> _stockTransactionRepository;
    private readonly ShopDocumentNumberGenerator _numberGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ShopStockAdjustmentManager(
        IRepository<ShopStockAdjustment, Guid> repository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        IRepository<ShopStockTransaction, Guid> stockTransactionRepository,
        ShopDocumentNumberGenerator numberGenerator,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _numberGenerator = numberGenerator;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<ShopStockAdjustment> CreateAsync(
        DateTime adjustmentDate,
        ShopStockAdjustmentReason reason,
        string? reasonDetails,
        string? notes,
        IReadOnlyList<ShopStockAdjustmentItemInput> items)
    {
        var tenantId = RequireTenant();
        var adjustmentId = GuidGenerator.Create();
        var itemEntities = await BuildItemEntitiesAsync(adjustmentId, tenantId, adjustmentDate, items);

        var number = await _numberGenerator.GetNextNumberAsync(tenantId, DocumentType, NumberPrefix);

        return new ShopStockAdjustment(adjustmentId, tenantId, number, adjustmentDate, reason, reasonDetails, notes, itemEntities);
    }

    public async Task UpdateAsync(
        ShopStockAdjustment adjustment,
        DateTime adjustmentDate,
        ShopStockAdjustmentReason reason,
        string? reasonDetails,
        string? notes,
        IReadOnlyList<ShopStockAdjustmentItemInput> items)
    {
        var tenantId = RequireTenantOwnership(adjustment);
        adjustment.EnsureEditable();

        var itemEntities = await BuildItemEntitiesAsync(adjustment.Id, tenantId, adjustmentDate, items);
        adjustment.Update(adjustmentDate, reason, reasonDetails, notes, itemEntities);
    }

    public async Task PostAsync(ShopStockAdjustment adjustment)
    {
        var tenantId = RequireTenantOwnership(adjustment);
        adjustment.EnsurePostable();

        var productIds = adjustment.Items.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);

        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        // Re-validate every item against the *current* state of its product before touching any stock,
        // so a stale draft (created before another transaction moved the stock, or before tracking flags
        // changed) cannot partially post before failing.
        foreach (var item in adjustment.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
                throw new BusinessException("ShopManagement:StockAdjustmentProductNotFound");
            if (!product.IsActive)
                throw new BusinessException("ShopManagement:StockAdjustmentProductInactive").WithData("Product", product.Name);
            if (!units.TryGetValue(product.UnitId, out var unit))
                throw new BusinessException("ShopManagement:StockAdjustmentProductNotFound");

            ValidateTrackingRules(product, item.AdjustmentType, item.BatchNumber, item.ExpiryDate, adjustment.AdjustmentDate);
            item.RefreshForPosting(product.CurrentStock, unit.AllowDecimal);

            var alreadyExists = await ExistsAsync(tenantId, item.Id);
            if (alreadyExists) throw new BusinessException("ShopManagement:StockAdjustmentTransactionAlreadyExists");
        }

        var postedByUserId = _currentUser.GetId();
        var postedDate = Clock.Now;

        foreach (var item in adjustment.Items)
        {
            var product = products[item.ProductId];

            if (item.AdjustmentType == ShopStockAdjustmentType.Increase)
            {
                product.IncreaseStock(item.AdjustmentQuantity);
            }
            else
            {
                product.DecreaseStock(item.AdjustmentQuantity);
            }

            await _productRepository.UpdateAsync(product, autoSave: true);

            var transactionType = item.AdjustmentType == ShopStockAdjustmentType.Increase
                ? ShopStockTransactionType.StockAdjustmentIncrease
                : ShopStockTransactionType.StockAdjustmentDecrease;
            var quantityIn = item.AdjustmentType == ShopStockAdjustmentType.Increase ? item.AdjustmentQuantity : 0;
            var quantityOut = item.AdjustmentType == ShopStockAdjustmentType.Decrease ? item.AdjustmentQuantity : 0;

            var transaction = new ShopStockTransaction(
                GuidGenerator.Create(), tenantId, product.Id, transactionType, ShopStockReferenceType.StockAdjustment,
                adjustment.Id, adjustment.AdjustmentNumber, item.Id, postedDate, quantityIn, quantityOut,
                product.CurrentStock, item.UnitCostSnapshot, item.BatchNumber, item.ExpiryDate, item.Notes,
                postedByUserId, postedDate);
            await _stockTransactionRepository.InsertAsync(transaction, autoSave: true);
        }

        adjustment.MarkAsPosted(postedByUserId, postedDate);
    }

    public Task CancelAsync(ShopStockAdjustment adjustment, string cancellationReason)
    {
        RequireTenantOwnership(adjustment);
        adjustment.MarkAsCancelled(_currentUser.GetId(), Clock.Now, cancellationReason);
        return Task.CompletedTask;
    }

    public Task ValidateDeleteAsync(ShopStockAdjustment adjustment)
    {
        RequireTenantOwnership(adjustment);
        adjustment.EnsureDeletable();
        return Task.CompletedTask;
    }

    private async Task<List<ShopStockAdjustmentItem>> BuildItemEntitiesAsync(
        Guid adjustmentId, Guid tenantId, DateTime adjustmentDate, IReadOnlyList<ShopStockAdjustmentItemInput> items)
    {
        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:StockAdjustmentRequiresItems");
        ValidateNoDuplicateProducts(items);

        var productIds = items.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);

        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var entities = new List<ShopStockAdjustmentItem>();
        foreach (var input in items)
        {
            if (!products.TryGetValue(input.ProductId, out var product))
                throw new BusinessException("ShopManagement:StockAdjustmentProductNotFound");
            if (!product.IsActive)
                throw new BusinessException("ShopManagement:StockAdjustmentProductInactive").WithData("Product", product.Name);
            if (!units.TryGetValue(product.UnitId, out var unit))
                throw new BusinessException("ShopManagement:StockAdjustmentProductNotFound");

            ValidateTrackingRules(product, input.AdjustmentType, input.BatchNumber, input.ExpiryDate, adjustmentDate);

            entities.Add(new ShopStockAdjustmentItem(
                GuidGenerator.Create(), tenantId, adjustmentId, product, unit.Name, unit.ShortName, unit.AllowDecimal,
                input.AdjustmentType, product.CurrentStock, input.AdjustmentQuantity, product.PurchasePrice,
                input.BatchNumber, input.ExpiryDate, input.Reason, input.Notes));
        }

        return entities;
    }

    private static void ValidateTrackingRules(
        ShopProduct product, ShopStockAdjustmentType adjustmentType, string? batchNumber, DateTime? expiryDate, DateTime adjustmentDate)
    {
        if (product.TrackSerialNumber)
            throw new BusinessException("ShopManagement:StockAdjustmentSerialTrackingNotSupported").WithData("Product", product.Name);

        if (product.TrackBatch)
        {
            if (string.IsNullOrWhiteSpace(batchNumber))
                throw new BusinessException("ShopManagement:StockAdjustmentBatchRequired").WithData("Product", product.Name);

            // Batch-level stock balances are not tracked in this codebase yet, so a Decrease cannot be
            // safely validated against the selected batch's actual remaining quantity.
            if (adjustmentType == ShopStockAdjustmentType.Decrease)
                throw new BusinessException("ShopManagement:StockAdjustmentBatchDeductionNotSupported").WithData("Product", product.Name);
        }

        if (product.TrackExpiry && adjustmentType == ShopStockAdjustmentType.Increase)
        {
            if (!expiryDate.HasValue)
                throw new BusinessException("ShopManagement:StockAdjustmentExpiryRequired").WithData("Product", product.Name);
            if (expiryDate.Value.Date <= adjustmentDate.Date)
                throw new BusinessException("ShopManagement:StockAdjustmentInvalidExpiryDate").WithData("Product", product.Name);
        }
    }

    private static void ValidateNoDuplicateProducts(IReadOnlyList<ShopStockAdjustmentItemInput> items)
    {
        var ids = items.Select(x => x.ProductId).ToList();
        if (ids.Distinct().Count() != ids.Count) throw new BusinessException("ShopManagement:StockAdjustmentDuplicateProduct");
    }

    private async Task<bool> ExistsAsync(Guid tenantId, Guid sourceItemId)
    {
        var query = await _stockTransactionRepository.GetQueryableAsync();
        return await AsyncExecuter.AnyAsync(query.Where(x =>
            x.TenantId == tenantId && x.ReferenceType == ShopStockReferenceType.StockAdjustment && x.SourceItemId == sourceItemId));
    }

    private Guid RequireTenantOwnership(ShopStockAdjustment adjustment)
    {
        var tenantId = RequireTenant();
        if (adjustment.TenantId != tenantId) throw new BusinessException("ShopManagement:StockAdjustmentNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}

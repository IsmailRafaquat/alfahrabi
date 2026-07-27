using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.ProductBatches;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.StockAdjustments;
using EHub.ShopManagement.Units;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace EHub.ShopManagement.StockCounts;

public class ShopStockCountManager : DomainService
{
    private const string DocumentType = "StockCount";
    private const string NumberPrefix = "SC-";

    private readonly IRepository<ShopStockCount, Guid> _repository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly IRepository<ShopProductCategory, Guid> _categoryRepository;
    private readonly IRepository<ShopProductBatch, Guid> _productBatchRepository;
    private readonly IRepository<ShopStockAdjustment, Guid> _stockAdjustmentRepository;
    private readonly ShopStockAdjustmentManager _stockAdjustmentManager;
    private readonly ShopDocumentNumberGenerator _numberGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ShopStockCountManager(
        IRepository<ShopStockCount, Guid> repository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        IRepository<ShopProductCategory, Guid> categoryRepository,
        IRepository<ShopProductBatch, Guid> productBatchRepository,
        IRepository<ShopStockAdjustment, Guid> stockAdjustmentRepository,
        ShopStockAdjustmentManager stockAdjustmentManager,
        ShopDocumentNumberGenerator numberGenerator,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _categoryRepository = categoryRepository;
        _productBatchRepository = productBatchRepository;
        _stockAdjustmentRepository = stockAdjustmentRepository;
        _stockAdjustmentManager = stockAdjustmentManager;
        _numberGenerator = numberGenerator;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<ShopStockCount> CreateAsync(
        DateTime countDate,
        ShopStockCountScope scope,
        Guid? productCategoryId,
        IReadOnlyList<Guid>? selectedProductIds,
        string? notes)
    {
        var tenantId = RequireTenant();
        var products = await ResolveProductsAsync(tenantId, scope, productCategoryId, selectedProductIds);

        var stockCountId = GuidGenerator.Create();
        var itemEntities = await BuildItemEntitiesAsync(stockCountId, tenantId, products);
        var number = await _numberGenerator.GetNextNumberAsync(tenantId, DocumentType, NumberPrefix);

        return new ShopStockCount(stockCountId, tenantId, number, countDate, scope, productCategoryId, notes, itemEntities);
    }

    public async Task UpdateAsync(
        ShopStockCount stockCount,
        DateTime countDate,
        ShopStockCountScope scope,
        Guid? productCategoryId,
        IReadOnlyList<Guid>? selectedProductIds,
        string? notes)
    {
        var tenantId = RequireTenantOwnership(stockCount);
        stockCount.EnsureEditable();

        var products = await ResolveProductsAsync(tenantId, scope, productCategoryId, selectedProductIds);
        var itemEntities = await BuildItemEntitiesAsync(stockCount.Id, tenantId, products);
        stockCount.Update(countDate, scope, productCategoryId, notes, itemEntities);
    }

    public Task StartAsync(ShopStockCount stockCount)
    {
        RequireTenantOwnership(stockCount);
        stockCount.MarkAsStarted(_currentUser.GetId(), Clock.Now);
        return Task.CompletedTask;
    }

    public async Task UpdateItemQuantityAsync(
        ShopStockCount stockCount,
        Guid stockCountItemId,
        decimal physicalQuantity,
        string? notes)
    {
        var tenantId = RequireTenantOwnership(stockCount);
        if (stockCount.Status != ShopStockCountStatus.InProgress)
            throw new BusinessException("ShopManagement:StockCountInvalidStatus");

        var item = stockCount.Items.FirstOrDefault(x => x.Id == stockCountItemId)
            ?? throw new BusinessException("ShopManagement:StockCountProductNotFound");

        var productQuery = await _productRepository.GetQueryableAsync();
        var product = productQuery.FirstOrDefault(x => x.Id == item.ProductId && x.TenantId == tenantId)
            ?? throw new BusinessException("ShopManagement:StockCountProductNotFound");

        var unitQuery = await _unitRepository.GetQueryableAsync();
        var unit = unitQuery.FirstOrDefault(x => x.Id == product.UnitId)
            ?? throw new BusinessException("ShopManagement:StockCountProductNotFound");

        item.SetPhysicalQuantity(physicalQuantity, notes, unit.AllowDecimal, _currentUser.GetId(), Clock.Now);
    }

    public Task CompleteCountAsync(ShopStockCount stockCount)
    {
        RequireTenantOwnership(stockCount);
        if (stockCount.Status != ShopStockCountStatus.InProgress)
            throw new BusinessException("ShopManagement:StockCountInvalidStatus");
        if (stockCount.Items.Any(x => !x.IsCounted))
            throw new BusinessException("ShopManagement:StockCountItemsNotCompleted");

        stockCount.MarkAsCounted(_currentUser.GetId(), Clock.Now);
        return Task.CompletedTask;
    }

    public async Task PostAsync(ShopStockCount stockCount)
    {
        var tenantId = RequireTenantOwnership(stockCount);
        stockCount.EnsurePostable();

        if (stockCount.GeneratedStockAdjustmentId.HasValue)
            throw new BusinessException("ShopManagement:StockCountAdjustmentAlreadyGenerated");

        var productIds = stockCount.Items.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);

        var batchIds = stockCount.Items.Where(x => x.ProductBatchId.HasValue).Select(x => x.ProductBatchId!.Value).Distinct().ToList();
        var batches = new Dictionary<Guid, ShopProductBatch>();
        if (batchIds.Count > 0)
        {
            var batchQuery = await _productBatchRepository.GetQueryableAsync();
            batches = batchQuery.Where(x => batchIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);
        }

        var changedProducts = new List<string>();
        foreach (var item in stockCount.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
                throw new BusinessException("ShopManagement:StockCountProductNotFound");
            if (!product.IsActive)
                throw new BusinessException("ShopManagement:StockCountProductInactive").WithData("Product", product.Name);

            if (item.ProductBatchId.HasValue)
            {
                if (!batches.TryGetValue(item.ProductBatchId.Value, out var batch))
                    throw new BusinessException("ShopManagement:BatchNotFound");
                if (batch.AvailableQuantity != item.SystemQuantitySnapshot)
                    changedProducts.Add($"{product.Name} ({item.BatchNumberSnapshot})");
            }
            else if (product.CurrentStock != item.SystemQuantitySnapshot)
            {
                changedProducts.Add(product.Name);
            }
        }

        if (changedProducts.Count > 0)
            throw new BusinessException("ShopManagement:StockCountStockChanged").WithData("Products", string.Join(", ", changedProducts));

        var adjustmentItems = stockCount.Items
            .Where(x => x.DifferenceQuantity != 0 && x.AdjustmentType.HasValue)
            .Select(x => new ShopStockAdjustmentItemInput
            {
                ProductId = x.ProductId,
                AdjustmentType = x.AdjustmentType!.Value,
                AdjustmentQuantity = Math.Abs(x.DifferenceQuantity),
                BatchNumber = x.BatchNumberSnapshot,
                ExpiryDate = x.ExpiryDateSnapshot,
                ProductBatchId = x.ProductBatchId,
                Reason = ShopStockAdjustmentReason.CountingCorrection,
                Notes = x.Notes,
            })
            .ToList();

        var postedByUserId = _currentUser.GetId();
        var postedDate = Clock.Now;
        Guid? generatedStockAdjustmentId = null;

        if (adjustmentItems.Count > 0)
        {
            var adjustment = await _stockAdjustmentManager.CreateAsync(
                postedDate, ShopStockAdjustmentReason.CountingCorrection,
                $"Generated from Stock Count {stockCount.StockCountNumber}", null, adjustmentItems);
            await _stockAdjustmentRepository.InsertAsync(adjustment, autoSave: true);

            await _stockAdjustmentManager.PostAsync(adjustment);
            await _stockAdjustmentRepository.UpdateAsync(adjustment, autoSave: true);

            generatedStockAdjustmentId = adjustment.Id;
        }

        stockCount.MarkAsPosted(postedByUserId, postedDate, generatedStockAdjustmentId);
    }

    public Task CancelAsync(ShopStockCount stockCount, string cancellationReason)
    {
        RequireTenantOwnership(stockCount);
        stockCount.MarkAsCancelled(_currentUser.GetId(), Clock.Now, cancellationReason);
        return Task.CompletedTask;
    }

    public Task ValidateDeleteAsync(ShopStockCount stockCount)
    {
        RequireTenantOwnership(stockCount);
        stockCount.EnsureDeletable();
        return Task.CompletedTask;
    }

    private async Task<List<ShopProduct>> ResolveProductsAsync(
        Guid tenantId, ShopStockCountScope scope, Guid? productCategoryId, IReadOnlyList<Guid>? selectedProductIds)
    {
        var productQuery = await _productRepository.GetQueryableAsync();
        List<ShopProduct> products;
        var blockIncompatibleProducts = false;

        switch (scope)
        {
            case ShopStockCountScope.AllProducts:
                products = productQuery.Where(x => x.TenantId == tenantId && x.IsActive).ToList();
                break;

            case ShopStockCountScope.Category:
                if (!productCategoryId.HasValue)
                    throw new BusinessException("ShopManagement:StockCountCategoryRequired");

                var categoryQuery = await _categoryRepository.GetQueryableAsync();
                var categoryExists = categoryQuery.Any(x => x.Id == productCategoryId.Value && x.TenantId == tenantId);
                if (!categoryExists) throw new BusinessException("ShopManagement:StockCountCategoryRequired");

                products = productQuery.Where(x => x.TenantId == tenantId && x.IsActive && x.CategoryId == productCategoryId.Value).ToList();
                break;

            case ShopStockCountScope.SelectedProducts:
                if (selectedProductIds == null || selectedProductIds.Count == 0)
                    throw new BusinessException("ShopManagement:StockCountRequiresProducts");

                var ids = selectedProductIds.ToList();
                if (ids.Distinct().Count() != ids.Count)
                    throw new BusinessException("ShopManagement:StockCountDuplicateProduct");

                products = productQuery.Where(x => ids.Contains(x.Id) && x.TenantId == tenantId).ToList();
                if (products.Count != ids.Count)
                    throw new BusinessException("ShopManagement:StockCountProductNotFound");
                foreach (var product in products)
                {
                    if (!product.IsActive)
                        throw new BusinessException("ShopManagement:StockCountProductInactive").WithData("Product", product.Name);
                }

                blockIncompatibleProducts = true;
                break;

            default:
                throw new BusinessException("ShopManagement:StockCountInvalidStatus");
        }

        if (blockIncompatibleProducts)
        {
            // The user explicitly chose these products, so an incompatible one is a mistake worth
            // surfacing rather than silently dropping. Batch-tracked products are fine now - they are
            // expanded into one row per batch by BuildItemEntitiesAsync.
            foreach (var product in products)
            {
                if (product.TrackSerialNumber)
                    throw new BusinessException("ShopManagement:StockCountSerialTrackingNotSupported").WithData("Product", product.Name);
            }
        }
        else
        {
            // Bulk scopes (AllProducts/Category) simply exclude serial-tracked products rather than
            // failing the whole count over a product the user did not explicitly pick.
            products = products.Where(x => !x.TrackSerialNumber).ToList();
        }

        if (products.Count == 0) throw new BusinessException("ShopManagement:StockCountRequiresProducts");

        return products;
    }

    private async Task<List<ShopStockCountItem>> BuildItemEntitiesAsync(Guid stockCountId, Guid tenantId, List<ShopProduct> products)
    {
        var unitIds = products.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var batchTrackedProductIds = products.Where(x => x.TrackBatch).Select(x => x.Id).ToList();
        var batchesByProduct = new Dictionary<Guid, List<ShopProductBatch>>();
        if (batchTrackedProductIds.Count > 0)
        {
            var batchQuery = await _productBatchRepository.GetQueryableAsync();
            var batches = batchQuery.Where(x => x.TenantId == tenantId && batchTrackedProductIds.Contains(x.ProductId) && !x.IsBlocked).ToList();
            batchesByProduct = batches.GroupBy(x => x.ProductId).ToDictionary(g => g.Key, g => g.ToList());
        }

        var items = new List<ShopStockCountItem>();
        foreach (var product in products)
        {
            if (!units.TryGetValue(product.UnitId, out var unit))
                throw new BusinessException("ShopManagement:StockCountProductNotFound");

            if (product.TrackBatch)
            {
                // A batch-tracked product is counted batch-by-batch, not as one product-level row.
                // A product with no batch history yet has nothing to count and is simply skipped.
                if (!batchesByProduct.TryGetValue(product.Id, out var productBatches)) continue;
                foreach (var batch in productBatches)
                {
                    items.Add(new ShopStockCountItem(GuidGenerator.Create(), tenantId, stockCountId, product, unit.Name, unit.ShortName, batch.AvailableQuantity, batch));
                }
            }
            else
            {
                items.Add(new ShopStockCountItem(GuidGenerator.Create(), tenantId, stockCountId, product, unit.Name, unit.ShortName, product.CurrentStock));
            }
        }

        if (items.Count == 0) throw new BusinessException("ShopManagement:StockCountRequiresProducts");

        return items;
    }

    private Guid RequireTenantOwnership(ShopStockCount stockCount)
    {
        var tenantId = RequireTenant();
        if (stockCount.TenantId != tenantId) throw new BusinessException("ShopManagement:StockCountNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}

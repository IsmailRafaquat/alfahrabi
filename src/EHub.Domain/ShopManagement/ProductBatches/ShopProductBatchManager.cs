using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.Units;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.ProductBatches;

public class ShopProductBatchManager : DomainService
{
    private readonly IRepository<ShopProductBatch, Guid> _repository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly ICurrentTenant _currentTenant;

    public ShopProductBatchManager(
        IRepository<ShopProductBatch, Guid> repository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _currentTenant = currentTenant;
    }

    public async Task<ShopProductBatch> FindOrCreateBatchAsync(
        Guid productId,
        string batchNumber,
        DateTime? manufacturingDate,
        DateTime? expiryDate,
        Guid? supplierId,
        Guid? goodsReceiptId,
        Guid? goodsReceiptItemId)
    {
        var tenantId = RequireTenant();
        var product = await GetProductAsync(productId, tenantId);

        if (!product.TrackBatch) throw new BusinessException("ShopManagement:BatchNumberRequired");
        var normalized = NormalizeBatchNumber(batchNumber);
        if (string.IsNullOrWhiteSpace(normalized)) throw new BusinessException("ShopManagement:BatchNumberRequired");

        ValidateExpiry(product, manufacturingDate, expiryDate);

        var query = await _repository.GetQueryableAsync();
        var existing = query.FirstOrDefault(x => x.TenantId == tenantId && x.ProductId == productId && x.NormalizedBatchNumber == normalized);

        if (existing != null)
        {
            existing.EnsureExpiryConsistent(manufacturingDate, expiryDate);
            return existing;
        }

        var batch = new ShopProductBatch(GuidGenerator.Create(), tenantId, productId, batchNumber.Trim(), normalized,
            manufacturingDate, expiryDate, supplierId, goodsReceiptId, goodsReceiptItemId);
        await _repository.InsertAsync(batch, autoSave: true);
        return batch;
    }

    public async Task UpdateMetadataAsync(ShopProductBatch batch, DateTime? manufacturingDate, DateTime? expiryDate, string? notes)
    {
        var tenantId = RequireTenant();
        RequireOwnership(batch, tenantId);

        batch.UpdateMetadata(manufacturingDate, expiryDate, notes);
        await _repository.UpdateAsync(batch, autoSave: true);
    }

    public async Task BlockAsync(ShopProductBatch batch, string reason)
    {
        var tenantId = RequireTenant();
        RequireOwnership(batch, tenantId);

        batch.Block(reason);
        await _repository.UpdateAsync(batch, autoSave: true);
    }

    public async Task UnblockBatchAsync(ShopProductBatch batch)
    {
        var tenantId = RequireTenant();
        RequireOwnership(batch, tenantId);

        batch.Unblock();
        await RefreshStatusAsync(batch);
    }

    public async Task AddStockAsync(ShopProductBatch batch, decimal quantity, decimal unitCost, DateTime movementDate)
    {
        var tenantId = RequireTenant();
        RequireOwnership(batch, tenantId);

        batch.AddStock(quantity, unitCost, movementDate);
        await RefreshStatusAsync(batch);
    }

    public async Task RemoveStockAsync(ShopProductBatch batch, decimal quantity, DateTime movementDate, bool allowExpired = false)
    {
        var tenantId = RequireTenant();
        RequireOwnership(batch, tenantId);

        if (batch.IsBlocked) throw new BusinessException("ShopManagement:BatchBlocked").WithData("BatchNumber", batch.BatchNumber);
        if (!allowExpired && batch.ExpiryDate.HasValue && batch.ExpiryDate.Value.Date < movementDate.Date)
            throw new BusinessException("ShopManagement:BatchExpired").WithData("BatchNumber", batch.BatchNumber);

        batch.RemoveStock(quantity, movementDate);
        await RefreshStatusAsync(batch);
    }

    public async Task<IReadOnlyList<ShopBatchAllocationResult>> AllocateAsync(Guid productId, decimal requiredQuantity, DateTime transactionDate)
    {
        var tenantId = RequireTenant();
        var product = await GetProductAsync(productId, tenantId);

        var eligible = await GetEligibleBatchesAsync(product, tenantId, transactionDate, allowExpired: !product.BlockExpiredSale);

        var results = new List<ShopBatchAllocationResult>();
        var remaining = requiredQuantity;

        foreach (var batch in eligible)
        {
            if (remaining <= 0) break;
            var take = Math.Min(remaining, batch.AvailableQuantity);
            if (take <= 0) continue;

            results.Add(new ShopBatchAllocationResult
            {
                ProductBatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                ExpiryDate = batch.ExpiryDate,
                Quantity = take,
                UnitCost = batch.UnitCost,
            });
            remaining -= take;
        }

        if (remaining > 0) throw new BusinessException("ShopManagement:NoValidBatchAvailable").WithData("Product", product.Name);

        return results;
    }

    public async Task ValidateManualAllocationsAsync(Guid productId, decimal requiredQuantity, IReadOnlyList<ShopBatchAllocationInput> allocations)
    {
        var tenantId = RequireTenant();
        var product = await GetProductAsync(productId, tenantId);
        var unit = await _unitRepository.GetAsync(product.UnitId);

        if (allocations == null || allocations.Count == 0) throw new BusinessException("ShopManagement:BatchAllocationRequired");

        var batchIds = allocations.Select(x => x.ProductBatchId).ToList();
        if (batchIds.Distinct().Count() != batchIds.Count) throw new BusinessException("ShopManagement:BatchAllocationMismatch");

        var query = await _repository.GetQueryableAsync();
        var batches = query.Where(x => batchIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        decimal total = 0;
        foreach (var allocation in allocations)
        {
            if (allocation.Quantity <= 0) throw new BusinessException("ShopManagement:BatchQuantityCannotBeNegative");
            if (!unit.AllowDecimal && allocation.Quantity != Math.Truncate(allocation.Quantity))
                throw new BusinessException("ShopManagement:BatchDecimalQuantityNotAllowed").WithData("Unit", unit.Name);
            if (!batches.TryGetValue(allocation.ProductBatchId, out var batch)) throw new BusinessException("ShopManagement:BatchNotFound");
            if (batch.TenantId != tenantId) throw new BusinessException("ShopManagement:BatchTenantMismatch");
            if (batch.ProductId != productId) throw new BusinessException("ShopManagement:BatchProductMismatch");
            if (batch.IsBlocked) throw new BusinessException("ShopManagement:BatchBlocked").WithData("BatchNumber", batch.BatchNumber);
            if (product.BlockExpiredSale && batch.ExpiryDate.HasValue && batch.ExpiryDate.Value.Date < Clock.Now.Date)
                throw new BusinessException("ShopManagement:BatchExpired").WithData("BatchNumber", batch.BatchNumber);
            if (allocation.Quantity > batch.AvailableQuantity)
                throw new BusinessException("ShopManagement:BatchAllocationExceedsAvailableStock").WithData("BatchNumber", batch.BatchNumber);

            total += allocation.Quantity;
        }

        if (total != requiredQuantity) throw new BusinessException("ShopManagement:BatchAllocationMismatch");
    }

    public async Task ReturnStockAsync(ShopProductBatch batch, decimal quantity, DateTime movementDate)
    {
        var tenantId = RequireTenant();
        RequireOwnership(batch, tenantId);

        // Returning stock is the inverse of RemoveStock: it gives quantity back to the batch without
        // going through AddStock (which would incorrectly inflate ReceivedQuantity / re-cost the batch).
        batch.ReturnIssuedStock(quantity, movementDate);
        await RefreshStatusAsync(batch);
    }

    public async Task RefreshStatusAsync(ShopProductBatch batch)
    {
        var product = await _productRepository.GetAsync(batch.ProductId);
        var status = ComputeStatus(batch, product);
        batch.SetStatus(status);
        await _repository.UpdateAsync(batch, autoSave: true);
    }

    public async Task RefreshProductStockAsync(Guid productId)
    {
        var tenantId = RequireTenant();
        var product = await GetProductAsync(productId, tenantId);
        if (!product.TrackBatch) return;

        var query = await _repository.GetQueryableAsync();
        var totalAvailable = query.Where(x => x.TenantId == tenantId && x.ProductId == productId).Sum(x => (decimal?)x.AvailableQuantity) ?? 0;

        var difference = totalAvailable - product.CurrentStock;
        if (difference > 0) product.IncreaseStock(difference);
        else if (difference < 0) product.DecreaseStock(-difference);

        if (difference != 0) await _productRepository.UpdateAsync(product, autoSave: true);
    }

    public async Task<IReadOnlyList<ShopProductBatch>> RefreshAllStatusesAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var batches = query.Where(x => !x.IsBlocked).ToList();

        var productIds = batches.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var changed = new List<ShopProductBatch>();
        foreach (var batch in batches)
        {
            if (!products.TryGetValue(batch.ProductId, out var product)) continue;
            var status = ComputeStatus(batch, product);
            if (status == batch.Status) continue;
            batch.SetStatus(status);
            await _repository.UpdateAsync(batch, autoSave: true);
            changed.Add(batch);
        }

        return changed;
    }

    private static ShopProductBatchStatus ComputeStatus(ShopProductBatch batch, ShopProduct product)
    {
        if (batch.IsBlocked) return ShopProductBatchStatus.Blocked;
        if (batch.AvailableQuantity <= 0) return ShopProductBatchStatus.Exhausted;

        if (product.TrackExpiry && batch.ExpiryDate.HasValue)
        {
            var today = DateTime.Today;
            var alertDays = product.ExpiryAlertDays ?? 30;
            if (batch.ExpiryDate.Value.Date < today) return ShopProductBatchStatus.Expired;
            if (batch.ExpiryDate.Value.Date <= today.AddDays(alertDays)) return ShopProductBatchStatus.NearExpiry;
        }

        return ShopProductBatchStatus.Active;
    }

    private async Task<List<ShopProductBatch>> GetEligibleBatchesAsync(ShopProduct product, Guid tenantId, DateTime transactionDate, bool allowExpired = false)
    {
        var query = await _repository.GetQueryableAsync();
        var batches = query.Where(x =>
            x.TenantId == tenantId && x.ProductId == product.Id && x.AvailableQuantity > 0 && !x.IsBlocked).ToList();

        if (!allowExpired)
            batches = batches.Where(x => !x.ExpiryDate.HasValue || x.ExpiryDate.Value.Date >= transactionDate.Date).ToList();

        return product.TrackExpiry
            ? batches.OrderBy(x => x.ExpiryDate ?? DateTime.MaxValue).ThenBy(x => x.FirstReceivedDate ?? DateTime.MaxValue).ThenBy(x => x.CreationTime).ToList()
            : batches.OrderBy(x => x.FirstReceivedDate ?? DateTime.MaxValue).ThenBy(x => x.CreationTime).ToList();
    }

    private static void ValidateExpiry(ShopProduct product, DateTime? manufacturingDate, DateTime? expiryDate)
    {
        if (product.TrackExpiry && !expiryDate.HasValue) throw new BusinessException("ShopManagement:BatchExpiryRequired");
        if (manufacturingDate.HasValue && expiryDate.HasValue && manufacturingDate.Value.Date > expiryDate.Value.Date)
            throw new BusinessException("ShopManagement:InvalidManufacturingDate");
    }

    private static string NormalizeBatchNumber(string batchNumber) => (batchNumber ?? string.Empty).Trim().ToUpperInvariant();

    private async Task<ShopProduct> GetProductAsync(Guid productId, Guid tenantId)
    {
        var query = await _productRepository.GetQueryableAsync();
        return query.FirstOrDefault(x => x.Id == productId && x.TenantId == tenantId)
            ?? throw new BusinessException("ShopManagement:BatchProductMismatch");
    }

    private static void RequireOwnership(ShopProductBatch batch, Guid tenantId)
    {
        if (batch.TenantId != tenantId) throw new BusinessException("ShopManagement:BatchTenantMismatch");
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}

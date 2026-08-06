using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.StockTransactions;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.Units;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.ProductBatches;

[Authorize(EHubPermissions.ShopProductBatches.View)]
public class ShopProductBatchAppService : ApplicationService, IShopProductBatchAppService
{
    private readonly IRepository<ShopProductBatch, Guid> _repository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly IRepository<ShopSupplier, Guid> _supplierRepository;
    private readonly IRepository<ShopGoodsReceipt, Guid> _goodsReceiptRepository;
    private readonly IRepository<ShopStockTransaction, Guid> _stockTransactionRepository;
    private readonly ShopProductBatchManager _manager;

    public ShopProductBatchAppService(
        IRepository<ShopProductBatch, Guid> repository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        IRepository<ShopSupplier, Guid> supplierRepository,
        IRepository<ShopGoodsReceipt, Guid> goodsReceiptRepository,
        IRepository<ShopStockTransaction, Guid> stockTransactionRepository,
        ShopProductBatchManager manager)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _supplierRepository = supplierRepository;
        _goodsReceiptRepository = goodsReceiptRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopProductBatchDto>> GetListAsync(GetShopProductBatchesInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopProductBatchDto>(0, new List<ShopProductBatchDto>());
        var tenantId = CurrentTenant.Id.Value;

        var batchQuery = await _repository.GetQueryableAsync();
        var productQuery = await _productRepository.GetQueryableAsync();
        var supplierQuery = await _supplierRepository.GetQueryableAsync();

        var canViewExpired = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopProductBatches.ViewExpired);

        var filtered = batchQuery.Where(x => x.TenantId == tenantId)
            .WhereIf(input.ProductId.HasValue, x => x.ProductId == input.ProductId)
            .WhereIf(input.SupplierId.HasValue, x => x.SupplierId == input.SupplierId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(!input.BatchNumber.IsNullOrWhiteSpace(), x => x.BatchNumber.Contains(input.BatchNumber!))
            .WhereIf(input.ExpiryFrom.HasValue, x => x.ExpiryDate != null && x.ExpiryDate >= input.ExpiryFrom!.Value)
            .WhereIf(input.ExpiryTo.HasValue, x => x.ExpiryDate != null && x.ExpiryDate <= input.ExpiryTo!.Value)
            .WhereIf(input.NearExpiryOnly == true, x => x.Status == ShopProductBatchStatus.NearExpiry)
            .WhereIf(input.ExpiredOnly == true, x => x.Status == ShopProductBatchStatus.Expired)
            .WhereIf(input.HasAvailableStock == true, x => x.AvailableQuantity > 0)
            .WhereIf(input.HasAvailableStock == false, x => x.AvailableQuantity <= 0)
            .WhereIf(!canViewExpired, x => x.Status != ShopProductBatchStatus.Expired);

        if (input.ProductCategoryId.HasValue)
        {
            var categoryProductIds = productQuery.Where(x => x.TenantId == tenantId && x.CategoryId == input.ProductCategoryId).Select(x => x.Id);
            filtered = filtered.Where(x => categoryProductIds.Contains(x.ProductId));
        }

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var matchingProductIds = await AsyncExecuter.ToListAsync(productQuery
                .Where(x => x.TenantId == tenantId && (x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!)))
                .Select(x => x.Id));
            var matchingSupplierIds = await AsyncExecuter.ToListAsync(supplierQuery
                .Where(x => x.TenantId == tenantId && x.Name.Contains(input.Filter!))
                .Select(x => x.Id));

            filtered = filtered.Where(x =>
                x.BatchNumber.Contains(input.Filter!) ||
                (x.Notes != null && x.Notes.Contains(input.Filter!)) ||
                matchingProductIds.Contains(x.ProductId) ||
                (x.SupplierId.HasValue && matchingSupplierIds.Contains(x.SupplierId.Value)));
        }

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "ExpiryDate asc, CreationTime desc" : input.Sorting!;
        var pagedBatches = await AsyncExecuter.ToListAsync(filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = await BuildDtosAsync(pagedBatches);
        return new PagedResultDto<ShopProductBatchDto>(totalCount, dtos);
    }

    public async Task<ShopProductBatchDto> GetAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        var dtos = await BuildDtosAsync(new List<ShopProductBatch> { entity });
        return dtos[0];
    }

    [Authorize(EHubPermissions.ShopProductBatches.EditMetadata)]
    public async Task<ShopProductBatchDto> UpdateAsync(Guid id, UpdateShopProductBatchDto input)
    {
        var entity = await FindEntityAsync(id);
        await _manager.UpdateMetadataAsync(entity, input.ManufacturingDate, input.ExpiryDate, input.Notes);
        return await GetAsync(id);
    }

    public async Task<ListResultDto<ShopProductBatchLookupDto>> GetAvailableBatchesAsync(Guid productId)
    {
        var tenantId = RequireTenant();
        var query = await _repository.GetQueryableAsync();
        var batches = query.Where(x =>
                x.TenantId == tenantId && x.ProductId == productId && x.AvailableQuantity > 0 && !x.IsBlocked &&
                (x.ExpiryDate == null || x.ExpiryDate >= DateTime.Today))
            .OrderBy(x => x.ExpiryDate ?? DateTime.MaxValue).ThenBy(x => x.FirstReceivedDate ?? DateTime.MaxValue)
            .ToList();

        var result = batches.Select(ToLookupDto).ToList();
        return new ListResultDto<ShopProductBatchLookupDto>(result);
    }

    public async Task<ShopBatchAvailabilityDto> CheckAvailabilityAsync(Guid productId, decimal requiredQuantity)
    {
        var tenantId = RequireTenant();
        var query = await _repository.GetQueryableAsync();
        var batches = query.Where(x =>
                x.TenantId == tenantId && x.ProductId == productId && x.AvailableQuantity > 0 && !x.IsBlocked &&
                (x.ExpiryDate == null || x.ExpiryDate >= DateTime.Today))
            .OrderBy(x => x.ExpiryDate ?? DateTime.MaxValue).ThenBy(x => x.FirstReceivedDate ?? DateTime.MaxValue)
            .ToList();

        var totalAvailable = batches.Sum(x => x.AvailableQuantity);

        return new ShopBatchAvailabilityDto
        {
            ProductId = productId,
            RequiredQuantity = requiredQuantity,
            TotalAvailableQuantity = totalAvailable,
            IsSufficient = totalAvailable >= requiredQuantity,
            Batches = batches.Select(ToLookupDto).ToList()
        };
    }

    [Authorize(EHubPermissions.ShopProductBatches.ViewTransactions)]
    public async Task<ListResultDto<ShopStockTransactionDto>> GetTransactionsAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        var product = await _productRepository.FindAsync(entity.ProductId);
        var canViewCost = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopProductBatches.ViewCost);

        var query = await _stockTransactionRepository.GetQueryableAsync();
        var transactions = query.Where(x => x.TenantId == entity.TenantId && x.ProductBatchId == id)
            .OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.CreationTime)
            .ToList();

        var dtos = transactions.Select(t => new ShopStockTransactionDto
        {
            Id = t.Id,
            ProductId = t.ProductId,
            ProductName = product?.Name ?? string.Empty,
            ProductCode = product?.Code ?? string.Empty,
            TransactionType = t.TransactionType,
            ReferenceType = t.ReferenceType,
            ReferenceId = t.ReferenceId,
            ReferenceNumber = t.ReferenceNumber,
            TransactionDate = t.TransactionDate,
            QuantityIn = t.QuantityIn,
            QuantityOut = t.QuantityOut,
            BalanceQuantity = t.BalanceQuantity,
            UnitCost = canViewCost ? t.UnitCost : null,
            TotalCost = canViewCost ? t.TotalCost : null,
            BatchNumber = t.BatchNumber,
            ExpiryDate = t.ExpiryDate,
            ProductBatchId = t.ProductBatchId,
            BatchBalanceQuantity = t.BatchBalanceQuantity,
            Notes = t.Notes,
            CreationTime = t.CreationTime
        }).ToList();

        return new ListResultDto<ShopStockTransactionDto>(dtos);
    }

    public async Task<ShopBatchSummaryDto> GetSummaryAsync(Guid productId)
    {
        var tenantId = RequireTenant();
        var query = await _repository.GetQueryableAsync();
        var batches = query.Where(x => x.TenantId == tenantId && x.ProductId == productId).ToList();

        return new ShopBatchSummaryDto
        {
            ProductId = productId,
            TotalBatches = batches.Count,
            ActiveBatches = batches.Count(x => x.Status == ShopProductBatchStatus.Active),
            NearExpiryBatches = batches.Count(x => x.Status == ShopProductBatchStatus.NearExpiry),
            ExpiredBatches = batches.Count(x => x.Status == ShopProductBatchStatus.Expired),
            ExhaustedBatches = batches.Count(x => x.Status == ShopProductBatchStatus.Exhausted),
            BlockedBatches = batches.Count(x => x.Status == ShopProductBatchStatus.Blocked),
            TotalReceivedQuantity = batches.Sum(x => x.ReceivedQuantity),
            TotalIssuedQuantity = batches.Sum(x => x.IssuedQuantity),
            TotalAvailableQuantity = batches.Sum(x => x.AvailableQuantity),
            NearestExpiryDate = batches.Where(x => x.ExpiryDate.HasValue && x.AvailableQuantity > 0).Select(x => x.ExpiryDate).OrderBy(x => x).FirstOrDefault()
        };
    }

    [Authorize(EHubPermissions.ShopProductBatches.Block)]
    public async Task<ShopProductBatchDto> BlockAsync(Guid id, BlockShopProductBatchDto input)
    {
        var entity = await FindEntityAsync(id);
        await _manager.BlockAsync(entity, input.BlockReason);
        return await GetAsync(id);
    }

    [Authorize(EHubPermissions.ShopProductBatches.Unblock)]
    public async Task<ShopProductBatchDto> UnblockAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        await _manager.UnblockBatchAsync(entity);
        return await GetAsync(id);
    }

    public async Task RefreshStatusesAsync()
    {
        await _manager.RefreshAllStatusesAsync();
    }

    private async Task<List<ShopProductBatchDto>> BuildDtosAsync(List<ShopProductBatch> entities)
    {
        var productIds = entities.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);
        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var supplierIds = entities.Where(x => x.SupplierId.HasValue).Select(x => x.SupplierId!.Value).Distinct().ToList();
        var supplierQuery = await _supplierRepository.GetQueryableAsync();
        var suppliers = supplierQuery.Where(x => supplierIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var goodsReceiptIds = entities.Where(x => x.GoodsReceiptId.HasValue).Select(x => x.GoodsReceiptId!.Value).Distinct().ToList();
        var goodsReceiptQuery = await _goodsReceiptRepository.GetQueryableAsync();
        var goodsReceipts = goodsReceiptQuery.Where(x => goodsReceiptIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var canViewCost = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopProductBatches.ViewCost);

        return entities.Select(entity =>
        {
            products.TryGetValue(entity.ProductId, out var product);
            ShopUnit? unit = null;
            if (product != null) units.TryGetValue(product.UnitId, out unit);
            ShopSupplier? supplier = null;
            if (entity.SupplierId.HasValue) suppliers.TryGetValue(entity.SupplierId.Value, out supplier);
            ShopGoodsReceipt? goodsReceipt = null;
            if (entity.GoodsReceiptId.HasValue) goodsReceipts.TryGetValue(entity.GoodsReceiptId.Value, out goodsReceipt);

            return new ShopProductBatchDto
            {
                Id = entity.Id,
                ProductId = entity.ProductId,
                ProductCode = product?.Code ?? string.Empty,
                ProductName = product?.Name ?? string.Empty,
                UnitName = unit?.Name ?? string.Empty,
                UnitShortName = unit?.ShortName ?? string.Empty,
                BatchNumber = entity.BatchNumber,
                ManufacturingDate = entity.ManufacturingDate,
                ExpiryDate = entity.ExpiryDate,
                ReceivedQuantity = entity.ReceivedQuantity,
                IssuedQuantity = entity.IssuedQuantity,
                ReservedQuantity = entity.ReservedQuantity,
                AvailableQuantity = entity.AvailableQuantity,
                UnitCost = canViewCost ? entity.UnitCost : null,
                Status = entity.Status,
                DaysToExpiry = entity.ExpiryDate.HasValue ? (int)(entity.ExpiryDate.Value.Date - DateTime.Today).TotalDays : null,
                FirstReceivedDate = entity.FirstReceivedDate,
                LastMovementDate = entity.LastMovementDate,
                SupplierId = entity.SupplierId,
                SupplierName = supplier?.Name,
                GoodsReceiptId = entity.GoodsReceiptId,
                GoodsReceiptNumber = goodsReceipt?.GoodsReceiptNumber,
                Notes = entity.Notes,
                IsBlocked = entity.IsBlocked,
                BlockReason = entity.BlockReason,
                CreationTime = entity.CreationTime
            };
        }).ToList();
    }

    private static ShopProductBatchLookupDto ToLookupDto(ShopProductBatch batch) => new()
    {
        Id = batch.Id,
        ProductId = batch.ProductId,
        BatchNumber = batch.BatchNumber,
        ExpiryDate = batch.ExpiryDate,
        DaysToExpiry = batch.ExpiryDate.HasValue ? (int)(batch.ExpiryDate.Value.Date - DateTime.Today).TotalDays : null,
        AvailableQuantity = batch.AvailableQuantity,
        Status = batch.Status,
        IsBlocked = batch.IsBlocked
    };

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopProductBatch> FindEntityAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:BatchNotFound");
    }
}

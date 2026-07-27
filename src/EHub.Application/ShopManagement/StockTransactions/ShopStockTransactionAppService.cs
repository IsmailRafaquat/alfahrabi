using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Products;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.StockTransactions;

[Authorize(EHubPermissions.ShopStockTransactions.Default)]
public class ShopStockTransactionAppService : ApplicationService, IShopStockTransactionAppService
{
    private readonly IRepository<ShopStockTransaction, Guid> _repository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;

    public ShopStockTransactionAppService(
        IRepository<ShopStockTransaction, Guid> repository,
        IRepository<ShopProduct, Guid> productRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
    }

    public async Task<PagedResultDto<ShopStockTransactionDto>> GetListAsync(GetShopStockTransactionsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopStockTransactionDto>(0, new List<ShopStockTransactionDto>());
        var tenantId = CurrentTenant.Id.Value;

        var transactions = await _repository.GetQueryableAsync();
        var products = await _productRepository.GetQueryableAsync();

        var filtered = transactions.Where(x => x.TenantId == tenantId)
            .WhereIf(input.ProductId.HasValue, x => x.ProductId == input.ProductId)
            .WhereIf(input.TransactionType.HasValue, x => x.TransactionType == input.TransactionType)
            .WhereIf(input.ReferenceType.HasValue, x => x.ReferenceType == input.ReferenceType)
            .WhereIf(input.DateFrom.HasValue, x => x.TransactionDate >= input.DateFrom!.Value)
            .WhereIf(input.DateTo.HasValue, x => x.TransactionDate <= input.DateTo!.Value)
            .WhereIf(!input.ReferenceNumber.IsNullOrWhiteSpace(), x => x.ReferenceNumber.Contains(input.ReferenceNumber!));

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var matchingProductIds = await AsyncExecuter.ToListAsync(products
                .Where(x => x.TenantId == tenantId && (x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!)))
                .Select(x => x.Id));

            filtered = filtered.Where(x => x.ReferenceNumber.Contains(input.Filter!) || matchingProductIds.Contains(x.ProductId));
        }

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "TransactionDate desc, CreationTime desc" : input.Sorting!;
        var pagedTransactions = filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount);

        var projected = from t in pagedTransactions
                        join product in products on t.ProductId equals product.Id
                        select MapProjection(t, product);

        var items = await AsyncExecuter.ToListAsync(projected);
        await HideCostIfNotAllowedAsync(items);
        return new PagedResultDto<ShopStockTransactionDto>(totalCount, items);
    }

    public async Task<ShopStockTransactionDto> GetAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var transactions = await _repository.GetQueryableAsync();
        var products = await _productRepository.GetQueryableAsync();

        var query = from t in transactions.Where(x => x.Id == id && x.TenantId == tenantId)
                    join product in products on t.ProductId equals product.Id
                    select MapProjection(t, product);

        var dto = await AsyncExecuter.FirstOrDefaultAsync(query) ?? throw new BusinessException("ShopManagement:StockTransactionNotFound");
        await HideCostIfNotAllowedAsync(new List<ShopStockTransactionDto> { dto });
        return dto;
    }

    private static ShopStockTransactionDto MapProjection(ShopStockTransaction t, ShopProduct product) => new()
    {
        Id = t.Id,
        ProductId = t.ProductId,
        ProductName = product.Name,
        ProductCode = product.Code,
        TransactionType = t.TransactionType,
        ReferenceType = t.ReferenceType,
        ReferenceId = t.ReferenceId,
        ReferenceNumber = t.ReferenceNumber,
        TransactionDate = t.TransactionDate,
        QuantityIn = t.QuantityIn,
        QuantityOut = t.QuantityOut,
        BalanceQuantity = t.BalanceQuantity,
        UnitCost = t.UnitCost,
        TotalCost = t.TotalCost,
        BatchNumber = t.BatchNumber,
        ExpiryDate = t.ExpiryDate,
        ProductBatchId = t.ProductBatchId,
        BatchBalanceQuantity = t.BatchBalanceQuantity,
        Notes = t.Notes,
        CreationTime = t.CreationTime
    };

    private async Task HideCostIfNotAllowedAsync(List<ShopStockTransactionDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopStockTransactions.ViewCost)) return;
        foreach (var dto in items)
        {
            dto.UnitCost = null;
            dto.TotalCost = null;
        }
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}

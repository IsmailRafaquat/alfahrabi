using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.Units;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.Sales;

[Authorize(EHubPermissions.ShopSales.Default)]
public class ShopSaleAppService : ApplicationService, IShopSaleAppService
{
    private readonly IRepository<ShopSale, Guid> _repository;
    private readonly IRepository<ShopCustomer, Guid> _customerRepository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly ShopSaleManager _manager;

    public ShopSaleAppService(
        IRepository<ShopSale, Guid> repository,
        IRepository<ShopCustomer, Guid> customerRepository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        ShopSaleManager manager)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopSaleDto>> GetListAsync(GetShopSalesInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopSaleDto>(0, new List<ShopSaleDto>());
        var tenantId = CurrentTenant.Id.Value;

        var sales = await _repository.GetQueryableAsync();
        var customers = await _customerRepository.GetQueryableAsync();

        var filtered = sales.Where(x => x.TenantId == tenantId)
            .WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.SaleType.HasValue, x => x.SaleType == input.SaleType)
            .WhereIf(input.SaleDateFrom.HasValue, x => x.SaleDate >= input.SaleDateFrom!.Value)
            .WhereIf(input.SaleDateTo.HasValue, x => x.SaleDate <= input.SaleDateTo!.Value)
            .WhereIf(input.MinimumGrandTotal.HasValue, x => x.GrandTotal >= input.MinimumGrandTotal!.Value)
            .WhereIf(input.MaximumGrandTotal.HasValue, x => x.GrandTotal <= input.MaximumGrandTotal!.Value)
            .WhereIf(input.HasPendingAmount == true, x => x.PendingAmount > 0)
            .WhereIf(input.HasPendingAmount == false, x => x.PendingAmount == 0);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var matchingCustomerIds = await AsyncExecuter.ToListAsync(customers
                .Where(x => x.TenantId == tenantId && (x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!)))
                .Select(x => x.Id));

            filtered = filtered.Where(x =>
                x.SaleNumber.Contains(input.Filter!) ||
                (x.ReferenceNumber != null && x.ReferenceNumber.Contains(input.Filter!)) ||
                (x.Notes != null && x.Notes.Contains(input.Filter!)) ||
                matchingCustomerIds.Contains(x.CustomerId));
        }

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "SaleDate desc, CreationTime desc" : input.Sorting!;
        var pagedSales = filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount);

        var projected = from s in pagedSales
                        join c in customers on s.CustomerId equals c.Id
                        select new ShopSaleDto
                        {
                            Id = s.Id,
                            SaleNumber = s.SaleNumber,
                            CustomerId = s.CustomerId,
                            CustomerCode = c.Code,
                            CustomerName = c.Name,
                            SaleDate = s.SaleDate,
                            DueDate = s.DueDate,
                            SaleType = s.SaleType,
                            PaymentMethod = s.PaymentMethod,
                            Status = s.Status,
                            SubTotal = s.SubTotal,
                            DiscountAmount = s.DiscountAmount,
                            TaxAmount = s.TaxAmount,
                            OtherCharges = s.OtherCharges,
                            GrandTotal = s.GrandTotal,
                            PaidAmount = s.PaidAmount,
                            PendingAmount = s.PendingAmount,
                            ReferenceNumber = s.ReferenceNumber,
                            Notes = s.Notes,
                            CompletedByUserId = s.CompletedByUserId,
                            CompletedDate = s.CompletedDate,
                            CancelledByUserId = s.CancelledByUserId,
                            CancelledDate = s.CancelledDate,
                            CancellationReason = s.CancellationReason,
                            CreationTime = s.CreationTime,
                            Items = new List<ShopSaleItemDto>()
                        };

        var items = await AsyncExecuter.ToListAsync(projected);
        await HidePriceIfNotAllowedAsync(items);
        await HideCostIfNotAllowedAsync(items);
        return new PagedResultDto<ShopSaleDto>(totalCount, items);
    }

    public async Task<ShopSaleDto> GetAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        var dto = await MapToDtoAsync(entity);
        var items = new List<ShopSaleDto> { dto };
        await HidePriceIfNotAllowedAsync(items);
        await HideCostIfNotAllowedAsync(items);
        return dto;
    }

    [Authorize(EHubPermissions.ShopSales.Create)]
    public async Task<ShopSaleDto> CreateAsync(CreateShopSaleDto input)
    {
        var entity = await _manager.CreateAsync(input.CustomerId, input.SaleDate, input.DueDate, input.SaleType,
            input.PaymentMethod, input.PaidAmount, input.ReferenceNumber, input.OtherCharges, input.Notes,
            ToItemInputs(input.Items));
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopSales.Edit)]
    public async Task<ShopSaleDto> UpdateAsync(Guid id, UpdateShopSaleDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.UpdateAsync(entity, input.CustomerId, input.SaleDate, input.DueDate, input.SaleType,
            input.PaymentMethod, input.PaidAmount, input.ReferenceNumber, input.OtherCharges, input.Notes,
            ToItemInputs(input.Items));
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopSales.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    [Authorize(EHubPermissions.ShopSales.Complete)]
    public async Task<ShopSaleDto> CompleteAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.CompleteAsync(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopSales.Cancel)]
    public async Task<ShopSaleDto> CancelAsync(Guid id, CancelShopSaleDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.CancelAsync(entity, input.CancellationReason);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public async Task<ListResultDto<ShopSaleProductLookupDto>> GetSaleProductLookupAsync(string? filter = null)
    {
        if (!CurrentTenant.Id.HasValue) return new ListResultDto<ShopSaleProductLookupDto>(new List<ShopSaleProductLookupDto>());
        var tenantId = CurrentTenant.Id.Value;

        var productQuery = await _productRepository.GetQueryableAsync();
        var filtered = productQuery.Where(x => x.TenantId == tenantId && x.IsActive)
            .WhereIf(!filter.IsNullOrWhiteSpace(), x => x.Code.Contains(filter!) || x.Name.Contains(filter!));

        var unitQuery = await _unitRepository.GetQueryableAsync();

        var projected = from p in filtered
                        join u in unitQuery on p.UnitId equals u.Id
                        orderby p.Name
                        select new ShopSaleProductLookupDto
                        {
                            Id = p.Id,
                            Code = p.Code,
                            Name = p.Name,
                            UnitId = p.UnitId,
                            UnitName = u.Name,
                            UnitShortName = u.ShortName,
                            UnitAllowDecimal = u.AllowDecimal,
                            CurrentStock = p.CurrentStock,
                            SalePrice = p.SalePrice,
                            TaxPercentage = p.TaxPercentage,
                            TrackBatch = p.TrackBatch,
                            TrackExpiry = p.TrackExpiry
                        };

        var items = await AsyncExecuter.ToListAsync(projected);
        await HidePriceIfNotAllowedAsync(items);
        return new ListResultDto<ShopSaleProductLookupDto>(items);
    }

    private static List<ShopSaleItemInput> ToItemInputs(IEnumerable<ShopSaleItemEditDtoBase> items) =>
        items.Select(x => new ShopSaleItemInput
        {
            ProductId = x.ProductId,
            Quantity = x.Quantity,
            UnitSalePrice = x.UnitSalePrice,
            DiscountPercentage = x.DiscountPercentage,
            TaxPercentage = x.TaxPercentage,
            BatchNumber = x.BatchNumber,
            ExpiryDate = x.ExpiryDate
        }).ToList();

    private async Task<ShopSaleDto> MapToDtoAsync(ShopSale entity)
    {
        var productIds = entity.Items.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);
        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        return new ShopSaleDto
        {
            Id = entity.Id,
            SaleNumber = entity.SaleNumber,
            CustomerId = entity.CustomerId,
            CustomerCode = entity.Customer?.Code ?? string.Empty,
            CustomerName = entity.Customer?.Name ?? string.Empty,
            SaleDate = entity.SaleDate,
            DueDate = entity.DueDate,
            SaleType = entity.SaleType,
            PaymentMethod = entity.PaymentMethod,
            Status = entity.Status,
            SubTotal = entity.SubTotal,
            DiscountAmount = entity.DiscountAmount,
            TaxAmount = entity.TaxAmount,
            OtherCharges = entity.OtherCharges,
            GrandTotal = entity.GrandTotal,
            PaidAmount = entity.PaidAmount,
            PendingAmount = entity.PendingAmount,
            ReferenceNumber = entity.ReferenceNumber,
            Notes = entity.Notes,
            CompletedByUserId = entity.CompletedByUserId,
            CompletedDate = entity.CompletedDate,
            CancelledByUserId = entity.CancelledByUserId,
            CancelledDate = entity.CancelledDate,
            CancellationReason = entity.CancellationReason,
            CreationTime = entity.CreationTime,
            Items = entity.Items.Select(x =>
            {
                products.TryGetValue(x.ProductId, out var product);
                ShopUnit? unit = null;
                if (product != null) units.TryGetValue(product.UnitId, out unit);
                return new ShopSaleItemDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductName = x.ProductNameSnapshot,
                    ProductCode = x.ProductCodeSnapshot,
                    UnitName = x.UnitNameSnapshot,
                    UnitShortName = x.UnitShortNameSnapshot,
                    UnitAllowDecimal = unit?.AllowDecimal ?? false,
                    Quantity = x.Quantity,
                    UnitSalePrice = x.UnitSalePrice,
                    UnitCostSnapshot = x.UnitCostSnapshot,
                    DiscountPercentage = x.DiscountPercentage,
                    DiscountAmount = x.DiscountAmount,
                    TaxPercentage = x.TaxPercentage,
                    TaxAmount = x.TaxAmount,
                    LineSubTotal = x.LineSubTotal,
                    LineTotal = x.LineTotal,
                    BatchNumber = x.BatchNumber,
                    ExpiryDate = x.ExpiryDate
                };
            }).ToList()
        };
    }

    private async Task HidePriceIfNotAllowedAsync(List<ShopSaleDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopSales.ViewPrice)) return;
        foreach (var dto in items)
        {
            dto.SubTotal = null;
            dto.DiscountAmount = null;
            dto.TaxAmount = null;
            dto.OtherCharges = null;
            dto.GrandTotal = null;
            dto.PaidAmount = null;
            dto.PendingAmount = null;
            foreach (var item in dto.Items)
            {
                item.UnitSalePrice = null;
                item.DiscountAmount = null;
                item.TaxAmount = null;
                item.LineSubTotal = null;
                item.LineTotal = null;
            }
        }
    }

    private async Task HidePriceIfNotAllowedAsync(List<ShopSaleProductLookupDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopSales.ViewPrice)) return;
        foreach (var item in items) item.SalePrice = null;
    }

    private async Task HideCostIfNotAllowedAsync(List<ShopSaleDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopSales.ViewCost)) return;
        foreach (var dto in items)
            foreach (var item in dto.Items)
                item.UnitCostSnapshot = null;
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopSale> FindEntityWithItemsAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.WithDetailsAsync(x => x.Items, x => x.Customer!);
        var entity = query.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId);
        return entity ?? throw new BusinessException("ShopManagement:SaleNotFound");
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace EHub.ShopManagement.PurchaseOrders;

[Authorize(EHubPermissions.ShopPurchaseOrders.Default)]
public class ShopPurchaseOrderAppService : ApplicationService, IShopPurchaseOrderAppService
{
    private readonly IRepository<ShopPurchaseOrder, Guid> _repository;
    private readonly IRepository<ShopSupplier, Guid> _supplierRepository;
    private readonly ShopPurchaseOrderManager _manager;

    public ShopPurchaseOrderAppService(
        IRepository<ShopPurchaseOrder, Guid> repository,
        IRepository<ShopSupplier, Guid> supplierRepository,
        ShopPurchaseOrderManager manager)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopPurchaseOrderDto>> GetListAsync(GetShopPurchaseOrdersInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopPurchaseOrderDto>(0, new List<ShopPurchaseOrderDto>());
        var tenantId = CurrentTenant.Id.Value;

        var orders = await _repository.GetQueryableAsync();
        var suppliers = await _supplierRepository.GetQueryableAsync();

        var filtered = orders.Where(x => x.TenantId == tenantId)
            .WhereIf(input.SupplierId.HasValue, x => x.SupplierId == input.SupplierId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.OrderDateFrom.HasValue, x => x.OrderDate >= input.OrderDateFrom!.Value)
            .WhereIf(input.OrderDateTo.HasValue, x => x.OrderDate <= input.OrderDateTo!.Value)
            .WhereIf(input.ExpectedDeliveryDateFrom.HasValue, x => x.ExpectedDeliveryDate >= input.ExpectedDeliveryDateFrom!.Value)
            .WhereIf(input.ExpectedDeliveryDateTo.HasValue, x => x.ExpectedDeliveryDate <= input.ExpectedDeliveryDateTo!.Value)
            .WhereIf(input.MinimumGrandTotal.HasValue, x => x.GrandTotal >= input.MinimumGrandTotal!.Value)
            .WhereIf(input.MaximumGrandTotal.HasValue, x => x.GrandTotal <= input.MaximumGrandTotal!.Value);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var matchingSupplierIds = await AsyncExecuter.ToListAsync(suppliers
                .Where(x => x.TenantId == tenantId && (x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!)))
                .Select(x => x.Id));

            filtered = filtered.Where(x =>
                x.PurchaseOrderNumber.Contains(input.Filter!) ||
                (x.SupplierReference != null && x.SupplierReference.Contains(input.Filter!)) ||
                (x.Notes != null && x.Notes.Contains(input.Filter!)) ||
                matchingSupplierIds.Contains(x.SupplierId));
        }

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "OrderDate desc, CreationTime desc" : input.Sorting!;
        var pagedOrders = filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount);

        var projected = from po in pagedOrders
                        join supplier in suppliers on po.SupplierId equals supplier.Id
                        select new ShopPurchaseOrderDto
                        {
                            Id = po.Id,
                            PurchaseOrderNumber = po.PurchaseOrderNumber,
                            SupplierId = po.SupplierId,
                            SupplierCode = supplier.Code,
                            SupplierName = supplier.Name,
                            OrderDate = po.OrderDate,
                            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                            Status = po.Status,
                            SupplierReference = po.SupplierReference,
                            SubTotal = po.SubTotal,
                            DiscountAmount = po.DiscountAmount,
                            TaxAmount = po.TaxAmount,
                            ShippingCharges = po.ShippingCharges,
                            OtherCharges = po.OtherCharges,
                            GrandTotal = po.GrandTotal,
                            Notes = po.Notes,
                            ApprovedByUserId = po.ApprovedByUserId,
                            ApprovedDate = po.ApprovedDate,
                            RejectedByUserId = po.RejectedByUserId,
                            RejectedDate = po.RejectedDate,
                            RejectionReason = po.RejectionReason,
                            CancelledByUserId = po.CancelledByUserId,
                            CancelledDate = po.CancelledDate,
                            CancellationReason = po.CancellationReason,
                            CreationTime = po.CreationTime,
                            Items = new List<ShopPurchaseOrderItemDto>()
                        };

        var items = await AsyncExecuter.ToListAsync(projected);
        await HideCostIfNotAllowedAsync(items);
        return new PagedResultDto<ShopPurchaseOrderDto>(totalCount, items);
    }

    public async Task<ShopPurchaseOrderDto> GetAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        var dto = MapToDto(entity);
        await HideCostIfNotAllowedAsync(new List<ShopPurchaseOrderDto> { dto });
        return dto;
    }

    [Authorize(EHubPermissions.ShopPurchaseOrders.Create)]
    public async Task<ShopPurchaseOrderDto> CreateAsync(CreateShopPurchaseOrderDto input)
    {
        var entity = await _manager.CreateAsync(input.SupplierId, input.OrderDate, input.ExpectedDeliveryDate,
            input.SupplierReference, input.ShippingCharges, input.OtherCharges, input.Notes, ToItemInputs(input.Items));
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopPurchaseOrders.Edit)]
    public async Task<ShopPurchaseOrderDto> UpdateAsync(Guid id, UpdateShopPurchaseOrderDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.UpdateAsync(entity, input.SupplierId, input.OrderDate, input.ExpectedDeliveryDate,
            input.SupplierReference, input.ShippingCharges, input.OtherCharges, input.Notes, ToItemInputs(input.Items));
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopPurchaseOrders.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    [Authorize(EHubPermissions.ShopPurchaseOrders.Submit)]
    public async Task<ShopPurchaseOrderDto> SubmitAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.SubmitAsync(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopPurchaseOrders.Approve)]
    public async Task<ShopPurchaseOrderDto> ApproveAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.ApproveAsync(entity, CurrentUser.GetId());
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopPurchaseOrders.Reject)]
    public async Task<ShopPurchaseOrderDto> RejectAsync(Guid id, RejectShopPurchaseOrderDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.RejectAsync(entity, CurrentUser.GetId(), input.RejectionReason);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopPurchaseOrders.Cancel)]
    public async Task<ShopPurchaseOrderDto> CancelAsync(Guid id, CancelShopPurchaseOrderDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.CancelAsync(entity, CurrentUser.GetId(), input.CancellationReason);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    private static List<ShopPurchaseOrderItemInput> ToItemInputs(IEnumerable<ShopPurchaseOrderItemEditDtoBase> items) =>
        items.Select(x => new ShopPurchaseOrderItemInput
        {
            ProductId = x.ProductId,
            Description = x.Description,
            OrderedQuantity = x.OrderedQuantity,
            UnitPurchasePrice = x.UnitPurchasePrice,
            DiscountPercentage = x.DiscountPercentage,
            TaxPercentage = x.TaxPercentage
        }).ToList();

    private async Task HideCostIfNotAllowedAsync(List<ShopPurchaseOrderDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopPurchaseOrders.ViewCost)) return;
        foreach (var dto in items)
        {
            dto.SubTotal = null;
            dto.DiscountAmount = null;
            dto.TaxAmount = null;
            dto.ShippingCharges = null;
            dto.OtherCharges = null;
            dto.GrandTotal = null;
            foreach (var item in dto.Items)
            {
                item.UnitPurchasePrice = null;
                item.DiscountAmount = null;
                item.TaxAmount = null;
                item.LineSubTotal = null;
                item.LineTotal = null;
            }
        }
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopPurchaseOrder> FindEntityWithItemsAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.WithDetailsAsync(x => x.Items, x => x.Supplier!);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId));
        return entity ?? throw new BusinessException("ShopManagement:PurchaseOrderNotFound");
    }

    private static ShopPurchaseOrderDto MapToDto(ShopPurchaseOrder entity) => new()
    {
        Id = entity.Id,
        PurchaseOrderNumber = entity.PurchaseOrderNumber,
        SupplierId = entity.SupplierId,
        SupplierCode = entity.Supplier?.Code ?? string.Empty,
        SupplierName = entity.Supplier?.Name ?? string.Empty,
        OrderDate = entity.OrderDate,
        ExpectedDeliveryDate = entity.ExpectedDeliveryDate,
        Status = entity.Status,
        SupplierReference = entity.SupplierReference,
        SubTotal = entity.SubTotal,
        DiscountAmount = entity.DiscountAmount,
        TaxAmount = entity.TaxAmount,
        ShippingCharges = entity.ShippingCharges,
        OtherCharges = entity.OtherCharges,
        GrandTotal = entity.GrandTotal,
        Notes = entity.Notes,
        ApprovedByUserId = entity.ApprovedByUserId,
        ApprovedDate = entity.ApprovedDate,
        RejectedByUserId = entity.RejectedByUserId,
        RejectedDate = entity.RejectedDate,
        RejectionReason = entity.RejectionReason,
        CancelledByUserId = entity.CancelledByUserId,
        CancelledDate = entity.CancelledDate,
        CancellationReason = entity.CancellationReason,
        CreationTime = entity.CreationTime,
        Items = entity.Items.Select(x => new ShopPurchaseOrderItemDto
        {
            Id = x.Id,
            ProductId = x.ProductId,
            ProductName = x.ProductNameSnapshot,
            ProductCode = x.ProductCodeSnapshot,
            UnitName = x.UnitNameSnapshot,
            UnitShortName = x.UnitShortNameSnapshot,
            Description = x.Description,
            OrderedQuantity = x.OrderedQuantity,
            ReceivedQuantity = x.ReceivedQuantity,
            UnitPurchasePrice = x.UnitPurchasePrice,
            DiscountPercentage = x.DiscountPercentage,
            DiscountAmount = x.DiscountAmount,
            TaxPercentage = x.TaxPercentage,
            TaxAmount = x.TaxAmount,
            LineSubTotal = x.LineSubTotal,
            LineTotal = x.LineTotal
        }).ToList()
    };
}

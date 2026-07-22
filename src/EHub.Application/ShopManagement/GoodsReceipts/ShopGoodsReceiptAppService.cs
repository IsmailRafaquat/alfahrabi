using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.Units;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.GoodsReceipts;

[Authorize(EHubPermissions.ShopGoodsReceipts.Default)]
public class ShopGoodsReceiptAppService : ApplicationService, IShopGoodsReceiptAppService
{
    private readonly IRepository<ShopGoodsReceipt, Guid> _repository;
    private readonly IRepository<ShopPurchaseOrder, Guid> _purchaseOrderRepository;
    private readonly IRepository<ShopSupplier, Guid> _supplierRepository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly ShopGoodsReceiptManager _manager;

    public ShopGoodsReceiptAppService(
        IRepository<ShopGoodsReceipt, Guid> repository,
        IRepository<ShopPurchaseOrder, Guid> purchaseOrderRepository,
        IRepository<ShopSupplier, Guid> supplierRepository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        ShopGoodsReceiptManager manager)
    {
        _repository = repository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopGoodsReceiptDto>> GetListAsync(GetShopGoodsReceiptsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopGoodsReceiptDto>(0, new List<ShopGoodsReceiptDto>());
        var tenantId = CurrentTenant.Id.Value;

        var receipts = await _repository.GetQueryableAsync();
        var purchaseOrders = await _purchaseOrderRepository.GetQueryableAsync();
        var suppliers = await _supplierRepository.GetQueryableAsync();

        var filtered = receipts.Where(x => x.TenantId == tenantId)
            .WhereIf(input.PurchaseOrderId.HasValue, x => x.PurchaseOrderId == input.PurchaseOrderId)
            .WhereIf(input.SupplierId.HasValue, x => x.SupplierId == input.SupplierId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.ReceiptDateFrom.HasValue, x => x.ReceiptDate >= input.ReceiptDateFrom!.Value)
            .WhereIf(input.ReceiptDateTo.HasValue, x => x.ReceiptDate <= input.ReceiptDateTo!.Value)
            .WhereIf(input.MinimumGrandTotal.HasValue, x => x.GrandTotal >= input.MinimumGrandTotal!.Value)
            .WhereIf(input.MaximumGrandTotal.HasValue, x => x.GrandTotal <= input.MaximumGrandTotal!.Value);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var matchingSupplierIds = await AsyncExecuter.ToListAsync(suppliers
                .Where(x => x.TenantId == tenantId && (x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!)))
                .Select(x => x.Id));
            var matchingPurchaseOrderIds = await AsyncExecuter.ToListAsync(purchaseOrders
                .Where(x => x.TenantId == tenantId && x.PurchaseOrderNumber.Contains(input.Filter!))
                .Select(x => x.Id));

            filtered = filtered.Where(x =>
                x.GoodsReceiptNumber.Contains(input.Filter!) ||
                (x.SupplierInvoiceNumber != null && x.SupplierInvoiceNumber.Contains(input.Filter!)) ||
                (x.Notes != null && x.Notes.Contains(input.Filter!)) ||
                matchingSupplierIds.Contains(x.SupplierId) ||
                matchingPurchaseOrderIds.Contains(x.PurchaseOrderId));
        }

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "ReceiptDate desc, CreationTime desc" : input.Sorting!;
        var pagedReceipts = filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount);

        var projected = from gr in pagedReceipts
                        join po in purchaseOrders on gr.PurchaseOrderId equals po.Id
                        join supplier in suppliers on gr.SupplierId equals supplier.Id
                        select new ShopGoodsReceiptDto
                        {
                            Id = gr.Id,
                            GoodsReceiptNumber = gr.GoodsReceiptNumber,
                            PurchaseOrderId = gr.PurchaseOrderId,
                            PurchaseOrderNumber = po.PurchaseOrderNumber,
                            SupplierId = gr.SupplierId,
                            SupplierCode = supplier.Code,
                            SupplierName = supplier.Name,
                            SupplierInvoiceNumber = gr.SupplierInvoiceNumber,
                            ReceiptDate = gr.ReceiptDate,
                            Status = gr.Status,
                            SubTotal = gr.SubTotal,
                            DiscountAmount = gr.DiscountAmount,
                            TaxAmount = gr.TaxAmount,
                            ShippingCharges = gr.ShippingCharges,
                            OtherCharges = gr.OtherCharges,
                            GrandTotal = gr.GrandTotal,
                            Notes = gr.Notes,
                            ReceivedByUserId = gr.ReceivedByUserId,
                            CompletedByUserId = gr.CompletedByUserId,
                            CompletedDate = gr.CompletedDate,
                            CancelledByUserId = gr.CancelledByUserId,
                            CancelledDate = gr.CancelledDate,
                            CancellationReason = gr.CancellationReason,
                            CreationTime = gr.CreationTime,
                            Items = new List<ShopGoodsReceiptItemDto>()
                        };

        var items = await AsyncExecuter.ToListAsync(projected);
        await HideCostIfNotAllowedAsync(items);
        return new PagedResultDto<ShopGoodsReceiptDto>(totalCount, items);
    }

    public async Task<ShopGoodsReceiptDto> GetAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        var dto = await MapToDtoAsync(entity);
        await HideCostIfNotAllowedAsync(new List<ShopGoodsReceiptDto> { dto });
        return dto;
    }

    public async Task<ShopPurchaseOrderReceivingDto> GetPurchaseOrderForReceivingAsync(Guid purchaseOrderId)
    {
        var tenantId = RequireTenant();
        var poQuery = await _purchaseOrderRepository.WithDetailsAsync(x => x.Items, x => x.Supplier!);
        var po = poQuery.FirstOrDefault(x => x.Id == purchaseOrderId && x.TenantId == tenantId)
            ?? throw new BusinessException("ShopManagement:GoodsReceiptPurchaseOrderNotFound");

        if (po.Status != ShopPurchaseOrderStatus.Approved && po.Status != ShopPurchaseOrderStatus.PartiallyReceived)
            throw new BusinessException("ShopManagement:GoodsReceiptInvalidPurchaseOrderStatus");

        var remainingItems = po.Items.Where(x => x.OrderedQuantity - x.ReceivedQuantity > 0).ToList();
        var productIds = remainingItems.Select(x => x.ProductId).Distinct().ToList();

        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);
        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var canViewCost = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopGoodsReceipts.ViewCost);

        var dto = new ShopPurchaseOrderReceivingDto
        {
            PurchaseOrderId = po.Id,
            PurchaseOrderNumber = po.PurchaseOrderNumber,
            SupplierId = po.SupplierId,
            SupplierCode = po.Supplier?.Code ?? string.Empty,
            SupplierName = po.Supplier?.Name ?? string.Empty,
            OrderDate = po.OrderDate,
            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
            Status = po.Status,
            Items = remainingItems.Select(item =>
            {
                products.TryGetValue(item.ProductId, out var product);
                ShopUnit? unit = null;
                if (product != null) units.TryGetValue(product.UnitId, out unit);
                return new ShopPurchaseOrderReceivingItemDto
                {
                    PurchaseOrderItemId = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductNameSnapshot,
                    ProductCode = item.ProductCodeSnapshot,
                    UnitName = unit?.Name ?? item.UnitNameSnapshot,
                    UnitShortName = unit?.ShortName ?? item.UnitShortNameSnapshot,
                    UnitAllowDecimal = unit?.AllowDecimal ?? false,
                    TrackBatch = product?.TrackBatch ?? false,
                    TrackExpiry = product?.TrackExpiry ?? false,
                    TrackSerialNumber = product?.TrackSerialNumber ?? false,
                    OrderedQuantity = item.OrderedQuantity,
                    PreviouslyReceivedQuantity = item.ReceivedQuantity,
                    RemainingQuantity = item.OrderedQuantity - item.ReceivedQuantity,
                    DefaultPurchasePrice = canViewCost ? product?.PurchasePrice : null,
                    DefaultSalePrice = canViewCost ? product?.SalePrice : null,
                };
            }).ToList()
        };

        return dto;
    }

    [Authorize(EHubPermissions.ShopGoodsReceipts.Create)]
    public async Task<ShopGoodsReceiptDto> CreateAsync(CreateShopGoodsReceiptDto input)
    {
        var entity = await _manager.CreateAsync(input.PurchaseOrderId, input.SupplierInvoiceNumber, input.ReceiptDate,
            input.ShippingCharges, input.OtherCharges, input.Notes, ToItemInputs(input.Items));
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopGoodsReceipts.Edit)]
    public async Task<ShopGoodsReceiptDto> UpdateAsync(Guid id, UpdateShopGoodsReceiptDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.UpdateAsync(entity, input.SupplierInvoiceNumber, input.ReceiptDate, input.ShippingCharges,
            input.OtherCharges, input.Notes, ToItemInputs(input.Items));
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopGoodsReceipts.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    [Authorize(EHubPermissions.ShopGoodsReceipts.Complete)]
    public async Task<ShopGoodsReceiptDto> CompleteAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.CompleteAsync(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopGoodsReceipts.Cancel)]
    public async Task<ShopGoodsReceiptDto> CancelAsync(Guid id, CancelShopGoodsReceiptDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.CancelAsync(entity, input.CancellationReason);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    private static List<ShopGoodsReceiptItemInput> ToItemInputs(IEnumerable<ShopGoodsReceiptItemEditDtoBase> items) =>
        items.Select(x => new ShopGoodsReceiptItemInput
        {
            PurchaseOrderItemId = x.PurchaseOrderItemId,
            ReceivedQuantity = x.ReceivedQuantity,
            BonusQuantity = x.BonusQuantity,
            PurchasePrice = x.PurchasePrice,
            SalePrice = x.SalePrice,
            BatchNumber = x.BatchNumber,
            ManufacturingDate = x.ManufacturingDate,
            ExpiryDate = x.ExpiryDate,
            DiscountPercentage = x.DiscountPercentage,
            TaxPercentage = x.TaxPercentage
        }).ToList();

    private async Task HideCostIfNotAllowedAsync(List<ShopGoodsReceiptDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopGoodsReceipts.ViewCost)) return;
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
                item.PurchasePrice = null;
                item.DiscountAmount = null;
                item.TaxAmount = null;
                item.LineSubTotal = null;
                item.LineTotal = null;
            }
        }
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopGoodsReceipt> FindEntityWithItemsAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.WithDetailsAsync(x => x.Items, x => x.Supplier!, x => x.PurchaseOrder!);
        var entity = query.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId);
        return entity ?? throw new BusinessException("ShopManagement:GoodsReceiptNotFound");
    }

    private async Task<ShopGoodsReceiptDto> MapToDtoAsync(ShopGoodsReceipt entity)
    {
        var productIds = entity.Items.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);
        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        return new ShopGoodsReceiptDto
        {
            Id = entity.Id,
            GoodsReceiptNumber = entity.GoodsReceiptNumber,
            PurchaseOrderId = entity.PurchaseOrderId,
            PurchaseOrderNumber = entity.PurchaseOrder?.PurchaseOrderNumber ?? string.Empty,
            SupplierId = entity.SupplierId,
            SupplierCode = entity.Supplier?.Code ?? string.Empty,
            SupplierName = entity.Supplier?.Name ?? string.Empty,
            SupplierInvoiceNumber = entity.SupplierInvoiceNumber,
            ReceiptDate = entity.ReceiptDate,
            Status = entity.Status,
            SubTotal = entity.SubTotal,
            DiscountAmount = entity.DiscountAmount,
            TaxAmount = entity.TaxAmount,
            ShippingCharges = entity.ShippingCharges,
            OtherCharges = entity.OtherCharges,
            GrandTotal = entity.GrandTotal,
            Notes = entity.Notes,
            ReceivedByUserId = entity.ReceivedByUserId,
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
                return new ShopGoodsReceiptItemDto
                {
                    Id = x.Id,
                    PurchaseOrderItemId = x.PurchaseOrderItemId,
                    ProductId = x.ProductId,
                    ProductName = x.ProductNameSnapshot,
                    ProductCode = x.ProductCodeSnapshot,
                    UnitName = x.UnitNameSnapshot,
                    UnitShortName = x.UnitShortNameSnapshot,
                    UnitAllowDecimal = unit?.AllowDecimal ?? false,
                    TrackBatch = product?.TrackBatch ?? false,
                    TrackExpiry = product?.TrackExpiry ?? false,
                    TrackSerialNumber = product?.TrackSerialNumber ?? false,
                    OrderedQuantity = x.OrderedQuantitySnapshot,
                    PreviouslyReceivedQuantity = x.PreviouslyReceivedQuantity,
                    RemainingQuantity = x.OrderedQuantitySnapshot - x.PreviouslyReceivedQuantity,
                    ReceivedQuantity = x.ReceivedQuantity,
                    BonusQuantity = x.BonusQuantity,
                    PurchasePrice = x.PurchasePrice,
                    SalePrice = x.SalePrice,
                    BatchNumber = x.BatchNumber,
                    ManufacturingDate = x.ManufacturingDate,
                    ExpiryDate = x.ExpiryDate,
                    DiscountPercentage = x.DiscountPercentage,
                    DiscountAmount = x.DiscountAmount,
                    TaxPercentage = x.TaxPercentage,
                    TaxAmount = x.TaxAmount,
                    LineSubTotal = x.LineSubTotal,
                    LineTotal = x.LineTotal
                };
            }).ToList()
        };
    }
}

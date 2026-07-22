using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.Settings;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.Units;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.PurchaseOrders;

public class ShopPurchaseOrderManager : DomainService
{
    private readonly IRepository<ShopPurchaseOrder, Guid> _repository;
    private readonly IRepository<ShopSupplier, Guid> _supplierRepository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly IRepository<ShopSetting, Guid> _settingRepository;
    private readonly ShopDocumentNumberGenerator _numberGenerator;
    private readonly ICurrentTenant _currentTenant;

    public ShopPurchaseOrderManager(
        IRepository<ShopPurchaseOrder, Guid> repository,
        IRepository<ShopSupplier, Guid> supplierRepository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        IRepository<ShopSetting, Guid> settingRepository,
        ShopDocumentNumberGenerator numberGenerator,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _settingRepository = settingRepository;
        _numberGenerator = numberGenerator;
        _currentTenant = currentTenant;
    }

    public async Task<ShopPurchaseOrder> CreateAsync(
        Guid supplierId,
        DateTime orderDate,
        DateTime? expectedDeliveryDate,
        string? supplierReference,
        decimal shippingCharges,
        decimal otherCharges,
        string? notes,
        IReadOnlyList<ShopPurchaseOrderItemInput> items)
    {
        var tenantId = RequireTenant();
        await ValidateSupplierAsync(supplierId, tenantId, requireActive: true);
        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:PurchaseOrderRequiresItems");
        ValidateNoDuplicateProducts(items);

        var orderId = GuidGenerator.Create();
        var itemEntities = new List<ShopPurchaseOrderItem>();
        foreach (var itemInput in items)
        {
            var (product, unit) = await ValidateProductAsync(itemInput.ProductId, tenantId, requireActive: true);
            ValidateQuantity(itemInput.OrderedQuantity, unit.AllowDecimal);

            itemEntities.Add(new ShopPurchaseOrderItem(GuidGenerator.Create(), tenantId, orderId, product.Id,
                product.Name, product.Code, unit.Name, unit.ShortName, itemInput.Description,
                itemInput.OrderedQuantity, itemInput.UnitPurchasePrice, itemInput.DiscountPercentage, itemInput.TaxPercentage));
        }

        var prefix = await GetPurchaseOrderPrefixAsync(tenantId);
        var purchaseOrderNumber = await _numberGenerator.GetNextNumberAsync(tenantId, "PurchaseOrder", prefix + "-");

        return new ShopPurchaseOrder(orderId, tenantId, purchaseOrderNumber, supplierId, orderDate, expectedDeliveryDate,
            supplierReference, shippingCharges, otherCharges, notes, itemEntities);
    }

    public async Task UpdateAsync(
        ShopPurchaseOrder purchaseOrder,
        Guid supplierId,
        DateTime orderDate,
        DateTime? expectedDeliveryDate,
        string? supplierReference,
        decimal shippingCharges,
        decimal otherCharges,
        string? notes,
        IReadOnlyList<ShopPurchaseOrderItemInput> items)
    {
        var tenantId = RequireTenantOwnership(purchaseOrder);
        purchaseOrder.EnsureEditable();

        await ValidateSupplierAsync(supplierId, tenantId, requireActive: true);
        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:PurchaseOrderRequiresItems");
        ValidateNoDuplicateProducts(items);

        var itemEntities = new List<ShopPurchaseOrderItem>();
        foreach (var itemInput in items)
        {
            var (product, unit) = await ValidateProductAsync(itemInput.ProductId, tenantId, requireActive: true);
            ValidateQuantity(itemInput.OrderedQuantity, unit.AllowDecimal);

            itemEntities.Add(new ShopPurchaseOrderItem(GuidGenerator.Create(), tenantId, purchaseOrder.Id, product.Id,
                product.Name, product.Code, unit.Name, unit.ShortName, itemInput.Description,
                itemInput.OrderedQuantity, itemInput.UnitPurchasePrice, itemInput.DiscountPercentage, itemInput.TaxPercentage));
        }

        purchaseOrder.UpdateHeaderAndItems(supplierId, orderDate, expectedDeliveryDate, supplierReference,
            shippingCharges, otherCharges, notes, itemEntities);
    }

    public Task SubmitAsync(ShopPurchaseOrder purchaseOrder)
    {
        RequireTenantOwnership(purchaseOrder);
        purchaseOrder.MarkAsPendingApproval();
        return Task.CompletedTask;
    }

    public Task ApproveAsync(ShopPurchaseOrder purchaseOrder, Guid approvedByUserId)
    {
        RequireTenantOwnership(purchaseOrder);
        purchaseOrder.MarkAsApproved(approvedByUserId, Clock.Now);
        return Task.CompletedTask;
    }

    public Task RejectAsync(ShopPurchaseOrder purchaseOrder, Guid rejectedByUserId, string rejectionReason)
    {
        RequireTenantOwnership(purchaseOrder);
        purchaseOrder.MarkAsRejected(rejectedByUserId, Clock.Now, rejectionReason);
        return Task.CompletedTask;
    }

    public Task CancelAsync(ShopPurchaseOrder purchaseOrder, Guid cancelledByUserId, string cancellationReason)
    {
        RequireTenantOwnership(purchaseOrder);
        purchaseOrder.MarkAsCancelled(cancelledByUserId, Clock.Now, cancellationReason);
        return Task.CompletedTask;
    }

    public Task ValidateDeleteAsync(ShopPurchaseOrder purchaseOrder)
    {
        RequireTenantOwnership(purchaseOrder);
        purchaseOrder.EnsureDeletable();
        return Task.CompletedTask;
    }

    private async Task<string> GetPurchaseOrderPrefixAsync(Guid tenantId)
    {
        var query = await _settingRepository.GetQueryableAsync();
        var setting = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.TenantId == tenantId));
        return setting?.PurchaseOrderPrefix ?? "PO";
    }

    private async Task ValidateSupplierAsync(Guid supplierId, Guid tenantId, bool requireActive)
    {
        var query = await _supplierRepository.GetQueryableAsync();
        var supplier = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == supplierId && x.TenantId == tenantId));
        if (supplier == null) throw new BusinessException("ShopManagement:PurchaseOrderSupplierNotFound");
        if (requireActive && !supplier.IsActive) throw new BusinessException("ShopManagement:PurchaseOrderSupplierInactive");
    }

    private async Task<(ShopProduct Product, ShopUnit Unit)> ValidateProductAsync(Guid productId, Guid tenantId, bool requireActive)
    {
        var productQuery = await _productRepository.GetQueryableAsync();
        var product = await AsyncExecuter.FirstOrDefaultAsync(productQuery.Where(x => x.Id == productId && x.TenantId == tenantId));
        if (product == null) throw new BusinessException("ShopManagement:PurchaseOrderProductNotFound");
        if (requireActive && !product.IsActive) throw new BusinessException("ShopManagement:PurchaseOrderProductInactive");

        var unitQuery = await _unitRepository.GetQueryableAsync();
        var unit = await AsyncExecuter.FirstOrDefaultAsync(unitQuery.Where(x => x.Id == product.UnitId && x.TenantId == tenantId));
        if (unit == null) throw new BusinessException("ShopManagement:PurchaseOrderProductNotFound");

        return (product, unit);
    }

    private static void ValidateQuantity(decimal quantity, bool allowDecimal)
    {
        if (quantity <= 0) throw new BusinessException("ShopManagement:PurchaseOrderInvalidQuantity");
        if (!allowDecimal && quantity != Math.Truncate(quantity)) throw new BusinessException("ShopManagement:PurchaseOrderWholeQuantityRequired");
    }

    private static void ValidateNoDuplicateProducts(IReadOnlyList<ShopPurchaseOrderItemInput> items)
    {
        var productIds = items.Select(x => x.ProductId).ToList();
        if (productIds.Distinct().Count() != productIds.Count) throw new BusinessException("ShopManagement:PurchaseOrderDuplicateProduct");
    }

    private Guid RequireTenantOwnership(ShopPurchaseOrder purchaseOrder)
    {
        var tenantId = RequireTenant();
        if (purchaseOrder.TenantId != tenantId) throw new BusinessException("ShopManagement:PurchaseOrderNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}

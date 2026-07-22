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

namespace EHub.ShopManagement.GoodsReceipts;

public class ShopGoodsReceiptManager : DomainService
{
    private const string DocumentType = "GoodsReceipt";
    private const string NumberPrefix = "GRN-";

    private readonly IRepository<ShopGoodsReceipt, Guid> _repository;
    private readonly IRepository<ShopPurchaseOrder, Guid> _purchaseOrderRepository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly IRepository<ShopStockTransaction, Guid> _stockTransactionRepository;
    private readonly ShopDocumentNumberGenerator _numberGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ShopGoodsReceiptManager(
        IRepository<ShopGoodsReceipt, Guid> repository,
        IRepository<ShopPurchaseOrder, Guid> purchaseOrderRepository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        IRepository<ShopStockTransaction, Guid> stockTransactionRepository,
        ShopDocumentNumberGenerator numberGenerator,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _numberGenerator = numberGenerator;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<ShopGoodsReceipt> CreateAsync(
        Guid purchaseOrderId,
        string? supplierInvoiceNumber,
        DateTime receiptDate,
        decimal shippingCharges,
        decimal otherCharges,
        string? notes,
        IReadOnlyList<ShopGoodsReceiptItemInput> items)
    {
        var tenantId = RequireTenant();
        var purchaseOrder = await GetReceivablePurchaseOrderAsync(purchaseOrderId, tenantId);

        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:GoodsReceiptRequiresItems");
        ValidateNoDuplicatePurchaseOrderItems(items);

        var goodsReceiptId = GuidGenerator.Create();
        var itemEntities = await BuildItemEntitiesAsync(goodsReceiptId, tenantId, purchaseOrder, items, receiptDate);

        var goodsReceiptNumber = await _numberGenerator.GetNextNumberAsync(tenantId, DocumentType, NumberPrefix);

        return new ShopGoodsReceipt(goodsReceiptId, tenantId, goodsReceiptNumber, purchaseOrderId, purchaseOrder.SupplierId,
            _currentUser.Id, supplierInvoiceNumber, receiptDate, shippingCharges, otherCharges, notes, itemEntities);
    }

    public async Task UpdateAsync(
        ShopGoodsReceipt goodsReceipt,
        string? supplierInvoiceNumber,
        DateTime receiptDate,
        decimal shippingCharges,
        decimal otherCharges,
        string? notes,
        IReadOnlyList<ShopGoodsReceiptItemInput> items)
    {
        var tenantId = RequireTenantOwnership(goodsReceipt);
        goodsReceipt.EnsureEditable();

        var purchaseOrder = await GetReceivablePurchaseOrderAsync(goodsReceipt.PurchaseOrderId, tenantId);

        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:GoodsReceiptRequiresItems");
        ValidateNoDuplicatePurchaseOrderItems(items);

        var itemEntities = await BuildItemEntitiesAsync(goodsReceipt.Id, tenantId, purchaseOrder, items, receiptDate);

        goodsReceipt.UpdateHeaderAndItems(supplierInvoiceNumber, receiptDate, shippingCharges, otherCharges, notes, itemEntities);
    }

    public async Task CompleteAsync(ShopGoodsReceipt goodsReceipt)
    {
        var tenantId = RequireTenantOwnership(goodsReceipt);
        goodsReceipt.EnsureCompletable();

        var poQuery = await _purchaseOrderRepository.WithDetailsAsync(x => x.Items);
        var purchaseOrder = poQuery.FirstOrDefault(x => x.Id == goodsReceipt.PurchaseOrderId && x.TenantId == tenantId)
            ?? throw new BusinessException("ShopManagement:GoodsReceiptPurchaseOrderNotFound");

        var productIds = goodsReceipt.Items.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id) && x.TenantId == tenantId).ToList()
            .ToDictionary(x => x.Id);

        var existingTransactionQuery = await _stockTransactionRepository.GetQueryableAsync();
        var itemIds = goodsReceipt.Items.Select(x => x.Id).ToList();
        var alreadyHasTransactions = existingTransactionQuery.Any(x =>
            x.TenantId == tenantId && x.ReferenceType == ShopStockReferenceType.GoodsReceipt && x.SourceItemId.HasValue && itemIds.Contains(x.SourceItemId.Value));
        if (alreadyHasTransactions) throw new BusinessException("ShopManagement:GoodsReceiptStockTransactionAlreadyExists");

        var completedByUserId = _currentUser.GetId();
        var completedDate = Clock.Now;

        foreach (var item in goodsReceipt.Items)
        {
            var poItem = purchaseOrder.Items.FirstOrDefault(x => x.Id == item.PurchaseOrderItemId)
                ?? throw new BusinessException("ShopManagement:GoodsReceiptPurchaseOrderItemNotFound");

            var remaining = poItem.OrderedQuantity - poItem.ReceivedQuantity;
            if (item.ReceivedQuantity > remaining) throw new BusinessException("ShopManagement:GoodsReceiptQuantityExceedsRemaining");
            poItem.IncreaseReceivedQuantity(item.ReceivedQuantity);

            if (!products.TryGetValue(item.ProductId, out var product)) throw new BusinessException("ShopManagement:GoodsReceiptProductNotFound");
            var stockIncrease = item.ReceivedQuantity + item.BonusQuantity;
            product.IncreaseStock(stockIncrease);

            var transaction = new ShopStockTransaction(
                GuidGenerator.Create(), tenantId, item.ProductId, ShopStockTransactionType.Purchase, ShopStockReferenceType.GoodsReceipt,
                goodsReceipt.Id, goodsReceipt.GoodsReceiptNumber, item.Id, goodsReceipt.ReceiptDate,
                stockIncrease, 0, product.CurrentStock, item.PurchasePrice, item.BatchNumber, item.ExpiryDate,
                null, completedByUserId, completedDate);
            await _stockTransactionRepository.InsertAsync(transaction, autoSave: true);
        }

        purchaseOrder.UpdateReceivingProgress();
        await _purchaseOrderRepository.UpdateAsync(purchaseOrder, autoSave: true);

        foreach (var product in products.Values)
        {
            await _productRepository.UpdateAsync(product, autoSave: true);
        }

        goodsReceipt.MarkAsCompleted(completedByUserId, completedDate);
    }

    public Task CancelAsync(ShopGoodsReceipt goodsReceipt, string cancellationReason)
    {
        RequireTenantOwnership(goodsReceipt);
        goodsReceipt.MarkAsCancelled(_currentUser.GetId(), Clock.Now, cancellationReason);
        return Task.CompletedTask;
    }

    public Task ValidateDeleteAsync(ShopGoodsReceipt goodsReceipt)
    {
        RequireTenantOwnership(goodsReceipt);
        goodsReceipt.EnsureDeletable();
        return Task.CompletedTask;
    }

    private async Task<ShopPurchaseOrder> GetReceivablePurchaseOrderAsync(Guid purchaseOrderId, Guid tenantId)
    {
        var query = await _purchaseOrderRepository.WithDetailsAsync(x => x.Items);
        var purchaseOrder = query.FirstOrDefault(x => x.Id == purchaseOrderId && x.TenantId == tenantId)
            ?? throw new BusinessException("ShopManagement:GoodsReceiptPurchaseOrderNotFound");

        if (purchaseOrder.Status != ShopPurchaseOrderStatus.Approved && purchaseOrder.Status != ShopPurchaseOrderStatus.PartiallyReceived)
            throw new BusinessException("ShopManagement:GoodsReceiptInvalidPurchaseOrderStatus");

        return purchaseOrder;
    }

    private async Task<List<ShopGoodsReceiptItem>> BuildItemEntitiesAsync(
        Guid goodsReceiptId,
        Guid tenantId,
        ShopPurchaseOrder purchaseOrder,
        IReadOnlyList<ShopGoodsReceiptItemInput> items,
        DateTime receiptDate)
    {
        var productIds = new List<Guid>();
        foreach (var input in items)
        {
            var poItem = purchaseOrder.Items.FirstOrDefault(x => x.Id == input.PurchaseOrderItemId)
                ?? throw new BusinessException("ShopManagement:GoodsReceiptPurchaseOrderItemNotFound");
            productIds.Add(poItem.ProductId);
        }

        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);

        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var itemEntities = new List<ShopGoodsReceiptItem>();
        foreach (var input in items)
        {
            var poItem = purchaseOrder.Items.First(x => x.Id == input.PurchaseOrderItemId);

            if (!products.TryGetValue(poItem.ProductId, out var product)) throw new BusinessException("ShopManagement:GoodsReceiptProductNotFound");
            if (!units.TryGetValue(product.UnitId, out var unit)) throw new BusinessException("ShopManagement:GoodsReceiptProductNotFound");

            if (product.TrackSerialNumber) throw new BusinessException("ShopManagement:GoodsReceiptSerialTrackingNotSupported");

            var remaining = poItem.OrderedQuantity - poItem.ReceivedQuantity;
            if (input.ReceivedQuantity <= 0) throw new BusinessException("ShopManagement:GoodsReceiptInvalidQuantity");
            if (!unit.AllowDecimal && input.ReceivedQuantity != Math.Truncate(input.ReceivedQuantity))
                throw new BusinessException("ShopManagement:GoodsReceiptWholeQuantityRequired");
            if (input.ReceivedQuantity > remaining) throw new BusinessException("ShopManagement:GoodsReceiptQuantityExceedsRemaining");
            if (input.BonusQuantity < 0) throw new BusinessException("ShopManagement:GoodsReceiptInvalidBonusQuantity");

            itemEntities.Add(new ShopGoodsReceiptItem(
                GuidGenerator.Create(), tenantId, goodsReceiptId, poItem.Id, product.Id,
                product.Name, product.Code, unit.Name, unit.ShortName,
                poItem.OrderedQuantity, poItem.ReceivedQuantity,
                input.ReceivedQuantity, input.BonusQuantity, input.PurchasePrice, input.SalePrice,
                input.BatchNumber, input.ManufacturingDate, input.ExpiryDate,
                input.DiscountPercentage, input.TaxPercentage,
                product.TrackBatch, product.TrackExpiry, receiptDate));
        }

        return itemEntities;
    }

    private static void ValidateNoDuplicatePurchaseOrderItems(IReadOnlyList<ShopGoodsReceiptItemInput> items)
    {
        var ids = items.Select(x => x.PurchaseOrderItemId).ToList();
        if (ids.Distinct().Count() != ids.Count) throw new BusinessException("ShopManagement:GoodsReceiptPurchaseOrderItemNotFound");
    }

    private Guid RequireTenantOwnership(ShopGoodsReceipt goodsReceipt)
    {
        var tenantId = RequireTenant();
        if (goodsReceipt.TenantId != tenantId) throw new BusinessException("ShopManagement:GoodsReceiptNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}

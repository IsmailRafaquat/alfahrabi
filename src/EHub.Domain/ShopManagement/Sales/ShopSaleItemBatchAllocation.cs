using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.ProductBatches;
using EHub.ShopManagement.Products;

namespace EHub.ShopManagement.Sales;

/// <summary>
/// Records which product batch(es) a completed sale item's quantity was deducted from, so a later
/// Sale Return can restock the exact same batches instead of guessing.
/// </summary>
public class ShopSaleItemBatchAllocation : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid SaleId { get; protected set; }
    public ShopSale? Sale { get; protected set; }

    public Guid SaleItemId { get; protected set; }
    public ShopSaleItem? SaleItem { get; protected set; }

    public Guid ProductId { get; protected set; }
    public ShopProduct? Product { get; protected set; }

    public Guid ProductBatchId { get; protected set; }
    public ShopProductBatch? ProductBatch { get; protected set; }

    public string BatchNumberSnapshot { get; protected set; } = string.Empty;
    public DateTime? ExpiryDateSnapshot { get; protected set; }
    public decimal Quantity { get; protected set; }
    public decimal UnitCostSnapshot { get; protected set; }

    protected ShopSaleItemBatchAllocation() { }

    internal ShopSaleItemBatchAllocation(
        Guid id,
        Guid tenantId,
        Guid saleId,
        Guid saleItemId,
        Guid productId,
        ShopProductBatch batch,
        decimal quantity,
        decimal unitCostSnapshot) : base(id)
    {
        if (quantity <= 0) throw new BusinessException("ShopManagement:BatchQuantityCannotBeNegative");

        TenantId = tenantId;
        SaleId = saleId;
        SaleItemId = saleItemId;
        ProductId = productId;
        ProductBatchId = batch.Id;
        BatchNumberSnapshot = batch.BatchNumber;
        ExpiryDateSnapshot = batch.ExpiryDate;
        Quantity = quantity;
        UnitCostSnapshot = unitCostSnapshot;
    }
}

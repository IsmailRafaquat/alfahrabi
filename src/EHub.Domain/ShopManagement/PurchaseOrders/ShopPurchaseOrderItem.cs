using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.Products;

namespace EHub.ShopManagement.PurchaseOrders;

public class ShopPurchaseOrderItem : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid PurchaseOrderId { get; protected set; }
    public ShopPurchaseOrder? PurchaseOrder { get; protected set; }

    public Guid ProductId { get; protected set; }
    public ShopProduct? Product { get; protected set; }

    public string ProductNameSnapshot { get; protected set; } = string.Empty;
    public string ProductCodeSnapshot { get; protected set; } = string.Empty;
    public string UnitNameSnapshot { get; protected set; } = string.Empty;
    public string UnitShortNameSnapshot { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }

    public decimal OrderedQuantity { get; protected set; }
    public decimal ReceivedQuantity { get; protected set; }
    public decimal UnitPurchasePrice { get; protected set; }
    public decimal DiscountPercentage { get; protected set; }
    public decimal DiscountAmount { get; protected set; }
    public decimal TaxPercentage { get; protected set; }
    public decimal TaxAmount { get; protected set; }
    public decimal LineSubTotal { get; protected set; }
    public decimal LineTotal { get; protected set; }

    protected ShopPurchaseOrderItem() { }

    internal ShopPurchaseOrderItem(
        Guid id,
        Guid tenantId,
        Guid purchaseOrderId,
        Guid productId,
        string productNameSnapshot,
        string productCodeSnapshot,
        string unitNameSnapshot,
        string unitShortNameSnapshot,
        string? description,
        decimal orderedQuantity,
        decimal unitPurchasePrice,
        decimal discountPercentage,
        decimal taxPercentage) : base(id)
    {
        TenantId = tenantId;
        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        ProductNameSnapshot = Check.NotNullOrWhiteSpace(productNameSnapshot, nameof(productNameSnapshot), ShopPurchaseOrderConsts.ProductNameSnapshotMaxLength);
        ProductCodeSnapshot = Check.NotNullOrWhiteSpace(productCodeSnapshot, nameof(productCodeSnapshot), ShopPurchaseOrderConsts.ProductCodeSnapshotMaxLength);
        UnitNameSnapshot = Check.NotNullOrWhiteSpace(unitNameSnapshot, nameof(unitNameSnapshot), ShopPurchaseOrderConsts.UnitNameSnapshotMaxLength);
        UnitShortNameSnapshot = Check.NotNullOrWhiteSpace(unitShortNameSnapshot, nameof(unitShortNameSnapshot), ShopPurchaseOrderConsts.UnitShortNameSnapshotMaxLength);
        ReceivedQuantity = 0;
        SetLineValues(description, orderedQuantity, unitPurchasePrice, discountPercentage, taxPercentage);
    }

    internal void UpdateLineValues(string? description, decimal orderedQuantity, decimal unitPurchasePrice, decimal discountPercentage, decimal taxPercentage) =>
        SetLineValues(description, orderedQuantity, unitPurchasePrice, discountPercentage, taxPercentage);

    private void SetLineValues(string? description, decimal orderedQuantity, decimal unitPurchasePrice, decimal discountPercentage, decimal taxPercentage)
    {
        if (orderedQuantity <= 0) throw new BusinessException("ShopManagement:PurchaseOrderInvalidQuantity");
        if (unitPurchasePrice < 0) throw new BusinessException("ShopManagement:PurchaseOrderInvalidUnitPrice");
        if (discountPercentage < 0 || discountPercentage > 100) throw new BusinessException("ShopManagement:PurchaseOrderInvalidDiscountPercentage");
        if (taxPercentage < 0 || taxPercentage > 100) throw new BusinessException("ShopManagement:PurchaseOrderInvalidTaxPercentage");

        Description = Check.Length(description?.Trim(), nameof(description), ShopPurchaseOrderConsts.DescriptionMaxLength);
        OrderedQuantity = orderedQuantity;
        UnitPurchasePrice = unitPurchasePrice;
        DiscountPercentage = discountPercentage;
        TaxPercentage = taxPercentage;

        LineSubTotal = Round(orderedQuantity * unitPurchasePrice);
        DiscountAmount = Round(LineSubTotal * discountPercentage / 100);
        var taxableAmount = LineSubTotal - DiscountAmount;
        TaxAmount = Round(taxableAmount * taxPercentage / 100);
        LineTotal = Round(taxableAmount + TaxAmount);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

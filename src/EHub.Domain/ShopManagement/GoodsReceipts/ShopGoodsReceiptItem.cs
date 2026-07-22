using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseOrders;

namespace EHub.ShopManagement.GoodsReceipts;

public class ShopGoodsReceiptItem : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid GoodsReceiptId { get; protected set; }
    public ShopGoodsReceipt? GoodsReceipt { get; protected set; }

    public Guid PurchaseOrderItemId { get; protected set; }
    public ShopPurchaseOrderItem? PurchaseOrderItem { get; protected set; }

    public Guid ProductId { get; protected set; }
    public ShopProduct? Product { get; protected set; }

    public string ProductNameSnapshot { get; protected set; } = string.Empty;
    public string ProductCodeSnapshot { get; protected set; } = string.Empty;
    public string UnitNameSnapshot { get; protected set; } = string.Empty;
    public string UnitShortNameSnapshot { get; protected set; } = string.Empty;

    public decimal OrderedQuantitySnapshot { get; protected set; }
    public decimal PreviouslyReceivedQuantity { get; protected set; }
    public decimal ReceivedQuantity { get; protected set; }
    public decimal BonusQuantity { get; protected set; }

    public decimal PurchasePrice { get; protected set; }
    public decimal SalePrice { get; protected set; }

    public string? BatchNumber { get; protected set; }
    public DateTime? ManufacturingDate { get; protected set; }
    public DateTime? ExpiryDate { get; protected set; }

    public decimal DiscountPercentage { get; protected set; }
    public decimal DiscountAmount { get; protected set; }
    public decimal TaxPercentage { get; protected set; }
    public decimal TaxAmount { get; protected set; }
    public decimal LineSubTotal { get; protected set; }
    public decimal LineTotal { get; protected set; }

    protected ShopGoodsReceiptItem() { }

    internal ShopGoodsReceiptItem(
        Guid id,
        Guid tenantId,
        Guid goodsReceiptId,
        Guid purchaseOrderItemId,
        Guid productId,
        string productNameSnapshot,
        string productCodeSnapshot,
        string unitNameSnapshot,
        string unitShortNameSnapshot,
        decimal orderedQuantitySnapshot,
        decimal previouslyReceivedQuantity,
        decimal receivedQuantity,
        decimal bonusQuantity,
        decimal purchasePrice,
        decimal salePrice,
        string? batchNumber,
        DateTime? manufacturingDate,
        DateTime? expiryDate,
        decimal discountPercentage,
        decimal taxPercentage,
        bool trackBatch,
        bool trackExpiry,
        DateTime receiptDate) : base(id)
    {
        TenantId = tenantId;
        GoodsReceiptId = goodsReceiptId;
        PurchaseOrderItemId = purchaseOrderItemId;
        ProductId = productId;
        ProductNameSnapshot = Check.NotNullOrWhiteSpace(productNameSnapshot, nameof(productNameSnapshot), ShopGoodsReceiptConsts.ProductNameSnapshotMaxLength);
        ProductCodeSnapshot = Check.NotNullOrWhiteSpace(productCodeSnapshot, nameof(productCodeSnapshot), ShopGoodsReceiptConsts.ProductCodeSnapshotMaxLength);
        UnitNameSnapshot = Check.NotNullOrWhiteSpace(unitNameSnapshot, nameof(unitNameSnapshot), ShopGoodsReceiptConsts.UnitNameSnapshotMaxLength);
        UnitShortNameSnapshot = Check.NotNullOrWhiteSpace(unitShortNameSnapshot, nameof(unitShortNameSnapshot), ShopGoodsReceiptConsts.UnitShortNameSnapshotMaxLength);
        OrderedQuantitySnapshot = orderedQuantitySnapshot;
        PreviouslyReceivedQuantity = previouslyReceivedQuantity;

        SetValues(receivedQuantity, bonusQuantity, purchasePrice, salePrice, batchNumber, manufacturingDate, expiryDate,
            discountPercentage, taxPercentage, trackBatch, trackExpiry, receiptDate);
    }

    internal void UpdateLineValues(
        decimal receivedQuantity,
        decimal bonusQuantity,
        decimal purchasePrice,
        decimal salePrice,
        string? batchNumber,
        DateTime? manufacturingDate,
        DateTime? expiryDate,
        decimal discountPercentage,
        decimal taxPercentage,
        bool trackBatch,
        bool trackExpiry,
        DateTime receiptDate) =>
        SetValues(receivedQuantity, bonusQuantity, purchasePrice, salePrice, batchNumber, manufacturingDate, expiryDate,
            discountPercentage, taxPercentage, trackBatch, trackExpiry, receiptDate);

    private void SetValues(
        decimal receivedQuantity,
        decimal bonusQuantity,
        decimal purchasePrice,
        decimal salePrice,
        string? batchNumber,
        DateTime? manufacturingDate,
        DateTime? expiryDate,
        decimal discountPercentage,
        decimal taxPercentage,
        bool trackBatch,
        bool trackExpiry,
        DateTime receiptDate)
    {
        if (receivedQuantity <= 0) throw new BusinessException("ShopManagement:GoodsReceiptInvalidQuantity");
        if (bonusQuantity < 0) throw new BusinessException("ShopManagement:GoodsReceiptInvalidBonusQuantity");
        if (purchasePrice < 0) throw new BusinessException("ShopManagement:GoodsReceiptInvalidPurchasePrice");
        if (salePrice < 0) throw new BusinessException("ShopManagement:GoodsReceiptInvalidSalePrice");
        if (discountPercentage < 0 || discountPercentage > 100) throw new BusinessException("ShopManagement:GoodsReceiptInvalidDiscountPercentage");
        if (taxPercentage < 0 || taxPercentage > 100) throw new BusinessException("ShopManagement:GoodsReceiptInvalidTaxPercentage");

        var trimmedBatchNumber = Check.Length(batchNumber?.Trim(), nameof(batchNumber), ShopGoodsReceiptConsts.BatchNumberMaxLength);
        if (trackBatch && string.IsNullOrWhiteSpace(trimmedBatchNumber)) throw new BusinessException("ShopManagement:GoodsReceiptBatchNumberRequired");

        if (trackExpiry)
        {
            if (!expiryDate.HasValue) throw new BusinessException("ShopManagement:GoodsReceiptExpiryDateRequired");
            if (expiryDate.Value.Date <= receiptDate.Date) throw new BusinessException("ShopManagement:GoodsReceiptInvalidExpiryDate");
            if (manufacturingDate.HasValue && manufacturingDate.Value.Date >= expiryDate.Value.Date)
                throw new BusinessException("ShopManagement:GoodsReceiptInvalidManufacturingDate");
        }
        else if (manufacturingDate.HasValue && expiryDate.HasValue && manufacturingDate.Value.Date >= expiryDate.Value.Date)
        {
            throw new BusinessException("ShopManagement:GoodsReceiptInvalidManufacturingDate");
        }

        ReceivedQuantity = receivedQuantity;
        BonusQuantity = bonusQuantity;
        PurchasePrice = purchasePrice;
        SalePrice = salePrice;
        BatchNumber = trimmedBatchNumber;
        ManufacturingDate = manufacturingDate;
        ExpiryDate = expiryDate;
        DiscountPercentage = discountPercentage;
        TaxPercentage = taxPercentage;

        LineSubTotal = Round(receivedQuantity * purchasePrice);
        DiscountAmount = Round(LineSubTotal * discountPercentage / 100);
        var taxableAmount = LineSubTotal - DiscountAmount;
        TaxAmount = Round(taxableAmount * taxPercentage / 100);
        LineTotal = Round(taxableAmount + TaxAmount);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

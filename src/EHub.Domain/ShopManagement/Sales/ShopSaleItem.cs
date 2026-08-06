using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.Products;

namespace EHub.ShopManagement.Sales;

public class ShopSaleItem : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid SaleId { get; protected set; }
    public ShopSale? Sale { get; protected set; }

    public Guid ProductId { get; protected set; }
    public ShopProduct? Product { get; protected set; }

    public string ProductCodeSnapshot { get; protected set; } = string.Empty;
    public string ProductNameSnapshot { get; protected set; } = string.Empty;
    public string UnitNameSnapshot { get; protected set; } = string.Empty;
    public string UnitShortNameSnapshot { get; protected set; } = string.Empty;

    public decimal Quantity { get; protected set; }
    public decimal UnitSalePrice { get; protected set; }

    /// <summary>
    /// The product's cost of goods sold, captured at sale completion time (not at draft
    /// creation time, since the draft is not yet final). Zero until the sale is completed.
    /// </summary>
    public decimal UnitCostSnapshot { get; protected set; }

    public decimal DiscountPercentage { get; protected set; }
    public decimal DiscountAmount { get; protected set; }
    public decimal TaxPercentage { get; protected set; }
    public decimal TaxAmount { get; protected set; }
    public decimal LineSubTotal { get; protected set; }
    public decimal LineTotal { get; protected set; }

    public string? BatchNumber { get; protected set; }
    public DateTime? ExpiryDate { get; protected set; }

    protected ShopSaleItem() { }

    internal ShopSaleItem(
        Guid id,
        Guid tenantId,
        Guid saleId,
        Guid productId,
        string productCodeSnapshot,
        string productNameSnapshot,
        string unitNameSnapshot,
        string unitShortNameSnapshot,
        decimal quantity,
        decimal unitSalePrice,
        decimal discountPercentage,
        decimal taxPercentage,
        string? batchNumber,
        DateTime? expiryDate) : base(id)
    {
        TenantId = tenantId;
        SaleId = saleId;
        ProductId = productId;
        ProductCodeSnapshot = Check.NotNullOrWhiteSpace(productCodeSnapshot, nameof(productCodeSnapshot), ShopSaleConsts.ProductCodeSnapshotMaxLength);
        ProductNameSnapshot = Check.NotNullOrWhiteSpace(productNameSnapshot, nameof(productNameSnapshot), ShopSaleConsts.ProductNameSnapshotMaxLength);
        UnitNameSnapshot = Check.NotNullOrWhiteSpace(unitNameSnapshot, nameof(unitNameSnapshot), ShopSaleConsts.UnitNameSnapshotMaxLength);
        UnitShortNameSnapshot = Check.NotNullOrWhiteSpace(unitShortNameSnapshot, nameof(unitShortNameSnapshot), ShopSaleConsts.UnitShortNameSnapshotMaxLength);

        SetValues(quantity, unitSalePrice, discountPercentage, taxPercentage, batchNumber, expiryDate);
    }

    internal void UpdateLineValues(
        decimal quantity,
        decimal unitSalePrice,
        decimal discountPercentage,
        decimal taxPercentage,
        string? batchNumber,
        DateTime? expiryDate) =>
        SetValues(quantity, unitSalePrice, discountPercentage, taxPercentage, batchNumber, expiryDate);

    internal void SetCostSnapshot(decimal unitCostSnapshot)
    {
        UnitCostSnapshot = unitCostSnapshot;
    }

    private void SetValues(
        decimal quantity,
        decimal unitSalePrice,
        decimal discountPercentage,
        decimal taxPercentage,
        string? batchNumber,
        DateTime? expiryDate)
    {
        if (quantity <= 0) throw new BusinessException("ShopManagement:SaleInvalidQuantity");
        if (unitSalePrice < 0) throw new BusinessException("ShopManagement:SaleInvalidUnitPrice");
        if (discountPercentage < 0 || discountPercentage > 100) throw new BusinessException("ShopManagement:SaleInvalidDiscountPercentage");
        if (taxPercentage < 0 || taxPercentage > 100) throw new BusinessException("ShopManagement:SaleInvalidTaxPercentage");

        Quantity = quantity;
        UnitSalePrice = unitSalePrice;
        DiscountPercentage = discountPercentage;
        TaxPercentage = taxPercentage;
        BatchNumber = Check.Length(batchNumber?.Trim(), nameof(batchNumber), ShopSaleConsts.BatchNumberMaxLength);
        ExpiryDate = expiryDate;

        LineSubTotal = Round(quantity * unitSalePrice);
        DiscountAmount = Round(LineSubTotal * discountPercentage / 100);
        var taxableAmount = LineSubTotal - DiscountAmount;
        TaxAmount = Round(taxableAmount * taxPercentage / 100);
        LineTotal = Round(taxableAmount + TaxAmount);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

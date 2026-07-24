using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.Sales;

namespace EHub.ShopManagement.SaleReturns;

public class ShopSaleReturnItem : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid SaleReturnId { get; protected set; }
    public ShopSaleReturn? SaleReturn { get; protected set; }

    public Guid SaleItemId { get; protected set; }
    public ShopSaleItem? SaleItem { get; protected set; }

    public Guid ProductId { get; protected set; }

    public string ProductCodeSnapshot { get; protected set; } = string.Empty;
    public string ProductNameSnapshot { get; protected set; } = string.Empty;
    public string UnitNameSnapshot { get; protected set; } = string.Empty;
    public string UnitShortNameSnapshot { get; protected set; } = string.Empty;

    public string? BatchNumber { get; protected set; }
    public DateTime? ExpiryDate { get; protected set; }

    public decimal SoldQuantitySnapshot { get; protected set; }
    public decimal PreviouslyReturnedQuantity { get; protected set; }
    public decimal ReturnQuantity { get; protected set; }

    public decimal UnitSalePrice { get; protected set; }
    public decimal UnitCostSnapshot { get; protected set; }
    public decimal DiscountPercentage { get; protected set; }
    public decimal DiscountAmount { get; protected set; }
    public decimal TaxPercentage { get; protected set; }
    public decimal TaxAmount { get; protected set; }
    public decimal LineSubTotal { get; protected set; }
    public decimal LineTotal { get; protected set; }

    public ShopSaleReturnReason Reason { get; protected set; }
    public string? Notes { get; protected set; }

    protected ShopSaleReturnItem() { }

    internal ShopSaleReturnItem(
        Guid id,
        Guid tenantId,
        Guid saleReturnId,
        ShopSaleItem source,
        decimal previouslyReturnedQuantity,
        decimal returnQuantity,
        bool unitAllowDecimal,
        ShopSaleReturnReason reason,
        string? notes) : base(id)
    {
        if (returnQuantity <= 0) throw new BusinessException("ShopManagement:SaleReturnInvalidQuantity");

        var returnable = source.Quantity - previouslyReturnedQuantity;
        if (returnQuantity > returnable) throw new BusinessException("ShopManagement:SaleReturnQuantityExceedsReturnable");

        if (!unitAllowDecimal && returnQuantity != decimal.Truncate(returnQuantity))
            throw new BusinessException("ShopManagement:SaleReturnWholeQuantityRequired");

        TenantId = tenantId;
        SaleReturnId = saleReturnId;
        SaleItemId = source.Id;
        ProductId = source.ProductId;
        ProductCodeSnapshot = source.ProductCodeSnapshot;
        ProductNameSnapshot = source.ProductNameSnapshot;
        UnitNameSnapshot = source.UnitNameSnapshot;
        UnitShortNameSnapshot = source.UnitShortNameSnapshot;
        BatchNumber = source.BatchNumber;
        ExpiryDate = source.ExpiryDate;
        SoldQuantitySnapshot = source.Quantity;
        PreviouslyReturnedQuantity = previouslyReturnedQuantity;
        ReturnQuantity = returnQuantity;
        UnitSalePrice = source.UnitSalePrice;
        UnitCostSnapshot = source.UnitCostSnapshot;
        DiscountPercentage = source.DiscountPercentage;
        TaxPercentage = source.TaxPercentage;
        Reason = reason;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopSaleReturnConsts.NotesMaxLength);

        LineSubTotal = Round(returnQuantity * UnitSalePrice);
        DiscountAmount = Round(LineSubTotal * DiscountPercentage / 100);
        var taxableAmount = LineSubTotal - DiscountAmount;
        TaxAmount = Round(taxableAmount * TaxPercentage / 100);
        LineTotal = Round(taxableAmount + TaxAmount);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

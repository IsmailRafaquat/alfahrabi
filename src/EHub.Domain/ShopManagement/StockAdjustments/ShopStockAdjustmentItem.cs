using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.Products;

namespace EHub.ShopManagement.StockAdjustments;

public class ShopStockAdjustmentItem : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid StockAdjustmentId { get; protected set; }
    public ShopStockAdjustment? StockAdjustment { get; protected set; }

    public Guid ProductId { get; protected set; }
    public ShopProduct? Product { get; protected set; }

    public string ProductCodeSnapshot { get; protected set; } = string.Empty;
    public string ProductNameSnapshot { get; protected set; } = string.Empty;
    public string UnitNameSnapshot { get; protected set; } = string.Empty;
    public string UnitShortNameSnapshot { get; protected set; } = string.Empty;

    public ShopStockAdjustmentType AdjustmentType { get; protected set; }
    public decimal SystemQuantitySnapshot { get; protected set; }
    public decimal AdjustmentQuantity { get; protected set; }
    public decimal FinalQuantity { get; protected set; }
    public decimal UnitCostSnapshot { get; protected set; }

    public string? BatchNumber { get; protected set; }
    public DateTime? ExpiryDate { get; protected set; }

    public ShopStockAdjustmentReason Reason { get; protected set; }
    public string? Notes { get; protected set; }

    protected ShopStockAdjustmentItem() { }

    internal ShopStockAdjustmentItem(
        Guid id,
        Guid tenantId,
        Guid stockAdjustmentId,
        ShopProduct product,
        string unitName,
        string unitShortName,
        bool unitAllowDecimal,
        ShopStockAdjustmentType adjustmentType,
        decimal systemQuantitySnapshot,
        decimal adjustmentQuantity,
        decimal unitCostSnapshot,
        string? batchNumber,
        DateTime? expiryDate,
        ShopStockAdjustmentReason reason,
        string? notes) : base(id)
    {
        TenantId = tenantId;
        StockAdjustmentId = stockAdjustmentId;
        ProductId = product.Id;
        ProductCodeSnapshot = product.Code;
        ProductNameSnapshot = product.Name;
        UnitNameSnapshot = unitName;
        UnitShortNameSnapshot = unitShortName;
        UnitCostSnapshot = unitCostSnapshot;
        Reason = reason;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopStockAdjustmentConsts.ItemNotesMaxLength);

        SetQuantities(adjustmentType, systemQuantitySnapshot, adjustmentQuantity, unitAllowDecimal);
        BatchNumber = Check.Length(batchNumber?.Trim(), nameof(batchNumber), ShopStockAdjustmentConsts.BatchNumberMaxLength);
        ExpiryDate = expiryDate;
    }

    /// <summary>
    /// Re-derives the system quantity snapshot and final quantity from the product's live stock at
    /// posting time, since the value captured when the draft was created/edited may be stale.
    /// </summary>
    internal void RefreshForPosting(decimal currentSystemQuantity, bool unitAllowDecimal)
    {
        SetQuantities(AdjustmentType, currentSystemQuantity, AdjustmentQuantity, unitAllowDecimal);
    }

    private void SetQuantities(ShopStockAdjustmentType adjustmentType, decimal systemQuantitySnapshot, decimal adjustmentQuantity, bool unitAllowDecimal)
    {
        if (adjustmentQuantity <= 0) throw new BusinessException("ShopManagement:StockAdjustmentInvalidQuantity");
        if (!unitAllowDecimal && adjustmentQuantity != decimal.Truncate(adjustmentQuantity))
            throw new BusinessException("ShopManagement:StockAdjustmentWholeQuantityRequired");

        AdjustmentType = adjustmentType;
        SystemQuantitySnapshot = systemQuantitySnapshot;
        AdjustmentQuantity = adjustmentQuantity;

        if (adjustmentType == ShopStockAdjustmentType.Decrease)
        {
            if (adjustmentQuantity > systemQuantitySnapshot) throw new BusinessException("ShopManagement:StockAdjustmentInsufficientStock");
            FinalQuantity = systemQuantitySnapshot - adjustmentQuantity;
        }
        else
        {
            FinalQuantity = systemQuantitySnapshot + adjustmentQuantity;
        }
    }
}

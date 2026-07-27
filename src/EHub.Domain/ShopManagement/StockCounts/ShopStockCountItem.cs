using System;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.StockAdjustments;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.StockCounts;

public class ShopStockCountItem : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid StockCountId { get; protected set; }
    public ShopStockCount? StockCount { get; protected set; }

    public Guid ProductId { get; protected set; }
    public ShopProduct? Product { get; protected set; }

    public string ProductCodeSnapshot { get; protected set; } = string.Empty;
    public string ProductNameSnapshot { get; protected set; } = string.Empty;
    public string UnitNameSnapshot { get; protected set; } = string.Empty;
    public string UnitShortNameSnapshot { get; protected set; } = string.Empty;

    public decimal SystemQuantitySnapshot { get; protected set; }
    public decimal? PhysicalQuantity { get; protected set; }
    public decimal DifferenceQuantity { get; protected set; }
    public ShopStockAdjustmentType? AdjustmentType { get; protected set; }
    public bool IsCounted { get; protected set; }
    public Guid? CountedByUserId { get; protected set; }
    public DateTime? CountedDate { get; protected set; }
    public string? Notes { get; protected set; }

    protected ShopStockCountItem() { }

    internal ShopStockCountItem(
        Guid id,
        Guid tenantId,
        Guid stockCountId,
        ShopProduct product,
        string unitName,
        string unitShortName,
        decimal systemQuantitySnapshot) : base(id)
    {
        TenantId = tenantId;
        StockCountId = stockCountId;
        ProductId = product.Id;
        ProductCodeSnapshot = product.Code;
        ProductNameSnapshot = product.Name;
        UnitNameSnapshot = unitName;
        UnitShortNameSnapshot = unitShortName;
        SystemQuantitySnapshot = systemQuantitySnapshot;
        DifferenceQuantity = 0;
        IsCounted = false;
    }

    internal void SetPhysicalQuantity(decimal physicalQuantity, string? notes, bool unitAllowDecimal, Guid countedByUserId, DateTime countedDate)
    {
        if (physicalQuantity < 0) throw new BusinessException("ShopManagement:StockCountPhysicalQuantityCannotBeNegative");
        if (!unitAllowDecimal && physicalQuantity != decimal.Truncate(physicalQuantity))
            throw new BusinessException("ShopManagement:StockCountWholeQuantityRequired");

        PhysicalQuantity = physicalQuantity;
        DifferenceQuantity = physicalQuantity - SystemQuantitySnapshot;
        AdjustmentType = DifferenceQuantity switch
        {
            > 0 => ShopStockAdjustmentType.Increase,
            < 0 => ShopStockAdjustmentType.Decrease,
            _ => null
        };
        IsCounted = true;
        CountedByUserId = countedByUserId;
        CountedDate = countedDate;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopStockCountConsts.ItemNotesMaxLength);
    }
}

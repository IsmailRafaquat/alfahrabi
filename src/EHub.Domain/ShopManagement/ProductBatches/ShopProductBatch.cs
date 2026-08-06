using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.Products;

namespace EHub.ShopManagement.ProductBatches;

public class ShopProductBatch : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid ProductId { get; protected set; }
    public ShopProduct? Product { get; protected set; }

    public string BatchNumber { get; protected set; } = string.Empty;
    public string NormalizedBatchNumber { get; protected set; } = string.Empty;
    public DateTime? ManufacturingDate { get; protected set; }
    public DateTime? ExpiryDate { get; protected set; }

    public decimal ReceivedQuantity { get; protected set; }
    public decimal IssuedQuantity { get; protected set; }
    public decimal AvailableQuantity { get; protected set; }
    public decimal ReservedQuantity { get; protected set; }
    public decimal UnitCost { get; protected set; }

    public ShopProductBatchStatus Status { get; protected set; } = ShopProductBatchStatus.Active;

    public DateTime? FirstReceivedDate { get; protected set; }
    public DateTime? LastMovementDate { get; protected set; }

    public Guid? SupplierId { get; protected set; }
    public Guid? GoodsReceiptId { get; protected set; }
    public Guid? GoodsReceiptItemId { get; protected set; }

    public string? Notes { get; protected set; }
    public bool IsBlocked { get; protected set; }
    public string? BlockReason { get; protected set; }

    protected ShopProductBatch() { }

    internal ShopProductBatch(
        Guid id,
        Guid tenantId,
        Guid productId,
        string batchNumber,
        string normalizedBatchNumber,
        DateTime? manufacturingDate,
        DateTime? expiryDate,
        Guid? supplierId,
        Guid? goodsReceiptId,
        Guid? goodsReceiptItemId) : base(id)
    {
        TenantId = tenantId;
        ProductId = productId;
        BatchNumber = Check.NotNullOrWhiteSpace(batchNumber, nameof(batchNumber), ShopProductBatchConsts.BatchNumberMaxLength);
        NormalizedBatchNumber = Check.NotNullOrWhiteSpace(normalizedBatchNumber, nameof(normalizedBatchNumber), ShopProductBatchConsts.NormalizedBatchNumberMaxLength);
        ValidateDates(manufacturingDate, expiryDate);
        ManufacturingDate = manufacturingDate;
        ExpiryDate = expiryDate;
        SupplierId = supplierId;
        GoodsReceiptId = goodsReceiptId;
        GoodsReceiptItemId = goodsReceiptItemId;
        ReceivedQuantity = 0;
        IssuedQuantity = 0;
        AvailableQuantity = 0;
        ReservedQuantity = 0;
        UnitCost = 0;
        Status = ShopProductBatchStatus.Active;
    }

    internal void EnsureExpiryConsistent(DateTime? manufacturingDate, DateTime? expiryDate)
    {
        // An existing batch found by (Product, BatchNumber) must not silently accept a conflicting
        // expiry from a later receipt - that would corrupt the FEFO ordering of everything already
        // allocated against the original date.
        if (expiryDate.HasValue != ExpiryDate.HasValue || (expiryDate.HasValue && expiryDate.Value.Date != ExpiryDate!.Value.Date))
            throw new BusinessException("ShopManagement:BatchExpiryConflict").WithData("BatchNumber", BatchNumber);
    }

    internal void AddStock(decimal quantity, decimal unitCost, DateTime movementDate)
    {
        if (quantity <= 0) throw new BusinessException("ShopManagement:BatchQuantityCannotBeNegative");
        if (unitCost < 0) throw new BusinessException("ShopManagement:BatchQuantityCannotBeNegative");

        ReceivedQuantity += quantity;
        AvailableQuantity += quantity;
        UnitCost = unitCost;
        FirstReceivedDate ??= movementDate;
        LastMovementDate = movementDate;
    }

    internal void RemoveStock(decimal quantity, DateTime movementDate)
    {
        if (quantity <= 0) throw new BusinessException("ShopManagement:BatchQuantityCannotBeNegative");
        if (AvailableQuantity < quantity) throw new BusinessException("ShopManagement:BatchInsufficientStock").WithData("BatchNumber", BatchNumber);

        IssuedQuantity += quantity;
        AvailableQuantity -= quantity;
        LastMovementDate = movementDate;
    }

    internal void ReturnIssuedStock(decimal quantity, DateTime movementDate)
    {
        if (quantity <= 0) throw new BusinessException("ShopManagement:BatchQuantityCannotBeNegative");
        if (quantity > IssuedQuantity) throw new BusinessException("ShopManagement:BatchAllocationExceedsAvailableStock").WithData("BatchNumber", BatchNumber);

        IssuedQuantity -= quantity;
        AvailableQuantity += quantity;
        LastMovementDate = movementDate;
    }

    internal void UpdateMetadata(DateTime? manufacturingDate, DateTime? expiryDate, string? notes)
    {
        var datesChanged = manufacturingDate != ManufacturingDate || expiryDate != ExpiryDate;

        // Once a batch has been issued from (sale, adjustment, etc.), its ManufacturingDate/ExpiryDate
        // are already baked into posted, immutable Stock Transaction snapshots and FEFO ordering that
        // already happened - silently changing them here would make those historical records lie.
        if (datesChanged && IssuedQuantity > 0)
            throw new BusinessException("ShopManagement:BatchHasInventoryHistory").WithData("BatchNumber", BatchNumber);

        ValidateDates(manufacturingDate, expiryDate);
        ManufacturingDate = manufacturingDate;
        ExpiryDate = expiryDate;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopProductBatchConsts.NotesMaxLength);
    }

    internal void Block(string reason)
    {
        IsBlocked = true;
        BlockReason = Check.NotNullOrWhiteSpace(reason, nameof(reason), ShopProductBatchConsts.BlockReasonMaxLength).Trim();
        Status = ShopProductBatchStatus.Blocked;
    }

    internal void Unblock()
    {
        IsBlocked = false;
        BlockReason = null;
    }

    internal void SetStatus(ShopProductBatchStatus status)
    {
        if (IsBlocked)
        {
            Status = ShopProductBatchStatus.Blocked;
            return;
        }

        Status = status;
    }

    private static void ValidateDates(DateTime? manufacturingDate, DateTime? expiryDate)
    {
        if (manufacturingDate.HasValue && expiryDate.HasValue && manufacturingDate.Value.Date > expiryDate.Value.Date)
            throw new BusinessException("ShopManagement:InvalidManufacturingDate");
    }
}

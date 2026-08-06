using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.StockAdjustments;

public class ShopStockAdjustment : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string AdjustmentNumber { get; protected set; } = string.Empty;
    public DateTime AdjustmentDate { get; protected set; }
    public ShopStockAdjustmentStatus Status { get; protected set; } = ShopStockAdjustmentStatus.Draft;
    public ShopStockAdjustmentReason Reason { get; protected set; }
    public string? ReasonDetails { get; protected set; }
    public string? Notes { get; protected set; }

    public Guid? PostedByUserId { get; protected set; }
    public DateTime? PostedDate { get; protected set; }
    public Guid? CancelledByUserId { get; protected set; }
    public DateTime? CancelledDate { get; protected set; }
    public string? CancellationReason { get; protected set; }

    public ICollection<ShopStockAdjustmentItem> Items { get; protected set; } = new List<ShopStockAdjustmentItem>();

    protected ShopStockAdjustment() { }

    internal ShopStockAdjustment(
        Guid id,
        Guid tenantId,
        string adjustmentNumber,
        DateTime adjustmentDate,
        ShopStockAdjustmentReason reason,
        string? reasonDetails,
        string? notes,
        List<ShopStockAdjustmentItem> items) : base(id)
    {
        TenantId = tenantId;
        AdjustmentNumber = Check.NotNullOrWhiteSpace(adjustmentNumber, nameof(adjustmentNumber), ShopStockAdjustmentConsts.AdjustmentNumberMaxLength);
        Status = ShopStockAdjustmentStatus.Draft;
        SetValues(adjustmentDate, reason, reasonDetails, notes, items);
    }

    internal void Update(
        DateTime adjustmentDate,
        ShopStockAdjustmentReason reason,
        string? reasonDetails,
        string? notes,
        List<ShopStockAdjustmentItem> items)
    {
        EnsureEditable();
        SetValues(adjustmentDate, reason, reasonDetails, notes, items);
    }

    internal void MarkAsPosted(Guid postedByUserId, DateTime postedDate)
    {
        EnsurePostable();
        Status = ShopStockAdjustmentStatus.Posted;
        PostedByUserId = postedByUserId;
        PostedDate = postedDate;
    }

    internal void MarkAsCancelled(Guid cancelledByUserId, DateTime cancelledDate, string cancellationReason)
    {
        EnsureCancellable();
        Status = ShopStockAdjustmentStatus.Cancelled;
        CancelledByUserId = cancelledByUserId;
        CancelledDate = cancelledDate;
        CancellationReason = Check.NotNullOrWhiteSpace(cancellationReason, nameof(cancellationReason), ShopStockAdjustmentConsts.CancellationReasonMaxLength).Trim();
    }

    internal void EnsureEditable()
    {
        if (Status != ShopStockAdjustmentStatus.Draft) throw new BusinessException("ShopManagement:StockAdjustmentCannotBeEdited");
    }

    internal void EnsureDeletable()
    {
        if (Status != ShopStockAdjustmentStatus.Draft) throw new BusinessException("ShopManagement:StockAdjustmentCannotBeDeleted");
    }

    internal void EnsurePostable()
    {
        if (Status == ShopStockAdjustmentStatus.Posted) throw new BusinessException("ShopManagement:StockAdjustmentAlreadyPosted");
        if (Status != ShopStockAdjustmentStatus.Draft) throw new BusinessException("ShopManagement:StockAdjustmentCannotBePosted");
    }

    private void EnsureCancellable()
    {
        if (Status != ShopStockAdjustmentStatus.Draft) throw new BusinessException("ShopManagement:StockAdjustmentCannotBeCancelled");
    }

    private void SetValues(
        DateTime adjustmentDate,
        ShopStockAdjustmentReason reason,
        string? reasonDetails,
        string? notes,
        List<ShopStockAdjustmentItem> items)
    {
        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:StockAdjustmentRequiresItems");

        AdjustmentDate = adjustmentDate;
        Reason = reason;
        ReasonDetails = Check.Length(reasonDetails?.Trim(), nameof(reasonDetails), ShopStockAdjustmentConsts.ReasonDetailsMaxLength);
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopStockAdjustmentConsts.NotesMaxLength);

        Items.Clear();
        foreach (var item in items) Items.Add(item);
    }
}

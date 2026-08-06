using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.StockCounts;

public class ShopStockCount : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string StockCountNumber { get; protected set; } = string.Empty;
    public DateTime CountDate { get; protected set; }
    public ShopStockCountScope Scope { get; protected set; }
    public Guid? ProductCategoryId { get; protected set; }
    public ShopStockCountStatus Status { get; protected set; } = ShopStockCountStatus.Draft;
    public string? Notes { get; protected set; }
    public Guid? GeneratedStockAdjustmentId { get; protected set; }

    public Guid? StartedByUserId { get; protected set; }
    public DateTime? StartedDate { get; protected set; }
    public Guid? CountedByUserId { get; protected set; }
    public DateTime? CountedDate { get; protected set; }
    public Guid? PostedByUserId { get; protected set; }
    public DateTime? PostedDate { get; protected set; }
    public Guid? CancelledByUserId { get; protected set; }
    public DateTime? CancelledDate { get; protected set; }
    public string? CancellationReason { get; protected set; }

    public ICollection<ShopStockCountItem> Items { get; protected set; } = new List<ShopStockCountItem>();

    protected ShopStockCount() { }

    internal ShopStockCount(
        Guid id,
        Guid tenantId,
        string stockCountNumber,
        DateTime countDate,
        ShopStockCountScope scope,
        Guid? productCategoryId,
        string? notes,
        List<ShopStockCountItem> items) : base(id)
    {
        TenantId = tenantId;
        StockCountNumber = Check.NotNullOrWhiteSpace(stockCountNumber, nameof(stockCountNumber), ShopStockCountConsts.StockCountNumberMaxLength);
        Status = ShopStockCountStatus.Draft;
        SetValues(countDate, scope, productCategoryId, notes, items);
    }

    internal void Update(
        DateTime countDate,
        ShopStockCountScope scope,
        Guid? productCategoryId,
        string? notes,
        List<ShopStockCountItem> items)
    {
        EnsureEditable();
        SetValues(countDate, scope, productCategoryId, notes, items);
    }

    internal void MarkAsStarted(Guid startedByUserId, DateTime startedDate)
    {
        if (Status != ShopStockCountStatus.Draft) throw new BusinessException("ShopManagement:StockCountAlreadyStarted");
        Status = ShopStockCountStatus.InProgress;
        StartedByUserId = startedByUserId;
        StartedDate = startedDate;
    }

    internal void MarkAsCounted(Guid countedByUserId, DateTime countedDate)
    {
        if (Status != ShopStockCountStatus.InProgress) throw new BusinessException("ShopManagement:StockCountInvalidStatus");
        Status = ShopStockCountStatus.Counted;
        CountedByUserId = countedByUserId;
        CountedDate = countedDate;
    }

    internal void MarkAsPosted(Guid postedByUserId, DateTime postedDate, Guid? generatedStockAdjustmentId)
    {
        EnsurePostable();
        Status = ShopStockCountStatus.Posted;
        PostedByUserId = postedByUserId;
        PostedDate = postedDate;
        GeneratedStockAdjustmentId = generatedStockAdjustmentId;
    }

    internal void MarkAsCancelled(Guid cancelledByUserId, DateTime cancelledDate, string cancellationReason)
    {
        EnsureCancellable();
        Status = ShopStockCountStatus.Cancelled;
        CancelledByUserId = cancelledByUserId;
        CancelledDate = cancelledDate;
        CancellationReason = Check.NotNullOrWhiteSpace(cancellationReason, nameof(cancellationReason), ShopStockCountConsts.CancellationReasonMaxLength).Trim();
    }

    internal void EnsureEditable()
    {
        if (Status != ShopStockCountStatus.Draft) throw new BusinessException("ShopManagement:StockCountCannotBeEdited");
    }

    internal void EnsureDeletable()
    {
        if (Status != ShopStockCountStatus.Draft) throw new BusinessException("ShopManagement:StockCountCannotBeDeleted");
    }

    internal void EnsurePostable()
    {
        if (Status == ShopStockCountStatus.Posted) throw new BusinessException("ShopManagement:StockCountAlreadyPosted");
        if (Status != ShopStockCountStatus.Counted) throw new BusinessException("ShopManagement:StockCountCannotBePosted");
    }

    private void EnsureCancellable()
    {
        if (Status is ShopStockCountStatus.Posted or ShopStockCountStatus.Cancelled)
            throw new BusinessException("ShopManagement:StockCountCannotBeCancelled");
    }

    private void SetValues(
        DateTime countDate,
        ShopStockCountScope scope,
        Guid? productCategoryId,
        string? notes,
        List<ShopStockCountItem> items)
    {
        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:StockCountRequiresProducts");

        CountDate = countDate;
        Scope = scope;
        ProductCategoryId = productCategoryId;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopStockCountConsts.NotesMaxLength);

        Items.Clear();
        foreach (var item in items) Items.Add(item);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.Suppliers;

namespace EHub.ShopManagement.PurchaseOrders;

public class ShopPurchaseOrder : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string PurchaseOrderNumber { get; protected set; } = string.Empty;

    public Guid SupplierId { get; protected set; }
    public ShopSupplier? Supplier { get; protected set; }

    public DateTime OrderDate { get; protected set; }
    public DateTime? ExpectedDeliveryDate { get; protected set; }
    public ShopPurchaseOrderStatus Status { get; protected set; } = ShopPurchaseOrderStatus.Draft;
    public string? SupplierReference { get; protected set; }

    public decimal SubTotal { get; protected set; }
    public decimal DiscountAmount { get; protected set; }
    public decimal TaxAmount { get; protected set; }
    public decimal ShippingCharges { get; protected set; }
    public decimal OtherCharges { get; protected set; }
    public decimal GrandTotal { get; protected set; }
    public string? Notes { get; protected set; }

    public Guid? ApprovedByUserId { get; protected set; }
    public DateTime? ApprovedDate { get; protected set; }
    public Guid? RejectedByUserId { get; protected set; }
    public DateTime? RejectedDate { get; protected set; }
    public string? RejectionReason { get; protected set; }
    public Guid? CancelledByUserId { get; protected set; }
    public DateTime? CancelledDate { get; protected set; }
    public string? CancellationReason { get; protected set; }

    public ICollection<ShopPurchaseOrderItem> Items { get; protected set; } = new List<ShopPurchaseOrderItem>();

    protected ShopPurchaseOrder() { }

    internal ShopPurchaseOrder(
        Guid id,
        Guid tenantId,
        string purchaseOrderNumber,
        Guid supplierId,
        DateTime orderDate,
        DateTime? expectedDeliveryDate,
        string? supplierReference,
        decimal shippingCharges,
        decimal otherCharges,
        string? notes,
        List<ShopPurchaseOrderItem> items) : base(id)
    {
        TenantId = tenantId;
        PurchaseOrderNumber = purchaseOrderNumber;
        SupplierId = supplierId;
        Status = ShopPurchaseOrderStatus.Draft;
        SetHeaderValues(orderDate, expectedDeliveryDate, supplierReference, shippingCharges, otherCharges, notes);
        ReplaceItems(items);
    }

    internal void UpdateHeaderAndItems(
        Guid supplierId,
        DateTime orderDate,
        DateTime? expectedDeliveryDate,
        string? supplierReference,
        decimal shippingCharges,
        decimal otherCharges,
        string? notes,
        List<ShopPurchaseOrderItem> items)
    {
        SupplierId = supplierId;
        SetHeaderValues(orderDate, expectedDeliveryDate, supplierReference, shippingCharges, otherCharges, notes);
        ReplaceItems(items);
    }

    internal void MarkAsPendingApproval()
    {
        if (Status != ShopPurchaseOrderStatus.Draft) throw new BusinessException("ShopManagement:PurchaseOrderCannotBeSubmitted");
        if (Items.Count == 0) throw new BusinessException("ShopManagement:PurchaseOrderRequiresItems");
        Status = ShopPurchaseOrderStatus.PendingApproval;
    }

    internal void MarkAsApproved(Guid approvedByUserId, DateTime approvedDate)
    {
        if (Status != ShopPurchaseOrderStatus.PendingApproval) throw new BusinessException("ShopManagement:PurchaseOrderCannotBeApproved");
        Status = ShopPurchaseOrderStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedDate = approvedDate;
    }

    internal void MarkAsRejected(Guid rejectedByUserId, DateTime rejectedDate, string rejectionReason)
    {
        if (Status != ShopPurchaseOrderStatus.PendingApproval) throw new BusinessException("ShopManagement:PurchaseOrderCannotBeRejected");
        Status = ShopPurchaseOrderStatus.Rejected;
        RejectedByUserId = rejectedByUserId;
        RejectedDate = rejectedDate;
        RejectionReason = Check.NotNullOrWhiteSpace(rejectionReason, nameof(rejectionReason), ShopPurchaseOrderConsts.RejectionReasonMaxLength).Trim();
    }

    internal void MarkAsCancelled(Guid cancelledByUserId, DateTime cancelledDate, string cancellationReason)
    {
        if (Status != ShopPurchaseOrderStatus.Draft && Status != ShopPurchaseOrderStatus.PendingApproval && Status != ShopPurchaseOrderStatus.Approved)
            throw new BusinessException("ShopManagement:PurchaseOrderCannotBeCancelled");
        Status = ShopPurchaseOrderStatus.Cancelled;
        CancelledByUserId = cancelledByUserId;
        CancelledDate = cancelledDate;
        CancellationReason = Check.NotNullOrWhiteSpace(cancellationReason, nameof(cancellationReason), ShopPurchaseOrderConsts.CancellationReasonMaxLength).Trim();
    }

    internal void EnsureEditable()
    {
        if (Status != ShopPurchaseOrderStatus.Draft) throw new BusinessException("ShopManagement:PurchaseOrderCannotBeEdited");
    }

    internal void EnsureDeletable()
    {
        if (Status != ShopPurchaseOrderStatus.Draft) throw new BusinessException("ShopManagement:PurchaseOrderCannotBeDeleted");
    }

    private void SetHeaderValues(
        DateTime orderDate,
        DateTime? expectedDeliveryDate,
        string? supplierReference,
        decimal shippingCharges,
        decimal otherCharges,
        string? notes)
    {
        if (expectedDeliveryDate.HasValue && expectedDeliveryDate.Value.Date < orderDate.Date)
            throw new BusinessException("ShopManagement:PurchaseOrderInvalidExpectedDeliveryDate");
        if (shippingCharges < 0) throw new BusinessException("ShopManagement:PurchaseOrderInvalidShippingCharges");
        if (otherCharges < 0) throw new BusinessException("ShopManagement:PurchaseOrderInvalidOtherCharges");

        OrderDate = orderDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        SupplierReference = Check.Length(supplierReference?.Trim(), nameof(supplierReference), ShopPurchaseOrderConsts.SupplierReferenceMaxLength);
        ShippingCharges = shippingCharges;
        OtherCharges = otherCharges;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopPurchaseOrderConsts.NotesMaxLength);
    }

    private void ReplaceItems(List<ShopPurchaseOrderItem> items)
    {
        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:PurchaseOrderRequiresItems");

        Items.Clear();
        foreach (var item in items) Items.Add(item);

        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        SubTotal = Round(Items.Sum(x => x.LineSubTotal));
        DiscountAmount = Round(Items.Sum(x => x.DiscountAmount));
        TaxAmount = Round(Items.Sum(x => x.TaxAmount));
        GrandTotal = Round(SubTotal - DiscountAmount + TaxAmount + ShippingCharges + OtherCharges);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

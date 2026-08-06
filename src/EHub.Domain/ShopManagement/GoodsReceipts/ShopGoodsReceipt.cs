using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.Suppliers;

namespace EHub.ShopManagement.GoodsReceipts;

public class ShopGoodsReceipt : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string GoodsReceiptNumber { get; protected set; } = string.Empty;

    public Guid PurchaseOrderId { get; protected set; }
    public ShopPurchaseOrder? PurchaseOrder { get; protected set; }

    public Guid SupplierId { get; protected set; }
    public ShopSupplier? Supplier { get; protected set; }

    public string? SupplierInvoiceNumber { get; protected set; }
    public DateTime ReceiptDate { get; protected set; }
    public ShopGoodsReceiptStatus Status { get; protected set; } = ShopGoodsReceiptStatus.Draft;

    public decimal SubTotal { get; protected set; }
    public decimal DiscountAmount { get; protected set; }
    public decimal TaxAmount { get; protected set; }
    public decimal ShippingCharges { get; protected set; }
    public decimal OtherCharges { get; protected set; }
    public decimal GrandTotal { get; protected set; }
    public string? Notes { get; protected set; }

    public Guid? ReceivedByUserId { get; protected set; }
    public Guid? CompletedByUserId { get; protected set; }
    public DateTime? CompletedDate { get; protected set; }
    public Guid? CancelledByUserId { get; protected set; }
    public DateTime? CancelledDate { get; protected set; }
    public string? CancellationReason { get; protected set; }

    public ICollection<ShopGoodsReceiptItem> Items { get; protected set; } = new List<ShopGoodsReceiptItem>();

    protected ShopGoodsReceipt() { }

    internal ShopGoodsReceipt(
        Guid id,
        Guid tenantId,
        string goodsReceiptNumber,
        Guid purchaseOrderId,
        Guid supplierId,
        Guid? receivedByUserId,
        string? supplierInvoiceNumber,
        DateTime receiptDate,
        decimal shippingCharges,
        decimal otherCharges,
        string? notes,
        List<ShopGoodsReceiptItem> items) : base(id)
    {
        TenantId = tenantId;
        GoodsReceiptNumber = goodsReceiptNumber;
        PurchaseOrderId = purchaseOrderId;
        SupplierId = supplierId;
        ReceivedByUserId = receivedByUserId;
        Status = ShopGoodsReceiptStatus.Draft;
        SetHeaderValues(supplierInvoiceNumber, receiptDate, shippingCharges, otherCharges, notes);
        ReplaceItems(items);
    }

    internal void UpdateHeaderAndItems(
        string? supplierInvoiceNumber,
        DateTime receiptDate,
        decimal shippingCharges,
        decimal otherCharges,
        string? notes,
        List<ShopGoodsReceiptItem> items)
    {
        SetHeaderValues(supplierInvoiceNumber, receiptDate, shippingCharges, otherCharges, notes);
        ReplaceItems(items);
    }

    internal void MarkAsCompleted(Guid completedByUserId, DateTime completedDate)
    {
        if (Status != ShopGoodsReceiptStatus.Draft) throw new BusinessException("ShopManagement:GoodsReceiptCannotBeCompleted");
        Status = ShopGoodsReceiptStatus.Completed;
        CompletedByUserId = completedByUserId;
        CompletedDate = completedDate;
    }

    internal void MarkAsCancelled(Guid cancelledByUserId, DateTime cancelledDate, string cancellationReason)
    {
        if (Status != ShopGoodsReceiptStatus.Draft) throw new BusinessException("ShopManagement:GoodsReceiptCannotBeCancelled");
        Status = ShopGoodsReceiptStatus.Cancelled;
        CancelledByUserId = cancelledByUserId;
        CancelledDate = cancelledDate;
        CancellationReason = Check.NotNullOrWhiteSpace(cancellationReason, nameof(cancellationReason), ShopGoodsReceiptConsts.CancellationReasonMaxLength).Trim();
    }

    internal void EnsureEditable()
    {
        if (Status != ShopGoodsReceiptStatus.Draft) throw new BusinessException("ShopManagement:GoodsReceiptCannotBeEdited");
    }

    internal void EnsureDeletable()
    {
        if (Status != ShopGoodsReceiptStatus.Draft) throw new BusinessException("ShopManagement:GoodsReceiptCannotBeDeleted");
    }

    internal void EnsureCompletable()
    {
        if (Status == ShopGoodsReceiptStatus.Completed) throw new BusinessException("ShopManagement:GoodsReceiptAlreadyCompleted");
        if (Status != ShopGoodsReceiptStatus.Draft) throw new BusinessException("ShopManagement:GoodsReceiptCannotBeCompleted");
    }

    private void SetHeaderValues(string? supplierInvoiceNumber, DateTime receiptDate, decimal shippingCharges, decimal otherCharges, string? notes)
    {
        if (shippingCharges < 0) throw new BusinessException("ShopManagement:GoodsReceiptInvalidShippingCharges");
        if (otherCharges < 0) throw new BusinessException("ShopManagement:GoodsReceiptInvalidOtherCharges");

        SupplierInvoiceNumber = Check.Length(supplierInvoiceNumber?.Trim(), nameof(supplierInvoiceNumber), ShopGoodsReceiptConsts.SupplierInvoiceNumberMaxLength);
        ReceiptDate = receiptDate;
        ShippingCharges = shippingCharges;
        OtherCharges = otherCharges;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopGoodsReceiptConsts.NotesMaxLength);
    }

    private void ReplaceItems(List<ShopGoodsReceiptItem> items)
    {
        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:GoodsReceiptRequiresItems");

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

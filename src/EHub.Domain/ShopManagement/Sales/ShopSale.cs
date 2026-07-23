using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.Customers;

namespace EHub.ShopManagement.Sales;

public class ShopSale : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string SaleNumber { get; protected set; } = string.Empty;

    public Guid CustomerId { get; protected set; }
    public ShopCustomer? Customer { get; protected set; }

    public DateTime SaleDate { get; protected set; }
    public DateTime? DueDate { get; protected set; }
    public ShopSaleType SaleType { get; protected set; }
    public ShopSalePaymentMethod PaymentMethod { get; protected set; }
    public ShopSaleStatus Status { get; protected set; } = ShopSaleStatus.Draft;

    public decimal SubTotal { get; protected set; }
    public decimal DiscountAmount { get; protected set; }
    public decimal TaxAmount { get; protected set; }
    public decimal OtherCharges { get; protected set; }
    public decimal GrandTotal { get; protected set; }
    public decimal PaidAmount { get; protected set; }
    public decimal PendingAmount { get; protected set; }

    public string? ReferenceNumber { get; protected set; }
    public string? Notes { get; protected set; }

    public Guid? CompletedByUserId { get; protected set; }
    public DateTime? CompletedDate { get; protected set; }
    public Guid? CancelledByUserId { get; protected set; }
    public DateTime? CancelledDate { get; protected set; }
    public string? CancellationReason { get; protected set; }

    public ICollection<ShopSaleItem> Items { get; protected set; } = new List<ShopSaleItem>();

    protected ShopSale() { }

    internal ShopSale(
        Guid id,
        Guid tenantId,
        string saleNumber,
        Guid customerId,
        DateTime saleDate,
        DateTime? dueDate,
        ShopSaleType saleType,
        ShopSalePaymentMethod paymentMethod,
        string? referenceNumber,
        decimal otherCharges,
        string? notes,
        decimal paidAmount,
        List<ShopSaleItem> items) : base(id)
    {
        TenantId = tenantId;
        SaleNumber = saleNumber;
        CustomerId = customerId;
        Status = ShopSaleStatus.Draft;
        SetHeaderValues(saleDate, dueDate, saleType, paymentMethod, referenceNumber, otherCharges, notes);
        ReplaceItems(items);
        SetPaidAmount(paidAmount);
    }

    internal void UpdateHeaderAndItems(
        Guid customerId,
        DateTime saleDate,
        DateTime? dueDate,
        ShopSaleType saleType,
        ShopSalePaymentMethod paymentMethod,
        string? referenceNumber,
        decimal otherCharges,
        string? notes,
        decimal paidAmount,
        List<ShopSaleItem> items)
    {
        CustomerId = customerId;
        SetHeaderValues(saleDate, dueDate, saleType, paymentMethod, referenceNumber, otherCharges, notes);
        ReplaceItems(items);
        SetPaidAmount(paidAmount);
    }

    internal void MarkAsCompleted(Guid completedByUserId, DateTime completedDate)
    {
        if (Status == ShopSaleStatus.Completed) throw new BusinessException("ShopManagement:SaleAlreadyCompleted");
        if (Status != ShopSaleStatus.Draft) throw new BusinessException("ShopManagement:SaleCannotBeCompleted");
        Status = ShopSaleStatus.Completed;
        CompletedByUserId = completedByUserId;
        CompletedDate = completedDate;
    }

    internal void MarkAsCancelled(Guid cancelledByUserId, DateTime cancelledDate, string cancellationReason)
    {
        if (Status != ShopSaleStatus.Draft) throw new BusinessException("ShopManagement:SaleCannotBeCancelled");
        Status = ShopSaleStatus.Cancelled;
        CancelledByUserId = cancelledByUserId;
        CancelledDate = cancelledDate;
        CancellationReason = Check.NotNullOrWhiteSpace(cancellationReason, nameof(cancellationReason), ShopSaleConsts.CancellationReasonMaxLength).Trim();
    }

    internal void EnsureEditable()
    {
        if (Status != ShopSaleStatus.Draft) throw new BusinessException("ShopManagement:SaleCannotBeEdited");
    }

    internal void EnsureDeletable()
    {
        if (Status != ShopSaleStatus.Draft) throw new BusinessException("ShopManagement:SaleCannotBeDeleted");
    }

    internal void EnsureCompletable()
    {
        if (Status == ShopSaleStatus.Completed) throw new BusinessException("ShopManagement:SaleAlreadyCompleted");
        if (Status != ShopSaleStatus.Draft) throw new BusinessException("ShopManagement:SaleCannotBeCompleted");
    }

    private void SetHeaderValues(
        DateTime saleDate,
        DateTime? dueDate,
        ShopSaleType saleType,
        ShopSalePaymentMethod paymentMethod,
        string? referenceNumber,
        decimal otherCharges,
        string? notes)
    {
        if (otherCharges < 0) throw new BusinessException("ShopManagement:SaleInvalidOtherCharges");

        if (saleType == ShopSaleType.Credit)
        {
            if (!dueDate.HasValue) throw new BusinessException("ShopManagement:SaleDueDateRequiredForCreditSale");
            if (dueDate.Value.Date < saleDate.Date) throw new BusinessException("ShopManagement:SaleDueDateCannotBeBeforeSaleDate");
        }
        else if (dueDate.HasValue && dueDate.Value.Date < saleDate.Date)
        {
            throw new BusinessException("ShopManagement:SaleDueDateCannotBeBeforeSaleDate");
        }

        SaleDate = saleDate;
        DueDate = dueDate;
        SaleType = saleType;
        PaymentMethod = paymentMethod;
        ReferenceNumber = Check.Length(referenceNumber?.Trim(), nameof(referenceNumber), ShopSaleConsts.ReferenceNumberMaxLength);
        OtherCharges = otherCharges;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopSaleConsts.NotesMaxLength);
    }

    private void ReplaceItems(List<ShopSaleItem> items)
    {
        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:SaleRequiresItems");

        Items.Clear();
        foreach (var item in items) Items.Add(item);

        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        SubTotal = Round(Items.Sum(x => x.LineSubTotal));
        DiscountAmount = Round(Items.Sum(x => x.DiscountAmount));
        TaxAmount = Round(Items.Sum(x => x.TaxAmount));
        GrandTotal = Round(SubTotal - DiscountAmount + TaxAmount + OtherCharges);
    }

    private void SetPaidAmount(decimal paidAmount)
    {
        if (paidAmount < 0) throw new BusinessException("ShopManagement:SalePaidAmountCannotBeNegative");
        if (paidAmount > GrandTotal) throw new BusinessException("ShopManagement:SalePaidAmountExceedsGrandTotal");
        PaidAmount = paidAmount;
        PendingAmount = Round(GrandTotal - paidAmount);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

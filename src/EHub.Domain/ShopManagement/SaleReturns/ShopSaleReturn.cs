using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.SaleReturns;

public class ShopSaleReturn : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string SaleReturnNumber { get; protected set; } = string.Empty;

    public Guid SaleId { get; protected set; }
    public Guid CustomerId { get; protected set; }

    public DateTime ReturnDate { get; protected set; }
    public ShopSaleReturnStatus Status { get; protected set; } = ShopSaleReturnStatus.Draft;
    public ShopSaleReturnReason Reason { get; protected set; }
    public string? ReasonDetails { get; protected set; }
    public ShopSaleReturnSettlementType SettlementType { get; protected set; }

    public decimal SubTotal { get; protected set; }
    public decimal DiscountAmount { get; protected set; }
    public decimal TaxAmount { get; protected set; }
    public decimal OtherCharges { get; protected set; }
    public decimal GrandTotal { get; protected set; }
    public decimal RefundAmount { get; protected set; }
    public decimal CustomerCreditAmount { get; protected set; }

    public string? Notes { get; protected set; }

    public Guid? CompletedByUserId { get; protected set; }
    public DateTime? CompletedDate { get; protected set; }
    public Guid? CancelledByUserId { get; protected set; }
    public DateTime? CancelledDate { get; protected set; }
    public string? CancellationReason { get; protected set; }

    public ICollection<ShopSaleReturnItem> Items { get; protected set; } = new List<ShopSaleReturnItem>();

    protected ShopSaleReturn() { }

    internal ShopSaleReturn(
        Guid id,
        Guid tenantId,
        string saleReturnNumber,
        Guid saleId,
        Guid customerId,
        DateTime returnDate,
        ShopSaleReturnReason reason,
        string? reasonDetails,
        ShopSaleReturnSettlementType settlementType,
        decimal otherCharges,
        string? notes,
        List<ShopSaleReturnItem> items) : base(id)
    {
        TenantId = tenantId;
        SaleReturnNumber = Check.NotNullOrWhiteSpace(saleReturnNumber, nameof(saleReturnNumber), ShopSaleReturnConsts.SaleReturnNumberMaxLength);
        SaleId = saleId;
        CustomerId = customerId;
        Status = ShopSaleReturnStatus.Draft;
        SetValues(returnDate, reason, reasonDetails, settlementType, otherCharges, notes, items);
    }

    internal void Update(
        DateTime returnDate,
        ShopSaleReturnReason reason,
        string? reasonDetails,
        ShopSaleReturnSettlementType settlementType,
        decimal otherCharges,
        string? notes,
        List<ShopSaleReturnItem> items)
    {
        EnsureEditable();
        SetValues(returnDate, reason, reasonDetails, settlementType, otherCharges, notes, items);
    }

    internal void MarkAsCompleted(Guid completedByUserId, DateTime completedDate)
    {
        EnsureCompletable();
        Status = ShopSaleReturnStatus.Completed;

        switch (SettlementType)
        {
            case ShopSaleReturnSettlementType.CashRefund:
                RefundAmount = GrandTotal;
                CustomerCreditAmount = 0;
                break;
            default:
                // CustomerCredit and Exchange both reduce the customer's receivable / add advance credit.
                CustomerCreditAmount = GrandTotal;
                RefundAmount = 0;
                break;
        }

        CompletedByUserId = completedByUserId;
        CompletedDate = completedDate;
    }

    internal void MarkAsCancelled(Guid cancelledByUserId, DateTime cancelledDate, string cancellationReason)
    {
        EnsureDraft();
        Status = ShopSaleReturnStatus.Cancelled;
        CancelledByUserId = cancelledByUserId;
        CancelledDate = cancelledDate;
        CancellationReason = Check.NotNullOrWhiteSpace(cancellationReason, nameof(cancellationReason), ShopSaleReturnConsts.CancellationReasonMaxLength).Trim();
    }

    internal void EnsureEditable()
    {
        if (Status != ShopSaleReturnStatus.Draft) throw new BusinessException("ShopManagement:SaleReturnCannotBeEdited");
    }

    internal void EnsureDeletable()
    {
        if (Status != ShopSaleReturnStatus.Draft) throw new BusinessException("ShopManagement:SaleReturnCannotBeDeleted");
    }

    internal void EnsureCompletable()
    {
        if (Status == ShopSaleReturnStatus.Completed) throw new BusinessException("ShopManagement:SaleReturnAlreadyCompleted");
        if (Status != ShopSaleReturnStatus.Draft) throw new BusinessException("ShopManagement:SaleReturnCannotBeCompleted");
    }

    private void EnsureDraft()
    {
        if (Status != ShopSaleReturnStatus.Draft) throw new BusinessException("ShopManagement:SaleReturnCannotBeCancelled");
    }

    private void SetValues(
        DateTime returnDate,
        ShopSaleReturnReason reason,
        string? reasonDetails,
        ShopSaleReturnSettlementType settlementType,
        decimal otherCharges,
        string? notes,
        List<ShopSaleReturnItem> items)
    {
        if (otherCharges < 0) throw new BusinessException("ShopManagement:SaleReturnOtherChargesCannotBeNegative");
        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:SaleReturnRequiresItems");

        ReturnDate = returnDate;
        Reason = reason;
        ReasonDetails = Check.Length(reasonDetails?.Trim(), nameof(reasonDetails), ShopSaleReturnConsts.ReasonDetailsMaxLength);
        SettlementType = settlementType;
        OtherCharges = otherCharges;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopSaleReturnConsts.NotesMaxLength);

        Items.Clear();
        foreach (var item in items) Items.Add(item);

        SubTotal = Round(Items.Sum(x => x.LineSubTotal));
        DiscountAmount = Round(Items.Sum(x => x.DiscountAmount));
        TaxAmount = Round(Items.Sum(x => x.TaxAmount));
        GrandTotal = Round(SubTotal - DiscountAmount + TaxAmount + OtherCharges);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

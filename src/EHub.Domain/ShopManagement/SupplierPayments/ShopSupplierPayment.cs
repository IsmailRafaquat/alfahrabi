using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.Suppliers;

namespace EHub.ShopManagement.SupplierPayments;

public class ShopSupplierPayment : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string PaymentNumber { get; protected set; } = string.Empty;

    public Guid SupplierId { get; protected set; }
    public ShopSupplier? Supplier { get; protected set; }

    public DateTime PaymentDate { get; protected set; }
    public ShopSupplierPaymentType PaymentType { get; protected set; }
    public ShopSupplierPaymentMethod PaymentMethod { get; protected set; }
    public decimal Amount { get; protected set; }
    public string? ReferenceNumber { get; protected set; }
    public string? ChequeNumber { get; protected set; }
    public string? BankName { get; protected set; }
    public string? Notes { get; protected set; }
    public ShopSupplierPaymentStatus Status { get; protected set; } = ShopSupplierPaymentStatus.Draft;

    public Guid? PostedByUserId { get; protected set; }
    public DateTime? PostedDate { get; protected set; }
    public Guid? CancelledByUserId { get; protected set; }
    public DateTime? CancelledDate { get; protected set; }
    public string? CancellationReason { get; protected set; }

    public ICollection<ShopSupplierPaymentAllocation> Allocations { get; protected set; } = new List<ShopSupplierPaymentAllocation>();

    protected ShopSupplierPayment() { }

    internal ShopSupplierPayment(
        Guid id,
        Guid tenantId,
        string paymentNumber,
        Guid supplierId,
        DateTime paymentDate,
        ShopSupplierPaymentType paymentType,
        ShopSupplierPaymentMethod paymentMethod,
        decimal amount,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        string? notes,
        List<ShopSupplierPaymentAllocation> allocations) : base(id)
    {
        TenantId = tenantId;
        PaymentNumber = paymentNumber;
        SupplierId = supplierId;
        Status = ShopSupplierPaymentStatus.Draft;
        SetValues(paymentDate, paymentType, paymentMethod, amount, referenceNumber, chequeNumber, bankName, notes);
        ReplaceAllocations(allocations);
    }

    internal void Update(
        DateTime paymentDate,
        ShopSupplierPaymentType paymentType,
        ShopSupplierPaymentMethod paymentMethod,
        decimal amount,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        string? notes,
        List<ShopSupplierPaymentAllocation> allocations)
    {
        SetValues(paymentDate, paymentType, paymentMethod, amount, referenceNumber, chequeNumber, bankName, notes);
        ReplaceAllocations(allocations);
    }

    internal void MarkAsPosted(Guid postedByUserId, DateTime postedDate)
    {
        if (Status != ShopSupplierPaymentStatus.Draft) throw new BusinessException("ShopManagement:SupplierPaymentCannotBePosted");
        Status = ShopSupplierPaymentStatus.Posted;
        PostedByUserId = postedByUserId;
        PostedDate = postedDate;
    }

    internal void MarkAsCancelled(Guid cancelledByUserId, DateTime cancelledDate, string cancellationReason)
    {
        if (Status != ShopSupplierPaymentStatus.Posted) throw new BusinessException("ShopManagement:SupplierPaymentCannotBeCancelled");
        Status = ShopSupplierPaymentStatus.Cancelled;
        CancelledByUserId = cancelledByUserId;
        CancelledDate = cancelledDate;
        CancellationReason = Check.NotNullOrWhiteSpace(cancellationReason, nameof(cancellationReason), ShopSupplierPaymentConsts.CancellationReasonMaxLength).Trim();
    }

    internal void EnsureEditable()
    {
        if (Status != ShopSupplierPaymentStatus.Draft) throw new BusinessException("ShopManagement:SupplierPaymentCannotBeEdited");
    }

    internal void EnsureDeletable()
    {
        if (Status != ShopSupplierPaymentStatus.Draft) throw new BusinessException("ShopManagement:SupplierPaymentCannotBeDeleted");
    }

    private void SetValues(
        DateTime paymentDate,
        ShopSupplierPaymentType paymentType,
        ShopSupplierPaymentMethod paymentMethod,
        decimal amount,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        string? notes)
    {
        if (amount <= 0) throw new BusinessException("ShopManagement:SupplierPaymentInvalidAmount");

        var trimmedChequeNumber = Check.Length(chequeNumber?.Trim(), nameof(chequeNumber), ShopSupplierPaymentConsts.ChequeNumberMaxLength);
        var trimmedBankName = Check.Length(bankName?.Trim(), nameof(bankName), ShopSupplierPaymentConsts.BankNameMaxLength);

        if (paymentMethod == ShopSupplierPaymentMethod.Cheque && string.IsNullOrWhiteSpace(trimmedChequeNumber))
            throw new BusinessException("ShopManagement:SupplierPaymentChequeNumberRequired");
        if ((paymentMethod == ShopSupplierPaymentMethod.Cheque || paymentMethod == ShopSupplierPaymentMethod.BankTransfer) && string.IsNullOrWhiteSpace(trimmedBankName))
            throw new BusinessException("ShopManagement:SupplierPaymentBankNameRequired");

        PaymentDate = paymentDate;
        PaymentType = paymentType;
        PaymentMethod = paymentMethod;
        Amount = amount;
        ReferenceNumber = Check.Length(referenceNumber?.Trim(), nameof(referenceNumber), ShopSupplierPaymentConsts.ReferenceNumberMaxLength);
        ChequeNumber = trimmedChequeNumber;
        BankName = trimmedBankName;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopSupplierPaymentConsts.NotesMaxLength);
    }

    private void ReplaceAllocations(List<ShopSupplierPaymentAllocation> allocations)
    {
        allocations ??= new List<ShopSupplierPaymentAllocation>();

        if (PaymentType == ShopSupplierPaymentType.InvoicePayment && allocations.Count == 0)
            throw new BusinessException("ShopManagement:SupplierPaymentRequiresAllocation");

        var totalAllocated = allocations.Sum(x => x.AllocatedAmount);
        if (totalAllocated > Amount) throw new BusinessException("ShopManagement:SupplierPaymentAllocationExceedsAmount");

        Allocations.Clear();
        foreach (var allocation in allocations) Allocations.Add(allocation);
    }
}

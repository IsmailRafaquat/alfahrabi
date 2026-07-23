using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.Customers;

namespace EHub.ShopManagement.CustomerPayments;

public class ShopCustomerPayment : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string PaymentNumber { get; protected set; } = string.Empty;

    public Guid CustomerId { get; protected set; }
    public ShopCustomer? Customer { get; protected set; }

    public DateTime PaymentDate { get; protected set; }
    public ShopCustomerPaymentType PaymentType { get; protected set; }
    public ShopCustomerPaymentMethod PaymentMethod { get; protected set; }
    public decimal Amount { get; protected set; }
    public string? ReferenceNumber { get; protected set; }
    public string? ChequeNumber { get; protected set; }
    public string? BankName { get; protected set; }
    public string? Notes { get; protected set; }
    public ShopCustomerPaymentStatus Status { get; protected set; } = ShopCustomerPaymentStatus.Draft;

    public Guid? PostedByUserId { get; protected set; }
    public DateTime? PostedDate { get; protected set; }
    public Guid? CancelledByUserId { get; protected set; }
    public DateTime? CancelledDate { get; protected set; }
    public string? CancellationReason { get; protected set; }

    public ICollection<ShopCustomerPaymentAllocation> Allocations { get; protected set; } = new List<ShopCustomerPaymentAllocation>();

    protected ShopCustomerPayment() { }

    internal ShopCustomerPayment(
        Guid id,
        Guid tenantId,
        string paymentNumber,
        Guid customerId,
        DateTime paymentDate,
        ShopCustomerPaymentType paymentType,
        ShopCustomerPaymentMethod paymentMethod,
        decimal amount,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        string? notes,
        List<ShopCustomerPaymentAllocation> allocations) : base(id)
    {
        TenantId = tenantId;
        PaymentNumber = paymentNumber;
        CustomerId = customerId;
        Status = ShopCustomerPaymentStatus.Draft;
        SetValues(paymentDate, paymentType, paymentMethod, amount, referenceNumber, chequeNumber, bankName, notes);
        ReplaceAllocations(allocations);
    }

    internal void Update(
        DateTime paymentDate,
        ShopCustomerPaymentType paymentType,
        ShopCustomerPaymentMethod paymentMethod,
        decimal amount,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        string? notes,
        List<ShopCustomerPaymentAllocation> allocations)
    {
        SetValues(paymentDate, paymentType, paymentMethod, amount, referenceNumber, chequeNumber, bankName, notes);
        ReplaceAllocations(allocations);
    }

    internal void MarkAsPosted(Guid postedByUserId, DateTime postedDate)
    {
        if (Status != ShopCustomerPaymentStatus.Draft) throw new BusinessException("ShopManagement:CustomerPaymentCannotBePosted");
        Status = ShopCustomerPaymentStatus.Posted;
        PostedByUserId = postedByUserId;
        PostedDate = postedDate;
    }

    internal void MarkAsCancelled(Guid cancelledByUserId, DateTime cancelledDate, string cancellationReason)
    {
        if (Status != ShopCustomerPaymentStatus.Posted) throw new BusinessException("ShopManagement:CustomerPaymentCannotBeCancelled");
        Status = ShopCustomerPaymentStatus.Cancelled;
        CancelledByUserId = cancelledByUserId;
        CancelledDate = cancelledDate;
        CancellationReason = Check.NotNullOrWhiteSpace(cancellationReason, nameof(cancellationReason), ShopCustomerPaymentConsts.CancellationReasonMaxLength).Trim();
    }

    internal void EnsureEditable()
    {
        if (Status != ShopCustomerPaymentStatus.Draft) throw new BusinessException("ShopManagement:CustomerPaymentCannotBeEdited");
    }

    internal void EnsureDeletable()
    {
        if (Status != ShopCustomerPaymentStatus.Draft) throw new BusinessException("ShopManagement:CustomerPaymentCannotBeDeleted");
    }

    private void SetValues(
        DateTime paymentDate,
        ShopCustomerPaymentType paymentType,
        ShopCustomerPaymentMethod paymentMethod,
        decimal amount,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        string? notes)
    {
        if (amount <= 0) throw new BusinessException("ShopManagement:CustomerPaymentInvalidAmount");

        var trimmedChequeNumber = Check.Length(chequeNumber?.Trim(), nameof(chequeNumber), ShopCustomerPaymentConsts.ChequeNumberMaxLength);
        var trimmedBankName = Check.Length(bankName?.Trim(), nameof(bankName), ShopCustomerPaymentConsts.BankNameMaxLength);

        if (paymentMethod == ShopCustomerPaymentMethod.Cheque && string.IsNullOrWhiteSpace(trimmedChequeNumber))
            throw new BusinessException("ShopManagement:CustomerPaymentChequeNumberRequired");
        if ((paymentMethod == ShopCustomerPaymentMethod.Cheque || paymentMethod == ShopCustomerPaymentMethod.BankTransfer) && string.IsNullOrWhiteSpace(trimmedBankName))
            throw new BusinessException("ShopManagement:CustomerPaymentBankNameRequired");

        PaymentDate = paymentDate;
        PaymentType = paymentType;
        PaymentMethod = paymentMethod;
        Amount = amount;
        ReferenceNumber = Check.Length(referenceNumber?.Trim(), nameof(referenceNumber), ShopCustomerPaymentConsts.ReferenceNumberMaxLength);
        ChequeNumber = trimmedChequeNumber;
        BankName = trimmedBankName;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopCustomerPaymentConsts.NotesMaxLength);
    }

    private void ReplaceAllocations(List<ShopCustomerPaymentAllocation> allocations)
    {
        allocations ??= new List<ShopCustomerPaymentAllocation>();

        if (PaymentType == ShopCustomerPaymentType.InvoicePayment && allocations.Count == 0)
            throw new BusinessException("ShopManagement:CustomerPaymentRequiresAllocation");

        var totalAllocated = allocations.Sum(x => x.AllocatedAmount);
        if (totalAllocated > Amount) throw new BusinessException("ShopManagement:CustomerPaymentAllocationExceedsAmount");

        Allocations.Clear();
        foreach (var allocation in allocations) Allocations.Add(allocation);
    }
}

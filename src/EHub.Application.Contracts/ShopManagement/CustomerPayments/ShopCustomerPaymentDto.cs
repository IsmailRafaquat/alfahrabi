using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.CustomerPayments;

public class ShopCustomerPaymentDto : EntityDto<Guid>
{
    public string PaymentNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public DateTime PaymentDate { get; set; }
    public ShopCustomerPaymentType PaymentType { get; set; }
    public ShopCustomerPaymentMethod PaymentMethod { get; set; }
    public decimal? Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ChequeNumber { get; set; }
    public string? BankName { get; set; }
    public Guid? BankAccountId { get; set; }
    public string? BankAccountCode { get; set; }
    public string? BankAccountName { get; set; }
    public string? Notes { get; set; }
    public ShopCustomerPaymentStatus Status { get; set; }

    public decimal? AllocatedAmount { get; set; }
    public decimal? UnallocatedAmount { get; set; }

    public Guid? PostedByUserId { get; set; }
    public DateTime? PostedDate { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreationTime { get; set; }

    public List<ShopCustomerPaymentAllocationDto> Allocations { get; set; } = new();
}

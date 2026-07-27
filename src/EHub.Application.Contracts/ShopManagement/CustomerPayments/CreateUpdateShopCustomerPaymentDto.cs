using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.CustomerPayments;

public class CreateUpdateShopCustomerPaymentDto
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [Required]
    public ShopCustomerPaymentType PaymentType { get; set; }

    [Required]
    public ShopCustomerPaymentMethod PaymentMethod { get; set; }

    // Must be > 0; enforced by ShopCustomerPayment as a localized BusinessException.
    public decimal Amount { get; set; }

    [StringLength(ShopCustomerPaymentConsts.ReferenceNumberMaxLength)]
    public string? ReferenceNumber { get; set; }

    [StringLength(ShopCustomerPaymentConsts.ChequeNumberMaxLength)]
    public string? ChequeNumber { get; set; }

    [StringLength(ShopCustomerPaymentConsts.BankNameMaxLength)]
    public string? BankName { get; set; }

    public Guid? BankAccountId { get; set; }

    [StringLength(ShopCustomerPaymentConsts.NotesMaxLength)]
    public string? Notes { get; set; }

    public List<CreateUpdateShopCustomerPaymentAllocationDto> Allocations { get; set; } = new();
}

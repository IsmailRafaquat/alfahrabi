using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.CustomerPayments;

public class CreateUpdateShopCustomerPaymentAllocationDto
{
    [Required]
    public Guid SaleId { get; set; }

    // Business validation (must be > 0 and not exceed the sale's pending amount) is
    // enforced by ShopCustomerPaymentManager as a localized BusinessException.
    public decimal AllocatedAmount { get; set; }
}

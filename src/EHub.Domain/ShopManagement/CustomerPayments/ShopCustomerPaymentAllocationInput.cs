using System;

namespace EHub.ShopManagement.CustomerPayments;

/// <summary>
/// Domain-layer input for a customer payment allocation, decoupled from the application layer's
/// DTOs so that <see cref="ShopCustomerPaymentManager"/> does not depend on EHub.Application.Contracts.
/// </summary>
public class ShopCustomerPaymentAllocationInput
{
    public Guid SaleId { get; set; }
    public decimal AllocatedAmount { get; set; }
}

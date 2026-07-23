using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.CustomerPayments;

public class GetShopCustomerPaymentsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? CustomerId { get; set; }
    public ShopCustomerPaymentType? PaymentType { get; set; }
    public ShopCustomerPaymentMethod? PaymentMethod { get; set; }
    public ShopCustomerPaymentStatus? Status { get; set; }
    public DateTime? PaymentDateFrom { get; set; }
    public DateTime? PaymentDateTo { get; set; }
}

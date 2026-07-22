using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.SupplierPayments;

public class GetShopSupplierPaymentsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? SupplierId { get; set; }
    public ShopSupplierPaymentType? PaymentType { get; set; }
    public ShopSupplierPaymentMethod? PaymentMethod { get; set; }
    public ShopSupplierPaymentStatus? Status { get; set; }
    public DateTime? PaymentDateFrom { get; set; }
    public DateTime? PaymentDateTo { get; set; }
}

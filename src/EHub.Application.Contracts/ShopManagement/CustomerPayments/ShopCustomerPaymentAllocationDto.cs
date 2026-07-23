using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.CustomerPayments;

public class ShopCustomerPaymentAllocationDto : EntityDto<Guid>
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal? GrandTotal { get; set; }
    public decimal? AllocatedAmount { get; set; }
    public DateTime CreationTime { get; set; }
}

using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Customers;

public class ShopCustomerLookupDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ShopCustomerType CustomerType { get; set; }
    public string? Phone { get; set; }
    public decimal? CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; }
    public bool IsWalkInCustomer { get; set; }
}

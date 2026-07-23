using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Customers;

public class GetShopCustomersInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public ShopCustomerType? CustomerType { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsWalkInCustomer { get; set; }
    public bool? HasOpeningBalance { get; set; }
    public bool? HasCreditLimit { get; set; }
}

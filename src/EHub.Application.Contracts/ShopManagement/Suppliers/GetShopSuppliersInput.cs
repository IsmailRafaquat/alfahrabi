using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Suppliers;

public class GetShopSuppliersInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool? IsActive { get; set; }
    public bool? HasOpeningBalance { get; set; }
    public bool? HasCreditLimit { get; set; }
}

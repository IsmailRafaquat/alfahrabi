using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.CashRegisters;

public class GetShopCashRegistersInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}

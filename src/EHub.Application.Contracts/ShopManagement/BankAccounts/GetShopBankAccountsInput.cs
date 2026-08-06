using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.BankAccounts;

public class GetShopBankAccountsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDefault { get; set; }
    public string? BankName { get; set; }
}

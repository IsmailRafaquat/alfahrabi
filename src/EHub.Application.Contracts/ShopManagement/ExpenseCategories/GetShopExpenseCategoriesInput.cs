using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.ExpenseCategories;

public class GetShopExpenseCategoriesInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}

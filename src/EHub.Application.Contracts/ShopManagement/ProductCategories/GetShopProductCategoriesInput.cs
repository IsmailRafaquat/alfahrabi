using System;
using Volo.Abp.Application.Dtos;
namespace EHub.ShopManagement.ProductCategories;
public class GetShopProductCategoriesInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool RootCategoriesOnly { get; set; }
    public bool? IsActive { get; set; }
}

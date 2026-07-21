using System;
using Volo.Abp.Application.Dtos;
namespace EHub.ShopManagement.ProductCategories;
public class ShopProductCategoryLookupDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

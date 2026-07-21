using System;
using Volo.Abp.Application.Dtos;
namespace EHub.ShopManagement.ProductCategories;
public class ShopProductCategoryDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }
}

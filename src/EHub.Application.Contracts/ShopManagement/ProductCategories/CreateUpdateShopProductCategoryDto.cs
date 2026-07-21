using System;
using System.ComponentModel.DataAnnotations;
namespace EHub.ShopManagement.ProductCategories;
public class CreateUpdateShopProductCategoryDto
{
    [Required, StringLength(ShopProductCategoryConsts.NameMaxLength)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(ShopProductCategoryConsts.CodeMaxLength)] public string Code { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    [StringLength(ShopProductCategoryConsts.DescriptionMaxLength)] public string? Description { get; set; }
    [Range(0, int.MaxValue)] public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

using System;
using System.Collections.Generic;
namespace EHub.ShopManagement.ProductCategories;
public class ShopProductCategoryTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<ShopProductCategoryTreeDto> Children { get; set; } = new();
}

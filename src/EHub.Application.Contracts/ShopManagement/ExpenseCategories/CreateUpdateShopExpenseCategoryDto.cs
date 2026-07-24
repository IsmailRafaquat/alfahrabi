using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.ExpenseCategories;

public class CreateUpdateShopExpenseCategoryDto
{
    [Required, StringLength(ShopExpenseCategoryConsts.CodeMaxLength)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(ShopExpenseCategoryConsts.NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(ShopExpenseCategoryConsts.DescriptionMaxLength)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

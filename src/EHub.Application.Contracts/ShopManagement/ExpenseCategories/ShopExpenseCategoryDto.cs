using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.ExpenseCategories;

public class ShopExpenseCategoryDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }
}

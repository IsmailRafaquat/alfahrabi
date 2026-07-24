using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.ExpenseCategories;

public class ShopExpenseCategoryLookupDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

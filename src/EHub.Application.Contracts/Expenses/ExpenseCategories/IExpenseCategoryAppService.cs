using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.Expenses.ExpenseCategories;

public interface IExpenseCategoryAppService : IApplicationService
{
    Task<ExpenseCategoryDto> GetAsync(Guid id);

    Task<PagedResultDto<ExpenseCategoryDto>> GetListAsync(GetExpenseCategoryListInput input);

    Task<ExpenseCategoryDto> CreateAsync(CreateUpdateExpenseCategoryDto input);

    Task<ExpenseCategoryDto> UpdateAsync(Guid id, CreateUpdateExpenseCategoryDto input);

    Task DeleteAsync(Guid id);

    Task SetActiveAsync(Guid id, bool isActive);

    Task<List<ExpenseCategoryLookupDto>> GetExpenseCategoryLookupAsync();
}

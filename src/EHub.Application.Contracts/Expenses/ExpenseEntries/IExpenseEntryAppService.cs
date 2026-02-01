using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.Expenses.ExpenseEntries;

public interface IExpenseEntryAppService : IApplicationService
{
    Task<ExpenseEntryDto> GetAsync(Guid id);

    Task<PagedResultDto<ExpenseEntryDto>> GetListAsync(GetExpenseEntryListInput input);

    Task<ExpenseEntryDto> CreateAsync(CreateUpdateExpenseEntryDto input);

    Task<ExpenseEntryDto> UpdateAsync(Guid id, CreateUpdateExpenseEntryDto input);

    Task DeleteAsync(Guid id);
}

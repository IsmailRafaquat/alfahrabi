using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.Expenses.ExpenseEntries;

[RemoteService(IsEnabled = true)]
[ControllerName("ExpenseEntries")]
[Area("app")]
[Route("api/app/expense-entry")]
public class ExpenseController(IExpenseEntryAppService appService) : AbpController, IExpenseEntryAppService
{
    [HttpGet("{id}")]
    public Task<ExpenseEntryDto> GetAsync(Guid id) => appService.GetAsync(id);

    [HttpGet]
    public Task<PagedResultDto<ExpenseEntryDto>> GetListAsync(GetExpenseEntryListInput input)
        => appService.GetListAsync(input);

    [HttpPost]
    public Task<ExpenseEntryDto> CreateAsync(CreateUpdateExpenseEntryDto input)
        => appService.CreateAsync(input);

    [HttpPut("{id}")]
    public Task<ExpenseEntryDto> UpdateAsync(Guid id, CreateUpdateExpenseEntryDto input)
        => appService.UpdateAsync(id, input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id) => appService.DeleteAsync(id);
}

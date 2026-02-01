using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.Expenses.ExpenseCategories;

[RemoteService(IsEnabled = true)]
[ControllerName("ExpenseCategories")]
[Area("app")]
[Route("api/app/expense-categories")]
public class ExpenseCategoryController(IExpenseCategoryAppService appService)
    : AbpController, IExpenseCategoryAppService
{
    [HttpPost]
    public Task<ExpenseCategoryDto> CreateAsync(CreateUpdateExpenseCategoryDto input)
        => appService.CreateAsync(input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
        => appService.DeleteAsync(id);

    [HttpGet("{id}")]
    public Task<ExpenseCategoryDto> GetAsync(Guid id)
        => appService.GetAsync(id);

    [HttpGet]
    public Task<PagedResultDto<ExpenseCategoryDto>> GetListAsync(GetExpenseCategoryListInput input)
        => appService.GetListAsync(input);

    [HttpPut("{id}")]
    public Task<ExpenseCategoryDto> UpdateAsync(Guid id, CreateUpdateExpenseCategoryDto input)
        => appService.UpdateAsync(id, input);

    [HttpPut("{id}/active")]
    public Task SetActiveAsync(Guid id, [FromBody] bool isActive)
        => appService.SetActiveAsync(id, isActive);

    [HttpGet("get-expense-category-lookup")]
    public Task<List<ExpenseCategoryLookupDto>> GetExpenseCategoryLookupAsync()
        => appService.GetExpenseCategoryLookupAsync();
}

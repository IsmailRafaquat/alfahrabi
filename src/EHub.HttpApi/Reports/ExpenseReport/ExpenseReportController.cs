using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.Reports.ExpenseReport;

[RemoteService(IsEnabled = true)]
[ControllerName("ExpenseReport")]
[Area("app")]
[Route("api/app/expense-report")]
public class ExpenseReportController : AbpController
{
    private readonly IExpenseReportAppService _expenseReportAppService;

    public ExpenseReportController(IExpenseReportAppService expenseReportAppService)
    {
        _expenseReportAppService = expenseReportAppService;
    }

    [HttpGet]
    public async Task<List<ExpenseReportDto>> GetListAsync(ExpenseReportFilterDto input)
    {
        return await _expenseReportAppService.GetExpenseReportAsync(input);
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.Expenses.ExpenseDashboards;

[RemoteService(IsEnabled = true)]
[ControllerName("ExpensesDashboard")]
[Area("app")]
[Route("api/app/expenses-dashboard")]
public class ExpensesDashboardController : AbpController, IExpensesDashboardAppService
{
    private readonly IExpensesDashboardAppService _service;

    public ExpensesDashboardController(IExpensesDashboardAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public Task<ExpensesDashboardDto> GetAsync(GetExpensesDashboardInput input)
    {
        return _service.GetAsync(input);
    }
}

using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.Expenses.ExpenseDashboards;

public interface IExpensesDashboardAppService : IApplicationService
{
    Task<ExpensesDashboardDto> GetAsync(GetExpensesDashboardInput input);
}

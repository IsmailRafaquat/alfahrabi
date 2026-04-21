using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.Reports.ExpenseReport;

public interface IExpenseReportAppService : IApplicationService
{
    Task<List<ExpenseReportDto>> GetExpenseReportAsync(ExpenseReportFilterDto input);
}

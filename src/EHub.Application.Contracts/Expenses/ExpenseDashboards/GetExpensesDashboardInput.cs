using System;
using Volo.Abp.Application.Dtos;

namespace EHub.Expenses.ExpenseDashboards;

public class GetExpensesDashboardInput : PagedAndSortedResultRequestDto
{
    public string? Month { get; set; }       // use 1st day of month
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public Guid? ExpenseCategoryId { get; set; }
    public string? Filter { get; set; }
}

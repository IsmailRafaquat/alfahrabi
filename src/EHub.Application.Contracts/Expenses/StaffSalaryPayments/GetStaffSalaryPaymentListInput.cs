using System;
using Volo.Abp.Application.Dtos;

namespace EHub.Expenses.StaffSalaryPayments;

public class GetStaffSalaryPaymentListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; } // staff name search (optional)
    public Guid? StaffId { get; set; }

    public DateTime? SalaryMonth { get; set; } // filter by month (any day ok)
    public DateTime? FromDate { get; set; } // payment date range
    public DateTime? ToDate { get; set; }
}

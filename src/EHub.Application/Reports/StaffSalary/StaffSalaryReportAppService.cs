using EHub.Permissions;
using EHub.Reports.SalaryReport;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;

namespace EHub.Reports.StaffSalary;

[RemoteService(false)]
[Authorize(EHubPermissions.Reports.Default)]
public class StaffSalaryReportAppService : EHubAppService, IStaffSalaryReportAppService
{
    private readonly IStaffSalaryReportRepository _staffSalaryReportRepository;

    public StaffSalaryReportAppService(IStaffSalaryReportRepository staffSalaryReportRepository)
    {
        _staffSalaryReportRepository = staffSalaryReportRepository;
    }

    public async Task<StaffSalaryReportDto> GetStaffSalaryReportAsync(StaffSalaryReportFilterDto input)
    {
        var today = DateTime.Today;

        var periodStart = input.PeriodStart ?? new DateTime(today.Year, 1, 1);
        var periodEnd = input.PeriodEnd ?? new DateTime(today.Year, 12, 31);

        var data = await _staffSalaryReportRepository.GetStaffSalaryReportAsync(
            periodStart,
            periodEnd,
            input.Filter,
            input.StaffIds
        );

        return new StaffSalaryReportDto
        {
            Rows = data.Rows.Select(x => new StaffSalaryReportRowDto
            {
                StaffId = x.StaffId,
                StaffName = x.StaffName,
                SalaryDate = x.SalaryDate,
                SalaryAmount = x.SalaryAmount
            }).ToList(),
            GrandTotal = data.GrandTotal
        };
    }
}

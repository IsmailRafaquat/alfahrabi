using EHub.EntityFrameworkCore;
using EHub.Expenses.StaffSalaryPayments;
using EHub.Staffs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace EHub.Reports.SalaryReport;

public class EfCoreStaffSalaryReportRepository : IStaffSalaryReportRepository
{
    private readonly IDbContextProvider<EHubDbContext> _dbContextProvider;

    public EfCoreStaffSalaryReportRepository(IDbContextProvider<EHubDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<StaffSalaryReport> GetStaffSalaryReportAsync(
         DateTime periodStart,
         DateTime periodEnd,
         string? filter,
         List<Guid>? staffIds = null)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();

        var startMonth = new DateTime(periodStart.Year, periodStart.Month, 1);
        var endMonth = new DateTime(periodEnd.Year, periodEnd.Month, 1);

        var query =
            from salary in dbContext.Set<StaffSalaryPayment>()
            join staff in dbContext.Set<Staff>()
                on salary.StaffId equals staff.Id
            where salary.SalaryMonth >= startMonth
                && salary.SalaryMonth <= endMonth
                && (staffIds == null || staffIds.Count == 0 || staffIds.Contains(staff.Id))
            select new
            {
                StaffId = staff.Id,
                StaffName = (staff.FirstName + " " + staff.LastName).Trim(),
                SalaryMonth = salary.SalaryMonth,
                SalaryAmount = salary.SalaryAmount,
                salary.Remarks
            };

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(x =>
                x.StaffName.Contains(filter) ||
                (x.Remarks != null && x.Remarks.Contains(filter))
            );
        }

        var rows = await query
            .OrderBy(x => x.StaffName)
            .ThenBy(x => x.SalaryMonth)
            .Select(x => new StaffSalaryReportRow
            {
                StaffId = x.StaffId,
                StaffName = x.StaffName,
                SalaryDate = x.SalaryMonth, // keep same DTO/model property for now
                SalaryAmount = x.SalaryAmount
            })
            .ToListAsync();

        return new StaffSalaryReport
        {
            Rows = rows,
            GrandTotal = rows.Sum(x => x.SalaryAmount)
        };
    }
}

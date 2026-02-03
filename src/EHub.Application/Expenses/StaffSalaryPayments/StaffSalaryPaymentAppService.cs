using EHub.Staffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.Expenses.StaffSalaryPayments;

[RemoteService(IsEnabled = false)]
public class StaffSalaryPaymentAppService : ApplicationService, IStaffSalaryPaymentAppService
{
    private readonly IRepository<StaffSalaryPayment, Guid> _repo;
    private readonly IRepository<Staff, Guid> _staffRepo;

    public StaffSalaryPaymentAppService(
        IRepository<StaffSalaryPayment, Guid> repo,
        IRepository<Staff, Guid> staffRepo)
    {
        _repo = repo;
        _staffRepo = staffRepo;
    }

    public async Task<StaffSalaryPaymentDto> GetAsync(Guid id)
    {
        var q = await _repo.GetQueryableAsync();
        var s = await _staffRepo.GetQueryableAsync();

        var dto = (from p in q
                   join st in s on p.StaffId equals st.Id
                   where p.Id == id
                   select new StaffSalaryPaymentDto
                   {
                       Id = p.Id,
                       TenantId = p.TenantId,
                       CreationTime = p.CreationTime,
                       CreatorId = p.CreatorId,
                       LastModificationTime = p.LastModificationTime,
                       LastModifierId = p.LastModifierId,

                       StaffId = p.StaffId,
                       StaffName = (st.FirstName + " " + st.LastName).Trim(),

                       SalaryMonth = p.SalaryMonth,
                       SalaryAmount = p.SalaryAmount,
                       PaymentDate = p.PaymentDate,
                       Remarks = p.Remarks
                   }).First();

        return dto;
    }

    public async Task<PagedResultDto<StaffSalaryPaymentDto>> GetListAsync(GetStaffSalaryPaymentListInput input)
    {
        var q = await _repo.GetQueryableAsync();
        var s = await _staffRepo.GetQueryableAsync();

        // normalize month filter (if provided)
        DateTime? month = null;
        if (input.SalaryMonth.HasValue)
            month = new DateTime(input.SalaryMonth.Value.Year, input.SalaryMonth.Value.Month, 1);

        q = q
            .WhereIf(input.StaffId.HasValue, x => x.StaffId == input.StaffId!.Value)
            .WhereIf(month.HasValue, x => x.SalaryMonth == month!.Value)
            .WhereIf(input.FromDate.HasValue, x => x.PaymentDate >= input.FromDate!.Value.Date)
            .WhereIf(input.ToDate.HasValue, x => x.PaymentDate <= input.ToDate!.Value.Date);

        // staff-name filter
        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var filter = input.Filter!.Trim();
            q = from p in q
                join st in s on p.StaffId equals st.Id
                where (st.FirstName + " " + st.LastName).Contains(filter)
                select p;
        }

        var totalCount = await AsyncExecuter.CountAsync(q);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "PaymentDate desc" : input.Sorting;

        var items = (from p in q
                     join st in s on p.StaffId equals st.Id
                     select new StaffSalaryPaymentDto
                     {
                         Id = p.Id,
                         TenantId = p.TenantId,
                         CreationTime = p.CreationTime,
                         CreatorId = p.CreatorId,
                         LastModificationTime = p.LastModificationTime,
                         LastModifierId = p.LastModifierId,

                         StaffId = p.StaffId,
                         StaffName = (st.FirstName + " " + st.LastName).Trim(),

                         SalaryMonth = p.SalaryMonth,
                         SalaryAmount = p.SalaryAmount,
                         PaymentDate = p.PaymentDate,
                         Remarks = p.Remarks
                     })
                          .OrderBy(sorting)
                          .Skip(input.SkipCount)
                          .Take(input.MaxResultCount)
                          .ToList();

        return new PagedResultDto<StaffSalaryPaymentDto>(totalCount, items);
    }

    public async Task<StaffSalaryPaymentDto> CreateAsync(CreateUpdateStaffSalaryPaymentDto input)
    {
        await _staffRepo.GetAsync(input.StaffId);

        await EnsureNotExistsAsync(input.StaffId, input.SalaryMonth);

        var entity = new StaffSalaryPayment(
            GuidGenerator.Create(),
            input.StaffId,
            input.SalaryMonth,
            input.SalaryAmount,
            input.PaymentDate,
            input.Remarks
        );

        await _repo.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<StaffSalaryPayment, StaffSalaryPaymentDto>(entity);
    }

    public async Task<StaffSalaryPaymentDto> UpdateAsync(Guid id, CreateUpdateStaffSalaryPaymentDto input)
    {
        await _staffRepo.GetAsync(input.StaffId);
        await EnsureNotExistsAsync(input.StaffId, input.SalaryMonth, excludeId: id);

        var entity = await _repo.GetAsync(id);

        entity.Update(
            input.StaffId,
            input.SalaryMonth,
            input.SalaryAmount,
            input.PaymentDate,
            input.Remarks
        );

        await _repo.UpdateAsync(entity, autoSave: true);

        return await GetAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }

    private static DateTime NormalizeMonth(DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, 1);
    }

    private async Task EnsureNotExistsAsync(Guid staffId, DateTime salaryMonth, Guid? excludeId = null)
    {
        var month = NormalizeMonth(salaryMonth);

        var queryable = await _repo.GetQueryableAsync();

        // per-tenant uniqueness
        queryable = queryable.Where(x => x.TenantId == CurrentTenant.Id);

        queryable = queryable.Where(x => x.StaffId == staffId && x.SalaryMonth == month);

        if (excludeId.HasValue)
        {
            queryable = queryable.Where(x => x.Id != excludeId.Value);
        }

        var exists = await AsyncExecuter.AnyAsync(queryable);
        if (exists)
        {
            // You can localize this later if you want
            throw new UserFriendlyException("Salary payment already exists for this staff for the selected month.");
        }
    }

}

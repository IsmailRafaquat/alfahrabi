using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.Expenses.StaffSalaryPayments;

public interface IStaffSalaryPaymentAppService : IApplicationService
{
    Task<StaffSalaryPaymentDto> GetAsync(Guid id);

    Task<PagedResultDto<StaffSalaryPaymentDto>> GetListAsync(GetStaffSalaryPaymentListInput input);

    Task<StaffSalaryPaymentDto> CreateAsync(CreateUpdateStaffSalaryPaymentDto input);

    Task<StaffSalaryPaymentDto> UpdateAsync(Guid id, CreateUpdateStaffSalaryPaymentDto input);

    Task DeleteAsync(Guid id);
}

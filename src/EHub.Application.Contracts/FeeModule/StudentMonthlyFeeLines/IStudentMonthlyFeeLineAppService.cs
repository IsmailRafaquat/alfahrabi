using EHub.FeeModule.StudentMonthlyFees;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public interface IStudentMonthlyFeeLineAppService : IApplicationService
{
    Task<StudentMonthlyFeeLineDto> GetAsync(Guid id);

    Task<PagedResultDto<StudentMonthlyFeeLineDto>> GetListAsync(GetStudentMonthlyFeeLineListInput input);

    Task<StudentMonthlyFeeLineDto> CreateAsync(CreateUpdateStudentMonthlyFeeLineDto input);

    Task UpdateAsync(Guid id, CreateUpdateStudentMonthlyFeeLineDto input);

    Task DeleteAsync(Guid id);
    Task<BulkGenerateStudentMonthlyFeeResultDto> BulkGenerateAsync(BulkGenerateStudentMonthlyFeeDto input);

    Task<CalculatedAmountsDto> CalculateAmountsAsync(CalculateFeeLineAmountsInput input);
    Task<CheckFeesDashboardDto> GetDashboardAsync(CheckFeesDashboardInput input);
}

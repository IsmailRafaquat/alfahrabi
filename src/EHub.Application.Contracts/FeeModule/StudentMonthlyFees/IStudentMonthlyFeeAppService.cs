using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.FeeModule.StudentMonthlyFees;

public interface IStudentMonthlyFeeAppService : IApplicationService
{
    Task<StudentMonthlyFeeDto> GetAsync(Guid id);
    Task<PagedResultDto<StudentMonthlyFeeDto>> GetListAsync(GetStudentMonthlyFeeListInput input);

    Task<StudentMonthlyFeeDto> CreateAsync(CreateUpdateStudentMonthlyFeeDto input);
    Task UpdateAsync(Guid id, CreateUpdateStudentMonthlyFeeDto input);
    Task DeleteAsync(Guid id);

    Task<BulkGenerateStudentMonthlyFeeResultDto> BulkGenerateAsync(BulkGenerateStudentMonthlyFeeDto input);
}

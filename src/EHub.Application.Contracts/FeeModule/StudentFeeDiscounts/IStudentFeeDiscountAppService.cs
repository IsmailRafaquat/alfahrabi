using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.FeeModule.StudentFeeDiscounts;

public interface IStudentFeeDiscountAppService : IApplicationService
{
    Task<StudentFeeDiscountDto> GetAsync(Guid id);

    Task<PagedResultDto<StudentFeeDiscountDto>> GetListAsync(GetStudentFeeDiscountListInput input);

    Task<StudentFeeDiscountDto> CreateAsync(CreateUpdateStudentFeeDiscountDto input);

    Task UpdateAsync(Guid id, CreateUpdateStudentFeeDiscountDto input);

    Task DeleteAsync(Guid id);

    Task<BulkAssignStudentFeeDiscountResultDto> BulkAssignAsync(BulkAssignStudentFeeDiscountDto input);
}

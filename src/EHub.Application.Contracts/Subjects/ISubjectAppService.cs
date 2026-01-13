using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.Subjects;

public interface ISubjectAppService : IApplicationService
{
    Task<SubjectDto> GetAsync(Guid id);

    Task<PagedResultDto<SubjectDto>> GetListAsync(GetSubjectListDto input);

    Task<SubjectDto> CreateAsync(CreateSubjectDto input);

    Task UpdateAsync(Guid id, UpdateSubjectDto input);

    Task DeleteAsync(Guid id);
}

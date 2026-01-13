using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.Subjects;

[RemoteService(IsEnabled = false)]
public class SubjectAppService : ApplicationService, ISubjectAppService
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly SubjectManager _subjectManager;

    public SubjectAppService(
        ISubjectRepository subjectRepository,
        SubjectManager subjectManager)
    {
        _subjectRepository = subjectRepository;
        _subjectManager = subjectManager;
    }

    public async Task<SubjectDto> GetAsync(Guid id)
    {
        var entity = await _subjectRepository.GetAsync(id);
        return ObjectMapper.Map<Subject, SubjectDto>(entity);
    }

    public async Task<PagedResultDto<SubjectDto>> GetListAsync(GetSubjectListDto input)
    {
        if (input.Sorting.IsNullOrWhiteSpace())
        {
            input.Sorting = nameof(Subject.Name);
        }

        var totalCount = await _subjectRepository.GetCountAsync(
            input.Filter,
            input.Code,
            input.Name,
            input.GradeLevel,
            input.IsActive
        );

        var subjects = await _subjectRepository.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting!,
            input.Filter,
            input.Code,
            input.Name,
            input.GradeLevel,
            input.IsActive
        );

        return new PagedResultDto<SubjectDto>(
            totalCount,
            ObjectMapper.Map<List<Subject>, List<SubjectDto>>(subjects));
    }

    public async Task<SubjectDto> CreateAsync(CreateSubjectDto input)
    {
        var subject = await _subjectManager.CreateAsync(
            CurrentTenant.Id,
            input.Code,           
            input.Name,
            input.ShortName,
            input.Description,
            input.GradeLevel,
            input.CreditHours
        );

        return ObjectMapper.Map<Subject, SubjectDto>(subject);
    }

    public async Task UpdateAsync(Guid id, UpdateSubjectDto input)
    {
        var subject = await _subjectRepository.GetAsync(id);

        subject = await _subjectManager.UpdateBasicsAsync(
            subject,
            input.Code,
            input.Name,
            input.ShortName,
            input.Description,
            input.GradeLevel,
            input.CreditHours
        );

        if (input.IsActive && !subject.IsActive)
        {
            await _subjectManager.ActivateAsync(subject);
        }
        else if (!input.IsActive && subject.IsActive)
        {
            await _subjectManager.DeactivateAsync(subject);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        await _subjectRepository.DeleteAsync(id);
    }
}

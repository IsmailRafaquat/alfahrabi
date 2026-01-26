using EHub.Students;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.FeeModule.StudentFeeProfiles;

[RemoteService(IsEnabled = false)]
public class StudentFeeProfileAppService : ApplicationService, IStudentFeeProfileAppService
{
    private readonly IStudentFeeProfileRepository _repo;
    private readonly IStudentRepository _studentRepository;
    private readonly StudentFeeProfileManager _manager;

    public StudentFeeProfileAppService(
        IStudentFeeProfileRepository repo,
         IStudentRepository studentRepository,
    StudentFeeProfileManager manager)
    {
        _repo = repo;
        _studentRepository = studentRepository;
        _manager = manager;
    }

    public async Task<StudentFeeProfileDto> GetAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return ObjectMapper.Map<StudentFeeProfile, StudentFeeProfileDto>(entity!);
    }

    public async Task<PagedResultDto<StudentFeeProfileDto>> GetListAsync(GetStudentFeeProfileListInput input)
    {
        if (input.Sorting.IsNullOrWhiteSpace())
        {
            input.Sorting = nameof(StudentFeeProfile.CreationTime) + " DESC";
        }

        var totalCount = await _repo.GetCountAsync(
            input.Filter,
            input.StudentId,
            input.FeeStructureId,
            input.IsActive,
            input.EffectiveFrom,
            input.EffectiveTo
        );

        var list = await _repo.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            input.Filter,
            input.StudentId,
            input.FeeStructureId,
            input.IsActive,
            input.EffectiveFrom,
            input.EffectiveTo
        );

        var items = list.Select(x => ObjectMapper.Map<StudentFeeProfile, StudentFeeProfileDto>(x)).ToList();
        return new PagedResultDto<StudentFeeProfileDto>(totalCount, items);
    }

    public async Task<StudentFeeProfileDto> CreateAsync(CreateUpdateStudentFeeProfileDto input)
    {
        var tenantId = CurrentTenant.Id;

        var entity = await _manager.CreateAsync(
            input.StudentId,
            input.FeeStructureId,
            input.EffectiveFrom,
            input.EffectiveTo,
            input.IsActive
        );

        entity = await _repo.InsertAsync(entity);

        return ObjectMapper.Map<StudentFeeProfile, StudentFeeProfileDto>(entity);
    }

    public async Task UpdateAsync(Guid id, CreateUpdateStudentFeeProfileDto input)
    {
        var entity = await _repo.GetAsync(id);

        await _manager.UpdateAsync(
            entity,
            input.StudentId,
            input.FeeStructureId,
            input.EffectiveFrom,
            input.EffectiveTo,
            input.IsActive
        );

        await _repo.UpdateAsync(entity, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }

    public async Task<BulkAssignStudentFeeProfileResultDto> BulkAssignAsync(BulkAssignStudentFeeProfileDto input)
    {
        if (input.EffectiveTo.HasValue && input.EffectiveTo.Value.Date < input.EffectiveFrom.Date)
            throw new UserFriendlyException("Invalid date range. EffectiveTo must be >= EffectiveFrom.");

        var students = await _studentRepository.GetListAsync(
            skipCount: 0,
            maxResultCount: int.MaxValue, // careful if you have huge dataset
            sorting: nameof(Student.FirstName),
            filter: null,
            admissionNo: null,
            firstName: null,
            lastName: null,
            gradeLevel: input.GradeLevel,
            section: input.Section,
            shift: input.Shift,
            term: input.Term,
            dob: null,
            gender: null,
            status: null
        );


        var result = new BulkAssignStudentFeeProfileResultDto
        {
            TotalStudents = students.Count
        };

        foreach (var s in students)
        {
            // duplicate check: Student + FeeStructure + EffectiveFrom
            var exists = await _repo.AnyAsync(x =>
                x.StudentId == s.Id &&
                x.FeeStructureId == input.FeeStructureId &&
                x.EffectiveFrom == input.EffectiveFrom.Date);

            if (exists)
            {
                if (!input.SkipExisting)
                    throw new UserFriendlyException($"Duplicate exists for student: {s.FirstName} {s.LastName}");

                result.SkippedExisting++;
                result.SkippedStudentNames.Add($"{s.FirstName} {s.LastName}");
                continue;
            }

            var entity = await _manager.CreateAsync(
                s.Id,
                input.FeeStructureId,
                input.EffectiveFrom,
                input.EffectiveTo,
                input.IsActive
            );

            await _repo.InsertAsync(entity, autoSave: true);
            result.Created++;
        }

        return result;
    }

}

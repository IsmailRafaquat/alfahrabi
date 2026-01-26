using EHub.Students;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.FeeModule.StudentMonthlyFees;

[RemoteService(IsEnabled = false)]
public class StudentMonthlyFeeAppService : ApplicationService, IStudentMonthlyFeeAppService
{
    private readonly IStudentMonthlyFeeRepository _repo;
    private readonly StudentMonthlyFeeManager _manager;
    private readonly IStudentRepository _studentRepo;

    public StudentMonthlyFeeAppService(
        IStudentMonthlyFeeRepository repo,
        StudentMonthlyFeeManager manager,
        IStudentRepository studentRepo)
    {
        _repo = repo;
        _manager = manager;
        _studentRepo = studentRepo;
    }

    public async Task<StudentMonthlyFeeDto> GetAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return ObjectMapper.Map<StudentMonthlyFee, StudentMonthlyFeeDto>(entity!);
    }

    public async Task<PagedResultDto<StudentMonthlyFeeDto>> GetListAsync(GetStudentMonthlyFeeListInput input)
    {
        if (input.Sorting.IsNullOrWhiteSpace())
            input.Sorting = nameof(StudentMonthlyFee.CreationTime) + " DESC";

        var total = await _repo.GetCountAsync(input.Filter, input.StudentId, input.Month);

        var list = await _repo.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            input.Filter,
            input.StudentId,
            input.Month);

        var items = list.Select(x => ObjectMapper.Map<StudentMonthlyFee, StudentMonthlyFeeDto>(x)).ToList();
        return new PagedResultDto<StudentMonthlyFeeDto>(total, items);
    }

    public async Task<StudentMonthlyFeeDto> CreateAsync(CreateUpdateStudentMonthlyFeeDto input)
    {
        var entity = await _manager.CreateAsync(
            input.StudentId,
            input.Month,
            input.DueDate,
            input.Remarks,
            skipIfExists: false);

        entity = await _repo.InsertAsync(entity, autoSave: true);
        return ObjectMapper.Map<StudentMonthlyFee, StudentMonthlyFeeDto>(entity);
    }

    public async Task UpdateAsync(Guid id, CreateUpdateStudentMonthlyFeeDto input)
    {
        var entity = await _repo.GetAsync(id);

        await _manager.UpdateAsync(entity, input.StudentId, input.Month, input.DueDate, input.Remarks);

        await _repo.UpdateAsync(entity, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }

    public async Task<BulkGenerateStudentMonthlyFeeResultDto> BulkGenerateAsync(BulkGenerateStudentMonthlyFeeDto input)
    {
        // Normalize month
        var month = new DateTime(input.Month.Year, input.Month.Month, 1);

        // Fetch students by filters (no paging, all matching)
        var studentsQ = await _studentRepo.GetQueryableAsync();

        studentsQ = studentsQ
            .WhereIf(input.GradeLevel.HasValue, x => x.GradeLevel == input.GradeLevel)
            .WhereIf(input.Section.HasValue, x => x.Section == input.Section)
            .WhereIf(input.Shift.HasValue, x => x.Shift == input.Shift)
            .WhereIf(input.Term.HasValue, x => x.Term == input.Term);

        var students =  studentsQ
            .Select(x => new { x.Id })
            .ToList();

        var result = new BulkGenerateStudentMonthlyFeeResultDto
        {
            TotalStudents = students.Count
        };

        if (!students.Any())
            return result;

        foreach (var s in students)
        {
            var entity = await _manager.CreateAsync(
                s.Id,
                month,
                input.DueDate,
                input.Remarks,
                input.SkipIfExists);

            if (entity == null)
            {
                result.Skipped++;
                continue;
            }

            await _repo.InsertAsync(entity); // batch insert, autosave later
            result.Created++;
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return result;
    }
}

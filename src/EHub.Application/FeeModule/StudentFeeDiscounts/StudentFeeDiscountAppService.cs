using EHub.Students;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.FeeModule.StudentFeeDiscounts;

[RemoteService(IsEnabled = false)]
public class StudentFeeDiscountAppService : ApplicationService, IStudentFeeDiscountAppService
{
    private readonly IStudentFeeDiscountRepository _repo;
    private readonly IStudentRepository _studentRepository;
    private readonly StudentFeeDiscountManager _manager;

    public StudentFeeDiscountAppService(
        IStudentFeeDiscountRepository repo,
        IStudentRepository studentRepository,
        StudentFeeDiscountManager manager)
    {
        _repo = repo;
        _studentRepository = studentRepository;
        _manager = manager;
    }

    public async Task<StudentFeeDiscountDto> GetAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return ObjectMapper.Map<StudentFeeDiscount, StudentFeeDiscountDto>(entity!);
    }

    public async Task<PagedResultDto<StudentFeeDiscountDto>> GetListAsync(GetStudentFeeDiscountListInput input)
    {
        if (input.Sorting.IsNullOrWhiteSpace())
        {
            input.Sorting = nameof(StudentFeeDiscount.CreationTime) + " DESC";
        }

        var totalCount = await _repo.GetCountAsync(
            input.Filter,
            input.StudentId,
            input.FeeHeadId,
            input.DiscountType,
            input.IsActive,
            input.IsApproved
        );

        var list = await _repo.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            input.Filter,
            input.StudentId,
            input.FeeHeadId,
            input.DiscountType,
            input.IsActive,
            input.IsApproved
        );

        var items = list.Select(x => ObjectMapper.Map<StudentFeeDiscount, StudentFeeDiscountDto>(x)).ToList();
        return new PagedResultDto<StudentFeeDiscountDto>(totalCount, items);
    }

    public async Task<StudentFeeDiscountDto> CreateAsync(CreateUpdateStudentFeeDiscountDto input)
    {
        var entity = await _manager.CreateAsync(
            input.StudentId,
            input.FeeHeadId,
            input.DiscountType,
            input.Value,
            input.Reason,
            input.StartMonth,
            input.EndMonth,
            input.ApprovedByStaffId,
            input.IsActive
        );

        entity = await _repo.InsertAsync(entity);

        return ObjectMapper.Map<StudentFeeDiscount, StudentFeeDiscountDto>(entity);
    }

    public async Task UpdateAsync(Guid id, CreateUpdateStudentFeeDiscountDto input)
    {
        var entity = await _repo.GetAsync(id);

        await _manager.UpdateAsync(
            entity,
            input.StudentId,
            input.FeeHeadId,
            input.DiscountType,
            input.Value,
            input.Reason,
            input.StartMonth,
            input.EndMonth,
            input.ApprovedByStaffId,
            input.IsActive
        );

        await _repo.UpdateAsync(entity, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }

    public async Task<BulkAssignStudentFeeDiscountResultDto> BulkAssignAsync(BulkAssignStudentFeeDiscountDto input)
    {
        if (input.StartMonth.HasValue && input.EndMonth.HasValue && input.EndMonth.Value < input.StartMonth.Value)
            throw new UserFriendlyException("Invalid month range. EndMonth must be >= StartMonth.");

        var students = await _studentRepository.GetListAsync(
            skipCount: 0,
            maxResultCount: int.MaxValue,
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

        var result = new BulkAssignStudentFeeDiscountResultDto
        {
            TotalStudents = students.Count
        };

        foreach (var s in students)
        {
            // Check for overlapping discount
            var exists = await _repo.HasOverlappingDiscountAsync(
                s.Id,
                input.FeeHeadId,
                input.StartMonth,
                input.EndMonth,
                exceptId: null);

            if (exists)
            {
                if (!input.SkipExisting)
                    throw new UserFriendlyException($"Overlapping discount exists for student: {s.FirstName} {s.LastName}");

                result.SkippedExisting++;
                result.SkippedStudentNames.Add($"{s.FirstName} {s.LastName}");
                continue;
            }

            var entity = await _manager.CreateAsync(
                s.Id,
                input.FeeHeadId,
                input.DiscountType,
                input.Value,
                input.Reason,
                input.StartMonth,
                input.EndMonth,
                input.ApprovedByStaffId,
                input.IsActive
            );

            await _repo.InsertAsync(entity, autoSave: true);
            result.Created++;
        }

        return result;
    }


}

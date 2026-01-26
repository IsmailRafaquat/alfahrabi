using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace EHub.FeeModule.StudentMonthlyFees;

public class StudentMonthlyFeeManager : DomainService
{
    private readonly IStudentMonthlyFeeRepository _repo;

    public StudentMonthlyFeeManager(IStudentMonthlyFeeRepository repo)
    {
        _repo = repo;
    }

    private static DateTime NormalizeMonth(DateTime dt) => new(dt.Year, dt.Month, 1);

    public async Task<StudentMonthlyFee> CreateAsync(
        Guid studentId,
        DateTime month,
        DateTime? dueDate,
        string? remarks,
        bool skipIfExists)
    {
        if (studentId == Guid.Empty)
            throw new UserFriendlyException("StudentId is required.");

        var m = NormalizeMonth(month);

        var exists = await _repo.ExistsAsync(studentId, m);
        if (exists)
        {
            if (skipIfExists)
                return null!; // handled by caller bulk logic (see below)

            throw new UserFriendlyException("Monthly fee already exists for this student and month.");
        }

        return new StudentMonthlyFee(GuidGenerator.Create(), studentId, m, dueDate, remarks);
    }

    public async Task UpdateAsync(
        StudentMonthlyFee entity,
        Guid studentId,
        DateTime month,
        DateTime? dueDate,
        string? remarks)
    {
        var m = NormalizeMonth(month);

        var dup = await _repo.ExistsAsync(studentId, m, exceptId: entity.Id);
        if (dup)
            throw new UserFriendlyException("Monthly fee already exists for this student and month.");

        // StudentId is private set, keep it stable (or change via reflection if you want)
        if (entity.StudentId != studentId)
            throw new UserFriendlyException("StudentId cannot be changed.");

        entity.ChangeMonth(m);
        entity.ChangeDueDate(dueDate);
        entity.ChangeRemarks(remarks);
    }
}

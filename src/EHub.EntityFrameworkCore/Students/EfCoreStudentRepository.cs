using EHub.EntityFrameworkCore;
using EHub.FeeModule.StudentMonthlyFees;
using EHub.StudentAttendances;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EHub.Students;

public class EfCoreStudentRepository : EfCoreRepository<EHubDbContext, Student, Guid>, IStudentRepository
{
    public EfCoreStudentRepository(IDbContextProvider<EHubDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<Student?> FindByAdmissionNoAsync(string admissionNo)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.AdmissionNo == admissionNo);
    }

    public async Task<Student?> FindByEmailAsync(string email)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<long> GetCountAsync(string? filter, string? admissionNo, string? firstName, string? lastName,
         GradeLevel? gradeLevel,
         Section? section,
         Shift? shift,
         Term? term,
        DateTime? dob, Gender? gender, Status? status)
    {
        var data = await GetFiltersAsync(filter, admissionNo, firstName, lastName, gradeLevel, section, shift, term, dob, gender, status);
        return await data.LongCountAsync();
    }

    public async Task<List<Student>> GetListAsync(int skipCount, int maxResultCount, string sorting, string? filter, string? admissionNo, string? firstName, string? lastName,
         GradeLevel? gradeLevel,
         Section? section,
         Shift? shift,
         Term? term,
        DateTime? dob, Gender? gender, Status? status)
    {
        var data = await GetFiltersAsync(filter, admissionNo, firstName, lastName, gradeLevel, section, shift, term, dob, gender, status);
        return await data.OrderBy(sorting).PageBy(skipCount, maxResultCount).ToListAsync();
    }

    public async Task<IQueryable<Student>> GetFiltersAsync(string? filter, string? admissionNo, string? firstName, string? lastName,
         GradeLevel? gradeLevel,
         Section? section,
         Shift? shift,
         Term? term,
        DateTime? DOB, Gender? gender, Status? status)
    {
        var queryable = await GetQueryableAsync();

        var query = queryable.AsQueryable()
            .WhereIf(!string.IsNullOrWhiteSpace(filter), x => x.FirstName.ToLower().Contains(filter!.ToLower())
                                                    || x.LastName.ToLower().Contains(filter!.ToLower())
                                                    || x.AdmissionNo.ToLower().Contains(filter!.ToLower())
                                                    || x.RollNo!.ToLower().Contains(filter!.ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(firstName), x => x.FirstName.ToLower().Contains(firstName!.ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(lastName), x => x.LastName.ToLower().Contains(lastName!.ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(admissionNo), x => x.AdmissionNo.ToLower().Contains(admissionNo!.ToLower()))
            .WhereIf(status.HasValue, x => x.Status == status)
            .WhereIf(gender.HasValue, x => x.Gender == gender)
            .WhereIf(gradeLevel.HasValue, x => x.GradeLevel == gradeLevel)
            .WhereIf(section.HasValue, x => x.Section == section)
            .WhereIf(term.HasValue, x => x.Term == term)
            .WhereIf(shift.HasValue, x => x.Shift == shift);
        return query;
    }

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Include(x => x.StudentDocuments).ThenInclude(x => x.FileAttachment)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Student>> GetStudentLookupAsync()
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .OrderBy(x => x.FirstName)
            .ToListAsync();
    }

    public async Task<List<Student>> GetByClassSectionAsync(GradeLevel gradeLevel, Section section)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.GradeLevel == gradeLevel && x.Section == section && x.Status == Status.Active)
            .OrderBy(x => x.AdmissionNo)
            .ToListAsync();
    }


    public async Task<List<string>> GetDeleteBlockersAsync(Guid studentId)
    {
        var blockers = new List<string>();

        if (await (await GetDbContextAsync()).Set<StudentMonthlyFee>()
            .AnyAsync(x => x.StudentId == studentId))
            blockers.Add("Monthly Fees");

        if (await (await GetDbContextAsync()).Set<StudentAttendance>()
            .AnyAsync(x => x.StudentId == studentId))
            blockers.Add("Attendance");

        return blockers;
    }
}

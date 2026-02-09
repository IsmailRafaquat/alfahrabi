using EHub.Students;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.Staffs;

[RemoteService(IsEnabled = false)]
public class StaffAppService : ApplicationService, IStaffAppService
{
    private readonly IStaffRepository _staffRepository;
    private readonly StaffManager _staffManager;

    public StaffAppService(
        IStaffRepository staffRepository,
        StaffManager staffManager)
    {
        _staffRepository = staffRepository;
        _staffManager = staffManager;
    }

    public async Task<StaffDto> GetAsync(Guid id)
    {
        var entity = await _staffRepository.GetByIdAsync(id);
        return ObjectMapper.Map<Staff, StaffDto>(entity!);
    }

    public async Task<PagedResultDto<StaffDto>> GetListAsync(GetStaffListDto input)
    {
        if (input.Sorting.IsNullOrWhiteSpace())
        {
            input.Sorting = nameof(Staff.FirstName);
        }

        var totalCount = await _staffRepository.GetCountAsync(
            input.Filter,
            input.FirstName,
            input.LastName,
            input.Department,
            input.JobStatus,
            input.Shift
        );

        var list = await _staffRepository.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            input.Filter,
            input.FirstName,
            input.LastName,
            input.Department,
            input.JobStatus,
            input.Shift
        );

        var items = list.Select(s => ObjectMapper.Map<Staff, StaffDto>(s)).ToList();
        return new PagedResultDto<StaffDto>(totalCount, items);
    }

    public async Task<StaffDto> CreateAsync(CreateStaffDto input)
    {
        var tenantId = CurrentTenant.Id;

        var staff = await _staffManager.CreateAsync(
            tenantId,
            input.FirstName,
            input.LastName,
            input.PhoneNo,
            input.Email,
            input.DOB,
            input.Nationality,
            input.RelationshipStatus,
            input.Gender,
            input.LanguageKnown,
            input.DisabilityStatus,
            input.StreetAddress,
            input.StreetAddressLine2,
            input.City,
            input.Province,
            input.ZipCode,
            input.JoiningDate,
            input.Designation,
            input.Department,
            input.EmploymentType,
            input.JobStatus,
            input.Salary,
            input.ReportingManager,
            input.WorkShift,
            input.ContractStartDate,
            input.ContractEndDate,
            input.Remarks
        );

        staff = await _staffRepository.InsertAsync(staff);
        return ObjectMapper.Map<Staff, StaffDto>(staff);
    }

    public async Task UpdateAsync(Guid id, UpdateStaffDto input)
    {
        var staff = await _staffRepository.GetAsync(id);

        await _staffManager.UpdatePersonalInfoAsync(
            staff,
            input.FirstName,
            input.LastName,
            input.PhoneNo,
            input.Email,
            input.DOB,
            input.Nationality,
            input.RelationshipStatus,
            input.Gender,
            input.LanguageKnown,
            input.DisabilityStatus
        );

        await _staffManager.UpdateAddressAsync(
            staff,
            input.StreetAddress,
            input.StreetAddressLine2,
            input.City,
            input.Province,
            input.ZipCode
        );

        await _staffManager.UpdateJobInfoAsync(
            staff,
            input.Designation,
            input.Department,
            input.EmploymentType,
            input.JobStatus,
            input.Salary,
            input.ReportingManager,
            input.WorkShift,
            input.ContractStartDate,
            input.ContractEndDate,
            input.Remarks
        );

        await _staffRepository.UpdateAsync(staff, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        var blockers = await _staffRepository.GetDeleteBlockersAsync(id);

        if (blockers.Count > 0)
        {
            throw new UserFriendlyException(
                $"This staff member cannot be deleted because it is used in: {string.Join(", ", blockers)}. " +
                "Please remove those related records first, then try again."
            );
        }

        await _staffRepository.DeleteAsync(id);
    }

    public async Task<List<StaffLookupDto>> GetStaffLookupAsync()
    {
        var students = await _staffRepository.GetStaffLookupAsync();

        return students.Select(x => new StaffLookupDto
        {
            Id = x.Id,
            EmployeeCode = x.EmployeeCode,
            FirstName = x.FirstName,
            LastName = x.LastName
        }).ToList();
    }
}

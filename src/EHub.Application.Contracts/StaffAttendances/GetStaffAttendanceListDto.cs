using EHub.AttendanceStatuss;
using System;
using Volo.Abp.Application.Dtos;

namespace EHub.StaffAttendances;

public class GetStaffAttendanceListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? StaffId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public AttendanceStatus? Status { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmployeeCode { get; set; }
}

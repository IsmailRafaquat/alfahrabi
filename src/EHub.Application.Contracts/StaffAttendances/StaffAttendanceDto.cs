using EHub.AttendanceStatuss;
using System;
using Volo.Abp.Application.Dtos;

namespace EHub.StaffAttendances;

public class StaffAttendanceDto : EntityDto<Guid>
{
    public Guid StaffId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }

    public string? StaffName { get; set; }
    public string? EmployeeCode { get; set; }
}

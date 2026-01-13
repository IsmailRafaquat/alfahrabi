using EHub.AttendanceStatuss;
using System;
using Volo.Abp.Application.Dtos;

namespace EHub.StudentAttendances;

public class StudentAttendanceDto : FullAuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
    public string? StudentName { get; set; }
    public string? AdmissionNo { get; set; }
}

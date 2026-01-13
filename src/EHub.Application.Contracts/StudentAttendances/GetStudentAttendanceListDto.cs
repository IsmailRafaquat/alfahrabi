using EHub.AttendanceStatuss;
using System;
using Volo.Abp.Application.Dtos;

namespace EHub.StudentAttendances;

public class GetStudentAttendanceListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? StudentId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public AttendanceStatus? Status { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AdmissionNo { get; set; }
}

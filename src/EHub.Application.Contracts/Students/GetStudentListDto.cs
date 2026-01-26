using System;
using Volo.Abp.Application.Dtos;

namespace EHub.Students;

public class GetStudentListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? AdmissionNo { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public GradeLevel? GradeLevel { get; set; }
    public Section? Section { get; set; }
    public Term? Term { get; set; }
    public Shift? Shift { get; set; }
    public DateTime? DOB { get; set; }
    public Gender? Gender { get; set; }
    public Status? Status { get; set; }
}

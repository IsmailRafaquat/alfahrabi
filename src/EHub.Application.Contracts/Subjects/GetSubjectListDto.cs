using EHub.Students;
using Volo.Abp.Application.Dtos;

namespace EHub.Subjects;

public class GetSubjectListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public GradeLevel? GradeLevel { get; set; }

    public bool? IsActive { get; set; }
}

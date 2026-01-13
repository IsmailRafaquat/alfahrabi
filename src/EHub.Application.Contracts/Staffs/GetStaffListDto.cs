using EHub.Students;
using Volo.Abp.Application.Dtos;

namespace EHub.Staffs;

public class GetStaffListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Department? Department { get; set; }
    public JobStatus? JobStatus { get; set; }
    public Shift? Shift { get; set; }
}

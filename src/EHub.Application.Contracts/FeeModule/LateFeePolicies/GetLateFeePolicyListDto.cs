using EHub.Students;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.LateFeePolicies;

public class GetLateFeePolicyListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public GradeLevel? GradeLevel { get; set; }
    public Section? Section { get; set; }
    public Shift? Shift { get; set; }
    public Term? Term { get; set; }
    public LateFeeType? Type { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsGlobal { get; set; }
}

using EHub.Students;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.FeeStructures;

public class GetFeeStructureListInput : PagedAndSortedResultRequestDto
{
    public GradeLevel? GradeLevel { get; set; }
    public Shift? Shift { get; set; }
    public Term? Term { get; set; }
    public bool? IsActive { get; set; }
}

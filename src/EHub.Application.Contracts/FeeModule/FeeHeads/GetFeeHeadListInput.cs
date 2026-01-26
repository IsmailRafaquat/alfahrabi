using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.FeeHeads;

public class GetFeeHeadListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}

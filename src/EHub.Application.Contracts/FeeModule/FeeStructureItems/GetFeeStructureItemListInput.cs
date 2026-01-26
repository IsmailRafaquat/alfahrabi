using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.FeeStructureItems;

public class GetFeeStructureItemListInput : PagedAndSortedResultRequestDto
{
    public Guid? FeeStructureId { get; set; }
    public Guid? FeeHeadId { get; set; }
    public bool? IsMandatory { get; set; }
}

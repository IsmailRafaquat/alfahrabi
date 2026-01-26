using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.FeeStructureItems;

public class FeeStructureItemDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public Guid FeeStructureId { get; set; }
    public Guid FeeHeadId { get; set; }

    public decimal MonthlyAmount { get; set; }
    public bool IsMandatory { get; set; }

    // Optional for UI display (if you load FeeHead name)
    public string? FeeHeadName { get; set; }
    public string? FeeStructureName { get; set; }
}

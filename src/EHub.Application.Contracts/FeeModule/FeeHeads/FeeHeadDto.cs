using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.FeeHeads;

public class FeeHeadDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; }
    public FeeHeadChargeType? ChargeType { get; set; }
}

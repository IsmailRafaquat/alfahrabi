using EHub.Students;
using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.FeeStructures;

public class FeeStructureDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public GradeLevel GradeLevel { get; set; }
    public Shift Shift { get; set; }
    public Term Term { get; set; }

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; }
}

using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.StudentFeeProfiles;

public class StudentFeeProfileDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public Guid StudentId { get; set; }
    public Guid FeeStructureId { get; set; }

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; }

    //public string? StudentName { get; set; }
    //public string? FeeStructureLabel { get; set; }
}

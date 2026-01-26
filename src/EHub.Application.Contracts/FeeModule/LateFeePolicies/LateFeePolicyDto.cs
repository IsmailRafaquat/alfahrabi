using EHub.Students;
using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.LateFeePolicies;

public class LateFeePolicyDto : EntityDto<Guid>
{
    public GradeLevel? GradeLevel { get; set; }
    public Section? Section { get; set; }
    public Shift? Shift { get; set; }
    public Term? Term { get; set; }

    public int GraceDays { get; set; }
    public LateFeeType Type { get; set; }
    public decimal Value { get; set; }

    public bool IsActive { get; set; }
    public bool IsGlobal { get; set; }
    public DateTime CreationTime { get; set; }
}

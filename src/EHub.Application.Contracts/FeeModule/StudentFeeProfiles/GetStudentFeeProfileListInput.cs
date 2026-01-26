using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.StudentFeeProfiles;

public class GetStudentFeeProfileListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? FeeStructureId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

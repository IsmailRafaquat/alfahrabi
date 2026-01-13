using EHub.Students;
using System;
using Volo.Abp.Application.Dtos;

namespace EHub.Subjects;

public class SubjectDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public GradeLevel? GradeLevel { get; set; }
    public decimal? CreditHours { get; set; }
    public bool IsActive { get; set; }
}

using EHub.Students;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.Subjects;

public class Subject : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string Code { get; set; }
    public string Name { get; set; }
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public GradeLevel? GradeLevel { get; set; }
    public decimal? CreditHours { get; set; }
    public bool IsActive { get; set; } = true;

    private Subject() { }

    internal Subject(
        Guid id,
        Guid? tenantId,
        string? code,
        string name,
        string? shortName,
        string? description,
        GradeLevel? gradeLevel,
        decimal? creditHours
    ) : base(id)
    {
        TenantId = tenantId;
        SetCode(code!);
        SetName(name);
        SetShortName(shortName);
        SetDescription(description);
        GradeLevel = gradeLevel;
        CreditHours = creditHours;
    }

    internal Subject ChangeBasics(
        string? code,
        string name,
        string? shortName,
        string? description,
        GradeLevel? gradeLevel,
        decimal? creditHours)
    {
        SetCode(code!);
        SetName(name);
        SetShortName(shortName);
        SetDescription(description);
        GradeLevel = gradeLevel;
        CreditHours = creditHours;
        return this;
    }

    internal Subject Activate() { IsActive = true; return this; }
    internal Subject Deactivate() { IsActive = false; return this; }

    private void SetCode(string code)
        => Code = Check.NotNullOrWhiteSpace(code, nameof(Code), maxLength: SubjectConsts.CodeMaxLength);

    private void SetName(string name)
        => Name = Check.NotNullOrWhiteSpace(name, nameof(Name), maxLength: SubjectConsts.NameMaxLength);

    private void SetShortName(string? value)
    {
        if (!value.IsNullOrWhiteSpace())
            Check.Length(value, nameof(ShortName), SubjectConsts.ShortNameMaxLength);
        ShortName = value;
    }

    private void SetDescription(string? value)
    {
        if (!value.IsNullOrWhiteSpace())
            Check.Length(value, nameof(Description), SubjectConsts.DescriptionMaxLength);
        Description = value;
    }
}

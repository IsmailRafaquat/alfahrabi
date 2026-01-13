using EHub.Students;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace EHub.Subjects;

public class SubjectManager : DomainService
{
    private readonly ISubjectRepository _subjectRepository;

    public SubjectManager(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<Subject> CreateAsync(
    Guid? tenantId,
    string? code,
    string name,
    string? shortName,
    string? description,
    GradeLevel? gradeLevel,
    decimal? creditHours)
    {
        if (creditHours is < 0)
            throw new InvalidCreditHoursException(creditHours);

        var same = await _subjectRepository.FindByNameAndGradeAsync(tenantId, name.Trim(), gradeLevel);
        if (same != null)
            throw new DuplicateSubjectException(name, gradeLevel);

        if (code.IsNullOrWhiteSpace())
        {
            code = BuildNameGradeCode(name, gradeLevel);

            var codeDupe = await _subjectRepository.FindByCodeAsync(tenantId, code);
            if (codeDupe != null)
                throw new DuplicateSubjectException(name, gradeLevel);
        }
        else
        {
            code = code.Trim();
            var codeDupe = await _subjectRepository.FindByCodeAsync(tenantId, code);
            if (codeDupe != null)
                throw new DuplicateSubjectCodeException(code);
        }

        var subject = new Subject(
            id: GuidGenerator.Create(),
            tenantId: tenantId,
            code: code!,
            name: name,
            shortName: shortName,
            description: description,
            gradeLevel: gradeLevel,
            creditHours: creditHours
        );

        return await _subjectRepository.InsertAsync(subject);
    }


    public async Task<Subject> UpdateBasicsAsync(
    Subject subject,
    string code,
    string name,
    string? shortName,
    string? description,
    GradeLevel? gradeLevel,
    decimal? creditHours)
    {
        if (creditHours is < 0)
            throw new InvalidCreditHoursException(creditHours);

        if (!code.IsNullOrWhiteSpace() && !string.Equals(subject.Code, code, StringComparison.Ordinal))
        {
            var dupeCode = await _subjectRepository.FindByCodeAsync(subject.TenantId, code.Trim());
            if (dupeCode != null && dupeCode.Id != subject.Id)
                throw new DuplicateSubjectCodeException(code);
        }

        var nameChanged = !string.Equals(subject.Name, name, StringComparison.Ordinal);
        var gradeChanged = subject.GradeLevel != gradeLevel;

        if (nameChanged || gradeChanged)
        {
            var dupe = await _subjectRepository.FindByNameAndGradeAsync(subject.TenantId, name.Trim(), gradeLevel);
            if (dupe != null && dupe.Id != subject.Id)
                throw new DuplicateSubjectException(name, gradeLevel);
        }

        subject.ChangeBasics(code, name, shortName, description, gradeLevel, creditHours);
        return await _subjectRepository.UpdateAsync(subject, autoSave: true);
    }


    public async Task<Subject> ActivateAsync(Subject subject)
    {
        subject.Activate();
        return await _subjectRepository.UpdateAsync(subject, autoSave: true);
    }

    public async Task<Subject> DeactivateAsync(Subject subject)
    {
        subject.Deactivate();
        return await _subjectRepository.UpdateAsync(subject, autoSave: true);
    }

    private static string BuildNameGradeCode(string name, GradeLevel? gradeLevel)
    {
        var cleanname = SanitizeCode(name);
        var gradePart = gradeLevel.HasValue
            ? gradeLevel.Value.ToString().Replace(" ", "") //e.g Grade 1 => Grade1
            : "General";

        var code = $"{cleanname}-{gradePart}";
        return code.Length <= SubjectConsts.CodeMaxLength
            ? code
            : code.Substring(0, SubjectConsts.CodeMaxLength);
    }

    private static string SanitizeCode(string input)
    {
        var s = (input ?? string.Empty).Trim();
        s = Regex.Replace(s, @"\s+", " ");
        s = Regex.Replace(s, @"[^A-Za-z0-9\-\s]", "");
        return s;
    }

    //private async Task<string> EnsureUniqueCodeAsync(Guid? tenantId, string baseCode)
    //{
    //    var code = baseCode;
    //    var existing = await _subjectRepository.FindByCodeAsync(tenantId, code);
    //    if (existing == null)
    //    {
    //        return code;
    //    }

    //    var i = 2;
    //    while (true)
    //    {
    //        var suffix = "-" + i;
    //        var maxBaseLen = SubjectConsts.CodeMaxLength - suffix.Length;
    //        var trimmedBase = baseCode.Length > maxBaseLen ? baseCode[..maxBaseLen] : baseCode;
    //        code = trimmedBase + suffix;

    //        var dupe = await _subjectRepository.FindByCodeAsync(tenantId, code);
    //        if (dupe == null) return code;
    //        i++;
    //    }
    //}
}

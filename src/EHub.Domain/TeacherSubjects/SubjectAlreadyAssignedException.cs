using EHub.Students;
using System;
using Volo.Abp;

namespace EHub.TeacherSubjects;

public class SubjectAlreadyAssignedException : BusinessException
{
    public SubjectAlreadyAssignedException(Guid staffId, Section? section) : base(EHubDomainErrorCodes.TeacherSubjectAlreadyExists)
    {
        WithData("staffId", staffId);
        WithData("section", section!);
    }
}

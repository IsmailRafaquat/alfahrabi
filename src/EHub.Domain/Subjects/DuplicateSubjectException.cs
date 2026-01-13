using Volo.Abp;
using EHub.Students;

namespace EHub.Subjects;

public class DuplicateSubjectException : BusinessException
{
    public DuplicateSubjectException(string name, GradeLevel? gradeLevel)
        : base(EHubDomainErrorCodes.DuplicateSubject) // keep exact key from your constants
    {
        WithData("Name", name);
        WithData("GradeLevel", gradeLevel?.ToString() ?? "General");
    }
}

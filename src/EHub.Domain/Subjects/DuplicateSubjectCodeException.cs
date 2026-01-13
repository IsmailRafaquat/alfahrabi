using Volo.Abp;

namespace EHub.Subjects;

public class DuplicateSubjectCodeException : BusinessException
{
    public DuplicateSubjectCodeException(string name) : base(EHubDomainErrorCodes.DuplicateSubject)
    {
        WithData("name", name);
    }
}

using Volo.Abp;

namespace EHub.Students;

public class StudentEmailAlreadyUsedException : BusinessException
{
    public StudentEmailAlreadyUsedException(string email) : base(EHubDomainErrorCodes.StudentAlreadyExists)
    {
        WithData("email", email);
    }
}

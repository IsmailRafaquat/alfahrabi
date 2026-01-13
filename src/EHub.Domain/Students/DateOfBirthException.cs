using Volo.Abp;

namespace EHub.Students;

public class DateOfBirthException : BusinessException
{
    public DateOfBirthException() : base(EHubDomainErrorCodes.InvalidDateOfBirth)
    {

    }
}

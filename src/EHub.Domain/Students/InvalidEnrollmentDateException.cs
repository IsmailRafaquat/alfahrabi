using Volo.Abp;

namespace EHub.Students;

public class InvalidEnrollmentDateException : BusinessException
{
    public InvalidEnrollmentDateException() : base(EHubDomainErrorCodes.InvalidEnrollmentDate)
    {

    }
}

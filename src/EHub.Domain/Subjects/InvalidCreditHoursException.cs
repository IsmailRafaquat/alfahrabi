using Volo.Abp;

namespace EHub.Subjects;

public class InvalidCreditHoursException : BusinessException
{
    public InvalidCreditHoursException(decimal? creditHours) : base(EHubDomainErrorCodes.InvalidCreditHours)
    {
        WithData("CreditHours", creditHours!);
    }
}

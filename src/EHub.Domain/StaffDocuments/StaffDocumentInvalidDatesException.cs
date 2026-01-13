using System;
using Volo.Abp;

namespace EHub.StaffDocuments;

public class StaffDocumentInvalidDatesException : BusinessException
{
    public StaffDocumentInvalidDatesException(DateTime issueDate, DateTime expireDate)
        : base(EHubDomainErrorCodes.StaffDocumentInvalidDates)
    {
        WithData("issueDate", issueDate);
        WithData("expireDate", expireDate);
    }
}

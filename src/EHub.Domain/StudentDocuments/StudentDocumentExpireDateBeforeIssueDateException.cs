using System;
using Volo.Abp;

namespace EHub.StudentDocuments;

public class StudentDocumentExpireDateBeforeIssueDateException : BusinessException
{
    public StudentDocumentExpireDateBeforeIssueDateException(DateTime? issueDate, DateTime? expireDate)
        : base(EHubDomainErrorCodes.StudentDocumentExpireDateBeforeIssueDate)
    {
        WithData("issueDate", issueDate);
        WithData("expireDate", expireDate);
    }
}

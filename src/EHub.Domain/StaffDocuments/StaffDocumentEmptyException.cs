using Volo.Abp;

namespace EHub.StaffDocuments;

public class StaffDocumentEmptyException : BusinessException
{
    public StaffDocumentEmptyException() : base(EHubDomainErrorCodes.StaffDocumentEmpty) { }
}


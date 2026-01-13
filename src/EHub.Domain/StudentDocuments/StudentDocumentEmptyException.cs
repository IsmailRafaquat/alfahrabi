using Volo.Abp;

namespace EHub.StudentDocuments;

public class StudentDocumentEmptyException : BusinessException
{
    public StudentDocumentEmptyException()
      : base(EHubDomainErrorCodes.StudentDocumentEmpty)
    {
    }
}

using Volo.Abp.BlobStoring;

namespace EHub.FileAttachments;

[BlobContainerName(FileContainerName)]
public class FileContainer
{
    public const string FileContainerName = "files";
}

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.FileAttachments;

public class FileManager(IBlobContainer<FileContainer> blobContainer, IConfiguration configuration, ICurrentTenant currentTenant) : DomainService
{
    private readonly IBlobContainer<FileContainer> _blobContainer = blobContainer;
    private readonly IConfiguration _configuration = configuration;
    private readonly ICurrentTenant _currentTenant = currentTenant;

    public async Task<FileAttachment> SaveAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new BusinessException(EHubDomainErrorCodes.EmptyFile);

        var fileExtension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(fileExtension))
            throw new BusinessException(EHubDomainErrorCodes.InvalidFileFormat);

        var cleanedFolder = folder?.Trim('/').Replace("\\", "/") ?? "misc";
        var blobName = $"{cleanedFolder}/{Guid.NewGuid()}{fileExtension}";

        var rootPath = _configuration["LocalStorageSetting:StoragePath"] ?? "images";

        var scopeSegment = _currentTenant.IsAvailable
            ? Path.Combine("tenants", _currentTenant.Id!.ToString())
            : "host";

        var fullFolderPath = Path.Combine(
            Environment.CurrentDirectory,
            "wwwroot",
            rootPath,
            scopeSegment,
            "files",
            cleanedFolder
        );

        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
        }

        await _blobContainer.SaveAsync(blobName, fileStream, overrideExisting: true, cancellationToken: cancellationToken);
        var filePath = BuildUrl(blobName);

        var fileAttachment = new FileAttachment(fileName, filePath, blobName);

        return fileAttachment;
    }


    public async Task DeleteFileAsync(FileAttachment attachment)
    {
        if (attachment == null)
            throw new BusinessException("FileAttachment cannot be null.");

        await _blobContainer.DeleteAsync(attachment.BlobName);

        // Convert URL -> absolute path under wwwroot
        var uri = new Uri(attachment.Path, UriKind.Absolute);
        var relPath = uri.AbsolutePath.TrimStart('/'); // e.g. images/tenants/.../file.png

        var fullPath = Path.Combine(
            Environment.CurrentDirectory,
            "wwwroot",
            relPath.Replace("/", Path.DirectorySeparatorChar.ToString())
        );

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    public async Task<byte[]> GetAllBytesAsync(string fileName, CancellationToken cancellationToken = default)
    {
        ValidateFileName(fileName);
        return await _blobContainer.GetAllBytesAsync(fileName, cancellationToken: cancellationToken);
    }

    public async Task<Stream> GetAsync(string fileName, CancellationToken cancellationToken = default)
    {
        ValidateFileName(fileName);
        return await _blobContainer.GetAsync(fileName, cancellationToken: cancellationToken);
    }


    private string BuildUrl(string blobName)
    {
        // blobName example: "student-documents/xxxx.png"
        var baseUrl = (_configuration["LocalStorageSetting:BaseUrl"] ?? "").TrimEnd('/');
        var basePath = (_configuration["LocalStorageSetting:StoragePath"] ?? "images").Trim('/');

        // Your actual folder layout in wwwroot:
        // /images/tenants/{tenantId}/files/{blobName}
        // or /images/host/files/{blobName} (host side)
        var scopeSegment = _currentTenant.IsAvailable
            ? $"tenants/{_currentTenant.Id}"
            : "host";

        var relative = $"{basePath}/{scopeSegment}/files/{blobName}".Replace("\\", "/");

        return string.IsNullOrWhiteSpace(baseUrl)
            ? "/" + relative
            : baseUrl + "/" + relative;
    }




    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new BusinessException(EHubDomainErrorCodes.NullField)
                .WithData("field", nameof(fileName));
        }

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext))
        {
            throw new BusinessException(EHubDomainErrorCodes.InvalidFileFormat);
        }
    }
}

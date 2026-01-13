using EHub.FileAttachments;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace EHub.StaffDocuments;

public class StaffDocumentManager : DomainService
{
    private readonly IRepository<StaffDocument, Guid> _repository;
    private readonly FileManager _fileManager;

    public StaffDocumentManager(
        IRepository<StaffDocument, Guid> repository,
        FileManager fileManager)
    {
        _repository = repository;
        _fileManager = fileManager;
    }

    public async Task<StaffDocument> CreateAsync(
        Guid staffId,
        StaffDocumentType documentType,
        DateTime? issueDate,
        DateTime? expireDate,
        string? description,
        bool isVerified,
        IFormFile file)
    {
        Check.NotNull(staffId, nameof(staffId));
        Check.NotNull(documentType, nameof(documentType));
        Check.NotNull(file, nameof(file));

        var existing = await _repository.FirstOrDefaultAsync(x =>
            x.StaffId == staffId && x.StaffDT == documentType);

        if (existing != null)
        {
            if (!string.IsNullOrWhiteSpace(existing.FileAttachments?.Path))
            {
                await _fileManager.DeleteFileAsync(existing.FileAttachments);
            }

            await _repository.DeleteAsync(existing, autoSave: true);
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        var attachment = await _fileManager.SaveAsync(stream, file.FileName, "staff-documents");

        var doc = new StaffDocument(
            GuidGenerator.Create(),
            staffId,
            documentType,
            issueDate,
            expireDate,
            description,
            isVerified,
            attachment
        );

        await _repository.InsertAsync(doc, autoSave: true);
        return doc;
    }

    public async Task DeleteAsync(Guid id)
    {
        Check.NotNull(id, nameof(id));

        var doc = await _repository.FirstOrDefaultAsync(x => x.Id == id);
        if (doc == null)
        {
            throw new StaffDocumentEmptyException();
        }

        if (doc.FileAttachments != null && !string.IsNullOrWhiteSpace(doc.FileAttachments.Path))
        {
            await _fileManager.DeleteFileAsync(doc.FileAttachments);
        }

        await _repository.DeleteAsync(doc, autoSave: true);
    }

    public async Task<StaffDocument> UpdateAsync(
        Guid id,
        Guid staffId,
        StaffDocumentType documentType,
        DateTime? issueDate,
        DateTime? expireDate,
        string? description,
        bool isVerified)
    {
        Check.NotNull(id, nameof(id));
        Check.NotNull(staffId, nameof(staffId));
        Check.NotNull(documentType, nameof(documentType));

        var doc = await _repository.FirstOrDefaultAsync(x => x.Id == id);
        if (doc == null)
        {
            throw new StaffDocumentEmptyException();
        }

        // Keep same style as your StudentDocumentManager (direct property set)
        doc.StaffId = staffId;
        doc.StaffDT = documentType;
        doc.IssueDate = issueDate;
        doc.ExpireDate = expireDate;
        doc.IsVerified = isVerified;
        doc.ChangeDescription(description);

        await _repository.UpdateAsync(doc, autoSave: true);
        return doc;
    }
}

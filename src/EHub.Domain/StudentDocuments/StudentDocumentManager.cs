using EHub.FileAttachments;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace EHub.StudentDocuments;

public class StudentDocumentManager : DomainService
{
    private readonly IRepository<StudentDocument, Guid> _repository;
    private readonly FileManager _fileManager;

    public StudentDocumentManager(
        IRepository<StudentDocument, Guid> repository,
        FileManager fileManager)
    {
        _repository = repository;
        _fileManager = fileManager;
    }

    public async Task<StudentDocument> CreateAsync(
        Guid studentId,
        StudentDocumentType documentType,
        DateTime? issueDate,
        DateTime? expireDate,
        string? description,
        bool isVerified,
        IFormFile file)
    {
        Check.NotNull(studentId, nameof(studentId));
        Check.NotNull(documentType, nameof(documentType));
        Check.NotNull(file, nameof(file));

        // One doc per Student + Type (same pattern as ConsumerDocument)
        var existing = await _repository.FirstOrDefaultAsync(x =>
            x.StudentId == studentId && x.DocumentType == documentType);

        if (existing != null)
        {
            if (!string.IsNullOrWhiteSpace(existing.FileAttachment?.Path))
            {
                await _fileManager.DeleteFileAsync(existing.FileAttachment);
            }

            await _repository.DeleteAsync(existing, autoSave: true);
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        var attachment = await _fileManager.SaveAsync(stream, file.FileName, "student-documents");

        var doc = new StudentDocument(
            GuidGenerator.Create(),
            studentId,
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
            throw new StudentDocumentEmptyException();
        }

        if (doc.FileAttachment != null && !string.IsNullOrWhiteSpace(doc.FileAttachment.Path))
        {
            await _fileManager.DeleteFileAsync(doc.FileAttachment);
        }

        await _repository.DeleteAsync(doc, autoSave: true);
    }

    public async Task<StudentDocument> UpdateAsync(
        Guid id,
        Guid studentId,
        StudentDocumentType documentType,
        DateTime? issueDate,
        DateTime? expireDate,
        string? description,
        bool isVerified)
    {
        Check.NotNull(id, nameof(id));
        Check.NotNull(studentId, nameof(studentId));
        Check.NotNull(documentType, nameof(documentType));

        var doc = await _repository.FirstOrDefaultAsync(x => x.Id == id);
        if (doc == null)
        {
            throw new StudentDocumentEmptyException();
        }

        doc.StudentId = studentId;
        doc.DocumentType = documentType;
        doc.IssueDate = issueDate;
        doc.ExpireDate = expireDate;
        doc.IsVerified = isVerified;
        doc.ChangeDescription(description);

        await _repository.UpdateAsync(doc, autoSave: true);
        return doc;
    }

}

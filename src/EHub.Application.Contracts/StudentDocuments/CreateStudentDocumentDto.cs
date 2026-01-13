using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.StudentDocuments;

public class CreateStudentDocumentDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public StudentDocumentType DocumentType { get; set; }

    public DateTime? IssueDate { get; set; }
    public DateTime? ExpireDate { get; set; }

    public string? Description { get; set; }

    public bool IsVerified { get; set; }
}

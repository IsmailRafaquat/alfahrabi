using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.StaffDocuments;

public class UpdateStaffDocumentDto
{
    [Required]
    public Guid StaffId { get; set; }

    [Required]
    public StaffDocumentType StaffDT { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public string? Description { get; set; }
    public bool IsVerified { get; set; }
}

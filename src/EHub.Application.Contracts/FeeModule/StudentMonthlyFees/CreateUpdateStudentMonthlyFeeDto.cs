using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.FeeModule.StudentMonthlyFees;

public class CreateUpdateStudentMonthlyFeeDto
{
    [Required] public Guid StudentId { get; set; }
    [Required] public DateTime Month { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Remarks { get; set; }
}

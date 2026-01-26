using EHub.Students;
using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.FeeModule.StudentMonthlyFees;

public class BulkGenerateStudentMonthlyFeeDto
{
    public GradeLevel? GradeLevel { get; set; }
    public Section? Section { get; set; }
    public Shift? Shift { get; set; }
    public Term? Term { get; set; }

    [Required] public DateTime Month { get; set; }      // will store yyyy-MM-01
    public DateTime? DueDate { get; set; }
    public string? Remarks { get; set; }

    public bool SkipIfExists { get; set; } = true;
}

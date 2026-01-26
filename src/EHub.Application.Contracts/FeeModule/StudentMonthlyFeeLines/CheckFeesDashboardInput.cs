using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class CheckFeesDashboardInput : PagedAndSortedResultRequestDto
{
    public DateTime Month { get; set; }
    public DateTime? AsOfDate { get; set; }

    // Optional filters (match your BulkGenerate filters)
    public int? GradeLevel { get; set; }
    public int? Section { get; set; }
    public int? Shift { get; set; }
    public int? Term { get; set; }

    public Guid? StudentId { get; set; }
}

using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.StudentMonthlyFees;

public class GetStudentMonthlyFeeListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? StudentId { get; set; }
    public DateTime? Month { get; set; } // month filter
    public DateTime? CollectedOn { get; set; }
}

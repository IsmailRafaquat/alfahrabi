using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class GetStudentMonthlyFeeLineListInput : PagedAndSortedResultRequestDto
{
    public Guid? StudentMonthlyFeeId { get; set; }
    public Guid? FeeHeadId { get; set; }
    public string? Filter { get; set; }
}

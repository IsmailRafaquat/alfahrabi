using EHub.Students;
using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.StudentMonthlyFees;

public class StudentMonthlyFeeDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public Guid StudentId { get; set; }
    public DateTime Month { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Remarks { get; set; }

    public string? StudentName { get; set; }
    public string? AdmissionNo { get; set; }
    public GradeLevel? GradeLevel { get; set; }

}

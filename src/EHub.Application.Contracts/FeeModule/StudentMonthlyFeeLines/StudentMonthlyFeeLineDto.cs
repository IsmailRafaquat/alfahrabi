using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class StudentMonthlyFeeLineDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public Guid StudentMonthlyFeeId { get; set; }
    public Guid FeeHeadId { get; set; }

    public decimal ExpectedAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal PaidAmount { get; set; }

    public decimal NetAmount { get; set; }
    public decimal OutstandingAmount { get; set; }

    public string? FeeHeadName { get; set; }
}

using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.StudentFeeDiscounts;

public class StudentFeeDiscountDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;

    public Guid? FeeHeadId { get; set; }
    public string? FeeHeadName { get; set; }

    public DiscountType DiscountType { get; set; }
    public decimal Value { get; set; }

    public int? StartMonth { get; set; }
    public int? EndMonth { get; set; }

    public string Reason { get; set; } = string.Empty;

    public Guid? ApprovedByStaffId { get; set; }
    public string? ApprovedByStaffName { get; set; }

    public bool IsActive { get; set; }
}

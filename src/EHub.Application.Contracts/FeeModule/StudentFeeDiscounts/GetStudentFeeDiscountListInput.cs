using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.StudentFeeDiscounts;

public class GetStudentFeeDiscountListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? FeeHeadId { get; set; }
    public DiscountType? DiscountType { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsApproved { get; set; }
}

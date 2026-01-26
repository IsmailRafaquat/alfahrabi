using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.FeeModule.FeeStructureItems;

public class CreateUpdateFeeStructureItemDto
{
    [Required]
    public Guid FeeStructureId { get; set; }

    [Required]
    public Guid FeeHeadId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MonthlyAmount { get; set; }

    public bool IsMandatory { get; set; }
}

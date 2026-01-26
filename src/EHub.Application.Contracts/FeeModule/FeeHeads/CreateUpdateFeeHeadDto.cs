using System.ComponentModel.DataAnnotations;

namespace EHub.FeeModule.FeeHeads;

public class CreateUpdateFeeHeadDto
{
    [Required]
    [StringLength(FeeModuleConsts.NameMaxLength)]
    public string Name { get; set; } = default!;

    public bool IsActive { get; set; } = true;
}

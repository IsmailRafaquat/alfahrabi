using System;
using Volo.Abp.Application.Dtos;

namespace EHub.FeeModule.FeeStructures;

public class FeeStructureLookupDto : EntityDto<Guid>
{
    public string DisplayName { get; set; } = default!;
}

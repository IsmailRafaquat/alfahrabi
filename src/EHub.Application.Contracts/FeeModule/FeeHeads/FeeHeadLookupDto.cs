using System;

namespace EHub.FeeModule.FeeHeads;

public class FeeHeadLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}

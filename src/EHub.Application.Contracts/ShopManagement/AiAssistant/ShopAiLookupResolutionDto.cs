using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiLookupChoiceDto
{
    public Guid Id { get; set; }
    public string DisplayText { get; set; } = string.Empty;
}

/// <summary>Emitted when a name the user gave (e.g. "Ali") matched more than one current-tenant record.</summary>
public class ShopAiLookupResolutionDto
{
    /// <summary>Which command field this ambiguity is for, e.g. "customerId".</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>What the user said, e.g. "Ali".</summary>
    public string Query { get; set; } = string.Empty;

    public List<ShopAiLookupChoiceDto> Choices { get; set; } = new();
}

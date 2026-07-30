using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Resolves a name Ollama extracted (e.g. "Ali") into a current-tenant entity ID. Ollama is never
/// trusted to supply an ID itself - see the *AiCommand payload types, none of which have an Id field.
/// </summary>
public interface IShopAiLookupResolver
{
    Task<ShopAiLookupResult> ResolveCustomerByNameAsync(string name, CancellationToken cancellationToken = default);
}

public class ShopAiLookupResult
{
    public Guid? Id { get; set; }
    public string? DisplayName { get; set; }
    public bool IsAmbiguous { get; set; }
    public List<ShopAiLookupChoiceDto> Choices { get; set; } = new();
    public bool Found => Id.HasValue;
}

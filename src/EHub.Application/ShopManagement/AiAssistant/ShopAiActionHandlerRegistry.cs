using System.Collections.Generic;
using System.Linq;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant;

public interface IShopAiActionHandlerRegistry
{
    bool TryGetHandler(ShopAiActionType action, out IShopAiActionHandler? handler);

    /// <summary>Action names Ollama is allowed to return - used to build the system prompt allowlist and to reject anything else.</summary>
    IReadOnlyCollection<string> GetAllowedActionNames();
}

/// <summary>
/// Explicit registry, built from whatever IShopAiActionHandler implementations DI resolves (each
/// one registers itself via ITransientDependency / AsImplementedInterfaces in its own file) -
/// deliberately NOT reflection-driven action dispatch. Adding a new action means adding a new
/// handler class and nothing else; there is no string-based Activator.CreateInstance anywhere here.
/// </summary>
public class ShopAiActionHandlerRegistry : IShopAiActionHandlerRegistry, ITransientDependency
{
    private readonly Dictionary<ShopAiActionType, IShopAiActionHandler> _handlers;

    public ShopAiActionHandlerRegistry(IEnumerable<IShopAiActionHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.ActionType);
    }

    public bool TryGetHandler(ShopAiActionType action, out IShopAiActionHandler? handler) =>
        _handlers.TryGetValue(action, out handler);

    public IReadOnlyCollection<string> GetAllowedActionNames() =>
        _handlers.Keys.Select(k => k.ToString()).ToList();
}

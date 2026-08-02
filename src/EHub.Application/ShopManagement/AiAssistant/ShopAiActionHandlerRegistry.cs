using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant;

public interface IShopAiActionHandlerRegistry
{
    bool TryGetHandler(ShopAiActionType action, out IShopAiActionHandler? handler);

    /// <summary>Action names Ollama is allowed to return - used to build the system prompt allowlist and to reject anything else.</summary>
    IReadOnlyCollection<string> GetAllowedActionNames();

    /// <summary>The subset of GetAllowedActionNames() that are read-only (IsWriteAction == false) - the only actions the command parser's prompt should ever offer under intent ReadBusinessData. Writes always go through intent StartRecordCreation + moduleKey instead, so a single action name is never ambiguous between the two intents.</summary>
    IReadOnlyCollection<string> GetReadActionNames();
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

    public ShopAiActionHandlerRegistry(IEnumerable<IShopAiActionHandler> handlers, ILogger<ShopAiActionHandlerRegistry> logger)
    {
        // GroupBy+First rather than ToDictionary: tolerates the same ActionType being registered
        // more than once (e.g. both conventionally and explicitly) instead of throwing.
        _handlers = handlers.GroupBy(h => h.ActionType).ToDictionary(g => g.Key, g => g.First());
        // TEMPORARY diagnostic - remove once the intent-misclassification investigation is done.
        logger.LogInformation("ShopAiActionHandlerRegistry resolved {Count} handlers: {ActionTypes}", _handlers.Count, string.Join(",", _handlers.Keys));
    }

    public bool TryGetHandler(ShopAiActionType action, out IShopAiActionHandler? handler) =>
        _handlers.TryGetValue(action, out handler);

    public IReadOnlyCollection<string> GetAllowedActionNames() =>
        _handlers.Keys.Select(k => k.ToString()).ToList();

    public IReadOnlyCollection<string> GetReadActionNames() =>
        _handlers.Values.Where(h => !h.IsWriteAction).Select(h => h.ActionType.ToString()).ToList();
}

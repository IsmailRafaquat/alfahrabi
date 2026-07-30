using System.Threading;
using System.Threading.Tasks;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// One handler per ShopAiActionType, registered explicitly in ShopAiActionHandlerRegistry - never
/// discovered or invoked via reflection. PrepareAsync is where the raw, untrusted
/// ShopAiParsedCommand gets deserialized into the handler's strongly-typed *AiCommand, validated
/// (via ShopAiCommandValidator) and resolved (via ShopAiLookupResolver) into a preview. ExecuteAsync
/// only ever runs after a confirmation token has been verified, and only ever calls an existing ABP
/// AppService or Domain Manager - it never touches a repository or DbContext directly.
/// </summary>
public interface IShopAiActionHandler
{
    ShopAiActionType ActionType { get; }

    bool IsWriteAction { get; }

    Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default);

    Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default);
}

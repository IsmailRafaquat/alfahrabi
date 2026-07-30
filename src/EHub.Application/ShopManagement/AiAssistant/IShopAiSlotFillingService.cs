using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Owns multi-turn field collection for guided record creation. It never calls a handler's
/// ExecuteAsync and never touches a repository for the target entity - once every required field
/// is present and validated it just hands back a ShopAiParsedCommand shaped exactly like a
/// single-message write request, so the caller can run it through the SAME
/// handler.PrepareAsync -> confirmation-token -> AwaitingConfirmation pipeline already used for
/// one-shot writes like CreateCustomer. A guided creation that happens to arrive complete in one
/// message (e.g. "Add a customer named Ali, phone 03001234567") never creates a ShopAiPendingAction
/// row at all - IsComplete is true immediately.
/// </summary>
public interface IShopAiSlotFillingService
{
    Task<ShopAiSlotFillingResult> StartAsync(
        ShopAiConversation conversation,
        System.Guid userId,
        System.Guid sourceMessageId,
        ShopAiModuleMetadata module,
        ShopAiParsedCommand command,
        CancellationToken cancellationToken = default);

    Task<ShopAiSlotFillingResult> ContinueAsync(
        ShopAiPendingAction pendingAction,
        string userMessage,
        CancellationToken cancellationToken = default);
}

public class ShopAiSlotFillingResult
{
    public bool IsComplete { get; set; }

    /// <summary>True when the pending action's collecting-information window (30 minutes) has passed - the caller must not continue it, only offer to start over.</summary>
    public bool IsExpired { get; set; }

    /// <summary>Set when IsComplete - Action = module.CreateAction, Parameters = every collected field, ready for handler.PrepareAsync.</summary>
    public ShopAiParsedCommand? CompletedCommand { get; set; }

    /// <summary>Set when !IsComplete - the created-or-updated row tracking what has been collected so far.</summary>
    public ShopAiPendingAction? PendingAction { get; set; }

    public string ModuleDisplayName { get; set; } = string.Empty;
    public List<ShopAiPreviewFieldDto> CollectedFields { get; set; } = new();
    public List<string> MissingRequiredFields { get; set; } = new();
    public int RequiredFieldCount { get; set; }
    public int CompletedRequiredFieldCount { get; set; }

    /// <summary>Human-readable question for the next missing field(s), in the module's own terms - never invented, built from field metadata.</summary>
    public string? NextQuestion { get; set; }

    public List<string> Warnings { get; set; } = new();
}

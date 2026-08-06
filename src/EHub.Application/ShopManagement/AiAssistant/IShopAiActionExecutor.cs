using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// The single entry point MCP tools use to reach existing AppServices/Domain Managers - it never
/// touches a repository or DbContext directly, and never duplicates field validation, lookup
/// resolution, confirmation-token handling, or business rules. Internally it is a thin wrapper
/// around the SAME IShopAiActionHandlerRegistry / IShopAiConfirmationService / ShopAiPendingAction
/// infrastructure the chat-based Shop AI Assistant already uses - a write action prepared through
/// an MCP tool and one prepared through a chat message go through byte-for-byte the same
/// PrepareAsync -> confirmation token -> handler.ExecuteAsync pipeline.
/// </summary>
public interface IShopAiActionExecutor
{
    /// <summary>Immediate execution for read-only actions - no confirmation, no pending action created.</summary>
    Task<ShopAiExecutionResultDto> ExecuteReadAsync(ShopAiActionType actionType, JsonElement payload, CancellationToken cancellationToken = default);

    /// <summary>Validates a write action and returns a confirmation preview. Nothing is written to business data yet.</summary>
    Task<ShopAiActionExecutorPrepareResult> PrepareAsync(ShopAiActionType actionType, JsonElement payload, CancellationToken cancellationToken = default);

    /// <summary>Re-validates everything (tenant, user, token, expiry, payload hash, permissions, business rules) and only then calls the real AppService/Domain Manager.</summary>
    Task<ShopAiExecutionResultDto> ConfirmAsync(Guid pendingActionId, string confirmationToken, CancellationToken cancellationToken = default);
}

/// <summary>Everything an MCP prepare-tool needs to hand back to its caller - the ids a subsequent confirm-tool call must supply, plus the human-readable preview.</summary>
public class ShopAiActionExecutorPrepareResult
{
    public Guid ConversationId { get; set; }
    public Guid MessageId { get; set; }
    public Guid PendingActionId { get; set; }
    public ShopAiActionPreviewDto Preview { get; set; } = default!;
}

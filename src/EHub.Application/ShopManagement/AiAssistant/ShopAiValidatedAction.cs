using System;
using System.Text.Json;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// What ExecuteAsync receives - the same extracted parameters PrepareAsync saw, reloaded from the
/// ShopAiMessage row (ActionPayloadJson) rather than from anything the frontend sends at
/// confirmation time. Handlers are expected to re-run their field/lookup validation here too
/// ("revalidate business data") rather than trust the preview built minutes earlier - the frontend
/// never gets a chance to resend or tamper with this payload either way: ConfirmActionAsync accepts
/// only a token, never a payload.
/// </summary>
public class ShopAiValidatedAction
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid MessageId { get; set; }
    public ShopAiActionType Action { get; set; }
    public ShopAiLanguage Language { get; set; }

    /// <summary>The validated, resolved payload (IDs already substituted for names) - handler-specific shape.</summary>
    public JsonElement PayloadJson { get; set; }
}

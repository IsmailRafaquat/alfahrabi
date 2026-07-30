using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Intentionally contains ONLY the token, not the payload again - the validated payload the token
/// was issued for is looked up server-side from the message it belongs to. See
/// IShopAiConfirmationService for why the frontend is never trusted to resend the payload.
/// </summary>
public class ConfirmShopAiActionDto
{
    [Required]
    public Guid ConversationId { get; set; }

    [Required]
    public Guid MessageId { get; set; }

    [Required]
    public string ConfirmationToken { get; set; } = string.Empty;
}

public class CancelShopAiActionDto
{
    [Required]
    public Guid ConversationId { get; set; }

    [Required]
    public Guid MessageId { get; set; }
}

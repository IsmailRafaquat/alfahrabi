using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Deliberately does NOT include TenantId or UserId - those always come from ICurrentTenant /
/// ICurrentUser on the server, never from the client.
/// </summary>
public class SendShopAiMessageDto
{
    [Required]
    public Guid ConversationId { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;
}

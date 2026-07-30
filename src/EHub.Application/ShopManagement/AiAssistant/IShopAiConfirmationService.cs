using System;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Pure token cryptography - no entity or database access. The caller (ShopAiAssistantAppService)
/// is responsible for persisting TokenHash on the ShopAiMessage (via ShopAiMessageManager) and for
/// re-checking message status / ownership / expiry before calling Verify.
/// </summary>
public interface IShopAiConfirmationService
{
    ShopAiConfirmationToken Issue(Guid tenantId, Guid userId, Guid conversationId, Guid messageId, ShopAiActionType action, string payloadJson);

    ShopAiConfirmationCheckResult Verify(
        string providedToken,
        string storedTokenHash,
        Guid tenantId,
        Guid userId,
        Guid conversationId,
        Guid messageId,
        ShopAiActionType action,
        string payloadJson,
        DateTime? expiryDate);
}

public class ShopAiConfirmationToken
{
    /// <summary>Plaintext token - returned to the client exactly once, in the preview. Never persisted.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>What actually gets stored on ShopAiMessage.ConfirmationTokenHash.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }
}

public class ShopAiConfirmationCheckResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public static ShopAiConfirmationCheckResult Ok() => new() { Success = true };
    public static ShopAiConfirmationCheckResult Fail(string errorCode, string errorMessage) => new() { Success = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
}

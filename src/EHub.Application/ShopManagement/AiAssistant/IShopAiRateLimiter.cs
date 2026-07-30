namespace EHub.ShopManagement.AiAssistant;

public enum ShopAiRateLimitCategory
{
    Text,
    Voice,
    Confirmation,
}

/// <summary>
/// Per-(tenant, user, category) sliding-window limiter. In-memory only - correct for a single
/// application instance; a multi-instance deployment would need a shared store (e.g. Redis) instead.
/// That's a known limitation for this phase, not a silent gap: exceeding the limit always fails
/// closed (request rejected), it never fails open.
/// </summary>
public interface IShopAiRateLimiter
{
    bool TryAcquire(System.Guid tenantId, System.Guid userId, ShopAiRateLimitCategory category);
}

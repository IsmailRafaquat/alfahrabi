using System;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiMessageManager : DomainService
{
    private readonly ICurrentTenant _currentTenant;

    public ShopAiMessageManager(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public ShopAiMessage Create(
        ShopAiConversation conversation,
        Guid userId,
        ShopAiMessageRole role,
        string messageText,
        string? originalTranscription,
        ShopAiLanguage detectedLanguage)
    {
        var tenantId = RequireTenant();
        if (conversation.TenantId != tenantId || conversation.UserId != userId)
        {
            throw new BusinessException("ShopManagement:AiConversationNotFound");
        }

        return new ShopAiMessage(GuidGenerator.Create(), tenantId, conversation.Id, userId, role, messageText, originalTranscription, detectedLanguage);
    }

    /// <summary>
    /// Every confirm/cancel request must call this before touching the message: it re-checks tenant
    /// AND user ownership by GUID alone is never sufficient (see IShopAiConfirmationService).
    /// </summary>
    public void ValidateOwnership(ShopAiMessage message, Guid tenantId, Guid userId, Guid conversationId)
    {
        if (message.TenantId != tenantId || message.UserId != userId || message.ConversationId != conversationId)
        {
            throw new BusinessException("ShopManagement:AiMessageNotFound");
        }
    }

    // Thin pass-throughs onto the entity's internal state-transition methods. They exist so the
    // Application layer (a different assembly) can drive the message lifecycle without the entity
    // exposing public setters that any caller could invoke out of order.
    public void MarkParsed(ShopAiMessage message, ShopAiActionType action, string actionPayloadJson, ShopAiLanguage language) =>
        message.MarkParsed(action, actionPayloadJson, language);

    public void MarkMissingInformation(ShopAiMessage message, string actionPayloadJson, ShopAiLanguage language) =>
        message.MarkMissingInformation(actionPayloadJson, language);

    public void SetAwaitingConfirmation(ShopAiMessage message, string confirmationTokenHash, DateTime expiryDate) =>
        message.SetAwaitingConfirmation(confirmationTokenHash, expiryDate);

    public void MarkExecuting(ShopAiMessage message) => message.MarkExecuting();

    public void MarkExecuted(ShopAiMessage message, string executionResultJson, DateTime executedDate) =>
        message.MarkExecuted(executionResultJson, executedDate);

    public void MarkFailed(ShopAiMessage message, string? errorCode, string errorMessage) =>
        message.MarkFailed(errorCode, errorMessage);

    public void MarkRejected(ShopAiMessage message) => message.MarkRejected();

    public void MarkCancelled(ShopAiMessage message) => message.MarkCancelled();

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}

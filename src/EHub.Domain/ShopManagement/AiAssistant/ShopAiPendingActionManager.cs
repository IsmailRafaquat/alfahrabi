using System;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiPendingActionManager : DomainService
{
    private readonly ICurrentTenant _currentTenant;

    public ShopAiPendingActionManager(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public ShopAiPendingAction Create(Guid userId, Guid conversationId, Guid sourceMessageId, ShopAiActionType actionType, string moduleKey, DateTime collectingExpiryDate)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
        return new ShopAiPendingAction(GuidGenerator.Create(), tenantId, userId, conversationId, sourceMessageId, actionType, moduleKey, collectingExpiryDate);
    }

    /// <summary>Every load-by-id must call this first - a pending action is only ever visible to the tenant/user/conversation it was created for.</summary>
    public void ValidateOwnership(ShopAiPendingAction pendingAction, Guid tenantId, Guid userId, Guid conversationId)
    {
        if (pendingAction.TenantId != tenantId || pendingAction.UserId != userId || pendingAction.ConversationId != conversationId)
        {
            throw new BusinessException("ShopManagement:AiPendingActionNotFound");
        }
    }

    public void UpdateCollectedState(ShopAiPendingAction pendingAction, string collectedValuesJson, string missingFieldsJson, string lookupResolutionsJson, DateTime collectingExpiryDate) =>
        pendingAction.UpdateCollectedState(collectedValuesJson, missingFieldsJson, lookupResolutionsJson, collectingExpiryDate);

    public void MarkReadyForConfirmation(ShopAiPendingAction pendingAction, DateTime confirmationExpiryDate) =>
        pendingAction.MarkReadyForConfirmation(confirmationExpiryDate);

    public void MarkExecuting(ShopAiPendingAction pendingAction) => pendingAction.MarkExecuting();
    public void MarkExecuted(ShopAiPendingAction pendingAction) => pendingAction.MarkExecuted();
    public void MarkCancelled(ShopAiPendingAction pendingAction) => pendingAction.MarkCancelled();
    public void MarkExpired(ShopAiPendingAction pendingAction) => pendingAction.MarkExpired();
    public void MarkFailed(ShopAiPendingAction pendingAction) => pendingAction.MarkFailed();
}

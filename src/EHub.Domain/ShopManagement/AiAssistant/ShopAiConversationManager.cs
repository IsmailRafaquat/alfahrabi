using System;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiConversationManager : DomainService
{
    private readonly ICurrentTenant _currentTenant;

    public ShopAiConversationManager(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public ShopAiConversation Create(Guid userId, string? title)
    {
        var tenantId = RequireTenant();
        return new ShopAiConversation(GuidGenerator.Create(), tenantId, userId, title);
    }

    /// <summary>
    /// Every conversation-scoped operation (send message, confirm, delete) must call this first.
    /// Without it, a user could reference another tenant's or another user's conversation by GUID alone.
    /// </summary>
    public void ValidateOwnership(ShopAiConversation conversation, Guid tenantId, Guid userId)
    {
        if (conversation.TenantId != tenantId || conversation.UserId != userId)
        {
            throw new BusinessException("ShopManagement:AiConversationNotFound");
        }
    }

    public void SetTitle(ShopAiConversation conversation, string? title) => conversation.SetTitle(title);

    public void TouchLastMessage(ShopAiConversation conversation, DateTime dateTime, ShopAiLanguage? detectedLanguage) =>
        conversation.TouchLastMessage(dateTime, detectedLanguage);

    public void Archive(ShopAiConversation conversation) => conversation.Archive();

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}

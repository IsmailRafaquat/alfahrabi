using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiConversation : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid UserId { get; protected set; }
    public string? Title { get; protected set; }
    public ShopAiLanguage DetectedLanguage { get; protected set; } = ShopAiLanguage.Unknown;
    public ShopAiConversationStatus Status { get; protected set; } = ShopAiConversationStatus.Active;
    public DateTime? LastMessageDate { get; protected set; }

    protected ShopAiConversation() { }

    internal ShopAiConversation(Guid id, Guid tenantId, Guid userId, string? title) : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Title = Check.Length(title?.Trim(), nameof(title), ShopAiConsts.TitleMaxLength);
    }

    internal void SetTitle(string? title) =>
        Title = Check.Length(title?.Trim(), nameof(title), ShopAiConsts.TitleMaxLength);

    internal void TouchLastMessage(DateTime dateTime, ShopAiLanguage? detectedLanguage)
    {
        LastMessageDate = dateTime;
        if (detectedLanguage.HasValue && detectedLanguage.Value != ShopAiLanguage.Unknown)
        {
            DetectedLanguage = detectedLanguage.Value;
        }
    }

    internal void Archive() => Status = ShopAiConversationStatus.Archived;
}

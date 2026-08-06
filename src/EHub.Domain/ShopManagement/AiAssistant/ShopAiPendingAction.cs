using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Server-side state for a guided, multi-turn record creation. The frontend chat history is a
/// convenience, not the source of truth - every value the user has supplied so far lives here,
/// validated and sanitized, so a page refresh or a new device can resume the same flow safely.
/// ExpiryDate means "act before this time or the row is dead" throughout, but its duration changes
/// meaning with Status: a 30-minute sliding window while collecting information, reset to a tight
/// 5-minute window the moment the action becomes ReadyForConfirmation (paired with issuing
/// ConfirmationTokenHash) - mirroring ShopAiMessage's confirmation-token expiry.
/// </summary>
public class ShopAiPendingAction : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid UserId { get; protected set; }
    public Guid ConversationId { get; protected set; }
    public Guid SourceMessageId { get; protected set; }
    public ShopAiActionType ActionType { get; protected set; }
    public string ModuleKey { get; protected set; } = string.Empty;
    public string CollectedValuesJson { get; protected set; } = "{}";
    public string MissingFieldsJson { get; protected set; } = "[]";
    public string LookupResolutionsJson { get; protected set; } = "[]";
    public ShopAiPendingActionStatus Status { get; protected set; }
    public DateTime ExpiryDate { get; protected set; }
    public string? ConfirmationTokenHash { get; protected set; }

    protected ShopAiPendingAction() { }

    internal ShopAiPendingAction(
        Guid id,
        Guid tenantId,
        Guid userId,
        Guid conversationId,
        Guid sourceMessageId,
        ShopAiActionType actionType,
        string moduleKey,
        DateTime collectingExpiryDate) : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        ConversationId = conversationId;
        SourceMessageId = sourceMessageId;
        ActionType = actionType;
        ModuleKey = Check.NotNullOrWhiteSpace(moduleKey, nameof(moduleKey), ShopAiConsts.ModuleKeyMaxLength);
        Status = ShopAiPendingActionStatus.CollectingInformation;
        ExpiryDate = collectingExpiryDate;
    }

    internal void UpdateCollectedState(string collectedValuesJson, string missingFieldsJson, string lookupResolutionsJson, DateTime collectingExpiryDate)
    {
        CollectedValuesJson = Truncate(collectedValuesJson, ShopAiConsts.CollectedValuesJsonMaxLength);
        MissingFieldsJson = Truncate(missingFieldsJson, ShopAiConsts.MissingFieldsJsonMaxLength);
        LookupResolutionsJson = Truncate(lookupResolutionsJson, ShopAiConsts.LookupResolutionsJsonMaxLength);
        Status = ShopAiPendingActionStatus.CollectingInformation;
        ExpiryDate = collectingExpiryDate;
    }

    internal void MoveToResolvingLookups(string lookupResolutionsJson, DateTime collectingExpiryDate)
    {
        LookupResolutionsJson = Truncate(lookupResolutionsJson, ShopAiConsts.LookupResolutionsJsonMaxLength);
        Status = ShopAiPendingActionStatus.ResolvingLookups;
        ExpiryDate = collectingExpiryDate;
    }

    /// <summary>
    /// All required fields are now collected and validated. The actual confirmation token is
    /// issued on the ShopAiMessage that carries the resulting preview (same mechanism as a
    /// one-shot write), not stored here - ConfirmationTokenHash on this row stays unused for the
    /// guided-creation flow; it exists for API-shape parity with the spec and is available for a
    /// future flow that confirms directly against the pending action instead of the message.
    /// </summary>
    internal void MarkReadyForConfirmation(DateTime confirmationExpiryDate)
    {
        Status = ShopAiPendingActionStatus.ReadyForConfirmation;
        ExpiryDate = confirmationExpiryDate;
    }

    internal void MarkExecuting()
    {
        Status = ShopAiPendingActionStatus.Executing;
        ConfirmationTokenHash = null;
    }

    internal void MarkExecuted()
    {
        Status = ShopAiPendingActionStatus.Executed;
        ConfirmationTokenHash = null;
    }

    internal void MarkCancelled()
    {
        Status = ShopAiPendingActionStatus.Cancelled;
        ConfirmationTokenHash = null;
    }

    internal void MarkExpired()
    {
        Status = ShopAiPendingActionStatus.Expired;
        ConfirmationTokenHash = null;
    }

    internal void MarkFailed()
    {
        Status = ShopAiPendingActionStatus.Failed;
        ConfirmationTokenHash = null;
    }

    internal bool IsActive => Status is ShopAiPendingActionStatus.CollectingInformation or ShopAiPendingActionStatus.ResolvingLookups or ShopAiPendingActionStatus.ReadyForConfirmation;

    private static string Truncate(string value, int maxLength) => value.Length <= maxLength ? value : value.Substring(0, maxLength);
}

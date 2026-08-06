using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Append-only outcome log for every action the assistant actually reached a terminal state on
/// (read actions execute and log immediately; write actions log once confirmed-and-executed, or
/// once rejected/expired). This is what "Conversation and action audit history" means in practice -
/// an admin can see exactly what the AI was allowed to do, for whom, and what it touched.
/// </summary>
public class ShopAiActionAudit : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid UserId { get; protected set; }
    public Guid ConversationId { get; protected set; }
    public Guid MessageId { get; protected set; }
    public string ActionName { get; protected set; } = string.Empty;
    public string SanitizedPayloadJson { get; protected set; } = string.Empty;
    public bool ConfirmationRequired { get; protected set; }
    public Guid? ConfirmedByUserId { get; protected set; }
    public DateTime? ConfirmationDate { get; protected set; }
    public ShopAiActionExecutionStatus ExecutionStatus { get; protected set; }
    public string? ResultReferenceType { get; protected set; }
    public Guid? ResultReferenceId { get; protected set; }
    public string? ErrorCode { get; protected set; }
    public string? ErrorMessage { get; protected set; }

    protected ShopAiActionAudit() { }

    internal ShopAiActionAudit(
        Guid id,
        Guid tenantId,
        Guid userId,
        Guid conversationId,
        Guid messageId,
        string actionName,
        string sanitizedPayloadJson,
        bool confirmationRequired,
        Guid? confirmedByUserId,
        DateTime? confirmationDate,
        ShopAiActionExecutionStatus executionStatus,
        string? resultReferenceType,
        Guid? resultReferenceId,
        string? errorCode,
        string? errorMessage) : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        ConversationId = conversationId;
        MessageId = messageId;
        ActionName = Check.NotNullOrWhiteSpace(actionName, nameof(actionName), ShopAiConsts.ActionNameMaxLength);
        SanitizedPayloadJson = sanitizedPayloadJson ?? "{}";
        if (SanitizedPayloadJson.Length > ShopAiConsts.SanitizedPayloadJsonMaxLength)
        {
            SanitizedPayloadJson = SanitizedPayloadJson.Substring(0, ShopAiConsts.SanitizedPayloadJsonMaxLength);
        }
        ConfirmationRequired = confirmationRequired;
        ConfirmedByUserId = confirmedByUserId;
        ConfirmationDate = confirmationDate;
        ExecutionStatus = executionStatus;
        ResultReferenceType = Check.Length(resultReferenceType, nameof(resultReferenceType), ShopAiConsts.ResultReferenceTypeMaxLength);
        ResultReferenceId = resultReferenceId;
        ErrorCode = Check.Length(errorCode, nameof(errorCode), ShopAiConsts.ErrorCodeMaxLength);
        ErrorMessage = Check.Length(errorMessage, nameof(errorMessage), ShopAiConsts.ErrorMessageMaxLength);
    }
}

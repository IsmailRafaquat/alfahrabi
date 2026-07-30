using System;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiActionAuditManager : DomainService
{
    private readonly ICurrentTenant _currentTenant;

    public ShopAiActionAuditManager(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public ShopAiActionAudit Record(
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
        string? errorMessage)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        return new ShopAiActionAudit(
            GuidGenerator.Create(), tenantId, userId, conversationId, messageId, actionName, sanitizedPayloadJson,
            confirmationRequired, confirmedByUserId, confirmationDate, executionStatus,
            resultReferenceType, resultReferenceId, errorCode, errorMessage);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using EHub.Localization;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// PendingActionId is purely a caller-facing handle - the actual confirmation-token verification
/// still runs against the same ShopAiMessage.ConfirmationTokenHash mechanism the chat flow already
/// uses (see IShopAiConfirmationService). This means every MCP write action also has a full row in
/// ShopAiConversations/ShopAiMessages, so it shows up in the same conversation-history/audit trail
/// as a chat-driven action - there is exactly one confirmation implementation in the whole system,
/// not two parallel ones.
/// </summary>
public class ShopAiActionExecutor : IShopAiActionExecutor, ITransientDependency
{
    private static readonly TimeSpan ConfirmationExpiry = TimeSpan.FromMinutes(5);

    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<ShopAiConversation, Guid> _conversationRepository;
    private readonly IRepository<ShopAiMessage, Guid> _messageRepository;
    private readonly IRepository<ShopAiPendingAction, Guid> _pendingActionRepository;
    private readonly IRepository<ShopAiActionAudit, Guid> _auditRepository;
    private readonly ShopAiConversationManager _conversationManager;
    private readonly ShopAiMessageManager _messageManager;
    private readonly ShopAiPendingActionManager _pendingActionManager;
    private readonly ShopAiActionAuditManager _auditManager;
    private readonly IShopAiActionHandlerRegistry _handlerRegistry;
    private readonly IShopAiModuleMetadataProvider _moduleMetadataProvider;
    private readonly IShopAiConfirmationService _confirmationService;
    private readonly IClock _clock;
    private readonly IStringLocalizer<EHubResource> _localizer;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public ShopAiActionExecutor(
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IRepository<ShopAiConversation, Guid> conversationRepository,
        IRepository<ShopAiMessage, Guid> messageRepository,
        IRepository<ShopAiPendingAction, Guid> pendingActionRepository,
        IRepository<ShopAiActionAudit, Guid> auditRepository,
        ShopAiConversationManager conversationManager,
        ShopAiMessageManager messageManager,
        ShopAiPendingActionManager pendingActionManager,
        ShopAiActionAuditManager auditManager,
        IShopAiActionHandlerRegistry handlerRegistry,
        IShopAiModuleMetadataProvider moduleMetadataProvider,
        IShopAiConfirmationService confirmationService,
        IClock clock,
        IStringLocalizer<EHubResource> localizer,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _pendingActionRepository = pendingActionRepository;
        _auditRepository = auditRepository;
        _conversationManager = conversationManager;
        _messageManager = messageManager;
        _pendingActionManager = pendingActionManager;
        _auditManager = auditManager;
        _handlerRegistry = handlerRegistry;
        _moduleMetadataProvider = moduleMetadataProvider;
        _confirmationService = confirmationService;
        _clock = clock;
        _localizer = localizer;
        _asyncExecuter = asyncExecuter;
    }

    public async Task<ShopAiExecutionResultDto> ExecuteReadAsync(ShopAiActionType actionType, JsonElement payload, CancellationToken cancellationToken = default)
    {
        var (tenantId, userId) = RequireIdentity();

        if (!_handlerRegistry.TryGetHandler(actionType, out var handler) || handler == null)
        {
            return Fail("AiActionNotSupported", "ShopManagement:AiActionNotSupported");
        }

        if (handler.IsWriteAction)
        {
            // A caller error, not a user error - MCP tool classes must only route write actions
            // through PrepareAsync/ConfirmAsync. Fail closed rather than silently writing.
            return Fail("AiActionNotSupported", "ShopManagement:AiActionNotSupported");
        }

        var command = BuildCommand(actionType, payload);

        try
        {
            await handler.PrepareAsync(command, cancellationToken);
            var validated = new ShopAiValidatedAction
            {
                TenantId = tenantId,
                UserId = userId,
                Action = actionType,
                Language = ShopAiLanguage.English,
                PayloadJson = payload,
            };
            return await handler.ExecuteAsync(validated, cancellationToken);
        }
        catch (ShopAiMissingInformationException ex)
        {
            return Fail("AiMissingInformation", ex.UserFriendlyMessage ?? "ShopManagement:AiMissingInformation");
        }
        catch (BusinessException ex)
        {
            return Fail(ex.Code, ShopAiExceptionFormatter.Format(_localizer, ex));
        }
    }

    public async Task<ShopAiActionExecutorPrepareResult> PrepareAsync(ShopAiActionType actionType, JsonElement payload, CancellationToken cancellationToken = default)
    {
        var (tenantId, userId) = RequireIdentity();

        if (!_handlerRegistry.TryGetHandler(actionType, out var handler) || handler == null || !handler.IsWriteAction)
        {
            throw new BusinessException("ShopManagement:AiActionNotSupported");
        }

        if (!_moduleMetadataProvider.GetModules().Any(m => m.CreateAction == actionType))
        {
            // Every approved write tool maps to exactly one module in the registry - if none
            // matches, this action was never meant to be reachable through the executor at all.
            throw new BusinessException("ShopManagement:AiActionNotSupported");
        }

        var module = _moduleMetadataProvider.GetModules().First(m => m.CreateAction == actionType);
        var command = BuildCommand(actionType, payload);

        ShopAiActionPreviewDto preview;
        try
        {
            preview = await handler.PrepareAsync(command, cancellationToken);
        }
        catch (ShopAiMissingInformationException ex)
        {
            throw new BusinessException("ShopManagement:AiMissingInformation")
                .WithData("MissingFields", string.Join(", ", ex.MissingFields));
        }

        // A dedicated conversation per MCP prepare call - MCP tools are one-shot request/response,
        // not a running chat, so there is no existing conversation to append to (unlike the chat
        // flow's ContinuePendingActionAsync, which reuses the conversation the user is already in).
        var conversation = _conversationManager.Create(userId, title: $"MCP: {module.DisplayName}");
        await _conversationRepository.InsertAsync(conversation, autoSave: true);

        var payloadJson = payload.GetRawText();
        var message = _messageManager.Create(conversation, userId, ShopAiMessageRole.Assistant, preview.ActionDisplayNameKey, originalTranscription: null, ShopAiLanguage.English);
        await _messageRepository.InsertAsync(message, autoSave: true);
        _messageManager.MarkParsed(message, actionType, payloadJson, ShopAiLanguage.English);

        var token = _confirmationService.Issue(tenantId, userId, conversation.Id, message.Id, actionType, payloadJson);
        _messageManager.SetAwaitingConfirmation(message, token.TokenHash, token.ExpiryDate);
        await _messageRepository.UpdateAsync(message, autoSave: true);

        var expiry = _clock.Now.Add(ConfirmationExpiry);
        var pendingAction = _pendingActionManager.Create(userId, conversation.Id, message.Id, actionType, module.ModuleKey, expiry);
        _pendingActionManager.UpdateCollectedState(pendingAction, payloadJson, "[]", "[]", expiry);
        _pendingActionManager.MarkReadyForConfirmation(pendingAction, token.ExpiryDate);
        await _pendingActionRepository.InsertAsync(pendingAction, autoSave: true);

        preview.ConfirmationToken = token.Token;
        preview.ConfirmationExpiryDate = token.ExpiryDate;

        return new ShopAiActionExecutorPrepareResult
        {
            ConversationId = conversation.Id,
            MessageId = message.Id,
            PendingActionId = pendingAction.Id,
            Preview = preview,
        };
    }

    public async Task<ShopAiExecutionResultDto> ConfirmAsync(Guid pendingActionId, string confirmationToken, CancellationToken cancellationToken = default)
    {
        var (tenantId, userId) = RequireIdentity();

        var pendingActionQuery = await _pendingActionRepository.GetQueryableAsync();
        var pendingAction = await _asyncExecuter.FirstOrDefaultAsync(pendingActionQuery.Where(x => x.Id == pendingActionId && x.TenantId == tenantId));
        if (pendingAction == null)
        {
            return Fail("AiPendingActionNotFound", "ShopManagement:AiPendingActionNotFound");
        }

        try
        {
            _pendingActionManager.ValidateOwnership(pendingAction, tenantId, userId, pendingAction.ConversationId);
        }
        catch (BusinessException ex)
        {
            return Fail(ex.Code, ex.Code ?? "ShopManagement:AiPendingActionNotFound");
        }

        if (pendingAction.Status != ShopAiPendingActionStatus.ReadyForConfirmation)
        {
            return Fail("AiPendingActionNotFound", "ShopManagement:AiPendingActionNotFound");
        }

        var messageQuery = await _messageRepository.GetQueryableAsync();
        var message = await _asyncExecuter.FirstOrDefaultAsync(messageQuery.Where(x => x.Id == pendingAction.SourceMessageId && x.TenantId == tenantId));
        if (message == null || message.Status != ShopAiMessageStatus.AwaitingConfirmation || message.DetectedAction == null || message.ActionPayloadJson == null)
        {
            return Fail("AiMessageNotAwaitingConfirmation", "ShopManagement:AiMessageNotAwaitingConfirmation");
        }

        var verify = _confirmationService.Verify(
            confirmationToken, message.ConfirmationTokenHash ?? string.Empty, tenantId, userId,
            pendingAction.ConversationId, message.Id, message.DetectedAction.Value, message.ActionPayloadJson, message.ConfirmationExpiryDate);

        if (!verify.Success)
        {
            if (verify.ErrorCode == "AiConfirmationExpired")
            {
                _messageManager.MarkFailed(message, verify.ErrorCode, verify.ErrorMessage ?? "ShopManagement:AiConfirmationExpired");
                await _messageRepository.UpdateAsync(message, autoSave: true);
                _pendingActionManager.MarkExpired(pendingAction);
                await _pendingActionRepository.UpdateAsync(pendingAction, autoSave: true);
            }
            return Fail(verify.ErrorCode, verify.ErrorMessage ?? "ShopManagement:AiConfirmationInvalid");
        }

        if (!_handlerRegistry.TryGetHandler(message.DetectedAction.Value, out var handler) || handler == null)
        {
            return Fail("AiActionNotSupported", "ShopManagement:AiActionNotSupported");
        }

        var reconstructedParameters = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, JsonElement>>(message.ActionPayloadJson) ?? new();
        var reconstructed = new ShopAiParsedCommand
        {
            Action = message.DetectedAction.Value,
            Language = message.DetectedLanguage,
            RequiresConfirmation = true,
            Confidence = 1m,
            Parameters = reconstructedParameters,
        };

        ShopAiExecutionResultDto result;
        try
        {
            // Recheck permissions and business rules against the CURRENT state of the data, not
            // whatever was true at prepare time - the same defense-in-depth the chat confirmation
            // flow already applies.
            await handler.PrepareAsync(reconstructed, cancellationToken);

            _messageManager.MarkExecuting(message);
            await _messageRepository.UpdateAsync(message, autoSave: true);
            _pendingActionManager.MarkExecuting(pendingAction);
            await _pendingActionRepository.UpdateAsync(pendingAction, autoSave: true);

            var validatedAction = new ShopAiValidatedAction
            {
                TenantId = tenantId,
                UserId = userId,
                ConversationId = pendingAction.ConversationId,
                MessageId = message.Id,
                Action = message.DetectedAction.Value,
                Language = message.DetectedLanguage,
                PayloadJson = JsonSerializer.SerializeToElement(reconstructed.Parameters),
            };
            result = await handler.ExecuteAsync(validatedAction, cancellationToken);
        }
        catch (ShopAiMissingInformationException)
        {
            result = new ShopAiExecutionResultDto { Success = false, ErrorCode = "AiMissingInformation", ErrorMessage = "ShopManagement:AiMissingInformation" };
        }
        catch (BusinessException ex)
        {
            result = new ShopAiExecutionResultDto { Success = false, ErrorCode = ex.Code, ErrorMessage = ShopAiExceptionFormatter.Format(_localizer, ex) };
        }

        var now = _clock.Now;
        if (result.Success)
        {
            _messageManager.MarkExecuted(message, JsonSerializer.Serialize(result), now);
            _pendingActionManager.MarkExecuted(pendingAction);
        }
        else
        {
            _messageManager.MarkFailed(message, result.ErrorCode, result.ErrorMessage ?? "ShopManagement:AiServiceUnavailable");
            _pendingActionManager.MarkFailed(pendingAction);
        }
        await _messageRepository.UpdateAsync(message, autoSave: true);
        await _pendingActionRepository.UpdateAsync(pendingAction, autoSave: true);

        var audit = _auditManager.Record(
            userId, pendingAction.ConversationId, message.Id, message.DetectedAction.Value.ToString(), message.ActionPayloadJson ?? "{}",
            confirmationRequired: true, confirmedByUserId: userId, confirmationDate: now,
            executionStatus: result.Success ? ShopAiActionExecutionStatus.Succeeded : ShopAiActionExecutionStatus.Failed,
            resultReferenceType: result.ResultReferenceType, resultReferenceId: result.ResultReferenceId,
            errorCode: result.ErrorCode, errorMessage: result.ErrorMessage);
        await _auditRepository.InsertAsync(audit, autoSave: true);

        return result;
    }

    private static ShopAiParsedCommand BuildCommand(ShopAiActionType actionType, JsonElement payload)
    {
        var parameters = new System.Collections.Generic.Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        if (payload.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in payload.EnumerateObject())
            {
                parameters[property.Name] = property.Value;
            }
        }

        return new ShopAiParsedCommand
        {
            Action = actionType,
            Intent = ShopAiIntentType.StartRecordCreation,
            Language = ShopAiLanguage.English,
            RequiresConfirmation = true,
            Confidence = 1m,
            Parameters = parameters,
        };
    }

    private (Guid tenantId, Guid userId) RequireIdentity()
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
        var userId = _currentUser.Id ?? throw new AbpAuthorizationException("ShopManagement:AiPermissionDenied");
        return (tenantId, userId);
    }

    private static ShopAiExecutionResultDto Fail(string? errorCode, string errorMessage) =>
        new() { Success = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EHub.Localization;
using EHub.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.AiAssistant;

[Authorize(EHubPermissions.ShopAiAssistant.Use)]
public class ShopAiAssistantAppService : ApplicationService, IShopAiAssistantAppService
{
    private static readonly Regex HtmlTagRegex = new("<[^>]*>", RegexOptions.None, TimeSpan.FromSeconds(1));

    private readonly IRepository<ShopAiConversation, Guid> _conversationRepository;
    private readonly IRepository<ShopAiMessage, Guid> _messageRepository;
    private readonly IRepository<ShopAiActionAudit, Guid> _auditRepository;
    private readonly IRepository<ShopAiPendingAction, Guid> _pendingActionRepository;
    private readonly ShopAiConversationManager _conversationManager;
    private readonly ShopAiMessageManager _messageManager;
    private readonly ShopAiActionAuditManager _auditManager;
    private readonly ShopAiPendingActionManager _pendingActionManager;
    private readonly IShopAiCommandParser _commandParser;
    private readonly IShopAiActionHandlerRegistry _handlerRegistry;
    private readonly IShopAiModuleMetadataProvider _moduleMetadataProvider;
    private readonly IShopAiSlotFillingService _slotFillingService;
    private readonly IShopAiConfirmationService _confirmationService;
    private readonly IShopAiRateLimiter _rateLimiter;
    private readonly IStringLocalizer<EHubResource> _localizer;

    public ShopAiAssistantAppService(
        IRepository<ShopAiConversation, Guid> conversationRepository,
        IRepository<ShopAiMessage, Guid> messageRepository,
        IRepository<ShopAiActionAudit, Guid> auditRepository,
        IRepository<ShopAiPendingAction, Guid> pendingActionRepository,
        ShopAiConversationManager conversationManager,
        ShopAiMessageManager messageManager,
        ShopAiActionAuditManager auditManager,
        ShopAiPendingActionManager pendingActionManager,
        IShopAiCommandParser commandParser,
        IShopAiActionHandlerRegistry handlerRegistry,
        IShopAiModuleMetadataProvider moduleMetadataProvider,
        IShopAiSlotFillingService slotFillingService,
        IShopAiConfirmationService confirmationService,
        IShopAiRateLimiter rateLimiter,
        IStringLocalizer<EHubResource> localizer)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _auditRepository = auditRepository;
        _pendingActionRepository = pendingActionRepository;
        _conversationManager = conversationManager;
        _messageManager = messageManager;
        _auditManager = auditManager;
        _pendingActionManager = pendingActionManager;
        _commandParser = commandParser;
        _handlerRegistry = handlerRegistry;
        _moduleMetadataProvider = moduleMetadataProvider;
        _slotFillingService = slotFillingService;
        _confirmationService = confirmationService;
        _rateLimiter = rateLimiter;
        _localizer = localizer;
    }

    public async Task<ShopAiConversationDto> CreateConversationAsync()
    {
        var userId = RequireUser();
        var conversation = _conversationManager.Create(userId, title: null);
        await _conversationRepository.InsertAsync(conversation, autoSave: true);
        return MapConversation(conversation, new List<ShopAiMessageDto>());
    }

    [Authorize(EHubPermissions.ShopAiAssistant.ViewHistory)]
    public async Task<PagedResultDto<ShopAiConversationListDto>> GetConversationsAsync(GetShopAiConversationsInput input)
    {
        var tenantId = RequireTenant();
        var userId = RequireUser();

        var query = await _conversationRepository.GetQueryableAsync();
        query = query.Where(x => x.TenantId == tenantId && x.UserId == userId)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Title != null && x.Title.Contains(input.Filter!));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "LastMessageDate desc, CreationTime desc" : input.Sorting!;
        var entities = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).PageBy(input));
        var items = ObjectMapper.Map<List<ShopAiConversation>, List<ShopAiConversationListDto>>(entities);
        return new PagedResultDto<ShopAiConversationListDto>(totalCount, items);
    }

    [Authorize(EHubPermissions.ShopAiAssistant.ViewHistory)]
    public async Task<ShopAiConversationDto> GetConversationAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var userId = RequireUser();
        var conversation = await FindConversationAsync(id, tenantId, userId);

        var messageQuery = (await _messageRepository.GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.ConversationId == conversation.Id)
            .OrderBy(x => x.CreationTime);
        var messages = await AsyncExecuter.ToListAsync(messageQuery);

        return MapConversation(conversation, messages.Select(MapMessage).ToList());
    }

    public Task<ShopAiResponseDto> SendMessageAsync(SendShopAiMessageDto input)
    {
        if (!_rateLimiter.TryAcquire(RequireTenant(), RequireUser(), ShopAiRateLimitCategory.Text))
        {
            throw new BusinessException("ShopManagement:AiRateLimitExceeded");
        }

        return SendUserTextAsync(input.ConversationId, input.Message, originalTranscription: null);
    }

    /// <summary>
    /// Shared by SendMessageAsync (typed text) and ShopAiVoiceAppService (accepted/edited
    /// transcription) - both end up as an ordinary user message. If the conversation has an active
    /// (not yet complete) guided creation in progress, this message is treated as the next answer
    /// in that flow and never goes through intent classification at all; otherwise it goes through
    /// the full parse -> intent routing pipeline.
    /// </summary>
    internal async Task<ShopAiResponseDto> SendUserTextAsync(Guid conversationId, string rawText, string? originalTranscription)
    {
        var tenantId = RequireTenant();
        var userId = RequireUser();

        var conversation = await FindConversationAsync(conversationId, tenantId, userId);
        var text = SanitizeInput(rawText);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new BusinessException("ShopManagement:AiMissingInformation");
        }

        var userMessage = _messageManager.Create(conversation, userId, ShopAiMessageRole.User, text, originalTranscription, ShopAiLanguage.Unknown);
        await _messageRepository.InsertAsync(userMessage, autoSave: true);

        if (conversation.Title == null)
        {
            _conversationManager.SetTitle(conversation, text.Length > 60 ? text.Substring(0, 60) : text);
        }

        var activePendingAction = await FindActivePendingActionAsync(conversation.Id, tenantId, userId);
        var response = activePendingAction != null
            ? await ContinuePendingActionAsync(conversation, userId, activePendingAction, text)
            : await ProcessUserTextAsync(conversation, userId, text);

        _conversationManager.TouchLastMessage(conversation, Clock.Now, response.DetectedLanguage);
        await _conversationRepository.UpdateAsync(conversation, autoSave: true);

        return response;
    }

    public async Task<ShopAiExecutionResultDto> ConfirmActionAsync(ConfirmShopAiActionDto input)
    {
        var tenantId = RequireTenant();
        var userId = RequireUser();

        if (!_rateLimiter.TryAcquire(tenantId, userId, ShopAiRateLimitCategory.Confirmation))
        {
            throw new BusinessException("ShopManagement:AiRateLimitExceeded");
        }

        var conversation = await FindConversationAsync(input.ConversationId, tenantId, userId);
        var message = await FindMessageAsync(input.MessageId, conversation, tenantId, userId);

        if (message.Status != ShopAiMessageStatus.AwaitingConfirmation || message.DetectedAction == null || message.ActionPayloadJson == null)
        {
            throw new BusinessException("ShopManagement:AiMessageNotAwaitingConfirmation");
        }

        var verify = _confirmationService.Verify(
            input.ConfirmationToken, message.ConfirmationTokenHash ?? string.Empty, tenantId, userId,
            conversation.Id, message.Id, message.DetectedAction.Value, message.ActionPayloadJson, message.ConfirmationExpiryDate);

        if (!verify.Success)
        {
            if (verify.ErrorCode == "AiConfirmationExpired")
            {
                _messageManager.MarkFailed(message, verify.ErrorCode, verify.ErrorMessage ?? "ShopManagement:AiConfirmationExpired");
                await _messageRepository.UpdateAsync(message, autoSave: true);
            }

            throw new BusinessException(verify.ErrorMessage ?? "ShopManagement:AiConfirmationInvalid");
        }

        if (!_handlerRegistry.TryGetHandler(message.DetectedAction.Value, out var handler) || handler == null)
        {
            _messageManager.MarkFailed(message, "AiActionNotSupported", "ShopManagement:AiActionNotSupported");
            await _messageRepository.UpdateAsync(message, autoSave: true);
            throw new BusinessException("ShopManagement:AiActionNotSupported");
        }

        // "Recheck permissions. Revalidate business data." - re-run the exact same PrepareAsync
        // path used at parse/collection time, against the stored parameters, before ever touching
        // ExecuteAsync. If anything about the request or the surrounding data changed since
        // preview, this throws and the action never executes.
        var reconstructed = new ShopAiParsedCommand
        {
            Action = message.DetectedAction.Value,
            Language = message.DetectedLanguage,
            RequiresConfirmation = true,
            Confidence = 1m,
            Parameters = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message.ActionPayloadJson) ?? new(),
        };

        ShopAiExecutionResultDto result;
        try
        {
            await handler.PrepareAsync(reconstructed);

            _messageManager.MarkExecuting(message);
            await _messageRepository.UpdateAsync(message, autoSave: true);

            var validatedAction = new ShopAiValidatedAction
            {
                TenantId = tenantId,
                UserId = userId,
                ConversationId = conversation.Id,
                MessageId = message.Id,
                Action = message.DetectedAction.Value,
                Language = message.DetectedLanguage,
                PayloadJson = JsonSerializer.SerializeToElement(reconstructed.Parameters),
            };

            result = await handler.ExecuteAsync(validatedAction);
        }
        catch (ShopAiMissingInformationException)
        {
            result = new ShopAiExecutionResultDto { Success = false, ErrorCode = "AiMissingInformation", ErrorMessage = "ShopManagement:AiMissingInformation" };
        }
        catch (ShopAiAmbiguousMatchException)
        {
            result = new ShopAiExecutionResultDto { Success = false, ErrorCode = "AiLookupAmbiguous", ErrorMessage = "ShopManagement:AiLookupAmbiguous" };
        }
        catch (BusinessException ex)
        {
            // ex.Data carries substitution values (e.g. WithData("Name", "Ai")) that a raw ex.Code
            // string would lose - ShopAiExceptionFormatter resolves the localized template AND
            // substitutes them here, this is the one place that context is still available.
            result = new ShopAiExecutionResultDto { Success = false, ErrorCode = ex.Code, ErrorMessage = ShopAiExceptionFormatter.Format(_localizer, ex) };
        }

        var now = Clock.Now;
        if (result.Success)
        {
            _messageManager.MarkExecuted(message, JsonSerializer.Serialize(result), now);
        }
        else
        {
            _messageManager.MarkFailed(message, result.ErrorCode, result.ErrorMessage ?? "ShopManagement:AiServiceUnavailable");
        }
        await _messageRepository.UpdateAsync(message, autoSave: true);

        // A guided (multi-turn) creation that reached ReadyForConfirmation is only ever "active" up
        // to this point - once its confirmation message is actually confirmed, close the pending
        // action row out too, or FindActivePendingActionAsync would keep treating the conversation
        // as "still collecting" for the next message the user sends.
        var pendingActionToClose = await FindActivePendingActionAsync(conversation.Id, tenantId, userId);
        if (pendingActionToClose != null)
        {
            if (result.Success)
            {
                _pendingActionManager.MarkExecuted(pendingActionToClose);
            }
            else
            {
                _pendingActionManager.MarkFailed(pendingActionToClose);
            }
            await _pendingActionRepository.UpdateAsync(pendingActionToClose, autoSave: true);
        }

        var audit = _auditManager.Record(
            userId, conversation.Id, message.Id, message.DetectedAction.Value.ToString(), message.ActionPayloadJson ?? "{}",
            confirmationRequired: true, confirmedByUserId: userId, confirmationDate: now,
            executionStatus: result.Success ? ShopAiActionExecutionStatus.Succeeded : ShopAiActionExecutionStatus.Failed,
            resultReferenceType: result.ResultReferenceType, resultReferenceId: result.ResultReferenceId,
            errorCode: result.ErrorCode, errorMessage: result.ErrorMessage);
        await _auditRepository.InsertAsync(audit, autoSave: true);

        _conversationManager.TouchLastMessage(conversation, now, null);
        await _conversationRepository.UpdateAsync(conversation, autoSave: true);

        // Same reasoning as ProcessUserTextAsync: this is a 200-OK DTO field, not a thrown
        // exception, so nothing localizes result.ErrorMessage automatically before it reaches
        // the frontend - do it here. _localizer[...] is a safe no-op for text it doesn't recognize.
        if (!result.Success && result.ErrorMessage != null)
        {
            result.ErrorMessage = _localizer[result.ErrorMessage].Value;
        }

        return result;
    }

    public async Task CancelActionAsync(CancelShopAiActionDto input)
    {
        var tenantId = RequireTenant();
        var userId = RequireUser();

        var conversation = await FindConversationAsync(input.ConversationId, tenantId, userId);
        var message = await FindMessageAsync(input.MessageId, conversation, tenantId, userId);

        if (message.Status == ShopAiMessageStatus.AwaitingConfirmation)
        {
            _messageManager.MarkCancelled(message);
            await _messageRepository.UpdateAsync(message, autoSave: true);
        }
        else if (message.Status != ShopAiMessageStatus.MissingInformation)
        {
            throw new BusinessException("ShopManagement:AiMessageNotAwaitingConfirmation");
        }

        // A guided creation still in progress (or sitting at ReadyForConfirmation) is tracked
        // separately from the message - only one can ever be active per conversation, so cancel it
        // too whenever the user cancels its confirmation/collection turn, so the next free-text
        // message starts fresh instead of being swallowed as "the next answer". Note the pending
        // action's SourceMessageId is the turn that STARTED collection, not necessarily this
        // message, so ownership is established by conversation scope (already validated above),
        // not by comparing message ids.
        var activePendingAction = await FindActivePendingActionAsync(conversation.Id, tenantId, userId);
        if (activePendingAction != null)
        {
            _pendingActionManager.MarkCancelled(activePendingAction);
            await _pendingActionRepository.UpdateAsync(activePendingAction, autoSave: true);
        }

        var audit = _auditManager.Record(
            userId, conversation.Id, message.Id, message.DetectedAction?.ToString() ?? "Unknown", message.ActionPayloadJson ?? "{}",
            confirmationRequired: true, confirmedByUserId: null, confirmationDate: null,
            executionStatus: ShopAiActionExecutionStatus.Rejected, resultReferenceType: null, resultReferenceId: null,
            errorCode: null, errorMessage: null);
        await _auditRepository.InsertAsync(audit, autoSave: true);
    }

    public async Task DeleteConversationAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var userId = RequireUser();
        var conversation = await FindConversationAsync(id, tenantId, userId);

        // Soft-delete the conversation only - its messages and any action audit rows are left in
        // place so "Conversation and action audit history" survives a user clearing their own list.
        await _conversationRepository.DeleteAsync(conversation, autoSave: true);
    }

    [Authorize(EHubPermissions.ShopAiAssistant.ProjectHelp)]
    public Task<ListResultDto<ShopAiModuleListDto>> GetSupportedModulesAsync()
    {
        var items = _moduleMetadataProvider.GetModules()
            .Select(m => new ShopAiModuleListDto { ModuleKey = m.ModuleKey, DisplayName = m.DisplayName, Description = m.Description, SupportsCreation = m.SupportsCreation })
            .OrderBy(m => m.DisplayName)
            .ToList();
        return Task.FromResult(new ListResultDto<ShopAiModuleListDto>(items));
    }

    [Authorize(EHubPermissions.ShopAiAssistant.ProjectHelp)]
    public Task<ShopAiModuleExplanationDto> GetModuleHelpAsync(string moduleKey)
    {
        if (!_moduleMetadataProvider.TryGetModule(moduleKey, out var module) || module == null)
        {
            throw new BusinessException("ShopManagement:AiModuleNotFound");
        }

        // Direct REST call outside a chat conversation - there is no detected message language to
        // key off, so this always returns the English/default text (GetDescription's fallback).
        return Task.FromResult(BuildModuleExplanation(module, ShopAiLanguage.Unknown));
    }

    // ------------------------------------------------------------------
    // Internal orchestration - fresh message (no guided creation in progress)
    // ------------------------------------------------------------------

    internal async Task<ShopAiResponseDto> ProcessUserTextAsync(ShopAiConversation conversation, Guid userId, string text)
    {
        var tenantId = conversation.TenantId!.Value;

        // The assistant's own reply never has an OriginalTranscription - that belongs to the user's
        // turn, which the caller already stored on the user message before calling this method.
        var assistantMessage = _messageManager.Create(conversation, userId, ShopAiMessageRole.Assistant, "…", originalTranscription: null, ShopAiLanguage.Unknown);
        await _messageRepository.InsertAsync(assistantMessage, autoSave: true);

        var parseResult = await _commandParser.ParseAsync(text);
        if (!parseResult.Success || parseResult.Command == null)
        {
            var parseErrorKey = parseResult.ErrorMessage ?? "ShopManagement:AiServiceUnavailable";
            _messageManager.MarkFailed(assistantMessage, parseResult.ErrorCode, parseErrorKey);
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);

            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.Failed,
                Action = ShopAiActionType.Unknown,
                ResponseType = ShopAiResponseType.Error,
                // AssistantMessage/ErrorMessage must be resolved server-side: this DTO is a plain
                // 200-OK response, not a thrown exception, so ABP's automatic exception-message
                // localization never touches it - unlike errors from Confirm/CancelActionAsync,
                // which the frontend reads pre-localized off a thrown BusinessException.
                AssistantMessage = _localizer[parseErrorKey],
                ErrorCode = parseResult.ErrorCode,
                ErrorMessage = _localizer[parseErrorKey],
            };
        }

        var command = parseResult.Command;
        var payloadJson = ShopAiPayloadSerializer.SerializeParameters(command.Parameters);

        if (command.Action == ShopAiActionType.MissingInformation)
        {
            _messageManager.MarkMissingInformation(assistantMessage, payloadJson, command.Language);
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);

            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.MissingInformation,
                Action = ShopAiActionType.MissingInformation,
                ResponseType = ShopAiResponseType.MissingInformation,
                DetectedLanguage = command.Language,
                // Ollama returns this both for "known action, missing a required field" and for
                // "request doesn't match any supported action/module at all" - it doesn't always
                // populate userFriendlyMessage for the latter case, so the bubble must never rely
                // on that alone.
                AssistantMessage = command.UserFriendlyMessage ?? _localizer["ShopManagement:AiMissingInformation"].Value,
                MissingFields = command.MissingFields,
                Warnings = command.Warnings,
            };
        }

        switch (command.Intent)
        {
            case ShopAiIntentType.ExplainModule:
            case ShopAiIntentType.ListModuleFields:
            case ShopAiIntentType.ExplainBusinessRule:
                return await BuildModuleKnowledgeResponseAsync(conversation, assistantMessage, command, payloadJson);

            case ShopAiIntentType.ExplainField:
                return await BuildFieldExplanationResponseAsync(conversation, assistantMessage, command, payloadJson);

            case ShopAiIntentType.StartRecordCreation:
                return await StartGuidedCreationInternalAsync(conversation, userId, assistantMessage, command, payloadJson);

            case ShopAiIntentType.ReadBusinessData:
                return await ProcessReadActionAsync(conversation, userId, tenantId, assistantMessage, command, payloadJson);

            default:
                // GeneralHelp / Unknown / ContinueRecordCreation-with-nothing-active / etc.
                _messageManager.MarkParsed(assistantMessage, command.Action, payloadJson, command.Language);
                var helpResult = new ShopAiExecutionResultDto { Success = true, ResultMessage = command.UserFriendlyMessage ?? "I can help with shop questions and a few supported actions - try asking about today's sales, adding a customer, or adding a unit." };
                _messageManager.MarkExecuted(assistantMessage, JsonSerializer.Serialize(helpResult), Clock.Now);
                await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);

                return new ShopAiResponseDto
                {
                    ConversationId = conversation.Id,
                    MessageId = assistantMessage.Id,
                    Status = ShopAiMessageStatus.Executed,
                    Action = command.Action,
                    ResponseType = ShopAiResponseType.TextAnswer,
                    DetectedLanguage = command.Language,
                    AssistantMessage = helpResult.ResultMessage,
                    ExecutionResult = helpResult,
                };
        }
    }

    /// <summary>The pre-existing read-only path (GetTodaySales, etc.) - unchanged behavior, just reached via Intent == ReadBusinessData instead of being the default branch.</summary>
    private async Task<ShopAiResponseDto> ProcessReadActionAsync(ShopAiConversation conversation, Guid userId, Guid tenantId, ShopAiMessage assistantMessage, ShopAiParsedCommand command, string payloadJson)
    {
        if (!_handlerRegistry.TryGetHandler(command.Action, out var handler) || handler == null)
        {
            _messageManager.MarkFailed(assistantMessage, "AiActionNotSupported", "ShopManagement:AiActionNotSupported");
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);

            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.Failed,
                Action = command.Action,
                ResponseType = ShopAiResponseType.Error,
                AssistantMessage = _localizer["ShopManagement:AiActionNotSupported"],
                ErrorCode = "AiActionNotSupported",
                ErrorMessage = _localizer["ShopManagement:AiActionNotSupported"],
            };
        }

        ShopAiActionPreviewDto preview;
        try
        {
            preview = await handler.PrepareAsync(command);
        }
        catch (ShopAiMissingInformationException ex)
        {
            _messageManager.MarkMissingInformation(assistantMessage, payloadJson, command.Language);
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);
            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.MissingInformation,
                Action = command.Action,
                ResponseType = ShopAiResponseType.MissingInformation,
                DetectedLanguage = command.Language,
                AssistantMessage = ex.UserFriendlyMessage ?? command.UserFriendlyMessage ?? _localizer["ShopManagement:AiMissingInformation"].Value,
                MissingFields = ex.MissingFields,
                Warnings = command.Warnings,
            };
        }
        catch (ShopAiAmbiguousMatchException ex)
        {
            _messageManager.MarkMissingInformation(assistantMessage, payloadJson, command.Language);
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);
            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.MissingInformation,
                Action = command.Action,
                ResponseType = ShopAiResponseType.LookupChoices,
                DetectedLanguage = command.Language,
                AssistantMessage = command.UserFriendlyMessage ?? _localizer["ShopManagement:AiLookupAmbiguous"].Value,
                AmbiguousLookups = ex.AmbiguousLookups,
            };
        }
        catch (BusinessException ex)
        {
            var formatted = ShopAiExceptionFormatter.Format(_localizer, ex);
            _messageManager.MarkFailed(assistantMessage, ex.Code, ex.Code ?? "ShopManagement:AiServiceUnavailable");
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);
            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.Failed,
                Action = command.Action,
                ResponseType = ShopAiResponseType.Error,
                AssistantMessage = formatted,
                ErrorCode = ex.Code,
                ErrorMessage = formatted,
            };
        }

        _messageManager.MarkParsed(assistantMessage, command.Action, payloadJson, command.Language);

        if (handler.IsWriteAction)
        {
            return await IssueConfirmationAsync(conversation, userId, assistantMessage, command, payloadJson, preview);
        }

        // Read action - executes immediately, no confirmation involved.
        _messageManager.MarkExecuting(assistantMessage);
        await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);

        var validated = new ShopAiValidatedAction
        {
            TenantId = tenantId,
            UserId = userId,
            ConversationId = conversation.Id,
            MessageId = assistantMessage.Id,
            Action = command.Action,
            Language = command.Language,
            PayloadJson = JsonSerializer.SerializeToElement(command.Parameters),
        };

        ShopAiExecutionResultDto executionResult;
        try
        {
            executionResult = await handler.ExecuteAsync(validated);
        }
        catch (BusinessException ex)
        {
            executionResult = new ShopAiExecutionResultDto { Success = false, ErrorCode = ex.Code, ErrorMessage = ShopAiExceptionFormatter.Format(_localizer, ex) };
        }

        var now = Clock.Now;
        if (executionResult.Success)
        {
            _messageManager.MarkExecuted(assistantMessage, JsonSerializer.Serialize(executionResult), now);
        }
        else
        {
            _messageManager.MarkFailed(assistantMessage, executionResult.ErrorCode, executionResult.ErrorMessage ?? "ShopManagement:AiServiceUnavailable");
        }
        await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);

        var readAudit = _auditManager.Record(
            userId, conversation.Id, assistantMessage.Id, command.Action.ToString(), payloadJson,
            confirmationRequired: false, confirmedByUserId: null, confirmationDate: null,
            executionStatus: executionResult.Success ? ShopAiActionExecutionStatus.Succeeded : ShopAiActionExecutionStatus.Failed,
            resultReferenceType: executionResult.ResultReferenceType, resultReferenceId: executionResult.ResultReferenceId,
            errorCode: executionResult.ErrorCode, errorMessage: executionResult.ErrorMessage);
        await _auditRepository.InsertAsync(readAudit, autoSave: true);

        // executionResult.ErrorMessage may already be a fully-formatted, data-substituted string
        // (from ShopAiExceptionFormatter above) or a plain resource key from a handler that returns
        // ErrorMessage = "Some:Key" directly (e.g. GetTodaySalesAiHandler's own failure paths).
        // _localizer[...] on text it doesn't recognize as a key just returns that same text back,
        // so it is always safe to run both kinds through it.
        var localizedErrorMessage = executionResult.Success || executionResult.ErrorMessage == null
            ? null
            : _localizer[executionResult.ErrorMessage].Value;

        var responseType = !executionResult.Success
            ? ShopAiResponseType.Error
            : executionResult.DataList != null
                ? ShopAiResponseType.DataList
                : ShopAiResponseType.ExecutionResult;

        return new ShopAiResponseDto
        {
            ConversationId = conversation.Id,
            MessageId = assistantMessage.Id,
            Status = executionResult.Success ? ShopAiMessageStatus.Executed : ShopAiMessageStatus.Failed,
            Action = command.Action,
            ResponseType = responseType,
            DetectedLanguage = command.Language,
            AssistantMessage = executionResult.ResultMessage ?? command.UserFriendlyMessage ?? localizedErrorMessage ?? _localizer["ShopManagement:AiServiceUnavailable"].Value,
            Warnings = command.Warnings,
            ExecutionResult = executionResult,
            DataList = executionResult.DataList,
            ErrorCode = executionResult.Success ? null : executionResult.ErrorCode,
            ErrorMessage = localizedErrorMessage,
        };
    }

    // ------------------------------------------------------------------
    // Project knowledge (ExplainModule / ListModuleFields / ExplainField / ExplainBusinessRule)
    // ------------------------------------------------------------------

    private async Task<ShopAiResponseDto> BuildModuleKnowledgeResponseAsync(ShopAiConversation conversation, ShopAiMessage assistantMessage, ShopAiParsedCommand command, string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(command.ModuleKey) || !_moduleMetadataProvider.TryGetModule(command.ModuleKey, out var module) || module == null)
        {
            return await RespondModuleNotFoundAsync(conversation, assistantMessage, command, payloadJson);
        }

        await CheckModuleReadPermissionAsync(module);

        // AssistantMessage is deliberately a SHORT lead-in only - the full field-by-field detail
        // lives exclusively in the structured Module/Fields DTOs below. Earlier this method dumped
        // the entire explanation into AssistantMessage too, so the chat bubble's plain text and the
        // module card rendered the exact same content twice; that duplication is the root cause
        // this fixes (see the frontend's now-mutually-exclusive card rendering for the other half).
        var explanation = BuildModuleExplanation(module, command.Language);
        var text = ShopAiPhrases.ModuleShortIntro(command.Language, module.DisplayName);

        _messageManager.MarkParsed(assistantMessage, ShopAiActionType.Unknown, payloadJson, command.Language);
        _messageManager.MarkExecuted(assistantMessage, JsonSerializer.Serialize(new ShopAiExecutionResultDto { Success = true, ResultMessage = text }), Clock.Now);
        await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);

        return new ShopAiResponseDto
        {
            ConversationId = conversation.Id,
            MessageId = assistantMessage.Id,
            Status = ShopAiMessageStatus.Executed,
            Action = ShopAiActionType.Unknown,
            ResponseType = command.Intent == ShopAiIntentType.ListModuleFields ? ShopAiResponseType.FieldList : ShopAiResponseType.ModuleExplanation,
            DetectedLanguage = command.Language,
            ModuleKey = module.ModuleKey,
            AssistantMessage = text,
            Module = explanation,
            Fields = explanation.RequiredFields.Concat(explanation.OptionalFields).ToList(),
        };
    }

    private async Task<ShopAiResponseDto> BuildFieldExplanationResponseAsync(ShopAiConversation conversation, ShopAiMessage assistantMessage, ShopAiParsedCommand command, string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(command.ModuleKey) || !_moduleMetadataProvider.TryGetModule(command.ModuleKey, out var module) || module == null)
        {
            return await RespondModuleNotFoundAsync(conversation, assistantMessage, command, payloadJson);
        }

        await CheckModuleReadPermissionAsync(module);

        var field = string.IsNullOrWhiteSpace(command.FieldKey)
            ? null
            : module.Fields.FirstOrDefault(f => string.Equals(f.FieldKey, command.FieldKey, StringComparison.OrdinalIgnoreCase)
                                                 || f.Aliases.Any(a => string.Equals(a, command.FieldKey, StringComparison.OrdinalIgnoreCase)));

        if (field == null)
        {
            _messageManager.MarkFailed(assistantMessage, "AiFieldNotFound", "ShopManagement:AiFieldNotFound");
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);
            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.Failed,
                ResponseType = ShopAiResponseType.Error,
                ModuleKey = module.ModuleKey,
                DetectedLanguage = command.Language,
                AssistantMessage = _localizer["ShopManagement:AiFieldNotFound"],
                ErrorCode = "AiFieldNotFound",
                ErrorMessage = _localizer["ShopManagement:AiFieldNotFound"],
            };
        }

        var dto = ToFieldDto(field, command.Language);
        var text = ShopAiPhrases.FieldShortIntro(command.Language, field.DisplayName);

        _messageManager.MarkParsed(assistantMessage, ShopAiActionType.Unknown, payloadJson, command.Language);
        _messageManager.MarkExecuted(assistantMessage, JsonSerializer.Serialize(new ShopAiExecutionResultDto { Success = true, ResultMessage = text }), Clock.Now);
        await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);

        return new ShopAiResponseDto
        {
            ConversationId = conversation.Id,
            MessageId = assistantMessage.Id,
            Status = ShopAiMessageStatus.Executed,
            ResponseType = ShopAiResponseType.FieldExplanation,
            ModuleKey = module.ModuleKey,
            DetectedLanguage = command.Language,
            AssistantMessage = text,
            Fields = new List<ShopAiFieldDescriptionDto> { dto },
        };
    }

    private async Task<ShopAiResponseDto> RespondModuleNotFoundAsync(ShopAiConversation conversation, ShopAiMessage assistantMessage, ShopAiParsedCommand command, string payloadJson)
    {
        _messageManager.MarkFailed(assistantMessage, "AiModuleNotFound", "ShopManagement:AiModuleNotFound");
        await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);
        return new ShopAiResponseDto
        {
            ConversationId = conversation.Id,
            MessageId = assistantMessage.Id,
            Status = ShopAiMessageStatus.Failed,
            ResponseType = ShopAiResponseType.Error,
            DetectedLanguage = command.Language,
            AssistantMessage = command.UserFriendlyMessage ?? _localizer["ShopManagement:AiModuleNotFound"].Value,
            ErrorCode = "AiModuleNotFound",
            ErrorMessage = _localizer["ShopManagement:AiModuleNotFound"],
        };
    }

    private async Task CheckModuleReadPermissionAsync(ShopAiModuleMetadata module)
    {
        if (!await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopAiAssistant.ProjectHelp))
        {
            throw new AbpAuthorizationException("ShopManagement:AiPermissionDenied");
        }
    }

    private ShopAiModuleExplanationDto BuildModuleExplanation(ShopAiModuleMetadata module, ShopAiLanguage language) => new()
    {
        ModuleKey = module.ModuleKey,
        DisplayName = module.DisplayName,
        Description = module.GetDescription(language),
        SupportsCreation = module.SupportsCreation,
        CreatesDraftOnly = module.CreatesDraftOnly,
        RequiredFields = module.Fields.Where(f => f.IsRequired && !f.IsSystemGenerated).Select(f => ToFieldDto(f, language)).ToList(),
        OptionalFields = module.Fields.Where(f => !f.IsRequired && !f.IsSystemGenerated && !f.IsCalculated).Select(f => ToFieldDto(f, language)).ToList(),
        SystemGeneratedFields = module.Fields.Where(f => f.IsSystemGenerated || f.IsCalculated).Select(f => ToFieldDto(f, language)).ToList(),
        BusinessRules = module.BusinessRules,
        RelatedModules = module.RelatedModules,
    };

    private static ShopAiFieldDescriptionDto ToFieldDto(ShopAiFieldMetadata field, ShopAiLanguage language) => new()
    {
        FieldKey = field.FieldKey,
        DisplayName = field.DisplayName,
        Description = field.GetDescription(language),
        DataType = field.DataType,
        IsRequired = field.IsRequired,
        IsLookup = field.IsLookup,
        IsSystemGenerated = field.IsSystemGenerated,
        ExampleValue = field.ExampleValue,
        AllowedValues = field.AllowedValues,
    };

    // ------------------------------------------------------------------
    // Guided creation (StartRecordCreation / continuation)
    // ------------------------------------------------------------------

    private async Task<ShopAiResponseDto> StartGuidedCreationInternalAsync(ShopAiConversation conversation, Guid userId, ShopAiMessage assistantMessage, ShopAiParsedCommand command, string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(command.ModuleKey) || !_moduleMetadataProvider.TryGetModule(command.ModuleKey, out var module) || module == null || !module.SupportsCreation)
        {
            return await RespondModuleNotFoundAsync(conversation, assistantMessage, command, payloadJson);
        }

        if (!_handlerRegistry.TryGetHandler(module.CreateAction, out _))
        {
            // TEMPORARY diagnostic - remove once the intent-misclassification investigation is done.
            Logger.LogWarning("StartGuidedCreationInternalAsync: no handler registered for {CreateAction} (module {ModuleKey}) - falling back to ExplainModule", module.CreateAction, module.ModuleKey);
            // Metadata says creation is supported in principle, but no handler is wired up for it
            // yet in this deployment - explain the module instead of silently failing.
            var explainInstead = new ShopAiParsedCommand
            {
                Action = command.Action,
                Intent = ShopAiIntentType.ExplainModule,
                ModuleKey = command.ModuleKey,
                FieldKey = command.FieldKey,
                Language = command.Language,
                RequiresConfirmation = command.RequiresConfirmation,
                Confidence = command.Confidence,
                UserFriendlyMessage = command.UserFriendlyMessage,
                Parameters = command.Parameters,
                MissingFields = command.MissingFields,
                Warnings = command.Warnings,
            };
            return await BuildModuleKnowledgeResponseAsync(conversation, assistantMessage, explainInstead, payloadJson);
        }

        await CheckModuleReadPermissionAsync(module);
        if (!await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopAiAssistant.GuidedCreation))
        {
            throw new AbpAuthorizationException("ShopManagement:AiPermissionDenied");
        }

        var result = await _slotFillingService.StartAsync(conversation, userId, assistantMessage.Id, module, command);
        return await HandleSlotFillingResultAsync(conversation, userId, assistantMessage, module, command.Language, result);
    }

    private async Task<ShopAiResponseDto> ContinuePendingActionAsync(ShopAiConversation conversation, Guid userId, ShopAiPendingAction pendingAction, string text)
    {
        var module = _moduleMetadataProvider.GetModule(pendingAction.ModuleKey);
        // Continuation turns skip full command parsing for speed (see ShopAiSlotFillingService),
        // so there is no fresh per-turn language detection - fall back to the conversation's last
        // detected language (kept current by TouchLastMessage on every turn) so the next question
        // still comes back in the language the user has been using.
        var language = conversation.DetectedLanguage;

        var assistantMessage = _messageManager.Create(conversation, userId, ShopAiMessageRole.Assistant, "…", originalTranscription: null, ShopAiLanguage.Unknown);
        await _messageRepository.InsertAsync(assistantMessage, autoSave: true);

        var result = await _slotFillingService.ContinueAsync(pendingAction, text, language);

        if (result.IsExpired)
        {
            _messageManager.MarkFailed(assistantMessage, "AiPendingActionNotFound", "ShopManagement:AiPendingActionNotFound");
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);
            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.Failed,
                ResponseType = ShopAiResponseType.Error,
                ModuleKey = module.ModuleKey,
                DetectedLanguage = language,
                AssistantMessage = _localizer["ShopManagement:AiPendingActionNotFound"],
                ErrorCode = "AiPendingActionNotFound",
                ErrorMessage = _localizer["ShopManagement:AiPendingActionNotFound"],
            };
        }

        return await HandleSlotFillingResultAsync(conversation, userId, assistantMessage, module, language, result);
    }

    private async Task<ShopAiResponseDto> HandleSlotFillingResultAsync(ShopAiConversation conversation, Guid userId, ShopAiMessage assistantMessage, ShopAiModuleMetadata module, ShopAiLanguage language, ShopAiSlotFillingResult result)
    {
        if (!result.IsComplete)
        {
            var progress = new ShopAiGuidedCreationProgressDto
            {
                ModuleKey = module.ModuleKey,
                ModuleDisplayName = module.DisplayName,
                CollectedFields = result.CollectedFields,
                MissingRequiredFields = result.MissingRequiredFields,
                RequiredFieldCount = result.RequiredFieldCount,
                CompletedRequiredFieldCount = result.CompletedRequiredFieldCount,
            };

            var missingJson = JsonSerializer.Serialize(result.MissingRequiredFields);
            _messageManager.MarkMissingInformation(assistantMessage, missingJson, language);
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);

            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.MissingInformation,
                Action = module.CreateAction,
                ResponseType = ShopAiResponseType.MissingInformation,
                ModuleKey = module.ModuleKey,
                DetectedLanguage = language,
                AssistantMessage = result.NextQuestion ?? _localizer["ShopManagement:AiMissingInformation"].Value,
                MissingFields = result.MissingRequiredFields,
                GuidedCreationProgress = progress,
            };
        }

        // Complete - hand off to the exact same handler.PrepareAsync -> confirmation-token ->
        // AwaitingConfirmation pipeline a one-shot write uses, via the shared helper below.
        var command = result.CompletedCommand!;
        var payloadJson = ShopAiPayloadSerializer.SerializeParameters(command.Parameters);

        if (!_handlerRegistry.TryGetHandler(module.CreateAction, out var handler) || handler == null)
        {
            _messageManager.MarkFailed(assistantMessage, "AiActionNotSupported", "ShopManagement:AiActionNotSupported");
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);
            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.Failed,
                ResponseType = ShopAiResponseType.Error,
                ModuleKey = module.ModuleKey,
                AssistantMessage = _localizer["ShopManagement:AiActionNotSupported"],
                ErrorCode = "AiActionNotSupported",
                ErrorMessage = _localizer["ShopManagement:AiActionNotSupported"],
            };
        }

        ShopAiActionPreviewDto preview;
        try
        {
            preview = await handler.PrepareAsync(command);
        }
        catch (ShopAiMissingInformationException ex)
        {
            _messageManager.MarkMissingInformation(assistantMessage, payloadJson, language);
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);
            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.MissingInformation,
                Action = module.CreateAction,
                ResponseType = ShopAiResponseType.MissingInformation,
                ModuleKey = module.ModuleKey,
                DetectedLanguage = language,
                AssistantMessage = ex.UserFriendlyMessage ?? _localizer["ShopManagement:AiMissingInformation"].Value,
                MissingFields = ex.MissingFields,
            };
        }
        catch (BusinessException ex)
        {
            var formatted = ShopAiExceptionFormatter.Format(_localizer, ex);
            _messageManager.MarkFailed(assistantMessage, ex.Code, ex.Code ?? "ShopManagement:AiServiceUnavailable");
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);
            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.Failed,
                Action = module.CreateAction,
                ResponseType = ShopAiResponseType.Error,
                ModuleKey = module.ModuleKey,
                AssistantMessage = formatted,
                ErrorCode = ex.Code,
                ErrorMessage = formatted,
            };
        }

        _messageManager.MarkParsed(assistantMessage, module.CreateAction, payloadJson, language);
        var response = await IssueConfirmationAsync(conversation, userId, assistantMessage, command, payloadJson, preview);
        response.ModuleKey = module.ModuleKey;
        return response;
    }

    /// <summary>Shared by the legacy read/write handler path and the guided-creation path - both end a successful PrepareAsync the same way.</summary>
    private async Task<ShopAiResponseDto> IssueConfirmationAsync(ShopAiConversation conversation, Guid userId, ShopAiMessage assistantMessage, ShopAiParsedCommand command, string payloadJson, ShopAiActionPreviewDto preview)
    {
        var tenantId = conversation.TenantId!.Value;
        var token = _confirmationService.Issue(tenantId, userId, conversation.Id, assistantMessage.Id, command.Action, payloadJson);
        _messageManager.SetAwaitingConfirmation(assistantMessage, token.TokenHash, token.ExpiryDate);
        await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);

        preview.ConfirmationToken = token.Token;
        preview.ConfirmationExpiryDate = token.ExpiryDate;

        return new ShopAiResponseDto
        {
            ConversationId = conversation.Id,
            MessageId = assistantMessage.Id,
            Status = ShopAiMessageStatus.AwaitingConfirmation,
            Action = command.Action,
            ResponseType = ShopAiResponseType.ActionPreview,
            DetectedLanguage = command.Language,
            AssistantMessage = command.UserFriendlyMessage ?? _localizer["AiAssistant.ActionPreview"].Value,
            Warnings = command.Warnings,
            Preview = preview,
        };
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private async Task<ShopAiConversation> FindConversationAsync(Guid id, Guid tenantId, Guid userId)
    {
        var query = await _conversationRepository.GetQueryableAsync();
        var conversation = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:AiConversationNotFound");
        _conversationManager.ValidateOwnership(conversation, tenantId, userId);
        return conversation;
    }

    private async Task<ShopAiMessage> FindMessageAsync(Guid id, ShopAiConversation conversation, Guid tenantId, Guid userId)
    {
        var query = await _messageRepository.GetQueryableAsync();
        var message = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:AiMessageNotFound");
        _messageManager.ValidateOwnership(message, tenantId, userId, conversation.Id);
        return message;
    }

    /// <summary>"Only one active pending action per conversation by default" - CollectingInformation/ResolvingLookups/ReadyForConfirmation all count as active.</summary>
    private async Task<ShopAiPendingAction?> FindActivePendingActionAsync(Guid conversationId, Guid tenantId, Guid userId)
    {
        var query = await _pendingActionRepository.GetQueryableAsync();
        var activeStatuses = new[] { ShopAiPendingActionStatus.CollectingInformation, ShopAiPendingActionStatus.ResolvingLookups, ShopAiPendingActionStatus.ReadyForConfirmation };
        return await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.TenantId == tenantId && x.UserId == userId && x.ConversationId == conversationId && activeStatuses.Contains(x.Status))
                .OrderByDescending(x => x.CreationTime));
    }

    private static string SanitizeInput(string text) => HtmlTagRegex.Replace(text, string.Empty).Trim();

    private ShopAiConversationDto MapConversation(ShopAiConversation conversation, List<ShopAiMessageDto> messages) => new()
    {
        Id = conversation.Id,
        Title = conversation.Title,
        DetectedLanguage = conversation.DetectedLanguage,
        Status = conversation.Status,
        LastMessageDate = conversation.LastMessageDate,
        CreationTime = conversation.CreationTime,
        Messages = messages,
    };

    private static ShopAiMessageDto MapMessage(ShopAiMessage message)
    {
        var dto = new ShopAiMessageDto
        {
            Id = message.Id,
            Role = message.Role,
            MessageText = message.MessageText,
            OriginalTranscription = message.OriginalTranscription,
            DetectedLanguage = message.DetectedLanguage,
            DetectedAction = message.DetectedAction,
            Status = message.Status,
            ErrorCode = message.ErrorCode,
            ErrorMessage = message.ErrorMessage,
            ExecutedDate = message.ExecutedDate,
            CreationTime = message.CreationTime,
        };

        if (message.Status == ShopAiMessageStatus.Executed && !string.IsNullOrWhiteSpace(message.ExecutionResultJson))
        {
            try { dto.ExecutionResult = JsonSerializer.Deserialize<ShopAiExecutionResultDto>(message.ExecutionResultJson); }
            catch (JsonException) { /* stored payload predates a shape change - show nothing rather than fail history loading */ }
        }

        return dto;
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
    private Guid RequireUser() => CurrentUser.Id ?? throw new AbpAuthorizationException("ShopManagement:AiPermissionDenied");
}

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
    private readonly ShopAiConversationManager _conversationManager;
    private readonly ShopAiMessageManager _messageManager;
    private readonly ShopAiActionAuditManager _auditManager;
    private readonly IShopAiCommandParser _commandParser;
    private readonly IShopAiActionHandlerRegistry _handlerRegistry;
    private readonly IShopAiConfirmationService _confirmationService;
    private readonly IShopAiRateLimiter _rateLimiter;
    private readonly IStringLocalizer<EHubResource> _localizer;

    public ShopAiAssistantAppService(
        IRepository<ShopAiConversation, Guid> conversationRepository,
        IRepository<ShopAiMessage, Guid> messageRepository,
        IRepository<ShopAiActionAudit, Guid> auditRepository,
        ShopAiConversationManager conversationManager,
        ShopAiMessageManager messageManager,
        ShopAiActionAuditManager auditManager,
        IShopAiCommandParser commandParser,
        IShopAiActionHandlerRegistry handlerRegistry,
        IShopAiConfirmationService confirmationService,
        IShopAiRateLimiter rateLimiter,
        IStringLocalizer<EHubResource> localizer)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _auditRepository = auditRepository;
        _conversationManager = conversationManager;
        _messageManager = messageManager;
        _auditManager = auditManager;
        _commandParser = commandParser;
        _handlerRegistry = handlerRegistry;
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
    /// transcription) - both end up as an ordinary user message that goes through the same
    /// parse -> validate -> preview-or-execute pipeline. Internal: only ShopAiVoiceAppService, in
    /// the same assembly, is meant to call this directly.
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

        var response = await ProcessUserTextAsync(conversation, userId, text);

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
        // path used at parse time, against the stored parameters, before ever touching ExecuteAsync.
        // If anything about the request or the surrounding data changed since preview, this throws
        // and the action never executes.
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
            result = new ShopAiExecutionResultDto { Success = false, ErrorCode = ex.Code, ErrorMessage = ex.Code };
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

        if (message.Status != ShopAiMessageStatus.AwaitingConfirmation)
        {
            throw new BusinessException("ShopManagement:AiMessageNotAwaitingConfirmation");
        }

        _messageManager.MarkCancelled(message);
        await _messageRepository.UpdateAsync(message, autoSave: true);

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

    // ------------------------------------------------------------------
    // Internal orchestration
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
                DetectedLanguage = command.Language,
                AssistantMessage = command.UserFriendlyMessage,
                MissingFields = command.MissingFields,
                Warnings = command.Warnings,
            };
        }

        if (command.Action == ShopAiActionType.GeneralHelp || command.Action == ShopAiActionType.Unknown)
        {
            _messageManager.MarkParsed(assistantMessage, command.Action, payloadJson, command.Language);
            var helpResult = new ShopAiExecutionResultDto { Success = true, ResultMessage = command.UserFriendlyMessage ?? "I can help with shop questions and a few supported actions - try asking about today's sales or adding a customer." };
            _messageManager.MarkExecuted(assistantMessage, JsonSerializer.Serialize(helpResult), Clock.Now);
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);

            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.Executed,
                Action = command.Action,
                DetectedLanguage = command.Language,
                AssistantMessage = helpResult.ResultMessage,
                ExecutionResult = helpResult,
            };
        }

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
                DetectedLanguage = command.Language,
                AssistantMessage = command.UserFriendlyMessage ?? _localizer["ShopManagement:AiLookupAmbiguous"].Value,
                AmbiguousLookups = ex.AmbiguousLookups,
            };
        }
        catch (BusinessException ex)
        {
            _messageManager.MarkFailed(assistantMessage, ex.Code, ex.Code);
            await _messageRepository.UpdateAsync(assistantMessage, autoSave: true);

            return new ShopAiResponseDto
            {
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                Status = ShopAiMessageStatus.Failed,
                Action = command.Action,
                AssistantMessage = ex.Code != null ? _localizer[ex.Code].Value : _localizer["ShopManagement:AiServiceUnavailable"].Value,
                ErrorCode = ex.Code,
                ErrorMessage = ex.Code,
            };
        }

        _messageManager.MarkParsed(assistantMessage, command.Action, payloadJson, command.Language);

        if (handler.IsWriteAction)
        {
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
                DetectedLanguage = command.Language,
                AssistantMessage = command.UserFriendlyMessage ?? _localizer["AiAssistant.ActionPreview"].Value,
                Warnings = command.Warnings,
                Preview = preview,
            };
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
            PayloadJson = JsonSerializer.SerializeToElement(command.Parameters),
        };

        ShopAiExecutionResultDto executionResult;
        try
        {
            executionResult = await handler.ExecuteAsync(validated);
        }
        catch (BusinessException ex)
        {
            var errorKey = ex.Code ?? "ShopManagement:AiServiceUnavailable";
            executionResult = new ShopAiExecutionResultDto { Success = false, ErrorCode = ex.Code, ErrorMessage = errorKey };
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

        // executionResult.ErrorMessage may be a plain English sentence a handler wrote directly
        // (e.g. GetTodaySalesAiHandler) or a "Module:Code" localization key bubbled up from a
        // thrown BusinessException (e.g. CreateCustomerAiHandler / the underlying domain manager).
        // _localizer[...] on a string it doesn't recognize as a key just returns that same string
        // back, so it is always safe to run both kinds through it.
        var localizedErrorMessage = executionResult.Success || executionResult.ErrorMessage == null
            ? null
            : _localizer[executionResult.ErrorMessage].Value;

        return new ShopAiResponseDto
        {
            ConversationId = conversation.Id,
            MessageId = assistantMessage.Id,
            Status = executionResult.Success ? ShopAiMessageStatus.Executed : ShopAiMessageStatus.Failed,
            Action = command.Action,
            DetectedLanguage = command.Language,
            AssistantMessage = executionResult.ResultMessage ?? command.UserFriendlyMessage ?? localizedErrorMessage ?? _localizer["ShopManagement:AiServiceUnavailable"].Value,
            Warnings = command.Warnings,
            ExecutionResult = executionResult,
            ErrorCode = executionResult.Success ? null : executionResult.ErrorCode,
            ErrorMessage = localizedErrorMessage,
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

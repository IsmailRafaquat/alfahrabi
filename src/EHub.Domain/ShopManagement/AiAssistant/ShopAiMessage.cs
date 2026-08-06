using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// One turn in a conversation - either what the user said (Role = User, possibly with
/// OriginalTranscription if it came from voice) or what the assistant produced (Role = Assistant,
/// carrying the parsed command / preview / execution result). Everything Ollama returned is kept
/// here as opaque JSON (ActionPayloadJson) - it is never deserialized back into a live command
/// without re-running it through ShopAiCommandValidator.
/// </summary>
public class ShopAiMessage : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid ConversationId { get; protected set; }
    public Guid UserId { get; protected set; }
    public ShopAiMessageRole Role { get; protected set; }
    public string MessageText { get; protected set; } = string.Empty;
    public string? OriginalTranscription { get; protected set; }
    public ShopAiLanguage DetectedLanguage { get; protected set; } = ShopAiLanguage.Unknown;
    public ShopAiActionType? DetectedAction { get; protected set; }
    public string? ActionPayloadJson { get; protected set; }
    public ShopAiMessageStatus Status { get; protected set; } = ShopAiMessageStatus.Received;
    public string? ErrorCode { get; protected set; }
    public string? ErrorMessage { get; protected set; }
    public string? ConfirmationTokenHash { get; protected set; }
    public DateTime? ConfirmationExpiryDate { get; protected set; }
    public DateTime? ExecutedDate { get; protected set; }
    public string? ExecutionResultJson { get; protected set; }

    protected ShopAiMessage() { }

    internal ShopAiMessage(
        Guid id,
        Guid tenantId,
        Guid conversationId,
        Guid userId,
        ShopAiMessageRole role,
        string messageText,
        string? originalTranscription,
        ShopAiLanguage detectedLanguage) : base(id)
    {
        TenantId = tenantId;
        ConversationId = conversationId;
        UserId = userId;
        Role = role;
        MessageText = Check.NotNullOrWhiteSpace(messageText, nameof(messageText), ShopAiConsts.MessageTextMaxLength);
        OriginalTranscription = Check.Length(originalTranscription, nameof(originalTranscription), ShopAiConsts.TranscriptionMaxLength);
        DetectedLanguage = detectedLanguage;
        Status = ShopAiMessageStatus.Received;
    }

    internal void MarkParsed(ShopAiActionType action, string actionPayloadJson, ShopAiLanguage language)
    {
        DetectedAction = action;
        ActionPayloadJson = Check.Length(actionPayloadJson, nameof(actionPayloadJson), ShopAiConsts.ActionPayloadJsonMaxLength);
        DetectedLanguage = language;
        Status = ShopAiMessageStatus.Parsed;
    }

    internal void MarkMissingInformation(string actionPayloadJson, ShopAiLanguage language)
    {
        DetectedAction = ShopAiActionType.MissingInformation;
        ActionPayloadJson = Check.Length(actionPayloadJson, nameof(actionPayloadJson), ShopAiConsts.ActionPayloadJsonMaxLength);
        DetectedLanguage = language;
        Status = ShopAiMessageStatus.MissingInformation;
    }

    internal void SetAwaitingConfirmation(string confirmationTokenHash, DateTime expiryDate)
    {
        ConfirmationTokenHash = Check.NotNullOrWhiteSpace(confirmationTokenHash, nameof(confirmationTokenHash), ShopAiConsts.ConfirmationTokenHashMaxLength);
        ConfirmationExpiryDate = expiryDate;
        Status = ShopAiMessageStatus.AwaitingConfirmation;
    }

    /// <summary>Clears the token hash immediately once consumed, so the same message can never be confirmed twice (replay protection).</summary>
    internal void MarkExecuting()
    {
        Status = ShopAiMessageStatus.Executing;
        ConfirmationTokenHash = null;
    }

    internal void MarkExecuted(string executionResultJson, DateTime executedDate)
    {
        Status = ShopAiMessageStatus.Executed;
        ExecutionResultJson = Check.Length(executionResultJson, nameof(executionResultJson), ShopAiConsts.ExecutionResultJsonMaxLength);
        ExecutedDate = executedDate;
        ConfirmationExpiryDate = null;
    }

    internal void MarkFailed(string? errorCode, string errorMessage)
    {
        Status = ShopAiMessageStatus.Failed;
        ErrorCode = Check.Length(errorCode, nameof(errorCode), ShopAiConsts.ErrorCodeMaxLength);
        ErrorMessage = Check.Length(errorMessage, nameof(errorMessage), ShopAiConsts.ErrorMessageMaxLength);
        ConfirmationTokenHash = null;
    }

    internal void MarkRejected()
    {
        Status = ShopAiMessageStatus.Rejected;
        ConfirmationTokenHash = null;
    }

    internal void MarkCancelled()
    {
        Status = ShopAiMessageStatus.Cancelled;
        ConfirmationTokenHash = null;
    }
}

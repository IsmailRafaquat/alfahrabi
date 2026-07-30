using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// The single shape returned by SendMessageAsync / SendVoiceMessageAsync / ConfirmActionAsync - one
/// assistant "turn" for the chat UI to render. Exactly one of Preview / ExecutionResult / (missing
/// fields) / AmbiguousLookups is meaningfully populated, driven by Status.
/// </summary>
public class ShopAiResponseDto
{
    public Guid ConversationId { get; set; }
    public Guid MessageId { get; set; }
    public ShopAiMessageStatus Status { get; set; }
    public ShopAiActionType Action { get; set; }
    public ShopAiLanguage DetectedLanguage { get; set; }

    /// <summary>Free-text assistant reply (help text, clarification question, or read-result summary).</summary>
    public string? AssistantMessage { get; set; }

    public List<string> MissingFields { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    /// <summary>Populated when Status == AwaitingConfirmation.</summary>
    public ShopAiActionPreviewDto? Preview { get; set; }

    /// <summary>Populated when a read action (or a just-confirmed write action) has already executed.</summary>
    public ShopAiExecutionResultDto? ExecutionResult { get; set; }

    public List<ShopAiLookupResolutionDto> AmbiguousLookups { get; set; } = new();

    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

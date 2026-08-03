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

    /// <summary>Populated when ResponseType == DataList - the frontend renders this as a data-list/table card, never the module-help card.</summary>
    public ShopAiDataListDto? DataList { get; set; }

    public List<ShopAiLookupResolutionDto> AmbiguousLookups { get; set; } = new();

    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    // --- Guided creation / project knowledge additions (additive, existing fields unchanged) ---

    /// <summary>Coarse-grained hint for the frontend on how to render this turn (module explanation, field list, field explanation, or one of the original kinds implied by Status).</summary>
    public ShopAiResponseType ResponseType { get; set; } = ShopAiResponseType.TextAnswer;

    public string? ModuleKey { get; set; }

    /// <summary>Populated when ResponseType == ModuleExplanation.</summary>
    public ShopAiModuleExplanationDto? Module { get; set; }

    /// <summary>Populated when ResponseType == FieldList or FieldExplanation.</summary>
    public List<ShopAiFieldDescriptionDto> Fields { get; set; } = new();

    /// <summary>
    /// Progress while a guided (multi-turn) creation is in flight - null once the action reaches
    /// AwaitingConfirmation (at which point Preview above already has the full field list).
    /// </summary>
    public ShopAiGuidedCreationProgressDto? GuidedCreationProgress { get; set; }
}

public class ShopAiGuidedCreationProgressDto
{
    public string ModuleKey { get; set; } = string.Empty;
    public string ModuleDisplayName { get; set; } = string.Empty;
    public List<ShopAiPreviewFieldDto> CollectedFields { get; set; } = new();
    public List<string> MissingRequiredFields { get; set; } = new();
    public int RequiredFieldCount { get; set; }
    public int CompletedRequiredFieldCount { get; set; }
}

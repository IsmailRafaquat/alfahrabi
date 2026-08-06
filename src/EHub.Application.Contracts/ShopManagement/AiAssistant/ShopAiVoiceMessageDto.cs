using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Sent only AFTER the user has reviewed (and optionally edited) the transcription returned by
/// TranscribeAsync. The raw audio is never parsed directly into a command - this keeps "review
/// transcription before acting" a real, separate stage rather than a UI-only formality.
/// </summary>
public class ShopAiVoiceMessageDto
{
    [Required]
    public Guid ConversationId { get; set; }

    [Required]
    public string AcceptedText { get; set; } = string.Empty;

    /// <summary>Pre-edit transcription, kept for audit history only.</summary>
    public string? OriginalTranscription { get; set; }

    public ShopAiLanguage? DetectedLanguage { get; set; }
}

using Volo.Abp.Content;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiVoiceUploadDto
{
    public IRemoteStreamContent Audio { get; set; } = default!;

    /// <summary>Optional hint such as "en" or "ur" - faster-whisper still auto-detects if omitted.</summary>
    public string? LanguageHint { get; set; }
}

public class ShopAiVoiceTranscriptionDto
{
    public string Text { get; set; } = string.Empty;
    public ShopAiLanguage DetectedLanguage { get; set; }
    public double LanguageProbability { get; set; }
    public double DurationSeconds { get; set; }

    /// <summary>True when LanguageProbability (or the model's own confidence) is low enough that the UI must force a review step.</summary>
    public bool IsLowConfidence { get; set; }
}

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>Talks to the local faster-whisper FastAPI service (tools/shop-ai-transcription). Never touches business data.</summary>
public interface IShopSpeechToTextClient
{
    Task<ShopAiSpeechResult> TranscribeAsync(Stream audioStream, string fileName, string contentType, string? languageHint, CancellationToken cancellationToken = default);

    /// <summary>Cheap connectivity probe for health checks.</summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

public class ShopAiSpeechResult
{
    public bool Success { get; set; }
    public string? Text { get; set; }
    public string? Language { get; set; }
    public double LanguageProbability { get; set; }
    public double DurationSeconds { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public static ShopAiSpeechResult Fail(string errorCode, string errorMessage) => new() { Success = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
}

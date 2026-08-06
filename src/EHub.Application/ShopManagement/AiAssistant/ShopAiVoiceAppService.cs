using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.AiAssistant;

[Authorize(EHubPermissions.ShopAiAssistant.UseVoice)]
public class ShopAiVoiceAppService : ApplicationService, IShopAiVoiceAppService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".webm", ".wav", ".mp3", ".m4a", ".ogg" };
    private static readonly HashSet<string> AllowedContentTypePrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "audio/webm", "audio/wav", "audio/x-wav", "audio/wave", "audio/mpeg", "audio/mp3", "audio/mp4", "audio/m4a", "audio/x-m4a", "audio/ogg",
    };

    // A transcription this uncertain forces the "review before continuing" UI path regardless of
    // what the frontend does on its own - the backend never assumes a low-confidence transcription
    // is good enough to hand straight to the command parser.
    private const double LowConfidenceThreshold = 0.55;

    private readonly IShopSpeechToTextClient _speechClient;
    private readonly ShopAiAssistantAppService _assistantAppService;
    private readonly IOptions<ShopAiSpeechOptions> _speechOptions;
    private readonly IShopAiRateLimiter _rateLimiter;

    public ShopAiVoiceAppService(
        IShopSpeechToTextClient speechClient,
        ShopAiAssistantAppService assistantAppService,
        IOptions<ShopAiSpeechOptions> speechOptions,
        IShopAiRateLimiter rateLimiter)
    {
        _speechClient = speechClient;
        _assistantAppService = assistantAppService;
        _speechOptions = speechOptions;
        _rateLimiter = rateLimiter;
    }

    public async Task<ShopAiVoiceTranscriptionDto> TranscribeAsync(ShopAiVoiceUploadDto input)
    {
        var tenantId = RequireTenant();
        var userId = RequireUser();

        if (!_rateLimiter.TryAcquire(tenantId, userId, ShopAiRateLimitCategory.Voice))
        {
            throw new BusinessException("ShopManagement:AiRateLimitExceeded");
        }

        var options = _speechOptions.Value;
        if (!options.Enabled)
        {
            throw new BusinessException("ShopManagement:AiVoiceDisabled");
        }

        var audio = input.Audio ?? throw new BusinessException("ShopManagement:AiAudioInvalid");
        var extension = ValidateAndGetExtension(audio.FileName, audio.ContentType);

        if (audio.ContentLength.HasValue && audio.ContentLength.Value <= 0)
        {
            throw new BusinessException("ShopManagement:AiAudioInvalid");
        }

        var maxBytes = (long)options.MaximumAudioSizeMb * 1024 * 1024;
        if (audio.ContentLength.HasValue && audio.ContentLength.Value > maxBytes)
        {
            throw new BusinessException("ShopManagement:AiAudioTooLarge");
        }

        // Never trust the browser-supplied file name - only its extension (already validated above)
        // informs the random name we actually forward.
        var safeFileName = Guid.NewGuid().ToString("N") + extension;

        await using var stream = audio.GetStream();
        var result = await _speechClient.TranscribeAsync(stream, safeFileName, audio.ContentType ?? "application/octet-stream", input.LanguageHint);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Text))
        {
            throw new BusinessException(result.ErrorMessage ?? "ShopManagement:AiSpeechServiceUnavailable");
        }

        if (result.DurationSeconds > options.MaximumDurationSeconds)
        {
            throw new BusinessException("ShopManagement:AiAudioTooLong");
        }

        var isLowConfidence = result.LanguageProbability > 0 && result.LanguageProbability < LowConfidenceThreshold;

        return new ShopAiVoiceTranscriptionDto
        {
            Text = result.Text,
            DetectedLanguage = MapLanguage(result.Language),
            LanguageProbability = result.LanguageProbability,
            DurationSeconds = result.DurationSeconds,
            IsLowConfidence = isLowConfidence,
        };
    }

    public Task<ShopAiResponseDto> SendVoiceMessageAsync(ShopAiVoiceMessageDto input)
    {
        var tenantId = RequireTenant();
        var userId = RequireUser();

        if (!_rateLimiter.TryAcquire(tenantId, userId, ShopAiRateLimitCategory.Voice))
        {
            throw new BusinessException("ShopManagement:AiRateLimitExceeded");
        }

        if (string.IsNullOrWhiteSpace(input.AcceptedText))
        {
            throw new BusinessException("ShopManagement:AiMissingInformation");
        }

        // AcceptedText is what the user reviewed and accepted (possibly hand-edited) - it goes
        // through exactly the same parse/validate/preview pipeline as a typed message. The raw audio
        // is never parsed directly into a command.
        return _assistantAppService.SendUserTextAsync(input.ConversationId, input.AcceptedText, input.OriginalTranscription);
    }

    private static string ValidateAndGetExtension(string? fileName, string? contentType)
    {
        var extension = string.IsNullOrWhiteSpace(fileName) ? null : Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new BusinessException("ShopManagement:AiAudioInvalid");
        }

        if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypePrefixes.Any(prefix => contentType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException("ShopManagement:AiAudioInvalid");
        }

        return extension;
    }

    private static ShopAiLanguage MapLanguage(string? whisperLanguageCode) => whisperLanguageCode?.ToLowerInvariant() switch
    {
        "ur" => ShopAiLanguage.Urdu,
        "en" => ShopAiLanguage.English,
        _ => ShopAiLanguage.Unknown,
    };

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
    private Guid RequireUser() => CurrentUser.Id ?? throw new BusinessException("ShopManagement:AiPermissionDenied");
}

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant;

public class ShopSpeechToTextClient : IShopSpeechToTextClient, ITransientDependency
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<ShopAiSpeechOptions> _options;
    private readonly ILogger<ShopSpeechToTextClient> _logger;

    public ShopSpeechToTextClient(HttpClient httpClient, IOptions<ShopAiSpeechOptions> options, ILogger<ShopSpeechToTextClient> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<ShopAiSpeechResult> TranscribeAsync(Stream audioStream, string fileName, string contentType, string? languageHint, CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        if (!options.Enabled)
        {
            return ShopAiSpeechResult.Fail("AiVoiceDisabled", "ShopManagement:AiVoiceDisabled");
        }

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(1, options.RequestTimeoutSeconds)));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            using var form = new MultipartFormDataContent();
            using var streamContent = new StreamContent(audioStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

            // A random, server-chosen file name only - the browser's original file name is never forwarded.
            form.Add(streamContent, "audio", fileName);
            if (!string.IsNullOrWhiteSpace(languageHint))
            {
                form.Add(new StringContent(languageHint), "language");
            }

            using var response = await _httpClient.PostAsync("/transcribe", form, linkedCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Speech-to-text request failed with status {StatusCode}", (int)response.StatusCode);
                return ShopAiSpeechResult.Fail("AiSpeechServiceUnavailable", "ShopManagement:AiSpeechServiceUnavailable");
            }

            var payload = await response.Content.ReadFromJsonAsync<SpeechTranscribeResponse>(cancellationToken: linkedCts.Token);
            if (payload == null || string.IsNullOrWhiteSpace(payload.Text))
            {
                return ShopAiSpeechResult.Fail("AiAudioInvalid", "ShopManagement:AiAudioInvalid");
            }

            return new ShopAiSpeechResult
            {
                Success = true,
                Text = payload.Text,
                Language = payload.Language,
                LanguageProbability = payload.LanguageProbability,
                DurationSeconds = payload.DurationSeconds,
            };
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            _logger.LogWarning("Speech-to-text request timed out after {Timeout}s", options.RequestTimeoutSeconds);
            return ShopAiSpeechResult.Fail("AiSpeechServiceUnavailable", "ShopManagement:AiSpeechServiceUnavailable");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Speech-to-text request could not reach {BaseUrl}", options.BaseUrl);
            return ShopAiSpeechResult.Fail("AiSpeechServiceUnavailable", "ShopManagement:AiSpeechServiceUnavailable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling the speech-to-text service");
            return ShopAiSpeechResult.Fail("AiSpeechServiceUnavailable", "ShopManagement:AiSpeechServiceUnavailable");
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Value.Enabled) return false;

        try
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            using var response = await _httpClient.GetAsync("/health", linkedCts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

internal class SpeechTranscribeResponse
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("languageProbability")]
    public double LanguageProbability { get; set; }

    [JsonPropertyName("durationSeconds")]
    public double DurationSeconds { get; set; }
}

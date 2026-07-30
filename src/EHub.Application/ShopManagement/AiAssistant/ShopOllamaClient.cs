using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant;

public class ShopOllamaClient : IShopOllamaClient, ITransientDependency
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<ShopAiOptions> _options;
    private readonly ILogger<ShopOllamaClient> _logger;

    public ShopOllamaClient(HttpClient httpClient, IOptions<ShopAiOptions> options, ILogger<ShopOllamaClient> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<ShopAiOllamaChatResult> ChatAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        if (!options.Enabled)
        {
            return ShopAiOllamaChatResult.Fail("AiAssistantDisabled", "ShopManagement:AiAssistantDisabled");
        }

        var request = new OllamaChatRequest
        {
            Model = options.ChatModel,
            Stream = false,
            Format = "json",
            Messages = new List<OllamaChatMessage>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user", Content = userMessage },
            },
            Options = new OllamaChatRequestOptions { Temperature = (double)options.Temperature },
            // Without this, Ollama unloads the model after 5 minutes idle (its default), so the
            // very next message pays the full ~30s load cost again on top of generation time -
            // easily blowing through RequestTimeoutSeconds on CPU inference. Keeping it warm for
            // 30 minutes means only the FIRST message after a long gap pays that cost.
            KeepAlive = "30m",
        };

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(1, options.RequestTimeoutSeconds)));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            using var response = await _httpClient.PostAsJsonAsync("/api/chat", request, linkedCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ollama chat request failed with status {StatusCode}", (int)response.StatusCode);
                return ShopAiOllamaChatResult.Fail("AiServiceUnavailable", "ShopManagement:AiServiceUnavailable");
            }

            var payload = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: linkedCts.Token);
            var content = payload?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Ollama chat response had no message content");
                return ShopAiOllamaChatResult.Fail("AiInvalidResponse", "ShopManagement:AiInvalidResponse");
            }

            return ShopAiOllamaChatResult.Ok(content);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            _logger.LogWarning("Ollama chat request timed out after {Timeout}s", options.RequestTimeoutSeconds);
            return ShopAiOllamaChatResult.Fail("AiServiceUnavailable", "ShopManagement:AiServiceUnavailable");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Ollama chat request could not reach {BaseUrl}", options.BaseUrl);
            return ShopAiOllamaChatResult.Fail("AiServiceUnavailable", "ShopManagement:AiServiceUnavailable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Ollama");
            return ShopAiOllamaChatResult.Fail("AiServiceUnavailable", "ShopManagement:AiServiceUnavailable");
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Value.Enabled) return false;

        try
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            using var response = await _httpClient.GetAsync("/api/tags", linkedCts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

internal class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OllamaChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("options")]
    public OllamaChatRequestOptions? Options { get; set; }

    [JsonPropertyName("keep_alive")]
    public string? KeepAlive { get; set; }
}

internal class OllamaChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

internal class OllamaChatRequestOptions
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }
}

internal class OllamaChatResponse
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("message")]
    public OllamaChatMessage? Message { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}

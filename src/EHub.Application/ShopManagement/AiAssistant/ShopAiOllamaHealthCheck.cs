using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Reports Ollama reachability only - never blocks application startup and never affects any
/// non-AI feature. The rest of Shop Management keeps working when this is Unhealthy.
/// </summary>
public class ShopAiOllamaHealthCheck : IHealthCheck, ITransientDependency
{
    private readonly IShopOllamaClient _ollamaClient;
    private readonly IOptions<ShopAiOptions> _options;

    public ShopAiOllamaHealthCheck(IShopOllamaClient ollamaClient, IOptions<ShopAiOptions> options)
    {
        _ollamaClient = ollamaClient;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!_options.Value.Enabled)
        {
            return HealthCheckResult.Healthy("ShopAi is disabled - Ollama is not required.");
        }

        var available = await _ollamaClient.IsAvailableAsync(cancellationToken);
        return available
            ? HealthCheckResult.Healthy($"Ollama reachable at {_options.Value.BaseUrl}.")
            : HealthCheckResult.Unhealthy($"Ollama not reachable at {_options.Value.BaseUrl}. Text/voice AI commands will fail until it is running (see README: ollama pull {_options.Value.ChatModel}).");
    }
}

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>Reports faster-whisper reachability only - text-based AI commands keep working when this is Unhealthy; only voice input is affected.</summary>
public class ShopAiSpeechHealthCheck : IHealthCheck, ITransientDependency
{
    private readonly IShopSpeechToTextClient _speechClient;
    private readonly IOptions<ShopAiSpeechOptions> _options;

    public ShopAiSpeechHealthCheck(IShopSpeechToTextClient speechClient, IOptions<ShopAiSpeechOptions> options)
    {
        _speechClient = speechClient;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!_options.Value.Enabled)
        {
            return HealthCheckResult.Healthy("Voice input is disabled - the speech-to-text service is not required.");
        }

        var available = await _speechClient.IsAvailableAsync(cancellationToken);
        return available
            ? HealthCheckResult.Healthy($"Speech-to-text service reachable at {_options.Value.BaseUrl}.")
            : HealthCheckResult.Unhealthy($"Speech-to-text service not reachable at {_options.Value.BaseUrl}. Voice input will be disabled until it is running (see tools/shop-ai-transcription/README.md).");
    }
}

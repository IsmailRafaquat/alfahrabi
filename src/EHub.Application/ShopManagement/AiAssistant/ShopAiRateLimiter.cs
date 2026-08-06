using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiRateLimiter : IShopAiRateLimiter, ISingletonDependency
{
    private readonly IOptions<ShopAiOptions> _options;
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _windows = new();
    private readonly object _sync = new();

    public ShopAiRateLimiter(IOptions<ShopAiOptions> options)
    {
        _options = options;
    }

    public bool TryAcquire(Guid tenantId, Guid userId, ShopAiRateLimitCategory category)
    {
        var limit = category switch
        {
            ShopAiRateLimitCategory.Text => _options.Value.TextCommandsPerMinute,
            ShopAiRateLimitCategory.Voice => _options.Value.VoiceCommandsPerMinute,
            ShopAiRateLimitCategory.Confirmation => _options.Value.ConfirmationAttemptsPerMinute,
            ShopAiRateLimitCategory.McpRead => _options.Value.McpReadToolsPerMinute,
            ShopAiRateLimitCategory.McpPrepare => _options.Value.McpPrepareToolsPerMinute,
            ShopAiRateLimitCategory.McpConfirm => _options.Value.McpConfirmToolsPerMinute,
            _ => 0,
        };
        if (limit <= 0) return true;

        var key = $"{tenantId:N}|{userId:N}|{category}";
        var now = DateTime.UtcNow;
        var windowStart = now.AddMinutes(-1);

        lock (_sync)
        {
            var queue = _windows.GetOrAdd(key, _ => new Queue<DateTime>());
            while (queue.Count > 0 && queue.Peek() < windowStart)
            {
                queue.Dequeue();
            }

            if (queue.Count >= limit) return false;

            queue.Enqueue(now);
            return true;
        }
    }
}

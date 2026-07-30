using System.Threading;
using System.Threading.Tasks;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// The ONLY thing allowed to talk to Ollama over HTTP. It knows nothing about customers, sales, or
/// any business concept - it sends a system prompt + a user message and hands back whatever text
/// came back. ShopAiCommandParser is responsible for treating that text as untrusted and validating
/// it before it becomes a ShopAiParsedCommand.
/// </summary>
public interface IShopOllamaClient
{
    Task<ShopAiOllamaChatResult> ChatAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default);

    /// <summary>Cheap connectivity probe for health checks - does not invoke the chat model.</summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}

public class ShopAiOllamaChatResult
{
    public bool Success { get; set; }

    /// <summary>Raw assistant message content - expected (but not guaranteed) to be a JSON document.</summary>
    public string? Content { get; set; }

    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public static ShopAiOllamaChatResult Ok(string content) => new() { Success = true, Content = content };
    public static ShopAiOllamaChatResult Fail(string errorCode, string errorMessage) => new() { Success = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
}

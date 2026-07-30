using System.Threading;
using System.Threading.Tasks;

namespace EHub.ShopManagement.AiAssistant;

public interface IShopAiCommandParser
{
    Task<ShopAiParseResult> ParseAsync(string userMessage, CancellationToken cancellationToken = default);
}

public class ShopAiParseResult
{
    public bool Success { get; set; }
    public ShopAiParsedCommand? Command { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public static ShopAiParseResult Ok(ShopAiParsedCommand command) => new() { Success = true, Command = command };
    public static ShopAiParseResult Fail(string errorCode, string errorMessage) => new() { Success = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
}

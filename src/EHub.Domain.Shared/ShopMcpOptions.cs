namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Bound from the "ShopMcp" configuration section - describes how the MAIN application (as an MCP
/// client) would reach the separately-hosted EHub.McpServer. Not yet consumed by any client code
/// this round (the Shop AI Assistant still calls IShopAiActionHandlerRegistry directly, in-process)
/// - this options class exists so the shape is in place for that follow-up work, and so
/// "Do not hardcode configuration values" already holds once it's wired up.
/// </summary>
public class ShopMcpOptions
{
    public bool Enabled { get; set; }
    public string Endpoint { get; set; } = "https://localhost:44300/mcp";
    public string Transport { get; set; } = "StreamableHttp";
    public int RequestTimeoutSeconds { get; set; } = 120;
    public bool RequireAuthentication { get; set; } = true;
    public bool EnableWriteTools { get; set; } = true;
    public bool RequireConfirmationForWrites { get; set; } = true;
}

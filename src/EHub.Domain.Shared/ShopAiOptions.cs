namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Bound from the "ShopAi" configuration section. Ollama is treated as untrusted, unreliable
/// infrastructure: every value here exists to bound how much we trust it (timeout, confidence,
/// input size) rather than to grant it any additional capability.
/// </summary>
public class ShopAiOptions
{
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Ollama";
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ChatModel { get; set; } = default!;
    public decimal Temperature { get; set; }
    public int RequestTimeoutSeconds { get; set; } = 120;
    public int MaxInputCharacters { get; set; } = 5000;
    public bool StoreConversationHistory { get; set; } = true;
    public bool RequireConfirmationForWrites { get; set; } = true;
    public int ConfirmationExpiryMinutes { get; set; } = 5;

    /// <summary>Minimum ShopAiParsedCommand.Confidence accepted for a write action without asking for clarification.</summary>
    public decimal MinimumWriteConfidence { get; set; } = 0.6m;

    public int TextCommandsPerMinute { get; set; } = 20;
    public int VoiceCommandsPerMinute { get; set; } = 5;
    public int ConfirmationAttemptsPerMinute { get; set; } = 10;

    public int McpReadToolsPerMinute { get; set; } = 60;
    public int McpPrepareToolsPerMinute { get; set; } = 20;
    public int McpConfirmToolsPerMinute { get; set; } = 10;
}

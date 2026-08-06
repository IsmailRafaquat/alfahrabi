namespace EHub.ShopManagement.AiAssistant;

/// <summary>Bound from the "ShopAiSpeech" configuration section - see tools/shop-ai-transcription for the service this points at.</summary>
public class ShopAiSpeechOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "http://localhost:8001";
    public string Model { get; set; } = "small";
    public string Device { get; set; } = "cpu";
    public string ComputeType { get; set; } = "int8";
    public int RequestTimeoutSeconds { get; set; } = 180;
    public int MaximumAudioSizeMb { get; set; } = 15;
    public int MaximumDurationSeconds { get; set; } = 120;
    public bool StoreAudioFiles { get; set; }
}

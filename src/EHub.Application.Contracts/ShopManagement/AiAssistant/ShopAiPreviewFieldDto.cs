namespace EHub.ShopManagement.AiAssistant;

/// <summary>One labeled row in an action preview / confirmation card, e.g. "Name: Ali".</summary>
public class ShopAiPreviewFieldDto
{
    /// <summary>Localization key, e.g. "::Name" - the frontend resolves this via abpLocalization.</summary>
    public string LabelKey { get; set; } = string.Empty;

    /// <summary>Display-ready value. Never a raw GUID - always a human-readable string.</summary>
    public string? Value { get; set; }

    public bool IsEmpty { get; set; }
}

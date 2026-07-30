using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// What the frontend renders as the confirmation card ("Create Customer / Name: Ali / Phone: ... /
/// [Confirm and Create] [Edit] [Cancel]"). For write actions, ConfirmationToken is the only thing
/// the frontend is allowed to send back - see IShopAiConfirmationService.
/// </summary>
public class ShopAiActionPreviewDto
{
    public ShopAiActionType Action { get; set; }

    /// <summary>Localization key for the card header, e.g. "::AiAction:CreateCustomer".</summary>
    public string ActionDisplayNameKey { get; set; } = string.Empty;

    public bool IsWriteAction { get; set; }
    public bool RequiresConfirmation { get; set; }
    public List<ShopAiPreviewFieldDto> Fields { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    /// <summary>Localization key for an explanatory note, e.g. "::AiAssistant.WillNotAffectBalanceUntilPosted".</summary>
    public string? NoteKey { get; set; }

    /// <summary>Opaque, single-use, short-lived token. Null for read actions (nothing to confirm).</summary>
    public string? ConfirmationToken { get; set; }
    public DateTime? ConfirmationExpiryDate { get; set; }
}

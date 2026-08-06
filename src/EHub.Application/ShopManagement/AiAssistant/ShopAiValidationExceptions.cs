using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Thrown by a handler's PrepareAsync when required fields are missing from the (already
/// deserialized) command. Caught by ShopAiAssistantAppService and turned into a
/// Status = MissingInformation response - never a 500, never a generic BusinessException.
/// </summary>
public class ShopAiMissingInformationException : Exception
{
    public List<string> MissingFields { get; }
    public string? UserFriendlyMessage { get; }

    public ShopAiMissingInformationException(List<string> missingFields, string? userFriendlyMessage = null)
        : base("Missing required information for AI action.")
    {
        MissingFields = missingFields;
        UserFriendlyMessage = userFriendlyMessage;
    }
}

/// <summary>Thrown when a name the user gave resolved to more than one current-tenant record.</summary>
public class ShopAiAmbiguousMatchException : Exception
{
    public List<ShopAiLookupResolutionDto> AmbiguousLookups { get; }

    public ShopAiAmbiguousMatchException(List<ShopAiLookupResolutionDto> ambiguousLookups)
        : base("The AI request matched more than one record.")
    {
        AmbiguousLookups = ambiguousLookups;
    }
}

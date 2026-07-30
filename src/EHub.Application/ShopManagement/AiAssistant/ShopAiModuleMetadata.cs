using System.Collections.Generic;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// The ONLY source of truth for what fields a module has and what Ollama is allowed to say about
/// them. Hand-authored from the real Create*Dto classes (see ShopAiModuleMetadataProvider) -
/// nothing here is model-generated, and the parser/slot-filling service never accept a field name
/// Ollama invents that isn't registered here.
/// </summary>
public class ShopAiModuleMetadata
{
    public string ModuleKey { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string Description { get; set; } = default!;

    public ShopAiActionType CreateAction { get; set; }
    public string RequiredPermission { get; set; } = default!;
    public string ExistingCreatePermission { get; set; } = default!;

    public bool SupportsCreation { get; set; }
    public bool RequiresConfirmation { get; set; } = true;
    public bool CreatesDraftOnly { get; set; }

    public List<string> Aliases { get; set; } = new();
    public List<ShopAiFieldMetadata> Fields { get; set; } = new();
    public List<string> BusinessRules { get; set; } = new();
    public List<string> RelatedModules { get; set; } = new();
}

public class ShopAiFieldMetadata
{
    public string FieldKey { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string DataType { get; set; } = default!;

    public bool IsRequired { get; set; }
    public bool IsLookup { get; set; }
    public string? LookupModuleKey { get; set; }
    public bool IsCalculated { get; set; }
    public bool IsSystemGenerated { get; set; }
    public bool IsUserEditable { get; set; } = true;

    public string? ExampleValue { get; set; }
    public decimal? MinimumValue { get; set; }
    public decimal? MaximumValue { get; set; }
    public int? MaximumLength { get; set; }
    public List<string> AllowedValues { get; set; } = new();
    public List<string> Aliases { get; set; } = new();
}

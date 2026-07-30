using System.Collections.Generic;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiFieldDescriptionDto
{
    public string FieldKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsLookup { get; set; }
    public bool IsSystemGenerated { get; set; }
    public string? ExampleValue { get; set; }
    public List<string> AllowedValues { get; set; } = new();
}

public class ShopAiModuleExplanationDto
{
    public string ModuleKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool SupportsCreation { get; set; }
    public bool CreatesDraftOnly { get; set; }
    public List<ShopAiFieldDescriptionDto> RequiredFields { get; set; } = new();
    public List<ShopAiFieldDescriptionDto> OptionalFields { get; set; } = new();
    public List<ShopAiFieldDescriptionDto> SystemGeneratedFields { get; set; } = new();
    public List<string> BusinessRules { get; set; } = new();
    public List<string> RelatedModules { get; set; } = new();
}

public class ShopAiModuleListDto
{
    public string ModuleKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool SupportsCreation { get; set; }
}

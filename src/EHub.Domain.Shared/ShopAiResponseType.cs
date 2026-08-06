namespace EHub.ShopManagement.AiAssistant;

public enum ShopAiResponseType
{
    TextAnswer = 0,
    ModuleExplanation = 1,
    FieldList = 2,
    FieldExplanation = 3,
    MissingInformation = 4,
    LookupChoices = 5,
    ActionPreview = 6,
    ExecutionResult = 7,
    Error = 8,

    /// <summary>A read action that returned a list of existing records (see ShopAiDataListDto) - rendered as a data-list card, never the module-help card.</summary>
    DataList = 9
}

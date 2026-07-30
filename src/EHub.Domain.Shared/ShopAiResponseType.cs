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
    Error = 8
}

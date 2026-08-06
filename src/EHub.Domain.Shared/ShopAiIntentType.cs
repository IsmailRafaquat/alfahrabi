namespace EHub.ShopManagement.AiAssistant;

public enum ShopAiIntentType
{
    Unknown = 0,
    GeneralHelp = 1,
    ExplainModule = 2,
    ListModuleFields = 3,
    ExplainField = 4,
    ExplainBusinessRule = 5,
    StartRecordCreation = 6,
    ContinueRecordCreation = 7,
    EditPendingRecord = 8,
    CancelPendingRecord = 9,
    ConfirmPendingRecord = 10,
    ReadBusinessData = 11
}

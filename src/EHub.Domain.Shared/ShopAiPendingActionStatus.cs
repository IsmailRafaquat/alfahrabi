namespace EHub.ShopManagement.AiAssistant;

public enum ShopAiPendingActionStatus
{
    CollectingInformation = 0,
    ResolvingLookups = 1,
    ReadyForConfirmation = 2,
    Executing = 3,
    Executed = 4,
    Cancelled = 5,
    Expired = 6,
    Failed = 7
}

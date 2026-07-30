namespace EHub.ShopManagement.AiAssistant;

public enum ShopAiMessageStatus
{
    Received = 0,
    Transcribed = 1,
    Parsed = 2,
    MissingInformation = 3,
    AwaitingConfirmation = 4,
    Executing = 5,
    Executed = 6,
    Failed = 7,
    Rejected = 8,
    Cancelled = 9
}

using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.AiAssistant;

public interface IShopAiAssistantAppService : IApplicationService
{
    Task<ShopAiConversationDto> CreateConversationAsync();

    Task<PagedResultDto<ShopAiConversationListDto>> GetConversationsAsync(GetShopAiConversationsInput input);

    Task<ShopAiConversationDto> GetConversationAsync(Guid id);

    /// <summary>
    /// The single entry point for every free-text turn - project questions (explain a module,
    /// list its fields, explain one field), starting/continuing a guided creation, and legacy
    /// read-only business queries are all routed from here based on detected intent. There is
    /// deliberately no separate AskProjectQuestionAsync/StartGuidedCreationAsync/
    /// ContinueGuidedCreationAsync: one composer, one endpoint, intent decides what happens -
    /// matching how the chat UI already works.
    /// </summary>
    Task<ShopAiResponseDto> SendMessageAsync(SendShopAiMessageDto input);

    /// <summary>
    /// Confirms whatever the conversation's most recent AwaitingConfirmation message is waiting on -
    /// a plain write action (CreateCustomer, etc.) or a completed guided creation (CreateUnit,
    /// etc.) are confirmed identically, since a guided creation only ever reaches
    /// AwaitingConfirmation once every required field has been collected and validated.
    /// </summary>
    Task<ShopAiExecutionResultDto> ConfirmActionAsync(ConfirmShopAiActionDto input);

    /// <summary>Cancels an AwaitingConfirmation message, or the conversation's active in-progress guided creation (ShopAiPendingAction) if one exists and no confirmation has been offered yet.</summary>
    Task CancelActionAsync(CancelShopAiActionDto input);

    Task DeleteConversationAsync(Guid id);

    Task<ListResultDto<ShopAiModuleListDto>> GetSupportedModulesAsync();

    Task<ShopAiModuleExplanationDto> GetModuleHelpAsync(string moduleKey);
}

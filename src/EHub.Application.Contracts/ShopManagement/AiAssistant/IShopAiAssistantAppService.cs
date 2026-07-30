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

    Task<ShopAiResponseDto> SendMessageAsync(SendShopAiMessageDto input);

    Task<ShopAiExecutionResultDto> ConfirmActionAsync(ConfirmShopAiActionDto input);

    Task CancelActionAsync(CancelShopAiActionDto input);

    Task DeleteConversationAsync(Guid id);
}

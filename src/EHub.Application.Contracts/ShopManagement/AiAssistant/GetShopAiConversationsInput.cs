using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.AiAssistant;

public class GetShopAiConversationsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}

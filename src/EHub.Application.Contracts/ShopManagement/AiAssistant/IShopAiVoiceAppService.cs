using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.AiAssistant;

public interface IShopAiVoiceAppService : IApplicationService
{
    Task<ShopAiVoiceTranscriptionDto> TranscribeAsync(ShopAiVoiceUploadDto input);

    Task<ShopAiResponseDto> SendVoiceMessageAsync(ShopAiVoiceMessageDto input);
}

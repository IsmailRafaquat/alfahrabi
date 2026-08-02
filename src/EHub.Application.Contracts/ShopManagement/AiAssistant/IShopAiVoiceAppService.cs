using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.AiAssistant;

public interface IShopAiVoiceAppService : IApplicationService
{
    // ABP's conventional-controller generator does not reliably produce a multipart/form-data
    // endpoint for a method whose only parameter is a DTO wrapping IRemoteStreamContent - it was
    // observed serving this route with a request-body content type the browser's real
    // multipart/form-data upload could never satisfy, causing a 415 on every voice upload. This
    // method is exposed instead through an explicit controller (see
    // EHub.HttpApi/ShopManagement/AiAssistant/ShopAiVoiceController) that declares
    // [Consumes("multipart/form-data")] and [FromForm] itself - RemoteService(false) here stops
    // ABP from ALSO generating a second, conflicting endpoint at the same route.
    [RemoteService(IsEnabled = false)]
    Task<ShopAiVoiceTranscriptionDto> TranscribeAsync(ShopAiVoiceUploadDto input);

    Task<ShopAiResponseDto> SendVoiceMessageAsync(ShopAiVoiceMessageDto input);
}

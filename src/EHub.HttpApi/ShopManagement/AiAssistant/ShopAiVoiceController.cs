using System.Threading.Tasks;
using EHub.Controllers;
using EHub.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Explicit controller for the one voice-upload endpoint ABP's conventional-controller generator
/// cannot correctly serve (see IShopAiVoiceAppService.TranscribeAsync). Every other AI Assistant
/// endpoint - including SendVoiceMessageAsync on this same AppService - continues to work through
/// the normal auto-generated conventional controller; only this route needed to move.
///
/// ApiExplorerSettings(IgnoreApi = true) hides this controller from Swagger/OpenAPI entirely. This
/// is deliberate, not cosmetic: `abp generate-proxy` reads that same OpenAPI document, and every
/// prior attempt at leaving this route discoverable caused the generator to regenerate
/// shop-ai-voice.service.ts into a broken state (a JSON-body "transcribe" stub, with
/// sendVoiceMessage dropped entirely) on every proxy refresh. The frontend already calls this
/// route directly via a hardcoded URL (ShopAiVoiceUploadService), so it was never discovered
/// through the generated proxy anyway - hiding it here costs nothing and stops the regeneration
/// damage at the source instead of hand-fixing the same file after every `abp generate-proxy` run.
/// </summary>
[Area("app")]
[Authorize(EHubPermissions.ShopAiAssistant.UseVoice)]
[Route("api/app/shop-ai-voice")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ShopAiVoiceController : EHubController
{
    private readonly IShopAiVoiceAppService _voiceAppService;

    public ShopAiVoiceController(IShopAiVoiceAppService voiceAppService)
    {
        _voiceAppService = voiceAppService;
    }

    [HttpPost("transcribe")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(15 * 1024 * 1024)]
    public Task<ShopAiVoiceTranscriptionDto> TranscribeAsync([FromForm] ShopAiVoiceUploadDto input)
    {
        // Permission, tenant, rate-limit, and audio validation all stay inside
        // ShopAiVoiceAppService.TranscribeAsync - this controller exists purely to fix HTTP-level
        // content-type/binding, not to re-implement any business rule.
        return _voiceAppService.TranscribeAsync(input);
    }
}

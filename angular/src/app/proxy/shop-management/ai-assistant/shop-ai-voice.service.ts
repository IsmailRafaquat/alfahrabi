import type { ShopAiResponseDto, ShopAiVoiceMessageDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopAiVoiceService {
  apiName = 'Default';


  // NOTE: transcribe() is deliberately NOT exposed here. The backend's TranscribeAsync is marked
  // [RemoteService(IsEnabled = false)] AND [ApiExplorerSettings(IgnoreApi = true)] on its explicit
  // controller (ShopAiVoiceController), specifically so `abp generate-proxy` never sees this route
  // again and can't regenerate a broken stub here. Use ShopAiVoiceUploadService.transcribe()
  // (FormData + HttpClient) instead - it calls the same URL directly.

  sendVoiceMessage = (input: ShopAiVoiceMessageDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopAiResponseDto>({
      method: 'POST',
      url: '/api/app/shop-ai-voice/send-voice-message',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}

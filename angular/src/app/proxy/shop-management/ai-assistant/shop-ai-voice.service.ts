import type { ShopAiResponseDto, ShopAiVoiceMessageDto, ShopAiVoiceTranscriptionDto, ShopAiVoiceUploadDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopAiVoiceService {
  apiName = 'Default';
  

  sendVoiceMessage = (input: ShopAiVoiceMessageDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopAiResponseDto>({
      method: 'POST',
      url: '/api/app/shop-ai-voice/send-voice-message',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  transcribe = (input: ShopAiVoiceUploadDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopAiVoiceTranscriptionDto>({
      method: 'POST',
      url: '/api/app/shop-ai-voice/transcribe',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}

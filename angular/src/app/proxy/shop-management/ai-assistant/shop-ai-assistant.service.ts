import type { CancelShopAiActionDto, ConfirmShopAiActionDto, GetShopAiConversationsInput, SendShopAiMessageDto, ShopAiConversationDto, ShopAiConversationListDto, ShopAiExecutionResultDto, ShopAiResponseDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopAiAssistantService {
  apiName = 'Default';
  

  cancelAction = (input: CancelShopAiActionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/shop-ai-assistant/cancel-action',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  confirmAction = (input: ConfirmShopAiActionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopAiExecutionResultDto>({
      method: 'POST',
      url: '/api/app/shop-ai-assistant/confirm-action',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  createConversation = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopAiConversationDto>({
      method: 'POST',
      url: '/api/app/shop-ai-assistant/conversation',
    },
    { apiName: this.apiName,...config });
  

  deleteConversation = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-ai-assistant/${id}/conversation`,
    },
    { apiName: this.apiName,...config });
  

  getConversation = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopAiConversationDto>({
      method: 'GET',
      url: `/api/app/shop-ai-assistant/${id}/conversation`,
    },
    { apiName: this.apiName,...config });
  

  getConversations = (input: GetShopAiConversationsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopAiConversationListDto>>({
      method: 'GET',
      url: '/api/app/shop-ai-assistant/conversations',
      params: { filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  sendMessage = (input: SendShopAiMessageDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopAiResponseDto>({
      method: 'POST',
      url: '/api/app/shop-ai-assistant/send-message',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}

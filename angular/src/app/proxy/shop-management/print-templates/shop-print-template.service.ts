import type { ShopPrintDocumentDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopPrintTemplateService {
  apiName = 'Default';
  

  getPrintDocument = (documentType: string, documentId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPrintDocumentDto>({
      method: 'GET',
      url: `/api/app/shop-print-template/print-document/${documentId}`,
      params: { documentType },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}

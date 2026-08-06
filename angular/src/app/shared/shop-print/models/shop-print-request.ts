import { ShopPrintPaperSize } from '../../../proxy/shop-management/print-settings';

export interface ShopPrintRequest {
  documentType: string;
  documentId: string;
  paperSize?: ShopPrintPaperSize;
  copies?: number;
  whatsAppShareMessage?: string;
}

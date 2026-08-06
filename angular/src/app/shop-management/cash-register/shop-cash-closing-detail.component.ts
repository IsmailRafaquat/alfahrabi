import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopCashClosingDto,
  ShopCashClosingStatus,
  ShopCashDirection,
  ShopCashRegisterService,
  ShopCashRegisterTransactionDto,
  ShopCashTransactionType,
} from '../../proxy/shop-management/cash-registers';
import { ShopPrintService } from '../../shared/shop-print/services/shop-print.service';
import { SHOP_PRINT_DOCUMENT_TYPES } from '../../shared/shop-print/models/shop-print-document-types';

@Component({ selector: 'app-shop-cash-closing-detail', standalone: false, templateUrl: './shop-cash-closing-detail.component.html', styleUrl: './shop-cash-closing-detail.component.scss' })
export class ShopCashClosingDetailComponent implements OnInit {
  private readonly service = inject(ShopCashRegisterService);
  private readonly printService = inject(ShopPrintService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly ShopCashClosingStatus = ShopCashClosingStatus;
  readonly ShopCashDirection = ShopCashDirection;

  closing: ShopCashClosingDto | null = null;
  transactions: ShopCashRegisterTransactionDto[] = [];
  loading = false;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.load(id);
  }

  load(id: string): void {
    this.loading = true;
    this.service.getClosing(id).subscribe(closing => {
      this.closing = closing;
      this.service
        .getTransactions({ cashClosingId: id, sorting: 'transactionDate asc, creationTime asc', maxResultCount: 1000 })
        .pipe(finalize(() => (this.loading = false)))
        .subscribe(result => (this.transactions = result.items || []));
    });
  }

  statusLabel(status: ShopCashClosingStatus): string {
    return '::' + ShopCashClosingStatus[status];
  }

  statusClass(status: ShopCashClosingStatus): string {
    switch (status) {
      case ShopCashClosingStatus.Open: return 'open';
      case ShopCashClosingStatus.Closed: return 'closed';
      case ShopCashClosingStatus.Cancelled: return 'cancelled';
      default: return 'open';
    }
  }

  typeLabel(type: ShopCashTransactionType): string {
    return '::' + ShopCashTransactionType[type];
  }

  directionLabel(direction: ShopCashDirection): string {
    return direction === ShopCashDirection.In ? '::DirectionIn' : '::DirectionOut';
  }

  print(): void {
    if (!this.closing) return;
    this.printService.openPreview({
      documentType: SHOP_PRINT_DOCUMENT_TYPES.CashClosingSlip,
      documentId: this.closing.id,
      whatsAppShareMessage: `Cash Closing - ${this.closing.cashRegisterName}`,
    });
  }

  back(): void {
    this.router.navigate(['/shop-management/cash-register/closings']);
  }
}

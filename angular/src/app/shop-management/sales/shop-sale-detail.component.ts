import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { CompleteShopSaleDto, ShopSaleDto, ShopSalePaymentMethod, ShopSaleProductLookupDto, ShopSaleService, ShopSaleStatus, ShopSaleType } from '../../proxy/shop-management/sales';
import { ShopCustomerPaymentService } from '../../proxy/shop-management/customer-payments';
import { ShopSaleReturnService, ShopSaleReturnStatus } from '../../proxy/shop-management/sale-returns';
import { ShopProductBatchLookupDto, ShopProductBatchService } from '../../proxy/shop-management/product-batches';
import { ShopSettingDto, ShopSettingService } from '../../proxy/shop-management/settings';

interface CompleteBatchAllocationRow {
  productBatchId: string;
  quantity: number | null;
}

interface CompleteBatchItem {
  saleItemId: string;
  productId: string;
  productName: string;
  unitShortName: string;
  quantity: number;
  mode: 'auto' | 'manual';
  loadingBatches: boolean;
  batches: ShopProductBatchLookupDto[];
  allocations: CompleteBatchAllocationRow[];
}

@Component({ selector: 'app-shop-sale-detail', standalone: false, templateUrl: './shop-sale-detail.component.html', styleUrl: './shop-sale-detail.component.scss' })
export class ShopSaleDetailComponent implements OnInit {
  private readonly service = inject(ShopSaleService);
  private readonly customerPaymentService = inject(ShopCustomerPaymentService);
  private readonly saleReturnService = inject(ShopSaleReturnService);
  private readonly productBatchService = inject(ShopProductBatchService);
  private readonly settingService = inject(ShopSettingService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly ShopSaleStatus = ShopSaleStatus;
  readonly ShopSaleType = ShopSaleType;
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.Sales.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.Sales.Delete');
  readonly canComplete = this.permissions.getGrantedPolicy('ShopManagement.Sales.Complete');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.Sales.Cancel');
  readonly canViewPrice = this.permissions.getGrantedPolicy('ShopManagement.Sales.ViewPrice');
  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.Sales.ViewCost');
  readonly canViewStockTransactions = this.permissions.getGrantedPolicy('ShopManagement.StockTransactions');
  readonly canCreateCustomerPayment = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.Create');
  readonly canViewCustomerLedger = this.permissions.getGrantedPolicy('ShopManagement.CustomerLedger');
  readonly canCreateSaleReturn = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.Create');

  id!: string;
  dto?: ShopSaleDto;
  loading = false;
  actionInProgress = false;

  settings?: ShopSettingDto;
  printing = false;
  printLangModalOpen = false;
  printLang: 'en' | 'ur' = 'en';

  totalPaidAmount?: number;
  currentPendingAmount?: number;
  paymentStatus: 'Unpaid' | 'PartiallyPaid' | 'Paid' = 'Unpaid';

  returnAmount = 0;
  refundAmount = 0;
  creditAmount = 0;
  hasReturnableItems = false;

  cancelModalOpen = false;
  readonly cancelForm = this.fb.group({ cancellationReason: ['', [Validators.required, Validators.maxLength(500)]] });

  completeModalOpen = false;
  completeBatchItems: CompleteBatchItem[] = [];
  products: ShopSaleProductLookupDto[] = [];

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .get(this.id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        this.dto = dto;
        if (dto.status === ShopSaleStatus.Completed && dto.customerId) {
          this.loadPaymentSummary(dto);
          this.checkReturnableItems();
        }
      });
  }

  private loadPaymentSummary(sale: ShopSaleDto): void {
    this.customerPaymentService.getOutstandingSales(sale.customerId!).subscribe(result => {
      const row = (result.items || []).find(x => x.saleId === this.id);
      const grandTotal = sale.grandTotal ?? 0;
      const totalPaid = row?.totalPaidAmount ?? grandTotal;
      const paymentOnlyPending = row?.pendingAmount ?? 0;
      this.totalPaidAmount = totalPaid;

      this.saleReturnService.getList({ saleId: this.id, status: ShopSaleReturnStatus.Completed, maxResultCount: 1000 }).subscribe(returns => {
        const rows = returns.items || [];
        this.returnAmount = rows.reduce((sum, x) => sum + (x.grandTotal ?? 0), 0);
        this.refundAmount = rows.reduce((sum, x) => sum + (x.refundAmount ?? 0), 0);
        this.creditAmount = rows.reduce((sum, x) => sum + (x.customerCreditAmount ?? 0), 0);

        const pending = Math.max(0, paymentOnlyPending - this.returnAmount);
        this.currentPendingAmount = pending;
        this.paymentStatus = pending <= 0 ? 'Paid' : (totalPaid > 0 || this.returnAmount > 0 ? 'PartiallyPaid' : 'Unpaid');
      });
    });
  }

  private checkReturnableItems(): void {
    this.saleReturnService.getSaleForReturn(this.id).subscribe({
      next: result => (this.hasReturnableItems = (result.items || []).length > 0),
      error: () => (this.hasReturnableItems = false),
    });
  }

  receiveCustomerPayment(): void {
    this.router.navigate(['/shop-management/customer-payments/create'], { queryParams: { saleId: this.id } });
  }

  createSaleReturn(): void {
    this.router.navigate(['/shop-management/sale-returns/create', this.id]);
  }

  viewCustomerLedger(): void {
    if (this.dto?.customerId) this.router.navigate(['/shop-management/customer-ledger', this.dto.customerId]);
  }

  openPrintLanguageDialog(): void {
    if (this.printing) return;
    this.printLangModalOpen = true;
  }

  selectPrintLanguage(lang: 'en' | 'ur'): void {
    this.printLang = lang;
    this.printLangModalOpen = false;

    if (this.settings) {
      this.triggerPrint();
      return;
    }

    this.printing = true;
    this.settingService
      .get()
      .pipe(finalize(() => (this.printing = false)))
      .subscribe({
        next: settings => {
          this.settings = settings;
          this.triggerPrint();
        },
        error: e => this.showError(e),
      });
  }

  private triggerPrint(): void {
    // Deferred one tick so the *ngIf="settings" invoice block has actually rendered before print grabs the DOM.
    setTimeout(() => window.print());
  }

  private readonly invoiceLabelsByLang = {
    en: {
      invoice: 'Invoice',
      product: 'Product',
      quantity: 'Quantity',
      unitPrice: 'Unit Price',
      discount: 'Discount',
      tax: 'Tax',
      lineTotal: 'Line Total',
      subTotal: 'Sub Total',
      discountAmount: 'Discount Amount',
      taxAmount: 'Tax Amount',
      otherCharges: 'Other Charges',
      grandTotal: 'Grand Total',
      totalPaid: 'Total Paid',
      pendingAmount: 'Pending Amount',
    },
    ur: {
      invoice: 'انوائس',
      product: 'پروڈکٹ',
      quantity: 'مقدار',
      unitPrice: 'یونٹ قیمت',
      discount: 'رعایت',
      tax: 'ٹیکس',
      lineTotal: 'کل رقم',
      subTotal: 'ذیلی کل',
      discountAmount: 'رعایت کی رقم',
      taxAmount: 'ٹیکس کی رقم',
      otherCharges: 'دیگر اخراجات',
      grandTotal: 'مجموعی کل',
      totalPaid: 'کل ادا شدہ',
      pendingAmount: 'بقایا رقم',
    },
  };

  get invoiceLabels() {
    return this.invoiceLabelsByLang[this.printLang];
  }

  get shopAddress(): string {
    const s = this.settings;
    if (!s) return '';
    return [s.addressLine1, s.addressLine2, s.city, s.stateOrProvince, s.postalCode, s.country]
      .filter(part => !!part && part.trim().length > 0)
      .join(', ');
  }

  statusLabel(status: ShopSaleStatus): string {
    return '::' + ShopSaleStatus[status];
  }

  paymentMethodLabel(method: ShopSalePaymentMethod): string {
    return '::' + ShopSalePaymentMethod[method];
  }

  statusClass(status: ShopSaleStatus): string {
    switch (status) {
      case ShopSaleStatus.Draft: return 'draft';
      case ShopSaleStatus.Completed: return 'completed';
      case ShopSaleStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  edit(): void {
    this.router.navigate(['/shop-management/sales', this.id, 'edit']);
  }

  back(): void {
    this.router.navigate(['/shop-management/sales']);
  }

  viewStockTransactions(): void {
    this.router.navigate(['/shop-management/stock-transactions'], { queryParams: { referenceNumber: this.dto!.saleNumber } });
  }

  remove(): void {
    this.confirmation.warn('::ConfirmDeleteSale', this.dto!.saleNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(this.id).subscribe({
        next: () => {
          this.toaster.success('::SaleDeletedSuccessfully');
          this.back();
        },
        error: e => this.showError(e),
      });
    });
  }

  complete(): void {
    this.service.getSaleProductLookup().subscribe(result => {
      this.products = result.items || [];
      const batchTrackedItems = (this.dto!.items || []).filter(item => this.products.find(p => p.id === item.productId)?.trackBatch);

      if (batchTrackedItems.length === 0) {
        this.confirmAndComplete({ itemBatchAllocations: [] });
        return;
      }

      this.completeBatchItems = batchTrackedItems.map(item => ({
        saleItemId: item.id,
        productId: item.productId!,
        productName: item.productName!,
        unitShortName: item.unitShortName,
        quantity: item.quantity,
        mode: 'auto',
        loadingBatches: false,
        batches: [],
        allocations: [],
      }));
      this.completeModalOpen = true;
    });
  }

  onAllocationModeChange(row: CompleteBatchItem): void {
    if (row.mode === 'manual' && row.batches.length === 0) {
      row.loadingBatches = true;
      this.productBatchService
        .getAvailableBatches(row.productId)
        .pipe(finalize(() => (row.loadingBatches = false)))
        .subscribe(result => (row.batches = result.items || []));
    }
  }

  toggleBatchAllocation(row: CompleteBatchItem, batch: ShopProductBatchLookupDto, checked: boolean): void {
    if (checked) row.allocations.push({ productBatchId: batch.id, quantity: null });
    else row.allocations = row.allocations.filter(x => x.productBatchId !== batch.id);
  }

  isBatchSelected(row: CompleteBatchItem, batchId: string): boolean {
    return row.allocations.some(x => x.productBatchId === batchId);
  }

  allocationFor(row: CompleteBatchItem, batchId: string): CompleteBatchAllocationRow | undefined {
    return row.allocations.find(x => x.productBatchId === batchId);
  }

  allocatedQuantity(row: CompleteBatchItem): number {
    return row.allocations.reduce((sum, x) => sum + (x.quantity || 0), 0);
  }

  remainingQuantity(row: CompleteBatchItem): number {
    return row.quantity - this.allocatedQuantity(row);
  }

  get completeModalInvalid(): boolean {
    return this.completeBatchItems.some(row => row.mode === 'manual' && (row.allocations.length === 0 || this.allocatedQuantity(row) !== row.quantity));
  }

  confirmCompleteModal(): void {
    if (this.completeModalInvalid) return;

    const input: CompleteShopSaleDto = {
      itemBatchAllocations: this.completeBatchItems
        .filter(row => row.mode === 'manual')
        .map(row => ({
          saleItemId: row.saleItemId,
          allocations: row.allocations.map(a => ({ productBatchId: a.productBatchId, quantity: a.quantity || 0 })),
        })),
    };

    this.completeModalOpen = false;
    this.confirmAndComplete(input);
  }

  private confirmAndComplete(input: CompleteShopSaleDto): void {
    this.confirmation.warn('::ConfirmCompleteSale', this.dto!.saleNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .complete(this.id, input)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: dto => {
            this.dto = dto;
            this.toaster.success('::SaleCompletedSuccessfully');
          },
          error: e => this.showError(e),
        });
    });
  }

  openCancel(): void {
    this.cancelForm.reset();
    this.cancelModalOpen = true;
  }

  confirmCancel(): void {
    this.cancelForm.markAllAsTouched();
    if (this.cancelForm.invalid) return;
    this.actionInProgress = true;
    this.service
      .cancel(this.id, this.cancelForm.getRawValue() as { cancellationReason: string })
      .pipe(finalize(() => (this.actionInProgress = false)))
      .subscribe({
        next: dto => {
          this.dto = dto;
          this.cancelModalOpen = false;
          this.toaster.success('::SaleCancelledSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}

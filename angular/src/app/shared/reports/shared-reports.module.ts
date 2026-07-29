import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { NzPaginationModule } from 'ng-zorro-antd/pagination';
import { SharedModule } from '../shared.module';
import { TopbarLayoutModule } from '../../components/topbar-layout/topbar-layout.module';
import { ReportPeriodFilterComponent } from './report-period-filter.component';
import { ReportExportMenuComponent } from './report-export-menu.component';
import { ReportTotalsCardsComponent } from './report-totals-cards.component';
import { ReportEmptyStateComponent } from './report-empty-state.component';
import { ReportStatusBadgeComponent } from './report-status-badge.component';
import { ReportPrintHeaderComponent } from './report-print-header.component';
import { ShopReportCurrencyPipe } from './shop-report-currency.pipe';
import { ShopReportQuantityPipe } from './shop-report-quantity.pipe';

@NgModule({
  declarations: [
    ReportPeriodFilterComponent,
    ReportExportMenuComponent,
    ReportTotalsCardsComponent,
    ReportEmptyStateComponent,
    ReportStatusBadgeComponent,
    ReportPrintHeaderComponent,
    ShopReportCurrencyPipe,
    ShopReportQuantityPipe,
  ],
  imports: [CommonModule, FormsModule, SharedModule, PageModule, NzPaginationModule, TopbarLayoutModule],
  exports: [
    CommonModule,
    FormsModule,
    SharedModule,
    PageModule,
    NzPaginationModule,
    TopbarLayoutModule,
    ReportPeriodFilterComponent,
    ReportExportMenuComponent,
    ReportTotalsCardsComponent,
    ReportEmptyStateComponent,
    ReportStatusBadgeComponent,
    ReportPrintHeaderComponent,
    ShopReportCurrencyPipe,
    ShopReportQuantityPipe,
  ],
})
export class SharedReportsModule {}

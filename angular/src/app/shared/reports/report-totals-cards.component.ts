import { Component, Input } from '@angular/core';

export interface ReportTotalCard {
  label: string;
  value: string;
  icon?: string;
  colorClass?: 'primary' | 'success' | 'info' | 'warning' | 'danger';
}

@Component({
  selector: 'app-report-totals-cards',
  standalone: false,
  templateUrl: './report-totals-cards.component.html',
  styleUrl: './report-totals-cards.component.scss',
})
export class ReportTotalsCardsComponent {
  @Input() cards: ReportTotalCard[] = [];
}

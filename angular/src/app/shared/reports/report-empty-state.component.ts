import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-report-empty-state',
  standalone: false,
  templateUrl: './report-empty-state.component.html',
  styleUrl: './report-empty-state.component.scss',
})
export class ReportEmptyStateComponent {
  @Input() messageKey = '::NoReportData';
}

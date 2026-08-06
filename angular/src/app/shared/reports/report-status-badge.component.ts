import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-report-status-badge',
  standalone: false,
  templateUrl: './report-status-badge.component.html',
  styleUrl: './report-status-badge.component.scss',
})
export class ReportStatusBadgeComponent {
  @Input() text = '';
  @Input() variant: 'success' | 'warning' | 'danger' | 'info' | 'secondary' = 'secondary';
}

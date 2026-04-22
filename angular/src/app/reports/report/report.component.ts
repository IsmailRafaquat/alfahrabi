import { ToasterService } from '@abp/ng.theme.shared';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

interface ReportItem {
  title: string;
  icon: string;
  route: string;
  colorClass: string;
}

@Component({
  selector: 'app-report',
  standalone: false,
  templateUrl: './report.component.html',
  styleUrl: './report.component.scss',
})
export class ReportComponent {
  private readonly router = inject(Router);
  constructor(private toaster: ToasterService) {}

  showInProgress(reportName: string) {
    this.toaster.info(`${reportName} report is in progress.`);
  }

   openExpenseReport() {
    this.router.navigate(['reports/report/expense-report']);
  }

  openSalaryReport() {
    this.router.navigate(['reports/report/salary-report']);
  }
}

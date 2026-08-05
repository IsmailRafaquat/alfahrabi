import { AuthService } from '@abp/ng.core';
import { Component, inject } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-root',
  template: `
    <abp-loader-bar></abp-loader-bar>
    <abp-dynamic-layout></abp-dynamic-layout>
    <app-system-guide-button *ngIf="authService.isAuthenticated"></app-system-guide-button>
  `,
})
export class AppComponent {
  protected readonly authService = inject(AuthService);
}

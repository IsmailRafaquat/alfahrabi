import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService, ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ShopNotificationService } from '../../proxy/shop-management/notifications/shop-notification.service';
import {
  GetShopNotificationsInput,
  ShopNotificationDto,
  ShopNotificationSettingsDto,
  UpdateShopNotificationSettingsDto,
} from '../../proxy/shop-management/notifications/models';
import { ShopNotificationType, shopNotificationTypeOptions } from '../../proxy/shop-management/notifications/shop-notification-type.enum';
import { ShopNotificationSeverity, shopNotificationSeverityOptions } from '../../proxy/shop-management/notifications/shop-notification-severity.enum';
import { ShopNotificationStatus, shopNotificationStatusOptions } from '../../proxy/shop-management/notifications/shop-notification-status.enum';

@Component({
  selector: 'app-shop-notification-center',
  standalone: false,
  templateUrl: './shop-notification-center.component.html',
  styleUrl: './shop-notification-center.component.scss',
})
export class ShopNotificationCenterComponent implements OnInit {
  private readonly service = inject(ShopNotificationService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly router = inject(Router);

  readonly ShopNotificationSeverity = ShopNotificationSeverity;
  readonly ShopNotificationStatus = ShopNotificationStatus;
  readonly typeOptions = shopNotificationTypeOptions;
  readonly severityOptions = shopNotificationSeverityOptions;
  readonly statusOptions = shopNotificationStatusOptions;

  readonly canMarkRead = this.permissions.getGrantedPolicy('ShopManagement.Notifications.MarkRead');
  readonly canDismiss = this.permissions.getGrantedPolicy('ShopManagement.Notifications.Dismiss');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.Notifications.Delete');
  readonly canManageSettings = this.permissions.getGrantedPolicy('ShopManagement.Notifications.ManageSettings');
  readonly canGenerateNow = this.permissions.getGrantedPolicy('ShopManagement.Notifications.GenerateNow');

  input: GetShopNotificationsInput = {
    unreadOnly: false,
    sorting: undefined,
    skipCount: 0,
    maxResultCount: 20,
  };

  items: ShopNotificationDto[] = [];
  totalCount = 0;
  pageIndex = 1;
  loading = false;
  generating = false;

  showSettings = false;
  settings?: ShopNotificationSettingsDto;
  settingsForm?: UpdateShopNotificationSettingsDto;
  savingSettings = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .getList(this.input)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: r => {
          this.items = r.items || [];
          this.totalCount = r.totalCount;
        },
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }

  onFilterChange(): void {
    this.input.skipCount = 0;
    this.pageIndex = 1;
    this.load();
  }

  onPageChange(page: number): void {
    this.pageIndex = page;
    this.input.skipCount = (page - 1) * (this.input.maxResultCount || 20);
    this.load();
  }

  markAsRead(item: ShopNotificationDto): void {
    this.service.markAsRead(item.id).subscribe(() => {
      item.status = ShopNotificationStatus.Read;
      item.isUnread = false;
    });
  }

  markAsUnread(item: ShopNotificationDto): void {
    this.service.markAsUnread(item.id).subscribe(() => {
      item.status = ShopNotificationStatus.Unread;
      item.isUnread = true;
    });
  }

  markAllAsRead(): void {
    this.service.markAllAsRead().subscribe(() => {
      this.toaster.success('::NotificationMarkedRead');
      this.load();
    });
  }

  dismiss(item: ShopNotificationDto): void {
    this.service.dismiss(item.id).subscribe(() => {
      this.toaster.success('::NotificationDismissed');
      this.load();
    });
  }

  dismissAll(): void {
    this.confirmation.warn('::DismissAllNotificationsConfirmMessage', '::DismissAll').subscribe((status: Confirmation.Status) => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.dismissAll().subscribe(() => {
        this.toaster.success('::NotificationDismissed');
        this.load();
      });
    });
  }

  deleteItem(item: ShopNotificationDto): void {
    this.confirmation.warn('::DeleteNotificationConfirmMessage', item.title).subscribe((status: Confirmation.Status) => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(item.id).subscribe(() => this.load());
    });
  }

  navigate(item: ShopNotificationDto): void {
    if (!this.canMarkRead) return;
    if (item.isUnread) this.markAsRead(item);
    if (item.navigationUrl) this.router.navigateByUrl(item.navigationUrl);
  }

  generateNow(): void {
    this.generating = true;
    this.service
      .generateNow()
      .pipe(finalize(() => (this.generating = false)))
      .subscribe({
        next: () => {
          this.toaster.success('::NotificationGenerationCompleted');
          this.load();
        },
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }

  toggleSettings(): void {
    this.showSettings = !this.showSettings;
    if (this.showSettings && !this.settings) {
      this.service.getSettings().subscribe(s => {
        this.settings = s;
        this.settingsForm = { ...s };
      });
    }
  }

  saveSettings(): void {
    if (!this.settingsForm) return;
    this.savingSettings = true;
    this.service
      .updateSettings(this.settingsForm)
      .pipe(finalize(() => (this.savingSettings = false)))
      .subscribe({
        next: () => this.toaster.success('::NotificationSettingsUpdated'),
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }

  severityVariant(severity: ShopNotificationSeverity): 'success' | 'warning' | 'danger' | 'info' | 'secondary' {
    switch (severity) {
      case ShopNotificationSeverity.Critical: return 'danger';
      case ShopNotificationSeverity.Warning: return 'warning';
      case ShopNotificationSeverity.Success: return 'success';
      default: return 'info';
    }
  }

  severityIcon(severity: ShopNotificationSeverity): string {
    switch (severity) {
      case ShopNotificationSeverity.Critical: return 'fas fa-exclamation-circle';
      case ShopNotificationSeverity.Warning: return 'fas fa-exclamation-triangle';
      case ShopNotificationSeverity.Success: return 'fas fa-check-circle';
      default: return 'fas fa-info-circle';
    }
  }

  typeLabel(type: ShopNotificationType): string {
    return '::' + ShopNotificationType[type];
  }

  timeAgo(dateStr: string): string {
    const date = new Date(dateStr).getTime();
    const seconds = Math.floor((Date.now() - date) / 1000);
    if (seconds < 60) return 'just now';
    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return `${minutes}m ago`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours}h ago`;
    const days = Math.floor(hours / 24);
    return `${days}d ago`;
  }
}

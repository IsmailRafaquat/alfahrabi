import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, interval, startWith, switchMap, takeUntil, catchError, of } from 'rxjs';
import { ShopNotificationService } from '../../proxy/shop-management/notifications/shop-notification.service';
import { ShopNotificationDto, ShopNotificationSummaryDto, GetShopNotificationsInput } from '../../proxy/shop-management/notifications/models';
import { ShopNotificationSeverity } from '../../proxy/shop-management/notifications/shop-notification-severity.enum';

const POLL_INTERVAL_MS = 60000;

@Component({
  selector: 'app-shop-notification-bell',
  standalone: false,
  templateUrl: './shop-notification-bell.component.html',
  styleUrl: './shop-notification-bell.component.scss',
})
export class ShopNotificationBellComponent implements OnInit, OnDestroy {
  private readonly service = inject(ShopNotificationService);
  private readonly router = inject(Router);
  private readonly destroyed$ = new Subject<void>();
  private pollingInFlight = false;

  readonly ShopNotificationSeverity = ShopNotificationSeverity;

  summary?: ShopNotificationSummaryDto;
  latest: ShopNotificationDto[] = [];
  loadingLatest = false;
  open = false;

  ngOnInit(): void {
    interval(POLL_INTERVAL_MS)
      .pipe(
        startWith(0),
        switchMap(() => {
          if (this.pollingInFlight) return of(null);
          this.pollingInFlight = true;
          return this.service.getSummary().pipe(
            catchError(() => of(undefined)),
          );
        }),
        takeUntil(this.destroyed$),
      )
      .subscribe(summary => {
        this.pollingInFlight = false;
        if (summary) this.summary = summary;
      });
  }

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  toggle(): void {
    this.open = !this.open;
    if (this.open) this.loadLatest();
  }

  close(): void {
    this.open = false;
  }

  private loadLatest(): void {
    this.loadingLatest = true;
    const input: GetShopNotificationsInput = { skipCount: 0, maxResultCount: 8, sorting: undefined, unreadOnly: false };
    this.service.getList(input).subscribe({
      next: r => {
        this.latest = r.items || [];
        this.loadingLatest = false;
      },
      error: () => (this.loadingLatest = false),
    });
  }

  markAsRead(item: ShopNotificationDto, event: Event): void {
    event.stopPropagation();
    this.service.markAsRead(item.id).subscribe(() => {
      item.isUnread = false;
      this.service.getSummary().subscribe(s => (this.summary = s));
    });
  }

  select(item: ShopNotificationDto): void {
    if (item.isUnread) this.markAsRead(item, new Event('click'));
    this.close();
    if (item.navigationUrl) this.router.navigateByUrl(item.navigationUrl);
  }

  viewAll(): void {
    this.close();
    this.router.navigateByUrl('/shop-management/notifications');
  }

  severityIcon(severity: ShopNotificationSeverity): string {
    switch (severity) {
      case ShopNotificationSeverity.Critical: return 'fas fa-exclamation-circle';
      case ShopNotificationSeverity.Warning: return 'fas fa-exclamation-triangle';
      case ShopNotificationSeverity.Success: return 'fas fa-check-circle';
      default: return 'fas fa-info-circle';
    }
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

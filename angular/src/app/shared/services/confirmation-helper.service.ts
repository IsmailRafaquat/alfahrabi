import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export const DELETE_CONFIRMATION_OPTIONS: Confirmation.Options = {
  yesText: '::Button:ConfirmDelete', // ✅ FIX (2 colons)
  cancelText: '::Button:Cancel',
  icon: 'fas fa-trash text-danger',
} as const;


@Injectable({ providedIn: 'root' })
export class ConfirmationHelperService {
  constructor(private confirmation: ConfirmationService) {}

  /**
   * Shortcut for delete confirmation
   */
  confirmDelete(
    messageKey = '::DeleteConfirmationMessage',
    titleKey = '::DeleteConfirmationTitle',
  ): Observable<Confirmation.Status> {
    return this.confirmation.warn(messageKey, titleKey, DELETE_CONFIRMATION_OPTIONS);
  }

}

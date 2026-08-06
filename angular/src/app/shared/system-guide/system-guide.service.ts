import { Injectable, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { GuideLanguage, ModuleGuide } from './system-guide.models';
import { SYSTEM_GUIDE_MODULES } from './system-guide.data';

const LANGUAGE_STORAGE_KEY = 'ehub.systemGuide.language';

/**
 * All guide content is static (system-guide.data.ts) - this service never calls an API. Permission
 * filtering reuses PermissionService.getGrantedPolicy, the same check route.provider.ts's
 * requiredPolicy relies on for the sidebar, so a module only ever appears here if it would also
 * appear in the menu.
 */
@Injectable({ providedIn: 'root' })
export class SystemGuideService {
  private readonly permissions = inject(PermissionService);

  getAccessibleModules(): ModuleGuide[] {
    return SYSTEM_GUIDE_MODULES.filter(
      m => !m.requiredPolicy || this.permissions.getGrantedPolicy(m.requiredPolicy),
    );
  }

  search(modules: ModuleGuide[], term: string): ModuleGuide[] {
    const trimmed = term.trim();
    if (!trimmed) return modules;
    const lower = trimmed.toLowerCase();
    return modules.filter(
      m =>
        m.name.en.toLowerCase().includes(lower) ||
        m.name.ur.includes(trimmed) ||
        m.shortDescription.en.toLowerCase().includes(lower) ||
        m.shortDescription.ur.includes(trimmed),
    );
  }

  loadLanguagePreference(): GuideLanguage {
    try {
      const stored = localStorage.getItem(LANGUAGE_STORAGE_KEY);
      if (stored === 'en' || stored === 'ur' || stored === 'both') return stored;
    } catch {
      // localStorage unavailable (e.g. private browsing) - fall through to the default.
    }
    return 'both';
  }

  saveLanguagePreference(language: GuideLanguage): void {
    try {
      localStorage.setItem(LANGUAGE_STORAGE_KEY, language);
    } catch {
      // Non-fatal: the preference just won't persist across reloads.
    }
  }
}

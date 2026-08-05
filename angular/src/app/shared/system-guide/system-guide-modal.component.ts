import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  OnDestroy,
  OnInit,
  Output,
  ViewChild,
  inject,
} from '@angular/core';
import { Router } from '@angular/router';
import { GuideLanguage, ModuleGuide } from './system-guide.models';
import { SystemGuideService } from './system-guide.service';

/** All content is static and permission-filtered once on open - no API calls happen anywhere in this component. */
@Component({
  selector: 'app-system-guide-modal',
  standalone: false,
  templateUrl: './system-guide-modal.component.html',
  styleUrl: './system-guide-modal.component.scss',
})
export class SystemGuideModalComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly guideService = inject(SystemGuideService);
  private readonly router = inject(Router);

  @Output() closed = new EventEmitter<void>();

  /** Section headings, per the guide's own EN/UR structure - distinct from the app's ABP locale, since these
   * follow the guide-language toggle inside the modal, not the current UI culture. */
  readonly sectionHeadings: Record<'en' | 'ur', Record<
    'purpose' | 'whenToUse' | 'requiredInfo' | 'workflow' | 'whatHappensNext' | 'related' | 'notes',
    string
  >> = {
    en: {
      purpose: 'What this module does',
      whenToUse: 'When to use it',
      requiredInfo: 'Information required',
      workflow: 'Step-by-step workflow',
      whatHappensNext: 'What happens next',
      related: 'Related modules',
      notes: 'Important notes',
    },
    ur: {
      purpose: 'یہ ماڈیول کیا کام کرتا ہے',
      whenToUse: 'اسے کب استعمال کریں',
      requiredInfo: 'کون سی معلومات درکار ہیں',
      workflow: 'مرحلہ وار طریقہ کار',
      whatHappensNext: 'اس کے بعد کیا ہوتا ہے',
      related: 'متعلقہ ماڈیولز',
      notes: 'اہم ہدایات',
    },
  };

  @ViewChild('dialog') dialogRef?: ElementRef<HTMLElement>;
  @ViewChild('searchInput') searchInputRef?: ElementRef<HTMLInputElement>;

  private previouslyFocusedElement: HTMLElement | null = null;

  modules: ModuleGuide[] = [];
  filteredModules: ModuleGuide[] = [];
  searchTerm = '';
  language: GuideLanguage = 'both';
  selectedModule?: ModuleGuide;
  /** Mobile only: true once a module is selected, to switch from the list view to the detail view. */
  showDetailOnMobile = false;

  ngOnInit(): void {
    this.modules = this.guideService.getAccessibleModules();
    this.filteredModules = this.modules;
    this.language = this.guideService.loadLanguagePreference();
    this.selectedModule = this.modules[0];
  }

  ngAfterViewInit(): void {
    this.previouslyFocusedElement = document.activeElement as HTMLElement | null;
    setTimeout(() => this.searchInputRef?.nativeElement.focus());
  }

  ngOnDestroy(): void {
    this.previouslyFocusedElement?.focus?.();
  }

  get displayLanguages(): Array<'en' | 'ur'> {
    return this.language === 'both' ? ['en', 'ur'] : [this.language];
  }

  onSearchChange(term: string): void {
    this.searchTerm = term;
    this.filteredModules = this.guideService.search(this.modules, term);
  }

  setLanguage(language: GuideLanguage): void {
    this.language = language;
    this.guideService.saveLanguagePreference(language);
  }

  selectModule(module: ModuleGuide): void {
    this.selectedModule = module;
    this.showDetailOnMobile = true;
  }

  relatedModuleName(key: string): string {
    const module = this.modules.find(m => m.key === key);
    if (!module) return key;
    return this.language === 'ur' ? module.name.ur : module.name.en;
  }

  selectModuleByKey(key: string): void {
    const module = this.modules.find(m => m.key === key);
    if (module) this.selectModule(module);
  }

  backToList(): void {
    this.showDetailOnMobile = false;
  }

  openModule(module: ModuleGuide): void {
    if (!module.route || module.comingSoon) return;
    const route = module.route;
    this.close();
    this.router.navigateByUrl(route);
  }

  close(): void {
    this.closed.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) this.close();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.close();
  }

  @HostListener('document:keydown.tab', ['$event'])
  onTab(event: KeyboardEvent): void {
    const dialog = this.dialogRef?.nativeElement;
    if (!dialog) return;

    const focusable = Array.from(
      dialog.querySelectorAll<HTMLElement>('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'),
    ).filter(el => !el.hasAttribute('disabled'));
    if (focusable.length === 0) return;

    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    const active = document.activeElement as HTMLElement | null;

    if (event.shiftKey && active === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && active === last) {
      event.preventDefault();
      first.focus();
    }
  }
}

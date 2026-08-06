/** 'both' shows English then Urdu stacked; 'en'/'ur' show only that language. */
export type GuideLanguage = 'en' | 'ur' | 'both';

export interface BilingualText {
  en: string;
  ur: string;
}

export interface BilingualList {
  en: string[];
  ur: string[];
}

/**
 * One entry per top-level Shop Management menu group (see route.provider.ts's shopManagementRoutes) -
 * the same granularity the module already uses for its sidebar, so the guide never drifts from what
 * a user actually sees in the menu.
 */
export interface ModuleGuide {
  /** Stable id, also used as the target of relatedModules[] references. */
  key: string;
  icon: string;
  /** Route the "Open Module" button navigates to. Omitted for entries with no navigable page of their own. */
  route?: string;
  /** ABP policy gating visibility - mirrors the same requiredPolicy used in route.provider.ts for this menu group. */
  requiredPolicy?: string;
  /** True only for functionality that does not exist yet - never invented, only reflects real gaps. */
  comingSoon?: boolean;

  name: BilingualText;
  shortDescription: BilingualText;
  purpose: BilingualText;
  whenToUse: BilingualText;
  requiredInformation: BilingualList;
  workflow: BilingualList;
  whatHappensNext: BilingualText;
  /** Keys of other ModuleGuide entries this one commonly connects to. */
  relatedModules: string[];
  importantNotes: BilingualList;
}

import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type ThemeMode = 'light' | 'dark' | 'system';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'data-bs-theme';
  private readonly attributeName = 'data-bs-theme';
  private readonly modeAttributeName = 'data-bs-theme-mode';
  private readonly defaultMode: ThemeMode = 'light';

  private readonly systemMediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
  private readonly modeSubject = new BehaviorSubject<ThemeMode>(this.resolveInitialMode());

  readonly mode$ = this.modeSubject.asObservable();

  constructor() {
    this.applyTheme(this.modeSubject.value);

    this.systemMediaQuery.addEventListener('change', () => {
      if (this.modeSubject.value === 'system') {
        this.applyTheme('system');
      }
    });
  }

  setMode(mode: ThemeMode) {
    if (this.modeSubject.value === mode) {
      return;
    }

    this.modeSubject.next(mode);
    this.persistMode(mode);
    this.applyTheme(mode);
  }

  private resolveInitialMode(): ThemeMode {
    const storedMode = localStorage.getItem(this.storageKey) as ThemeMode | null;

    if (storedMode === 'light' || storedMode === 'dark' || storedMode === 'system') {
      return storedMode;
    }

    return this.defaultMode;
  }

  private persistMode(mode: ThemeMode) {
    localStorage.setItem(this.storageKey, mode);
  }

  private applyTheme(mode: ThemeMode) {
    const resolvedTheme = mode === 'system' ? this.getSystemTheme() : mode;

    document.documentElement.setAttribute(this.attributeName, resolvedTheme);
    document.documentElement.setAttribute(this.modeAttributeName, mode);
  }

  private getSystemTheme(): 'light' | 'dark' {
    return this.systemMediaQuery.matches ? 'dark' : 'light';
  }
}

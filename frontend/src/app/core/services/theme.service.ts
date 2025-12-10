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
  private readonly modeSubject = new BehaviorSubject<ThemeMode>(this.defaultMode);
  private isInitialized = false;

  readonly mode$ = this.modeSubject.asObservable();

  initialize() {
    if (this.isInitialized) {
      return;
    }

    const initialMode = this.resolveInitialMode();

    this.modeSubject.next(initialMode);
    this.persistMode(initialMode);
    this.applyTheme(initialMode);

    this.systemMediaQuery.addEventListener('change', () => {
      if (this.modeSubject.value === 'system') {
        this.applyTheme('system');
      }
    });

    this.isInitialized = true;
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
    const modeAttribute = document.documentElement.getAttribute(this.modeAttributeName) as ThemeMode | null;
    const themeAttribute = document.documentElement.getAttribute(this.attributeName) as ThemeMode | null;

    if (this.isValidMode(storedMode)) {
      return storedMode;
    }

    if (this.isValidMode(modeAttribute)) {
      return modeAttribute;
    }

    if (themeAttribute === 'light' || themeAttribute === 'dark') {
      return themeAttribute;
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

    if (document.body) {
      document.body.setAttribute(this.attributeName, resolvedTheme);
      document.body.setAttribute(this.modeAttributeName, mode);
      document.body.classList.toggle('app-theme-dark', resolvedTheme === 'dark');
    }
  }

  private getSystemTheme(): 'light' | 'dark' {
    return this.systemMediaQuery.matches ? 'dark' : 'light';
  }

  private isValidMode(mode: string | null): mode is ThemeMode {
    return mode === 'light' || mode === 'dark' || mode === 'system';
  }
}

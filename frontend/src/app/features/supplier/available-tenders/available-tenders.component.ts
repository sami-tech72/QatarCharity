import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, finalize, Subject, takeUntil } from 'rxjs';

import { SupplierRfxService } from '../../../core/services/supplier-rfx.service';
import { SupplierRfx } from '../../../shared/models/supplier-rfx.model';

@Component({
  selector: 'app-available-tenders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './available-tenders.component.html',
  styleUrl: './available-tenders.component.scss',
})
export class AvailableTendersComponent implements OnInit, OnDestroy {
  searchTerm = '';
  tenders: SupplierRfx[] = [];
  loading = false;
  error?: string;

  private readonly destroy$ = new Subject<void>();
  private readonly search$ = new Subject<string>();

  constructor(private readonly supplierRfxService: SupplierRfxService) {}

  ngOnInit(): void {
    this.search$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((term) => this.loadTenders(term));

    this.loadTenders();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearch(term: string): void {
    this.search$.next(term);
  }

  refreshTenders(): void {
    this.loadTenders(this.searchTerm);
  }

  getRemainingDays(deadline: string): number {
    const date = new Date(deadline);
    const diff = date.getTime() - Date.now();

    if (Number.isNaN(date.getTime())) {
      return 0;
    }

    return Math.max(Math.ceil(diff / (1000 * 60 * 60 * 24)), 0);
  }

  formatBudget(tender: SupplierRfx): string {
    if (tender.hideBudget) {
      return 'Budget hidden';
    }

    const currency = tender.currency || 'USD';

    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency,
      maximumFractionDigits: 0,
    }).format(tender.estimatedBudget);
  }

  private loadTenders(search: string = this.searchTerm): void {
    this.loading = true;
    this.error = undefined;

    this.supplierRfxService
      .loadPublishedRfx({ pageNumber: 1, pageSize: 20, search })
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (result) => {
          this.searchTerm = search;
          this.tenders = result.items;
        },
        error: (err) => {
          this.tenders = [];
          this.error = err?.message ?? 'Unable to load available tenders.';
        },
      });
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, Subject, takeUntil } from 'rxjs';

import { SupplierRfxService } from '../../../core/services/supplier-rfx.service';
import { PagedResult } from '../../../shared/models/pagination.model';
import { SupplierBidSummary } from '../../../shared/models/bid-evaluation.model';

@Component({
  selector: 'app-my-bids',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './my-bids.component.html',
  styleUrl: './my-bids.component.scss',
})
export class MyBidsComponent implements OnInit, OnDestroy {
  readonly searchControl = new FormControl('', { nonNullable: true });

  bids: SupplierBidSummary[] = [];
  loading = false;

  private readonly destroy$ = new Subject<void>();

  constructor(private readonly supplierRfxService: SupplierRfxService) {}

  ngOnInit(): void {
    this.loadBids();

    this.searchControl.valueChanges.pipe(debounceTime(300), takeUntil(this.destroy$)).subscribe((term) => {
      this.loadBids(term);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  trackByBidId(_: number, bid: SupplierBidSummary): string {
    return bid.id;
  }

  get emptyMessage(): string {
    if (this.loading) {
      return 'Loading your bids...';
    }

    return this.searchControl.value ? 'No bids match your search.' : 'You have not submitted any bids yet.';
  }

  private loadBids(search?: string): void {
    this.loading = true;

    this.supplierRfxService
      .loadMyBids({ pageNumber: 1, pageSize: 50, search: search?.trim() })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: PagedResult<SupplierBidSummary>) => {
          this.bids = result.items;
          this.loading = false;
        },
        error: () => {
          this.bids = [];
          this.loading = false;
        },
      });
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Subject, debounceTime, takeUntil } from 'rxjs';

import { BidEvaluationService } from '../../../core/services/bid-evaluation.service';
import { AuthService } from '../../../core/services/auth.service';
import { PagedResult } from '../../../shared/models/pagination.model';
import { SupplierBidSummary } from '../../../shared/models/bid-evaluation.model';

@Component({
  selector: 'app-bid-evaluation',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './bid-evaluation.component.html',
  styleUrl: './bid-evaluation.component.scss',
})
export class BidEvaluationComponent implements OnInit, OnDestroy {
  readonly searchControl = new FormControl('', { nonNullable: true });

  bids: SupplierBidSummary[] = [];
  loading = false;
  isCommitteeUser = false;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly bidService: BidEvaluationService,
    private readonly auth: AuthService,
  ) {}

  ngOnInit(): void {
    this.isCommitteeUser = !!this.auth.currentSession()?.procurementRole;
    this.loadBids();

    this.searchControl.valueChanges.pipe(debounceTime(300), takeUntil(this.destroy$)).subscribe((term) => {
      this.loadBids(term);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get emptyStateMessage(): string {
    if (this.loading) {
      return 'Loading supplier bids...';
    }

    return this.searchControl.value
      ? 'No supplier bids match your search.'
      : this.isCommitteeUser
        ? 'No supplier bids are assigned to your committee yet.'
        : 'No supplier bids have been submitted yet.';
  }

  trackByBidId(_: number, bid: SupplierBidSummary): string {
    return bid.id;
  }

  private loadBids(search?: string): void {
    this.loading = true;
    this.bidService
      .loadSupplierBids({ pageNumber: 1, pageSize: 50, search: search?.trim() })
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

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
  reviewing = new Set<string>();
  statusTotals = {
    total: 0,
    underReview: 0,
    accepted: 0,
    pending: 0,
  };

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

  getBidNumber(bid: SupplierBidSummary): string {
    return bid.rfxReferenceNumber || `BID-${bid.id.slice(0, 8).toUpperCase()}`;
  }

  getStatusDisplay(bid: SupplierBidSummary): { label: string; cssClass: string } {
    const normalized = (bid.status || 'submitted').toLowerCase();

    switch (normalized) {
      case 'accepted':
        return { label: 'accepted', cssClass: 'badge-success' };
      case 'under_review':
      case 'review':
      case 'under review':
        return { label: 'under review', cssClass: 'badge-info' };
      case 'pending':
      case 'pending_review':
      case 'pending review':
        return { label: 'pending review', cssClass: 'badge-warning' };
      default:
        return { label: 'submitted', cssClass: 'badge-primary' };
    }
  }

  getReviewDecision(bid: SupplierBidSummary): string | null {
    const userId = this.auth.currentSession()?.userId;

    if (!bid.reviews?.length || !userId) {
      return null;
    }

    const reviewerEntry = bid.reviews.find((review) => review.reviewerUserId === userId);
    return reviewerEntry?.decision ?? null;
  }

  getReviewTotals(bid: SupplierBidSummary): { approved: number; rejected: number } {
    const totals = { approved: 0, rejected: 0 };

    bid.reviews?.forEach((review) => {
      if (review.decision === 'approved') {
        totals.approved += 1;
      } else if (review.decision === 'rejected') {
        totals.rejected += 1;
      }
    });

    return totals;
  }

  submitReview(bid: SupplierBidSummary, decision: 'approved' | 'rejected' | 'review'): void {
    if (this.reviewing.has(bid.id)) {
      return;
    }

    this.reviewing.add(bid.id);

    this.bidService
      .reviewBid(bid.id, decision)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updated) => {
          this.bids = this.bids.map((existing) => (existing.id === updated.id ? updated : existing));
          this.refreshStatusTotals();
        },
        error: () => {
          this.reviewing.delete(bid.id);
        },
        complete: () => {
          this.reviewing.delete(bid.id);
        },
      });
  }

  private loadBids(search?: string): void {
    this.loading = true;
    this.bidService
      .loadSupplierBids({ pageNumber: 1, pageSize: 50, search: search?.trim() })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: PagedResult<SupplierBidSummary>) => {
          this.bids = result.items;
          this.refreshStatusTotals();
          this.loading = false;
        },
        error: () => {
          this.bids = [];
          this.refreshStatusTotals();
          this.loading = false;
        },
      });
  }

  private refreshStatusTotals(): void {
    const totals = {
      total: this.bids.length,
      underReview: 0,
      accepted: 0,
      pending: 0,
    };

    this.bids.forEach((bid) => {
      const status = (bid.status || 'submitted').toLowerCase();

      if (status === 'accepted') {
        totals.accepted += 1;
      } else if (status === 'under_review' || status === 'under review' || status === 'review') {
        totals.underReview += 1;
      } else {
        totals.pending += 1;
      }
    });

    this.statusTotals = totals;
  }
}

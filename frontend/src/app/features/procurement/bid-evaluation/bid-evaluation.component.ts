import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, debounceTime, takeUntil } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { BidEvaluationService } from '../../../core/services/bid-evaluation.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PagedResult } from '../../../shared/models/pagination.model';
import { BidReview, EvaluateBidRequest, SupplierBidEvaluation } from '../../../shared/models/bid-evaluation.model';

@Component({
  selector: 'app-bid-evaluation',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './bid-evaluation.component.html',
  styleUrls: ['./bid-evaluation.component.scss'],
})
export class BidEvaluationComponent implements OnInit, OnDestroy {
  statuses: string[] = [];
  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly reviewForm = new FormGroup({
    status: new FormControl<string>('Under Review', { nonNullable: true, validators: [Validators.required] }),
    reviewNotes: new FormControl<string>('', { nonNullable: true }),
  });

  bids: SupplierBidEvaluation[] = [];
  selectedBid: SupplierBidEvaluation | null = null;
  loading = false;
  submitting = false;
  total = 0;
  reviewModalOpen = false;
  summary = {
    total: 0,
    pendingReview: 0,
    underReview: 0,
    approved: 0,
  };
  canApprove = false;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly authService: AuthService,
    private readonly bidEvaluationService: BidEvaluationService,
    private readonly notification: NotificationService,
  ) {}

  ngOnInit(): void {
    this.canApprove = this.isProcurementApprover();
    this.statuses = this.buildStatuses();
    this.loadBids();

    this.searchControl.valueChanges.pipe(debounceTime(300), takeUntil(this.destroy$)).subscribe((term) => {
      this.loadBids(term);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  trackByBidId(_: number, bid: SupplierBidEvaluation): string {
    return bid.id;
  }

  trackByReview(_: number, review: BidReview): string {
    return `${review.reviewerName}-${review.reviewedAtUtc}`;
  }

  statusBadgeClass(status: string): string {
    switch (status) {
      case 'Approved':
        return 'badge-light-success';
      case 'Recommended':
      case 'Under Review':
        return 'badge-light-info';
      case 'Needs Clarification':
        return 'badge-light-warning';
      case 'Rejected':
        return 'badge-light-danger';
      default:
        return 'badge-light-secondary';
    }
  }

  selectBid(bid: SupplierBidEvaluation): void {
    this.selectedBid = bid;
    this.statuses = this.buildStatuses(bid.evaluationStatus);
    this.reviewForm.setValue({
      status: bid.evaluationStatus || 'Under Review',
      reviewNotes: bid.evaluationNotes ?? '',
    });
  }

  openReviewModal(bid: SupplierBidEvaluation): void {
    this.selectBid(bid);
    this.reviewModalOpen = true;
  }

  closeReviewModal(): void {
    this.reviewModalOpen = false;
  }

  submitReview(): void {
    if (!this.selectedBid || this.reviewForm.invalid) {
      return;
    }

    const payload: EvaluateBidRequest = {
      status: this.reviewForm.controls.status.value,
      reviewNotes: this.reviewForm.controls.reviewNotes.value,
    };

    this.submitting = true;
    this.bidEvaluationService
      .evaluateBid(this.selectedBid.rfxId, this.selectedBid.id, payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updated) => {
          this.selectedBid = updated;
          this.bids = this.bids.map((bid) => (bid.id === updated.id ? updated : bid));
          this.updateSummary();
          this.notification.success('Bid review saved successfully.');
          this.reviewModalOpen = false;
          this.submitting = false;
        },
        error: (error) => {
          this.notification.error(error.message || 'Unable to save bid review.');
          this.submitting = false;
        },
      });
  }

  private loadBids(search?: string): void {
    this.loading = true;
    this.bidEvaluationService
      .loadBids({ pageNumber: 1, pageSize: 50, search: search?.trim() })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: PagedResult<SupplierBidEvaluation>) => {
          this.bids = result.items.map((bid) => ({
            ...bid,
            reviews: (bid.reviews || []).sort(
              (a, b) => new Date(b.reviewedAtUtc).getTime() - new Date(a.reviewedAtUtc).getTime(),
            ),
            evaluationStatus: bid.evaluationStatus || 'Pending Review',
          }));
          this.total = result.totalCount;
          this.updateSummary();
          this.loading = false;
          if (this.bids.length > 0) {
            this.selectBid(this.bids[0]);
          } else {
            this.selectedBid = null;
          }
        },
        error: (error) => {
          this.notification.error(error.message || 'Unable to load bids.');
          this.bids = [];
          this.total = 0;
          this.updateSummary();
          this.loading = false;
          this.selectedBid = null;
        },
      });
  }

  private updateSummary(): void {
    const summary = this.bids.reduce(
      (acc, bid) => {
        const latestStatus = this.deriveBidStatus(bid);
        acc.total += 1;

        if (latestStatus === 'Pending Review') {
          acc.pendingReview += 1;
        } else if (latestStatus === 'Under Review' || latestStatus === 'Needs Clarification') {
          acc.underReview += 1;
        } else if (latestStatus === 'Approved' || latestStatus === 'Recommended') {
          acc.approved += 1;
        }

        return acc;
      },
      { total: 0, pendingReview: 0, underReview: 0, approved: 0 },
    );

    this.summary = {
      ...summary,
      total: this.total || summary.total,
    };
  }

  private deriveBidStatus(bid: SupplierBidEvaluation): string {
    if (!bid.reviews || bid.reviews.length === 0) {
      return 'Pending Review';
    }

    return bid.reviews[0].status || 'Pending Review';
  }

  private buildStatuses(currentStatus?: string | null): string[] {
    const allStatuses = ['Pending Review', 'Under Review', 'Recommended', 'Approved', 'Rejected', 'Needs Clarification'];

    if (this.canApprove || currentStatus === 'Approved') {
      return allStatuses;
    }

    return allStatuses.filter((status) => status !== 'Approved');
  }

  private isProcurementApprover(): boolean {
    const session = this.authService.currentSession();

    return session?.role === 'Procurement' && !session.procurementRole;
  }
}

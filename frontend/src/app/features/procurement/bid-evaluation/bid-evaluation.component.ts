import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, debounceTime, takeUntil } from 'rxjs';

import { BidEvaluationService } from '../../../core/services/bid-evaluation.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PagedResult } from '../../../shared/models/pagination.model';
import { EvaluateBidRequest, SupplierBidEvaluation } from '../../../shared/models/bid-evaluation.model';

@Component({
  selector: 'app-bid-evaluation',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './bid-evaluation.component.html',
  styleUrl: './bid-evaluation.component.scss',
})
export class BidEvaluationComponent implements OnInit, OnDestroy {
  readonly statuses = ['Pending Review', 'Under Review', 'Recommended', 'Approved', 'Rejected', 'Needs Clarification'];
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

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly bidEvaluationService: BidEvaluationService,
    private readonly notification: NotificationService,
  ) {}

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

  trackByBidId(_: number, bid: SupplierBidEvaluation): string {
    return bid.id;
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
    this.reviewForm.setValue({
      status: bid.evaluationStatus || 'Under Review',
      reviewNotes: bid.evaluationNotes ?? '',
    });
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
          this.notification.success('Bid review saved successfully.');
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
          this.bids = result.items;
          this.total = result.totalCount;
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
          this.loading = false;
          this.selectedBid = null;
        },
      });
  }
}

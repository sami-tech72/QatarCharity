import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { debounceTime, distinctUntilChanged, finalize, Subject, takeUntil } from 'rxjs';

import { SupplierRfxService } from '../../../core/services/supplier-rfx.service';
import {
  BidDocumentSubmission,
  BidInputSubmission,
  SupplierBidRequest,
  SupplierRfx,
} from '../../../shared/models/supplier-rfx.model';

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
  selectedTender?: SupplierRfx;
  bidSubmitting = false;
  bidSuccess?: string;
  bidError?: string;
  bidRequest: SupplierBidRequest = this.createBidRequest();

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

  viewDetails(tender: SupplierRfx): void {
    const hasChanged = this.selectedTender?.id !== tender.id;
    this.selectedTender = tender;
    this.bidSuccess = undefined;
    this.bidError = undefined;

    if (hasChanged) {
      this.bidRequest = this.createBidRequest(tender);
    }
  }

  startBid(tender: SupplierRfx): void {
    this.viewDetails(tender);
    this.bidRequest = this.createBidRequest(tender);
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

  submitBid(form: NgForm): void {
    if (!this.selectedTender || form.invalid) {
      return;
    }

    this.bidSubmitting = true;
    this.bidError = undefined;
    this.bidSuccess = undefined;

    this.supplierRfxService
      .submitBid(this.selectedTender.id, this.bidRequest)
      .pipe(
        finalize(() => {
          this.bidSubmitting = false;
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (message) => {
          this.bidSuccess = message || 'Bid submitted successfully.';
          this.bidRequest = this.createBidRequest(this.selectedTender);
          form.resetForm(this.bidRequest);
        },
        error: (err) => {
          this.bidError = err?.message ?? 'Unable to submit your bid right now.';
        },
      });
  }

  trackByTenderId(_: number, tender: SupplierRfx): string {
    return tender.id;
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
          this.selectedTender =
            this.selectedTender &&
            result.items.find((tender) => tender.id === this.selectedTender?.id);
        },
        error: (err) => {
          this.tenders = [];
          this.error = err?.message ?? 'Unable to load available tenders.';
        },
      });
  }

  private createBidRequest(tender?: SupplierRfx): SupplierBidRequest {
    const documents: BidDocumentSubmission[] = tender
      ? (tender.requiredDocuments || []).map((name) => ({ name, value: '' }))
      : [];

    const inputs: BidInputSubmission[] = tender
      ? (tender.requiredInputs || []).map((name) => ({ name, value: '' }))
      : [];

    return {
      bidAmount: null,
      currency: tender?.currency || 'USD',
      expectedDeliveryDate: '',
      proposalSummary: '',
      notes: '',
      documents,
      inputs,
    };
  }
}

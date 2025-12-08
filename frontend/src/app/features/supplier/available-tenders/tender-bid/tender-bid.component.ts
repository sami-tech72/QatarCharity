import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { finalize, Subject, takeUntil } from 'rxjs';

import { SupplierRfxService } from '../../../../core/services/supplier-rfx.service';
import {
  BidDocumentSubmission,
  BidInputSubmission,
  SupplierBidRequest,
  SupplierRfx,
} from '../../../../shared/models/supplier-rfx.model';

@Component({
  selector: 'app-tender-bid',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './tender-bid.component.html',
  styleUrl: './tender-bid.component.scss',
})
export class TenderBidComponent implements OnInit, OnDestroy {
  tender?: SupplierRfx;
  bidRequest: SupplierBidRequest = this.createBidRequest();
  loading = false;
  error?: string;
  bidSubmitting = false;
  bidSuccess?: string;
  bidError?: string;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly supplierRfxService: SupplierRfxService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.error = 'No tender ID was provided.';
      return;
    }

    this.loadTender(id);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  goBack(): void {
    this.router.navigate(['/supplier/available-tenders']);
  }

  submitBid(form: NgForm): void {
    if (!this.tender || form.invalid) {
      return;
    }

    this.bidSubmitting = true;
    this.bidError = undefined;
    this.bidSuccess = undefined;

    this.supplierRfxService
      .submitBid(this.tender.id, this.bidRequest)
      .pipe(
        finalize(() => (this.bidSubmitting = false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (message) => {
          this.bidSuccess = message || 'Bid submitted successfully.';
          this.bidRequest = this.createBidRequest(this.tender);
          form.resetForm(this.bidRequest);
        },
        error: (err) => {
          this.bidError = err?.message ?? 'Unable to submit your bid right now.';
        },
      });
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

  private loadTender(id: string): void {
    this.loading = true;
    this.error = undefined;

    this.supplierRfxService
      .getPublishedRfxById(id)
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (tender) => {
          this.tender = tender;
          this.bidRequest = this.createBidRequest(tender);
        },
        error: (err) => {
          this.error = err?.message ?? 'Unable to load this tender.';
        },
      });
  }

  private createBidRequest(tender?: SupplierRfx): SupplierBidRequest {
    const documents: BidDocumentSubmission[] = tender
      ? (tender.requiredDocuments || []).map((name) => ({ name, fileName: '', contentBase64: '' }))
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

  onDocumentSelected(event: Event, document: BidDocumentSubmission, form: NgForm): void {
    const target = event.target as HTMLInputElement;
    const file = target.files && target.files.length ? target.files[0] : undefined;

    document.fileName = '';
    document.contentBase64 = '';

    if (!file) {
      form?.control.markAsTouched();
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      document.fileName = file.name;
      document.contentBase64 = typeof reader.result === 'string' ? reader.result.split(',').pop() ?? '' : '';
      form?.control.markAsDirty();
    };

    reader.readAsDataURL(file);
  }
}

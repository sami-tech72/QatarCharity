import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged, finalize, Subject, takeUntil } from 'rxjs';

import { SupplierRfxService } from '../../../core/services/supplier-rfx.service';
import {
  BidDocumentSubmission,
  BidInputSubmission,
  SupplierBidRequest,
  SupplierRfx,
} from '../../../shared/models/supplier-rfx.model';

type BidStepKey = 'basics' | 'documents' | 'inputs' | 'review';

interface BidStep {
  key: BidStepKey;
  label: string;
  hint: string;
  anchorId: string;
}

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
  currentStep = 1;
  requirementsReviewed = false;
  submissionComplete = false;
  stepperSteps: BidStep[] = [];

  private readonly destroy$ = new Subject<void>();
  private readonly search$ = new Subject<string>();

  constructor(
    private readonly supplierRfxService: SupplierRfxService,
    private readonly router: Router
  ) {}

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
    this.currentStep = 1;
    this.requirementsReviewed = false;
    this.submissionComplete = false;
    this.buildStepper(tender);

    if (hasChanged) {
      this.bidRequest = this.createBidRequest(tender);
    }
  }

  startBid(tender: SupplierRfx): void {
    this.goToBid(tender);
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
    this.currentStep = Math.max(this.currentStep, 2);

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
          this.submissionComplete = true;
          this.currentStep = this.stepperSteps.length;
        },
        error: (err) => {
          this.bidError = err?.message ?? 'Unable to submit your bid right now.';
          this.submissionComplete = false;
        },
      });
  }

  trackByTenderId(_: number, tender: SupplierRfx): string {
    return tender.id;
  }

  goToBid(tender: SupplierRfx): void {
    this.router.navigate(['/supplier/available-tenders', tender.id, 'bid']);
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

  proceedToBid(): void {
    this.requirementsReviewed = true;
    this.currentStep = 1;
    this.scrollToSection('bid-basics');
  }

  reopenForm(): void {
    this.currentStep = 1;
    this.submissionComplete = false;
  }

  resetFlow(): void {
    this.bidRequest = this.createBidRequest(this.selectedTender);
    this.bidSuccess = undefined;
    this.bidError = undefined;
    this.submissionComplete = false;
    this.currentStep = 1;
    this.buildStepper(this.selectedTender);
  }

  stepClass(index: number, step: BidStep): string {
    const stepNumber = index + 1;
    const complete = this.isStepComplete(step.key);

    if (this.currentStep === stepNumber) {
      return 'stepper__step stepper__step--active';
    }

    if (this.currentStep > stepNumber || complete) {
      return 'stepper__step stepper__step--done';
    }

    return 'stepper__step';
  }

  goToStep(index: number, step: BidStep): void {
    this.currentStep = index + 1;
    this.scrollToSection(step.anchorId);
  }

  private scrollToSection(anchorId: string): void {
    setTimeout(() => {
      const element = document.getElementById(anchorId);
      element?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
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
          if (this.selectedTender) {
            this.buildStepper(this.selectedTender);
          }
        },
        error: (err) => {
          this.tenders = [];
          this.error = err?.message ?? 'Unable to load available tenders.';
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

  private buildStepper(tender?: SupplierRfx): void {
    const hasDocuments = Boolean(tender?.requiredDocuments?.length);
    const hasInputs = Boolean(tender?.requiredInputs?.length);

    const steps: BidStep[] = [
      {
        key: 'basics',
        label: 'Bid basics',
        hint: 'Amount, delivery, and overview',
        anchorId: 'bid-basics',
      },
    ];

    if (hasDocuments) {
      steps.push({
        key: 'documents',
        label: 'Required documents',
        hint: 'Upload each requested file',
        anchorId: 'bid-documents',
      });
    }

    if (hasInputs) {
      steps.push({
        key: 'inputs',
        label: 'Required inputs',
        hint: 'Respond to tender prompts',
        anchorId: 'bid-inputs',
      });
    }

    steps.push({
      key: 'review',
      label: 'Review & submit',
      hint: 'Confirm and send bid',
      anchorId: 'bid-submit',
    });

    this.stepperSteps = steps;
    this.currentStep = 1;
  }

  private isStepComplete(step: BidStepKey): boolean {
    switch (step) {
      case 'basics':
        return Boolean(
          this.bidRequest.bidAmount &&
            this.bidRequest.expectedDeliveryDate &&
            (this.bidRequest.proposalSummary?.trim()?.length || 0) > 0
        );
      case 'documents':
        return !this.bidRequest.documents?.length || this.bidRequest.documents.every((doc) => !!doc.contentBase64);
      case 'inputs':
        return !this.bidRequest.inputs?.length || this.bidRequest.inputs.every((input) => !!input.value?.trim());
      case 'review':
        return this.submissionComplete;
      default:
        return false;
    }
  }
}

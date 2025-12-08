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

type BidStepKey = 'basics' | 'documents' | 'inputs' | 'review';

interface BidStep {
  key: BidStepKey;
  label: string;
  hint: string;
  anchorId: string;
}

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
  currentStep = 1;
  submissionComplete = false;
  stepperSteps: BidStep[] = [];

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
    this.currentStep = Math.max(this.currentStep, 2);

    if (!this.tender || form.invalid) {
      this.focusFirstIncompleteStep();
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
          this.submissionComplete = true;
          this.currentStep = this.stepperSteps.length;
        },
        error: (err) => {
          this.bidError = err?.message ?? 'Unable to submit your bid right now.';
          this.submissionComplete = false;
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
          this.buildStepper(tender);
          this.currentStep = 1;
          this.submissionComplete = false;
        },
        error: (err) => {
          this.error = err?.message ?? 'Unable to load this tender.';
        },
      });
  }

  reopenForm(): void {
    this.currentStep = 1;
  }

  resetFlow(): void {
    this.bidRequest = this.createBidRequest(this.tender);
    this.bidSuccess = undefined;
    this.bidError = undefined;
    this.submissionComplete = false;
    this.currentStep = 1;
    this.buildStepper(this.tender);
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

  private focusFirstIncompleteStep(): void {
    const firstIncompleteIndex = this.stepperSteps.findIndex((step) => !this.isStepComplete(step.key));

    if (firstIncompleteIndex >= 0) {
      this.currentStep = firstIncompleteIndex + 1;
      this.scrollToSection(this.stepperSteps[firstIncompleteIndex].anchorId);
    }
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

  private scrollToSection(anchorId: string): void {
    setTimeout(() => {
      const element = document.getElementById(anchorId);
      element?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
  }

  private buildStepper(tender?: SupplierRfx): void {
    const hasDocuments = Boolean(tender?.requiredDocuments?.length);
    const hasInputs = Boolean(tender?.requiredInputs?.length);

    const documentHint = hasDocuments
      ? `${tender?.requiredDocuments?.length} required file${tender?.requiredDocuments?.length === 1 ? '' : 's'}`
      : 'Upload each requested file';
    const inputHint = hasInputs
      ? `${tender?.requiredInputs?.length} tender prompt${tender?.requiredInputs?.length === 1 ? '' : 's'}`
      : 'Respond to tender prompts';

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
        hint: documentHint,
        anchorId: 'bid-documents',
      });
    }

    if (hasInputs) {
      steps.push({
        key: 'inputs',
        label: 'Required inputs',
        hint: inputHint,
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

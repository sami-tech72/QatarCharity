import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';

interface Step {
  id: number;
  label: string;
  description: string;
}

@Component({
  selector: 'app-create-rfx',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './create-rfx.component.html',
  styleUrl: './create-rfx.component.scss',
})
export class CreateRfxComponent {
  readonly steps: Step[] = [
    { id: 1, label: 'Basic Information', description: 'Define key RFx details' },
    { id: 2, label: 'Requirements', description: 'List scope and deliverables' },
    { id: 3, label: 'Evaluation Criteria', description: 'Configure scoring guidance' },
    { id: 4, label: 'Review & Publish', description: 'Confirm and release' },
  ];

  currentStepIndex = 0;

  readonly basicInfoForm: FormGroup;
  readonly requirementsForm: FormGroup;
  readonly evaluationForm: FormGroup;

  constructor(private readonly fb: FormBuilder) {
    this.basicInfoForm = this.fb.group({
      tenderId: ['', Validators.required],
      title: ['', Validators.required],
      category: ['', Validators.required],
      submissionDeadline: ['', Validators.required],
      estimatedValue: ['', Validators.required],
      procurementMethod: ['Open Tender', Validators.required],
    });

    this.requirementsForm = this.fb.group({
      summary: [''],
      deliverables: [''],
      documents: [''],
    });

    this.evaluationForm = this.fb.group({
      criteria: ['Technical capability, Experience, Pricing'],
      scoringModel: ['Weighted scoring'],
      reviewers: ['Procurement team, Technical lead'],
    });
  }

  get currentStep(): Step {
    return this.steps[this.currentStepIndex];
  }

  get progressPercent(): number {
    if (this.steps.length <= 1) {
      return 0;
    }

    return (this.currentStepIndex / (this.steps.length - 1)) * 100;
  }

  goToStep(index: number): void {
    if (index < 0 || index >= this.steps.length) {
      return;
    }

    this.currentStepIndex = index;
  }

  nextStep(): void {
    if (this.currentStepIndex < this.steps.length - 1) {
      this.currentStepIndex += 1;
    }
  }

  previousStep(): void {
    if (this.currentStepIndex > 0) {
      this.currentStepIndex -= 1;
    }
  }

  publish(): void {
    // Placeholder for submission logic
    alert('RFx submitted for publishing');
  }
}

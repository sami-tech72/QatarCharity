import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { RouterModule } from '@angular/router';

interface Step {
  id: number;
  label: string;
  description: string;
}

type CriterionForm = {
  title: FormControl<string>;
  weight: FormControl<number>;
  description: FormControl<string>;
  type: FormControl<'technical' | 'commercial'>;
};

@Component({
  selector: 'app-create-rfx',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './create-rfx.component.html',
  styleUrl: './create-rfx.component.scss',
})
export class CreateRfxComponent {
  private readonly requiredDocLabels: Record<string, string> = {
    companyProfile: 'Company Profile',
    tradeLicense: 'Trade License',
    financialStatements: 'Financial Statements',
    references: 'References/Past Projects',
    certifications: 'Certifications',
    methodology: 'Methodology',
    other: 'Others',
  };

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
      rfxType: ['', Validators.required],
      category: ['', Validators.required],
      title: ['', Validators.required],
      description: ['', Validators.required],
      estimatedBudget: ['', Validators.required],
      department: ['', Validators.required],
      hideBudget: [false],
      publicationDate: ['', Validators.required],
      closingDate: ['', Validators.required],
      currency: ['QAR', Validators.required],
      priority: ['High', Validators.required],
      tenderBondRequired: ['Yes', Validators.required],
      contactPerson: ['', Validators.required],
      contactEmail: ['', [Validators.required, Validators.email]],
      contactPhone: ['', Validators.required],
      submissionDeadline: ['', Validators.required],
    });

    this.requirementsForm = this.fb.group({
      scope: ['', Validators.required],
      technicalSpec: ['', Validators.required],
      deliverables: ['', Validators.required],
      timeline: ['', Validators.required],
      requiredDocs: this.fb.group({
        companyProfile: [true],
        tradeLicense: [true],
        financialStatements: [false],
        references: [false],
        certifications: [false],
        methodology: [false],
        other: [false],
      }),
      attachments: this.fb.array<FormControl<string>>([
        this.fb.nonNullable.control('Specifications.pdf'),
        this.fb.nonNullable.control('Drawings.zip'),
      ]),
    });

    this.evaluationForm = this.fb.group({
      criteria: this.fb.array<FormGroup<CriterionForm>>([
        this.createCriterion('Technical Compliance', 25, 'Alignment with required specs', 'technical'),
        this.createCriterion('Experience & Qualifications', 25, 'Relevant past performance', 'technical'),
        this.createCriterion('Methodology & Approach', 25, 'Quality of delivery approach', 'technical'),
        this.createCriterion('Price', 25, 'Commercial competitiveness', 'commercial'),
      ]),
      minimumScore: [70, [Validators.required, Validators.min(0), Validators.max(100)]],
      evaluationNotes: [''],
      committee: this.fb.array<FormControl<string>>([
        this.fb.nonNullable.control('John Smith'),
        this.fb.nonNullable.control('Sarah Ahmed'),
        this.fb.nonNullable.control('Procurement Manager'),
        this.fb.nonNullable.control('Technical Expert'),
      ]),
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

  get attachments(): FormArray<FormControl<string>> {
    return this.requirementsForm.get('attachments') as FormArray<FormControl<string>>;
  }

  get criteriaArray(): FormArray<FormGroup<CriterionForm>> {
    return this.evaluationForm.get('criteria') as FormArray<FormGroup<CriterionForm>>;
  }

  get committeeMembers(): FormArray<FormControl<string>> {
    return this.evaluationForm.get('committee') as FormArray<FormControl<string>>;
  }

  get requiredDocumentsSelected(): string[] {
    const values = this.requirementsForm.get('requiredDocs')?.value as Record<string, boolean> | undefined;

    if (!values) {
      return [];
    }

    return Object.entries(values)
      .filter(([, selected]) => !!selected)
      .map(([key]) => this.requiredDocLabels[key] ?? key);
  }

  get technicalCriteriaTotal(): number {
    return this.sumCriteriaByType('technical');
  }

  get commercialCriteriaTotal(): number {
    return this.sumCriteriaByType('commercial');
  }

  get totalWeight(): number {
    return this.criteriaArray.controls.reduce((sum, control) => sum + (control.value.weight || 0), 0);
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

  addAttachment(): void {
    this.attachments.push(this.fb.nonNullable.control(''));
  }

  removeAttachment(index: number): void {
    this.attachments.removeAt(index);
  }

  addCriterion(type: 'technical' | 'commercial'): void {
    this.criteriaArray.push(this.createCriterion('', 0, '', type));
  }

  removeCriterion(index: number): void {
    this.criteriaArray.removeAt(index);
  }

  addCommitteeMember(): void {
    this.committeeMembers.push(this.fb.nonNullable.control(''));
  }

  removeCommitteeMember(index: number): void {
    this.committeeMembers.removeAt(index);
  }

  publish(): void {
    // Placeholder for submission logic
    alert('RFx submitted for publishing');
  }

  private createCriterion(
    title: string,
    weight: number,
    description: string,
    type: 'technical' | 'commercial'
  ): FormGroup<CriterionForm> {
    return this.fb.nonNullable.group<CriterionForm>({
      title: this.fb.nonNullable.control(title),
      weight: this.fb.nonNullable.control(weight),
      description: this.fb.nonNullable.control(description),
      type: this.fb.nonNullable.control(type),
    });
  }

  private sumCriteriaByType(type: 'technical' | 'commercial'): number {
    return this.criteriaArray.controls
      .filter((ctrl) => ctrl.value.type === type)
      .reduce((sum, control) => sum + (control.value.weight || 0), 0);
  }
}

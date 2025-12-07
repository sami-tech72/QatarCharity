import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

import { NotificationService } from '../../../core/services/notification.service';
import { RfxService } from '../../../core/services/rfx.service';
import { UserManagementService } from '../../../core/services/user-management.service';
import { WorkflowService } from '../../../core/services/workflow.service';
import { ManagedUser } from '../../../shared/models/user-management.model';
import { WorkflowSummary } from '../../../shared/models/workflow.model';
import { CreateRfxRequest, RfxCriterion } from '../../../shared/models/rfx.model';

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
export class CreateRfxComponent implements OnInit, OnDestroy {
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

  workflowOptions: WorkflowSummary[] = [];
  committeeOptions: ManagedUser[] = [];
  isSubmitting = false;
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly rfxService: RfxService,
    private readonly workflowService: WorkflowService,
    private readonly userService: UserManagementService,
    private readonly notification: NotificationService,
    private readonly router: Router,
  ) {
    this.basicInfoForm = this.fb.group({
      rfxType: ['', Validators.required],
      category: ['', Validators.required],
      title: ['', Validators.required],
      description: ['', Validators.required],
      estimatedBudget: [0, [Validators.required, Validators.min(0)]],
      department: ['', Validators.required],
      hideBudget: [false],
      publicationDate: ['', Validators.required],
      closingDate: ['', Validators.required],
      currency: ['QAR', Validators.required],
      priority: ['High', Validators.required],
      tenderBondRequired: this.fb.control<boolean>(true, { nonNullable: true }),
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
      attachments: this.fb.array<FormControl<File | null>>([
        this.fb.control<File | null>(null),
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
      workflowId: this.fb.control<string | null>(null),
      committee: this.fb.array<FormControl<string>>([this.fb.nonNullable.control('')]),
    });
  }

  ngOnInit(): void {
    this.loadWorkflows();
    this.loadCommitteeOptions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
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

  get attachments(): FormArray<FormControl<File | null>> {
    return this.requirementsForm.get('attachments') as FormArray<FormControl<File | null>>;
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
    this.attachments.push(this.fb.control<File | null>(null));
  }

  removeAttachment(index: number): void {
    this.attachments.removeAt(index);
  }

  onAttachmentChange(event: Event, index: number): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    this.attachments.at(index).setValue(file);
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
    if (!this.validateSteps()) {
      return;
    }

    this.isSubmitting = true;

    const request = this.buildRequest();

    this.rfxService.createRfx(request).subscribe({
      next: () => {
        this.notification.success('RFx created and sent to the selected workflow.');
        this.router.navigate(['/procurement/rfx-management']);
      },
      error: (error: Error) => {
        this.isSubmitting = false;
        this.notification.error(error.message || 'Failed to create RFx.');
      },
    });
  }

  committeeMemberLabel(memberId: string): string {
    if (!memberId) {
      return '';
    }

    const match = this.committeeOptions.find((member) => member.id === memberId);
    return match ? `${match.displayName} (${match.email}) — ${this.formatCommitteeRole(match)}` : 'Unassigned member';
  }

  hasCommitteeSelection(): boolean {
    return this.committeeMembers.controls.some((ctrl) => !!ctrl.value);
  }

  workflowNameById(id: string | null): string {
    const workflow = this.workflowOptions.find((option) => option.id === id);
    return workflow?.name ?? '';
  }

  private loadWorkflows(): void {
    this.workflowService
      .loadWorkflows({ pageNumber: 1, pageSize: 50 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => (this.workflowOptions = result.items),
        error: () => (this.workflowOptions = []),
      });
  }

  private loadCommitteeOptions(): void {
    this.userService
      .getUserLookup()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (users) => {
          this.committeeOptions = users.filter((user) => {
            if (user.role !== 'Procurement') {
              return false;
            }

            const subRole = user.procurementRole?.name?.trim().toLowerCase();
            return !!subRole && subRole.includes('committee');
          });
        },
        error: () => (this.committeeOptions = []),
      });
  }

  formatCommitteeRole(user: ManagedUser): string {
    if (user.role === 'Procurement') {
      const subRole = user.procurementRole?.name;
      return subRole ? `${user.role} — ${subRole}` : `${user.role} — No sub-role`;
    }

    return user.role;
  }

  private validateSteps(): boolean {
    const forms = [this.basicInfoForm, this.requirementsForm, this.evaluationForm];
    const invalidIndex = forms.findIndex((form) => form.invalid);

    if (invalidIndex !== -1) {
      forms[invalidIndex].markAllAsTouched();
      this.currentStepIndex = invalidIndex;
      return false;
    }

    return true;
  }

  private buildRequest(): CreateRfxRequest {
    const requiredDocs = Object.entries(
      (this.requirementsForm.get('requiredDocs')?.value as Record<string, boolean>) || {},
    )
      .filter(([, selected]) => selected)
      .map(([key]) => this.requiredDocLabels[key] ?? key);

    const evaluationCriteria: RfxCriterion[] = this.criteriaArray.controls.map((control) => (
      control.getRawValue()
    ));

    return {
      rfxType: this.basicInfoForm.value.rfxType,
      category: this.basicInfoForm.value.category,
      title: this.basicInfoForm.value.title,
      department: this.basicInfoForm.value.department,
      description: this.basicInfoForm.value.description,
      estimatedBudget: Number(this.basicInfoForm.value.estimatedBudget),
      currency: this.basicInfoForm.value.currency,
      hideBudget: !!this.basicInfoForm.value.hideBudget,
      publicationDate: this.basicInfoForm.value.publicationDate,
      closingDate: this.basicInfoForm.value.closingDate,
      submissionDeadline: this.basicInfoForm.value.submissionDeadline,
      priority: this.basicInfoForm.value.priority,
      tenderBondRequired: !!this.basicInfoForm.value.tenderBondRequired,
      contactPerson: this.basicInfoForm.value.contactPerson,
      contactEmail: this.basicInfoForm.value.contactEmail,
      contactPhone: this.basicInfoForm.value.contactPhone,
      scope: this.requirementsForm.value.scope,
      technicalSpecification: this.requirementsForm.value.technicalSpec,
      deliverables: this.requirementsForm.value.deliverables,
      timeline: this.requirementsForm.value.timeline,
      requiredDocuments: requiredDocs,
      evaluationCriteria,
      minimumScore: Number(this.evaluationForm.value.minimumScore),
      evaluationNotes: this.evaluationForm.value.evaluationNotes || null,
      committeeMemberIds: this.committeeMembers.controls
        .map((ctrl) => ctrl.value)
        .filter((id, index, arr) => !!id && arr.indexOf(id) === index),
      workflowId: this.evaluationForm.value.workflowId || null,
      status: 'Published',
    };
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

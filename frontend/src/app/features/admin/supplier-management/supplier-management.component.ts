import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';

import { SupplierManagementService } from '../../../core/services/supplier-management.service';
import { SupplierQueryRequest, SupplierRequest, SupplierStatus, Supplier } from '../../../shared/models/supplier.model';

@Component({
  selector: 'app-supplier-management-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './supplier-management.component.html',
  styleUrl: './supplier-management.component.scss',
})
export class SupplierManagementComponent implements OnInit, OnDestroy {
  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly fb = inject(FormBuilder);
  private readonly supplierService = inject(SupplierManagementService);
  private readonly destroy$ = new Subject<void>();

  suppliers: Supplier[] = [];
  paginationState: SupplierQueryRequest = { pageNumber: 1, pageSize: 10, search: '' };
  isLoading = false;
  isSubmitting = false;
  alertMessage = '';
  alertType: 'danger' | 'info' | 'success' = 'info';
  editingSupplier: Supplier | null = null;
  selectedDocuments: string[] = [];
  activeTab: TabId = 'company';

  constructor() {
    this.setPortalAccessValidation(this.supplierForm.controls.hasPortalAccess.value);
    this.supplierForm.controls.hasPortalAccess.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((hasAccess) => {
        this.setPortalAccessValidation(hasAccess);
      });
  }

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((search) => {
        this.paginationState = { ...this.paginationState, search: search.trim(), pageNumber: 1 };
        this.loadSuppliers();
      });

    this.loadSuppliers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  readonly companyStepControls: TabControlKey[] = [
    'companyName',
    'registrationNumber',
    'primaryContactName',
    'primaryContactEmail',
    'primaryContactPhone',
    'portalUserEmail',
  ];

  readonly businessStepControls: TabControlKey[] = [
    'businessCategories',
    'companyAddress',
    'website',
    'yearEstablished',
    'numberOfEmployees',
    'status',
  ];

  readonly categoriesOptions: string[] = [
    'Construction',
    'Logistics',
    'Warehousing',
    'Medical',
    'Pharmaceutical',
    'Print',
    'Media',
    'Print & Media',
    'Technology',
    'Cloud Services',
    'Facilities Management',
    'Consulting',
    'Energy',
    'General',
  ];

  readonly supplierForm = this.fb.nonNullable.group({
    companyName: ['', [Validators.required, Validators.maxLength(150)]],
    registrationNumber: ['', [Validators.required, Validators.maxLength(50)]],
    primaryContactName: ['', [Validators.required, Validators.maxLength(120)]],
    primaryContactEmail: ['', [Validators.required, Validators.email]],
    primaryContactPhone: ['', [Validators.required, Validators.maxLength(20)]],
    businessCategories: this.fb.nonNullable.control<string[]>([], [this.requireAtLeastOne]),
    companyAddress: ['', [Validators.required, Validators.maxLength(250)]],
    website: ['', [Validators.maxLength(150)]],
    yearEstablished: [new Date().getFullYear(), [Validators.required, Validators.min(1800)]],
    numberOfEmployees: [1, [Validators.required, Validators.min(1)]],
    uploadedDocuments: this.fb.nonNullable.control<string[]>([]),
    status: this.fb.nonNullable.control<SupplierStatus>('Pending'),
    hasPortalAccess: this.fb.nonNullable.control(true),
    portalUserEmail: ['', [Validators.email, Validators.maxLength(150)]],
  });

  get filteredSuppliers(): Supplier[] {
    return this.suppliers;
  }

  trackById(_: number, supplier: Supplier): string {
    return supplier.id;
  }

  loadSuppliers(): void {
    this.isLoading = true;
    this.supplierService
      .loadSuppliers(this.paginationState)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (page) => {
          this.suppliers = page.items;
          this.paginationState = {
            pageNumber: page.pageNumber,
            pageSize: page.pageSize,
          search: this.paginationState.search,
        };
        this.isLoading = false;
        if (this.alertType !== 'success') {
          this.clearAlert();
        }
        },
        error: (error) => {
          this.isLoading = false;
          this.setAlert(this.getErrorMessage(error, 'Unable to load suppliers.'), 'danger');
        },
      });
  }

  getStatusClass(status: Supplier['status']): string {
    switch (status) {
      case 'Approved':
        return 'badge-light-success';
      case 'Pending':
        return 'badge-light-warning';
      default:
        return 'badge-light-info';
    }
  }

  startCreate(): void {
    this.editingSupplier = null;
    this.selectedDocuments = [];
    this.activeTab = 'company';
    this.supplierForm.reset({
      companyName: '',
      registrationNumber: '',
      primaryContactName: '',
      primaryContactEmail: '',
      primaryContactPhone: '',
      businessCategories: [],
      companyAddress: '',
      website: '',
      yearEstablished: new Date().getFullYear(),
      numberOfEmployees: 1,
      uploadedDocuments: [],
      status: 'Pending',
      hasPortalAccess: true,
      portalUserEmail: '',
    });

    this.setPortalAccessValidation(this.supplierForm.controls.hasPortalAccess.value);
  }

  startEdit(supplier: Supplier): void {
    this.editingSupplier = supplier;
    this.selectedDocuments = supplier.uploadedDocuments ?? [];
    this.activeTab = 'company';
    this.supplierForm.reset({
      companyName: supplier.companyName,
      registrationNumber: supplier.registrationNumber,
      primaryContactName: supplier.primaryContactName,
      primaryContactEmail: supplier.primaryContactEmail,
      primaryContactPhone: supplier.primaryContactPhone,
      businessCategories: supplier.businessCategories,
      companyAddress: supplier.companyAddress,
      website: supplier.website ?? '',
      yearEstablished: supplier.yearEstablished,
      numberOfEmployees: supplier.numberOfEmployees,
      uploadedDocuments: supplier.uploadedDocuments ?? [],
      status: supplier.status,
      hasPortalAccess: supplier.hasPortalAccess,
      portalUserEmail: supplier.portalUserEmail ?? '',
    });

    this.setPortalAccessValidation(supplier.hasPortalAccess);
  }

  onCancel(): void {
    this.startCreate();
  }

  onSubmit(): void {
    if (this.supplierForm.invalid || this.isSubmitting) {
      this.supplierForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formValue = this.supplierForm.getRawValue();
    const payload: SupplierRequest = {
      companyName: formValue.companyName.trim(),
      registrationNumber: formValue.registrationNumber.trim(),
      primaryContactName: formValue.primaryContactName.trim(),
      primaryContactEmail: formValue.primaryContactEmail.trim(),
      primaryContactPhone: formValue.primaryContactPhone.trim(),
      businessCategories: formValue.businessCategories,
      companyAddress: formValue.companyAddress.trim(),
      website: formValue.website?.trim(),
      yearEstablished: formValue.yearEstablished,
      numberOfEmployees: formValue.numberOfEmployees,
      uploadedDocuments: formValue.uploadedDocuments,
      status: formValue.status,
      hasPortalAccess: formValue.hasPortalAccess,
      portalUserEmail: formValue.hasPortalAccess ? formValue.portalUserEmail.trim() : undefined,
    };

    const request$ = this.editingSupplier
      ? this.supplierService.updateSupplier(this.editingSupplier.id, payload)
      : this.supplierService.createSupplier(payload);

    request$.pipe(takeUntil(this.destroy$)).subscribe({
      next: (supplier) => {
        if (this.editingSupplier) {
          this.suppliers = this.suppliers.map((existing) =>
            existing.id === supplier.id ? supplier : existing
          );
        } else {
          this.suppliers = [supplier, ...this.suppliers];
        }

        this.isSubmitting = false;
        this.setAlert(`Supplier ${supplier.companyName} saved successfully.`, 'success');
        this.startCreate();
        this.loadSuppliers();
      },
      error: (error) => {
        this.isSubmitting = false;
        this.setAlert(this.getErrorMessage(error, 'Unable to save supplier.'), 'danger');
      },
    });
  }

  clearSearch(): void {
    this.searchControl.setValue('', { emitEvent: true });
  }

  onDocumentsSelected(event: Event): void {
    const files = (event.target as HTMLInputElement).files;
    if (!files) {
      return;
    }

    const fileNames = Array.from(files).map((file) => file.name);
    this.selectedDocuments = fileNames;
    this.supplierForm.patchValue({ uploadedDocuments: fileNames });
  }

  onTogglePortalAccess(): void {
    const hasAccess = this.supplierForm.controls.hasPortalAccess.value;
    this.setPortalAccessValidation(hasAccess);

    if (!hasAccess) {
      this.supplierForm.patchValue({ portalUserEmail: '' });
    } else if (!this.supplierForm.controls.portalUserEmail.value) {
      this.syncPortalEmailWithPrimary();
    }
  }

  syncPortalEmailWithPrimary(): void {
    const primaryEmail = this.supplierForm.controls.primaryContactEmail.value;
    if (primaryEmail) {
      this.supplierForm.patchValue({ portalUserEmail: primaryEmail });
    }
  }

  private setAlert(message: string, type: 'danger' | 'info' | 'success'): void {
    this.alertMessage = message;
    this.alertType = type;
  }

  private clearAlert(): void {
    this.alertMessage = '';
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (typeof error === 'string') {
      return error;
    }

    if (error instanceof Error) {
      return error.message || fallback;
    }

    return fallback;
  }

  private requireAtLeastOne(control: AbstractControl<string[] | null>): ValidationErrors | null {
    const value = control.value || [];
    return value.length ? null : { required: true };
  }

  goToTab(tab: TabId): void {
    if (tab === 'business' && !this.ensureStepValid(this.companyStepControls)) {
      return;
    }

    if (
      tab === 'documents' &&
      (!this.ensureStepValid(this.companyStepControls) || !this.ensureStepValid(this.businessStepControls))
    ) {
      return;
    }

    this.activeTab = tab;
  }

  goToNextFromCompany(): void {
    if (this.ensureStepValid(this.companyStepControls)) {
      this.activeTab = 'business';
    }
  }

  goToNextFromBusiness(): void {
    if (this.ensureStepValid(this.businessStepControls)) {
      this.activeTab = 'documents';
    }
  }

  goToPrevious(tab: TabId): void {
    this.activeTab = tab;
  }

  private ensureStepValid(controlNames: TabControlKey[]): boolean {
    let valid = true;

    controlNames.forEach((name) => {
      const control = this.supplierForm.get(name);
      if (!control) {
        return;
      }

      control.markAsTouched();
      control.updateValueAndValidity();

      if (control.invalid) {
        valid = false;
      }
    });

    return valid;
  }

  private setPortalAccessValidation(hasAccess: boolean): void {
    this.applyPortalEmailValidators(this.supplierForm, hasAccess);
  }

  private applyPortalEmailValidators(form: FormGroup, hasAccess: boolean): void {
    const portalEmail = form.get('portalUserEmail');
    if (!portalEmail) {
      return;
    }

    const validators = [Validators.email, Validators.maxLength(150)];
    if (hasAccess) {
      validators.unshift(Validators.required);
    }

    portalEmail.setValidators(validators);
    portalEmail.updateValueAndValidity();
  }
}

type TabId = 'company' | 'business' | 'documents';
type TabControlKey =
  | 'companyName'
  | 'registrationNumber'
  | 'primaryContactName'
  | 'primaryContactEmail'
  | 'primaryContactPhone'
  | 'portalUserEmail'
  | 'businessCategories'
  | 'companyAddress'
  | 'website'
  | 'yearEstablished'
  | 'numberOfEmployees'
  | 'status';

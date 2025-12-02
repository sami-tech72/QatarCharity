import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';

interface Supplier {
  id: string;
  companyName: string;
  registrationNumber: string;
  primaryContactName: string;
  primaryContactEmail: string;
  primaryContactPhone: string;
  businessCategories: string[];
  companyAddress: string;
  website: string;
  yearEstablished: number;
  numberOfEmployees: number;
  uploadedDocuments: string[];
  category: string;
  contactPerson: string;
  submissionDate: string;
  status: 'Approved' | 'Pending' | 'On Hold';
}

@Component({
  selector: 'app-supplier-management-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './supplier-management.component.html',
  styleUrl: './supplier-management.component.scss',
})
export class SupplierManagementComponent {
  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly fb = new FormBuilder();
  isSubmitting = false;
  editingSupplier: Supplier | null = null;
  selectedDocuments: string[] = [];

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
    status: this.fb.nonNullable.control<Supplier['status']>('Pending'),
  });

  suppliers: Supplier[] = [
    {
      id: '#SUB-8702',
      companyName: 'Ibn Sina Medical Supplies',
      registrationNumber: 'CR-102938',
      primaryContactName: 'Dr. Amina Rahman',
      primaryContactEmail: 'amina.rahman@ibnsina.qa',
      primaryContactPhone: '+974 4412 0001',
      businessCategories: ['Medical', 'Pharmaceutical'],
      companyAddress: 'Building 12, Street 210, Industrial Area, Doha',
      website: 'https://ibnsinamed.qa',
      yearEstablished: 2008,
      numberOfEmployees: 85,
      uploadedDocuments: ['Trade License.pdf', 'Tax Certificate.pdf'],
      category: 'Medical',
      contactPerson: 'Dr. Amina Rahman',
      submissionDate: '12/01/2024',
      status: 'Approved',
    },
    {
      id: '#SUB-5120',
      companyName: 'Doha Logistics Partners',
      registrationNumber: 'CR-548210',
      primaryContactName: 'Yousef Al-Khaled',
      primaryContactEmail: 'yousef.khaled@dohalogistics.com',
      primaryContactPhone: '+974 4488 3321',
      businessCategories: ['Logistics', 'Warehousing'],
      companyAddress: 'Office 7, Ras Abu Aboud Street, Doha',
      website: 'https://dohalogistics.com',
      yearEstablished: 2014,
      numberOfEmployees: 140,
      uploadedDocuments: ['Safety Compliance.pdf'],
      category: 'Logistics',
      contactPerson: 'Yousef Al-Khaled',
      submissionDate: '11/22/2024',
      status: 'Pending',
    },
    {
      id: '#SUB-4416',
      companyName: 'Gulf Printworks',
      registrationNumber: 'CR-776541',
      primaryContactName: 'Mariam Al-Thani',
      primaryContactEmail: 'mariam.t@gulfprintworks.qa',
      primaryContactPhone: '+974 4433 1188',
      businessCategories: ['Print', 'Media'],
      companyAddress: 'Warehouse 3, Salwa Road, Doha',
      website: 'https://gulfprintworks.qa',
      yearEstablished: 2010,
      numberOfEmployees: 65,
      uploadedDocuments: ['Portfolio.pdf', 'Insurance.pdf'],
      category: 'Print & Media',
      contactPerson: 'Mariam Al-Thani',
      submissionDate: '11/10/2024',
      status: 'Approved',
    },
    {
      id: '#SUB-2334',
      companyName: 'Azure Cloud Services',
      registrationNumber: 'CR-993022',
      primaryContactName: 'Omar Haddad',
      primaryContactEmail: 'omar.haddad@azurecloud.qa',
      primaryContactPhone: '+974 4422 9911',
      businessCategories: ['Technology', 'Cloud Services'],
      companyAddress: 'Level 10, Marina Twin Towers, Lusail',
      website: 'https://azurecloud.qa',
      yearEstablished: 2017,
      numberOfEmployees: 45,
      uploadedDocuments: ['ISO27001.pdf'],
      category: 'Technology',
      contactPerson: 'Omar Haddad',
      submissionDate: '10/28/2024',
      status: 'On Hold',
    },
  ];

  get filteredSuppliers(): Supplier[] {
    const term = this.searchControl.value.trim().toLowerCase();

    if (!term) {
      return this.suppliers;
    }

    return this.suppliers.filter((supplier) =>
      [
        supplier.id,
        supplier.companyName,
        supplier.registrationNumber,
        supplier.primaryContactName,
        supplier.primaryContactEmail,
        supplier.primaryContactPhone,
        supplier.businessCategories.join(', '),
        supplier.companyAddress,
        supplier.website,
      ].some((field) => field.toLowerCase().includes(term))
    );
  }

  trackById(_: number, supplier: Supplier): string {
    return supplier.id;
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
    });
  }

  startEdit(supplier: Supplier): void {
    this.editingSupplier = supplier;
    this.selectedDocuments = supplier.uploadedDocuments;
    this.supplierForm.reset({
      companyName: supplier.companyName,
      registrationNumber: supplier.registrationNumber,
      primaryContactName: supplier.primaryContactName,
      primaryContactEmail: supplier.primaryContactEmail,
      primaryContactPhone: supplier.primaryContactPhone,
      businessCategories: supplier.businessCategories,
      companyAddress: supplier.companyAddress,
      website: supplier.website,
      yearEstablished: supplier.yearEstablished,
      numberOfEmployees: supplier.numberOfEmployees,
      uploadedDocuments: supplier.uploadedDocuments,
      status: supplier.status,
    });
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
    const parsedCategories = formValue.businessCategories;
    const parsedDocuments = formValue.uploadedDocuments;

    const payload: Supplier = {
      id: this.editingSupplier?.id ?? this.generateId(),
      companyName: formValue.companyName.trim(),
      registrationNumber: formValue.registrationNumber.trim(),
      primaryContactName: formValue.primaryContactName.trim(),
      primaryContactEmail: formValue.primaryContactEmail.trim(),
      primaryContactPhone: formValue.primaryContactPhone.trim(),
      businessCategories: parsedCategories,
      companyAddress: formValue.companyAddress.trim(),
      website: formValue.website.trim(),
      yearEstablished: formValue.yearEstablished,
      numberOfEmployees: formValue.numberOfEmployees,
      uploadedDocuments: parsedDocuments,
      category: parsedCategories[0] || 'General',
      contactPerson: formValue.primaryContactName.trim(),
      submissionDate: this.editingSupplier?.submissionDate ?? this.formatDate(new Date()),
      status: formValue.status,
    };

    if (this.editingSupplier) {
      this.suppliers = this.suppliers.map((supplier) =>
        supplier.id === this.editingSupplier?.id ? payload : supplier
      );
    } else {
      this.suppliers = [payload, ...this.suppliers];
    }

    this.isSubmitting = false;
    this.startCreate();
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

  private requireAtLeastOne(control: AbstractControl<string[] | null>): ValidationErrors | null {
    const value = control.value || [];
    return value.length ? null : { required: true };
  }

  private generateId(): string {
    const random = Math.floor(Math.random() * 9000) + 1000;
    return `#SUB-${random}`;
  }

  private formatDate(date: Date): string {
    return new Intl.DateTimeFormat('en-US', {
      month: '2-digit',
      day: '2-digit',
      year: 'numeric',
    }).format(date);
  }
}

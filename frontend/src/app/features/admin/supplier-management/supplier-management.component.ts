import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

interface Supplier {
  id: string;
  name: string;
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

  suppliers: Supplier[] = [
    {
      id: '#SUB-8702',
      name: 'Ibn Sina Medical Supplies',
      category: 'Medical',
      contactPerson: 'Dr. Amina Rahman',
      submissionDate: '12/01/2024',
      status: 'Approved',
    },
    {
      id: '#SUB-5120',
      name: 'Doha Logistics Partners',
      category: 'Logistics',
      contactPerson: 'Yousef Al-Khaled',
      submissionDate: '11/22/2024',
      status: 'Pending',
    },
    {
      id: '#SUB-4416',
      name: 'Gulf Printworks',
      category: 'Print & Media',
      contactPerson: 'Mariam Al-Thani',
      submissionDate: '11/10/2024',
      status: 'Approved',
    },
    {
      id: '#SUB-2334',
      name: 'Azure Cloud Services',
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
      [supplier.id, supplier.name, supplier.category, supplier.contactPerson].some((field) =>
        field.toLowerCase().includes(term)
      )
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
}

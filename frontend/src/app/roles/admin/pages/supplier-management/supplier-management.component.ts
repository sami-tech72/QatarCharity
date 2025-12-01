import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface Supplier {
  name: string;
  category: string;
  health: 'Good' | 'Attention' | 'Review';
  spend: string;
}

@Component({
  selector: 'app-supplier-management-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './supplier-management.component.html',
  styleUrl: './supplier-management.component.scss',
})
export class SupplierManagementComponent {
  suppliers: Supplier[] = [
    { name: 'Ibn Sina Medical Supplies', category: 'Medical', health: 'Good', spend: '$82,400' },
    { name: 'Doha Logistics Partners', category: 'Logistics', health: 'Attention', spend: '$41,900' },
    { name: 'Gulf Printworks', category: 'Print & Media', health: 'Good', spend: '$16,250' },
    { name: 'Azure Cloud Services', category: 'Technology', health: 'Review', spend: '$102,600' },
  ];

  trackByName(_: number, supplier: Supplier): string {
    return supplier.name;
  }
}

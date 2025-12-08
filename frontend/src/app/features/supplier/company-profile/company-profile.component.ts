import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface CompanyDetails {
  companyName: string;
  industry: string;
  registrationNumber: string;
  taxId: string;
  website: string;
  address: string;
  city: string;
  country: string;
}

@Component({
  selector: 'app-company-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './company-profile.component.html',
  styleUrl: './company-profile.component.scss',
})
export class CompanyProfileComponent {
  companyDetails: CompanyDetails = {
    companyName: 'ACME Corporation',
    industry: 'Technology',
    registrationNumber: '',
    taxId: '',
    website: '',
    address: '123 Business St',
    city: 'New York',
    country: 'USA',
  };

  onSaveChanges(): void {
    console.log('Saving company details:', this.companyDetails);
    // Handle save logic here
  }

  onCancel(): void {
    console.log('Cancelling changes');
    // Handle cancel logic here
  }
}

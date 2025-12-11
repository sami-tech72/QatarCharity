import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { SupplierProfileService } from '../../../core/services/supplier-profile.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SupplierProfile } from '../../../shared/models/supplier-profile.model';
import { Supplier } from '../../../shared/models/supplier.model';

@Component({
  selector: 'app-company-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './company-profile.component.html',
  styleUrl: './company-profile.component.scss',
})
export class CompanyProfileComponent implements OnInit {
  profileForm: FormGroup;
  isLoading = false;
  isSaving = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly supplierProfileService: SupplierProfileService,
    private readonly notifier: NotificationService,
  ) {
    this.profileForm = this.fb.group({
      companyName: ['', [Validators.required, Validators.maxLength(256)]],
      registrationNumber: ['', [Validators.maxLength(100)]],
      primaryContactName: ['', [Validators.required, Validators.maxLength(150)]],
      primaryContactEmail: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      primaryContactPhone: ['', [Validators.required, Validators.maxLength(50)]],
      companyAddress: ['', [Validators.required, Validators.maxLength(500)]],
      website: ['', [Validators.maxLength(256)]],
      yearEstablished: [new Date().getFullYear(), [Validators.min(1800), Validators.max(new Date().getFullYear())]],
      numberOfEmployees: [0, [Validators.min(0)]],
    });
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.supplierProfileService.loadProfile().subscribe({
      next: (supplier) => this.patchForm(supplier),
      error: () => {
        this.notifier.error('Unable to load your company profile.');
        this.isLoading = false;
      },
    });
  }

  onSaveChanges(): void {
    if (this.profileForm.invalid || this.isSaving) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    const value = this.profileForm.getRawValue();
    const payload: SupplierProfile = {
      companyName: value.companyName.trim(),
      registrationNumber: value.registrationNumber?.trim() ?? '',
      primaryContactName: value.primaryContactName.trim(),
      primaryContactEmail: value.primaryContactEmail.trim(),
      primaryContactPhone: value.primaryContactPhone.trim(),
      companyAddress: value.companyAddress.trim(),
      website: value.website?.trim(),
      yearEstablished: value.yearEstablished,
      numberOfEmployees: value.numberOfEmployees,
    };

    this.supplierProfileService.updateProfile(payload).subscribe({
      next: (supplier) => {
        this.patchForm(supplier);
        this.notifier.success('Company profile updated successfully.');
        this.isSaving = false;
      },
      error: () => {
        this.notifier.error('Unable to save your company profile.');
        this.isSaving = false;
      },
    });
  }

  onCancel(): void {
    this.loadProfile();
  }

  private patchForm(supplier: Supplier): void {
    const yearEstablished = supplier.yearEstablished || new Date().getFullYear();
    this.profileForm.patchValue({
      companyName: supplier.companyName,
      registrationNumber: supplier.registrationNumber,
      primaryContactName: supplier.primaryContactName,
      primaryContactEmail: supplier.primaryContactEmail,
      primaryContactPhone: supplier.primaryContactPhone,
      companyAddress: supplier.companyAddress,
      website: supplier.website,
      yearEstablished,
      numberOfEmployees: supplier.numberOfEmployees,
    });

    this.isLoading = false;
  }
}

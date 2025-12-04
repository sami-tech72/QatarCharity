import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import {
  AssignProcurementSubRoleRequest,
  ProcurementAdminService,
  ProcurementSubRoleUpdateResult,
} from '../services/procurement-admin.service';

@Component({
  selector: 'app-procurement-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './procurement-settings.component.html',
  styleUrl: './procurement-settings.component.scss',
})
export class ProcurementSettingsComponent {
  readonly subRoleForm = this.fb.group({
    userId: ['', Validators.required],
    name: ['', Validators.required],
    canView: [true],
    canCreate: [false],
    canUpdate: [false],
    canDelete: [false],
  });

  saving = false;
  lastResult: ProcurementSubRoleUpdateResult | null = null;
  error: string | null = null;

  constructor(
    private readonly fb: FormBuilder,
    private readonly procurementAdminService: ProcurementAdminService,
  ) {}

  submit() {
    this.error = null;

    if (this.subRoleForm.invalid) {
      this.subRoleForm.markAllAsTouched();
      return;
    }

    const { userId, ...payload } = this.subRoleForm.getRawValue();

    this.saving = true;
    this.procurementAdminService
      .assignSubRole(userId!, payload as AssignProcurementSubRoleRequest)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: (response) => {
          if (!response.success || !response.data) {
            this.error = response.message || 'Unable to save sub-role.';
            return;
          }

          this.lastResult = response.data;
        },
        error: () => {
          this.error = 'Unable to save sub-role.';
        },
      });
  }
}

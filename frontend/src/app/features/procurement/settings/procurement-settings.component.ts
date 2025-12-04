import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
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
export class ProcurementSettingsComponent implements OnChanges {
  @Input() userId: string | null = null;
  @Input() userName: string | null = null;

  readonly subRoleForm: ReturnType<ProcurementSettingsComponent['buildForm']>;

  saving = false;
  lastResult: ProcurementSubRoleUpdateResult | null = null;
  error: string | null = null;

  constructor(
    private readonly fb: FormBuilder,
    private readonly procurementAdminService: ProcurementAdminService,
  ) {
    this.subRoleForm = this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['userId']) {
      const control = this.subRoleForm.controls.userId;

      if (this.userId) {
        control.disable({ emitEvent: false });
        control.setValue(this.userId);
      } else {
        control.enable({ emitEvent: false });
        control.reset('');
      }
    }
  }

  private buildForm() {
    return this.fb.group({
      userId: ['', Validators.required],
      name: ['', Validators.required],
      canView: [true],
      canCreate: [false],
      canUpdate: [false],
      canDelete: [false],
    });
  }

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

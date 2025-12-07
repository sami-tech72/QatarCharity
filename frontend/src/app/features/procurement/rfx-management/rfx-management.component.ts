import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, debounceTime, finalize, takeUntil } from 'rxjs';

import { RfxService } from '../../../core/services/rfx.service';
import { PagedResult } from '../../../shared/models/pagination.model';
import { RfxDetail, RfxSummary } from '../../../shared/models/rfx.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-rfx-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './rfx-management.component.html',
  styleUrl: './rfx-management.component.scss',
})
export class RfxManagementComponent implements OnInit, OnDestroy {
  readonly searchControl = new FormControl('', { nonNullable: true });

  rfxRecords: RfxSummary[] = [];
  loading = false;
  total = 0;
  approvingId: string | null = null;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly rfxService: RfxService,
    private readonly notification: NotificationService,
  ) {}

  ngOnInit(): void {
    this.loadRfx();

    this.searchControl.valueChanges.pipe(debounceTime(300), takeUntil(this.destroy$)).subscribe((term) => {
      this.loadRfx(term);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get filteredRecords(): RfxSummary[] {
    return this.rfxRecords;
  }

  get emptyStateMessage(): string {
    if (this.loading) {
      return 'Loading RFx records...';
    }

    return this.searchControl.value ? 'No results match your search.' : 'No RFx records available yet.';
  }

  trackByTenderId(_: number, record: RfxSummary): string {
    return record.referenceNumber;
  }

  statusBadgeClass(status: RfxSummary['status']): string {
    switch (status) {
      case 'Published':
        return 'badge-light-success';
      case 'Closed':
        return 'badge-light-danger';
      default:
        return 'badge-light-warning';
    }
  }

  committeeBadgeClass(status: RfxSummary['committeeStatus']): string {
    switch (status) {
      case 'Assigned':
        return 'badge-light-info';
      case 'Approved':
        return 'badge-light-success';
      case 'In Review':
        return 'badge-light-info';
      default:
        return 'badge-light-warning';
    }
  }

  isApproving(record: RfxSummary): boolean {
    return this.approvingId === record.id;
  }

  approve(record: RfxSummary): void {
    if (!record.canApprove || record.status !== 'Draft') {
      return;
    }

    this.approvingId = record.id;

    this.rfxService
      .approveRfx(record.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => (this.approvingId = null)),
      )
      .subscribe({
        next: (updated: RfxDetail) => {
          record.status = updated.status;
          record.committeeStatus = 'Approved';
          record.canApprove = false;
          this.notification.success('RFx approved and published.');
        },
        error: (error) => {
          this.notification.error(error.message || 'Unable to approve RFx.');
        },
      });
  }

  private loadRfx(search?: string): void {
    this.loading = true;
    this.rfxService
      .loadRfx({ pageNumber: 1, pageSize: 50, search: search?.trim() })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: PagedResult<RfxSummary>) => {
          this.rfxRecords = result.items;
          this.total = result.totalCount;
          this.loading = false;
        },
        error: () => {
          this.rfxRecords = [];
          this.loading = false;
        },
      });
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, debounceTime, finalize, takeUntil } from 'rxjs';

import { RfxService } from '../../../core/services/rfx.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PagedResult } from '../../../shared/models/pagination.model';
import { RfxDetail, RfxSummary } from '../../../shared/models/rfx.model';

@Component({
  selector: 'app-tender-committee',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './tender-committee.component.html',
  styleUrl: './tender-committee.component.scss',
})
export class TenderCommitteeComponent implements OnInit, OnDestroy {
  readonly searchControl = new FormControl('', { nonNullable: true });

  rfxRecords: RfxSummary[] = [];
  loading = false;
  approvingId: string | null = null;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly rfxService: RfxService,
    private readonly notification: NotificationService,
  ) {}

  ngOnInit(): void {
    this.loadAssignedRfx();

    this.searchControl.valueChanges.pipe(debounceTime(300), takeUntil(this.destroy$)).subscribe((term) => {
      this.loadAssignedRfx(term);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get emptyStateMessage(): string {
    if (this.loading) {
      return 'Loading committee assignments...';
    }

    return this.searchControl.value
      ? 'No assigned RFx records match your search.'
      : 'No RFx assignments are available for your committee role.';
  }

  trackByRfxId(_: number, record: RfxSummary): string {
    return record.id;
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

  private loadAssignedRfx(search?: string): void {
    this.loading = true;
    this.rfxService
      .loadRfx({ pageNumber: 1, pageSize: 50, search: search?.trim(), assignedOnly: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: PagedResult<RfxSummary>) => {
          this.rfxRecords = result.items;
          this.loading = false;
        },
        error: () => {
          this.rfxRecords = [];
          this.loading = false;
        },
      });
  }
}

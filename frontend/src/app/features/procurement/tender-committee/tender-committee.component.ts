import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Subject, debounceTime, takeUntil } from 'rxjs';

import { NotificationService } from '../../../core/services/notification.service';
import { RfxService } from '../../../core/services/rfx.service';
import { PagedResult } from '../../../shared/models/pagination.model';
import { RfxSummary } from '../../../shared/models/rfx.model';

@Component({
  selector: 'app-tender-committee',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tender-committee.component.html',
  styleUrls: ['./tender-committee.component.scss'],
})
export class TenderCommitteeComponent implements OnInit, OnDestroy {
  readonly searchControl = new FormControl('', { nonNullable: true });

  queue: RfxSummary[] = [];
  loading = false;
  private readonly approvals = new Map<string, 'approved'>();
  private readonly destroy$ = new Subject<void>();

  constructor(private readonly rfxService: RfxService, private readonly notificationService: NotificationService) {}

  ngOnInit(): void {
    this.loadQueue();

    this.searchControl.valueChanges
      .pipe(debounceTime(250), takeUntil(this.destroy$))
      .subscribe((term) => this.loadQueue(term));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get filteredQueue(): RfxSummary[] {
    const term = this.searchControl.value.trim().toLowerCase();
    if (!term) {
      return this.queue;
    }

    return this.queue.filter(
      (item) =>
        item.referenceNumber.toLowerCase().includes(term) ||
        item.title.toLowerCase().includes(term) ||
        item.category.toLowerCase().includes(term)
    );
  }

  get awaitingPublicationCount(): number {
    return this.queue.filter((item) => item.committeeStatus !== 'Approved').length;
  }

  get emptyStateMessage(): string {
    if (this.loading) {
      return 'Loading committee assignments...';
    }

    return this.searchControl.value ? 'No committee items match your search.' : 'No committee assignments found yet.';
  }

  myApprovalStatus(item: RfxSummary): 'approved' | 'pending' {
    return this.approvals.has(item.id) ? 'approved' : 'pending';
  }

  approve(item: RfxSummary): void {
    if (this.myApprovalStatus(item) === 'approved') {
      return;
    }

    this.approvals.set(item.id, 'approved');
    this.queue = this.queue.map((record) =>
      record.id === item.id
        ? {
            ...record,
            committeeStatus: 'Approved',
            status: record.status === 'Draft' ? 'Published' : record.status,
          }
        : record
    );

    this.notificationService.success(
      `${item.referenceNumber} approved. The RFx will publish once all committee approvals are captured.`,
      'Committee approval recorded'
    );
  }

  trackById(_: number, record: RfxSummary): string {
    return record.id;
  }

  private loadQueue(search?: string): void {
    this.loading = true;
    this.rfxService
      .loadRfx({ pageNumber: 1, pageSize: 50, search })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: PagedResult<RfxSummary>) => {
          this.queue = result.items;
          this.loading = false;
        },
        error: () => {
          this.queue = [];
          this.loading = false;
        },
      });
  }
}

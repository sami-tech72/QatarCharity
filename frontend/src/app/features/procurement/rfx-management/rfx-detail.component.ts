import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { finalize, Subject, takeUntil } from 'rxjs';

import { NotificationService } from '../../../core/services/notification.service';
import { RfxService } from '../../../core/services/rfx.service';
import { RfxDetail } from '../../../shared/models/rfx.model';

@Component({
  selector: 'app-rfx-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './rfx-detail.component.html',
  styleUrl: './rfx-detail.component.scss',
})
export class RfxDetailComponent implements OnInit, OnDestroy {
  rfx?: RfxDetail;
  loading = true;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly rfxService: RfxService,
    private readonly notification: NotificationService,
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      const id = params.get('id');

      if (!id) {
        this.notification.error('Invalid RFx identifier.');
        this.router.navigate(['/procurement/rfx-management']);
        return;
      }

      this.loadRfx(id);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get hasEvaluation(): boolean {
    return !!this.rfx?.evaluationCriteria?.length;
  }

  get hasCommittee(): boolean {
    return !!this.rfx?.committeeMembers?.length;
  }

  private loadRfx(id: string): void {
    this.loading = true;
    this.rfxService
      .getRfx(id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => (this.loading = false)),
      )
      .subscribe({
        next: (detail) => {
          this.rfx = detail;
        },
        error: (error) => {
          this.notification.error(error.message || 'Unable to load RFx details.');
          this.router.navigate(['/procurement/rfx-management']);
        },
      });
  }
}

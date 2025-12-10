import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';

import { ContractManagementService } from '../../../core/services/contract-management.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ContractReadyBid } from '../../../shared/models/contract-management.model';

@Component({
  selector: 'app-contract-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './contract-management.component.html',
  styleUrl: './contract-management.component.scss',
})
export class ContractManagementComponent implements OnInit, OnDestroy {
  contracts: ContractReadyBid[] = [];
  filteredContracts: ContractReadyBid[] = [];
  selectedBid?: ContractReadyBid;
  summary = {
    total: 0,
    approved: 0,
    totalValue: 0,
  };
  loading = false;
  showCreateModal = false;
  createForm: FormGroup;
  readonly statuses = ['Draft', 'Active', 'Pending', 'Closed'];

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly contractManagementService: ContractManagementService,
    private readonly notification: NotificationService,
    private readonly formBuilder: FormBuilder,
  ) {
    this.createForm = this.formBuilder.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      bidId: ['', Validators.required],
      rfxId: ['', Validators.required],
      supplierName: [{ value: '', disabled: true }, Validators.required],
      supplierUserId: ['', Validators.required],
      contractValue: [0, [Validators.required, Validators.min(0.01)]],
      currency: [{ value: '', disabled: true }, Validators.required],
      startDateUtc: ['', Validators.required],
      endDateUtc: ['', Validators.required],
      status: ['Draft', Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadContracts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearch(event: Event): void {
    const query = (event.target as HTMLInputElement).value.trim().toLowerCase();
    this.filteredContracts = this.contracts.filter((contract) =>
      `${contract.referenceNumber} ${contract.title} ${contract.supplierName}`.toLowerCase().includes(query),
    );
  }

  loadContracts(search?: string): void {
    this.loading = true;
    this.contractManagementService
      .loadReadyBids({ pageNumber: 1, pageSize: 50, search })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.contracts = result.items || [];
          this.filteredContracts = [...this.contracts];
          this.summary = {
            total: result.totalCount,
            approved: result.totalCount,
            totalValue: this.contracts.reduce((sum, contract) => sum + contract.bidAmount, 0),
          };
          this.loading = false;
        },
        error: (error) => {
          this.notification.error(error.message || 'Unable to load contract-ready bids.');
          this.contracts = [];
          this.filteredContracts = [];
          this.summary = { total: 0, approved: 0, totalValue: 0 };
          this.loading = false;
        },
      });
  }

  openCreateModal(bid?: ContractReadyBid): void {
    this.showCreateModal = true;
    const preselected = bid ?? this.contracts[0];
    if (preselected) {
      this.onBidSelected(preselected.bidId);
      this.createForm.patchValue({
        title: preselected.title,
        contractValue: preselected.bidAmount,
        status: 'Draft',
      });
    } else {
      this.selectedBid = undefined;
      this.createForm.reset({ status: 'Draft' });
    }
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
  }

  onBidSelected(bidId: string): void {
    const bid = this.contracts.find((c) => c.bidId === bidId);
    if (!bid) {
      return;
    }

    this.selectedBid = bid;
    this.createForm.patchValue({
      bidId: bid.bidId,
      rfxId: bid.rfxId,
      supplierName: bid.supplierName,
      supplierUserId: bid.supplierUserId,
      currency: bid.currency,
      contractValue: bid.bidAmount,
    });
  }

  submitContract(): void {
    if (this.createForm.invalid || !this.selectedBid) {
      this.createForm.markAllAsTouched();
      return;
    }

    const payload = {
      ...this.createForm.getRawValue(),
      startDateUtc: new Date(this.createForm.value.startDateUtc).toISOString(),
      endDateUtc: new Date(this.createForm.value.endDateUtc).toISOString(),
    };

    this.contractManagementService
      .createContract(payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notification.success('Contract created successfully.');
          this.showCreateModal = false;
          this.createForm.reset({ status: 'Draft' });
        },
        error: (error) => {
          this.notification.error(error.message || 'Unable to create contract.');
        },
      });
  }

  viewContract(contract: ContractReadyBid): void {
    this.notification.info(`View contract for ${contract.supplierName} - ${contract.referenceNumber}`);
  }

  editContract(contract: ContractReadyBid): void {
    this.notification.info(`Manage contract for ${contract.supplierName} (bid ${contract.referenceNumber}).`);
  }

  getStatusClass(status: string): string {
    return `status-${status?.toLowerCase().replace(/\s+/g, '-')}`;
  }
}

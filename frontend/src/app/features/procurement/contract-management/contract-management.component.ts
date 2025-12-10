import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';

import { ContractManagementService } from '../../../core/services/contract-management.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ContractReadyBid, ContractRecord } from '../../../shared/models/contract-management.model';

@Component({
  selector: 'app-contract-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './contract-management.component.html',
  styleUrls: ['./contract-management.component.scss'],
})
export class ContractManagementComponent implements OnInit, OnDestroy {
  readyBids: ContractReadyBid[] = [];
  filteredReadyBids: ContractReadyBid[] = [];
  contracts: ContractRecord[] = [];
  filteredContracts: ContractRecord[] = [];
  selectedBid?: ContractReadyBid;
  searchTerm = '';
  summary = {
    total: 0,
    approved: 0,
    totalValue: 0,
  };
  loading = false;
  showCreateModal = false;
  createForm: FormGroup;

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
    });
  }

  ngOnInit(): void {
    this.loadReadyBids();
    this.loadExistingContracts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearch(event: Event): void {
    this.searchTerm = (event.target as HTMLInputElement).value.trim().toLowerCase();
    this.applySearchFilters();
  }

  loadReadyBids(search?: string): void {
    this.loading = true;
    this.contractManagementService
      .loadReadyBids({ pageNumber: 1, pageSize: 50, search })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.readyBids = result.items || [];
          this.summary = {
            total: result.totalCount,
            approved: result.totalCount,
            totalValue: this.readyBids.reduce((sum, contract) => sum + contract.bidAmount, 0),
          };
          this.applySearchFilters();
          this.loading = false;
        },
        error: (error) => {
          this.notification.error(error.message || 'Unable to load contract-ready bids.');
          this.readyBids = [];
          this.applySearchFilters();
          this.summary = { total: 0, approved: 0, totalValue: 0 };
          this.loading = false;
        },
      });
  }

  loadExistingContracts(): void {
    this.contractManagementService
      .loadContracts({ pageNumber: 1, pageSize: 50 })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.contracts = result.items || [];
          this.applySearchFilters();
        },
        error: (error) => {
          this.notification.error(error.message || 'Unable to load created contracts.');
          this.contracts = [];
          this.applySearchFilters();
        },
      });
  }

  openCreateModal(bid?: ContractReadyBid): void {
    if (!this.readyBids.length) {
      this.notification.error('No approved bids are available to create a contract.');
      return;
    }

    const preselected = bid ?? this.readyBids[0];
    this.createForm.reset();
    this.selectedBid = undefined;

    if (preselected) {
      this.onBidSelected(preselected.bidId);
      this.createForm.patchValue({
        title: preselected.title,
        contractValue: preselected.bidAmount,
      });
    }

    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    console.log('closeCreateModal called');
    this.showCreateModal = false;
    this.selectedBid = undefined;
    this.createForm.reset();
  }

  onBidSelected(bidId: string): void {
    const bid = this.readyBids.find((c) => c.bidId === bidId);
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
        next: (created) => {
          this.notification.success('Contract created successfully.');
          this.showCreateModal = false;
          this.createForm.reset();
          this.contracts = [created, ...this.contracts];
          this.readyBids = this.readyBids.filter((bid) => bid.bidId !== created.bidId);
          this.applySearchFilters();
          this.selectedBid = undefined;
          this.loadExistingContracts();
          this.loadReadyBids();
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

  private applySearchFilters(): void {
    if (!this.searchTerm) {
      this.filteredReadyBids = [...this.readyBids];
      this.filteredContracts = [...this.contracts];
      return;
    }

    this.filteredReadyBids = this.readyBids.filter((contract) =>
      `${contract.referenceNumber} ${contract.title} ${contract.supplierName}`.toLowerCase().includes(this.searchTerm),
    );

    this.filteredContracts = this.contracts.filter((contract) =>
      `${contract.referenceNumber} ${contract.title} ${contract.supplierName}`.toLowerCase().includes(this.searchTerm),
    );
  }
}

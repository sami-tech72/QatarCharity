import { CommonModule, NgIfContext } from '@angular/common';
import { Component, OnDestroy, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
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
  directContract = false;
  searchTerm = '';
  summary = {
    total: 0,
    approved: 0,
    totalValue: 0,
  };
  loading = false;
  showCreateModal = false;
  createForm: FormGroup;

  @ViewChild('loadingStateTpl', { static: true }) loadingStateTpl!: TemplateRef<NgIfContext<boolean>>;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly contractManagementService: ContractManagementService,
    private readonly notification: NotificationService,
    private readonly formBuilder: FormBuilder,
    private readonly router: Router,
  ) {
    this.createForm = this.formBuilder.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      bidId: ['', Validators.required],
      rfxId: ['', Validators.required],
      directContract: [false],
      supplierName: [{ value: '', disabled: true }, Validators.required],
      supplierUserId: [{ value: '', disabled: true }, Validators.required],
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
    const creatingDirectly = !this.readyBids.length;
    const preselected = creatingDirectly ? undefined : bid ?? this.readyBids[0];

    this.createForm.reset({ directContract: creatingDirectly });
    this.selectedBid = undefined;
    this.setContractMode(creatingDirectly);

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
    this.directContract = false;
    this.createForm.reset();
  }

  setContractMode(isDirect: boolean): void {
    this.directContract = isDirect;
    this.createForm.get('directContract')?.setValue(isDirect, { emitEvent: false });

    const bidIdControl = this.createForm.get('bidId');
    const rfxIdControl = this.createForm.get('rfxId');
    const supplierNameControl = this.createForm.get('supplierName');
    const supplierUserIdControl = this.createForm.get('supplierUserId');
    const currencyControl = this.createForm.get('currency');

    if (isDirect) {
      bidIdControl?.clearValidators();
      rfxIdControl?.clearValidators();
      supplierNameControl?.enable();
      supplierUserIdControl?.enable();
      currencyControl?.enable();
      this.selectedBid = undefined;
      bidIdControl?.setValue(null);
      rfxIdControl?.setValue(null);
      currencyControl?.setValue('');
      supplierNameControl?.setValue('');
      supplierUserIdControl?.setValue('');
    } else {
      bidIdControl?.setValidators([Validators.required]);
      rfxIdControl?.setValidators([Validators.required]);
      supplierNameControl?.disable();
      supplierUserIdControl?.disable();
      currencyControl?.disable();
    }

    bidIdControl?.updateValueAndValidity();
    rfxIdControl?.updateValueAndValidity();
    supplierNameControl?.updateValueAndValidity();
    supplierUserIdControl?.updateValueAndValidity();
    currencyControl?.updateValueAndValidity();
  }

  onBidSelected(bidId: string): void {
    this.setContractMode(false);
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
      directContract: false,
    });
  }

  submitContract(): void {
    if (this.createForm.invalid || (!this.directContract && !this.selectedBid)) {
      this.createForm.markAllAsTouched();
      return;
    }

    const payload = {
      ...this.createForm.getRawValue(),
      bidId: this.directContract ? null : this.createForm.value.bidId,
      rfxId: this.directContract ? null : this.createForm.value.rfxId,
      directContract: this.directContract,
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

  viewContract(contract: ContractReadyBid | ContractRecord): void {
    if (!('id' in contract) || !contract.id) {
      this.notification.error('Only created contracts can be opened. Please create the contract first.');
      return;
    }

    this.router.navigate(['/procurement/contract-management/view', contract.id], {
      state: {
        contract,
      },
    });
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

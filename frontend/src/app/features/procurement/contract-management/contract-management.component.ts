import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';

import { ContractManagementService } from '../../../core/services/contract-management.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ContractReadyBid } from '../../../shared/models/contract-management.model';

@Component({
  selector: 'app-contract-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contract-management.component.html',
  styleUrl: './contract-management.component.scss',
})
export class ContractManagementComponent implements OnInit, OnDestroy {
  contracts: ContractReadyBid[] = [];
  filteredContracts: ContractReadyBid[] = [];
  summary = {
    total: 0,
    approved: 0,
    totalValue: 0,
  };
  loading = false;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly contractManagementService: ContractManagementService,
    private readonly notification: NotificationService,
  ) {}

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

  createContract(): void {
    this.notification.info('Contract creation flow coming soon.');
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

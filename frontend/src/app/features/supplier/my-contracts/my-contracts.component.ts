import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import { NotificationService } from '../../../core/services/notification.service';
import { SupplierContractsService } from '../../../core/services/supplier-contracts.service';
import {
  SupplierContract,
  SupplierContractResponse,
} from '../../../shared/models/supplier-contract.model';

interface Statistics {
  totalContracts: number;
  activeContracts: number;
  totalValue: number;
  drafts: number;
}

@Component({
  selector: 'app-my-contracts',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-contracts.component.html',
  styleUrls: ['./my-contracts.component.scss'],
})
export class MyContractsComponent implements OnInit {
  statistics: Statistics = {
    totalContracts: 0,
    activeContracts: 0,
    totalValue: 0,
    drafts: 0,
  };

  contracts: SupplierContract[] = [];
  filteredContracts: SupplierContract[] = [];
  loading = false;
  signingContractIds = new Set<string>();

  constructor(
    private readonly supplierContractsService: SupplierContractsService,
    private readonly notification: NotificationService,
  ) {}

  ngOnInit(): void {
    this.loadContracts();
  }

  loadContracts(): void {
    this.loading = true;
    this.supplierContractsService
      .loadContracts({ pageNumber: 1, pageSize: 100 })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (response: SupplierContractResponse) => {
          this.contracts = response.items || [];
          this.filteredContracts = [...this.contracts];
          this.calculateStatistics();
        },
        error: (error) => {
          this.notification.error(error.message || 'Unable to load your contracts.');
          this.contracts = [];
          this.filteredContracts = [];
          this.calculateStatistics();
        },
      });
  }

  calculateStatistics(): void {
    this.statistics.totalContracts = this.contracts.length;
    this.statistics.activeContracts = this.contracts.filter((c) => c.status.toLowerCase() === 'active').length;
    this.statistics.totalValue = this.contracts.reduce((sum, c) => sum + c.contractValue, 0);
    this.statistics.drafts = this.contracts.filter((c) => c.status.toLowerCase() === 'draft').length;
  }

  filterContracts(event: Event): void {
    const searchTerm = (event.target as HTMLInputElement).value.toLowerCase();
    this.filteredContracts = this.contracts.filter(
      (contract) =>
        contract.referenceNumber.toLowerCase().includes(searchTerm) ||
        contract.title.toLowerCase().includes(searchTerm) ||
        contract.supplierName.toLowerCase().includes(searchTerm),
    );
  }

  getStatusClass(status: string): string {
    const normalized = status.toLowerCase();
    const statusClasses: { [key: string]: string } = {
      active: 'bg-success bg-opacity-10 text-success',
      draft: 'bg-warning bg-opacity-10 text-warning',
      pending: 'bg-info bg-opacity-10 text-info',
      closed: 'bg-secondary bg-opacity-10 text-secondary',
    };
    return statusClasses[normalized] || 'bg-light text-muted';
  }

  canSign(contract: SupplierContract): boolean {
    return contract.status.toLowerCase() === 'draft' && !this.signingContractIds.has(contract.id);
  }

  signContract(contract: SupplierContract): void {
    if (!this.canSign(contract)) {
      return;
    }

    const signature = (prompt('Type your signature to activate this contract:') || '').trim();
    if (!signature) {
      this.notification.warning('Signature is required to activate the contract.');
      return;
    }

    this.signingContractIds.add(contract.id);
    this.supplierContractsService
      .signContract(contract.id, { signature })
      .pipe(
        finalize(() => {
          this.signingContractIds.delete(contract.id);
        }),
      )
      .subscribe({
        next: (updated) => {
          this.notification.success('Contract signed successfully.');
          this.contracts = this.contracts.map((c) => (c.id === updated.id ? { ...c, ...updated } : c));
          this.filteredContracts = this.filteredContracts.map((c) => (c.id === updated.id ? { ...c, ...updated } : c));
          this.calculateStatistics();
        },
        error: (error) => {
          this.notification.error(error.message || 'Unable to sign the contract.');
        },
      });
  }
}

import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

interface Contract {
  id: string;
  contractNumber: string;
  title: string;
  supplier: string;
  value: number;
  status: 'active' | 'inactive' | 'expired' | 'pending';
  startDate: string;
  endDate: string;
}

@Component({
  selector: 'app-contract-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contract-management.component.html',
  styleUrl: './contract-management.component.scss',
})
export class ContractManagementComponent {
  totalContracts = 1;
  activeContracts = 1;
  totalValue = '$150,000';
  expiringContracts = 3;
  searchQuery = '';

  contracts: Contract[] = [
    {
      id: '1',
      contractNumber: 'CTR-00001',
      title: 'Cloud Services Agreement',
      supplier: 'ACME Corporation',
      value: 150000,
      status: 'active',
      startDate: '12/1/2025',
      endDate: '12/1/2026'
    }
  ];

  filteredContracts = this.contracts;

  onSearch(event: Event) {
    const query = (event.target as HTMLInputElement).value.toLowerCase();
    this.searchQuery = query;
    this.filteredContracts = this.contracts.filter(contract =>
      contract.contractNumber.toLowerCase().includes(query) ||
      contract.title.toLowerCase().includes(query) ||
      contract.supplier.toLowerCase().includes(query)
    );
  }

  createContract() {
    console.log('Create new contract');
  }

  viewContract(contract: Contract) {
    console.log('View contract:', contract);
  }

  editContract(contract: Contract) {
    console.log('Edit contract:', contract);
  }

  getStatusClass(status: string): string {
    return `status-${status}`;
  }
}

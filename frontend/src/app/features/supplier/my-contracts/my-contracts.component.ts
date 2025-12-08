import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Contract {
  contractNumber: string;
  title: string;
  value: number;
  status: 'active' | 'completed' | 'pending' | 'expired';
  startDate: Date;
  endDate: Date;
}

interface Statistics {
  totalContracts: number;
  activeContracts: number;
  totalValue: number;
  completed: number;
}

@Component({
  selector: 'app-my-contracts',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-contracts.component.html',
  styleUrl: './my-contracts.component.scss',
})
export class MyContractsComponent implements OnInit {
  statistics: Statistics = {
    totalContracts: 0,
    activeContracts: 0,
    totalValue: 0,
    completed: 0,
  };

  contracts: Contract[] = [
    {
      contractNumber: 'CTR-00001',
      title: 'Cloud Services Agreement',
      value: 150000,
      status: 'active',
      startDate: new Date('2025-01-12'),
      endDate: new Date('2026-01-12'),
    },
    {
      contractNumber: 'CTR-00002',
      title: 'Software License Agreement',
      value: 75000,
      status: 'active',
      startDate: new Date('2025-02-01'),
      endDate: new Date('2026-02-01'),
    },
    {
      contractNumber: 'CTR-00003',
      title: 'Maintenance Services',
      value: 50000,
      status: 'completed',
      startDate: new Date('2024-01-15'),
      endDate: new Date('2024-12-31'),
    },
  ];

  filteredContracts: Contract[] = [];

  ngOnInit(): void {
    this.calculateStatistics();
    this.filteredContracts = [...this.contracts];
  }

  calculateStatistics(): void {
    this.statistics.totalContracts = this.contracts.length;
    this.statistics.activeContracts = this.contracts.filter(c => c.status === 'active').length;
    this.statistics.totalValue = this.contracts.reduce((sum, c) => sum + c.value, 0);
    this.statistics.completed = this.contracts.filter(c => c.status === 'completed').length;
  }

  filterContracts(event: any): void {
    const searchTerm = event.target.value.toLowerCase();
    this.filteredContracts = this.contracts.filter(contract =>
      contract.contractNumber.toLowerCase().includes(searchTerm) ||
      contract.title.toLowerCase().includes(searchTerm)
    );
  }

  getStatusClass(status: string): string {
    const statusClasses: { [key: string]: string } = {
      'active': 'bg-success bg-opacity-10 text-success',
      'completed': 'bg-info bg-opacity-10 text-info',
      'pending': 'bg-warning bg-opacity-10 text-warning',
      'expired': 'bg-danger bg-opacity-10 text-danger',
    };
    return statusClasses[status] || 'bg-secondary bg-opacity-10 text-secondary';
  }
}

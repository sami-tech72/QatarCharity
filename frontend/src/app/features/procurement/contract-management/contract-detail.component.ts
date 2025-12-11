import { CommonModule, Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

import { ContractReadyBid, ContractRecord } from '../../../shared/models/contract-management.model';

type ContractData = ContractReadyBid | ContractRecord;

type LineItem = {
  name: string;
  description: string;
  quantity: number;
  rate: number;
  total: number;
};

@Component({
  selector: 'app-contract-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './contract-detail.component.html',
  styleUrls: ['./contract-detail.component.scss'],
})
export class ContractDetailComponent implements OnInit {
  contract?: ContractData;
  lineItems: LineItem[] = [];
  totals = { subtotal: 0, tax: 0, total: 0 };
  statusBadge = 'Pending';
  statusClass = 'pending';
  referenceNumber = '';
  currency = '';
  supplierName = '';
  startDate?: string | null;
  endDate?: string | null;
  createdDate?: string | null;
  missingContractMessage = '';

  constructor(
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly location: Location,
  ) {}

  ngOnInit(): void {
    const navigationState = (this.router.getCurrentNavigation()?.extras.state as { contract?: ContractData })?.contract;
    const locationState = (this.location.getState() as { contract?: ContractData })?.contract;

    this.contract = navigationState || locationState;

    if (this.contract) {
      this.populateView(this.contract);
    } else {
      const identifier = this.route.snapshot.paramMap.get('id');
      this.missingContractMessage = identifier
        ? 'Contract details could not be loaded. Please return to Contract Management and open the contract again.'
        : 'No contract details were provided.';
    }
  }

  goBack(): void {
    this.router.navigate(['/procurement/contract-management']);
  }

  formatDate(value?: string | null): string {
    if (!value) {
      return 'Not available';
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return 'Not available';
    }

    return new Intl.DateTimeFormat('en-US', { month: 'short', day: '2-digit', year: 'numeric' }).format(date);
  }

  private populateView(contract: ContractData): void {
    const monetaryValue = 'contractValue' in contract ? contract.contractValue : contract.bidAmount;
    const contractCurrency = contract.currency || 'USD';
    const startDate = 'startDateUtc' in contract ? contract.startDateUtc : contract.submittedAtUtc;
    const endDate = 'endDateUtc' in contract ? contract.endDateUtc : contract.evaluatedAtUtc;
    const status = 'status' in contract ? contract.status : contract.evaluationStatus;

    this.lineItems = [
      {
        name: 'Contract Scope',
        description: contract.title,
        quantity: 1,
        rate: monetaryValue,
        total: monetaryValue,
      },
      {
        name: 'Engagement Timeline',
        description: `${this.formatDate(startDate)} - ${this.formatDate(endDate)}`,
        quantity: 1,
        rate: monetaryValue,
        total: monetaryValue,
      },
    ];

    this.totals = {
      subtotal: monetaryValue,
      tax: 0,
      total: monetaryValue,
    };

    this.referenceNumber = contract.referenceNumber || 'Direct';
    this.supplierName = contract.supplierName;
    this.currency = contractCurrency;
    this.statusBadge = status || 'Pending';
    this.statusClass = (status || 'pending').toLowerCase().replace(/\s+/g, '-');
    this.startDate = startDate;
    this.endDate = endDate;
    this.createdDate = 'createdAtUtc' in contract ? contract.createdAtUtc : contract.submittedAtUtc;
  }
}

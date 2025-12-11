import { CommonModule, Location } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

import { SupplierContractsService } from '../../../../core/services/supplier-contracts.service';
import { ContractDetail } from '../../../../shared/models/contract-management.model';

type LineItem = {
  name: string;
  description: string;
  quantity: number;
  rate: number;
  total: number;
};

@Component({
  selector: 'app-supplier-contract-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './contract-detail.component.html',
  styleUrls: ['./contract-detail.component.scss'],
})
export class SupplierContractDetailComponent implements OnInit, OnDestroy {
  contract?: ContractDetail;
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
  loading = true;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly location: Location,
    private readonly supplierContractsService: SupplierContractsService,
  ) {}

  ngOnInit(): void {
    const navigationState = (this.router.getCurrentNavigation()?.extras.state as { contract?: unknown })?.contract;
    const locationState = (this.location.getState() as { contract?: unknown })?.contract;

    const initialContract = this.extractContractDetail(navigationState) || this.extractContractDetail(locationState);
    if (initialContract) {
      this.populateView(initialContract);
    }

    const identifier = this.route.snapshot.paramMap.get('id');

    if (identifier) {
      this.supplierContractsService
        .loadContract(identifier)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (contract) => {
            this.populateView(contract);
          },
          error: () => {
            this.loading = false;
            this.missingContractMessage =
              'Contract details could not be loaded. Please return to My Contracts and open the contract again.';
          },
        });
    } else if (!this.contract) {
      this.loading = false;
      this.missingContractMessage = 'No contract details were provided.';
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  goBack(): void {
    this.router.navigate(['/supplier/my-contracts']);
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

  isImageSignature(signature?: string | null): boolean {
    if (!signature) {
      return false;
    }

    return /^data:image\//i.test(signature.trim());
  }

  private extractContractDetail(value: unknown): ContractDetail | undefined {
    if (!value || typeof value !== 'object') {
      return undefined;
    }

    const candidate = value as ContractDetail;
    if (candidate.issuerCompany && candidate.supplier) {
      return candidate;
    }

    return undefined;
  }

  private populateView(contract: ContractDetail): void {
    this.contract = contract;

    const monetaryValue = contract.contractValue;
    const contractCurrency = contract.currency || 'USD';
    const startDate = contract.startDateUtc;
    const endDate = contract.endDateUtc;
    const status = contract.status;

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
    this.createdDate = contract.createdAtUtc;
    this.loading = false;
  }
}

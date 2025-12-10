import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { SupplierRfxService } from '../../../core/services/supplier-rfx.service';
import { PagedResult } from '../../../shared/models/pagination.model';
import { SupplierBidQueryRequest, SupplierBidSummary } from '../../../shared/models/supplier-rfx.model';

interface Statistics {
  totalBids: number;
  underReview: number;
  accepted: number;
  winRate: number;
}

@Component({
  selector: 'app-my-bids',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-bids.component.html',
  styleUrl: './my-bids.component.scss',
})
export class MyBidsComponent implements OnInit {
  loading = false;
  errorMessage = '';
  searchTerm = '';

  page: PagedResult<SupplierBidSummary> = {
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPages: 0,
  };

  readonly query: SupplierBidQueryRequest = {
    pageNumber: 1,
    pageSize: 10,
    search: '',
  };

  statistics: Statistics = {
    totalBids: 0,
    underReview: 0,
    accepted: 0,
    winRate: 0,
  };

  bids: SupplierBidSummary[] = [];

  constructor(private readonly supplierRfxService: SupplierRfxService) {}

  selectedBid: SupplierBidSummary | null = null;

  ngOnInit(): void {
    this.loadBids();
  }

  calculateStatistics(): void {
    this.statistics.totalBids = this.bids.length;
    this.statistics.underReview = this.bids.filter(b => this.normalizeStatus(b.evaluationStatus) === 'under-review').length;
    this.statistics.accepted = this.bids.filter(b => this.normalizeStatus(b.evaluationStatus) === 'accepted').length;
    this.statistics.winRate = this.statistics.totalBids > 0
      ? (this.statistics.accepted / this.statistics.totalBids) * 100
      : 0;
  }

  onSearchChange(event: Event): void {
    const target = event.target as HTMLInputElement | null;
    this.searchTerm = target?.value ?? '';
    this.query.search = this.searchTerm;
    this.loadBids();
  }

  getStatusClass(status: string): string {
    const statusClasses: { [key: string]: string } = {
      'accepted': 'bg-success bg-opacity-10 text-success',
      'submitted': 'bg-info bg-opacity-10 text-info',
      'under-review': 'bg-warning bg-opacity-10 text-warning',
      'rejected': 'bg-danger bg-opacity-10 text-danger',
      'withdrawn': 'bg-secondary bg-opacity-10 text-secondary',
    };
    return statusClasses[this.normalizeStatus(status)] || 'bg-secondary bg-opacity-10 text-secondary';
  }

  createNewBid(): void {
    console.log('Create new bid clicked');
    // Handle navigation to create bid page
  }

  viewBid(bid: SupplierBidSummary): void {
    this.selectedBid = bid;
  }

  closeDetails(): void {
    this.selectedBid = null;
  }

  private loadBids(): void {
    this.loading = true;
    this.errorMessage = '';

    this.supplierRfxService.loadSupplierBids(this.query).subscribe({
      next: (page) => {
        this.page = page;
        this.bids = page.items;
        this.calculateStatistics();
        this.loading = false;
      },
      error: (error: unknown) => {
        this.loading = false;
        this.errorMessage = error instanceof Error ? error.message : 'Failed to load bids.';
      },
    });
  }

  private normalizeStatus(status: string): string {
    const normalized = status?.toLowerCase();

    if (normalized === 'pending review') {
      return 'under-review';
    }

    return normalized;
  }
}

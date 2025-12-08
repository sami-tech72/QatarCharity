import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Bid {
  bidNumber: string;
  rfxTitle: string;
  rfxNumber: string;
  bidAmount: number;
  status: 'accepted' | 'submitted' | 'under-review' | 'rejected' | 'withdrawn';
  submittedDate: Date;
}

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
  statistics: Statistics = {
    totalBids: 0,
    underReview: 0,
    accepted: 0,
    winRate: 0,
  };

  bids: Bid[] = [
    {
      bidNumber: 'BID-00001',
      rfxTitle: 'IT Infrastructure Upgrade',
      rfxNumber: 'RFX-00001',
      bidAmount: 250000,
      status: 'accepted',
      submittedDate: new Date('2025-12-01'),
    },
    {
      bidNumber: 'BID-00002',
      rfxTitle: 'Office Supplies Contract',
      rfxNumber: 'RFX-00002',
      bidAmount: 54,
      status: 'submitted',
      submittedDate: new Date('2025-12-08'),
    },
  ];

  filteredBids: Bid[] = [];

  ngOnInit(): void {
    this.calculateStatistics();
    this.filteredBids = [...this.bids];
  }

  calculateStatistics(): void {
    this.statistics.totalBids = this.bids.length;
    this.statistics.underReview = this.bids.filter(b => b.status === 'under-review').length;
    this.statistics.accepted = this.bids.filter(b => b.status === 'accepted').length;
    this.statistics.winRate = this.statistics.totalBids > 0
      ? (this.statistics.accepted / this.statistics.totalBids) * 100
      : 0;
  }

  filterBids(event: any): void {
    const searchTerm = event.target.value.toLowerCase();
    this.filteredBids = this.bids.filter(bid =>
      bid.bidNumber.toLowerCase().includes(searchTerm) ||
      bid.rfxTitle.toLowerCase().includes(searchTerm) ||
      bid.rfxNumber.toLowerCase().includes(searchTerm)
    );
  }

  getStatusClass(status: string): string {
    const statusClasses: { [key: string]: string } = {
      'accepted': 'bg-success bg-opacity-10 text-success',
      'submitted': 'bg-info bg-opacity-10 text-info',
      'under-review': 'bg-warning bg-opacity-10 text-warning',
      'rejected': 'bg-danger bg-opacity-10 text-danger',
      'withdrawn': 'bg-secondary bg-opacity-10 text-secondary',
    };
    return statusClasses[status] || 'bg-secondary bg-opacity-10 text-secondary';
  }

  createNewBid(): void {
    console.log('Create new bid clicked');
    // Handle navigation to create bid page
  }

  viewBid(bid: Bid): void {
    console.log('View bid:', bid);
    // Handle navigation to bid details
  }
}

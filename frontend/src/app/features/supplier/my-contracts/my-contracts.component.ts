import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
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
  @ViewChild('signatureCanvas') signatureCanvas?: ElementRef<HTMLCanvasElement>;

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
  signatureModalOpen = false;
  activeContract: SupplierContract | null = null;
  private ctx: CanvasRenderingContext2D | null = null;
  private isDrawing = false;
  private hasSignature = false;

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
    this.filteredContracts = this.contracts.filter((contract) => {
      const reference = contract.referenceNumber?.toLowerCase() ?? '';
      return (
        reference.includes(searchTerm) ||
        contract.title.toLowerCase().includes(searchTerm) ||
        contract.supplierName.toLowerCase().includes(searchTerm)
      );
    });
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

  openSignatureModal(contract: SupplierContract): void {
    if (!this.canSign(contract)) {
      return;
    }

    this.activeContract = contract;
    this.signatureModalOpen = true;
    this.hasSignature = false;

    setTimeout(() => this.initializeCanvas(), 0);
  }

  closeSignatureModal(): void {
    this.signatureModalOpen = false;
    this.activeContract = null;
    this.clearSignature();
  }

  initializeCanvas(): void {
    const canvas = this.signatureCanvas?.nativeElement;
    if (!canvas) {
      return;
    }

    const parentWidth = canvas.parentElement?.clientWidth || 500;
    canvas.width = parentWidth;
    canvas.height = 220;

    const context = canvas.getContext('2d');
    if (!context) {
      return;
    }

    context.lineWidth = 2;
    context.lineCap = 'round';
    context.strokeStyle = '#1f2937';
    context.fillStyle = '#ffffff';
    context.fillRect(0, 0, canvas.width, canvas.height);

    this.ctx = context;
  }

  startDrawing(event: MouseEvent | TouchEvent): void {
    if (!this.ctx || !this.signatureCanvas?.nativeElement) {
      return;
    }

    event.preventDefault();
    const { x, y } = this.getPosition(event);
    this.ctx.beginPath();
    this.ctx.moveTo(x, y);
    this.isDrawing = true;
  }

  drawStroke(event: MouseEvent | TouchEvent): void {
    if (!this.isDrawing || !this.ctx) {
      return;
    }

    event.preventDefault();
    const { x, y } = this.getPosition(event);
    this.ctx.lineTo(x, y);
    this.ctx.stroke();
    this.hasSignature = true;
  }

  finishDrawing(): void {
    this.isDrawing = false;
  }

  clearSignature(): void {
    const canvas = this.signatureCanvas?.nativeElement;
    if (!canvas || !this.ctx) {
      return;
    }

    this.ctx.clearRect(0, 0, canvas.width, canvas.height);
    this.ctx.fillStyle = '#ffffff';
    this.ctx.fillRect(0, 0, canvas.width, canvas.height);
    this.hasSignature = false;
  }

  submitSignature(): void {
    if (!this.activeContract || !this.signatureCanvas?.nativeElement) {
      return;
    }

    if (!this.hasSignature) {
      this.notification.warning('Please provide a digital signature to continue.');
      return;
    }

    const signature = this.signatureCanvas.nativeElement.toDataURL('image/png');
    this.signingContractIds.add(this.activeContract.id);

    this.supplierContractsService
      .signContract(this.activeContract.id, { signature })
      .pipe(
        finalize(() => {
          this.signingContractIds.delete(this.activeContract?.id || '');
        }),
      )
      .subscribe({
        next: (updated) => {
          this.notification.success('Contract signed successfully.');
          this.contracts = this.contracts.map((c) => (c.id === updated.id ? { ...c, ...updated } : c));
          this.filteredContracts = this.filteredContracts.map((c) => (c.id === updated.id ? { ...c, ...updated } : c));
          this.calculateStatistics();
          this.closeSignatureModal();
        },
        error: (error) => {
          this.notification.error(error.message || 'Unable to sign the contract.');
        },
      });
  }

  private getPosition(event: MouseEvent | TouchEvent): { x: number; y: number } {
    const canvas = this.signatureCanvas?.nativeElement;
    if (!canvas) {
      return { x: 0, y: 0 };
    }

    const rect = canvas.getBoundingClientRect();
    const clientX = event instanceof MouseEvent ? event.clientX : event.touches[0].clientX;
    const clientY = event instanceof MouseEvent ? event.clientY : event.touches[0].clientY;

    return {
      x: clientX - rect.left,
      y: clientY - rect.top,
    };
  }
}

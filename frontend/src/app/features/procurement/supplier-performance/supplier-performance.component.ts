import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-supplier-performance-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './supplier-performance.component.html',
  styleUrl: './supplier-performance.component.scss',
})
export class SupplierPerformanceComponent {
  scorecards = ['On-time Delivery', 'Quality', 'Compliance', 'Service Levels'];
}

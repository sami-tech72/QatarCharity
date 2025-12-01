import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-reports-analytics-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reports-analytics.component.html',
  styleUrl: './reports-analytics.component.scss',
})
export class ReportsAnalyticsComponent {
  charts = ['Spend by category', 'Cycle times', 'Supplier distribution'];
}

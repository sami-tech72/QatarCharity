import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-bid-evaluation-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bid-evaluation.component.html',
  styleUrl: './bid-evaluation.component.scss',
})
export class BidEvaluationComponent {
  criteria = ['Commercial', 'Technical', 'Compliance', 'Risk'];
}

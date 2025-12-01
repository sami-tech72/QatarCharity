import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-my-bids-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-bids.component.html',
  styleUrl: './my-bids.component.scss',
})
export class MyBidsComponent {
  statuses = ['Draft', 'Submitted', 'Clarifications', 'Awarded'];
}

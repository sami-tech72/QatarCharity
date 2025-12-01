import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-tender-committee-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tender-committee.component.html',
  styleUrl: './tender-committee.component.scss',
})
export class TenderCommitteeComponent {
  committeeRoles = ['Chair', 'Secretary', 'Voting Member', 'Advisor'];
}

import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-my-contracts-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-contracts.component.html',
  styleUrl: './my-contracts.component.scss',
})
export class MyContractsComponent {
  contractStates = ['Draft', 'Under Review', 'Active', 'Expires Soon'];
}

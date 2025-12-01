import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-available-tenders-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './available-tenders.component.html',
  styleUrl: './available-tenders.component.scss',
})
export class AvailableTendersComponent {
  tenderTags = ['IT', 'Logistics', 'Facilities', 'Consulting'];
}

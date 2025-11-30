import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  principles = [
    {
      title: 'Standalone-first',
      description: 'Each feature owns its standalone entry component and routing, keeping NgModules out of the critical path.'
    },
    {
      title: 'Feature-first folders',
      description: 'Routes and UI live beside their services and models inside domain-specific directories.'
    },
    {
      title: 'Stable core & shared',
      description: 'Shell concerns stay in core while shared holds presentational building blocks reused across features.'
    }
  ];
}

import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-architecture',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './architecture.component.html',
  styleUrl: './architecture.component.scss'
})
export class ArchitectureComponent {
  checklist = [
    'Core shell owns configuration and layout scaffolding only',
    'Features expose standalone routes via loadComponent for lazy loading',
    'Shared folder holds UI primitives without feature knowledge',
    'Routes favor clear, shallow paths that mirror the folder tree',
    'Testing stays close to the feature to keep feedback loops short'
  ];
}

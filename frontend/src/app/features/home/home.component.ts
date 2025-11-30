import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  readonly highlights = [
    {
      title: 'Feature-first structure',
      description: 'Group routes, components, and services by domain for clearer ownership and better scaling.'
    },
    {
      title: 'Standalone components',
      description: 'Use standalone components to keep routing lightweight and avoid unnecessary NgModules.'
    },
    {
      title: 'Shared building blocks',
      description: 'Keep reusable UI and utilities in shared folders with minimal dependencies to avoid coupling.'
    }
  ];
}

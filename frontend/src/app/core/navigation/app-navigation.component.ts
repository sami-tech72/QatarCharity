import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface NavLink {
  label: string;
  path: string;
}

@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './app-navigation.component.html',
  styleUrl: './app-navigation.component.scss'
})
export class AppNavigationComponent {
  readonly links: NavLink[] = [
    { label: 'Home', path: '/home' },
    { label: 'Campaigns', path: '/campaigns' }
  ];
}

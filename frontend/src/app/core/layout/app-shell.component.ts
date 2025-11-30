import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppNavigationComponent } from '../navigation/app-navigation.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [AppNavigationComponent, RouterOutlet],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShellComponent {}

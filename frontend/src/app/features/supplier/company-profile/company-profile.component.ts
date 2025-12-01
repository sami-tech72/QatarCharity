import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-company-profile-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './company-profile.component.html',
  styleUrl: './company-profile.component.scss',
})
export class CompanyProfileComponent {
  sections = ['Corporate info', 'Documents', 'Banking', 'Compliance'];
}
